using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Character), typeof(Movement))]
public class Jump : NetworkBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;
    private Movement movement;

    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float minJumpCooldown = 0.1f;
    private float curJumpCooldown = 0f;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        rigidbody = character.rigidbody;
        movement = character.movement;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) enabled = false;
    }

    private void Update()
    {
        JumpCooldown();
    }

    private void JumpCooldown()
    {
        if(curJumpCooldown > 0f)
        {
            curJumpCooldown -= Time.deltaTime;
            if(curJumpCooldown <= 0f)
            {
                OnJumpCooldown();
            }
        }
    }

    private void OnJumpCooldown()
    {
        movement.limitSpeedOnSlopes = true;
    }

    public void JumpIfCanTo()
    {
        if (!CanJump()) return;

        character.movement.limitSpeedOnSlopes = false;
        curJumpCooldown = minJumpCooldown;

        rigidbody.linearVelocityY = jumpForce;
    }

    private bool CanJump()
    {
        if (!IsOwner) return false;
        return movement.isGrounded && curJumpCooldown <= 0f;
    }
}
