using LoopWars.Players;
using LoopWars.GameMode;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayersScoreManager : MonoBehaviour
{
    [SerializeField] private Transform playersScoreContainer;
    private GameObject _pSP;
    private GameObject playerScorePrefab
    {
        get
        {
            if (_pSP == null)
                _pSP = Resources.Load<GameObject>("PlayerScoreHandler");
            return _pSP;
        }
    }

    private Dictionary<Player, int> playersScore = new Dictionary<Player, int>();
    private Dictionary<Player, PlayerScoreHandler> playersScoreHandlers = new Dictionary<Player, PlayerScoreHandler>();

    private void Awake()
    {
        CreatePlayersScoreUIHandlers();
    }

    private void CreatePlayersScoreUIHandlers()
    {
        foreach (var player in PlayersContainer.players)
        {
            if (player.multiplayerMode == GameMode.multiplayerMode)
            {
                playersScore.Add(player, 0);
                CreateNewPlayerScoreHandler(player);
            }
        }
    }

    private void CreateNewPlayerScoreHandler(Player player)
    {
        playersScoreHandlers.Add(player, CreatePlayerScoreHandler(player));
    }

    private PlayerScoreHandler CreatePlayerScoreHandler(Player player)
    {
        PlayerScoreHandler playerScoreHandler = Instantiate(playerScorePrefab, playersScoreContainer).GetComponent<PlayerScoreHandler>();
        playerScoreHandler.color = player.color;
        playerScoreHandler.score = 0;
        return playerScoreHandler;
    }

    private void OnPlayerWon(Player player)
    {
        if (!playersScore.ContainsKey(player)) return;
        playersScore[player] += 1;
        if (!playersScoreHandlers.ContainsKey(player))
            CreateNewPlayerScoreHandler(player);
        playersScoreHandlers[player].score = playersScore[player];
    }

    private void OnEnable()
    {
        GameManager.onPlayerWon += OnPlayerWon;
    }

    private void OnDisable()
    {
        GameManager.onPlayerWon -= OnPlayerWon;
    }
}
