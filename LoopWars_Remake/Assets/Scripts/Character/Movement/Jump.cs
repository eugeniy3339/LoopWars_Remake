using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Character), typeof(Movement))]
public class Jump : NetworkBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;
    private MovementManager movementManager;

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
        movementManager = character.movementManager;
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
        movementManager.limitSpeedOnSlopes = true;
    }

    public void JumpIfCanTo()
    {
        if (!CanJump()) return;

        movementManager.limitSpeedOnSlopes = false;
        curJumpCooldown = minJumpCooldown;

        rigidbody.linearVelocityY = jumpForce;
    }

    public bool CanJump()
    {
        if (!IsOwner) return false;
        return movementManager.isGrounded && curJumpCooldown <= 0f;
    }
}
