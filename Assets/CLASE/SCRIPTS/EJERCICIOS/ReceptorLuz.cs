using UnityEngine;
public class ReceptorLuz : MonoBehaviour
{
    public EmisorControl emisor;
    private void OnEnable() { emisor.alActivar += ResponderLuz; }
    private void OnDisable() { emisor.alActivar -= ResponderLuz; }
    private void ResponderLuz() { Debug.Log("Luz reaccionó al evento"); }
}