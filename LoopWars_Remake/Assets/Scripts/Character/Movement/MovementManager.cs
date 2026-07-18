using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class MovementManager : NetworkBehaviour
{
    private Character character;

    private Rigidbody2D rigidbody;
    private Movement movement;
    private Jump jump;
    private Dash dash;

    private MovementState _mS;
    public MovementState movementState
    {
        get { return _mS; }
        private set
        {
            if (_mS == value) { return; }
            _mS = value;
            onMovementStateChanged?.Invoke(value);
        }
    }
    public event Action<MovementState> onMovementStateChanged;

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

    [Header("On Wall Settings")]
    [SerializeField] private float onWallDrag = 10f;

    [Header("MovementSettings")]
    [SerializeField] private float _maxMovementCurrency = 100f;
    public float maxMovementCurrency { get { return _maxMovementCurrency; } }
    private float _cMC;
    public float curMovementCurrency
    {
        get { return _cMC; }
        private set
        {
            if (value > maxMovementCurrency)
                value = maxMovementCurrency;
            else if (value < 0f)
                value = 0f;

            _cMC = value;
        }
    }
    [SerializeField] private float movementCurrencyPerSecond = 30f;

    [SerializeField] private Transform _movementCurrencyVisualTransform;
    public Transform movementCurrencyVisualTransform { get { return _movementCurrencyVisualTransform; } }

    public bool onWall { get; private set; }
    private List<Collider2D> wallsImCollidingWith = new List<Collider2D>();
    public Collider2D curWall { get; private set; }

    public event Action onGrounded;
    public event Action onUngrounded;
    public static event Action<Character> onGroundedStatic;
    public static event Action<Character> onUngroundedStatic;

    public event Action<Collider2D> onGotOnTheWall;
    public event Action onGotOffTheWall;

    public static event Action<Character> onStartedMoving;
    public static event Action<Character> onStopedMoving;

    public float lastMoveDir { get; private set; }
    public float moveDir { get; private set; }
    private Vector2 lastMoveInput;

    public event Action<float> onChangedMoveDir;


    private bool canChangeLinearDamping = true;
    private bool canChangeUseGravity = true;

    private float _lD;
    public float linearDamping
    {
        get { return _lD; }
        private set
        {
            if (!canChangeLinearDamping) return;

            _lD = value;
            rigidbody.linearDamping = value;
        }
    }


    public const float defaultGravityScale = 3f;

    private bool _ug;
    public bool useGravity
    {
        get { return _ug; }
        private set
        {
            if (!canChangeUseGravity) return;
            _ug = value;
            rigidbody.gravityScale = value ? defaultGravityScale : 0f;
        }
    }


    public bool limitSpeed { get; private set; } = true;
    public bool limitSpeedOnSlopes { get; private set; } = true;
    public bool canMove { get; private set; } = true;

    private bool subscribedOnEvents = false;

    private void Awake()
    {
        character = GetComponent<Character>();
        curMovementCurrency = maxMovementCurrency;
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
        //if (!IsOwner) enabled = false;
    }



    private void Update()
    {
        bool isGrounded = IsGrounded(out groundHit);

        if (isGrounded != this.isGrounded)
        {
            if (isGrounded)
            {
                onGrounded?.Invoke();
                onGroundedStatic?.Invoke(character);
            }
            else
            {
                onUngrounded?.Invoke();
                onUngroundedStatic?.Invoke(character);
            }

            linearDamping = isGrounded ? groundDrag : 0f;
            this.isGrounded = isGrounded;
        }

        onSlope = OnSlope(groundHit);
        useGravity = !onSlope;

        MovementCurrencyManagement();
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

    private void MovementCurrencyManagement()
    {
        if (curMovementCurrency < maxMovementCurrency)
        {
            curMovementCurrency += Time.deltaTime * movementCurrencyPerSecond;
        }
    }



    private bool GetOnAWallIfCanTo(Collider2D wallCantToGetOn = null)
    {
        if (!wallsImCollidingWith.Any()) return false;
        Collider2D wallToGetOn = wallsImCollidingWith.Find((wall) => wall != wallCantToGetOn);
        if (wallToGetOn == null) return false;

        return GetOnTheWall(wallToGetOn);
    }

    private bool GetOnTheWall(Collider2D wall)
    {
        if (movementState != MovementState.Default) return false;

        linearDamping = onWallDrag;
        curWall = wall;
        onWall = true;

        onGotOnTheWall?.Invoke(wall);
        return true;
    }

    private void GetOffTheWall()
    {
        if (!onWall) return;
        if (GetOnAWallIfCanTo(curWall)) return;

        curWall = null;

        linearDamping = isGrounded ? groundDrag : 0f;
        onWall = false;

        onGotOffTheWall?.Invoke();
    }


    private void OnGrounded()
    {
        GetOffTheWall();
    }

    private void OnStartedDash()
    {
        canChangeLinearDamping = true;
        canChangeUseGravity = true;

        linearDamping = 0f;
        useGravity = false;
        limitSpeed = false;
        canMove = false;

        movementState = MovementState.Dashing;

        canChangeLinearDamping = false;
        canChangeUseGravity = false;

        curMovementCurrency -= dash.maxMovementCurrencyCost;
    }

    private void OnEndedDash()
    {
        canChangeLinearDamping = true;
        canChangeUseGravity = true;

        linearDamping = isGrounded ? groundDrag : 0f;
        useGravity = true;
        limitSpeed = true;
        canMove = true;

        movementState = MovementState.Default;

        GetOnAWallIfCanTo();
    }

    private void OnJump()
    {
        canChangeLinearDamping = true;

        linearDamping = 0f;

        movementState = MovementState.Jumping;

        canChangeLinearDamping = false;

        curMovementCurrency -= jump.maxMovementCurrencyCost;
    }

    private void OnJumpEnd()
    {
        if (movementState == MovementState.Dashing)
            return;

        canChangeLinearDamping = true;
        linearDamping = isGrounded ? groundDrag : 0f;

        movementState = MovementState.Default;

        GetOnAWallIfCanTo();
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
        if (collision.collider.gameObject.tag == "Wall")
        {
            if (collision.contacts[0].normal.y <= 0.1f)
            {
                wallsImCollidingWith.Add(collision.collider);

                if (!onWall && !isGrounded)
                {
                    GetOnTheWall(collision.collider);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.tag == "Wall")
        {
            if (wallsImCollidingWith.Contains(collision.collider))
            {
                wallsImCollidingWith.Remove(collision.collider);

                if (onWall && curWall == collision.collider)
                {
                    GetOffTheWall();
                }
            }
        }
    }



    private void SubscribeOnEvents()
    {
        if (!subscribedOnEvents)
        {
            onGrounded += OnGrounded;

            subscribedOnEvents = true;
        }

        if (dash != null && !dash.subscribedOnEvents)
        {
            dash.onDash += OnStartedDash;
            dash.onDashEnd += OnEndedDash;

            dash.subscribedOnEvents = true;
        }

        if (jump != null && !jump.subscribedOnEvents)
        {
            jump.onJump += OnJump;
            jump.onWallJump += OnJump;
            jump.onJumpEnd += OnJumpEnd;

            jump.subscribedOnEvents = true;
        }
    }

    private void UnsubscribeFromEvents()
    {
        onGrounded -= OnGrounded;

        subscribedOnEvents = false;

        if (dash != null)
        {
            dash.onDash -= OnStartedDash;
            dash.onDashEnd -= OnEndedDash;

            dash.subscribedOnEvents = false;
        }

        if (jump != null)
        {
            jump.onJump -= OnJump;
            jump.onWallJump -= OnJump;
            jump.onJumpEnd -= OnJumpEnd;

            jump.subscribedOnEvents = false;
        }
    }



    public void OnMove(Vector2 moveDir)
    {
        if (!IsOwner) return;
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


        if (NetworkObject.IsSpawned)
        {
            if (Mathf.Abs(moveDir) >= 0.1f)
                CallOnStartedMovingEventRpc();
            else
                CallOnStopedMovingEventRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void CallOnStartedMovingEventRpc()
    {
        onStartedMoving?.Invoke(character);
    }

    [Rpc(SendTo.Everyone)]
    private void CallOnStopedMovingEventRpc()
    {
        onStopedMoving?.Invoke(character);
    }

    public void OnJump(bool start)
    {
        if (start)
        {
            if (jump.CanJump())
                jump.JumpIfCanTo();
            else
                dash.DashIfCanTo(lastMoveInput);
        }
        else
        {
            jump.CancelTheJump();
        }
    }

    public enum MovementState
    {
        Default,
        Dashing,
        Jumping
    }
}
