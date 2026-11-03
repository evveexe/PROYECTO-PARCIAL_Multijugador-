using System;
using UnityEngine;

public class EmisorSaludo : MonoBehaviour
{
    public event Action alSaludar;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            alSaludar?.Invoke();
        }
    }
}