using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUICanvas : MonoBehaviourPunCallbacks
{
    [SerializeField] private GamePlayerListing _playerListingPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private Transform _winnerPanel;
    [SerializeField] private TMP_Text _winnerText;

    private GameManager _gameManager;

    private List<GamePlayerListing> _listings = new List<GamePlayerListing>();
    public List<GamePlayerListing> Listings { get { return _listings; } }

    public Transform WinnerPanel { get { return _winnerPanel; } }

    public TMP_Text WinnerText { get { return _winnerText; } }

    public void FirstInitialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    private void OnEnable()
    {
        base.OnEnable();
        GetCurrentRoomPlayers();
    }

    private void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < _listings.Count; i++)
        {
            Destroy(_listings[i].gameObject);
        }
        _listings.Clear();
    }
    

    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null) return;
        foreach (KeyValuePair<int, Player> playerinfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerinfo.Value);
        }
        _listings.Sort((x, y) => x.Player.NickName.CompareTo(y.Player.NickName));
    }
    private void AddPlayerListing(Player player)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if (index == -1)
        {
            GamePlayerListing listing = Instantiate(_playerListingPrefab, _content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                _listings.Add(listing);
            }
        }
        else
        {
            _listings[index].SetPlayerInfo(player);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = _listings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(_listings[index].gameObject);
            _listings.RemoveAt(index);
        }
    }

}
