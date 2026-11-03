using UnityEngine;

public class ReceptorSaludo : MonoBehaviour
{
    public EmisorSaludo emisor;

    private void OnEnable()
    {
        emisor.alSaludar += Responder;
    }

    private void OnDisable()
    {
        emisor.alSaludar -= Responder;
    }

    private void Responder()
    {
        Debug.Log("Holis, evento activado correctamente uwu");
    }
}