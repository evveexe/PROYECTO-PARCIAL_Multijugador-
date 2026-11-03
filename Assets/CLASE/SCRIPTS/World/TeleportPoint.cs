using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportPoint : NetworkBehaviour
{
    [SerializeField] private float cooldownSeconds = 1.5f;
    private static readonly List<TeleportPoint> _registered = new List<TeleportPoint>();

    public static IReadOnlyList<TeleportPoint> Registered => _registered;

    public float CooldownSeconds => cooldownSeconds;

    public override void Spawned()
    {
        if (!_registered.Contains(this)) _registered.Add(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _registered.Remove(this);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestTeleport(PlayerRef requester)
    {
        if (!HasStateAuthority) return;

        var candidates = new List<TeleportPoint>();
        foreach (var tp in _registered)
        {
            if (tp == null || tp == this) continue;
            candidates.Add(tp);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[TeleportPoint] No hay otros teleports registrados.");
            return;
        }

        var target = candidates[Random.Range(0, candidates.Count)];
        Vector3 destination = target.transform.position;

        NetworkObject playerObj = null;
        foreach (var no in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (no != null && no.InputAuthority == requester)
            {
                playerObj = no;
                break;
            }
        }
        if (playerObj == null) return;

        var health = playerObj.GetComponent<PlayerHealth>();
        var kcc = playerObj.GetComponent<Fusion.Addons.SimpleKCC.KCC>();
        if (kcc != null) kcc.SetPosition(destination);
        else playerObj.transform.position = destination;

        if (health != null) health.SetSpawnProtection(2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerNo = other.GetComponentInParent<NetworkObject>();
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerNo == null || playerHealth == null) return;
        if (playerHealth.HasSpawnProtection) return;

        RPC_RequestTeleport(playerNo.InputAuthority);
    }
}
