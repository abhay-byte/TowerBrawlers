using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DollyPathInputController : MonoBehaviour
{
    [Header("Optional Dolly Control")]
    [SerializeField] private Component controlledDolly;

    [Header("Castle Anchors")]
    [SerializeField] private CombatTarget playerCastle;
    [SerializeField] private CombatTarget enemyCastle;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.35f;
    [SerializeField] private float dragSensitivity = 1.15f;
    [SerializeField] private Key moveTowardRedKey = Key.D;
    [SerializeField] private Key moveTowardBlueKey = Key.A;
    [SerializeField] private Key snapBlueKey = Key.Home;
    [SerializeField] private Key snapRedKey = Key.End;
    [SerializeField] [Range(0f, 1f)] private float initialNormalizedPosition;

    private PropertyInfo cameraPositionProperty;
    private float normalizedPosition;
    private float fixedY;
    private float fixedZ;
    private bool blockCurrentPointerDrag;
    private bool castleOffsetInitialized;
    private float cameraOffsetFromBlueCastle;

    private void Awake()
    {
        CacheProperty();
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        ResolveCastles();
        CacheCastleOffset();
        SetPosition(initialNormalizedPosition);
    }

    private void OnEnable()
    {
        CacheProperty();
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        ResolveCastles();
        CacheCastleOffset();
        SetPosition(initialNormalizedPosition);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        ResolveCastles();
        HandlePointerDrag();

        float currentPosition = GetPosition();
        float direction = 0f;

        if (Keyboard.current != null && Keyboard.current[moveTowardBlueKey].isPressed)
            direction -= 1f;

        if (Keyboard.current != null && Keyboard.current[moveTowardRedKey].isPressed)
            direction += 1f;

        if (direction != 0f)
            SetPosition(currentPosition + (direction * moveSpeed * Time.unscaledDeltaTime));

        if (Keyboard.current != null && Keyboard.current[snapBlueKey].wasPressedThisFrame)
            SetPosition(0f);

        if (Keyboard.current != null && Keyboard.current[snapRedKey].wasPressedThisFrame)
            SetPosition(1f);
    }

    private void CacheProperty()
    {
        cameraPositionProperty = controlledDolly != null
            ? controlledDolly.GetType().GetProperty("CameraPosition", BindingFlags.Instance | BindingFlags.Public)
            : null;
    }

    private float GetPosition()
    {
        if (cameraPositionProperty == null || controlledDolly == null)
            return normalizedPosition;

        object value = cameraPositionProperty.GetValue(controlledDolly);
        return value is float position ? position : 0f;
    }

    private void SetPosition(float position)
    {
        normalizedPosition = Mathf.Clamp01(position);

        if (cameraPositionProperty != null && controlledDolly != null)
        {
            cameraPositionProperty.SetValue(controlledDolly, normalizedPosition);
            return;
        }

        ApplyCastleCameraPosition();
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        dragSensitivity = Mathf.Max(0.1f, dragSensitivity);
        initialNormalizedPosition = Mathf.Clamp01(initialNormalizedPosition);
        CacheProperty();
    }

    private void HandlePointerDrag()
    {
        if (TryGetTouchPointer(out Vector2 touchPosition, out Vector2 touchDelta, out bool touchStarted, out bool touchEnded))
        {
            UpdateDragState(touchPosition, touchStarted, touchEnded);
            if (!blockCurrentPointerDrag)
                SetPosition(GetPosition() + ((touchDelta.x / Mathf.Max(1f, Screen.width)) * dragSensitivity));

            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        bool started = mouse.leftButton.wasPressedThisFrame;
        bool ended = mouse.leftButton.wasReleasedThisFrame;
        Vector2 mousePosition = mouse.position.ReadValue();
        UpdateDragState(mousePosition, started, ended);

        if (!mouse.leftButton.isPressed || blockCurrentPointerDrag)
            return;

        Vector2 delta = mouse.delta.ReadValue();
        SetPosition(GetPosition() + ((delta.x / Mathf.Max(1f, Screen.width)) * dragSensitivity));
    }

    private void UpdateDragState(Vector2 screenPosition, bool started, bool ended)
    {
        if (started)
            blockCurrentPointerDrag = IsScreenPositionOverUi(screenPosition);

        if (ended)
            blockCurrentPointerDrag = false;
    }

    private bool IsScreenPositionOverUi(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        return raycastResults.Count > 0;
    }

    private bool TryGetTouchPointer(out Vector2 position, out Vector2 delta, out bool started, out bool ended)
    {
        position = Vector2.zero;
        delta = Vector2.zero;
        started = false;
        ended = false;

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return false;

        var touch = touchscreen.primaryTouch;
        bool active = touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame;
        if (!active)
            return false;

        position = touch.position.ReadValue();
        delta = touch.delta.ReadValue();
        started = touch.press.wasPressedThisFrame;
        ended = touch.press.wasReleasedThisFrame;
        return true;
    }

    private void ResolveCastles()
    {
        if (playerCastle == null)
            playerCastle = FindCastleForTeam(CombatTeam.Blue);

        if (enemyCastle == null)
            enemyCastle = FindCastleForTeam(CombatTeam.Red);
    }

    private CombatTarget FindCastleForTeam(CombatTeam team)
    {
        CombatTarget[] targets = FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (CombatTarget target in targets)
        {
            if (target == null || target.Team != team)
                continue;

            if ((target.TargetType & CombatTargetType.Building) == 0)
                continue;

            if (target.name.IndexOf("castle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return target;
        }

        return null;
    }

    private void CacheCastleOffset()
    {
        if (castleOffsetInitialized || playerCastle == null)
            return;

        cameraOffsetFromBlueCastle = transform.position.x - playerCastle.transform.position.x;
        castleOffsetInitialized = true;
    }

    private void ApplyCastleCameraPosition()
    {
        if (playerCastle == null || enemyCastle == null)
            return;

        CacheCastleOffset();

        float blueX = playerCastle.transform.position.x + cameraOffsetFromBlueCastle;
        float redX = enemyCastle.transform.position.x + cameraOffsetFromBlueCastle;
        float cameraX = Mathf.Lerp(blueX, redX, normalizedPosition);
        transform.position = new Vector3(cameraX, fixedY, fixedZ);
    }
}
