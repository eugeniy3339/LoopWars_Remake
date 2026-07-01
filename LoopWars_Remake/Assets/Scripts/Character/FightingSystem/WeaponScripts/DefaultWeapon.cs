using UnityEngine;

public class DefaultWeapon : Weapon
{
    protected override void Attack()
    {
        if (!IsOwner) return;

        LaunchProjectileRpc(transform.position, transform.right);
        //attacker.rigidbody.AddForce(-transform.right * shotBackForce, ForceMode2D.Impulse);

        ammo -= 1;
        curAttackCooldown = attackCooldown;
    }
}