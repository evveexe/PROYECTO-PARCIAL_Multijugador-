using UnityEngine;

public class ServersMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject serversPanel;

    void Start()
    {
        serversPanel.SetActive(false);
    }

    public void showServers()
    {
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(true);
    }

    public void showMainMenu()
    {
        serversPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
