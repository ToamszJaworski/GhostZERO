using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameHolder : MonoBehaviour
{
    [SerializeField] private InputField _nameInput = null;

    public static string Name { get; private set; }

    private const string _playerPrefsKey = "PlayerName";

    private void Start()
    {
        if (!PlayerPrefs.HasKey(_playerPrefsKey))
            return;

        var _name = PlayerPrefs.GetString(_playerPrefsKey);

        _nameInput.text = _name;

        Name = _name;
    }

    public void SetName()
    {
        Name = _nameInput.text;

        PlayerPrefs.SetString(_playerPrefsKey, _nameInput.text);
    }
}
