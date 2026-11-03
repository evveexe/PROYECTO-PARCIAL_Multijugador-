using UnityEngine;
using UnityEngine.InputSystem;

public class startScreenController : MonoBehaviour
{
    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject serversPanel;

    private bool isConnecting = false;

    void Start()
    {
        showStartScreen();
    }

    void Update()
    {
        checkStartInput();
    }

    private void showStartScreen()
    {
        startPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(false);
    }

    private void checkStartInput()
    {
        if (startPanel.activeSelf == false)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (checkAnyPress())
        {
            isConnecting = true;
            startPanel.SetActive(false);
            photonManager.startConnection();
        }
    }

    private bool checkAnyPress()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }
        return false;
    }

    public void showMainMenu()
    {
        startPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        serversPanel.SetActive(false);
    }
}