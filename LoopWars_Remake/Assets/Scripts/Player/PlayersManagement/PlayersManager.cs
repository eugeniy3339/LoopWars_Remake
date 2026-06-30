using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using LoopWars.Players;
using LoopWars.GameMode;
using Unity.Netcode;
using System;
using UnityEngine.TextCore.Text;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; protected set; }

    [SerializeField] private GameObject playerPrefab;
    public List<Character> alivePlayers = new List<Character>();

    public static event Action<Character, List<Character>> onPlayerDied;

    private void Awake()
    {
        Instance = this;

        if (NetworkManager.Singleton.IsServer)
            NetworkObject.Spawn(true);
    }


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PlayerInputManager.instance.playerPrefab = playerPrefab;



        foreach (var player in PlayersContainer.players)
        {
            if (player.multiplayerMode != GameMode.multiplayerMode) continue;



            Character character;
            if (GameMode.multiplayerMode == MultiplayerMode.LocalMultiplayer)
            {
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, player.controllScheme, player.devices.ToArray());
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
            HealthSystem.onCharacterDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
            HealthSystem.onCharacterDied -= OnPlayerDied;
    }
}
