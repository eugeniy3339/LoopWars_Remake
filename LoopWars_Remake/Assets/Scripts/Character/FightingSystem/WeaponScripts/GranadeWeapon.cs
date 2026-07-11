using UnityEngine;

public class GranadeWeapon : Weapon
{
    protected override void Attack()
    {
        attacker.weaponManager.ThrowWeapon();
    }
}
