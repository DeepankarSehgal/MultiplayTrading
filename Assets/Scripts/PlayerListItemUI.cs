using TMPro;
using UnityEngine;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text phoneText;
    [SerializeField] private TMP_Text hostBadge;

    public void SetInfo(string username, string email, string phone, bool isHost)
    {
        nameText.text = string.IsNullOrEmpty(username) ? "Connecting..." : username;
        emailText.text = email;
        phoneText.text = phone;
        
        if (hostBadge != null)
        {
            hostBadge.gameObject.SetActive(isHost);
        }
    }
}
