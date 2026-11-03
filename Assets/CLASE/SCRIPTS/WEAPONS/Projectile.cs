using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float impactEffectLifetime = 5f;
    [SerializeField] private float impactOffset = 0.05f;
    [Tooltip("Tiempo que el host espera tras el impacto antes de hacer Despawn, para que los clientes (que arrancan la bala más tarde por latencia) lleguen a mostrar el impacto.")]
    [SerializeField] private float despawnGraceSeconds = 0.5f;

    public PlayerRef AttackerPlayerRef { get; private set; }

    [Networked] private Vector3 FireOrigin { get; set; }
    [Networked] private Vector3 FireDirection { get; set; }
    [Networked] private float FireSpeed { get; set; }
    [Networked] private NetworkBool FireHasHit { get; set; }
    [Networked] private Vector3 FireHitPoint { get; set; }
    [Networked] private Vector3 FireHitNormal { get; set; }

    private Renderer[] _renderers;
    private Vector3 _direction;
    private float _speed;
    private bool _isActive;
    private bool _hasHit;
    private Vector3 _hitPoint;
    private Vector3 _hitNormal;
    private float _distanceToImpact;
    private float _flownDistance;
    private float _localDespawnTime;
    private float _hostDespawnAt;

    public void ServerLaunch(Vector3 origin, Vector3 direction, float speed, Transform shooterRoot, PlayerRef attacker)
    {
        AttackerPlayerRef = attacker;

        Vector3 dir = direction.normalized;
        float maxRange = speed * lifetime;
        bool hasHit = false;
        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.up;

        RaycastHit[] hits = Physics.RaycastAll(origin, dir, maxRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        int closestIndex = -1;
        float closestDistance = maxRange;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (shooterRoot != null && hit.collider.transform.IsChildOf(shooterRoot)) continue;
            if (hit.collider.GetComponentInParent<Projectile>() != null) continue;

            if (hit.distance <= closestDistance)
            {
                closestDistance = hit.distance;
                closestIndex = i;
            }
        }

        if (closestIndex >= 0)
        {
            RaycastHit closestHit = hits[closestIndex];
            hasHit = true;
            hitPoint = closestHit.point;
            hitNormal = closestHit.normal;

            PlayerHealth health = closestHit.collider.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.ServerApplyDamage(attacker);
            }
        }

        FireOrigin = origin;
        FireDirection = dir;
        FireSpeed = speed;
        FireHasHit = hasHit;
        FireHitPoint = hitPoint;
        FireHitNormal = hitNormal;
    }

    public override void Spawned()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        transform.position = FireOrigin;
        if (FireDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(FireDirection);

        _direction = FireDirection;
        _speed = FireSpeed;
        _hasHit = FireHasHit;
        _hitPoint = FireHitPoint;
        _hitNormal = FireHitNormal;
        _distanceToImpact = _hasHit ? Vector3.Distance(FireOrigin, _hitPoint) : float.MaxValue;
        _flownDistance = 0f;
        _isActive = true;
        _localDespawnTime = Time.time + lifetime;
        _hostDespawnAt = 0f;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_isActive && _hasHit)
        {
            SpawnImpact(_hitPoint, _hitNormal);
        }
        _isActive = false;
    }

    private void SpawnImpact(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab == null) return;

        Vector3 spawnPos = position + normal * impactOffset;
        GameObject impact = Instantiate(impactEffectPrefab, spawnPos, Quaternion.Euler(0, 0, 0));
        Destroy(impact, impactEffectLifetime);
    }

    private void SetVisible(bool visible)
    {
        if (_renderers == null) return;
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null) _renderers[i].enabled = visible;
        }
    }

    private void Despawn()
    {
        if (Runner != null && Runner.IsRunning && Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }

    private void Update()
    {
        if (_hostDespawnAt > 0f)
        {
            if (HasStateAuthority && Time.time >= _hostDespawnAt)
            {
                _hostDespawnAt = 0f;
                Despawn();
            }
            return;
        }

        if (!_isActive) return;

        float step = _speed * Time.deltaTime;
        transform.position += _direction * step;
        _flownDistance += step;

        if (_flownDistance >= _distanceToImpact)
        {
            transform.position = _hitPoint;
            _isActive = false;
            SetVisible(false);
            SpawnImpact(_hitPoint, _hitNormal);
            if (HasStateAuthority) _hostDespawnAt = Time.time + despawnGraceSeconds;
            return;
        }

        if (Time.time >= _localDespawnTime)
        {
            _isActive = false;
            SetVisible(false);
            if (HasStateAuthority) Despawn();
        }
    }
}
