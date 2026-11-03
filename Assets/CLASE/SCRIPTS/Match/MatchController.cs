using System.Collections.Generic;
using Fusion;
using UnityEngine;

[System.Serializable]
public struct KillFeedEntry
{
    public string message;
    public float addedAtUnscaledTime;
    public string playerId;
}

public class MatchController : NetworkBehaviour
{
    public const int KillsToWin = 5;
    public const string FormatOneKillAway = "FALTA 1 ASESINATO PARA QUE JUGADOR {0} GANE";
    public const string FormatKill = "Jugador {0} mató a Jugador {1}";
    public const string FormatVictory = "VICTORIA";
    public const string FormatDefeat = "DERROTA";

    public static MatchController Instance { get; private set; }

    [Networked] public int MatchActive { get; set; }
    [Networked] public int WinnerPlayerNumber { get; set; }

    private readonly List<KillFeedEntry> _killFeedBuffer = new List<KillFeedEntry>();
    private const int MaxFeedEntries = 12;

    public override void Spawned()
    {
        Instance = this;
        if (HasStateAuthority)
        {
            MatchActive = 1;
            WinnerPlayerNumber = 0;
        }
        Debug.Log($"MatchController inicializado. HasStateAuthority={HasStateAuthority}");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this) Instance = null;
    }

    public int AssignNextPlayerNumber(NetworkRunner runner)
    {
        if (!HasStateAuthority) return 0;
        var sessionPlayers = FindObjectsByType<SessionPlayer>(FindObjectsSortMode.None);
        int max = 0;
        foreach (var sp in sessionPlayers)
        {
            if (sp == null) continue;
            if (sp.PlayerNumber > max) max = sp.PlayerNumber;
        }
        return max + 1;
    }

    public void RegisterKillFeedEntry(string message, float lifetime = 5f)
    {
    }

    public void ServerReportKill(int killerPlayerNumber, int victimPlayerNumber)
    {
        if (!HasStateAuthority) return;

        string killMessage = string.Format(FormatKill, killerPlayerNumber, victimPlayerNumber);
        RPC_BroadcastKillFeed(killMessage);

        int remainingKills = KillsToWin - GetKillsFor(killerPlayerNumber);
        if (remainingKills == 1)
        {
            string warning = string.Format(FormatOneKillAway, killerPlayerNumber);
            RPC_BroadcastKillFeed(warning);
        }

        if (GetKillsFor(killerPlayerNumber) >= KillsToWin)
        {
            WinnerPlayerNumber = killerPlayerNumber;
            MatchActive = 0;
            foreach (var sp in FindObjectsByType<SessionPlayer>(FindObjectsSortMode.None))
            {
                if (sp != null) sp.ServerSetMatchEnded();
            }
            Debug.Log($"Partida finalizada. Ganador: Jugador {killerPlayerNumber}");
        }
    }

    private int GetKillsFor(int playerNumber)
    {
        foreach (var sp in FindObjectsByType<SessionPlayer>(FindObjectsSortMode.None))
        {
            if (sp != null && sp.PlayerNumber == playerNumber) return sp.Kills;
        }
        return 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastKillFeed(string message)
    {
        HUDController.Instance?.AddKillFeedEntry(message);
    }

    public void ServerForceMatchEnd(int winnerNumber)
    {
        if (!HasStateAuthority) return;
        if (WinnerPlayerNumber != 0) return;
        WinnerPlayerNumber = winnerNumber;
        MatchActive = 0;
        foreach (var sp in FindObjectsByType<SessionPlayer>(FindObjectsSortMode.None))
        {
            if (sp != null) sp.ServerSetMatchEnded();
        }
        RPC_BroadcastKillFeed($"GANADOR: Jugador {winnerNumber}");
    }
}
