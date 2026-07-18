using System;
using Unity.Netcode;
using UnityEngine;

public class MapsManager : NetworkBehaviour
{
    public static event Action onMapSpawned;
    public static event Action onMapDespawned;

    private void OnGameStarted()
    {
        SpawnMap();
    }

    private void DespawnMap()
    {
        if (!IsServer) return;
        if (MapHandler.Instance == null) return;

        NetworkObject mapnetworkObject = MapHandler.Instance.GetComponent<NetworkObject>();
        if (mapnetworkObject.IsSpawned)
        {
            mapnetworkObject.Despawn(true);
            onMapDespawned?.Invoke();
        }
    }

    private void SpawnMap()
    {
        if (!IsServer) return;
        DespawnMap();

        GameObject map = Instantiate(MusicManager.Instance?.curMusic != null ? MusicListScriptableObject.Instance.GetMaps(MusicManager.Instance.curMusic).GetRandomMap() : MapsListScriptableObject.Instance.GetRandomMap());
        map.GetComponent<NetworkObject>().Spawn(true);
        onMapSpawned?.Invoke();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsServer)
            GameManager.onGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
            GameManager.onGameStarted -= OnGameStarted;
    }
}
