using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EmisorControl : MonoBehaviour
{
    public event Action alActivar;
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            alActivar?.Invoke();
        }
    }
}