using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    bool onGround;
    bool inTray;
    public int sideValue;

    private void OnTriggerStay(Collider collider)
    {
        if(collider.tag == Constants.BOARD_FLOOR_TAG)
        {
            onGround = true;
            inTray = false;
        }
        else if(collider.tag == Constants.TRAY_FLOOR_TAG)
        {
            inTray = true;
            onGround = false;
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        if(collider.tag == Constants.BOARD_FLOOR_TAG)
        {
            onGround = false;
        }
        else if(collider.tag == Constants.TRAY_FLOOR_TAG)
        {
            inTray = false;
        }
    }

    public bool IsOnGround()
    {
        return onGround;
    }

    public bool IsInTray()
    {
        return inTray;
    }

}
