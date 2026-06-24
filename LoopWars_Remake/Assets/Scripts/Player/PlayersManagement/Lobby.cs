using UnityEngine;
using UnityEngine.InputSystem;

public class Lobby : MonoBehaviour
{
    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

        foreach(var player in PlayersManager.players)
        {
            if (player.devices[0] == null)
            {
                KickPlayer(player);
                continue;
            }

            PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, player.controllScheme, player.devices[0]);
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayersManager.Player player = PlayersManager.Player.GetPlayerByDevice(playerInput.devices[0]);
        if (player != null) return;

        player = new PlayersManager.Player(playerInput.devices, playerInput.currentControlScheme);
        player.playerInput = playerInput;
        PlayersManager.players.Add(player);

        playerInput.onDeviceLost += OnDeviceDiscontected;
    }

    private void OnDeviceDiscontected(PlayerInput playerInput)
    {
        KickPlayer(PlayersManager.Player.GetPlayerByPlayerInput(playerInput));
    }

    private void KickPlayer(PlayersManager.Player player)
    {
        if (player == null) return;

        PlayersManager.players.Remove(player);

        if(player.playerInput != null)
        {
            player.playerInput.onDeviceLost -= OnDeviceDiscontected;
            Destroy(player.playerInput.gameObject);
        }
    }

    private void OnPlayerLeft()
    {

    }
}
