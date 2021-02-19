using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField _roomName;
    [SerializeField] private Button _createRoomButton;

    private RoomsCanvases _roomsCanvases;

    private void Start()
    {
        _createRoomButton.interactable = false;
    }
    private void Update()
    {
        string roomName = _roomName.text;
        SetRoomName(roomName);
    }

    private void SetRoomName(string name)
    {
        _createRoomButton.interactable = !string.IsNullOrEmpty(name);
    }
    public void FirstInitialize(RoomsCanvases roomsCanvases)
    {
        _roomsCanvases = roomsCanvases;
    }
    public void OnClick_CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MasterManager.GameSettings.MaxPlayerCount;
        PhotonNetwork.CreateRoom(_roomName.text, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room creation successfull");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed");
    }
    

}
