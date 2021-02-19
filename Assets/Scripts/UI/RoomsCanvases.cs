using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsCanvases : MonoBehaviour
{
    [SerializeField] private CreateOrJoinRoomCanvas _createOrJoinRoomCanvas;
    public CreateOrJoinRoomCanvas CreateOrJoinRoomCanvas { get { return _createOrJoinRoomCanvas; } }

    [SerializeField] private CurrentRoomCanvas _currentRoomCanvas;
    public CurrentRoomCanvas CurrentRoomCanvas { get { return _currentRoomCanvas; } }

    [SerializeField] private RoomListingsMenu _roomListingsMenu;
    public RoomListingsMenu RoomListingsMenu { get { return _roomListingsMenu; } }

    [SerializeField] private MainMenuScript _mainMenuScript;
    public MainMenuScript MainMenuScript { get { return _mainMenuScript; } }

    [SerializeField] private NameInputCanvas _nameInputCanvas;
    public NameInputCanvas NameInputCanvas { get { return _nameInputCanvas; } }
    private void Awake()
    {
        FirstInitialize();
    }

    private void FirstInitialize()
    {
        CreateOrJoinRoomCanvas.FirstInitialize(this);
        CurrentRoomCanvas.FirstInitialize(this);
        RoomListingsMenu.FirstInitialize(this);
        MainMenuScript.FirstInitialize(this);
        NameInputCanvas.FirstInitialize(this);
    }
}
