using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trigger : NetworkBehaviour
{
    public event Action<Character, Trigger> onTriggerEnter;
    public event Action<Character, Trigger> onTriggerExit;
    protected virtual bool areThereTriggerConditions { get { return false; } }

    protected List<Character> charactersInTrigger = new List<Character>();

    protected virtual void Awake()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsThereCharacter(collision.gameObject, out Character character, out bool isLocalClient))
        {
            if (!isLocalClient) return;

            if (!areThereTriggerConditions)
                OnTriggerEnt(character);
            else
                TriggerEnterServerRpc(NetworkManager.LocalClientId, character.NetworkObjectId);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsThereCharacter(collision.gameObject, out Character character, out bool isLocalClient))
        {
            if (!isLocalClient) return;

            if (!areThereTriggerConditions)
                OnTriggerExt(character);
            else if(NetworkObject != null && NetworkObject.IsSpawned)
                TriggerExitServerRpc(character.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    protected virtual void TriggerEnterServerRpc(ulong clientId, ulong characterNetworkObjectId)
    {
        if (CanTrigger())
            TriggerEnterClientRpc(characterNetworkObjectId, new RpcSendParams { Target = NetworkManager.RpcTarget.Single(clientId, RpcTargetUse.Temp) });
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TriggerEnterClientRpc(ulong characterNetworkObjectId, RpcParams sendParams)
    {
        Character character = Character.FindCharacter(characterNetworkObjectId);
        if (character != null)
        {
            OnTriggerEnt(character);
        }
    }

    [Rpc(SendTo.Server)]
    protected virtual void TriggerExitServerRpc(ulong characterNetworkObjectId) { OnTriggerExt(Character.FindCharacter(characterNetworkObjectId)); }

    protected virtual bool CanTrigger()
    {
        return NetworkObject != null && NetworkObject.IsSpawned;
    }

    private bool IsThereCharacter(GameObject gameObject, out Character character, out bool isLocalPlayer)
    {
        character = gameObject.GetComponentInChildren<Character>();

        if (character != null)
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

    protected virtual void OnTriggerEnt(Character character)
    {
        if (character == null) return;
        if (charactersInTrigger.Contains(character)) return;
        charactersInTrigger.Add(character);
        onTriggerEnter?.Invoke(character, this);
    }

    protected virtual void OnTriggerExt(Character character)
    {
        if (character == null) return;
        if (!charactersInTrigger.Contains(character)) return;
        charactersInTrigger.Remove(character);
        onTriggerExit?.Invoke(character, this);
    }

    protected void DespawnTrigger()
    {
        if (!IsServer) return;
        if (NetworkObject == null || !NetworkObject.IsSpawned) return;

        NetworkObject.Despawn();
    }

    protected virtual void OnDisable()
    {
        for(int i = charactersInTrigger.Count - 1; i >= 0; i--)
        {
            OnTriggerExt(charactersInTrigger[i]);
        }
    }
}
