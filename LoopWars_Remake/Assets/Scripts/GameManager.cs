using LoopWars.Players;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private Coroutine curEndRoundCoroutine;

    private void Awake()
    {
        if (curEndRoundCoroutine != null)
            StopCoroutine(curEndRoundCoroutine);
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
            PlayersManager.onPlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
            PlayersManager.onPlayerDied -= OnPlayerDied;
    }
}
