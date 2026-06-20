using UnityEngine;

public class DefaultWeapon : Weapon
{
    protected override void Attack(Vector2 direction)
    {
        SpawnProjectile(bulletScriptableObject, transform.position, direction);

        ammo -= 1;
        curAttackCooldown = attackCooldown;
    }
}