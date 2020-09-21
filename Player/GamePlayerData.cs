using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GamePlayerData : NetworkBehaviour
{

    public static GamePlayerData singleton;

    [SyncVar]
    public string Name = "Loading...";

    [SyncVar]
    public bool IsKiller;

    [SyncVar]
    public bool IsDead;

    private GhostZERONetworkManager _gameRoom;

    private GhostZERONetworkManager GameRoom
    {
        get
        {
            if (_gameRoom != null) return _gameRoom;

            return _gameRoom = NetworkManager.singleton as GhostZERONetworkManager;
        }
    }

    private void Awake()
    {
        singleton = this;
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        GameRoom.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        GameRoom.GamePlayers.Remove(this);
    }

    [Server]
    public void SetName(string _name)
    {
        this.Name = _name;
    }   
}
