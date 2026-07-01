using System;
using Unity.Netcode;
using UnityEngine;

public class HealthSystem : NetworkBehaviour, IDamagable
{
    private Character character;

    [SerializeField] private float maxHealth;
    private NetworkVariable<float> health = new NetworkVariable<float>();

    private bool sentDeathEvent = false;
    public static event Action<Character> onCharacterDied;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        health.OnValueChanged += OnHealthValueChanged;

        if (IsServer)
            health.Value = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        health.OnValueChanged -= OnHealthValueChanged;
    }

    public bool Damage(Transform damager, Transform damageObject, float damage)
    {
        if (!IsServer) return false;
        health.Value -= damage;

        return true;
    }

    protected virtual void OnHealthValueChanged(float prevValue, float newValue)
    {
        if (IsServer)
        {
            if (newValue <= 0f)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (!IsServer) return;
        if (NetworkObject == null || !NetworkObject.IsSpawned) return;

        SendDeathEvent();

        NetworkObject.Despawn(true);
    }

    public override void OnDestroy()
    {
        SendDeathEvent();
    }

    private void SendDeathEvent()
    {
        if(!sentDeathEvent)
        {
            onCharacterDied?.Invoke(character);
            sentDeathEvent = true;
        }
    }
}