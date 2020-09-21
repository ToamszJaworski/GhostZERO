using Mirror.LiteNetLib4Mirror;
using Mirror;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GhostZERONetworkManager : LiteNetLib4MirrorNetworkManager
{
    public static GhostZERONetworkManager instance;

    [Scene] [SerializeField] private string _menuScene = string.Empty;
    public int MinPlayers = 2;

    [Header("Lobby")]
    [SerializeField] private NetworkPlayerLobby _lobbyPlayer;

    [Header("Game")]
    [SerializeField] private GamePlayerData _gamePlayer;
    [SerializeField] private GameObject _playerSpawnSystem = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied; 

    public List<NetworkPlayerLobby> LobbyPlayers { get; } = new List<NetworkPlayerLobby>();
    public List<GamePlayerData> GamePlayers { get; } = new List<GamePlayerData>();

    private void Awake()
    {
        instance = this;
    }

    public override void OnStartServer()
    { 
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    public override void OnStartClient()
    {
        var _spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach(var _prefab in _spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(_prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        OnClientConnected?.Invoke();

        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        OnClientDisconnected?.Invoke();

        base.OnClientDisconnect(conn);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            Debug.LogError("Too much players!");
            return;
        }

        if(SceneManager.GetActiveScene().path != _menuScene)
        {
            conn.Disconnect();
            Debug.LogError(SceneManager.GetActiveScene().path + " != " + _menuScene);
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if(SceneManager.GetActiveScene().path == _menuScene)
        {
            var _isLeader = LobbyPlayers.Count == 0;

            NetworkPlayerLobby _room = Instantiate(_lobbyPlayer);

            _room.IsHost = _isLeader;

            NetworkServer.AddPlayerForConnection(conn, _room.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if(conn.identity != null)
        {
            var _player = conn.identity.GetComponent<NetworkPlayerLobby>();

            LobbyPlayers.Remove(_player);

            CheckReadyStates();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        LobbyPlayers.Clear();
    }

    public void CheckReadyStates()
    {
        foreach (var _player in LobbyPlayers)
            _player.HandleReadyToStart(IsReadyToStart());
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < MinPlayers) return false;

        foreach (var _player in LobbyPlayers)
            if (!_player.IsReady) return false;

        return true;
    }

    public void StartGame()
    {
        if(SceneManager.GetActiveScene().path == _menuScene)
        {
            if (!IsReadyToStart()) return;

            ServerChangeScene("Game_Map_Test");
        }
    }

    [Server]
    public override void ServerChangeScene(string newSceneName)
    {
        if(SceneManager.GetActiveScene().path == _menuScene && newSceneName.StartsWith("Game_Map"))
        {
            SetKiller();

            for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
            {
                var _conn = LobbyPlayers[i].connectionToClient;

                var _camera = GameObject.Find("LobbyCamera");

                if(_camera != null)
                    _camera.GetComponent<AudioListener>().enabled = false;

                var _gamePlayerInstance = Instantiate(_gamePlayer);
                _gamePlayerInstance.SetName(LobbyPlayers[i].DisplayName);

                _gamePlayerInstance.IsKiller = LobbyPlayers[i].IsKiller;

                NetworkServer.ReplacePlayerForConnection(_conn, _gamePlayerInstance.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    [Server]
    public override void OnServerChangeScene(string newSceneName)
    {
        if(newSceneName.StartsWith("Game_Map"))
        {
            var _playerSpawnerInstance = Instantiate(_playerSpawnSystem);
            NetworkServer.Spawn(_playerSpawnerInstance);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

    private void SetKiller()
    {
        var _randomKiller = UnityEngine.Random.Range(0, LobbyPlayers.Count);

        LobbyPlayers[_randomKiller].IsKiller = true;
    }

    void EndRound()
    {
        spawnPrefabs = null;

        ServerChangeScene(_menuScene);

        StopHost();
    }

    public void CheckRoundEnd()
    {
        var _killerAlive = false;
        var _playerAlive = false;

        foreach (var _players in GamePlayers)
        {
            if (_players.IsDead) continue;

            if (_players.IsKiller) _killerAlive = true;
            else _playerAlive = true;
        }

        if(!_playerAlive || !_killerAlive)
        {
            //EndRound();
        }
    }
}
