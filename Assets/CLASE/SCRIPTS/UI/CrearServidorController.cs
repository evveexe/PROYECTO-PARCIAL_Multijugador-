using UnityEngine;
using TMPro;

public class CrearServidorController : MonoBehaviour
{
    [SerializeField] private GameObject serversPanel;
    [SerializeField] private GameObject createPanel;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField playerCountInput;
    [SerializeField] private UnityEngine.UI.Toggle privateToggle;

    void Start()
    {
        createPanel.SetActive(false);
    }

    public void showCreatePanel()
    {
        serversPanel.SetActive(false);
        createPanel.SetActive(true);
    }

    public void showServersPanel()
    {
        createPanel.SetActive(false);
        serversPanel.SetActive(true);
    }

    public void createMatch()
    {
        string roomName = roomNameInput.text;
        int playerCount = checkPlayerCount();
        bool isPrivate = privateToggle.isOn;
        PhotonManager.Instance.createCustomSession(roomName, playerCount, isPrivate);
    }

    private int checkPlayerCount()
    {
        int playerCount = 2;
        int.TryParse(playerCountInput.text, out playerCount);
        if (playerCount < 2)
        {
            playerCount = 2;
        }
        if (playerCount > 10)
        {
            playerCount = 10;
        }
        return playerCount;
    }
}