using Fusion;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnPlayerInfoChanged))]
    public NetworkString<_32> Username { get; set; }

    [Networked, OnChangedRender(nameof(OnPlayerInfoChanged))]
    public NetworkString<_64> Email { get; set; }

    [Networked, OnChangedRender(nameof(OnPlayerInfoChanged))]
    public NetworkString<_32> PhoneNumber { get; set; }

    public static LobbyPlayer Local { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            // Send local player info to the state authority (Host/Server) via RPC
            RPC_SetPlayerInfo(PlayerData.Name, PlayerData.Email, PlayerData.PhoneNumber);
        }

        // Register this player with our singleton manager
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RegisterPlayer(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.UnregisterPlayer(this);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerInfo(string name, string email, string phone)
    {
        Username = name;
        Email = email;
        PhoneNumber = phone;

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SavePlayerInfo(Object.InputAuthority, name, email, phone);
        }
    }

    private void OnPlayerInfoChanged()
    {
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.UpdatePlayerList();
        }
    }
}
