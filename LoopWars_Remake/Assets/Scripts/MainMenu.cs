using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : WindowsManager
{
    public static MainMenu Instance { get; private set; }

    [SerializeField] private GameObject lobbyPlayerUIPrefab;

    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject localMultiplayerLobbyPanel;
    [SerializeField] private Button localMultiplayerStartButton;
    [SerializeField] private Transform _localMultiplayerPlayersUIsContainer;
    public Transform localMultiplayerPlayersUIsContainer { get { return _localMultiplayerPlayersUIsContainer; } }

    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject networkMultiplayerLobbyPanel;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

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

    public void ActivateLocalMultiplayerStartButton(bool activate)
    {
        localMultiplayerStartButton.interactable = activate;
    }


    public LobbyPlayerUIHandler CreateNewLobbyUI(Transform container, string name, Color color)
    {
        LobbyPlayerUIHandler lobbyPlayerUIHandler = Instantiate(lobbyPlayerUIPrefab, container).GetComponent<LobbyPlayerUIHandler>();
        lobbyPlayerUIHandler.SetUp(name, false, color);
        return lobbyPlayerUIHandler;
    }
}
