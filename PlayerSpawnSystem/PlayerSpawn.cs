using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Security.Permissions;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    private GhostZERONetworkManager _networkManager;

    private static List<Transform> _spawnPoints = new List<Transform>();

    private int _nextIndex = 0;

    private void Awake()
    {
        _networkManager = GameObject.Find("NetworkSystem").GetComponent<GhostZERONetworkManager>();
        DontDestroyOnLoad(gameObject);
    }

    public static void AddSpawnPoint(Transform _transform)
    {
        _spawnPoints.Add(_transform);

        _spawnPoints = _spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    public static void RemoveSpawnPoint(Transform _transform) => _spawnPoints.Remove(_transform);

    public override void OnStartServer() => GhostZERONetworkManager.OnServerReadied += SpawnPlayer;

    [ServerCallback]
    private void OnDestroy() => GhostZERONetworkManager.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection _conn)
    {
        var _spawnPoint = _spawnPoints.ElementAtOrDefault(_nextIndex);

        if (_spawnPoint == null)
            return;

        var _playerInstance = Instantiate(_playerPrefab, _spawnPoints[_nextIndex].position, _spawnPoints[_nextIndex].rotation);

        foreach (GamePlayerData _data in _networkManager.GamePlayers)
        {
            if (_data.connectionToClient == _conn)
            {
                var _inGameData = _playerInstance.GetComponent<InGameData>();

                _inGameData.IsKiller = _data.IsKiller;
                _inGameData.Name = _data.Name;

                if (_inGameData.IsKiller)
                {
                    _playerInstance.layer = LayerMask.NameToLayer("Killer");
                    _playerInstance.GetComponent<PlayerMovement>().CmdSetFreeze(5);
                }
            }
        }

        NetworkServer.Spawn(_playerInstance, _conn);

        _nextIndex++;
    }
}
