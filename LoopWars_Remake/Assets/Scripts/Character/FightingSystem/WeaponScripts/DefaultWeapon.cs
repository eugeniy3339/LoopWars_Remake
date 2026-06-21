using UnityEngine;

public class DefaultWeapon : Weapon
{
    protected override void Attack()
    {
        SpawnProjectile(bulletScriptableObject, transform.position);
        attacker.rigidbody.AddForce(-transform.right * shotBackForce, ForceMode2D.Impulse);

        ammo -= 1;
        curAttackCooldown = attackCooldown;
    }
}