using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Manager/GameSettings")]

public class GameSettings : ScriptableObject
{
    [SerializeField] private string _gameVersion = "0.0.0";
    public string GameVersion { get { return _gameVersion; } }

    [SerializeField] private byte _maxPlayerCount = 5;
    public byte MaxPlayerCount { get { return _maxPlayerCount; } }

    [SerializeField] private const string _playerPrefsNameKey = "PlayerName";
    public string PlayerPrefsNameKey { get { return _playerPrefsNameKey; } }

}
