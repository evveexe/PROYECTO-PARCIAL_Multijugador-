using Fusion;
using UnityEngine;

[System.Serializable]
public class WeaponConfig
{
    public string name = "Weapon";
    [Tooltip("Proyectiles por disparo. 1 = semiautomática, 2 = doble.")]
    public int bulletsPerShot = 1;
    [Tooltip("Pausa entre proyectil 1 y 2 (sólo si bulletsPerShot > 1).")]
    public float secondsBetweenBullets = 0.12f;
    [Tooltip("Cooldown total después de la ráfaga antes de permitir otro disparo (segundos).")]
    public float fireCooldown = 0.25f;
}

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private WeaponConfig weapon1;
    [SerializeField] private WeaponConfig weapon2;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private NetworkPrefabRef projectilePrefabRef;
    [Tooltip("Velocidad lineal de la bala en m/s. Se usa para mover la bala localmente en cada peer y para el raycast del host.")]
    [SerializeField] private float bulletSpeed = 60f;

    private Vector3 _nextShotOrigin;
    private Vector3 _nextShotDirection;

    [Networked] public int ActiveWeapon { get; set; }
    [Networked] private int _fireTickCooldown { get; set; }

    public WeaponConfig CurrentConfig
    {
        get
        {
            return ActiveWeapon == 2 ? weapon2 : weapon1;
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            if (weapon1 == null) weapon1 = new WeaponConfig { name = "Pistola", bulletsPerShot = 1, fireCooldown = 0.25f };
            if (weapon2 == null) weapon2 = new WeaponConfig { name = "Doble", bulletsPerShot = 2, secondsBetweenBullets = 0.15f, fireCooldown = 0.55f };
            ActiveWeapon = 1;
            _fireTickCooldown = 0;
        }
    }

    public void ServerProcessInput(bool firePressed, float weaponScrollInput)
    {
        if (!HasStateAuthority) return;
        if (MatchController.Instance == null || MatchController.Instance.MatchActive == 0) return;
        if (PlayerHealth_IsDeadProxy()) return;

        if (Mathf.Abs(weaponScrollInput) > 0.01f)
        {
            int newWeapon = weaponScrollInput > 0f ? 2 : 1;
            if (newWeapon != ActiveWeapon)
            {
                ActiveWeapon = newWeapon;
                RPC_OnWeaponChanged(ActiveWeapon);
            }
        }

        if (firePressed)
        {
            TryFire();
        }
    }

    private bool PlayerHealth_IsDeadProxy()
    {
        var ph = GetComponent<PlayerHealth>();
        return ph != null && ph.IsDead == 1;
    }

    private void TryFire()
    {
        var config = CurrentConfig;
        if (config == null) return;
        if (Runner == null) return;
        if (Runner.Tick.Raw < _fireTickCooldown) return;

        StartCoroutine(ServerFireBurst(config, Runner.Tick.Raw));
    }

    private System.Collections.IEnumerator ServerFireBurst(WeaponConfig config, int startTick)
    {
        int bullets = config.bulletsPerShot;
        float delay = config.secondsBetweenBullets;
        for (int i = 0; i < bullets; i++)
        {
            SpawnProjectile();
            if (i < bullets - 1 && delay > 0f) yield return new WaitForSeconds(delay);
        }

        long cooldownTicks = (long)(config.fireCooldown * Runner.TickRate);
        _fireTickCooldown = (int)(Runner.Tick.Raw + cooldownTicks);
    }

    private void SpawnProjectile()
    {
        if (Runner == null || projectilePrefabRef == null || projectilePrefabRef.IsValid == false) return;
        if (shootPoint == null) return;

        _nextShotOrigin = shootPoint.position;
        _nextShotDirection = shootPoint.forward;

        Runner.Spawn(projectilePrefabRef, _nextShotOrigin, Quaternion.LookRotation(_nextShotDirection), Object.InputAuthority, PrepareProjectile);
    }

    private void PrepareProjectile(NetworkRunner runner, NetworkObject projectileObject)
    {
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null) return;

        projectile.ServerLaunch(_nextShotOrigin, _nextShotDirection, bulletSpeed, transform, Object.InputAuthority);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnWeaponChanged(int newWeapon)
    {
        ActiveWeapon = newWeapon;
        HUDController.Instance?.OnWeaponChanged(newWeapon);
    }
}
