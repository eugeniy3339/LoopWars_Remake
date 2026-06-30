using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : NetworkBehaviour
{
    private Character character;
    private PlayerInput playerInput;
    private WeaponManager weaponManager;

    private Vector2 lastMoveInput;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        playerInput = character.playerInput;
        weaponManager = character.weaponManager;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) enabled = false;
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
        character.movement.OnMove(input.x);
        if (input.magnitude > 0.1f)
            lastMoveInput = input;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.started) return;

        character.jump.JumpIfCanTo();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.started) return;

        character.dash.DashIfCanTo(lastMoveInput);
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
