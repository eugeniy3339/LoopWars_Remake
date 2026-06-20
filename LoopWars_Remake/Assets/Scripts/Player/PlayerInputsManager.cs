using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : MonoBehaviour
{
    private Character character;

    private Vector2 lastMoveInput;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        character.movement.OnMove(input.x);
        if (input.magnitude > 0.1f)
            lastMoveInput = input;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        character.jump.JumpIfCanTo();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        character.dash.DashIfCanTo(lastMoveInput);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started)
            character.weaponManager.StartAttacking();
        else if(context.canceled)
            character.weaponManager.StopAttacking();
    }
}
