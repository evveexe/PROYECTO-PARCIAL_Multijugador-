using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxShields = 5;
    [SerializeField] private int damagePerHit = 20;
    [SerializeField] private int medkitHealAmount = 20;
    [SerializeField] private float spawnProtectionSeconds = 2f;

    [Networked] public int CurrentHealth { get; set; }
    [Networked] public int CurrentShields { get; set; }
    [Networked] public int IsDead { get; set; }
    [Networked] public float SpawnProtectionEndsAt { get; set; }

    [Networked] private int _pendingRespawn { get; set; }
    [Networked] private Vector3 _pendingRespawnPosition { get; set; }

    private SessionPlayer sessionPlayer;

    public int MaxHealth => maxHealth;
    public int MaxShields => maxShields;
    public int DamagePerHit => damagePerHit;
    public int MedkitHealAmount => medkitHealAmount;

    public bool HasSpawnProtection => Runner != null && Runner.SimulationTime < SpawnProtectionEndsAt;

    public override void Spawned()
    {
        sessionPlayer = GetComponent<SessionPlayer>();
        if (HasStateAuthority)
        {
            CurrentHealth = maxHealth;
            CurrentShields = 0;
            IsDead = 0;
            SpawnProtectionEndsAt = Runner.SimulationTime + spawnProtectionSeconds;
        }
        if (HasInputAuthority)
        {
            HUDController.Instance?.RegisterHealth(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        HUDController.Instance?.UnregisterHealth(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (_pendingRespawn == 1)
        {
            TeleportPlayer(_pendingRespawnPosition);
            _pendingRespawn = 0;
        }
    }

    public void ServerApplyDamage(PlayerRef attacker)
    {
        if (!HasStateAuthority) return;
        if (IsDead == 1) return;
        if (HasSpawnProtection) return;

        if (CurrentShields > 0)
        {
            CurrentShields--;
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damagePerHit);

        if (CurrentHealth <= 0)
        {
            ServerKillPlayer(attacker);
        }
    }

    private void ServerKillPlayer(PlayerRef attacker)
    {
        IsDead = 1;

        SessionPlayer attackerSp = FindSessionByPlayerRef(attacker);
        if (attackerSp != null)
        {
            attackerSp.ServerAddKill();
            if (MatchController.Instance != null)
            {
                int victimNumber = sessionPlayer != null ? sessionPlayer.PlayerNumber : 0;
                MatchController.Instance.ServerReportKill(attackerSp.PlayerNumber, victimNumber);
            }
        }

        ServerScheduleRespawn();
    }

    private void ServerScheduleRespawn()
    {
        if (Runner == null) return;
        float respawnDelay = 3f;
        StartCoroutine(ServerRespawnAfter(respawnDelay));
    }

    private System.Collections.IEnumerator ServerRespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this == null || Runner == null) yield break;

        SetSpawnProtection(spawnProtectionSeconds);

        _pendingRespawnPosition = PlayerSpawnPoint.GetRespawnPosition(transform.position);
        _pendingRespawn = 1;

        CurrentHealth = maxHealth;
        CurrentShields = 0;
        IsDead = 0;
    }

    public void SetSpawnProtection(float seconds)
    {
        if (Runner == null) return;
        SpawnProtectionEndsAt = Runner.SimulationTime + seconds;
    }

    private void TeleportPlayer(Vector3 position)
    {
        var kcc = GetComponent<Fusion.Addons.SimpleKCC.KCC>();
        if (kcc != null) kcc.SetPosition(position);
        else transform.position = position;
    }

    private SessionPlayer FindSessionByPlayerRef(PlayerRef pr)
    {
        foreach (var sp in FindObjectsByType<SessionPlayer>(FindObjectsSortMode.None))
        {
            if (sp != null && sp.Object != null && sp.Object.InputAuthority == pr) return sp;
        }
        return null;
    }

    public void ServerHeal(int amount)
    {
        if (!HasStateAuthority) return;
        if (IsDead == 1) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public bool ServerTryAddShield(int amount)
    {
        if (!HasStateAuthority) return false;
        if (IsDead == 1) return false;
        if (CurrentShields >= maxShields) return false;
        CurrentShields = Mathf.Min(maxShields, CurrentShields + amount);
        return true;
    }

    public bool ServerConsumeMedkit()
    {
        if (!HasStateAuthority) return false;
        if (IsDead == 1) return false;
        if (CurrentHealth >= maxHealth) return false;
        ServerHeal(medkitHealAmount);
        return true;
    }
}
