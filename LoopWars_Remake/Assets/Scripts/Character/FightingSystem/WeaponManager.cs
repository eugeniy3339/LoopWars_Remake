using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Weapon curWeapon;

    private Vector2 target;

    public void StartAttacking()
    {
        curWeapon.onAttackCooldown += OnAttackCooldown;
    }

    public void StopAttacking()
    {
        curWeapon.onAttackCooldown -= OnAttackCooldown;
    }

    private void Attack()
    {
        if (curWeapon == null) { return; }

        Vector2 direction = (target - new Vector2(transform.position.x, transform.position.y)).normalized;
        if (!curWeapon.AttackIfCanTo(direction, out bool becauseOfBullets) && becauseOfBullets)
        {
            //Play out of bullets sound
            StopAttacking();
        }
    }

    private void OnAttackCooldown()
    {
        Attack();
    }
}
