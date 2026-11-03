using Fusion;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldPickup : NetworkBehaviour
{
    [SerializeField] private float respawnSeconds = 12f;
    [SerializeField] private Color availableColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color reloadingColor = Color.gray;

    private Collider _collider;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _mpb;
    [Networked] private int IsAvailable { get; set; }
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

        if (!health.ServerTryAddShield(1)) return;

        IsAvailable = 0;
        StartCoroutine(CoRespawn());
    }

    private System.Collections.IEnumerator CoRespawn()
    {
        yield return new WaitForSeconds(respawnSeconds);
        if (this == null) yield break;
        IsAvailable = 1;
    }
}
