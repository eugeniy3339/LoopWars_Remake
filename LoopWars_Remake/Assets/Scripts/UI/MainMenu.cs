using LoopWars.Players;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : WindowsManager
{
    public static MainMenu Instance { get; private set; }

    [SerializeField] private GameObject lobbyPlayerUIPrefab;

    [Header("Player Settings")]
    [SerializeField] private TMP_InputField playerNameInputField;
    private string lastPlayerName;

    [Header("Local Multiplayer Lobby")]
    [SerializeField] private GameObject localMultiplayerLobbyPanel;
    [SerializeField] private Transform localMultiplayerPlayersUIsContainer;
    [SerializeField] private Button localMultiplayerStartButton;

    [Header("Network Multiplayer Lobby")]
    [SerializeField] private GameObject networkMultiplayerLobbyPanel;
    [SerializeField] private Transform networkMultiplayerPlayersUIsContainer;
    [SerializeField] private Button networkMultiplayerReadyButton, networkMultiplayerUnreadyButton;
    [SerializeField] private Button networkMultiplayerStartButton;
    [SerializeField] private TMP_Text relayCodeText;
    [SerializeField] private TMP_InputField relayCodeInputField;
    [SerializeField] private Button createNetworkLobbyButton;
    [SerializeField] private Button joinNetworkLobbyButton;
    [SerializeField] private Button leaveNetworkLobbyButton;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Button skipTutorialButton;

    private Dictionary<Player, LobbyPlayerUIHandler> lobbyPlayerUIHandlers = new Dictionary<Player, LobbyPlayerUIHandler>();

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        lastPlayerName = PlayerSettings.name;
        playerNameInputField.text = PlayerSettings.name;

        localMultiplayerStartButton.interactable = false;

        networkMultiplayerReadyButton.gameObject.SetActive(false);
        networkMultiplayerUnreadyButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.gameObject.SetActive(false);

        leaveNetworkLobbyButton.gameObject.SetActive(false);

        createNetworkLobbyButton.gameObject.SetActive(true);
        relayCodeInputField.gameObject.SetActive(true);
        joinNetworkLobbyButton.gameObject.SetActive(true);

        relayCodeText.gameObject.SetActive(false);
        relayCodeText.text = string.Empty;

        networkMultiplayerStartButton.interactable = false;
    }

    protected override void Start()
    {
        if (!PlayerSettings.endedTutorial)
            OpenPanel(tutorialPanel);
        else
            OpenPanel(0);
    }

    public LobbyPlayerUIHandler CreateNewLobbyUI(Transform container, GameObject prefab, string name, Color color, bool ready = false)
    {
        LobbyPlayerUIHandler lobbyPlayerUIHandler = Instantiate(prefab, container).GetComponent<LobbyPlayerUIHandler>();
        lobbyPlayerUIHandler.SetUp(name, ready, color);
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


    public void OpenLocalLobbyPanel()
    {
        OpenPanel(localMultiplayerLobbyPanel);
        LocalMultiplayerLobbyManager.Instance.enabled = true;
    }

    public void OpenNetworkLobbyPanel()
    {
        OpenPanel(networkMultiplayerLobbyPanel);
        LocalMultiplayerLobbyManager.Instance.enabled = false;
    }


    private void OnJoinedNetworkLobby(bool isHost, string lobbyCode)
    {
        networkMultiplayerReadyButton.gameObject.SetActive(true);
        networkMultiplayerUnreadyButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.gameObject.SetActive(isHost);

        leaveNetworkLobbyButton.gameObject.SetActive(true);

        createNetworkLobbyButton.gameObject.SetActive(false);
        relayCodeInputField.gameObject.SetActive(false);
        joinNetworkLobbyButton.gameObject.SetActive(false);

        relayCodeText.gameObject.SetActive(true);
        networkMultiplayerStartButton.gameObject.SetActive(isHost);
        relayCodeText.text = lobbyCode.ToUpper();

        if (curOpenedPanel == GetPanel(networkMultiplayerLobbyPanel))
            SetSelectedObject(networkMultiplayerReadyButton.gameObject);
    }

    private void OnLeftNetworkLobby()
    {
        networkMultiplayerReadyButton.gameObject.SetActive(false);
        networkMultiplayerUnreadyButton.gameObject.SetActive(false);
        networkMultiplayerStartButton.gameObject.SetActive(false);

        leaveNetworkLobbyButton.gameObject.SetActive(false);

        createNetworkLobbyButton.gameObject.SetActive(true);
        relayCodeInputField.gameObject.SetActive(true);
        joinNetworkLobbyButton.gameObject.SetActive(true);

        relayCodeText.gameObject.SetActive(false);
        relayCodeText.text = string.Empty;

        if (curOpenedPanel == GetPanel(networkMultiplayerLobbyPanel))
            SetSelectedObject(createNetworkLobbyButton.gameObject);
    }

    private void OnLocalLobbyPlayerJoined(Player player)
    {
        if (lobbyPlayerUIHandlers.ContainsKey(player)) return;
        lobbyPlayerUIHandlers.Add(player, CreateNewLobbyUI(localMultiplayerPlayersUIsContainer, lobbyPlayerUIPrefab, player.name, player.color));
    }

    private void OnLocalLobbyCanStartChanged(bool canStart)
    {
        localMultiplayerStartButton.interactable = canStart;
        if (!canStart && curOpenedPanel.panel == localMultiplayerLobbyPanel)
            SetSelectedObject(GetActiveSelectable());
    }

    private void OnNetworkLobbyPlayerJoined(Player player)
    {
        if (lobbyPlayerUIHandlers.ContainsKey(player)) return;
        lobbyPlayerUIHandlers.Add(player, CreateNewLobbyUI(networkMultiplayerPlayersUIsContainer, lobbyPlayerUIPrefab, player.name, player.color));
    }

    private void OnNetworkLobbyCanStartChanged(bool canStart)
    {
        GameObject curSelectable = EventSystem.current.currentSelectedGameObject;
        networkMultiplayerStartButton.interactable = canStart;
        if (!canStart && curSelectable == networkMultiplayerStartButton.gameObject)
            SetSelectedObject(GetActiveSelectable());
    }

    private void OnNetworkPlayerReady(Player player)
    {
        OnPlayerReady(player);
        if(player.playerId == NetworkManager.Singleton.LocalClientId)
        {
            networkMultiplayerReadyButton.gameObject.SetActive(false);
            networkMultiplayerUnreadyButton.gameObject.SetActive(true);
            if (curOpenedPanel == GetPanel(networkMultiplayerLobbyPanel))
                SetSelectedObject(networkMultiplayerUnreadyButton.gameObject);
        }
    }

    private void OnNetworkPlayerUnready(Player player)
    {
        OnPlayerUnready(player);
        if (player.playerId == NetworkManager.Singleton.LocalClientId)
        {
            networkMultiplayerReadyButton.gameObject.SetActive(true);
            networkMultiplayerUnreadyButton.gameObject.SetActive(false);
            if (curOpenedPanel == GetPanel(networkMultiplayerLobbyPanel))
                SetSelectedObject(networkMultiplayerReadyButton.gameObject);
        }
    }

    private void OnRelayCodeChanged(string relayCode)
    {
        NetworkMultiplayerLobbyManager.Instance.curRelayCode = relayCode;
    }

    private void OnPlayerReady(Player player)
    {
        if (!lobbyPlayerUIHandlers.ContainsKey(player)) return;
        lobbyPlayerUIHandlers[player].SetReadyState(true);
    }

    private void OnPlayerUnready(Player player)
    {
        if (!lobbyPlayerUIHandlers.ContainsKey(player)) return;
        lobbyPlayerUIHandlers[player].SetReadyState(false);
    }

    private void OnLobbyPlayerLeft(Player player)
    {
        if (!lobbyPlayerUIHandlers.ContainsKey(player)) return;
        Destroy(lobbyPlayerUIHandlers[player].gameObject);
        lobbyPlayerUIHandlers.Remove(player);
    }

    public void StartTutorial()
    {
        Tutorial.StartTutorial();
        PlayerSettings.endedTutorial = true;
    }

    private void OnSkipTutorialButtonClicked()
    {
        OpenPanel(0);
        PlayerSettings.endedTutorial = true;
    }




    private void OnEnable()
    {
        LocalMultiplayerLobbyManager.onPlayerJoined += OnLocalLobbyPlayerJoined;
        LocalMultiplayerLobbyManager.onPlayerLeft += OnLobbyPlayerLeft;

        LocalLobbyPlayer.onPlayerReady += OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady += OnPlayerUnready;

        LocalMultiplayerLobbyManager.onCanStartChanged += OnLocalLobbyCanStartChanged;

        NetworkMultiplayerLobbyManager.onPlayerJoined += OnNetworkLobbyPlayerJoined;
        NetworkMultiplayerLobbyManager.onPlayerLeft += OnLobbyPlayerLeft;

        NetworkMultiplayerLobbyManager.onPlayerReady += OnNetworkPlayerReady;
        NetworkMultiplayerLobbyManager.onPlayerUnready += OnNetworkPlayerUnready;

        NetworkMultiplayerLobbyManager.onJoinedLobby += OnJoinedNetworkLobby;
        NetworkMultiplayerLobbyManager.onLeftLobby += OnLeftNetworkLobby;

        NetworkMultiplayerLobbyManager.onCanStartChanged += OnNetworkLobbyCanStartChanged;

        playerNameInputField?.onSubmit.AddListener(OnPlayerNameEntered);
        playerNameInputField?.onDeselect.AddListener(OnPlayerNameEntered);

        localMultiplayerStartButton?.onClick.AddListener(LocalMultiplayerLobbyManager.Instance.StartGame);
        networkMultiplayerReadyButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.Ready);
        networkMultiplayerUnreadyButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.Unready);
        networkMultiplayerStartButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.StartGame);
        relayCodeInputField?.onValueChanged.AddListener(OnRelayCodeChanged);
        createNetworkLobbyButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.CreateLobby);
        joinNetworkLobbyButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.JoinLobby);
        leaveNetworkLobbyButton?.onClick.AddListener(NetworkMultiplayerLobbyManager.Instance.Leave);
        skipTutorialButton?.onClick.AddListener(OnSkipTutorialButtonClicked);
    }

    private void OnDisable()
    {
        LocalMultiplayerLobbyManager.onPlayerJoined -= OnLocalLobbyPlayerJoined;
        LocalMultiplayerLobbyManager.onPlayerLeft -= OnLobbyPlayerLeft;

        LocalLobbyPlayer.onPlayerReady -= OnPlayerReady;
        LocalLobbyPlayer.onPlayerUneady -= OnPlayerUnready;

        LocalMultiplayerLobbyManager.onCanStartChanged -= OnLocalLobbyCanStartChanged;

        NetworkMultiplayerLobbyManager.onPlayerJoined -= OnNetworkLobbyPlayerJoined;
        NetworkMultiplayerLobbyManager.onPlayerLeft -= OnLobbyPlayerLeft;

        NetworkMultiplayerLobbyManager.onPlayerReady -= OnNetworkPlayerReady;
        NetworkMultiplayerLobbyManager.onPlayerUnready -= OnNetworkPlayerUnready;

        NetworkMultiplayerLobbyManager.onJoinedLobby -= OnJoinedNetworkLobby;
        NetworkMultiplayerLobbyManager.onLeftLobby -= OnLeftNetworkLobby;

        NetworkMultiplayerLobbyManager.onCanStartChanged -= OnNetworkLobbyCanStartChanged;

        playerNameInputField?.onSubmit.RemoveListener(OnPlayerNameEntered);
        playerNameInputField?.onDeselect.RemoveListener(OnPlayerNameEntered);

        localMultiplayerStartButton?.onClick.RemoveListener(LocalMultiplayerLobbyManager.Instance.StartGame);
        networkMultiplayerReadyButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.Ready);
        networkMultiplayerUnreadyButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.Unready);
        networkMultiplayerStartButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.StartGame);
        relayCodeInputField?.onValueChanged.RemoveListener(OnRelayCodeChanged);
        createNetworkLobbyButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.CreateLobby);
        joinNetworkLobbyButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.JoinLobby);
        leaveNetworkLobbyButton?.onClick.RemoveListener(NetworkMultiplayerLobbyManager.Instance.Leave);
        skipTutorialButton?.onClick.RemoveListener(OnSkipTutorialButtonClicked);
    }
}
