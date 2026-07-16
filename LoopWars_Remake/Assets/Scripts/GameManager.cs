using LoopWars.Players;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using LoopWars.GameMode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private Coroutine curEndRoundCoroutine;

    private int loadedPlayersCount;

    private void Awake()
    {
        Instance = this;

        if (curEndRoundCoroutine != null)
            StopCoroutine(curEndRoundCoroutine);
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (sceneName != "GameScene") return;

        loadedPlayersCount++;

        if (loadedPlayersCount >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGame();
        }
    }

    private void SpawnMap()
    {
        if (!IsServer) return;
        GameObject map = Instantiate(MapsListScriptableObject.Instance.GetRandomMap());
        map.GetComponent<NetworkObject>().Spawn(true);
    }

    private void StartGame()
    {
        if (MapHandler.Instance == null)
            SpawnMap();
        PlayersManager.Instance.SpawnPlayers();
    }

    private void EndRound(Player winner)
    {
        if (!IsServer) return;
        if(GameMode.multiplayerMode == MultiplayerMode.NetworkMultiplayer)
            EndRoundRpc(winner.playerId);
        else
            curEndRoundCoroutine = StartCoroutine(EndRoundCoro(winner));
    }

    [Rpc(SendTo.Everyone)]
    private void EndRoundRpc(ulong winnerId)
    {
        curEndRoundCoroutine = StartCoroutine(EndRoundCoro(PlayersContainer.GetPlayerById(winnerId)));
    }

    private IEnumerator EndRoundCoro(Player winner)
    {
        Debug.Log(winner.name + " has won!");

        yield return new WaitForSeconds(3f);

        if (IsServer)
            NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }



    private void OnPlayerDied(Character character, List<Character> alivePlayers) // On player object is destroyed
    {
        if(alivePlayers.Count == 1)
            EndRound(PlayersContainer.GetPlayerByCharacter(alivePlayers[0]));
    }

    private void OnClientDisconnected(ulong playerId)
    {
        Player player = PlayersContainer.GetPlayerById(playerId);
        PlayersContainer.KickPlayer(player);

        if (PlayersContainer.GetPlayersByMultiplayerMode(GameMode.multiplayerMode).Count < 2)
        {
            NetworkManager.Singleton.Shutdown();
        }

    }

    private void OnClientStopped(bool b)
    {
        OnGameStopped();
    }

    private void OnServerStopped(bool b)
    {
        OnGameStopped();
    }

    private void OnGameStopped()
    {
        SceneManager.LoadScene(0);
    }



    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayersManager.onPlayerDied += OnPlayerDied;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }

        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        PlayersManager.onPlayerDied -= OnPlayerDied;
        try
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
        catch { }

        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
