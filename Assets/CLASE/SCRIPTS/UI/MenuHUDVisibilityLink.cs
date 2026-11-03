using UnityEngine;

public class MenuHUDVisibilityLink : MonoBehaviour
{
    [SerializeField] private GameObject hud;

    private void OnEnable()
    {
        if (hud != null && hud.activeSelf) hud.SetActive(false);
    }

    private void OnDisable()
    {
        if (hud != null && !hud.activeSelf) hud.SetActive(true);
    }
}
