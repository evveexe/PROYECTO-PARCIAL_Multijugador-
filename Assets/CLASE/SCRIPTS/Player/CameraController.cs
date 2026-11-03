using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float minAngleY = -80f;
    [SerializeField] private float maxAngleY = 80f;
    [SerializeField] private SimpleKCC kcc;

    [SerializeField, Range(0, 0.1f)] private float walkingAmplitude = 0.015f;
    [SerializeField, Range(0, 0.1f)] private float runningAmplitude = 0.015f;
    [SerializeField, Range(0, 15)] private float walkingFrequency = 10f;
    [SerializeField, Range(10, 20)] private float runningFrequency = 18f;
    [SerializeField] private float walkingSpeed = 1f;
    [SerializeField] private float resetPosSpeed = 3f;
    [SerializeField] private bool moveHead;

    [Header("Audio")]
    [SerializeField] private AudioListener audioListener;

    private Vector3 startPos;
    private InputManager inputManager;
    private bool isInitialized = false;
    private Camera cameraComponent;

    [Networked] private float _aimPitch { get; set; }
    [Networked] private float _aimYaw { get; set; }

    private int _selfLayer = -1;
    private int _defaultLayer = 0;

    private void Awake()
    {
        if (kcc == null) kcc = GetComponentInParent<SimpleKCC>();
        startPos = transform.localPosition;

        cameraComponent = GetComponent<Camera>();
        if (audioListener == null) audioListener = GetComponent<AudioListener>();
    }

    public override void Spawned()
    {
        if (isInitialized) return;
        isInitialized = true;

        inputManager = InputManager.Instance != null ? InputManager.Instance : FindObjectOfType<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError("No se encontró InputManager en la escena");
            return;
        }

        if (HasInputAuthority)
        {
            if (cameraComponent != null)
            {
                cameraComponent.enabled = true;
            }

            if (audioListener != null)
            {
                audioListener.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ApplyBodyLayer(local: true);
        }
        else
        {
            if (cameraComponent != null)
            {
                cameraComponent.enabled = false;
            }

            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            ApplyBodyLayer(local: false);
        }
    }

    private void ApplyBodyLayer(bool local)
    {
        if (_selfLayer < 0)
        {
            _selfLayer = LayerMask.NameToLayer("Self");
            if (_selfLayer < 0) _selfLayer = 0;
        }

        int targetLayer = local ? _selfLayer : _defaultLayer;
        Transform root = transform.root;

        Transform cameraT = root.Find("Main Camera");
        SetLayerRecursivelyExcept(root.gameObject, targetLayer, cameraT);

    }

    private static void SetLayerRecursivelyExcept(GameObject obj, int layer, Transform excludeRoot)
    {
        obj.layer = layer;
        var transforms = obj.GetComponentsInChildren<Transform>(includeInactive: true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (excludeRoot != null && t.IsChildOf(excludeRoot)) continue;
            t.gameObject.layer = layer;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority || HasStateAuthority)
        {
            if (kcc != null && GetInput(out NetworkInfoData input))
            {
                Vector2 lookDelta = new Vector2(input.mouseDeltaX, input.mouseDeltaY) * mouseSensitivity * 0.1f;
                Vector2 lookBefore = kcc.GetLookRotation();
                kcc.SetLookRotation(lookBefore.x - lookDelta.y, lookBefore.y + lookDelta.x);
                Vector2 lookAfter = kcc.GetLookRotation();

                _aimPitch = lookAfter.x;
                _aimYaw = lookAfter.y;
            }
        }

        Transform playerRoot = transform.root;
        if (playerRoot != null)
        {
            playerRoot.rotation = Quaternion.Euler(0f, _aimYaw, 0f);
            transform.localRotation = Quaternion.Euler(_aimPitch, 0f, 0f);
        }
    }

    public override void Render()
    {
        if (!HasInputAuthority) return;

        if (kcc != null)
        {
            Vector2 lr = kcc.GetLookRotation();
            transform.localRotation = Quaternion.Euler(lr.x, 0f, 0f);
        }
        if (moveHead) { BlobMove(); ResetPosition(); }
    }

    private void BlobMove()
    {
        if (inputManager == null || !inputManager.IsMoveInputPressed()) return;
        transform.localPosition += inputManager.WasRunInputPressed() ? RunningFootStepMotion() : FootStepMotion();
    }

    private void ResetPosition()
    {
        if (transform.localPosition == startPos) return;
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Time.deltaTime);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }

    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }
}
