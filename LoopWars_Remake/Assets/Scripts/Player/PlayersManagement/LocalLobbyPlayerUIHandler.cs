using UnityEngine;
using UnityEngine.InputSystem;

public class LocalLobbyPlayerUIHandler : LobbyPlayerUIHandler
{
    public void SetUp(string name, bool ready, Color color, PlayerInput playerInput)
    {
        SetUp(name, ready, color);
        foreach(var customISelectable in GetComponentsInChildren<CustomISelectable>())
        {
            customISelectable.playerInput = playerInput;
        }
    }
}
