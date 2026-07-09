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
    private List<Player> readyPlayers = new List<Player>();

    private List<int> usedColors = new List<int>();

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
        player.name = PlayerSettings.name + (connectedPlayers.Count + 1).ToString();
        int color = Player.GetRandomColor(usedColors.ToArray());
        player.color = Player.playersColors[color];
        usedColors.Add(color);
        player.playerInput = playerInput;

        LobbyManager.JoinPlayer(player);
        connectedPlayers.Add(player);

        playerInput.onDeviceLost += OnDeviceDiscontected;

        LocalLobbyPlayer localLobbyPlayer = playerInput.GetComponent<LocalLobbyPlayer>();
        localLobbyPlayer.player = player;
        localLobbyPlayer.lobbyPlayerUIHandler = MainMenu.Instance.CreateNewLobbyUI(playerInput, MainMenu.Instance.localMultiplayerPlayersUIsContainer, player.name, player.color);

        CheckIfCanStart();
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        playerInput.onDeviceLost -= OnDeviceDiscontected;
    }

    private void OnPlayerLeave(Player player)
    {
        KickPlayer(player);
        CheckIfCanStart();
    }

    private void OnDeviceDiscontected(PlayerInput playerInput)
    {
        KickPlayer(PlayersContainer.GetPlayerByPlayerInput(playerInput));
    }

    private void KickPlayer(Player player)
    {
        OnPlayerUnready(player);
        LobbyManager.KickPlayer(player);
    }

    private void OnPlayerReady(Player player)
    {
        if (readyPlayers.Contains(player)) return;
        readyPlayers.Add(player);
        CheckIfCanStart();
    }

    private void OnPlayerUnready(Player player)
    {
        if (!readyPlayers.Contains(player)) return;
        readyPlayers.Remove(player);
        CheckIfCanStart();
    }

    private void CheckIfCanStart()
    {
        MainMenu.Instance.ActivateLocalMultiplayerStartButton(CanStart());
    }

    public bool CanStart()
    {
        return connectedPlayers.Count > 1 && readyPlayers.Count == connectedPlayers.Count;
    }

    private void OnGameStarted(MultiplayerMode multiplayerMode, string relayCode)
    {
        if (multiplayerMode == MultiplayerMode.LocalMultiplayer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    private void OnEnable()
    {
        LobbyManager.onGameStarted += OnGameStarted;

        if (playerInputManager != null)
            playerInputManager.EnableJoining();

        LocalLobbyPlayer.onPlayerReady += OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady += OnPlayerUnready;
        LocalLobbyPlayer.onPlayerLeave += OnPlayerLeave;

        CheckIfCanStart();
    }

    private void OnDisable()
    {
        LobbyManager.onGameStarted -= OnGameStarted;

        if (playerInputManager != null)
            playerInputManager.DisableJoining();

        LocalLobbyPlayer.onPlayerReady -= OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady -= OnPlayerUnready;
        LocalLobbyPlayer.onPlayerLeave -= OnPlayerLeave;
    }
}
