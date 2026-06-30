using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
public class Trigger : NetworkBehaviour
{
    public event Action<Character, Trigger> onTriggerEnter;
    protected virtual bool areThereTriggerConditions { get; set; } = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsThereCharacter(collision.gameObject, out Character character, out bool isLocalClient))
        {
            if (!isLocalClient) return;

            if (!areThereTriggerConditions)
                OnTrigger(character);
            else
                TryToTriggerRpc(NetworkManager.LocalClientId, character.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    protected virtual void TryToTriggerRpc(ulong clientId, ulong characterNetworkObjectId)
    {
        if(CanTrigger())
            TriggerRpc(characterNetworkObjectId, new RpcSendParams { Target = NetworkManager.RpcTarget.Single(clientId, RpcTargetUse.Temp) });
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TriggerRpc(ulong characterNetworkObjectId, RpcParams sendParams)
    {
        Character character = Character.FindCharacter(characterNetworkObjectId);
        if(character != null)
        {
            OnTrigger(character);
        }
    }

    protected virtual bool CanTrigger()
    {
        return NetworkObject != null && NetworkObject.IsSpawned;
    }

    private bool IsThereCharacter(GameObject gameObject, out Character character, out bool isLocalPlayer)
    {
        character = gameObject.GetComponentInChildren<Character>();

        if(character != null)
        {
            isLocalPlayer = character.IsOwner;
            return true;
        }
        else
        {
            isLocalPlayer = false;
            return false;
        }
    }

    protected virtual void OnTrigger(Character character)
    {
        onTriggerEnter?.Invoke(character, this);
    }

    protected void DespawnTrigger()
    {
        if (!IsServer) return;
        if (NetworkObject == null || !NetworkObject.IsSpawned) return;

        NetworkObject.Despawn();
    }
}
