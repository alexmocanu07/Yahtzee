using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using Unity.UIElements.Runtime;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private UnityEngine.UI.Button _rollButton;
    [SerializeField] private UnityEngine.UI.Button _doneRollingButton;
    [SerializeField] private GameUICanvas _gameUICanvas;

    private List<GamePlayerListing> _listings = new List<GamePlayerListing>();
    private GamePlayerListing _currentPlayer = null;
    private int _currentPlayerIndex = -1;

    //private bool _myTurn = false;

    private List<GameObject> _diceInstances = new List<GameObject>();
    private List<Dice> _onBoardDice = new List<Dice>();
    private List<Dice> _inTrayDice = new List<Dice>();
    private Dictionary<Dice, int> _diceSpots = new Dictionary<Dice, int>();
    private int[] _savedDice = new int[] { 0, 0, 0, 0, 0, 0, 0 };
    private bool[] _freeSpots = new bool[] { true, true, true, true, true };
    public Vector3 _initialDicePosition = new Vector3(0, 6, -5);
    private GameObject diceInstance;

    public int _currentRollCount = 1;
    private int _scoredFormations = 0;

    private PanelRenderer tableRenderer;
    private UnityEngine.UIElements.Button oneScoreButton, twoScoreButton, threeScoreButton, fourScoreButton, fiveScoreButton, sixScoreButton, onePairButton, twoPairButton, 
        threeOfAKindButton, fourOfAKindButton, fullHouseButton, smallStraightButton, largeStraightButton, yahtzeeButton, bonusButton;
    Label totalScoreLabel;
    private string _hoveredButton;
    private Dictionary<UnityEngine.UIElements.Button, bool> _clickedFormationButtons = new Dictionary<UnityEngine.UIElements.Button, bool>();

    private bool _addedScore;
    private bool _turnEnded;

    public void FirstInitialize()
    {
        _gameUICanvas.FirstInitialize(this);
    }
    private void Awake()
    {
        FirstInitialize();
        _listings = _gameUICanvas.Listings;
        _addedScore = false;
        _turnEnded = false;
        
    }
    private void OnEnable()
    {
        tableRenderer = GameObject.FindWithTag(Constants.TABLE_PANEL_TAG).GetComponent<PanelRenderer>();
        tableRenderer.postUxmlReload = BindTable;
    }
    private void Start()
    {
        _currentRollCount = 1;
        _listings.Sort((x, y) => x.Player.NickName.CompareTo(y.Player.NickName));
        _currentPlayer = _listings[0];
        _currentPlayerIndex = 0;

        if(PhotonNetwork.LocalPlayer.NickName != _currentPlayer.Player.NickName)
        {
            DisableButtons();
        }
        else
        {
            _rollButton.gameObject.SetActive(true);
            _doneRollingButton.gameObject.SetActive(false);
        }
    }




    IEnumerator SummonDice(int numberOfDice)
    {

        _onBoardDice = new List<Dice>();
        for (int i = 0; i < numberOfDice; i++)
        {
            if (!PhotonNetwork.IsConnected)
                yield break;
            
            diceInstance = PhotonNetwork.Instantiate(Constants.DICE_PREFAB_NAME, _initialDicePosition, Quaternion.identity);
            
            diceInstance.GetComponent<Rigidbody>().AddForce(Random.Range(-5, 5), Random.Range(-10, 5), Random.Range(5, 20), ForceMode.Impulse);
            diceInstance.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(1, 100), Random.Range(1, 100), Random.Range(1, 100)));
            diceInstance.SetActive(true);
            _diceInstances.Add(diceInstance);

            yield return new WaitForSeconds(.2f);
        }
    }

    IEnumerator RerollDice()
    {
        for (int i = 0; i < _onBoardDice.Count; i++)
        {
            _onBoardDice[i].transform.position = _initialDicePosition + new Vector3(0, 0, -i);
            _onBoardDice[i].transform.rotation = Quaternion.identity;
        }
        for (int i = 0; i < _onBoardDice.Count; i++)
        {
            _onBoardDice[i].transform.position = _initialDicePosition;
            _onBoardDice[i].GetComponent<Rigidbody>().AddForce(Random.Range(-5, 5), Random.Range(-10, 5), Random.Range(5, 20), ForceMode.Impulse);
            _onBoardDice[i].GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(1, 100), Random.Range(1, 100), Random.Range(1, 100)));

            yield return new WaitForSeconds(.2f);
        }
    }
    public void AddDiceToBoard(Dice dice)
    {
        _onBoardDice.Add(dice);
    }
    public void AddAllDiceToTray()
    {
        for(int i = 0; i < _onBoardDice.Count;)
        {
            while (!_onBoardDice[i].GetComponent<Rigidbody>().IsSleeping())
                _onBoardDice[i].GetComponent<Rigidbody>().useGravity = true;
            _onBoardDice[i].addedToTray = true;
            AddDiceToTray(_onBoardDice[i]);
            
        }
    }
    public void AddDiceToTray(Dice dice)
    {
        
        float Xoffset = 0;
        float Yoffset = 0.1f;
        if (dice.diceValue != 1 && dice.diceValue != 6) Xoffset = 0.25f;
        else Yoffset = 0.5f;
        Vector3 firstSpot = GameObject.Find(Constants.TRAY_FLOOR_TAG).transform.position + new Vector3(0, 0, -2 * 0.55f);
        for (int i = 0; i < 5; i++)
        {
            if (_freeSpots[i])
            {
                dice.GetComponent<Rigidbody>().transform.position = firstSpot + new Vector3(0 - Xoffset, 0 + Yoffset, i * 0.55f);
                _freeSpots[i] = false;
                _diceSpots.Add(dice, i);
                break;
            }
        }
        dice.setVisibleSide(dice.diceValue);
        _inTrayDice.Add(dice);
        _savedDice[dice.diceValue]++;
        _onBoardDice.Remove(dice);
    }

    public void RemoveDiceFromTray(Dice dice)
    {
        _inTrayDice.Remove(dice);
        _freeSpots[_diceSpots[dice]] = true;
        _diceSpots.Remove(dice);
        _onBoardDice.Add(dice);
        _savedDice[dice.diceValue]--;
    }   

    public void OnClick_RollButton()
    {
        if (_turnEnded)
        {
            return;
        }
        if(_currentRollCount != 1) 
            for (int i = 0; i < _onBoardDice.Count; i++)
            {
                if (!_onBoardDice[i].GetComponent<Rigidbody>().IsSleeping()) return;
            }
        if (_currentRollCount == 1)
        {
            _onBoardDice = new List<Dice>();
            StartCoroutine(SummonDice(5 - _inTrayDice.Count));
            _currentRollCount++;
            _doneRollingButton.gameObject.SetActive(true);
        }
        else if (_currentRollCount == 2)
        {
            StartCoroutine(RerollDice());
            _currentRollCount++;
        }
        else if (_currentRollCount == 3)
        {
            StartCoroutine(RerollDice());
            _currentRollCount++;
            _turnEnded = true;
        }
        else
        {
            return;
        }
    }

    public void OnClick_DoneRollingButton()
    {
        for (int i = 0; i < _onBoardDice.Count;i++)
        {
            if (!_onBoardDice[i].GetComponent<Rigidbody>().IsSleeping()) return;
            if (_onBoardDice[i].diceValue == 0) return;
        }
        _turnEnded = true;
        _currentRollCount = 4;
        AddAllDiceToTray();
        DisableButtons();
    }

    public void ResetDice()
    {
        _inTrayDice = new List<Dice>();
        _onBoardDice = new List<Dice>();
        _freeSpots = new bool[] { true, true, true, true, true };
        _diceSpots = new Dictionary<Dice, int>();
        
        for (int i = 1; i <= 6; i++)
            _savedDice[i] = 0;
        for (int i = 0; i < _diceInstances.Count; i++)
        {
            PhotonNetwork.Destroy(_diceInstances[i].gameObject);
        }
        _diceInstances = new List<GameObject>();
    }

    private int ComputeScore(string formation)
    {
        int result = 0;
        if (formation == Constants.ONE_FORMATION_NAME)
        {
            result = _savedDice[1];
        }
        else if (formation == Constants.TWO_FORMATION_NAME)
        {
            result = (2 * _savedDice[2]);
        }
        else if (formation == Constants.THREE_FORMATION_NAME)
        {
            result = (3 * _savedDice[3]);
        }
        else if (formation == Constants.FOUR_FORMATION_NAME)
        {
            result = (4 * _savedDice[4]);
        }
        else if (formation == Constants.FIVE_FORMATION_NAME)
        {
            result = (5 * _savedDice[5]);
        }
        else if (formation == Constants.SIX_FORMATION_NAME)
        {
            result = (6 * _savedDice[6]);
        }
        else if (formation == Constants.ONE_PAIR_FORMATION_NAME)
        {
            int i;
            for (i = 6; i >= 1; i--)
                if (_savedDice[i] >= 2)
                    break;
            result = (2 * i);
        }
        else if (formation == Constants.TWO_PAIRS_FORMATION_NAME)
        {
            int i, j = 0;
            for (i = 6; i >= 1; i--)
                if (_savedDice[i] >= 2)
                {
                    for (j = i - 1; j >= 1; j--)
                        if (_savedDice[j] >= 2)
                            break;
                    if (j > 0) break;
                }
            result = (2 * i + 2 * j);
        }
        else if (formation == Constants.THREE_OF_A_KIND_FORMATION_NAME)
        {
            int i, sum = 0;
            bool found = false;
            for (i = 6; i >= 1; i--)
            {
                sum += i * _savedDice[i];
                if (_savedDice[i] >= 3)
                    found = true;
            }
            if (!found) result = 0;
            else result = sum;
        }
        else if (formation == Constants.FOUR_OF_A_KIND_FORMATION_NAME)
        {
            int i;
            for (i = 6; i >= 1; i--)
                if (_savedDice[i] >= 4)
                    break;
            if (i != 0) result = 40;
            else result = 0;
        }
        else if (formation == Constants.FULL_HOUSE_FORMATION_NAME)
        {
            bool foundThree = false, foundTwo = false;
            for (int i = 1; i <= 6; i++)
            {
                if (_savedDice[i] == 2) foundTwo = true;
                else if (_savedDice[i] == 3) foundThree = true;
            }

            if (foundTwo && foundThree) result = 30;
            else result = 0;
        }
        else if (formation == Constants.SMALL_STRAIGHT_FORMATION_NAME)
        {
            int size = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (_savedDice[i] == 0)
                    if (size < 4) size = 0;
                    else break;
                else size++;
            }
            if (size >= 4) result = 15;
            else result = 0;
        }
        else if (formation == Constants.LARGE_STRAIGHT_FORMATION_NAME)
        {
            int size = 0;
            for (int i = 1; i <= 6; i++)
            {
                if (_savedDice[i] == 0)
                    if (size < 5) size = 0;
                    else break;
                else size++;
            }
            if (size == 5) result = 25;
            else result = 0;
        }
        else if (formation == Constants.YAHTZEE_FORMATION_NAME)
        {
            int i;
            for (i = 1; i <= 6; i++)
                if (_savedDice[i] == 5) break;
            if (i == 7) result = 0;
            else result = 50;
        }
        else if (formation == Constants.BONUS_FORMATION_NAME)
        {
            int sum = 0;
            for (int i = 1; i <= 6; i++)
                sum += i * _savedDice[i];
            result = sum;
        }

        return result;
    }

    private void OnHoverScoreButton(bool enter, string formation, UnityEngine.UIElements.Button button)
    {
        if (!enter)
        {
            if(_hoveredButton == formation)
            {
                button.text = "";
                _hoveredButton = null;
                
            }
            return;
        }
        if (_currentRollCount != 4 || _clickedFormationButtons.ContainsKey(button))
        {
            return;
        }
        if (_addedScore)
        {
            return;
        }

        if (string.IsNullOrEmpty(button.text))
        {
            int score = ComputeScore(formation);
            button.text = score.ToString();
            _hoveredButton = formation;
        }
    }

    private IEnumerable<UnityEngine.Object> BindTable()
    {
        var root = tableRenderer.visualTree;
        oneScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.ONE_SCORE_BUTTON_NAME);
        if (oneScoreButton != null)
        {

            oneScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.ONE_FORMATION_NAME, oneScoreButton);
            };
            oneScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.ONE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            oneScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.ONE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }
        twoScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.TWO_SCORE_BUTTON_NAME);
        if (twoScoreButton != null)
        {
            twoScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.TWO_FORMATION_NAME, twoScoreButton);
            };
            twoScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.TWO_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            twoScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.TWO_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        threeScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.THREE_SCORE_BUTTON_NAME);
        if (threeScoreButton != null)
        {
            threeScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.THREE_FORMATION_NAME, threeScoreButton);
            };
            threeScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.THREE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            threeScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.THREE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }
        fourScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.FOUR_SCORE_BUTTON_NAME);
        if (fourScoreButton != null)
        {
            fourScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.FOUR_FORMATION_NAME, fourScoreButton);
            };
            fourScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.FOUR_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            fourScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.FOUR_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }
        fiveScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.FIVE_SCORE_BUTTON_NAME);
        if (fiveScoreButton != null)
        {
            fiveScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.FIVE_FORMATION_NAME, fiveScoreButton);
            };
            fiveScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.FIVE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            fiveScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.FIVE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }
        sixScoreButton = root.Q<UnityEngine.UIElements.Button>(Constants.SIX_SCORE_BUTTON_NAME);
        if (sixScoreButton != null)
        {
            sixScoreButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.SIX_FORMATION_NAME, sixScoreButton);
            };
            sixScoreButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.SIX_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            sixScoreButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.SIX_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        onePairButton = root.Q<UnityEngine.UIElements.Button>(Constants.ONE_PAIR_SCORE_BUTTON_NAME);
        if (onePairButton != null)
        {
            onePairButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.ONE_PAIR_FORMATION_NAME, onePairButton);
            };
            onePairButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.ONE_PAIR_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            onePairButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.ONE_PAIR_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        twoPairButton = root.Q<UnityEngine.UIElements.Button>(Constants.TWO_PAIR_SCORE_BUTTON_NAME);
        if (twoPairButton != null)
        {
            twoPairButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.TWO_PAIRS_FORMATION_NAME, twoPairButton);
            };
            twoPairButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.TWO_PAIRS_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            twoPairButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.TWO_PAIRS_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        threeOfAKindButton = root.Q<UnityEngine.UIElements.Button>(Constants.THREE_OF_A_KIND_SCORE_BUTTON_NAME);
        if (threeOfAKindButton != null)
        {
            threeOfAKindButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.THREE_OF_A_KIND_FORMATION_NAME, threeOfAKindButton);
            };
            threeOfAKindButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.THREE_OF_A_KIND_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            threeOfAKindButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.THREE_OF_A_KIND_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        fourOfAKindButton = root.Q<UnityEngine.UIElements.Button>(Constants.FOUR_OF_A_KIND_SCORE_BUTTON_NAME);
        if (fourOfAKindButton != null)
        {
            fourOfAKindButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.FOUR_OF_A_KIND_FORMATION_NAME, fourOfAKindButton);
            };
            fourOfAKindButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.FOUR_OF_A_KIND_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            fourOfAKindButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.FOUR_OF_A_KIND_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        fullHouseButton = root.Q<UnityEngine.UIElements.Button>(Constants.FULL_HOUSE_SCORE_BUTTON_NAME);
        if (fullHouseButton != null)
        {
            fullHouseButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.FULL_HOUSE_FORMATION_NAME, fullHouseButton);
            };
            fullHouseButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.FULL_HOUSE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            fullHouseButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.FULL_HOUSE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        smallStraightButton = root.Q<UnityEngine.UIElements.Button>(Constants.SMALL_STRAIGHT_SCORE_BUTTON_NAME);
        if (smallStraightButton != null)
        {
            smallStraightButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.SMALL_STRAIGHT_FORMATION_NAME, smallStraightButton);
            };
            smallStraightButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.SMALL_STRAIGHT_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            smallStraightButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.SMALL_STRAIGHT_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        largeStraightButton = root.Q<UnityEngine.UIElements.Button>(Constants.LARGE_STRAIGHT_SCORE_BUTTON_NAME);
        if (largeStraightButton != null)
        {
            largeStraightButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.LARGE_STRAIGHT_FORMATION_NAME, largeStraightButton);
            };
            largeStraightButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.LARGE_STRAIGHT_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            largeStraightButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.LARGE_STRAIGHT_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        yahtzeeButton = root.Q<UnityEngine.UIElements.Button>(Constants.YAHTZEE_SCORE_BUTTON_NAME);
        if (yahtzeeButton != null)
        {
            yahtzeeButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.YAHTZEE_FORMATION_NAME, yahtzeeButton);
            };
            yahtzeeButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.YAHTZEE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            yahtzeeButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.YAHTZEE_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        bonusButton = root.Q<UnityEngine.UIElements.Button>(Constants.BONUS_SCORE_BUTTON_NAME);
        if (bonusButton != null)
        {
            bonusButton.clickable.clickedWithEventInfo += (evt) =>
            {
                ClickScoreButton(Constants.BONUS_FORMATION_NAME, bonusButton);
            };
            bonusButton.RegisterCallback<MouseEnterEvent>(e => OnHoverScoreButton(true, Constants.BONUS_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
            bonusButton.RegisterCallback<MouseLeaveEvent>(e => OnHoverScoreButton(false, Constants.BONUS_FORMATION_NAME, e.target as UnityEngine.UIElements.Button));
        }

        totalScoreLabel = root.Q<Label>(Constants.TOTAL_SCORE_LABEL_NAME);
        if (totalScoreLabel != null)
        {
            totalScoreLabel.text = Constants.ZERO;
        }
        return null;
    }

    private void ClickScoreButton(string formation, UnityEngine.UIElements.Button button)
    {
        if (_currentRollCount != 4 || _clickedFormationButtons.ContainsKey(button))
        {
            Debug.Log("Click not allowed!");
            return;
        }
        if (_addedScore)
        {
            Debug.Log("Already entered score!");
            return;
        }

        _hoveredButton = null;
        _clickedFormationButtons.Add(button, true);
        AddPointsToTotal(Convert.ToInt32(button.text));
        _scoredFormations++;
        PlayerEndedTurn();  
    }

    public void AddPointsToTotal(int points)
    {
        _addedScore = true;
        totalScoreLabel.text = (Convert.ToInt32(totalScoreLabel.text) + points).ToString();

        base.photonView.RPC(Constants.RPC_UPDATE_SCORES_NAME, RpcTarget.All, PhotonNetwork.LocalPlayer, points);
    }


    public void DisableButtons()
    {
        _rollButton.gameObject.SetActive(false);
        _doneRollingButton.gameObject.SetActive(false);
    }

    public void EnableButtons()
    {
        _rollButton.gameObject.SetActive(true);
        _doneRollingButton.gameObject.SetActive(true);
    }

    private void GetNextPlayerIndex()
    {
        _currentPlayerIndex++;
        _currentPlayerIndex %= _listings.Count;
        _currentPlayer = _listings[_currentPlayerIndex];
    }
    public void OnClick_EndTurn()
    {
        if (!_addedScore)
        {
            Debug.Log("You need to add score");
            return;
        }
        GetNextPlayerIndex();
        PlayerEndedTurn();
    }

    private void ResetPlayer()
    {
        ResetDice();
        _addedScore = false;
        _currentRollCount = 1;
    }

    private void PlayerEndedTurn()
    {
        GetNextPlayerIndex();
        DisableButtons();
        ResetPlayer();
        base.photonView.RPC(Constants.RPC_NEXT_PLAYER_NAME, RpcTarget.All, _currentPlayer.Player);
    }

    private void DisplayWinner(GamePlayerListing winner)
    {
        _gameUICanvas.WinnerText.text = winner.Player.NickName + " won the game with\n" + winner.GetScore() + " points!";
        _gameUICanvas.WinnerPanel.gameObject.SetActive(true);
        
    }
    [PunRPC]
    private void RPC_NextPlayer(Player nextPlayer)
    {
        _currentPlayerIndex = _listings.FindIndex(x => x.Player.NickName == nextPlayer.NickName);
        _currentPlayer = _listings[_currentPlayerIndex];
        if (nextPlayer.NickName == PhotonNetwork.LocalPlayer.NickName)
        {
            if(_scoredFormations == Constants.FORMATION_COUNT)
            {
                base.photonView.RPC(Constants.RPC_GAME_ENDED_NAME, RpcTarget.All);
                return;
            }
            _rollButton.gameObject.SetActive(true);
            _currentRollCount = 1;
            _turnEnded = false;
        }
        else
        {
            DisableButtons();
        }
    }
    [PunRPC]
    private void RPC_GameEnded()
    {
        GamePlayerListing winner = _listings[0];
        for(int i = 1; i < _listings.Count; i++)
        {
            if (_listings[i].GetScore() > winner.GetScore())
                winner = _listings[i];
        }
        DisplayWinner(winner);
    }

    [PunRPC]
    private void RPC_UpdateScores(Player player, int points)
    {
        int index = _gameUICanvas.Listings.FindIndex(x => x.Player.NickName == player.NickName);
        if (index != -1)
        {
            _gameUICanvas.Listings[index].AddScore(points);
        }
    }

    
}
