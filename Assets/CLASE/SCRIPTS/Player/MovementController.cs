using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;

    private InputManager inputManager;
    private bool isInitialized = false;
    private WeaponController weaponController;

    public override void Spawned()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (kcc == null) kcc = GetComponent<SimpleKCC>();
        weaponController = GetComponent<WeaponController>();

        inputManager = InputManager.Instance != null ? InputManager.Instance : FindFirstObjectByType<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError("No se encontró InputManager en la escena");
            return;
        }

        Gravity gravityComponent = GetComponent<Gravity>();
        if (kcc != null && gravityComponent != null) kcc.SetGravity(-gravityComponent.GetGravityForce());

        if (HasInputAuthority || HasStateAuthority)
        {
            enabled = true;
            if (HasInputAuthority)
                gameObject.name = $"Player_{Object.InputAuthority.PlayerId}";
        }
        else
        {
            enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if ((!HasInputAuthority && !HasStateAuthority) || !enabled) return;

        if (GetInput(out NetworkInfoData input))
        {
            Movement(input);

            if (HasStateAuthority && weaponController != null)
            {
                weaponController.ServerProcessInput(input.fireInputPressed, input.weaponScroll);
            }
        }

        if (HasInputAuthority && inputManager != null && _animator != null)
        {
            _animator.SetBool("IsWalking", inputManager.IsMoveInputPressed());
            _animator.SetBool("IsRunning", inputManager.WasRunInputPressed());
            _animator.SetFloat("WalkingZ", inputManager.GetMoveInput().y);
            _animator.SetFloat("WalkingX", inputManager.GetMoveInput().x);
        }
    }

    private void Movement(NetworkInfoData input)
    {
        if (kcc == null) return;

        Vector3 localInput = new Vector3(input.movementAxis.x, 0f, input.movementAxis.y);
        Vector3 worldDir = transform.rotation * localInput;
        kcc.Move(worldDir * (Runner.DeltaTime * Speed(input)));
    }

    private float Speed(NetworkInfoData input)
    {
        if (input.movingBackwards || input.movingOnXAxis) return walkSpeed;
        return input.runInputPressed ? runSpeed : walkSpeed;
    }

    public bool IsReady()
    {
        return isInitialized && HasInputAuthority && inputManager != null;
    }
}
