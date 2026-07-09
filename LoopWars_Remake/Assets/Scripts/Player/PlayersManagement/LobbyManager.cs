using LoopWars.GameMode;
using LoopWars.Players;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private int gameScene = 1;
    public static event Action<MultiplayerMode, string> onGameStarted;

    [SerializeField] private List<Color> playersColors = new List<Color>();

    private void Awake()
    {
        Instance = this;
        Player.playersColors = playersColors;
    }

    public static void JoinPlayer(Player player)
    {
        PlayersContainer.AddPlayer(player);
    }

    public static void KickPlayer(Player player)
    {
        PlayersContainer.KickPlayer(player);
    }

    public void StartLocalGame()
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
        onGameStarted?.Invoke(MultiplayerMode.LocalMultiplayer, "");
    }

    public async void StartNetworkGame()
    {
        GameMode.multiplayerMode = MultiplayerMode.NetworkMultiplayer;
        onGameStarted?.Invoke(MultiplayerMode.NetworkMultiplayer, await NetworkMultiplayerLobby.Instance.CreateRelayAsync());
    }
}
