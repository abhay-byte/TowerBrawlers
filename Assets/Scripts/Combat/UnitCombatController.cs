using System.Collections;
using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(CombatTarget))]
public class UnitCombatController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1.1f;
    [SerializeField] private float obstacleProbeDistance = 1.4f;
    [SerializeField] private float obstacleProbeRadius = 0.18f;
    [SerializeField] private int avoidanceSamplesPerSide = 5;
    [SerializeField] private float avoidanceAngleStep = 22f;
    [SerializeField] private LayerMask obstacleLayers = Physics2D.DefaultRaycastLayers;

    // ── Unit-to-unit separation ──────────────────────────────────────────────
    // Assign the layer(s) your unit GameObjects live on.
    // Must be DIFFERENT from obstacleLayers so units don't hard-block steering.
    [Header("Unit Separation")]
    [SerializeField] private LayerMask unitLayers;
    // Roughly 1.5–2× your unit collider half-width.
    [SerializeField] private float unitSeparationRadius = 0.7f;
    // Tune so units spread without drifting excessively.
    [SerializeField] private float unitSeparationStrength = 1.8f;
    // ────────────────────────────────────────────────────────────────────────

    [Header("Targeting")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private CombatTargetType validTargetTypes = CombatTargetType.Any;
    [SerializeField] private float retargetInterval = 0.25f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float attackDamageDelay = 0.2f;
    [SerializeField] private string attackStateName;
    [SerializeField] private string alternateAttackStateName;

    [Header("Animation")]
    [SerializeField] private string idleStateName;
    [SerializeField] private string runStateName;
    [SerializeField] private bool flipSpriteByMovement = true;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CombatTarget selfTarget;
    [SerializeField] private Collider2D selfCollider;

    // Static reusable buffers — zero GC alloc per frame
    private static readonly RaycastHit2D[] s_CastBuffer    = new RaycastHit2D[16];
    private static readonly Collider2D[]   s_OverlapBuffer = new Collider2D[16];

    private CombatTarget currentTarget;
    private Vector2 desiredVelocity;
    private float nextRetargetTime;
    private float nextAttackTime;
    private bool isAttacking;
    private bool hasManualDestination;
    private bool holdAtManualDestination;
    private Vector3 manualDestination;
    private Action<UnitCombatController> manualArrivalCallback;
    private CombatTarget advanceTarget;
    private bool hasAdvanceTarget;

    // Stable per-unit angle so groups orbit a target instead of all converging
    // to the exact same pixel and physically deadlocking each other.
    private float _orbitAngleOffset;

    private void Awake()
    {
        CacheReferences();
        // Up to 8 distinct 45° slots, stable across frames (not random).
        int slot = Mathf.Abs(GetEntityId().GetHashCode()) % 8;
        _orbitAngleOffset = slot * 45f;
    }

    private void OnEnable()
    {
        CacheReferences();
        nextRetargetTime = 0f;
        nextAttackTime   = 0f;
    }

    private void Update()
    {
        if (selfTarget == null || !selfTarget.IsAlive)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        if (hasManualDestination)
        {
            UpdateManualMovement();
            UpdateFacing();
            UpdateAnimation();
            return;
        }

        if (Time.time >= nextRetargetTime)
        {
            CombatTarget bestTarget = FindBestTarget();
            currentTarget    = bestTarget != null ? bestTarget : ResolveAdvanceTarget();
            nextRetargetTime = Time.time + retargetInterval;
        }

        UpdateFacing();
        UpdateAnimation();

        if (isAttacking)
        {
            // Unit separation still runs while attacking so standing units
            // don't freeze each other in a pile at the target's feet.
            desiredVelocity = ComputeUnitSeparationVelocity();
            return;
        }

        if (currentTarget == null || !currentTarget.IsAlive)
        {
            desiredVelocity = ComputeUnitSeparationVelocity();
            return;
        }

        // Each unit approaches its personal orbit point around the target,
        // not the raw AimPoint, to prevent groups converging on one pixel.
        Vector2 orbitAim = GetOrbitAimPoint(currentTarget);
        Vector2 toTarget = orbitAim - (Vector2)transform.position;
        float   distance = toTarget.magnitude;

        float attackDistance = GetAttackDistanceForTarget(currentTarget);
        if (distance <= attackDistance)
        {
            // In range — stay put but keep spreading from neighbours.
            desiredVelocity = ComputeUnitSeparationVelocity();

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());

            return;
        }

        if (distance > detectionRange && currentTarget != advanceTarget)
        {
            currentTarget   = null;
            desiredVelocity = ComputeUnitSeparationVelocity();
            return;
        }

        Vector2 steer   = ResolveMovementVelocity(toTarget, Mathf.Max(0.1f, distance - attackDistance), currentTarget);
        desiredVelocity = steer + ComputeUnitSeparationVelocity();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = isAttacking ? Vector2.zero : desiredVelocity;
    }

    // ─── ORBIT AIM POINT ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns a point offset from the target's AimPoint at this unit's
    /// personal angle. Groups of units spread naturally around the target
    /// rather than all arriving at the same pixel and deadlocking.
    /// </summary>
    private Vector2 GetOrbitAimPoint(CombatTarget target)
    {
        if (target == null) return Vector2.zero;
        float   orbitRadius = stoppingDistance * 0.45f;
        float   rad         = _orbitAngleOffset * Mathf.Deg2Rad;
        Vector2 offset      = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
        return (Vector2)target.AimPoint + offset;
    }

    // ─── UNIT-TO-UNIT SEPARATION ─────────────────────────────────────────────

    /// <summary>
    /// Soft repulsion from nearby units on <see cref="unitLayers"/>.
    /// Runs every frame including idle/attacking states so units never freeze
    /// in a pile regardless of what the steering system is doing.
    /// Force falls off linearly with distance → zero at separationRadius.
    /// </summary>
    private Vector2 ComputeUnitSeparationVelocity()
    {
        if (unitLayers == 0) return Vector2.zero;

        int count = Physics2D.OverlapCircleNonAlloc(
            (Vector2)transform.position, unitSeparationRadius,
            s_OverlapBuffer, unitLayers);

        Vector2 push = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = s_OverlapBuffer[i];
            if (col == null || col == selfCollider) continue;

            Vector2 closest = col.ClosestPoint((Vector2)transform.position);
            Vector2 pushDir = (Vector2)transform.position - closest;
            float   dist    = pushDir.magnitude;

            if (dist < 0.005f)
            {
                // Centres exactly coincide — push along bearing to collider centre.
                Vector2 toOther = (Vector2)transform.position - (Vector2)col.bounds.center;
                pushDir = toOther.sqrMagnitude > 0.0001f ? toOther.normalized : Vector2.right;
                dist    = 0.005f;
            }

            float overlap = unitSeparationRadius - dist;
            if (overlap <= 0f) continue;

            float strength = (overlap / unitSeparationRadius) * unitSeparationStrength;
            push += pushDir.normalized * strength;
        }

        // Cap so separation never flings units faster than moveSpeed.
        if (push.magnitude > moveSpeed)
            push = push.normalized * moveSpeed;

        return push;
    }

    // ─── OBSTACLE STEERING ───────────────────────────────────────────────────

    private Vector2 ResolveMovementVelocity(Vector2 desiredDirection, float remainingDistance, CombatTarget allowedTarget)
    {
        if (desiredDirection.sqrMagnitude < 0.0001f) return Vector2.zero;

        Vector2    normalizedDirection = desiredDirection.normalized;
        Collider2D allowedCollider     = allowedTarget != null ? allowedTarget.GetComponent<Collider2D>() : null;
        float      probeRadius         = GetProbeRadius();
        float      probeDistance       = Mathf.Max(0.4f, Mathf.Min(obstacleProbeDistance, remainingDistance + probeRadius));

        Vector2 obstacleSep  = ComputeObstacleSeparationVelocity(allowedCollider, probeRadius);

        Vector2 bestDirection = normalizedDirection;
        float   bestScore     = float.NegativeInfinity;

        for (int sample = 0; sample <= avoidanceSamplesPerSide; sample++)
        {
            float angle = avoidanceAngleStep * sample;

            Vector2 rightDir   = sample == 0 ? normalizedDirection : Rotate(normalizedDirection, -angle);
            float   rightScore = ScoreDirection(rightDir, probeDistance, remainingDistance, normalizedDirection, allowedCollider, probeRadius);
            if (rightScore > bestScore) { bestScore = rightScore; bestDirection = rightDir; }

            if (sample == 0) continue;

            Vector2 leftDir   = Rotate(normalizedDirection, angle);
            float   leftScore = ScoreDirection(leftDir, probeDistance, remainingDistance, normalizedDirection, allowedCollider, probeRadius);
            if (leftScore > bestScore) { bestScore = leftScore; bestDirection = leftDir; }
        }

        return bestDirection.normalized * moveSpeed + obstacleSep;
    }

    private Vector2 ComputeObstacleSeparationVelocity(Collider2D allowedCollider, float probeRadius)
    {
        float separationRadius = probeRadius + 0.15f;
        int   count = Physics2D.OverlapCircleNonAlloc(
            (Vector2)transform.position, separationRadius, s_OverlapBuffer, obstacleLayers);

        Vector2 totalPush = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = s_OverlapBuffer[i];
            if (col == null || col == selfCollider) continue;
            if (allowedCollider != null && col == allowedCollider) continue;

            Vector2 closest = col.ClosestPoint((Vector2)transform.position);
            Vector2 pushDir = (Vector2)transform.position - closest;
            float   dist    = pushDir.magnitude;

            if (dist < 0.005f)
            {
                Vector2 toUs = (Vector2)transform.position - (Vector2)col.bounds.center;
                pushDir = toUs.sqrMagnitude > 0.0001f ? toUs.normalized : Vector2.up;
                dist    = 0.005f;
            }

            float penetration = separationRadius - dist;
            if (penetration <= 0f) continue;

            float pushStrength = Mathf.Clamp((penetration / separationRadius) * moveSpeed * 2.5f, 0f, moveSpeed * 2f);
            totalPush += pushDir.normalized * pushStrength;
        }

        return totalPush;
    }

    private float ScoreDirection(
        Vector2 direction, float probeDistance, float remainingDistance,
        Vector2 preferredDirection, Collider2D allowedCollider, float probeRadius)
    {
        int hitCount = Physics2D.CircleCastNonAlloc(
            (Vector2)transform.position, probeRadius, direction,
            s_CastBuffer, probeDistance, obstacleLayers);

        float freeDistance = probeDistance;
        bool  anyBlocked   = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = s_CastBuffer[i];
            if (!IsBlockingHit(hit, remainingDistance, allowedCollider)) continue;
            float d = Mathf.Max(0f, hit.distance);
            if (d < freeDistance) { freeDistance = d; anyBlocked = true; }
        }

        if (anyBlocked && freeDistance < 0.05f) return -1000f;

        float alignment = Vector2.Dot(preferredDirection, direction.normalized);
        return freeDistance * 2f + alignment * probeDistance * 0.35f;
    }

    private bool IsBlockingHit(RaycastHit2D hit, float remainingDistance, Collider2D allowedCollider)
    {
        if (hit.collider == null)                                        return false;
        if (selfCollider    != null && hit.collider == selfCollider)     return false;
        if (allowedCollider != null && hit.collider == allowedCollider)  return false;
        if (hit.distance >= remainingDistance)                           return false;
        return true;
    }

    private float GetProbeRadius()
    {
        if (selfCollider == null) return obstacleProbeRadius;
        Bounds bounds        = selfCollider.bounds;
        float  colliderRadius = Mathf.Min(bounds.extents.x, bounds.extents.y);
        return Mathf.Max(obstacleProbeRadius, colliderRadius * 0.9f);
    }

    private static Vector2 Rotate(Vector2 value, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            (value.x * cos) - (value.y * sin),
            (value.x * sin) + (value.y * cos));
    }

    // ─── SHARED HELPERS ───────────────────────────────────────────────────────

    public void SetManualDestination(Vector3 destination, bool holdPosition, Action<UnitCombatController> onArrived = null)
    {
        manualDestination       = destination;
        holdAtManualDestination = holdPosition;
        manualArrivalCallback   = onArrived;
        hasManualDestination    = true;
        currentTarget           = null;
        desiredVelocity         = Vector2.zero;
        nextRetargetTime        = Time.time + retargetInterval;
    }

    public void ClearManualDestination()
    {
        hasManualDestination    = false;
        holdAtManualDestination = false;
        manualArrivalCallback   = null;
    }

    public void SetAdvanceTarget(CombatTarget target)
    {
        advanceTarget    = target;
        hasAdvanceTarget = target != null;
    }

    public void ClearAdvanceTarget()
    {
        advanceTarget    = null;
        hasAdvanceTarget = false;
    }

    private IEnumerator AttackRoutine()
    {
        if (isAttacking) yield break;

        isAttacking       = true;
        desiredVelocity   = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        nextAttackTime    = Time.time + attackCooldown;

        string chosenAttackState = attackStateName;
        if (!string.IsNullOrWhiteSpace(alternateAttackStateName) && UnityEngine.Random.value > 0.5f)
            chosenAttackState = alternateAttackStateName;

        PlayState(chosenAttackState);

        float hitDelay = Mathf.Clamp(attackDamageDelay, 0f, attackAnimationDuration);
        if (hitDelay > 0f) yield return new WaitForSeconds(hitDelay);

        if (currentTarget != null && currentTarget.IsAlive)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.AimPoint);
            if (distance <= GetAttackDistanceForTarget(currentTarget) + 0.35f)
                currentTarget.TakeDamage(attackDamage);
        }

        float remainingTime = Mathf.Max(0f, attackAnimationDuration - hitDelay);
        if (remainingTime > 0f) yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    private CombatTarget FindBestTarget()
    {
        CombatTarget[] targets     = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CombatTarget   bestTarget  = null;
        float          bestDistSqr = detectionRange * detectionRange;

        foreach (CombatTarget target in targets)
        {
            if (target == null || target == selfTarget || !target.IsAlive)  continue;
            if (!target.IsEnemy(selfTarget.Team))                           continue;
            if ((validTargetTypes & target.TargetType) == 0)               continue;

            float distSqr = ((Vector2)(target.AimPoint - transform.position)).sqrMagnitude;
            if (distSqr > bestDistSqr) continue;

            bestDistSqr = distSqr;
            bestTarget  = target;
        }
        return bestTarget;
    }

    private CombatTarget ResolveAdvanceTarget()
    {
        if (!hasAdvanceTarget) return null;

        if (advanceTarget == null || !advanceTarget.IsAlive ||
            !advanceTarget.IsEnemy(selfTarget.Team) ||
            (advanceTarget.TargetType & CombatTargetType.Building) == 0)
            advanceTarget = FindClosestEnemyBuilding();

        hasAdvanceTarget = advanceTarget != null;
        return advanceTarget;
    }

    private CombatTarget FindClosestEnemyBuilding()
    {
        CombatTarget[] targets    = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CombatTarget   closest    = null;
        float          bestDistSqr = float.MaxValue;

        foreach (CombatTarget target in targets)
        {
            if (target == null || target == selfTarget || !target.IsAlive)  continue;
            if (!target.IsEnemy(selfTarget.Team))                           continue;
            if ((target.TargetType & CombatTargetType.Building) == 0)      continue;

            float distSqr = ((Vector2)(target.AimPoint - transform.position)).sqrMagnitude;
            if (distSqr >= bestDistSqr) continue;

            bestDistSqr = distSqr;
            closest     = target;
        }
        return closest;
    }

    private void UpdateManualMovement()
    {
        currentTarget = null;
        if (isAttacking) { desiredVelocity = Vector2.zero; return; }

        Vector2 toDestination   = manualDestination - transform.position;
        float   arrivalDistance = Mathf.Max(0.1f, stoppingDistance * 0.6f);
        float   distance        = toDestination.magnitude;

        if (distance <= arrivalDistance)
        {
            desiredVelocity = Vector2.zero;
            if (holdAtManualDestination) return;
            Action<UnitCombatController> callback = manualArrivalCallback;
            ClearManualDestination();
            callback?.Invoke(this);
            return;
        }

        desiredVelocity = ResolveMovementVelocity(toDestination, Mathf.Max(0.1f, distance - arrivalDistance), null)
                        + ComputeUnitSeparationVelocity();
    }

    private float GetAttackDistanceForTarget(CombatTarget target)
    {
        float attackDistance = stoppingDistance;
        if (target == null) return attackDistance;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider == null) return attackDistance;
        Bounds bounds     = targetCollider.bounds;
        float  bonusRange = Mathf.Max(bounds.extents.x, bounds.extents.y * 0.75f);
        return attackDistance + bonusRange;
    }

    private void UpdateFacing()
    {
        if (!flipSpriteByMovement || spriteRenderer == null) return;
        Vector2 facingVector = desiredVelocity;
        if (facingVector.sqrMagnitude < 0.01f && currentTarget != null)
            facingVector = currentTarget.AimPoint - transform.position;
        if (Mathf.Abs(facingVector.x) > 0.01f)
            spriteRenderer.flipX = facingVector.x < 0f;
    }

    private void UpdateAnimation()
    {
        if (animator == null || isAttacking) return;
        bool isMoving = desiredVelocity.sqrMagnitude > 0.01f;
        PlayState(isMoving ? runStateName : idleStateName);
    }

    private void PlayState(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName)) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) return;
        animator.Play(stateName, 0, 0f);
    }

    private void CacheReferences()
    {
        if (animator       == null) animator       = GetComponent<Animator>();
        if (rb             == null) rb             = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (selfTarget     == null) selfTarget     = GetComponent<CombatTarget>();
        if (selfCollider   == null) selfCollider   = GetComponent<Collider2D>();
    }

    private void OnValidate()
    {
        attackAnimationDuration = Mathf.Max(0.1f, attackAnimationDuration);
        attackCooldown          = Mathf.Max(0.05f, attackCooldown);
        attackDamageDelay       = Mathf.Clamp(attackDamageDelay, 0f, attackAnimationDuration);
        stoppingDistance        = Mathf.Max(0.1f, stoppingDistance);
        detectionRange          = Mathf.Max(stoppingDistance, detectionRange);
        retargetInterval        = Mathf.Max(0.05f, retargetInterval);
        moveSpeed               = Mathf.Max(0f, moveSpeed);
        attackDamage            = Mathf.Max(0.1f, attackDamage);
        obstacleProbeDistance   = Mathf.Max(0.2f, obstacleProbeDistance);
        obstacleProbeRadius     = Mathf.Max(0.05f, obstacleProbeRadius);
        avoidanceSamplesPerSide = Mathf.Max(1, avoidanceSamplesPerSide);
        avoidanceAngleStep      = Mathf.Clamp(avoidanceAngleStep, 5f, 90f);
        unitSeparationRadius    = Mathf.Max(0.1f, unitSeparationRadius);
        unitSeparationStrength  = Mathf.Max(0f, unitSeparationStrength);
        CacheReferences();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.75f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.45f, 0.35f, 0.55f);
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // Green ring = unit separation radius
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, unitSeparationRadius);

        // Yellow line = orbit aim point
        if (currentTarget != null)
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.8f);
            Vector2 orbit = GetOrbitAimPoint(currentTarget);
            Gizmos.DrawLine(transform.position, orbit);
            Gizmos.DrawWireSphere(orbit, 0.12f);
        }
    }
}