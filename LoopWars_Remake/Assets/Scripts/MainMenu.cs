using System.Collections.Generic;
using UnityEngine;

public class MainMenu : WindowsManager
{
    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject localMultiplayerLobbyPanel;

    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject networkMultiplayerLobbyPanel;

    public void OpenLocalMultiplayerLobby()
    {
        OpenPanel(localMultiplayerLobbyPanel);
        LocalMultiplayerLobby.Instance.enabled = true;
    }

    public void OpenNetworkMultiplayerLobby()
    {
        OpenPanel(networkMultiplayerLobbyPanel);
        LocalMultiplayerLobby.Instance.enabled = false;
    }
}
