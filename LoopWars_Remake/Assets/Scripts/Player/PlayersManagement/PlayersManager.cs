using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class PlayersManager : MonoBehaviour
{
    public static List<Player> players = new List<Player>();
    private List<PlayerInput> alivePlayers = new List<PlayerInput>();

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerSpawned;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerDied;

        foreach (var player in players)
        {
            PlayerInputManager.instance.JoinPlayer(-1, -1, player.controllScheme, player.devices[0]);
        }
    }

    private void OnPlayerSpawned(PlayerInput playerInput)
    {
        print(playerInput + " Spawned");
        if (alivePlayers.Contains(playerInput)) return;

        alivePlayers.Add(playerInput);
        Player player = Player.GetPlayerByDevice(playerInput.devices[0]);
        player.character = playerInput.GetComponent<Character>();
    }

    private void OnPlayerDied(PlayerInput playerInput)
    {
        print(playerInput + " Left");
        if (!alivePlayers.Contains(playerInput)) return;

        alivePlayers.Remove(playerInput);
        if(alivePlayers.Count == 1)
        {
            EndRound(Player.GetPlayerByDevice(alivePlayers[0].devices[0]));
        }
    }

    private void EndRound(Player winner)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public class Player
    {
        public ReadOnlyArray<InputDevice> devices;
        public string controllScheme;
        public PlayerInput playerInput;
        private Character _char;
        public Character character
        {
            get { return character; }
            set { 
                character = value;
                playerInput = character.playerInput;
            }
        }

        public Player(ReadOnlyArray<InputDevice> devices, string controllScheme)
        {
            this.devices = devices;
            this.controllScheme = controllScheme;
        }

        public static Player GetPlayerByDevice(InputDevice device)
        {
            return players.Find((player) => { foreach (var playerDevice in player.devices) { if (playerDevice == device) return true; } return false; });
        }

        public static Player GetPlayerByPlayerInput(PlayerInput playerInput)
        {
            return players.Find((player) => player.playerInput == playerInput);
        }
    }
}
