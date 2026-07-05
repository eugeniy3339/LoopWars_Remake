using UnityEngine;

public class DefaultWeapon : Weapon
{
    protected override void Attack()
    {
        if (!IsOwner) return;

        ShootRpc(shootPosition.position, shootPosition.right);

        ammo -= 1;
        curAttackCooldown = attackCooldown;
    }
}