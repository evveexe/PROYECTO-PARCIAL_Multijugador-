using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PhotonManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;

    [SerializeField] private UnityEvent onPlayerJoined;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private NetworkPrefabRef matchControllerPrefab;

    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject serversPanel;
    [SerializeField] private GameObject createPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private TMP_Text connectionErrorText;

    private readonly Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();
    private NetworkObject _matchControllerObj;
    private bool _matchControllerSpawned;

    public const string DoomMatchModeName = "DoomMatch";
    public const string ArenaMapName = "Arena";
    public const int DefaultMaxPlayers = 2;

    public static PhotonManager Instance { get; private set; }

    public static event Action onSessionListUpdated;
    public static List<SessionInfo> availableSessions = new List<SessionInfo>();

    private void Awake()
    {
        Instance = this;
        runner = GetComponent<NetworkRunner>();
        if (runner == null) runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void Start()
    {
        if (runner == null)
        {
            Debug.LogError("PhotonManager no encontró NetworkRunner en la escena.");
            return;
        }
        runner.ProvideInput = true;
        runner.AddCallbacks(this);
    }

    public async void startConnection()
    {
        startPanel.SetActive(false);
        connectingPanel.SetActive(true);

        try
        {
            await ConnectToServerLobby();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Fallo conexion: " + ex.Message);
        }

        connectingPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private async Task ConnectToServerLobby()
    {
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private async Task StartGame(GameMode mode, string sessionName, int maxPlayers, bool isVisible)
    {
        hideMenuPanels();
        connectionErrorText.gameObject.SetActive(false);
        connectingPanel.SetActive(true);

        var args = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            IsVisible = isVisible,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        };

        var result = await runner.StartGame(args);

        connectingPanel.SetActive(false);

        if (result.Ok == false)
        {
            menuPanel.SetActive(true);
            connectionErrorText.text = getErrorMessage(result.ShutdownReason);
            connectionErrorText.gameObject.SetActive(true);
        }
    }

    public void StartGameAsHost()
    {
        Task startGame = StartGame(GameMode.Host, createSessionName(), DefaultMaxPlayers, true);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartGame(GameMode.Client, null, DefaultMaxPlayers, true);
    }

    public void createCustomSession(string sessionName, int maxPlayers, bool isPrivate)
    {
        if (string.IsNullOrEmpty(sessionName))
        {
            sessionName = createSessionName();
        }

        Task startGame = StartGame(GameMode.Host, sessionName, maxPlayers, isPrivate == false);
    }

    public void joinSessionByName(string sessionName)
    {
        Task startGame = StartGame(GameMode.Client, sessionName, DefaultMaxPlayers, true);
    }

    private string createSessionName()
    {
        int sessionNumber = UnityEngine.Random.Range(1000, 10000);
        return DoomMatchModeName + "-" + sessionNumber;
    }

    private void hideMenuPanels()
    {
        menuPanel.SetActive(false);
        serversPanel.SetActive(false);
        createPanel.SetActive(false);
        joinPanel.SetActive(false);
    }

    private string getErrorMessage(ShutdownReason reason)
    {
        if (reason == ShutdownReason.GameNotFound)
        {
            return "No se encontró ninguna partida";
        }
        if (reason == ShutdownReason.GameIsFull)
        {
            return "La partida está llena";
        }
        if (reason == ShutdownReason.GameIdAlreadyExists)
        {
            return "Ya existe una partida con ese nombre";
        }
        return "No se pudo conectar (" + reason + ")";
    }

    #region Metodos de Photon

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner == null) return;
        if (runner.IsServer && !_matchControllerSpawned && matchControllerPrefab != null && matchControllerPrefab.IsValid)
        {
            _matchControllerObj = runner.Spawn(matchControllerPrefab, Vector3.zero, Quaternion.identity);
            _matchControllerSpawned = _matchControllerObj != null;
        }

        onPlayerJoined?.Invoke();

        if (runner.IsServer && !players.ContainsKey(player))
        {
            Vector3 spawnPos = Vector3.zero;
            NetworkObject playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            if (playerObj != null)
            {
                players.Add(player, playerObj);
            }
            else
            {
                Debug.LogError($"[Host] runner.Spawn devolvió null para el Player. ¿Está el prefab registrado en la PrefabTable del runner?");
            }
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputManager localInput = InputManager.Instance != null
            ? InputManager.Instance
            : FindFirstObjectByType<InputManager>();

        if (localInput == null)
        {
            input.Set(new NetworkInfoData());
            return;
        }

        Vector2 movement = localInput.GetMoveInput();
        Vector2 mouse = localInput.GetMouseDelta();
        bool fire = localInput.WasFirePressed();
        float weaponScroll = localInput.GetWeaponScroll();
        input.Set(new NetworkInfoData
        {
            movementAxis = movement,
            runInputPressed = localInput.WasRunInputPressed(),
            movingBackwards = movement.y < 0f,
            movingOnXAxis = !Mathf.Approximately(movement.x, 0f),
            mouseDeltaX = mouse.x,
            mouseDeltaY = mouse.y,
            fireInputPressed = fire,
            reloadInputPressed = localInput.WasReloadPressed(),
            weaponScroll = weaponScroll
        });
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (HasStateAuthority && players.TryGetValue(player, out NetworkObject playerObject))
        {
            if (playerObject != null && playerObject.IsValid)
                runner.Despawn(playerObject);
            players.Remove(player);
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        availableSessions = sessionList;

        if (onSessionListUpdated != null)
        {
            onSessionListUpdated();
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (_matchControllerObj != null) _matchControllerObj = null;
        players.Clear();
        _matchControllerSpawned = false;
    }

    #endregion

    #region Basura
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
    #endregion
}