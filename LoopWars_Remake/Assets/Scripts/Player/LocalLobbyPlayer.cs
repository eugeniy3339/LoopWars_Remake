using LoopWars.Players;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalLobbyPlayer : MonoBehaviour
{
    private PlayerInput playerInput;
    public Player player;

    private bool ready;

    public static Action<Player> onPlayerReady;
    public static Action<Player> onPlayerUneady;

    public static Action<Player> onPlayerLeave;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnReadyUnready(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        ready = !ready;
        if (ready)
            onPlayerReady?.Invoke(player);
        else
            onPlayerUneady?.Invoke(player);
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if(context.started)
            LocalMultiplayerLobbyManager.Instance.StartGameIfCanTo();
    }

    public void OnLeave(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        onPlayerLeave?.Invoke(player);
    }
}
