using UnityEngine;

public class WeaponToPickUp : Trigger
{
    [SerializeField] private WeaponScriptableObject weaponToPickUp;

    protected override void OnTrigger(Character character)
    {
        if(character.weaponManager.SetCurWeapon(weaponToPickUp))
        {
            base.OnTrigger(character);
            Destroy(gameObject);
        }
    }
}
