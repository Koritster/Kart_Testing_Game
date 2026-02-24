using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class Session : MonoBehaviour
{
    static public Session Instance { get; private set; }

    public string localPlayerName = "Player1";
    public string localPlayerKart = "default";

    [SerializeField] private TMP_InputField m_SessionName;

    [Header("Lista de sesiones")]
    [SerializeField] private GameObject m_SesionBtnPrefab;
    [SerializeField] private GameObject m_ContentParent;

    [SerializeField] private GameObject m_LobbiesPanel;
    [SerializeField] private GameObject m_CreatePanel;
    [SerializeField] private GameObject m_SessionJoinedPanel;

    private IList<GameObject> sessionBtnInstances = new List<GameObject>();
    private IList<ISessionInfo> sessions;

    private ISession actualSession;

    [Header("Perfil")]
    [SerializeField] private TMP_InputField m_UsernameInput;

    [Header("Players on session")]
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayersContentParent;

    [Header("Start Game")]
    [SerializeField] private GameObject m_StartGameBtn;

    private IList<GameObject> playersConnectedInstances = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
            Instance = this;
        }
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            await AuthenticationService.Instance.GetPlayerNameAsync();
            m_UsernameInput.text = AuthenticationService.Instance.PlayerName;
            localPlayerName = AuthenticationService.Instance.PlayerName;
            RefreshSessionList();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        RegisterEvents();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    #region Listar lobbies

    public void Refresh()
    {
        RefreshSessionList();
    }

    internal async void RefreshSessionList()
    {
        await UpdateSessions();

        foreach (var listItem in sessionBtnInstances)
        {
            Destroy(listItem);
        }

        if (sessions == null)
            return;

        foreach (var sessionInfo in sessions)
        {
            var itemPrefab = Instantiate(m_SesionBtnPrefab, m_ContentParent.transform);
            if (itemPrefab.TryGetComponent<SessionButton>(out var sessionItem))
            {
                sessionItem.SetSession(sessionInfo);
            }

            sessionBtnInstances.Add(itemPrefab);
        }
    }

    async Task UpdateSessions()
    {
        var options = new QuerySessionsOptions();

        var sessions = await MultiplayerService.Instance.QuerySessionsAsync(options);
        this.sessions = sessions.Sessions;
    }

    #endregion

    #region Perfil

    void RegisterEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"The player has successfully signed in");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log($"The access token was not refreshed and has expired");
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log($"The player has successfully signed out");
        };
    }

    public async void RegisterProfileChanges()
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(m_UsernameInput.text);
        await AuthenticationService.Instance.GetPlayerNameAsync();
        m_UsernameInput.text = AuthenticationService.Instance.PlayerName;
        localPlayerName = AuthenticationService.Instance.PlayerName;
    }

    #endregion

    #region Crear sesiones

    public async void CreateSession()
    {
        try
        {
            await StartSessionAsHost();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    async Task StartSessionAsHost()
    {
        /*var playerData = new Dictionary<string, SessionProperty>
        {
            {
                //"PlayerName", new SessionProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member)
            }
        };*/

        var playerData = new Dictionary<string, PlayerProperty>
        {
            {
                "PlayerName", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member)
            }
        };

        string sessionName = m_SessionName.text != "" ? m_SessionName.text : localPlayerName; 

        var options = new SessionOptions
        {
            Name = sessionName,
            MaxPlayers = 4,
            PlayerProperties = playerData
            //SessionProperties = playerData
        }.WithRelayNetwork();

        actualSession = await MultiplayerService.Instance.CreateSessionAsync(options);

        Debug.Log($"Session {actualSession.Id} created! Join code: {actualSession.Code}");

        m_LobbiesPanel.SetActive(false);
        m_SessionJoinedPanel.SetActive(true);

        m_StartGameBtn.SetActive(true);

        actualSession.PlayerJoined += PlayerJoinedSession;

        RefreshPlayersOnSession(actualSession);
    }

    #endregion

    #region Unirse a una sesion

    public async void JoinSessionWithId(string sessionId)
    {
        await JoinSessionWithIdTask(sessionId);
    }

    async Task JoinSessionWithIdTask(string sessionId)
    {
        var playerData = new Dictionary<string, PlayerProperty>
        {
            {
                "PlayerName", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member)
            }
        };

        var options = new JoinSessionOptions
        {
            PlayerProperties = playerData
        };

        actualSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, options);

        actualSession.PlayerJoined += PlayerJoinedSession;

        m_LobbiesPanel.SetActive(false);
        m_SessionJoinedPanel.SetActive(true);

        RefreshPlayersOnSession(actualSession);
    }

    #endregion

    private void RefreshPlayersOnSession(ISession m_Session)
    {
        foreach (var player in playersConnectedInstances)
        {
            Destroy(player);
        }

        if (m_Session.Players == null)
        {
            return;
        }

        foreach (var player in m_Session.Players)
        {
            var instance = Instantiate(m_PlayerPrefab, m_PlayersContentParent.transform);

            if (instance.TryGetComponent<PlayerLobby>(out PlayerLobby m_playerLobby))
            {
                m_playerLobby.SetSession(player);
            }

            playersConnectedInstances.Add(instance);
        }
    }

    private void PlayerJoinedSession(string m_PlayerId)
    {
        Debug.Log($"Player {m_PlayerId} has joined");

        foreach (var player in actualSession.Players)
        {
            Debug.Log($"Player ID: {player.Id}");
            if (player.Properties != null && player.Properties.TryGetValue("PlayerName", out PlayerProperty playerNameProperty))
            {
                Debug.Log($"PlayerName: {playerNameProperty.Value}");
            }
            else
            {
                Debug.Log("PlayerName property not found for player.");
            }
        }

        RefreshPlayersOnSession(actualSession);
    }

    void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        NetcodeLobby.instance.AddPlayerServerRpc(new FixedString32Bytes(localPlayerName), new FixedString32Bytes(localPlayerKart));
    }
}