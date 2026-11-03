using UnityEngine;
public class ReceptorSonido : MonoBehaviour
{
    public EmisorControl emisor;
    private void OnEnable() { emisor.alActivar += ResponderLuz; }
    private void OnDisable() { emisor.alActivar -= ResponderLuz; }
    private void ResponderLuz() { Debug.Log("Sonido reaccionó al evento"); }
}