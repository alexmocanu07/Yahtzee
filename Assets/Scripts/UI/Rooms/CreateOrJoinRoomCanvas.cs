using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOrJoinRoomCanvas : MonoBehaviour
{
    [SerializeField] CreateRoomMenu _createRoomMenu;
    public CreateRoomMenu CreateRoomMenu { get { return _createRoomMenu; } }

    [SerializeField] private RoomListingsMenu _roomListingsMenu;
    
    private RoomsCanvases _roomCanvases;
    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
        _createRoomMenu.FirstInitialize(canvases);
        _roomListingsMenu.FirstInitialize(canvases);
    }
}
