using LoopWars.Players;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkPlayersSynchronizer : NetworkBehaviour
{
    public static NetworkPlayersSynchronizer Instance { get; private set; }

    public NetworkList<PlayerToSynchronize> playersToSynchronize = new NetworkList<PlayerToSynchronize>();

    [HideInInspector] public string localPlayerName;
    [HideInInspector] public int localPlayerColor;

    private bool toJoinPlayer = false;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playersToSynchronize.OnListChanged += OnPlayersToSynchronizeChanged;
        foreach(var playerToSynchronize in playersToSynchronize)
        {
            OnPlayerAdded(playerToSynchronize);
        }

        if(toJoinPlayer)
        {
            toJoinPlayer = false;
            JoinPlayerRpc(NetworkManager.LocalClientId, localPlayerColor, localPlayerName);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        playersToSynchronize.OnListChanged -= OnPlayersToSynchronizeChanged;
    }

    public void JoinLocalPlayer(string name, int color)
    {
        localPlayerName = name;
        localPlayerColor = color;
        if (!NetworkObject.IsSpawned)
        { 
            toJoinPlayer = true; 
            return; 
        }

        toJoinPlayer = false;
        JoinPlayerRpc(NetworkManager.LocalClientId, localPlayerColor, localPlayerName);
    }

    [Rpc(SendTo.Server)]
    private void JoinPlayerRpc(ulong clientId, int color, FixedString32Bytes name)
    {
        NetworkMultiplayerLobby.Instance.JoinPlayer(clientId, color, name);
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
    }

    private void OnPlayerRemoved(PlayerToSynchronize removedPlayer)
    {
        Player player = PlayersContainer.GetPlayerById(removedPlayer.Id);
        if (player != null)
            PlayersContainer.KickPlayer(player);
    }

    private void OnPlayerAdded(PlayerToSynchronize addedPlayer)
    {
        Player player = PlayersContainer.GetPlayerById(addedPlayer.Id);
        if (player != null) return;

        player = new Player(addedPlayer.Id);
        print(addedPlayer.Color);
        player.color = Player.playersColors[addedPlayer.Color];
        player.name = addedPlayer.Name.ToString();
        PlayersContainer.AddPlayer(player);
    }





    public struct PlayerToSynchronize : INetworkSerializable, IEquatable<PlayerToSynchronize>
    {
        public FixedString32Bytes Name;
        public int Color;
        public ulong Id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Color);
            serializer.SerializeValue(ref Id);
        }

        public bool Equals(PlayerToSynchronize other)
        {
            return Id == other.Id &&
                   Name.Equals(other.Name) &&
                   Color.Equals(other.Color);
        }
    }
}