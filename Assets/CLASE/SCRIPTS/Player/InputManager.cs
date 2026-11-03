using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerControls playerControls;

    private Vector2 _accumulatedMouseDelta;
    private float _accumulatedScroll;
    private bool _fireLatched;
    private bool _reloadLatched;

    private const float MaxAccumulatedDelta = 300f;
    private const float MaxAccumulatedScroll = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerControls = new PlayerControls();
        Debug.Log($"InputManager creado en {gameObject.scene.name} - ID: {GetInstanceID()}");
    }

    private void Update()
    {
        if (playerControls == null) return;

        _accumulatedMouseDelta += playerControls.Player.Look.ReadValue<Vector2>();
        if (_accumulatedMouseDelta.magnitude > MaxAccumulatedDelta)
            _accumulatedMouseDelta = _accumulatedMouseDelta.normalized * MaxAccumulatedDelta;

        if (Mouse.current != null)
            _accumulatedScroll += Mouse.current.scroll.ReadValue().y;
        _accumulatedScroll = Mathf.Clamp(_accumulatedScroll, -MaxAccumulatedScroll, MaxAccumulatedScroll);

        if (playerControls.Player.Fire.WasPressedThisFrame())
            _fireLatched = true;
        if (playerControls.Player.Reload.WasPressedThisFrame())
            _reloadLatched = true;
    }

    private void OnEnable()
    {
        if (playerControls != null)
            playerControls.Enable();
    }

    private void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
    }

    private void OnDestroy()
    {
        if (playerControls != null)
        {
            playerControls.Disable();
            playerControls.Dispose();
        }
        if (Instance == this) Instance = null;
        Debug.Log($"InputManager destruido - ID: {GetInstanceID()}");
    }

    public Vector2 GetMoveInput()
    {
        if (playerControls == null) return Vector2.zero;
        return playerControls.Player.Move.ReadValue<Vector2>();
    }

    public bool IsMoveInputPressed()
    {
        if (playerControls == null) return false;
        return playerControls.Player.Move.IsPressed();
    }

    public bool WasRunInputPressed()
    {
        if (playerControls == null) return false;
        return playerControls.Player.Run.IsPressed();
    }

    public bool IsMovingBackwards()
    {
        if (playerControls == null) return false;
        return playerControls.Player.Move.ReadValue<Vector2>().y < 0;
    }

    public bool IsMovingOnXAxis()
    {
        if (playerControls == null) return false;
        return playerControls.Player.Move.ReadValue<Vector2>().x != 0;
    }

    public Vector2 GetMouseDelta()
    {
        Vector2 result = _accumulatedMouseDelta;
        _accumulatedMouseDelta = Vector2.zero;
        return result;
    }

    public bool WasFirePressed()
    {
        bool result = _fireLatched;
        _fireLatched = false;
        return result;
    }

    public bool WasReloadPressed()
    {
        bool result = _reloadLatched;
        _reloadLatched = false;
        return result;
    }

    public float GetWeaponScroll()
    {
        float result = _accumulatedScroll;
        _accumulatedScroll = 0f;
        return result;
    }
}