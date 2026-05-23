using Fusion;
using TMPro;
using UnityEngine;

public class GamePlayerCharacter : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    [Header("UI Above Head")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Canvas nameTextCanvas;

    [Networked, OnChangedRender(nameof(OnPlayerInfoChanged))]
    public NetworkString<_32> Username { get; set; }

    [Networked]
    public NetworkString<_64> Email { get; set; }

    [Networked]
    public NetworkString<_32> PhoneNumber { get; set; }

    public void Initialize(string name, string email, string phone)
    {
        // Only state authority (Server/Host) can assign networked properties
        if (Object.HasStateAuthority)
        {
            Username = name;
            Email = email;
            PhoneNumber = phone;
            UpdateNameplate();
        }
    }

    public override void Spawned()
    {
        UpdateNameplate();
    }

    public override void FixedUpdateNetwork()
    {
        // Retrieve client inputs sent over the network
        if (GetInput(out NetworkInputData inputData))
        {
            Vector3 moveDir = new Vector3(inputData.movement.x, 0, inputData.movement.y);
            transform.Translate(moveDir * speed * Runner.DeltaTime, Space.World);
        }
    }

    private void LateUpdate()
    {
        // Billboarding effect: keep nameplate canvas facing the main camera
        if (nameTextCanvas != null && Camera.main != null)
        {
            nameTextCanvas.transform.LookAt(
                nameTextCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward, 
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    private void OnPlayerInfoChanged()
    {
        UpdateNameplate();
    }

    private void UpdateNameplate()
    {
        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(Username.ToString()) ? "Loading..." : Username.ToString();
        }
    }
}
