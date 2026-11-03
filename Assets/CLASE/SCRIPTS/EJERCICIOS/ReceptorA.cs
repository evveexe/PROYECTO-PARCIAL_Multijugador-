using UnityEngine;

public class ReceptorA : MonoBehaviour
{
    public EmisorDinamico emisor;

    private void OnEnable()
    {
        emisor.alAvisar += ResponderA;
    }

    private void OnDisable()
    {
        emisor.alAvisar -= ResponderA;
    }

    private void ResponderA()
    {
        Debug.Log("Suscriptor A reaccionó");
    }
}