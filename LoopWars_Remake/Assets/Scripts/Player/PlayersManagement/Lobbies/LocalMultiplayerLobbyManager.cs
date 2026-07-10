using LoopWars.GameMode;
using LoopWars.Players;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LocalMultiplayerLobbyManager : MonoBehaviour
{
    private static LocalMultiplayerLobbyManager _i;
    public static LocalMultiplayerLobbyManager Instance { get { if (_i == null) { _i = FindObjectOfType<LocalMultiplayerLobbyManager>(); } return _i; } private set { _i = value; } }

    private PlayerInputManager playerInputManager;

    private List<Player> connectedPlayers = new List<Player>();
    private List<Player> readyPlayers = new List<Player>();

    private List<int> usedColors = new List<int>();

    public static event Action<Player> onPlayerJoined;
    public static event Action<Player> onPlayerLeft;

    private bool canStart;
    public static event Action<bool> onCanStartChanged;

    private void Awake()
    {
        Instance = this;

        playerInputManager = FindAnyObjectByType<PlayerInputManager>();
    }

    private void Start()
    {
        KickConnectedPlayers();
        enabled = false;
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

        PlayersContainer.AddPlayer(player);
        connectedPlayers.Add(player);

        playerInput.onDeviceLost += OnDeviceDiscontected;

        LocalLobbyPlayer localLobbyPlayer = playerInput.GetComponent<LocalLobbyPlayer>();
        localLobbyPlayer.player = player;

        onPlayerJoined?.Invoke(player);

        CheckIfCanStart();
    }

    private void OnPlayerLeft(PlayerInput playerInput) //On player object is despawned
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
        PlayersContainer.KickPlayer(player);
        onPlayerLeft?.Invoke(player);
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
        bool canStart = CanStart();
        if(canStart != this.canStart)
        {
            this.canStart = canStart;
            onCanStartChanged?.Invoke(canStart);
        }
    }

    public bool CanStart()
    {
        return connectedPlayers.Count > 1 && readyPlayers.Count == connectedPlayers.Count;
    }

    public void StartGameIfCanTo()
    {
        if (CanStart())
            StartGame();
    }

    public void StartGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(
            ipv4Address: "127.0.0.1",
            port: 7777,
            listenAddress: "0.0.0.0");

        bool startedHost = NetworkManager.Singleton.StartHost();
        if (!startedHost)
            return;
        GameMode.multiplayerMode = MultiplayerMode.LocalMultiplayer;
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }



    private void OnGameStarted(MultiplayerMode multiplayerMode, string relayCode)
    {
        if (multiplayerMode == MultiplayerMode.LocalMultiplayer)
        {
            
        }
        else
        {
            KickConnectedPlayers();
        }
    }

    private void KickConnectedPlayers()
    {
        for (int i = PlayersContainer.players.Count - 1; i >= 0; i--)
        {
            Player player = PlayersContainer.players[i];
            if (player.multiplayerMode == MultiplayerMode.LocalMultiplayer)
                KickPlayer(player);
        }
    }

    private void OnEnable()
    {
        if (playerInputManager != null)
            playerInputManager.EnableJoining();

        LocalLobbyPlayer.onPlayerReady += OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady += OnPlayerUnready;
        LocalLobbyPlayer.onPlayerLeave += OnPlayerLeave;

        CheckIfCanStart();
    }

    private void OnDisable()
    {
        if (playerInputManager != null)
            playerInputManager.DisableJoining();

        LocalLobbyPlayer.onPlayerReady -= OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady -= OnPlayerUnready;
        LocalLobbyPlayer.onPlayerLeave -= OnPlayerLeave;
    }
}
