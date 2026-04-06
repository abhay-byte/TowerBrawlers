using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string attackStateName = "Shoot";

    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Vector2 currentVelocity;
    private Vector2 smoothDampVelocity;
    private float lastAttackTime;
    private bool isAttacking;
    [SerializeField] private float attackCooldown = 0.5f;
    private bool isInitialized;
    private InputAction moveAction;
    private InputAction attackAction;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize Input System
        SetupInput();

        isInitialized = true;
    }

    private void SetupInput()
    {
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Move", throwIfNotFound: false);
            attackAction = inputActions.FindAction("Attack", throwIfNotFound: false);

            moveAction?.Enable();
            attackAction?.Enable();
        }

        if (moveAction == null || attackAction == null)
        {
            Debug.LogWarning("PlayerController requires Move and Attack actions from the assigned InputActionAsset.");
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.performed += OnAttackPerformed;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        PerformAttack();
    }

    // Called by Animation event when attack animation ends
    private void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
        if (attackAction != null)
        {
            attackAction.Disable();
        }
    }

    private void Update()
    {
        if (!isInitialized) return;
        UpdateAttackState();
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;
        UpdateMovement();
    }

    private void HandleInput()
    {
        if (isAttacking)
        {
            currentVelocity = Vector2.zero;
            smoothDampVelocity = Vector2.zero;
            animator.SetFloat(MoveSpeedHash, 0f);
            return;
        }

        Vector2 inputVector = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector2 targetVelocity = inputVector.normalized * moveSpeed;

        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref smoothDampVelocity,
            targetVelocity.magnitude > 0 ? 1f / acceleration : 1f / deceleration
        );

        animator.SetFloat(MoveSpeedHash, currentVelocity.magnitude);

        if (spriteRenderer != null && Mathf.Abs(inputVector.x) > 0.01f)
        {
            spriteRenderer.flipX = inputVector.x < 0;
        }
    }

    private void UpdateMovement()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = currentVelocity;
    }

    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    private void PerformAttack()
    {
        if (!CanAttack() || isAttacking)
            return;

        isAttacking = true;
        currentVelocity = Vector2.zero;
        smoothDampVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        lastAttackTime = Time.time;
        animator.SetFloat(MoveSpeedHash, 0f);
        animator.SetTrigger(AttackHash);
        Debug.Log($"{gameObject.name} performs attack!");
    }

    private void UpdateAttackState()
    {
        if (!isAttacking || animator == null)
            return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
        bool isInAttackState = currentState.IsName(attackStateName);
        bool isTransitioningToAttack = animator.IsInTransition(0) && nextState.IsName(attackStateName);

        if (isInAttackState || isTransitioningToAttack)
            return;

        if (Time.time > lastAttackTime)
            isAttacking = false;
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }

    public bool IsMoving()
    {
        return currentVelocity.sqrMagnitude > 0.01f;
    }

    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
