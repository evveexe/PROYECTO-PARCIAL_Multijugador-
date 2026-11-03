using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EmisorDinamico : MonoBehaviour
{
    public event Action alAvisar;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            alAvisar?.Invoke();
        }
    }
}