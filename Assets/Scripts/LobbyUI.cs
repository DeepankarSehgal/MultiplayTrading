using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject userInfoPanel;
    [SerializeField] private GameObject roomSelectionPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Screen 1 - User Info Inputs")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField phoneInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text userInfoErrorText;

    [Header("Screen 2 - Room Selection")]
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button roomSelectBackButton;
    [SerializeField] private TMP_Text roomSelectErrorText;

    [Header("Screen 3 - Lobby Waiting")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text lobbyStatusText;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Button leaveLobbyButton;

    [Header("Loading Status")]
    [SerializeField] private TMP_Text loadingStatusText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Show only user info panel on start
        ShowPanel(userInfoPanel);

        // Clear errors
        if (userInfoErrorText != null) userInfoErrorText.text = "";
        if (roomSelectErrorText != null) roomSelectErrorText.text = "";

        // Add listeners
        nextButton.onClick.AddListener(OnNextButtonClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        roomSelectBackButton.onClick.AddListener(OnRoomSelectBackClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        userInfoPanel.SetActive(panelToShow == userInfoPanel);
        roomSelectionPanel.SetActive(panelToShow == roomSelectionPanel);
        lobbyPanel.SetActive(panelToShow == lobbyPanel);
        loadingPanel.SetActive(panelToShow == loadingPanel);
    }

    public void ResetToUserInfoScreen()
    {
        ShowPanel(userInfoPanel);
    }

    #region Screen 1 - User Info
    private void OnNextButtonClicked()
    {
        string username = nameInputField.text.Trim();
        string email = emailInputField.text.Trim();
        string phone = phoneInputField.text.Trim();

        if (ValidateUserInfo(username, email, phone, out string errorMessage))
        {
            // Save data locally
            PlayerData.Name = username;
            PlayerData.Email = email;
            PlayerData.PhoneNumber = phone;

            // Advance to screen 2
            if (userInfoErrorText != null) userInfoErrorText.text = "";
            ShowPanel(roomSelectionPanel);
        }
        else
        {
            if (userInfoErrorText != null)
            {
                userInfoErrorText.text = errorMessage;
                userInfoErrorText.color = Color.red;
            }
        }
    }

    private bool ValidateUserInfo(string username, string email, string phone, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(username))
        {
            error = "Name cannot be empty.";
            return false;
        }
        if (username.Length < 2)
        {
            error = "Name must be at least 2 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            error = "Phone number cannot be empty.";
            return false;
        }
        if (!Regex.IsMatch(phone, @"^[0-9+\-\s()]+$") || phone.Length < 7)
        {
            error = "Invalid phone number format. Must be numeric and at least 7 digits.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            error = "Email cannot be empty.";
            return false;
        }
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            error = "Invalid email format. E.g., user@example.com";
            return false;
        }

        return true;
    }
    #endregion

    #region Screen 2 - Room Selection
    private async void OnCreateRoomClicked()
    {
        string roomCode = GenerateRandomRoomCode();
        ShowLoading($"Creating Room: {roomCode}...");
        
        var result = await NetworkManager.Instance.StartGame(GameMode.Host, roomCode);
        
        if (result.Ok)
        {
            roomCodeText.text = $"ROOM CODE: {roomCode}";
            ShowPanel(lobbyPanel);
            UpdatePlayerList();
        }
        else
        {
            ShowPanel(roomSelectionPanel);
            if (roomSelectErrorText != null)
            {
                roomSelectErrorText.text = $"Failed to create room: {result.ShutdownReason}";
                roomSelectErrorText.color = Color.red;
            }
        }
    }

    private async void OnJoinRoomClicked()
    {
        string roomCode = roomCodeInputField.text.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(roomCode) || roomCode.Length < 3)
        {
            if (roomSelectErrorText != null)
            {
                roomSelectErrorText.text = "Please enter a valid Room Code.";
                roomSelectErrorText.color = Color.red;
            }
            return;
        }

        ShowLoading($"Joining Room: {roomCode}...");

        var result = await NetworkManager.Instance.StartGame(GameMode.Client, roomCode);

        if (result.Ok)
        {
            roomCodeText.text = $"ROOM CODE: {roomCode}";
            ShowPanel(lobbyPanel);
            UpdatePlayerList();
        }
        else
        {
            ShowPanel(roomSelectionPanel);
            if (roomSelectErrorText != null)
            {
                roomSelectErrorText.text = $"Failed to join room: {result.ShutdownReason}";
                roomSelectErrorText.color = Color.red;
            }
        }
    }

    private void OnRoomSelectBackClicked()
    {
        ShowPanel(userInfoPanel);
    }

    private string GenerateRandomRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new System.Random();
        char[] code = new char[5];
        for (int i = 0; i < 5; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }
    #endregion

    #region Screen 3 - Lobby Waiting
    private void OnLeaveLobbyClicked()
    {
        ShowLoading("Leaving lobby...");
        NetworkManager.Instance.Disconnect();
    }

    public void UpdatePlayerList()
    {
        if (NetworkManager.Instance == null) return;

        // Clear existing player items
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        var players = NetworkManager.Instance.Players;
        lobbyStatusText.text = $"Waiting for players ({players.Count}/5)...";

        foreach (var player in players)
        {
            var itemObj = Instantiate(playerListItemPrefab, playerListContainer);
            var itemUI = itemObj.GetComponent<PlayerListItemUI>();
            if (itemUI != null)
            {
                // In Host/Client mode, the PlayerRef with ID 1 (or the Host runner itself) is the Host.
                // We check if this LobbyPlayer has StateAuthority, which belongs to the server/host.
                bool isHost = player.Object.HasStateAuthority;
                itemUI.SetInfo(
                    player.Username.ToString(), 
                    player.Email.ToString(), 
                    player.PhoneNumber.ToString(), 
                    isHost
                );
            }
        }
    }
    #endregion

    #region Loading Indicator
    private void ShowLoading(string message)
    {
        if (loadingStatusText != null)
        {
            loadingStatusText.text = message;
        }
        ShowPanel(loadingPanel);
    }
    #endregion
}
