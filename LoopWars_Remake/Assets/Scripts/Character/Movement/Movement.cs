using LoopWars.GameMode;
using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class Movement : MovementComponent
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float airMultiplier = 0.8f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) enabled = false;
    }

    private void Update()
    {
        SpeedControll();
    }

    private void FixedUpdate()
    {
        Move(movementManager.moveDir);
    }

    public void Move(float dir)
    {
        if (!IsOwner) return;
        if (!movementManager.canMove) return;
        if (Mathf.Abs(dir) <= 0.1f) return;
        Vector2 moveDir = new Vector2(dir, 0f).normalized;

        if (movementManager.onSlope)
            moveDir = GetSlopeMoveDirection(moveDir, movementManager.groundHit);

        rigidbody.AddForce(moveDir * speed * 80f * (movementManager.isGrounded ? 1f : airMultiplier), ForceMode2D.Force);
    }

    private Vector3 GetSlopeMoveDirection(Vector2 moveDirection, RaycastHit2D raycastHit)
    {
        return Vector3.ProjectOnPlane(moveDirection, raycastHit.normal).normalized;
    }

    private void SpeedControll()
    {
        if (!IsOwner) return;
        if (!movementManager.limitSpeed) return;

        if (movementManager.onSlope)
        {
            LimitOnSlopeSpeed();
        }
        else
        {
            LimitSpeed();
        }    
    }

    private void LimitOnSlopeSpeed()
    {
        if (!movementManager.limitSpeedOnSlopes) return;

        if(rigidbody.linearVelocity.magnitude > speed)
        {
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * speed;
        }
    }

    private void LimitSpeed()
    {
        float curFlatSpeed = Mathf.Abs(rigidbody.linearVelocityX);
        if(curFlatSpeed > speed)
        {
            rigidbody.linearVelocityX = BetterMath.NormalizedFloat(rigidbody.linearVelocityX) * speed;
        }
    }

/*#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (movementManager.footPosition == null)
            return;

        Gizmos.color = Color.red;
        Vector2 raycastOrigin = movementManager.footPosition.position + Vector3.up * groundCheckRadius;
        Gizmos.DrawWireSphere(raycastOrigin + Vector2.down * groundCheckDistance, groundCheckRadius);
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * groundCheckDistance);
    }

#endif*/
}
