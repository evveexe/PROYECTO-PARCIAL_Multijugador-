using UnityEngine;
public class ReceptorUI : MonoBehaviour
{
    public EmisorControl emisor;
    private void OnEnable() { emisor.alActivar += ResponderLuz; }
    private void OnDisable() { emisor.alActivar -= ResponderLuz; }
    private void ResponderLuz() { Debug.Log("UI reaccionó al evento"); }
}