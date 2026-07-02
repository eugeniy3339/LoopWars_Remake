using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class MovementManager : NetworkBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;
    private Movement movement;
    private Jump jump;
    private Dash dash;

    public bool isGrounded { get; private set; }
    public bool onTheWall { get; private set; }
    public bool onSlope { get; private set; }
    public RaycastHit2D groundHit;

    [SerializeField] private float groundDrag = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform _footPosition;
    public Transform footPosition { get { return _footPosition; } }
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;

    public float lastMoveDir { get; private set; }
    public float moveDir { get; private set; }
    private Vector2 lastMoveInput;

    public event Action<float> onChangedMoveDir;

    public event Action onGrounded;
    public event Action onUngrounded;

    public event Action<Collider2D> onGotOnTheWall;
    public event Action onGotOffTheWall;


    private float _lD;
    public float linearDamping
    {
        get { return _lD; }
        set
        {
            if (!canChangeLinearDamping) return;

            _lD = value;
            rigidbody.linearDamping = value;
        }
    }


    const float defaultGravityScale = 3f;

    private bool _ug;
    public bool useGravity
    {
        get { return _ug; }
        set
        {
            if (!canChangeUseGravity) return;
            _ug = value;
            rigidbody.gravityScale = value ? defaultGravityScale : 0f;
        }
    }


    private bool canChangeLinearDamping = true;
    private bool canChangeUseGravity = true;

    [HideInInspector] public bool limitSpeedOnSlopes = true;
    [HideInInspector] public bool canMove = true;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        rigidbody = character.rigidbody;
        movement = character.movement;
        jump = character.jump;
        dash = character.dash;

        useGravity = true;

        SubscribeOnEvents();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) enabled = false;
    }



    private void Update()
    {
        bool isGrounded = IsGrounded(out groundHit);

        if (isGrounded != this.isGrounded)
        {
            if (isGrounded)
            {
                onGrounded?.Invoke();
            }
            else
            {
                onUngrounded?.Invoke();
            }

            linearDamping = isGrounded ? groundDrag : 0f;
            this.isGrounded = isGrounded;
        }

        onSlope = OnSlope(groundHit);
        useGravity = !onSlope;
    }

    private bool IsGrounded(out RaycastHit2D raycastHit)
    {
        Vector2 raycastOrigin = footPosition.position + Vector3.up * groundCheckRadius;
        raycastHit = Physics2D.CircleCast(raycastOrigin, groundCheckRadius, Vector2.down, groundCheckDistance, groundLayer);
        return raycastHit;
    }

    private bool OnSlope(RaycastHit2D raycastHit)
    {
        if (!raycastHit)
            return false;

        float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        return angle > 0f && angle < maxSlopeAngle;
    }



    private void OnStartedDash()
    {
        canChangeLinearDamping = true;
        canChangeUseGravity = true;

        useGravity = false;
        linearDamping = 0f;

        canChangeLinearDamping = false;
        canChangeUseGravity = false;
    }

    private void OnEndedDash()
    {
        canChangeLinearDamping = true;
        canChangeUseGravity = true;

        useGravity = true;
    }



    private void OnGotOnTheWall(Collider2D wall)
    {

    }

    private void OnGotOffWall()
    {

    }



    private void OnEnable()
    {
        SubscribeOnEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print(collision.contacts[0].normal);
    }



    private void SubscribeOnEvents()
    {
        if(dash != null && !dash.subscribedOnEvents)
        {
            dash.onDash += OnStartedDash;
            dash.onDashEnd += OnEndedDash;

            dash.subscribedOnEvents = true;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (dash != null)
        {
            dash.onDash -= OnStartedDash;
            dash.onDashEnd -= OnEndedDash;

            dash.subscribedOnEvents = false;
        }
    }



    public void OnMove(Vector2 moveDir)
    {
        if (moveDir.magnitude > 0.1f)
            lastMoveInput = moveDir;
        OnMove(moveDir.x);
    }

    private void OnMove(float moveDir)
    {
        if (!IsOwner) return;

        moveDir = BetterMath.NormalizedFloat(moveDir);
        this.moveDir = moveDir;
        if (lastMoveDir != moveDir)
        {
            onChangedMoveDir?.Invoke(moveDir);
            lastMoveDir = moveDir;
        }
    }

    public void OnJump()
    {
        if (jump.CanJump())
            jump.JumpIfCanTo();
        else
            dash.DashIfCanTo(lastMoveInput);
    }
}
