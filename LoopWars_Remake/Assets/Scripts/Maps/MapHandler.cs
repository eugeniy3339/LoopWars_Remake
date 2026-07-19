using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public static MapHandler Instance
    {
        get
        {
            foreach(var MapHandler in FindObjectsOfType<MapHandler>())
            {
                if (!MapHandler.despawned)
                    return MapHandler;
            }

            return null;
        }
    }

    private NetworkObject _nO;
    public NetworkObject networkObject { get { if (_nO == null) { _nO = GetComponent<NetworkObject>(); } return _nO; } }

    private SpawnPointsContainer spawnPointsContainer;
    public Transform[] spawnPoints { get; private set; }

    [SerializeField] private Transform _minWeaponsSpawnPos, _maxWeaponsSpawnPos;
    public Transform minWeaponsSpawnPos { get { return _minWeaponsSpawnPos; } }
    public Transform maxWeaponsSpawnPos { get { return _maxWeaponsSpawnPos; } }

    [SerializeField] private WeaponsListScriptableObject _weaponsList;
    public WeaponsListScriptableObject weaponsList { get { if (_weaponsList == null) { _weaponsList = WeaponsListScriptableObject.Instance; } return _weaponsList; } }

    [SerializeField] private Transform enviromentTransform;
    const float startXPos = 27f;
    [SerializeField] private float moveSpeed = 20f;
    private Coroutine curMoveCoroutine;

    public event Action<MapHandler> onMoved;

    [HideInInspector] public bool despawned = false;

    private void Awake()
    {
        spawnPointsContainer = GetComponentInChildren<SpawnPointsContainer>();
        spawnPoints = spawnPointsContainer.spawnPoints;
        enviromentTransform.position = new Vector2(startXPos, 0f);
        Move(true);
    }

    public void Move(bool onTheScreen)
    {
        if (curMoveCoroutine != null) StopCoroutine(curMoveCoroutine);
        Vector2 endPosition = onTheScreen ? Vector2.zero : new Vector2(startXPos, 0f);
        curMoveCoroutine = StartCoroutine(MoveCoro(enviromentTransform.position, endPosition, moveSpeed));
    }

    private IEnumerator MoveCoro(Vector2 startPosition, Vector2 endPosition, float speed)
    {
        float moveTime = Vector2.Distance(startPosition, endPosition) / speed;
        float curTime = 0f;

        do
        {
            curTime += Time.deltaTime;
            enviromentTransform.position = Vector2.Lerp(startPosition, endPosition, curTime/moveTime);
            yield return new WaitForEndOfFrame();
        }
        while (curTime < moveTime);

        onMoved?.Invoke(this);
    }

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
    }
}
