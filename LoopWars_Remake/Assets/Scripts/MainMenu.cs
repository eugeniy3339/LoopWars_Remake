using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu : WindowsManager
{
    public static MainMenu Instance { get; private set; }

    [SerializeField] private GameObject localLobbyPlayerUIPrefab;
    [SerializeField] private GameObject lobbyPlayerUIPrefab;

    [Header("Player Settings")]
    [SerializeField] private TMP_InputField playerNameInputField;
    private string lastPlayerName;

    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject localMultiplayerLobbyPanel;
    [SerializeField] private Button localMultiplayerStartButton;
    [SerializeField] private Transform _localMultiplayerPlayersUIsContainer;
    public Transform localMultiplayerPlayersUIsContainer { get { return _localMultiplayerPlayersUIsContainer; } }

    [Header("Network Multiplayer Lobby")]
    [SerializeField] private GameObject networkMultiplayerLobbyPanel;
    [SerializeField] private Transform _networkMultiplayerPlayersUIsContainer;
    public Transform networkMultiplayerPlayersUIsContainer { get { return _networkMultiplayerPlayersUIsContainer; } }
    [SerializeField] private Button networkMultiplayerReadyButton, networkMultiplayerUnreadyButton;
    [SerializeField] private Button _networkMultiplayerStartButton;
    public Button networkMultiplayerStartButton { get { return _networkMultiplayerStartButton; } }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        lastPlayerName = PlayerSettings.name;
        playerNameInputField.text = PlayerSettings.name;

        networkMultiplayerReadyButton.gameObject.SetActive(false);
        networkMultiplayerUnreadyButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.interactable = false;
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
        //localMultiplayerStartButton.interactable = activate;
    }

    private void ReadyUnreadyNetworkLobby(bool ready)
    {
        NetworkMultiplayerLobby.Instance.UpdatePlayerDataAsync(ready);
        networkMultiplayerReadyButton.gameObject.SetActive(!ready);
        networkMultiplayerUnreadyButton.gameObject.SetActive(ready);
    }

    private void ReadyNetworkLobby()
    {
        ReadyUnreadyNetworkLobby(true);
    }

    private void UnreadyNetworkLobby()
    {
        ReadyUnreadyNetworkLobby(false);
    }


    public LobbyPlayerUIHandler CreateNewLobbyUI(Transform container, string name, Color color, bool ready = false)
    {
        LobbyPlayerUIHandler lobbyPlayerUIHandler = Instantiate(lobbyPlayerUIPrefab, container).GetComponent<LobbyPlayerUIHandler>();
        lobbyPlayerUIHandler.SetUp(name, ready, color);
        return lobbyPlayerUIHandler;
    }

    public LobbyPlayerUIHandler CreateNewLobbyUI(PlayerInput playerInput, Transform container, string name, Color color, bool ready = false)
    {
        LocalLobbyPlayerUIHandler lobbyPlayerUIHandler = Instantiate(localLobbyPlayerUIPrefab, container).GetComponent<LocalLobbyPlayerUIHandler>();
        lobbyPlayerUIHandler.SetUp(name, ready, color, playerInput);
        return lobbyPlayerUIHandler;
    }

    private void OnPlayerNameEntered(string newName)
    {
        if(Encoding.UTF8.GetByteCount(newName) > 32)
        {
            playerNameInputField.text = lastPlayerName;
            newName = lastPlayerName;
        }

        lastPlayerName = newName;
        PlayerSettings.name = newName;
    }

    private void OnJoinedNetworkLobby(bool isHost)
    {
        networkMultiplayerReadyButton.gameObject.SetActive(true);
        networkMultiplayerUnreadyButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.gameObject.SetActive(isHost);
    }

    private void OnEnable()
    {
        playerNameInputField?.onSubmit.AddListener(OnPlayerNameEntered);
        playerNameInputField?.onDeselect.AddListener(OnPlayerNameEntered);

        networkMultiplayerStartButton?.onClick.AddListener(LobbyManager.Instance.StartNetworkGame);
        networkMultiplayerReadyButton?.onClick.AddListener(ReadyNetworkLobby);
        networkMultiplayerUnreadyButton?.onClick.AddListener(UnreadyNetworkLobby);

        NetworkMultiplayerLobby.onJoinedLobby += OnJoinedNetworkLobby;
    }

    private void OnDisable()
    {
        playerNameInputField?.onSubmit.RemoveListener(OnPlayerNameEntered);
        playerNameInputField?.onDeselect.RemoveListener(OnPlayerNameEntered);

        networkMultiplayerStartButton?.onClick.RemoveListener(LobbyManager.Instance.StartNetworkGame);
        networkMultiplayerReadyButton?.onClick.RemoveListener(ReadyNetworkLobby);
        networkMultiplayerUnreadyButton?.onClick.RemoveListener(UnreadyNetworkLobby);

        NetworkMultiplayerLobby.onJoinedLobby -= OnJoinedNetworkLobby;
    }
}
