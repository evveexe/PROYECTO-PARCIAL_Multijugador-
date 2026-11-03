using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public ShootType shootType;

    public int maxAmmo;
    public int bulletDamage;
    public int reserveAmmo;
    public int currentAmmo;

    public GameObject bulletPrefab;

    public abstract void PhysicShoot();
    public abstract void RaycastShoot();
    public abstract void Reload();
}