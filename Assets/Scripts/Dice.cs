using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public Rigidbody rigidbody;

    public bool thrown;
    bool hasLanded;
    public bool addedToTray;

    Vector3 initPosition;
    public Vector3 boardPosition;

    public int diceValue;
    public DiceSide[] diceSides;

    public bool _addToTrayOnceLanded = false;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rigidbody.useGravity = true;
        thrown = false;
        hasLanded = false;
        addedToTray = false;
        GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>().AddDiceToBoard(this);
        
    }
    
    void Update()
    {   
        if (rigidbody.IsSleeping() && !hasLanded)
        {
            SideValueCheck();
        }
        else if(hasLanded && !rigidbody.IsSleeping())
        {
            hasLanded = false;
            diceValue = 0;
        }
    }
    public void setThrown(bool thrown)
    {
        
        this.thrown = thrown;
    }

   
    public void setVisibleSide(int side)
    {
        if(side == 6)
        {
            diceValue = 6;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if(side == 1)
        {
            diceValue = 1;
            transform.eulerAngles = new Vector3(-180, 0, 0);
        }
        else if(side == 2)
        {
            diceValue = 2;   
            transform.eulerAngles = new Vector3(90, 0, -90);
        }
        else if(side == 5)
        {
            diceValue = 5;
            transform.eulerAngles = new Vector3(270, 0, -90);
        }
        else if(side == 4)
        {
            diceValue = 4;
            transform.eulerAngles = new Vector3(360, 0, -90);
        }
        else if(side == 3)
        {
            diceValue = 3;
            transform.eulerAngles = new Vector3(180, 0, -90);
        }

        rigidbody.useGravity = true;
    }


    void Roll()
    {
        if(!thrown && !hasLanded)
        {
            thrown = true;
            rigidbody.useGravity = true;
            rigidbody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
        }
    }

    public void RollAgain()
    {
        rigidbody.transform.position = GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>()._initialDicePosition;
        rigidbody.AddForce(Random.Range(-5, 5), Random.Range(-10, 5), Random.Range(5, 20), ForceMode.Impulse);
        rigidbody.AddTorque(new Vector3(Random.Range(1, 100), Random.Range(1, 100), Random.Range(1, 100)));
    }

    void SideValueCheck()
    {
        int value = 0;
        foreach(DiceSide side in diceSides)
        {
            if (side.IsOnGround())
            {
                value = side.sideValue;
                if(value != diceValue) diceValue = value;
                boardPosition = transform.position;
                hasLanded = true;
                if(GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>()._currentRollCount == 4 || _addToTrayOnceLanded)
                {
                    rigidbody.useGravity = true;
                    GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>().AddDiceToTray(this);
                    GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>().DisableButtons();
                    addedToTray = true;
                }
            }
            else if (side.IsInTray())
            {
                diceValue = side.sideValue;
            }
        }
        
    }

    private void OnMouseDown()
    {
        if (addedToTray)
        {
            transform.position = boardPosition;
            addedToTray = false;
            GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>().RemoveDiceFromTray(this);
            return;
        }
        else if (rigidbody.IsSleeping()) 
        {
            if (hasLanded)
            {
                if (!addedToTray)
                {
                    if (diceValue == 0)
                    {
                        RollAgain();
                    }
                    else
                    {
                        rigidbody.useGravity = true;
                        GameObject.Find(Constants.GAME_MANAGER_TAG).GetComponent<GameManager>().AddDiceToTray(this);
                        addedToTray = true;
                    }
                }
            }
            else RollAgain();
        }
        
    }
}
