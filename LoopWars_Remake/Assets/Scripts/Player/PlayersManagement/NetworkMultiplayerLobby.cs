using LoopWars.GameMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkMultiplayerLobby : MonoBehaviour
{
    public static NetworkMultiplayerLobby Instance { protected set; get; }

    private Lobby hostedLobby;
    private Lobby _jL;
    private Lobby joinedLobby
    {
        get
        {
            return _jL;
        }
        set
        {
            Lobby oldLobby = _jL;
            _jL = value;

            OnLobbyChanged(oldLobby, value);
        }
    }

    const float heartbeatTimer = 15f;
    float curHeartbeatTimer;
    const float getLobbyTimer = 1.1f;
    float curGetLobbyTimer;

    public event Action<string, bool> onGameCodeChanged;
    public event Action<List<Player>, bool> onPlayersChanged;

    private bool toStartGame;
    private int lobbyPlayersCount;
    private int joinedPlayersCount;



    private void Awake()
    {
        Instance = this;

        curGetLobbyTimer = getLobbyTimer;
        curHeartbeatTimer = heartbeatTimer;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HeartbeatTimer();
        GetLobbyTimer();
    }


    private void HeartbeatTimer()
    {
        if (hostedLobby == null) return;

        if (curHeartbeatTimer > 0f)
        {
            curHeartbeatTimer -= Time.deltaTime;
            if (curHeartbeatTimer <= 0f)
            {
                OnHeartbeatTimer();
            }
        }
    }

    private void OnHeartbeatTimer()
    {
        curHeartbeatTimer = heartbeatTimer;
        HeartbeatLobby();
    }

    private void HeartbeatLobby()
    {
        try
        {
            if (hostedLobby == null) return;
            LobbyService.Instance.SendHeartbeatPingAsync(hostedLobby.Id);
        }
        catch
        {

        }
    }


    private void GetLobbyTimer()
    {
        if (joinedLobby == null) return;

        if (curGetLobbyTimer > 0f)
        {
            curGetLobbyTimer -= Time.deltaTime;
            if (curGetLobbyTimer <= 0f)
            {
                OnGetLobbyTimer();
            }
        }
    }

    private void OnGetLobbyTimer()
    {
        curGetLobbyTimer = getLobbyTimer;
        GetJoinedLobbyAsync();
    }




    public void QuickCreateLobby()
    {
        CreateLobby("MyLobby", 4, false);
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        try
        {
            //Change Later

            hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject> {
                    { "GameCode", new DataObject(DataObject.VisibilityOptions.Member, "", DataObject.IndexOptions.S5) }
                }
            });

            joinedLobby = hostedLobby;
            curHeartbeatTimer = heartbeatTimer;
        }
        catch
        {

        }
    }




    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach (var lobby in queryResponse.Results)
            {
                print(lobby.Name);
                // Change Later
            }
        }
        catch
        {

        }
    }

    public async void JoinLobbyByCode(string code)
    {
        try
        {
            if (joinedLobby != null) return;
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
        }
        catch
        {

        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            if (joinedLobby != null) return;
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions { Filter = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) } });
        }
        catch
        {

        }
    }




    public async Task<string> CreateRelayAsync()
    {
        try
        {
            if (hostedLobby == null) return "";

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(hostedLobby.MaxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch
        {
            return "";
        }
    }

    private async void JoinRelay(string code)
    {
        if (code == string.Empty) return;
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

        NetworkManager.Singleton.StartClient();
    }




    public async void UpdateLobbyDataAsync(string gameCode = "")
    {
        try
        {
            if (hostedLobby == null) return;
            hostedLobby = await Lobbies.Instance.UpdateLobbyAsync(hostedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                { "GameCode", new DataObject(DataObject.VisibilityOptions.Member, gameCode, DataObject.IndexOptions.S5) }
            }
            });

            joinedLobby = hostedLobby;
        }
        catch
        {

        }
    }

    public async void GetJoinedLobbyAsync()
    {
        try
        {
            if (joinedLobby == null) return;

            joinedLobby = await Lobbies.Instance.GetLobbyAsync(joinedLobby.Id);
        }
        catch
        {

        }
    }

    private void OnLobbyChanged(Lobby oldLobby, Lobby curLobby)
    {
        string oldGameCode = oldLobby?.Data["GameCode"].Value;
        List<Player> oldPlayers = oldLobby?.Players;

        string curGameCode = curLobby?.Data["GameCode"].Value;
        List<Player> curPlayers = curLobby?.Players;

        if (curGameCode != oldGameCode)
            onGameCodeChanged?.Invoke(curGameCode, hostedLobby != null);

        if (!SameList(oldPlayers, curPlayers))
            onPlayersChanged?.Invoke(curPlayers, hostedLobby != null);
    }

    private bool SameList(List<Player> list1, List<Player> list2)
    {
        if ((list1 == null && list2 != null) || (list1 != null && list2 == null)) { return false; }
        else if (list1 == null && list2 == null) { return true; }

        List<string> firstNotSecond = new List<string>();
        List<string> secondNotFirst = new List<string>();

        foreach (var player in list1)
        {
            if (list2.Find((x) => x.Id == player.Id) == null)
            { firstNotSecond.Add(player.Id); }
        }

        foreach (var player in list2)
        {
            if (list1.Find((x) => x.Id == player.Id) == null)
            { secondNotFirst.Add(player.Id); }    
        }

        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    private void OnGameCodeChanged(string newGameCode, bool isHost)
    {
        if (isHost) return;

        JoinRelay(newGameCode);
    }

    private void OnPlayersChanged(List<Player> players, bool isHost)
    {

    }



    private void OnGameStarted(MultiplayerMode multiplayerMode, string relayCode)
    {
        if (multiplayerMode == MultiplayerMode.NetworkMultiplayer)
        {
            if (hostedLobby == null) return;
            UpdateLobbyDataAsync(relayCode);

            lobbyPlayersCount = joinedLobby.Players.Count;
            toStartGame = true;
        }
    }



    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (GameMode.multiplayerMode != MultiplayerMode.NetworkMultiplayer) return;
        if (PlayersContainer.GetPlayerById(clientId) != null) return;

        LoopWars.Players.Player player = new LoopWars.Players.Player(clientId);
        PlayersContainer.AddPlayer(player);
        joinedPlayersCount++;

        if (toStartGame && joinedPlayersCount >= lobbyPlayersCount)
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnServerStarted()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        OnClientConnected(NetworkManager.Singleton.LocalClientId);
    }



    private void OnEnable()
    {
        onGameCodeChanged += OnGameCodeChanged;
        onPlayersChanged += OnPlayersChanged;
        LobbyManager.onGameStarted += OnGameStarted;

        NetworkManager networkManager = NetworkManager.Singleton != null ? NetworkManager.Singleton : FindAnyObjectByType<NetworkManager>();

        networkManager.OnServerStarted += OnServerStarted;
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        onGameCodeChanged -= OnGameCodeChanged;
        onPlayersChanged -= OnPlayersChanged;
        LobbyManager.onGameStarted -= OnGameStarted;

        NetworkManager networkManager = NetworkManager.Singleton != null ? NetworkManager.Singleton : FindAnyObjectByType<NetworkManager>();

        networkManager.OnServerStarted -= OnServerStarted;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }
}
