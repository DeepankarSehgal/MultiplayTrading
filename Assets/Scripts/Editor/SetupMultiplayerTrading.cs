using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class SetupMultiplayerTrading : EditorWindow
{
    [MenuItem("Tools/Setup Multiplayer Trading Project")]
    public static void ExecuteSetup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Please exit Play Mode before running the setup.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Setup Multiplayer Project",
            "This will create LobbyScene, GameScene, necessary prefabs, and configure your Build Settings automatically. Are you sure you want to continue?",
            "Yes, Setup", "Cancel"
        );

        if (!confirm) return;

        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        // 1. Create Prefabs First
        GameObject lobbyPlayerPrefab = CreateLobbyPlayerPrefab();
        GameObject playerListItemPrefab = CreatePlayerListItemPrefab();
        GameObject playerCharacterPrefab = CreatePlayerCharacterPrefab();

        // 2. Setup Lobby Scene
        string lobbyScenePath = "Assets/Scenes/LobbyScene.unity";
        var lobbyScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        SetupLobbySceneObjects(lobbyPlayerPrefab, playerListItemPrefab, playerCharacterPrefab);
        
        EditorSceneManager.SaveScene(lobbyScene, lobbyScenePath);
        Debug.Log("Created and saved LobbyScene.");

        // 3. Setup Game Scene
        string gameScenePath = "Assets/Scenes/GameScene.unity";
        var gameScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        SetupGameSceneObjects();
        
        EditorSceneManager.SaveScene(gameScene, gameScenePath);
        Debug.Log("Created and saved GameScene.");

        // 4. Update Build Settings
        var buildScenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(lobbyScenePath, true),
            new EditorBuildSettingsScene(gameScenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("Configured Build Settings with LobbyScene and GameScene.");

        // Reload the lobby scene for the user
        EditorSceneManager.OpenScene(lobbyScenePath);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Multiplayer project setup completed successfully!\n\nCheck 'Assets/Scenes/LobbyScene' and enjoy testing!", "Awesome");
    }

    private static GameObject CreateLobbyPlayerPrefab()
    {
        string path = "Assets/Prefabs/LobbyPlayer.prefab";
        var obj = new GameObject("LobbyPlayer", typeof(NetworkObject), typeof(LobbyPlayer));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        GameObject.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreatePlayerListItemPrefab()
    {
        string path = "Assets/Prefabs/PlayerListItem.prefab";
        var listItemObj = new GameObject("PlayerListItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(PlayerListItemUI));
        var listItemRect = listItemObj.GetComponent<RectTransform>();
        listItemRect.sizeDelta = new Vector2(480, 60);

        var listItemImage = listItemObj.GetComponent<Image>();
        listItemImage.color = new Color(0.15f, 0.18f, 0.25f, 0.6f); // Soft dark glass look

        // Name Text (Left)
        var nameObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(listItemObj.transform, false);
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0, 0.5f);
        nameRect.pivot = new Vector2(0, 0.5f);
        nameRect.anchoredPosition = new Vector2(15, 0);
        nameRect.sizeDelta = new Vector2(200, 40);

        var nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 18;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;

        // Email Text (Top Right)
        var emailObj = new GameObject("EmailText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        emailObj.transform.SetParent(listItemObj.transform, false);
        var emailRect = emailObj.GetComponent<RectTransform>();
        emailRect.anchorMin = new Vector2(1, 0.5f);
        emailRect.anchorMax = new Vector2(1, 0.5f);
        emailRect.pivot = new Vector2(1, 0.5f);
        emailRect.anchoredPosition = new Vector2(-15, 10);
        emailRect.sizeDelta = new Vector2(250, 20);

        var emailText = emailObj.GetComponent<TextMeshProUGUI>();
        emailText.fontSize = 11;
        emailText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        emailText.alignment = TextAlignmentOptions.Right;

        // Phone Text (Bottom Right)
        var phoneObj = new GameObject("PhoneText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        phoneObj.transform.SetParent(listItemObj.transform, false);
        var phoneRect = phoneObj.GetComponent<RectTransform>();
        phoneRect.anchorMin = new Vector2(1, 0.5f);
        phoneRect.anchorMax = new Vector2(1, 0.5f);
        phoneRect.pivot = new Vector2(1, 0.5f);
        phoneRect.anchoredPosition = new Vector2(-15, -10);
        phoneRect.sizeDelta = new Vector2(250, 20);

        var phoneText = phoneObj.GetComponent<TextMeshProUGUI>();
        phoneText.fontSize = 11;
        phoneText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        phoneText.alignment = TextAlignmentOptions.Right;

        // Host Badge (Middle-Right)
        var badgeObj = new GameObject("HostBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        badgeObj.transform.SetParent(listItemObj.transform, false);
        var badgeRect = badgeObj.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(20, 0);
        badgeRect.sizeDelta = new Vector2(80, 25);

        var badgeText = badgeObj.GetComponent<TextMeshProUGUI>();
        badgeText.text = "HOST";
        badgeText.fontSize = 12;
        badgeText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
        badgeText.alignment = TextAlignmentOptions.Center;

        // Bind references
        var uiComp = listItemObj.GetComponent<PlayerListItemUI>();
        var so = new SerializedObject(uiComp);
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("emailText").objectReferenceValue = emailText;
        so.FindProperty("phoneText").objectReferenceValue = phoneText;
        so.FindProperty("hostBadge").objectReferenceValue = badgeText;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(listItemObj, path);
        GameObject.DestroyImmediate(listItemObj);
        return prefab;
    }

    private static GameObject CreatePlayerCharacterPrefab()
    {
        string path = "Assets/Prefabs/PlayerCharacter.prefab";
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "PlayerCharacter";
        obj.AddComponent<NetworkObject>();
        
        var charComp = obj.AddComponent<GamePlayerCharacter>();

        // Add visual color modifier
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = new Color(0.2f, 0.6f, 1f); // Sleek cyan
        }

        // Add name canvas above character's head
        var nameCanvasObj = new GameObject("NameCanvas", typeof(RectTransform), typeof(Canvas));
        nameCanvasObj.transform.SetParent(obj.transform, false);
        nameCanvasObj.transform.localPosition = new Vector3(0, 1.3f, 0);
        nameCanvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        var nameCanvas = nameCanvasObj.GetComponent<Canvas>();
        nameCanvas.renderMode = RenderMode.WorldSpace;

        var nameTextObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameTextObj.transform.SetParent(nameCanvasObj.transform, false);
        var nameTextRect = nameTextObj.GetComponent<RectTransform>();
        nameTextRect.sizeDelta = new Vector2(200, 50);
        nameTextRect.anchoredPosition = Vector2.zero;

        var nameText = nameTextObj.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 20;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.text = "Initializing...";

        // Bind character reference properties
        var so = new SerializedObject(charComp);
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("nameTextCanvas").objectReferenceValue = nameCanvas;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        GameObject.DestroyImmediate(obj);
        return prefab;
    }

    private static void SetupLobbySceneObjects(GameObject lobbyPlayer, GameObject listItem, GameObject playerChar)
    {
        // 1. Create NetworkManager
        var netMgrObj = new GameObject("NetworkManager", typeof(NetworkManager), typeof(NetworkSceneManagerDefault));
        var netMgr = netMgrObj.GetComponent<NetworkManager>();

        var soNet = new SerializedObject(netMgr);
        soNet.FindProperty("playerLobbyPrefab").objectReferenceValue = lobbyPlayer;
        soNet.FindProperty("playerCharacterPrefab").objectReferenceValue = playerChar;
        soNet.ApplyModifiedProperties();

        // 2. Setup Canvas
        var canvasObj = new GameObject("LobbyCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // LobbyManager UI Component
        var lobbyUiObj = new GameObject("LobbyUI", typeof(LobbyUI));
        var lobbyUI = lobbyUiObj.GetComponent<LobbyUI>();

        // Background Panel (FullScreen blur style)
        var bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f); // Slate dark background

        // Container Panel (Center Card)
        var cardObj = new GameObject("ContainerCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObj.transform.SetParent(canvasObj.transform, false);
        var cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(600, 520);
        cardRect.anchoredPosition = Vector2.zero;
        cardObj.GetComponent<Image>().color = new Color(0.13f, 0.15f, 0.20f, 0.95f); // Solid card container

        // --- Screen 1: User Info Panel ---
        var userInfoPanel = new GameObject("Panel_UserInfo", typeof(RectTransform));
        userInfoPanel.transform.SetParent(cardObj.transform, false);
        var uiPanelRect = userInfoPanel.GetComponent<RectTransform>();
        uiPanelRect.anchorMin = Vector2.zero;
        uiPanelRect.anchorMax = Vector2.one;
        uiPanelRect.sizeDelta = Vector2.zero;

        CreateText("Title", "PLAYER DETAILS", userInfoPanel.transform, new Vector2(0, 190), new Vector2(500, 50), 28, Color.white);
        
        var nameInput = CreateInputField("InputField_Name", "Enter Name...", userInfoPanel.transform, new Vector2(0, 90), new Vector2(400, 55));
        var phoneInput = CreateInputField("InputField_Phone", "Enter Phone...", userInfoPanel.transform, new Vector2(0, 20), new Vector2(400, 55));
        var emailInput = CreateInputField("InputField_Email", "Enter Email...", userInfoPanel.transform, new Vector2(0, -50), new Vector2(400, 55));
        
        var nextBtn = CreateButton("Button_Next", "Next Step", userInfoPanel.transform, new Vector2(0, -140), new Vector2(400, 55));
        var userInfoError = CreateText("ErrorText", "", userInfoPanel.transform, new Vector2(0, -210), new Vector2(500, 40), 14, Color.red);

        // --- Screen 2: Room Selection Panel ---
        var roomSelectionPanel = new GameObject("Panel_RoomSelection", typeof(RectTransform));
        roomSelectionPanel.transform.SetParent(cardObj.transform, false);
        var roomSelRect = roomSelectionPanel.GetComponent<RectTransform>();
        roomSelRect.anchorMin = Vector2.zero;
        roomSelRect.anchorMax = Vector2.one;
        roomSelRect.sizeDelta = Vector2.zero;
        roomSelectionPanel.SetActive(false);

        CreateText("Title", "MULTIPLAYER ROOMS", roomSelectionPanel.transform, new Vector2(0, 190), new Vector2(500, 50), 28, Color.white);

        var createBtn = CreateButton("Button_CreateRoom", "Create New Room", roomSelectionPanel.transform, new Vector2(0, 80), new Vector2(400, 60));
        createBtn.GetComponent<Image>().color = new Color(0.18f, 0.65f, 0.43f, 1f); // Nice Green

        CreateText("Separator", "— OR JOIN EXISTING —", roomSelectionPanel.transform, new Vector2(0, 10), new Vector2(400, 30), 14, new Color(0.6f, 0.6f, 0.6f));

        var roomCodeInput = CreateInputField("InputField_RoomCode", "Enter 5-Letter Room Code...", roomSelectionPanel.transform, new Vector2(0, -50), new Vector2(400, 55));
        
        var joinBtn = CreateButton("Button_JoinRoom", "Join Room with Code", roomSelectionPanel.transform, new Vector2(0, -120), new Vector2(400, 55));
        var backBtn = CreateButton("Button_Back", "Back", roomSelectionPanel.transform, new Vector2(0, -210), new Vector2(150, 40));
        backBtn.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        var roomSelectError = CreateText("ErrorText", "", roomSelectionPanel.transform, new Vector2(0, -170), new Vector2(500, 30), 14, Color.red);

        // --- Screen 3: Lobby Waiting Panel ---
        var lobbyPanel = new GameObject("Panel_Lobby", typeof(RectTransform));
        lobbyPanel.transform.SetParent(cardObj.transform, false);
        var lobbyRect = lobbyPanel.GetComponent<RectTransform>();
        lobbyRect.anchorMin = Vector2.zero;
        lobbyRect.anchorMax = Vector2.one;
        lobbyRect.sizeDelta = Vector2.zero;
        lobbyPanel.SetActive(false);

        var lobbyTitleText = CreateText("RoomCodeText", "ROOM CODE: -----", lobbyPanel.transform, new Vector2(0, 200), new Vector2(500, 40), 24, Color.white);
        var lobbyStatusText = CreateText("StatusText", "Waiting for players (0/5)...", lobbyPanel.transform, new Vector2(0, 160), new Vector2(500, 35), 16, new Color(0.8f, 0.8f, 0.8f));

        // Player List Container inside Card
        var listContainerObj = new GameObject("PlayerListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        listContainerObj.transform.SetParent(lobbyPanel.transform, false);
        var listContainerRect = listContainerObj.GetComponent<RectTransform>();
        listContainerRect.anchoredPosition = new Vector2(0, -10);
        listContainerRect.sizeDelta = new Vector2(500, 260);

        var layoutGroup = listContainerObj.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 10f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        var sizeFitter = listContainerObj.GetComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var leaveBtn = CreateButton("Button_LeaveLobby", "Leave Lobby", lobbyPanel.transform, new Vector2(0, -200), new Vector2(300, 50));
        leaveBtn.GetComponent<Image>().color = new Color(0.75f, 0.22f, 0.25f, 1f); // Dark red

        // --- Loading Panel ---
        var loadingPanel = new GameObject("Panel_Loading", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        loadingPanel.transform.SetParent(cardObj.transform, false);
        var loadingRect = loadingPanel.GetComponent<RectTransform>();
        loadingRect.anchorMin = Vector2.zero;
        loadingRect.anchorMax = Vector2.one;
        loadingRect.sizeDelta = Vector2.zero;
        loadingPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        loadingPanel.SetActive(false);

        var loadText = CreateText("LoadingText", "Connecting...", loadingPanel.transform, new Vector2(0, 0), new Vector2(500, 50), 22, Color.white);

        // --- Bind references to LobbyUI ---
        var soUI = new SerializedObject(lobbyUI);
        soUI.FindProperty("userInfoPanel").objectReferenceValue = userInfoPanel;
        soUI.FindProperty("roomSelectionPanel").objectReferenceValue = roomSelectionPanel;
        soUI.FindProperty("lobbyPanel").objectReferenceValue = lobbyPanel;
        soUI.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;

        soUI.FindProperty("nameInputField").objectReferenceValue = nameInput;
        soUI.FindProperty("phoneInputField").objectReferenceValue = phoneInput;
        soUI.FindProperty("emailInputField").objectReferenceValue = emailInput;
        soUI.FindProperty("nextButton").objectReferenceValue = nextBtn;
        soUI.FindProperty("userInfoErrorText").objectReferenceValue = userInfoError;

        soUI.FindProperty("roomCodeInputField").objectReferenceValue = roomCodeInput;
        soUI.FindProperty("createRoomButton").objectReferenceValue = createBtn;
        soUI.FindProperty("joinRoomButton").objectReferenceValue = joinBtn;
        soUI.FindProperty("roomSelectBackButton").objectReferenceValue = backBtn;
        soUI.FindProperty("roomSelectErrorText").objectReferenceValue = roomSelectError;

        soUI.FindProperty("roomCodeText").objectReferenceValue = lobbyTitleText;
        soUI.FindProperty("lobbyStatusText").objectReferenceValue = lobbyStatusText;
        soUI.FindProperty("playerListContainer").objectReferenceValue = listContainerObj.transform;
        soUI.FindProperty("playerListItemPrefab").objectReferenceValue = listItem;
        soUI.FindProperty("leaveLobbyButton").objectReferenceValue = leaveBtn;

        soUI.FindProperty("loadingStatusText").objectReferenceValue = loadText;
        soUI.ApplyModifiedProperties();
    }

    private static void SetupGameSceneObjects()
    {
        // Add Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "GroundPlane";
        ground.transform.localScale = new Vector3(3, 1, 3);
        ground.transform.position = Vector3.zero;

        var renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = new Color(0.18f, 0.22f, 0.28f); // Soft dark grey ground
        }

        // Setup Main Camera View
        var mainCam = GameObject.FindWithTag("MainCamera");
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 10, -12);
            mainCam.transform.rotation = Quaternion.Euler(40, 0, 0);
        }

        // Setup Scene Info Text (Telling player they are in game scene)
        var canvasObj = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var title = CreateText("Title", "GAME SCENE LOADED", canvasObj.transform, new Vector2(0, 480), new Vector2(800, 60), 24, Color.green);
        var instructions = CreateText("Instructions", "Use WASD or Arrow Keys to move your avatar.\nAll players are synced in real-time.", canvasObj.transform, new Vector2(0, 420), new Vector2(800, 80), 16, new Color(0.8f, 0.8f, 0.8f));
    }

    private static TMP_InputField CreateInputField(string name, string placeholderText, Transform parent, Vector2 pos, Vector2 size)
    {
        var inputFieldObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        inputFieldObj.transform.SetParent(parent, false);

        var rect = inputFieldObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var image = inputFieldObj.GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0.05f); // Transparent dark background

        // TextArea
        var textAreaObj = new GameObject("Text Area", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectMask2D));
        textAreaObj.transform.SetParent(inputFieldObj.transform, false);
        var textAreaRect = textAreaObj.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = new Vector2(-20, -10); // Padding

        // Placeholder Text
        var placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        placeholderObj.transform.SetParent(textAreaObj.transform, false);
        var placeholderTextComp = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeholderTextComp.text = placeholderText;
        placeholderTextComp.fontSize = 16;
        placeholderTextComp.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);
        placeholderTextComp.alignment = TextAlignmentOptions.Left;
        placeholderTextComp.fontStyle = FontStyles.Italic;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;

        // Input Text
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(textAreaObj.transform, false);
        var textComp = textObj.GetComponent<TextMeshProUGUI>();
        textComp.fontSize = 16;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.Left;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Bind
        var inputField = inputFieldObj.GetComponent<TMP_InputField>();
        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComp;
        inputField.placeholder = placeholderTextComp;

        return inputField;
    }

    private static Button CreateButton(string name, string labelText, Transform parent, Vector2 pos, Vector2 size)
    {
        var buttonObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(parent, false);

        var rect = buttonObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var image = buttonObj.GetComponent<Image>();
        image.color = new Color(0.18f, 0.48f, 0.85f, 1f); // Premium blue

        // Text label
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(buttonObj.transform, false);
        var textComp = textObj.GetComponent<TextMeshProUGUI>();
        textComp.text = labelText;
        textComp.fontSize = 18;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.Center;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        var button = buttonObj.GetComponent<Button>();
        return button;
    }

    private static TextMeshProUGUI CreateText(string name, string textContent, Transform parent, Vector2 pos, Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent, false);

        var rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var textComp = textObj.GetComponent<TextMeshProUGUI>();
        textComp.text = textContent;
        textComp.fontSize = fontSize;
        textComp.color = color;
        textComp.alignment = alignment;

        return textComp;
    }
}
