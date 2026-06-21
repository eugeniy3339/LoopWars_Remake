using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Dash : MonoBehaviour
{
    private Character character;
    private Rigidbody2D rigidbody;

    [SerializeField] private float dashCooldown = 3f;
    private float curDashCooldown;

    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashTime = 2f;

    private bool dashing;
    private Coroutine curDashCoroutine;

    public event Action onDash;
    public event Action onDashEnd;

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
        return !dashing && curDashCooldown <= 0f;
    }

    private IEnumerator DashCoro(Vector2 dashDirection, float dashSpeed, float dashTime)
    {
        onDash?.Invoke();
        dashing = true;

        float curDashTime = 0f;
        Vector2 velocity = dashDirection.normalized * dashSpeed;

        character.movement.enabled = false;
        character.useGravity = false;

        rigidbody.linearDamping = 0f;
        rigidbody.linearVelocity = velocity;


        while (curDashTime < dashTime)
        {
            rigidbody.linearVelocity = velocity;

            curDashTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        character.movement.enabled = true;

        character.useGravity = true;
        rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0f);

        curDashCoroutine = null;
        dashing = false;
        curDashCooldown = dashCooldown;
        onDashEnd?.Invoke();
        yield break;
    }
}
