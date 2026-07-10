using LoopWars.Players;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private Coroutine curEndRoundCoroutine;

    private int loadedPlayersCount;

    private void Awake()
    {
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
        EndRoundRpc(winner.playerId);
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



    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayersManager.onPlayerDied += OnPlayerDied;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayersManager.onPlayerDied -= OnPlayerDied;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }
}
