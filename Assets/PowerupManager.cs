using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupManager : MonoBehaviourPun, IPunObservable
{
public GameObject[] powerupInventory = new GameObject[5];
public GameObject inventory;
public string[][] powerup = new string[4][];
public string[][] attack = new string[3][];


public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
        
}

void Start()
{
    powerup[0] = new string[1] { "skip" };
    powerup[1] = new string[3] { "50/50", "reverse", "boost" };
    powerup[2] = new string[1] { "charDesc" };
    powerup[3] = new string[3] { "anime", "moreTime", "barrier" };

    attack[0] = new string[2] { "stealSkip","stealPowerup" };
    attack[1] = new string[3] { "negate", "viewPowerup","revealAns" };
    attack[2] = new string[4] { "pixelatePic", "captchaAns","lessTime","incChoices" };
}


void Update()
{
        
}

public void dropLoot(List<PlayerAns> playerAns)
{
    int players = PhotonNetwork.PlayerList.Length;
    int correctPlayers = 0;
    for (int i=0;i<playerAns.Count;i++)
    {
        if(playerAns[i].isCorrect == true)
        {
            correctPlayers++;
        }
    }
    if (players == 4)
    {
        if (correctPlayers == 0)
        {
            for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
            {
                int rand = Random.Range(1,10);
                if (rand > 1)
                {
                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName,GetAttackName());
                }
                else
                {
                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                }
            }
            }
            if (correctPlayers == 1)
        {
            for (int i = 0; i < playerAns.Count; i++)
            {
                if (playerAns[i].isCorrect == true)
                {
                    for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                    {
                        if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                        {
                            int rand = Random.Range(1, 10);
                            if (rand > 1)
                            {
                                photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                            }
                            else
                            {
                                photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                            }
                            break;
                        }
                    }

                }
                else
                {
                    int k = Random.Range(1, 3);
                    if (k == 1)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (correctPlayers == 2)
        {
            for (int i = 0; i < playerAns.Count; i++)
            {
                if (playerAns[i].isCorrect == true)
                {
                    int k = Random.Range(1, 2);
                    if (k == 1)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int k = Random.Range(1, 2);
                    if (k == 1)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (correctPlayers == 3)
        {
            for (int i = 0; i < playerAns.Count; i++)
            {
                if (playerAns[i].isCorrect == true)
                {
                    int k = Random.Range(1, 3);
                    if (k == 1)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {

                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                break;
                            }
                        }
                        
                }
            }
        }
    }
    else if(players == 3)
        {
            if (correctPlayers == 0)
            {
                for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                {
                    int rand = Random.Range(1, 10);
                    if (rand > 1)
                    {
                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                    }
                    else
                    {
                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                    }
                }
            }
            if (correctPlayers == 1)
            {
                for (int i = 0; i < playerAns.Count; i++)
                {
                    if (playerAns[i].isCorrect == true)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                break;
                            }
                        }

                    }
                    else
                    {
                        int k = Random.Range(1, 2);
                        if (k == 1)
                        {
                            for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                            {
                                if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                                {
                                    int rand = Random.Range(1, 10);
                                    if (rand > 1)
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                    }
                                    else
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (correctPlayers == 2)
            {
                for (int i = 0; i < playerAns.Count; i++)
                {
                    if (playerAns[i].isCorrect == true)
                    {
                        int k = Random.Range(1, 2);
                        if (k == 1)
                        {
                            for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                            {
                                if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                                {
                                    int rand = Random.Range(1, 10);
                                    if (rand > 1)
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                    }
                                    else
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                            for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                            {
                                if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                                {
                                    int rand = Random.Range(1, 10);
                                    if (rand > 1)
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                    }
                                    else
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                    }
                                    break;
                                }
                            }
                        
                    }
                }
            }
        }
        else if(players ==2)
        {
            if (correctPlayers == 0)
            {
                for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                {
                    int rand = Random.Range(1, 10);
                    if (rand > 1)
                    {
                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                    }
                    else
                    {
                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                    }
                }
            }
            if (correctPlayers == 1)
            {
                for (int i = 0; i < playerAns.Count; i++)
                {
                    if (playerAns[i].isCorrect == true)
                    {
                        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                        {
                            if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                            {
                                int rand = Random.Range(1, 10);
                                if (rand > 1)
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                }
                                else
                                {
                                    photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                }
                                break;
                            }
                        }

                    }
                    else
                    {
                        int k = Random.Range(1, 2);
                        if (k == 1)
                        {
                            for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
                            {
                                if (PhotonNetwork.PlayerList[x].NickName.Equals(playerAns[i].username))
                                {
                                    int rand = Random.Range(1, 10);
                                    if (rand > 1)
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetAttackName());
                                    }
                                    else
                                    {
                                        photonView.RPC("GetPowerup", PhotonNetwork.PlayerList[x], PhotonNetwork.PlayerList[x].NickName, GetPowerupName());
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
public string GetAttackName()
{
    int i = Random.Range(1,10);
    if (i <= 2)
    {
        return (attack[0][Random.Range(0, 1)]);
    }
    else if (i <= 5)
    {
        return (attack[1][Random.Range(0, 2)]);
    }
    else if (i <= 10)
    {
        return (attack[2][Random.Range(0, 3)]);
    }
    else return "";
}
public string GetPowerupName()
{
    int i = Random.Range(1, 10);
    if (i <= 1)
    {
        return (powerup[0][0]);
    }
    else if (i <= 3)
    {
        return (powerup[1][Random.Range(0, 2)]);
    }
    else if (i <= 6)
    {
        return (powerup[2][0]);
    }
    else if (i <= 10)
    {
        return (powerup[3][Random.Range(0, 2)]);
    }
    else return "";
}

[PunRPC]
public void GetPowerup(string username,string powerup)
{

}
/*
IEnumerator LerpFunction(float endValue, float duration)
{
    float time = 0;
    float startValue = elementToFade.GetAlpha();

    while (time < duration)
    {
        elementToFade.SetAlpha(Mathf.Lerp(startValue, endValue, time / duration));

        time += Time.deltaTime;
        yield return null;
    }
    elementToFade.SetAlpha(endValue);
} */
}
