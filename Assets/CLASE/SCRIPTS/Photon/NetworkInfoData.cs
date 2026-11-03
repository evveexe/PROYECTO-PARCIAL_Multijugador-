using Fusion;
using UnityEngine;

public struct NetworkInfoData : INetworkInput
{
    public Vector2 movementAxis;
    public bool runInputPressed;
    public bool movingBackwards;
    public bool movingOnXAxis;

    public float mouseDeltaX;
    public float mouseDeltaY;

    public bool fireInputPressed;
    public bool reloadInputPressed;
    public float weaponScroll;
}
