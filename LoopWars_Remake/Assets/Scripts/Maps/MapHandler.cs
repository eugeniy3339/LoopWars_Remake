using UnityEngine;
using System.Collections.Generic;

public class MapHandler : MonoBehaviour
{
    private static MapHandler _i;
    public static MapHandler Instance { get { if (_i == null) { _i = FindObjectOfType<MapHandler>(); } return _i; } private set { _i = value; } }

    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    public List<Transform> spawnPoints { get { return _spawnPoints; } }

    [SerializeField] private Transform _minWeaponsSpawnPos, _maxWeaponsSpawnPos;
    public Transform minWeaponsSpawnPos { get { return _minWeaponsSpawnPos; } }
    public Transform maxWeaponsSpawnPos { get { return _maxWeaponsSpawnPos; } }

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
