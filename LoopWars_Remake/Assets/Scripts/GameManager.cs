using LoopWars.GameMode;
using LoopWars.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private Coroutine curEndRoundCoroutine;

    private int loadedPlayersCount;

    public static event Action onAllThePlayersLoaded;
    public static event Action onGameStarted;
    public static event Action onGameEnded;

    private bool subscribedToPlayerDiedEvent = false;

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
            onAllThePlayersLoaded?.Invoke();
        }
    }



    private void EndCurRound()
    {
        UnsubscribeFromOnPlayerDiedEvent();
        onGameEnded?.Invoke();
    }

    private void StartNextRound()
    {
        UnsubscribeFromOnPlayerDiedEvent();
        onGameStarted?.Invoke();
    }



    private void EndRound(Player winner)
    {
        if (!IsServer) return;
        if (GameMode.multiplayerMode == MultiplayerMode.NetworkMultiplayer)
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
        EndCurRound();
        Debug.Log(winner.name + " has won!");

        yield return new WaitForSeconds(3f);

        /*if (IsServer)
            NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);*/
        StartNextRound();
    }



    private void OnMusicStarted(SoundScriptableObject music)
    {
        StartNextRound();
    }



    private void OnPlayerDied(Character character, List<Character> alivePlayers) // On player object is destroyed
    {
        if (alivePlayers.Count == 1)
            EndRound(PlayersContainer.GetPlayerByCharacter(alivePlayers[0]));
    }

    private void SubscribeToOnPlayerDiedEvent()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (subscribedToPlayerDiedEvent) return;

        subscribedToPlayerDiedEvent = true;
        PlayersManager.onPlayerDied += OnPlayerDied;
    }

    private void UnsubscribeFromOnPlayerDiedEvent()
    {
        subscribedToPlayerDiedEvent = false;
        PlayersManager.onPlayerDied -= OnPlayerDied;
    }

    private void OnAllPlayersSpawned()
    {
        SubscribeToOnPlayerDiedEvent();
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
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;

        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        MusicManager.onMusicStarted += OnMusicStarted;
        PlayersManager.onAllPlayersSpawned += OnAllPlayersSpawned;
    }

    private void OnDisable()
    {
        try
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
        catch { }

        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        MusicManager.onMusicStarted -= OnMusicStarted;
        PlayersManager.onAllPlayersSpawned -= OnAllPlayersSpawned;
    }

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
