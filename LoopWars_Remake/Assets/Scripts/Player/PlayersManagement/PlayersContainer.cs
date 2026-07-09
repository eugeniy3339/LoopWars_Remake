using LoopWars.GameMode;
using LoopWars.Players;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public static class PlayersContainer
{
    public static List<Player> players = new List<Player>();

    public static void AddPlayer(Player player)
    {
        if (player == null) return;
        if (players.Contains(player)) return;

        players.Add(player);
    }

    public static Player GetPlayerByDevice(InputDevice device) //Used for local multiplayer
    {
        return players.Find((player) => { foreach (var playerDevice in player.devices) { if (playerDevice == device) return true; } return false; });
    }

    public static Player GetPlayerByPlayerInput(PlayerInput playerInput)
    {
        return players.Find((player) => player.playerInput == playerInput);
    }

    public static Player GetPlayerById(ulong id)
    {
        return players.Find((player) => player.playerId == id);
    }

    public static Player GetPlayerByCharacter(Character character)
    {
        Player player;

        if (GameMode.multiplayerMode == MultiplayerMode.LocalMultiplayer)
            player = GetPlayerByDevice(character.playerInput.devices[0]);
        else
            player = GetPlayerById(character.OwnerClientId);

        return player;
    }

    public static void KickPlayer(Player player)
    {
        if (player == null) return;

        players.Remove(player);

        if (player.playerInput != null)
        {
            NetworkObject playerNetowrkObject = player.playerInput.GetComponent<NetworkObject>();
            if (playerNetowrkObject != null && playerNetowrkObject.IsSpawned)
                playerNetowrkObject.Despawn();
            else
            { GameObject.Destroy(player.playerInput.gameObject); }
        }
    }
}


namespace LoopWars
{
    namespace Players
    {
        public class Player
        {
            public static List<Color> playersColors = new List<Color>();
            public static Color GetRandomColor()
            {
                return playersColors[UnityEngine.Random.Range(0, playersColors.Count)];
            }
            public static Color GetRandomColor(Color[] colorsToIgnore)
            {
                Color color;
                do
                {
                    color = GetRandomColor();
                }
                while (colorsToIgnore.Contains(color));
                return color;
            }
            public static int GetRandomColor(int[] colorsToIgnore)
            {
                int randomIndex;
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, playersColors.Count);
                }
                while (colorsToIgnore.Contains(randomIndex));
                Debug.Log(randomIndex);
                return randomIndex;
            }

            public MultiplayerMode multiplayerMode;

            public string name;
            public Color color;

            public List<InputDevice> devices = new List<InputDevice>(); //Used for local multiplayer
            public string controllScheme; //Used for local multiplayer

            public ulong playerId; // Used for network multiplayer;

            public PlayerInput playerInput;
            private Character _char;
            public Character character
            {
                get { return _char; }
                set
                {
                    _char = value;
                    playerInput = _char.playerInput;
                }
            }

            public Player(ReadOnlyArray<InputDevice> devices, string controllScheme)
            {
                this.devices.AddRange(devices);
                this.controllScheme = controllScheme;
                this.multiplayerMode = MultiplayerMode.LocalMultiplayer;
            }

            public Player(ulong playerId)
            {
                this.playerId = playerId;
                this.multiplayerMode = MultiplayerMode.NetworkMultiplayer;
            }
        }
    }

    namespace GameMode
    {
        public static class GameMode
        {
            public static MultiplayerMode multiplayerMode;
        }

        public enum MultiplayerMode
        {
            LocalMultiplayer,
            NetworkMultiplayer
        }
    }
}
