using Fusion;
using UnityEngine;

public class SessionPlayer : NetworkBehaviour
{
    [Networked] public int PlayerNumber { get; set; }
    [Networked] public int Kills { get; set; }
    [Networked] public int MatchEndedFlag { get; set; }

    public bool IsLocalPlayer => HasInputAuthority;
    public bool IsMatchEnded => MatchEndedFlag == 1;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            PlayerNumber = 0;
            Kills = 0;
            MatchEndedFlag = 0;
        }

        if (HasStateAuthority && MatchController.Instance != null)
        {
            int assigned = MatchController.Instance.AssignNextPlayerNumber(Runner);
            PlayerNumber = assigned;
            Debug.Log($"[Host] Jugador asignado número {assigned} a {gameObject.name}");
        }

        if (HasInputAuthority)
        {
            HUDController.Instance?.RegisterSessionPlayer(this);
            HUDController.Instance?.ActivateForGame();
            Debug.Log($"[Local] Sesión de Jugador {PlayerNumber} iniciada en {gameObject.name}");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            HUDController.Instance?.UnregisterSessionPlayer(this);
        }
    }

    public void ServerAddKill()
    {
        if (!HasStateAuthority) return;
        Kills++;
    }

    public void ServerSetMatchEnded()
    {
        if (!HasStateAuthority) return;
        MatchEndedFlag = 1;
    }
}
