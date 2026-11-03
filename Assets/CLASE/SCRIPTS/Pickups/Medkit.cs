using Fusion;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Medkit : NetworkBehaviour
{
    [SerializeField] private float respawnSeconds = 10f;
    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color reloadingColor = Color.gray;

    private Collider _collider;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _mpb;
    [Networked] private int IsAvailable { get; set; }
    private float _consumedAt;

    private static readonly int _colorProp = Shader.PropertyToID("_BaseColor");

    public override void Spawned()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
        if (HasStateAuthority) IsAvailable = 1;
        UpdateVisuals();
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_renderer == null) return;
        bool available = IsAvailable == 1;
        Color c = available ? availableColor : reloadingColor;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(_colorProp, c);
        _renderer.SetPropertyBlock(_mpb);

        if (_collider != null) _collider.enabled = available;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (IsAvailable != 1) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        if (!health.ServerConsumeMedkit()) return;

        IsAvailable = 0;
        _consumedAt = Runner.SimulationTime;
        StartCoroutine(CoRespawn());
    }

    private System.Collections.IEnumerator CoRespawn()
    {
        yield return new WaitForSeconds(respawnSeconds);
        if (this == null) yield break;
        IsAvailable = 1;
    }
}
