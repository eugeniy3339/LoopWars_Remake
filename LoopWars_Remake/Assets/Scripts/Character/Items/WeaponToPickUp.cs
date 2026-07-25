using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class WeaponToPickUp : Trigger
{
    [SerializeField] private WeaponScriptableObject weaponToPickUp;
    protected override bool areThereTriggerConditions { get { return true; } }



    protected override void OnTriggerEnt(Character character)
    {
        TryToPickUpWeapon(character);
        base.OnTriggerEnt(character);
    }

    private void TryToPickUpWeapon(Character character)
    {
        if (character.weaponManager.SetCurWeapon(weaponToPickUp))
        {
            DespawnTrigger();
        }
    }

    [Rpc(SendTo.Server)]
    protected override void TriggerEnterServerRpc(ulong clientId, ulong characterNetworkObjectId)
    {
        if (!CanTrigger()) return;
        OnTriggerEnt(Character.FindCharacter(characterNetworkObjectId));
    }

    protected void OnWeaponDestroyed(Character character, WeaponScriptableObject weaponScriptableObject)
    {
        print(charactersInTrigger.Count);
        print(charactersInTrigger.Contains(character));
        print(character.weaponManager.curWeapon == null);
        if (charactersInTrigger.Contains(character) && character.weaponManager.curWeapon == null)
            TryToPickUpWeapon(character);

    }

    private void OnEnable()
    {
        WeaponManager.onWeaponDestroyed += OnWeaponDestroyed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        WeaponManager.onWeaponDestroyed -= OnWeaponDestroyed;
    }
}
