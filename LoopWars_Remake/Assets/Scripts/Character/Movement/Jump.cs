using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.LightAnchor;

[RequireComponent(typeof(Character), typeof(Movement))]
public class Jump : MovementComponent
{
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float minJumpTime = 0.2f;
    private float curMinJumpTime = 0f;
    [SerializeField] private float jumpCancelationForceMultiplayer = 0.5f;

    [SerializeField, Tooltip("Wall Jump angle to the right side")] private float wallJumpAngle = 30f;
    [SerializeField] private float wallJumpForce = 8f;
    private Vector2 wallJumpDirection;

    public event Action onJump;
    public event Action onWallJump;
    public event Action onJumpEnd;

    private bool jumping = false;
    private bool toCancelTheJump = false;

    protected override void Awake()
    {
        base.Awake();
        wallJumpDirection = GetWallJumpDirection(wallJumpAngle);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) enabled = false;
    }

    private void Update()
    {
        JumpCooldown();

        if(jumping)
        {
            if(rigidbody.linearVelocityY <= 0f)
            {
                EndTheJump();
            }
        }
    }

    private void JumpCooldown()
    {
        if(curMinJumpTime > 0f)
        {
            curMinJumpTime -= Time.deltaTime;
            if(curMinJumpTime <= 0f)
            {
                OnMinJumpTime();
            }
        }
    }

    private void OnMinJumpTime()
    {
        if(jumping && toCancelTheJump)
            CancelTheJump();
    }

    public void JumpIfCanTo()
    {
        if (!CanJump()) return;

        jumping = true;
        if (movementManager.onWall)
        {
            DoWallJump();
        }
        else
        {
            DoJump();
        }
    }

    private void DoJump()
    {
        curMinJumpTime = minJumpTime;
        rigidbody.linearVelocityY = jumpForce;
        onJump?.Invoke();
    }

    private void DoWallJump()
    {
        Vector2 jumpDirection = new Vector2(wallJumpDirection.x * BetterMath.NormalizedFloat(transform.position.x - movementManager.curWall.transform.position.x), wallJumpDirection.y);
        rigidbody.linearVelocity = jumpDirection * wallJumpForce;

        onWallJump?.Invoke();
    }

    public void CancelTheJump()
    {
        if (!jumping) return;
        if (curMinJumpTime > 0f) { toCancelTheJump = true; return; }
        if (rigidbody.linearVelocityY <= 0f || rigidbody.linearVelocityY > jumpForce) return;

        rigidbody.linearVelocityY = rigidbody.linearVelocityY * jumpCancelationForceMultiplayer;
        EndTheJump();
    }

    private void EndTheJump()
    {
        jumping = false;
        toCancelTheJump = false;
        onJumpEnd();
    }

    public bool CanJump()
    {
        if (!IsOwner) return false;
        return movementManager.movementState == MovementManager.MovementState.Default && !jumping && (movementManager.isGrounded || movementManager.onWall) && curMinJumpTime <= 0f;
    }

    private Vector2 GetWallJumpDirection(float angle)
    {
        float tanA = Mathf.Tan(angle * Mathf.Deg2Rad);
        return new Vector2(1f, tanA).normalized;
    }

    private void OnValidate()
    {
        if (beforeWallJumpAngle != wallJumpAngle)
        {
            wallJumpDirection = GetWallJumpDirection(wallJumpAngle);

#if UNITY_EDITOR
            curArchToDraw = GetJumpArch(wallJumpDirection, wallJumpForce, MovementManager.defaultGravityScale);
            beforeWallJumpAngle = wallJumpAngle;
#endif
        }

#if UNITY_EDITOR
        if (beforeWallJumpForce != wallJumpForce)
        {
            curArchToDraw = GetJumpArch(wallJumpDirection, wallJumpForce, MovementManager.defaultGravityScale);
            beforeWallJumpForce = wallJumpForce;
        }
#endif

    }

#if UNITY_EDITOR
    private float beforeWallJumpAngle;
    private float beforeWallJumpForce;

    private Vector2[] curArchToDraw;

    private Vector2[] GetJumpArch(Vector2 jumpDirection, float jumpForce, float gravityScale)
    {
        int parts = 20;
        float g = 9.81f;

        if(jumpDirection.magnitude != 1f)
            jumpDirection = jumpDirection.normalized;

        Vector2 initialForce = jumpDirection * jumpForce;
        float jumpTime = initialForce.y * 2 / (g * gravityScale);

        Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);

        Vector2[] points = new Vector2[parts + 1];

        for(int part = 0; part <= parts; part++)
        {
            float t = (float)part / parts;
            float curTime = jumpTime * t;
            Vector2 curPos = new Vector2(curTime * initialForce.x, initialForce.y * curTime - g * gravityScale * curTime * curTime / 2f) + playerPos;

            points[part] = curPos;
        }

        return points;
    }

    private void OnDrawGizmosSelected()
    {
        if (curArchToDraw != null)
        {
            DrawGizmos(curArchToDraw);
        }
    }

    private void DrawGizmos(Vector2[] points)
    {
        for(int point = 1; point < points.Length; point++)
        {
            Gizmos.DrawLine(points[point - 1], points[point]);
        }
    }

#endif
}
