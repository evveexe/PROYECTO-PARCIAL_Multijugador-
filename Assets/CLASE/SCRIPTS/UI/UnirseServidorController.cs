using UnityEngine;
using TMPro;

public class UnirseServidorController : MonoBehaviour
{
    [SerializeField] private GameObject serversPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private TMP_InputField joinNameInput;
    [SerializeField] private TMP_Text errorText;

    void Start()
    {
        joinPanel.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    public void showJoinPanel()
    {
        serversPanel.SetActive(false);
        joinPanel.SetActive(true);
        errorText.gameObject.SetActive(false);
    }

    public void showServersPanel()
    {
        joinPanel.SetActive(false);
        serversPanel.SetActive(true);
    }

    public void joinMatch()
    {
        string joinName = joinNameInput.text;

        if (string.IsNullOrEmpty(joinName))
        {
            errorText.gameObject.SetActive(true);
            return;
        }

        errorText.gameObject.SetActive(false);
        PhotonManager.Instance.joinSessionByName(joinName);
    }
}