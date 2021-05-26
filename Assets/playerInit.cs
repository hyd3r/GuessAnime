using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class playerInit : MonoBehaviourPunCallbacks
{
    public int imageIndex = 0;
    public int incIndex()
    {
        if (imageIndex <= 7)
        {
            imageIndex++;
        }
        else
        {
            imageIndex = 0;
        }
        return imageIndex;
    }
    public void changeImg()
    {
        GameObject.Find("RoomController").GetComponent<RoomController>().ChangeImage();
    }
}
