using System;
using Unity.Netcode;
using UnityEngine;

public class MapsManager : NetworkBehaviour
{
    public static event Action onMapSpawned;
    public static event Action onMapDespawned;

    private bool toSpawnMap = false;

    private void OnGameStarted()
    {
        SpawnMap();
    }

    private void DespawnMap()
    {
        if (!IsServer) return;
        if (MapHandler.Instance == null) return;

        DespawnMapRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void DespawnMapRpc()
    {
        if (MapHandler.Instance == null) return;

        MapHandler.Instance.onMoved -= OnMapSpawned;
        MapHandler.Instance.onMoved -= OnMapDespawned;
        MapHandler.Instance.onMoved += OnMapDespawned;

        MapHandler.Instance.Move(false);
    }

    private void OnMapDespawned(MapHandler mapHandler)
    {
        mapHandler.despawned = true;
        mapHandler.onMoved -= OnMapDespawned;

        if (!IsServer) return;

        if (mapHandler.networkObject.IsSpawned)
        {
            mapHandler.networkObject.Despawn(true);
        }
        onMapDespawned?.Invoke();

        if (toSpawnMap) 
            SpawnMap();
    }

    private void SpawnMap()
    {
        if (!IsServer) return;
        if (MapHandler.Instance != null) { DespawnMap(); toSpawnMap = true; return; }

        toSpawnMap = false;

        GameObject map = Instantiate(MusicManager.curMusic != null ? MusicMapsPairsListScriptableObject.Instance.GetMaps(MusicManager.curMusic).GetRandomMap() : MapsListScriptableObject.Instance.GetRandomMap());
        MapHandler mapHandler = map.GetComponent<MapHandler>();
        mapHandler.networkObject.Spawn(true);
        mapHandler.onMoved += OnMapSpawned;
    }

    private void OnMapSpawned(MapHandler mapHandler)
    {
        mapHandler.onMoved -= OnMapSpawned;
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
