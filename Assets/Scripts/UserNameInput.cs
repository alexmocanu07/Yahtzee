using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField = null;
    [SerializeField] private Button continueButton = null;

    private void Start() => SetUpInputField();

    private void Update()
    {
        string userName = usernameInputField.text;
        SetUserName(userName);
    }

    public void SetUpInputField()
    {
        if(!PlayerPrefs.HasKey(MasterManager.GameSettings.PlayerPrefsNameKey)) {
            continueButton.interactable = false;    
            return; 
        }

        string defaultName = PlayerPrefs.GetString(MasterManager.GameSettings.PlayerPrefsNameKey);

        usernameInputField.text = defaultName;
        Debug.Log(defaultName);
        SetUserName(defaultName);
    }
    
    public void SetUserName(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SaveUserName()
    {
        string userName = usernameInputField.text;

        PhotonNetwork.NickName = userName;

        PlayerPrefs.SetString(MasterManager.GameSettings.PlayerPrefsNameKey, userName);
    }
}
