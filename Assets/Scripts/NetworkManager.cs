using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef playerLobbyPrefab;
    [SerializeField] private NetworkPrefabRef playerCharacterPrefab;

    [Header("Settings")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private int maxPlayers = 5;

    private NetworkRunner _runner;
    private List<LobbyPlayer> _players = new List<LobbyPlayer>();
    private Dictionary<PlayerRef, (string Name, string Email, string Phone)> _playerInfoMap = 
        new Dictionary<PlayerRef, (string, string, string)>();

    public List<LobbyPlayer> Players => _players;
    public NetworkRunner Runner => _runner;
    public string CurrentRoomCode { get; private set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerInfo(PlayerRef player, string name, string email, string phone)
    {
        _playerInfoMap[player] = (name, email, phone);
        Debug.Log($"[Server] Saved player info for {player}: {name}");
    }

    public (string Name, string Email, string Phone) GetSavedPlayerInfo(PlayerRef player)
    {
        if (_playerInfoMap.TryGetValue(player, out var info))
        {
            return info;
        }
        return ("Unknown", "", "");
    }

    public async Task<StartGameResult> StartGame(GameMode mode, string roomCode)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        // Add callbacks
        _runner.AddCallbacks(this);

        // Configure scene manager
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        CurrentRoomCode = roomCode;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = roomCode,
            PlayerCount = maxPlayers,
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(0) // Start from current lobby scene index
        });

        if (result.Ok)
        {
            Debug.Log($"Connected successfully to session: {roomCode} in mode: {mode}");
        }
        else
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            CurrentRoomCode = "";
        }

        return result;
    }

    public void Disconnect()
    {
        if (_runner != null)
        {
            _runner.Shutdown();
        }
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
            Debug.Log($"Registered LobbyPlayer. Total: {_players.Count}/{maxPlayers}");

            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdatePlayerList();
            }

            // Check if lobby is full (5 players) and trigger transition
            CheckPlayerCountAndStart();
        }
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        if (_players.Contains(player))
        {
            _players.Remove(player);
            Debug.Log($"Unregistered LobbyPlayer. Total: {_players.Count}/{maxPlayers}");

            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdatePlayerList();
            }
        }
    }

    private void CheckPlayerCountAndStart()
    {
        // Only the Host (Server) has authority to load scenes in Host/Client mode
        if (_runner != null && _runner.IsServer)
        {
            if (_players.Count == maxPlayers)
            {
                Debug.Log("Lobby is full! Loading the game scene...");
                _runner.LoadScene(gameSceneName, LoadSceneMode.Single, LocalPhysicsMode.None, true);
            }
        }
    }

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"Player joined: {player}. Spawning LobbyPlayer prefab.");
            // Spawn lobby syncing entity for this player and give them input authority
            runner.Spawn(playerLobbyPrefab, Vector3.zero, Quaternion.identity, inputAuthority: player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"Player left: {player}");
            _playerInfoMap.Remove(player);

            // Clean up player characters in game scene if any exist
            foreach (var character in FindObjectsOfType<GamePlayerCharacter>())
            {
                if (character.Object.InputAuthority == player)
                {
                    runner.Despawn(character.Object);
                }
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var inputData = new NetworkInputData();
        Vector2 move = Vector2.zero;

        // Read input using Keyboard from the new Input System
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            float moveX = 0f;
            float moveY = 0f;

            if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed || UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed || UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
            if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

            move = new Vector2(moveX, moveY).normalized;
        }

        inputData.movement = move;
        input.Set(inputData);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.LogWarning($"Fusion Runner Shutdown: {shutdownReason}");
        _players.Clear();
        _playerInfoMap.Clear();
        CurrentRoomCode = "";

        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ResetToUserInfoScreen();
        }
        else
        {
            // If we are in GameScene, return to LobbyScene
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // When scene finishes loading, check if we entered the game scene
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            if (runner.IsServer)
            {
                Debug.Log("GameScene loaded on server. Spawning player characters...");
                
                // Spawn a gameplay character for every connected player
                foreach (var player in runner.ActivePlayers)
                {
                    // Generate a random spawn position around the center
                    Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0.5f, UnityEngine.Random.Range(-3f, 3f));
                    
                    var charObj = runner.Spawn(playerCharacterPrefab, spawnPos, Quaternion.identity, inputAuthority: player);
                    
                    // Copy player info to the character network entity
                    var character = charObj.GetComponent<GamePlayerCharacter>();
                    if (character != null)
                    {
                        var info = GetSavedPlayerInfo(player);
                        character.Initialize(info.Name, info.Email, info.Phone);
                    }
                }
            }
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}
