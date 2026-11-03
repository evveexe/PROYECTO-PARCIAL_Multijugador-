//using System;
//using UnityEngine;

//public class TestEventos : MonoBehaviour
//{
//    public event Action eventoDePrueba;

//    // Este se manda a llamar cuando se apaga el script
//    private void OnDisable()
//    {
//        eventoDePrueba += Metodo1; //Suscripcion> Al hechobde agregar un metodo a evento
//    }

//    // Esto se manda a llamar cuando se prende el script
//    private void OnEnable()
//    {       
        
//    }

//    private void Start()
//    {
//        Metodo = Metodo1;
//        Metodo();
//    }

//    private void Metodo1()
//    {
//        Debug.Log("Hola mundo");
//    }


//}