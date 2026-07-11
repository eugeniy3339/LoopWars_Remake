using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapHandler : MonoBehaviour
{
    private static MapHandler _i;
    public static MapHandler Instance { get { if (_i == null) { _i = FindObjectOfType<MapHandler>(); } return _i; } private set { _i = value; } }

    private SpawnPointsContainer spawnPointsContainer;
    public Transform[] spawnPoints { get; private set; }

    [SerializeField] private Transform _minWeaponsSpawnPos, _maxWeaponsSpawnPos;
    public Transform minWeaponsSpawnPos { get { return _minWeaponsSpawnPos; } }
    public Transform maxWeaponsSpawnPos { get { return _maxWeaponsSpawnPos; } }

    private void Awake()
    {
        Instance = this;

        spawnPointsContainer = GetComponentInChildren<SpawnPointsContainer>();
        spawnPoints = spawnPointsContainer.spawnPoints;
    }

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
