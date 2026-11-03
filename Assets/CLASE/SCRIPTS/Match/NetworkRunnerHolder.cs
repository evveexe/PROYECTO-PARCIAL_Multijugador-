using Fusion;
using UnityEngine;

public class NetworkRunnerHolder : MonoBehaviour
{
    public static NetworkRunnerHolder Instance { get; private set; }
    public NetworkRunner Runner { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Runner = GetComponent<NetworkRunner>();
        if (Runner == null) Runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
