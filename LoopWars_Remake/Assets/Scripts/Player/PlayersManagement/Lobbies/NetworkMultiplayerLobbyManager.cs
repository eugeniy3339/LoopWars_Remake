using LoopWars.GameMode;
using LoopWars.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkMultiplayerLobbyManager : NetworkBehaviour
{
    private static NetworkMultiplayerLobbyManager _i;
    public static NetworkMultiplayerLobbyManager Instance { get { if (_i == null) { _i = FindObjectOfType<NetworkMultiplayerLobbyManager>(); } return _i; } private set { _i = value; } }

    private NetworkList<PlayerToSynchronize> playersToSynchronize = new NetworkList<PlayerToSynchronize>();

    private int maxPlayers;

    private JoinAllocation curJoinedAllocation;
    private Allocation curHostedAllocation;

    private List<int> usedColors = new List<int>();

    public static event Action<Player> onPlayerJoined;
    public static event Action<Player> onPlayerLeft;

    public static event Action<Player> onPlayerReady;
    public static event Action<Player> onPlayerUnready;

    private bool canStart;
    public static event Action<bool> onCanStartChanged;

    public static Action<bool, string> onJoinedLobby;

    [HideInInspector] public string curRelayCode;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playersToSynchronize.OnListChanged += OnPlayersToSynchronizeChanged;
        foreach (var playerToSynchronize in playersToSynchronize)
        {
            OnPlayerAdded(playerToSynchronize);
        }

        AddPlayerRpc(PlayerSettings.name, NetworkManager.Singleton.LocalClientId);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        playersToSynchronize.OnListChanged -= OnPlayersToSynchronizeChanged;
    }



    public void JoinLobby()
    {
        JoinGame(curRelayCode);
    }

    public async void JoinGame(string joinCode)
    {
        try
        {
            if (curJoinedAllocation != null) return;
            curJoinedAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                curJoinedAllocation.RelayServer.IpV4,
                (ushort)curJoinedAllocation.RelayServer.Port,
                curJoinedAllocation.AllocationIdBytes,
                curJoinedAllocation.Key,
                curJoinedAllocation.ConnectionData,
                curJoinedAllocation.HostConnectionData
            );

            GameMode.multiplayerMode = MultiplayerMode.NetworkMultiplayer;
            NetworkManager.Singleton.StartClient();
            onJoinedLobby?.Invoke(false, joinCode);
        }
        catch
        {

        }
    }

    public async void CreateLobby()
    {
        await CreateGameAsync(4);
    }

    public async Task<string> CreateGameAsync(int maxPlayersCount)
    {
        try
        {
            if (curHostedAllocation != null) return null;
            maxPlayers = maxPlayersCount;
            curHostedAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayersCount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(curHostedAllocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                curHostedAllocation.RelayServer.IpV4,
                (ushort)curHostedAllocation.RelayServer.Port,
                curHostedAllocation.AllocationIdBytes,
                curHostedAllocation.Key,
                curHostedAllocation.ConnectionData
            );

            GameMode.multiplayerMode = MultiplayerMode.NetworkMultiplayer;
            NetworkManager.Singleton.StartServer();
            onJoinedLobby?.Invoke(true, joinCode);
            return joinCode;
        }
        catch
        {
            return null;
        }
    }



    [Rpc(SendTo.Server)]
    private void AddPlayerRpc(FixedString32Bytes playerName, ulong playerId)
    {
        int playerColor = Player.GetRandomColor(usedColors.ToArray());
        playersToSynchronize.Add(new PlayerToSynchronize
        {
            Id = playerId,
            Name = playerName,
            Color = playerColor
        });

        usedColors.Add(playerColor);
    }

    private void OnPlayersToSynchronizeChanged(NetworkListEvent<PlayerToSynchronize> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<PlayerToSynchronize>.EventType.Remove)
        {
            OnPlayerRemoved(changeEvent.Value);
        }
        else if (changeEvent.Type == NetworkListEvent<PlayerToSynchronize>.EventType.Add)
        {
            OnPlayerAdded(changeEvent.Value);
        }
        else if (changeEvent.Type == NetworkListEvent<PlayerToSynchronize>.EventType.Value)
            OnPlayerChanged(changeEvent.Value);

        CheckIfCanStartGame();
    }

    private void OnPlayerChanged(PlayerToSynchronize changedPlayer)
    {
        Player player = PlayersContainer.GetPlayerById(changedPlayer.Id);
        if (player == null) return;

        if (changedPlayer.IsReady)
            onPlayerReady?.Invoke(player);
        else
            onPlayerUnready?.Invoke(player);
    }

    private void OnPlayerRemoved(PlayerToSynchronize removedPlayer)
    {
        Player player = PlayersContainer.GetPlayerById(removedPlayer.Id);
        if (player != null)
            KickPlayer(player);
        onPlayerLeft?.Invoke(player);
    }

    private void OnPlayerAdded(PlayerToSynchronize addedPlayer)
    {
        Player player = PlayersContainer.GetPlayerById(addedPlayer.Id);
        if (player != null) return;

        player = new Player(addedPlayer.Id);
        player.color = Player.playersColors[addedPlayer.Color];
        player.name = addedPlayer.Name.ToString();
        PlayersContainer.AddPlayer(player);
        onPlayerJoined?.Invoke(player);
        if (addedPlayer.IsReady)
            onPlayerReady?.Invoke(player);
    }

    private void KickPlayer(Player player)
    {
        if (player == null) return;

        int color = Player.playersColors.IndexOf(player.color);
        if (usedColors.Contains(color))
            usedColors.Remove(color);

        PlayersContainer.KickPlayer(player);
    }



    public void Ready()
    {
        ReadyRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void Unready()
    {
        UnreadyRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void ReadyRpc(ulong clientId)
    {
        UpdatePlayerReadyState(clientId, true);
    }

    [Rpc(SendTo.Server)]
    private void UnreadyRpc(ulong clientId)
    {
        UpdatePlayerReadyState(clientId, false);
    }

    private void UpdatePlayerReadyState(ulong clientId, bool isReady)
    {
        if (!IsServer) return;
        foreach (var player in playersToSynchronize)
        {
            if (player.Id == clientId)
            {
                PlayerToSynchronize updatedPlayer = player;
                updatedPlayer.IsReady = isReady;
                playersToSynchronize[playersToSynchronize.IndexOf(player)] = updatedPlayer;
                break;
            }
        }
    }



    private void CheckIfCanStartGame()
    {
        bool canStart = CanStartGame();
        if (this.canStart != canStart)
        {
            this.canStart = canStart;
            onCanStartChanged?.Invoke(canStart);
        }
    }

    private bool CanStartGame()
    {
        return playersToSynchronize.Count > 1 && AllPlayersReady();
    }

    private bool AllPlayersReady()
    {
        foreach (var player in playersToSynchronize)
        {
            if (!player.IsReady)
                return false;
        }
        return true;
    }

    public void StartGame()
    {
        GameMode.multiplayerMode = MultiplayerMode.NetworkMultiplayer;
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }


    private void OnClientConnected(ulong clientId)
    {
        if (GameMode.multiplayerMode != MultiplayerMode.NetworkMultiplayer) return;
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count > maxPlayers)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }
    }

    public struct PlayerToSynchronize : INetworkSerializable, IEquatable<PlayerToSynchronize>
    {
        public FixedString32Bytes Name;
        public int Color;
        public ulong Id;
        public bool IsReady;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Color);
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref IsReady);
        }

        public bool Equals(PlayerToSynchronize other)
        {
            return Id == other.Id &&
                   Name.Equals(other.Name) &&
                   Color.Equals(other.Color) &&
                   IsReady.Equals(other.IsReady);
        }
    }
}
