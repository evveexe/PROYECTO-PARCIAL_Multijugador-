using UnityEngine;

public class ReceptorB : MonoBehaviour
{
    public EmisorDinamico emisor;

    private void OnEnable()
    {
        emisor.alAvisar += ResponderB;
    }

    private void OnDisable()
    {
        emisor.alAvisar -= ResponderB;
    }

    private void ResponderB()
    {
        Debug.Log("Suscriptor B reaccionó");
        emisor.alAvisar -= ResponderB; 
    }
}