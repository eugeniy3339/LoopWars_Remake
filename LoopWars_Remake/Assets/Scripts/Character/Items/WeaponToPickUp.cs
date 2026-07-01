using Unity.Netcode;
using UnityEngine;

public class WeaponToPickUp : Trigger
{
    [SerializeField] private WeaponScriptableObject weaponToPickUp;

    protected override void Awake()
    {
        base.Awake();
        areThereTriggerConditions = true;
    }

    protected override void OnTrigger(Character character)
    {
        if(character.weaponManager.SetCurWeapon(weaponToPickUp))
        {
            base.OnTrigger(character);
            DespawnTrigger();
        }
    }

    [Rpc(SendTo.Server)]
    protected override void TriggerServerRpc(ulong clientId, ulong characterNetworkObjectId)
    {
        if (!CanTrigger()) return;
        OnTrigger(Character.FindCharacter(characterNetworkObjectId));
    }
}
