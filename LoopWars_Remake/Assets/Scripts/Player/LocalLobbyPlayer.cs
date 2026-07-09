using LoopWars.Players;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalLobbyPlayer : MonoBehaviour
{
    private PlayerInput playerInput;
    [HideInInspector] public LobbyPlayerUIHandler lobbyPlayerUIHandler;
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
        lobbyPlayerUIHandler.SetUp(player.name, ready, player.color);
        if (ready)
            onPlayerReady?.Invoke(player);
        else
            onPlayerUneady?.Invoke(player);
    }

    public void OnLeave(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        onPlayerLeave?.Invoke(player);
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!LocalMultiplayerLobby.Instance.CanStart()) return;
        LobbyManager.Instance.StartLocalGame();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        CustomEventSystem.Instance?.OnMove(context, playerInput);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        CustomEventSystem.Instance?.OnInteract(context, playerInput);
    }

    private void OnDestroy()
    {
        if (ready)
            onPlayerUneady?.Invoke(player);
    }
}
