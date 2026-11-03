using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private Transform listContent;
    [SerializeField] private SessionEntry sessionRowPrefab;
    [SerializeField] private TMP_Text emptyText;

    private List<SessionEntry> sessionRows = new List<SessionEntry>();

    private void refreshSessions()
    {
        removeClosedSessions();

        for (int i = 0; i < PhotonManager.availableSessions.Count; i++)
        {
            SessionInfo sessionInfo = PhotonManager.availableSessions[i];
            SessionEntry row = findSessionRow(sessionInfo.Name);

            if (row == null)
            {
                row = Instantiate(sessionRowPrefab, listContent);
                sessionRows.Add(row);
            }

            row.SetInfo(buildSessionData(sessionInfo));
        }

        emptyText.gameObject.SetActive(sessionRows.Count == 0);
    }

    private ServerDataStruct buildSessionData(SessionInfo sessionInfo)
    {
        ServerDataStruct sessionData = new ServerDataStruct();
        sessionData.serverName = sessionInfo.Name;
        sessionData.gameMode = PhotonManager.DoomMatchModeName;
        sessionData.mapName = PhotonManager.ArenaMapName;
        sessionData.currentPlayers = sessionInfo.PlayerCount;
        sessionData.maxPlayers = sessionInfo.MaxPlayers;
        return sessionData;
    }

    private SessionEntry findSessionRow(string sessionName)
    {
        for (int i = 0; i < sessionRows.Count; i++)
        {
            if (sessionRows[i].getSessionName() == sessionName)
            {
                return sessionRows[i];
            }
        }
        return null;
    }

    private void removeClosedSessions()
    {
        for (int i = sessionRows.Count - 1; i >= 0; i--)
        {
            if (sessionStillExists(sessionRows[i].getSessionName()) == false)
            {
                Destroy(sessionRows[i].gameObject);
                sessionRows.RemoveAt(i);
            }
        }
    }

    private bool sessionStillExists(string sessionName)
    {
        for (int i = 0; i < PhotonManager.availableSessions.Count; i++)
        {
            if (PhotonManager.availableSessions[i].Name == sessionName)
            {
                return true;
            }
        }
        return false;
    }

    private void OnEnable()
    {
        PhotonManager.onSessionListUpdated += refreshSessions;
        refreshSessions();
    }

    private void OnDisable()
    {
        PhotonManager.onSessionListUpdated -= refreshSessions;
    }
}
