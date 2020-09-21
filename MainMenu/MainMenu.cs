using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuGUI = null;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void HostLobby()
    {
        if (string.IsNullOrEmpty(NameHolder.Name)) return;

        GhostZERONetworkManager.instance.StartHost();

        _mainMenuGUI.SetActive(false);
    }
}
