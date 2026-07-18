using System;
using Unity.Netcode;
using UnityEngine;

public class MapsManager : NetworkBehaviour
{
    public static event Action onMapSpawned;

    private void OnGameStarted()
    {
        SpawnMap();
    }

    private void SpawnMap()
    {
        if (!IsServer) return;
        GameObject map = Instantiate(MusicListScriptableObject.Instance.GetMaps(MusicManager.Instance.curMusic).GetRandomMap());
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
