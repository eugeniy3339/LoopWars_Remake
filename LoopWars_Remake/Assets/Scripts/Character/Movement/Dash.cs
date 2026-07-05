using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Dash : MovementComponent
{
    [SerializeField] private float dashCooldown = 3f;
    private float curDashCooldown;

    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashTime = 2f;

    private Coroutine curDashCoroutine;

    public event Action onDash;
    public static event Action<Character> onDashStatic;
    public event Action onDashEnd;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) enabled = false;
    }

    private void Update()
    {
        DashCooldown();
    }

    private void DashCooldown()
    {
        if(curDashCooldown > 0f)
        {
            curDashCooldown -= Time.deltaTime;
            /*if(curDashCooldown <= 0f)
            {

            }*/
        }
    }

    public void DashIfCanTo(Vector2 direction)
    {
        if (!CanDash()) return;

        if (curDashCoroutine != null) StopCoroutine(curDashCoroutine);
        curDashCoroutine = StartCoroutine(DashCoro(direction, dashSpeed, dashTime));
    }

    private bool CanDash()
    {
        if (!IsOwner) return false;
        return (movementManager.movementState == MovementManager.MovementState.Default || movementManager.movementState == MovementManager.MovementState.Jumping) && curDashCooldown <= 0f;
    }

    private IEnumerator DashCoro(Vector2 dashDirection, float dashSpeed, float dashTime)
    {
        if (!IsOwner) yield break;
        CallOnDashEventRpc();

        float curDashTime = 0f;
        Vector2 velocity = dashDirection.normalized * dashSpeed;

        rigidbody.linearVelocity = velocity;


        while (curDashTime < dashTime)
        {
            rigidbody.linearVelocity = velocity;

            curDashTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0f);

        curDashCoroutine = null;
        curDashCooldown = dashCooldown;
        onDashEnd?.Invoke();
        yield break;
    }

    [Rpc(SendTo.Everyone)]
    private void CallOnDashEventRpc()
    {
        onDash?.Invoke();
        onDashStatic?.Invoke(character);
    }
}
