using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
using LoopWars.Players;
using System;
using Unity.Netcode;
using LoopWars.GameMode;

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
        return players.Find((player) => player.character == character);
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
                GameObject.Destroy(player.playerInput.gameObject);
        }
    }
}


namespace LoopWars
{
    namespace Players
    {
        public class Player
        {
            public MultiplayerMode multiplayerMode;

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
