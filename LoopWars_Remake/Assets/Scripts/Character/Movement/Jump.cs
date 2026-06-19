using UnityEngine;

[RequireComponent(typeof(Character), typeof(Movement))]
public class Jump : MonoBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;
    private Movement movement;

    [SerializeField] private float jumpForce = 8f;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        rigidbody = character.rigidbody;
        movement = character.movement;
    }

    public void JumpIfCanTo()
    {
        if (!CanJump()) return;

        rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private bool CanJump()
    {
        return movement.isGrounded;
    }
}
