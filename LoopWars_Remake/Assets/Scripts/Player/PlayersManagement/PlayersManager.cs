using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using LoopWars.Players;
using LoopWars.GameMode;
using Unity.Netcode;
using System;

public class PlayersManager : NetworkBehaviour
{
    private static PlayersManager _i;
    public static PlayersManager Instance { get { if (_i == null) { _i = FindAnyObjectByType<PlayersManager>(); } return _i; } protected set { _i = value; } }
    private PlayerInputManager playerInputManager;

    [SerializeField] private GameObject playerPrefab;
    [HideInInspector] public List<Character> alivePlayers = new List<Character>();

    public static event Action onAllPlayersSpawned;
    public static event Action<Character, List<Character>> onPlayerDied;

    private void Awake()
    {
        try
        {
            Instance = this;
            playerInputManager = FindAnyObjectByType<PlayerInputManager>();

            playerInputManager.playerPrefab = playerPrefab;

            if (GameMode.multiplayerMode != MultiplayerMode.LocalMultiplayer)
                Destroy(playerInputManager);
        }
        catch { }
    }

    private void OnMapSpawned()
    {
        SpawnPlayers();
    }

    private void DespawnPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        print("DespawnPlayers");

        foreach (var character in FindObjectsOfType<Character>())
        {
            if(character.IsSpawned)
                character.NetworkObject.Despawn(true);
        }
    }

    private void SpawnPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        print("SpawnPlayers");
        DespawnPlayers();

        foreach (var player in PlayersContainer.players)
        {
            if (player.multiplayerMode != GameMode.multiplayerMode) continue;



            Character character;
            Transform spawnPoint = MapHandler.Instance.GetRandomSpawnPoint();
            if (GameMode.multiplayerMode == MultiplayerMode.LocalMultiplayer)
            {
                PlayerInput playerInput = playerInputManager.JoinPlayer(-1, -1, player.controllScheme, player.devices.ToArray());
                character = playerInput.GetComponent<Character>();
                character.transform.position = spawnPoint.position;
                character.NetworkObject.Spawn(true);
            }
            else
            {
                character = Instantiate(playerPrefab).GetComponent<Character>();
                character.transform.position = spawnPoint.position;
                character.NetworkObject.SpawnWithOwnership(player.playerId, true);
            }

            OnPlayerCreated(character, GameMode.multiplayerMode);
        }

        onAllPlayersSpawned?.Invoke();
    }



    private void OnPlayerCreated(Character character, MultiplayerMode multiplayerMode)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (alivePlayers.Contains(character)) return;

        alivePlayers.Add(character);
        Player player = multiplayerMode == MultiplayerMode.LocalMultiplayer ? PlayersContainer.GetPlayerByDevice(character.playerInput.devices[0]) : PlayersContainer.GetPlayerById(character.OwnerClientId);
        player.character = character;
    }



    private void OnPlayerDied(Character character) // On player object is destroyed
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (!alivePlayers.Contains(character)) return;

        alivePlayers.Remove(character);
        onPlayerDied?.Invoke(character, alivePlayers);
    }



    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            HealthSystem.onCharacterDied += OnPlayerDied;
            MapsManager.onMapSpawned += OnMapSpawned;
        }
    }

    private void OnDisable()
    {
        HealthSystem.onCharacterDied -= OnPlayerDied;
        MapsManager.onMapSpawned -= OnMapSpawned;
    }
}
