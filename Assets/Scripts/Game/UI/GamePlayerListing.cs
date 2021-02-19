using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class GamePlayerListing : MonoBehaviour
{
    [SerializeField] private new TMP_Text name;
    [SerializeField] private TMP_Text scoreText;

    private int score = 0;

    public Player Player { get; private set; }

    public void SetPlayerInfo(Player player)
    {
        Player = player;
        name.text = player.NickName;
        scoreText.text = "0 points";
    }


    public void AddScore(int points)
    {
        score += points;
        scoreText.text = score.
            ToString() + " points";
    }

    public int GetScore()
    {
        return score;
    }
}
