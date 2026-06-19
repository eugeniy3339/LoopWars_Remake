using System;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class Movement : MonoBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float airMultiplier = 0.8f;
    [SerializeField] private float groundDrag = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform footPosition;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;

    public bool isGrounded { get; protected set; }
    private RaycastHit2D groundHit;
    private bool onSlope;

    private float lastMoveDir;
    private float moveDir;

    public event Action<float> onChangedMoveDir;

    public event Action onGrounded;
    public event Action onUngrounded;

    [HideInInspector] public bool canMove = true;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        rigidbody = character.rigidbody;
    }

    private void Update()
    {
        bool isGrounded = IsGrounded(out groundHit);
        
        if(isGrounded != this.isGrounded)
        {
            if(isGrounded)
            {
                onGrounded?.Invoke();
            }
            else
            {
                onUngrounded?.Invoke();
            }

            rigidbody.linearDamping = isGrounded ? groundDrag : 0f;
            this.isGrounded = isGrounded;
        }
        onSlope = OnSlope(groundHit);
        character.useGravity = !onSlope;
    }

    private void FixedUpdate()
    {
        Move(moveDir);
    }

    public void Move(float dir)
    {
        if (!canMove) return;
        if (Mathf.Abs(dir) <= 0.1f) return;
        Vector2 moveDir = new Vector2(dir, 0f).normalized;

        if (onSlope)
            moveDir = GetSlopeMoveDirection(moveDir, groundHit);

        if(CanAddForce(moveDir))
            rigidbody.AddForce(moveDir * speed * 80f * (isGrounded ? 1f : airMultiplier), ForceMode2D.Force);
    }

    private bool IsGrounded(out RaycastHit2D raycastHit)
    {
        Vector2 raycastOrigin = footPosition.position + Vector3.up * groundCheckRadius;
        raycastHit = Physics2D.CircleCast(raycastOrigin, groundCheckRadius, Vector2.down, groundCheckDistance, groundLayer);
        return raycastHit;
    }

    private bool OnSlope(RaycastHit2D raycastHit)
    {
        if(!raycastHit)
            return false;

        float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        return angle > 0f && angle < maxSlopeAngle;
    }

    private Vector3 GetSlopeMoveDirection(Vector2 moveDirection, RaycastHit2D raycastHit)
    {
        return Vector3.ProjectOnPlane(moveDirection, raycastHit.normal).normalized;
    }

    private bool CanAddForce(Vector2 moveDir)
    {
        Vector2 velocity = onSlope ? rigidbody.linearVelocity : new Vector2(rigidbody.linearVelocity.x, 0f);
        float velocityForce = velocity.magnitude;
        Vector2 normalizedVelocity = velocity.normalized;

        return velocityForce <= speed || Vector2.Dot(normalizedVelocity, moveDir) <= 0.1f;
    }

    public void OnMove(float moveDir)
    {
        moveDir = BetterMath.NormalizedFloat(moveDir);
        this.moveDir = moveDir;
        if(lastMoveDir != moveDir)
        {
            onChangedMoveDir?.Invoke(moveDir);
            lastMoveDir = moveDir;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (footPosition == null)
            return;

        Gizmos.color = Color.red;
        Vector2 raycastOrigin = footPosition.position + Vector3.up * groundCheckRadius;
        Gizmos.DrawWireSphere(raycastOrigin + Vector2.down * groundCheckDistance, groundCheckRadius);
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * groundCheckDistance);
    }

#endif
}
