using UnityEngine;

public class GranadeWeapon : Weapon
{
    protected override void Attack()
    {
        print("Attack");
        attacker.weaponManager.ThrowWeapon();
    }
}
