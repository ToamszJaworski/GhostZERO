using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobby : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuGUI;
    [SerializeField] private InputField _ipAdressInput = null;
    [SerializeField] private Button _joinButton = null;

    private void OnEnable()
    {
        GhostZERONetworkManager.OnClientConnected += HandleClientConnected;
        GhostZERONetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        GhostZERONetworkManager.OnClientConnected -= HandleClientConnected;
        GhostZERONetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    private void HandleClientConnected()
    {
        _joinButton.interactable = true;

        _mainMenuGUI.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        _joinButton.interactable = true;

        _mainMenuGUI.SetActive(true);
    }

    public void Join()
    {
        if (string.IsNullOrEmpty(NameHolder.Name)) return;

        var _ipAdress = _ipAdressInput.text;

        GhostZERONetworkManager.instance.networkAddress = string.IsNullOrEmpty(_ipAdress) ? "localhost" : _ipAdress;
        GhostZERONetworkManager.instance.StartClient();

        _joinButton.interactable = false;
    }
}
