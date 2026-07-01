using System.Collections.Generic;
using LoopWars.GameMode;
using LoopWars.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LocalMultiplayerLobby : MonoBehaviour
{
    public static LocalMultiplayerLobby Instance { get; protected set; }

    private PlayerInputManager playerInputManager;

    private List<Player> connectedPlayers = new List<Player>();

    private void Awake()
    {
        Instance = this;

        playerInputManager = FindAnyObjectByType<PlayerInputManager>();
    }

    private void Start()
    {
        SpawnConnectedPlayers();
        enabled = false;
    }

    private void SpawnConnectedPlayers()
    {
        for (int i = PlayersContainer.players.Count - 1; i >= 0; i--)
        {
            Player player = PlayersContainer.players[i];
            if (player.multiplayerMode != MultiplayerMode.LocalMultiplayer) continue;

            if (player.devices[0] == null)
            {
                KickPlayer(player);
                continue;
            }

            PlayerInput playerInput = playerInputManager.JoinPlayer(-1, -1, player.controllScheme, player.devices.ToArray());
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput) //On player object is spawned
    {
        if (!enabled) return;

        Player player = PlayersContainer.GetPlayerByDevice(playerInput.devices[0]);
        if (player != null) return;

        player = new Player(playerInput.devices, playerInput.currentControlScheme);
        player.playerInput = playerInput;
        LobbyManager.JoinPlayer(player);
        connectedPlayers.Add(player);

        playerInput.onDeviceLost += OnDeviceDiscontected;
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        playerInput.onDeviceLost -= OnDeviceDiscontected;
    }

    private void OnPlayerLeave(PlayerInput playerInput)
    {
        KickPlayer(playerInput);
    }

    private void OnPlayerLeave(Player player)
    {
        KickPlayer(player);
    }

    private void OnDeviceDiscontected(PlayerInput playerInput)
    {
        Debug.Log(playerInput + "'s device has been disconected");
        KickPlayer(playerInput);
    }

    private void KickPlayer(Player player)
    {
        LobbyManager.KickPlayer(player);
    }

    private void KickPlayer(PlayerInput playerInput)
    {
        Player player = PlayersContainer.GetPlayerByPlayerInput(playerInput);

        if(player != null)
            KickPlayer(player);
        else
        {
            Destroy(playerInput.gameObject);
        }
    }

    private void OnGameStarted(MultiplayerMode multiplayerMode, string relayCode)
    {
        if(multiplayerMode == MultiplayerMode.LocalMultiplayer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    private void OnEnable()
    {
        LobbyManager.onGameStarted += OnGameStarted;

        if (playerInputManager != null)
            playerInputManager.EnableJoining();
    }

    private void OnDisable()
    {
        LobbyManager.onGameStarted -= OnGameStarted;

        if (playerInputManager != null)
            playerInputManager.DisableJoining();
    }
}
