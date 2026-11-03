using TMPro;
using UnityEngine;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private TMP_Text playersText;

    private string sessionName;

    public string getSessionName()
    {
        return sessionName;
    }

    public void SetInfo(ServerDataStruct sessionData)
    {
        sessionName = sessionData.serverName;
        sessionNameText.text = sessionData.serverName;
        playersText.text = sessionData.currentPlayers + "/" + sessionData.maxPlayers;
    }

    public void joinThisSession()
    {
        PhotonManager.Instance.joinSessionByName(sessionName);
    }
}
