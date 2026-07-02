using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : NetworkBehaviour
{
    private Character character;
    private PlayerInput _pI;
    private PlayerInput playerInput 
    { get { if (_pI == null) { _pI = character.playerInput != null ? character.playerInput : GetComponent<PlayerInput>(); } return _pI; } }
    private MovementManager movementManager;
    private WeaponManager weaponManager;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        weaponManager = character.weaponManager;
        movementManager = character.movementManager;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            Destroy(playerInput);
        }
    }

    private void Update()
    {
        if(playerInput.currentControlScheme == "KeyBoard")
        {
            KeyBoardAim();
        }
    }

    private void KeyBoardAim()
    {
        if (!IsOwner) return;

        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Mouse.current.position.x.magnitude, Mouse.current.position.y.magnitude));
        weaponManager.direction = worldMousePosition - new Vector2(transform.position.x, transform.position.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Vector2 input = context.ReadValue<Vector2>();
        movementManager.OnMove(input);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.started) return;

        movementManager.OnJump();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.started)
            character.weaponManager.StartAttacking();
        if(context.canceled)
            character.weaponManager.StopAttacking();
    }

    public void OnThrowWeapon(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.started) return;

        character.weaponManager.ThrowWeapon();

    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Vector2 value = context.ReadValue<Vector2>();
        if(value.magnitude >= 0.1f)
        {
            weaponManager.direction = value;
        }
    }
}
