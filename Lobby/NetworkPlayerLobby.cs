using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Runtime.Remoting.Messaging;

public class NetworkPlayerLobby : NetworkBehaviour
{
    private bool _isHost;

    [SerializeField] private GameObject _lobbyGUI = null;
    [SerializeField] private Text[] _playerNames = new Text[6];
    [SerializeField] private Text[] _readyStates = new Text[6];
    [SerializeField] private Text[] _hostTitles = new Text[6];
    [SerializeField] private Button _startGame = null;

    [SyncVar(hook = nameof(HandleLobbyNameChanged))]
    public string DisplayName = "Loading...";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    [SyncVar(hook = nameof(HandleHostChanged))]
    public bool LobbyHost = false;

    [SyncVar]
    public bool IsKiller;

    public bool IsHost
    {
        set => _isHost = value;
    }

    private GhostZERONetworkManager _lobby;

    private GhostZERONetworkManager Lobby
    {
        get
        {
            if (_lobby != null) return _lobby;

            return _lobby = NetworkManager.singleton as GhostZERONetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetLobbyName(NameHolder.Name);
        CmdSetLobbyHost(_isHost);

        _lobbyGUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Lobby.LobbyPlayers.Add(this);

        Lobby.CheckReadyStates();

        UpdateLobby();
    }

    public override void OnStopClient()
    {
        Lobby.LobbyPlayers.Remove(this);

        UpdateLobby();
    }

    public void HandleReadyStatusChanged(bool _oldValue, bool _newValue) => UpdateLobby();
    public void HandleLobbyNameChanged(string _oldValue, string _newValue) => UpdateLobby();
    public void HandleHostChanged(bool _oldValue, bool _newValue) => UpdateLobby();

    private void UpdateLobby()
    {
        if(!hasAuthority)
        {
            foreach(var _player in Lobby.LobbyPlayers)
            {
                if(_player.hasAuthority)
                {
                    _player.UpdateLobby();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < _playerNames.Length; i++)
        {
            _playerNames[i].text = string.Empty;
            _readyStates[i].text = string.Empty;
            _hostTitles[i].text = string.Empty;
        }

        for (int i = 0; i < Lobby.LobbyPlayers.Count; i++)
        {
            _playerNames[i].text = Lobby.LobbyPlayers[i].DisplayName;
            _readyStates[i].text = Lobby.LobbyPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
            _hostTitles[i].text = Lobby.LobbyPlayers[i].LobbyHost ? "Host" : string.Empty;
        }
    }

    public void HandleReadyToStart(bool _isReady)
    {
        if (!_isHost) return;

        _startGame.gameObject.SetActive(_isReady);
    }

    [Command]
    private void CmdSetLobbyName(string _name)
    {
        DisplayName = _name;
    }

    [Command]
    private void CmdSetLobbyHost(bool _isHost)
    {
        LobbyHost = _isHost;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Lobby.CheckReadyStates();
    }
    
    [Command]
    public void CmdStartGame()
    {
        if (Lobby.LobbyPlayers[0].connectionToClient != connectionToClient) return;

        Lobby.StartGame();
    }
}
