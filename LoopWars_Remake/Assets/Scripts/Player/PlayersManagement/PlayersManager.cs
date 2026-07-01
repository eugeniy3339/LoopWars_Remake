using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using LoopWars.Players;
using LoopWars.GameMode;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; protected set; }
    private PlayerInputManager playerInputManager;

    [SerializeField] private GameObject playerPrefab;
    [HideInInspector] public List<Character> alivePlayers = new List<Character>();

    public static event Action<Character, List<Character>> onPlayerDied;

    private int loadedPlayersCount;
    private bool spawnedPlayers;

    private void Awake()
    {
        Instance = this;
        playerInputManager = FindAnyObjectByType<PlayerInputManager>();

        playerInputManager.playerPrefab = playerPrefab;

        if (GameMode.multiplayerMode != MultiplayerMode.LocalMultiplayer)
            Destroy(playerInputManager);
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName != "GameScene") return;

        print("Load complete for " + clientId + " player");
        loadedPlayersCount++;

        if (loadedPlayersCount >= NetworkManager.Singleton.ConnectedClients.Count)
            SpawnPlayers();
    }



    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
    }



    private void SpawnPlayers()
    {
        if (spawnedPlayers) return;
        spawnedPlayers = true;

        foreach (var player in PlayersContainer.players)
        {
            if (player.multiplayerMode != GameMode.multiplayerMode) continue;



            Character character;
            if (GameMode.multiplayerMode == MultiplayerMode.LocalMultiplayer)
            {
                PlayerInput playerInput = playerInputManager.JoinPlayer(-1, -1, player.controllScheme, player.devices.ToArray());
                character = playerInput.GetComponent<Character>();
                character.NetworkObject.Spawn(true);
            }
            else
            {
                character = Instantiate(playerPrefab).GetComponent<Character>();
                character.NetworkObject.SpawnWithOwnership(player.playerId, true);
            }

            OnPlayerCreated(character, GameMode.multiplayerMode);
        }
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
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            HealthSystem.onCharacterDied -= OnPlayerDied;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }
}
