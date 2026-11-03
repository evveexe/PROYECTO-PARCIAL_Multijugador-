using Fusion;
using UnityEngine;

public class Handgun : Weapon
{
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float bulletForce = 20f;
    [SerializeField] private float shootDistance = 50f;
    [SerializeField] private NetworkPrefabRef projectilePrefabRef;

    private NetworkRunner runner;

    public override void Reload()
    {
        currentAmmo = maxAmmo;
    }

    public override void RaycastShoot()
    {
        Ray ray = new Ray(shootPoint.position, shootPoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootDistance, hitLayer))
        {
            Debug.Log("Impacto en: " + hit.collider.gameObject.name);
        }
    }

    public override void PhysicShoot()
    {
        if (runner == null || !runner.IsServer) return;

        NetworkObject projectile = runner.Spawn(projectilePrefabRef, shootPoint.position, shootPoint.rotation);

        if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(shootPoint.forward * bulletForce, ForceMode.Impulse);
        }
    }

    public void Fire()
    {
        Debug.Log($"Fire! currentAmmo: {currentAmmo}");

        if (currentAmmo <= 0)
            return;

        currentAmmo--;

        switch (shootType)
        {
            case ShootType.Raycast:
                RaycastShoot();
                break;
            case ShootType.Physic:
                PhysicShoot();
                break;
        }
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        runner = FindAnyObjectByType<NetworkRunner>();
    }
}