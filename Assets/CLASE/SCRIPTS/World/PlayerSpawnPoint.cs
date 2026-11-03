using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private bool isActiveSpawn = true;
    [SerializeField] private float spawnHeightOffset = 0.5f;

    private static readonly List<PlayerSpawnPoint> _registered = new List<PlayerSpawnPoint>();

    public static IReadOnlyList<PlayerSpawnPoint> Registered => _registered;

    private static Vector3? _lastSpawnPosition;
    private const float DefaultSpawnHeightOffset = 0.5f;

    public bool IsActiveSpawn { get => isActiveSpawn; set => isActiveSpawn = value; }
    public float SpawnHeightOffset => spawnHeightOffset;

    private void OnEnable()
    {
        if (!_registered.Contains(this)) _registered.Add(this);
    }

    private void OnDisable()
    {
        _registered.Remove(this);
    }

    public static Vector3 GetInitialSpawnPosition()
    {
        var candidates = new List<PlayerSpawnPoint>();
        foreach (var s in _registered)
        {
            if (s != null && s.gameObject.activeInHierarchy && s.isActiveSpawn) candidates.Add(s);
        }
        if (candidates.Count == 0)
        {
            Debug.LogWarning("[PlayerSpawnPoint] No hay spawn points registrados. Usando (0,0,0).");
            return new Vector3(0f, 1f, 0f);
        }
        var chosen = candidates[Random.Range(0, candidates.Count)];
        _lastSpawnPosition = chosen.GetSpawnWorldPosition();
        return chosen.GetSpawnWorldPosition();
    }

    public static Vector3 GetRespawnPosition(Vector3 previousPosition)
    {
        var candidates = new List<PlayerSpawnPoint>();
        foreach (var s in _registered)
        {
            if (s == null || !s.gameObject.activeInHierarchy || !s.isActiveSpawn) continue;
            if (_lastSpawnPosition.HasValue && (s.GetSpawnWorldPosition() - _lastSpawnPosition.Value).sqrMagnitude < 0.001f) continue;
            candidates.Add(s);
        }
        if (candidates.Count == 0)
        {
            foreach (var s in _registered)
            {
                if (s != null && s.gameObject.activeInHierarchy && s.isActiveSpawn) candidates.Add(s);
            }
        }
        if (candidates.Count == 0) return previousPosition + Vector3.up * DefaultSpawnHeightOffset;
        var chosen = candidates[Random.Range(0, candidates.Count)];
        _lastSpawnPosition = chosen.GetSpawnWorldPosition();
        return chosen.GetSpawnWorldPosition();
    }

    public Vector3 GetSpawnWorldPosition()
    {
        return transform.position + Vector3.up * spawnHeightOffset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActiveSpawn ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeightOffset, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * spawnHeightOffset);
    }
}
