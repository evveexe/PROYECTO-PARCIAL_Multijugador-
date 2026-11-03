using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float gravityForce = 30f;

    public float GetGravityForce()
    {
        return gravityForce;
    }
}
