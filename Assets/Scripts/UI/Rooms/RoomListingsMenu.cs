using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomListing _roomListing;
    [SerializeField] private Transform _content;
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _roomMessage;

    private List<RoomListing> _listings = new List<RoomListing>();
    private RoomsCanvases _roomsCanvases;

    private const string _masterMessage = Constants.MASTER_ROOM_MESSAGE;
    private const string _playerMessage = Constants.PLAYER_ROOM_MESSAGE;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public override void OnJoinedRoom()
    {
        _roomsCanvases.CurrentRoomCanvas.Show();
        
        _content.DestroyChildren();
        _listings.Clear();

        if (!PhotonNetwork.IsMasterClient)
        {
            _startButton.gameObject.SetActive(false);
            _roomMessage.text = _playerMessage;
        }
        else _roomMessage.text = _masterMessage;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if(index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            else
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index == -1)
                {
                    RoomListing listing = Instantiate(_roomListing, _content);
                    if (listing != null)
                    {
                        listing.SetRoomInfo(info);
                        _listings.Add(listing);
                    }

                }
            }
        }
    }
}
