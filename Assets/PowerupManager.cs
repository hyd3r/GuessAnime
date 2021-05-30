using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupManager : MonoBehaviourPun
{
    public GameObject inventory;
    public string[][] powerup = new string[4][];
    public string[][] attack = new string[3][];
    public List<PowerupDetails> allPowerupDetails;
    public GameObject powerupObject;
    public int powerupLimit = 1;
    public int useItemChildIndex;
    public GameManager gm;
    public PlayerAndScoreHandler pash;
    public GameObject announcePanel;
    public GameObject itemPanel;
    public bool isBoosted = false;
    public bool hasReverse = false;
    public bool hasBarrier = false;
    public bool isNegated = false;
    public bool isTargetingPlayer = false;
    public GameObject targetPanel;
    public string targetPlayerName;
    public GameObject targetPlayerObject;
    public GameObject useButton;
    public string currentItemName;
    public IEnumerator ann=null;
    public GameObject blurPanel;
    public Image blackPanel;
    public char[] symbols;

    void Start()
    {
        powerup[0] = new string[1] { "Skip" };
        powerup[1] = new string[3] { "50/50", "Reverse", "Boost" };
        powerup[2] = new string[3] { "Anime Name", "More Time", "Barrier" };

        attack[0] = new string[2] { "Steal Skip", "Steal Powerup" };
        attack[1] = new string[3] { "Negate", "Reveal Powerups", "Reveal Answer" };
        attack[2] = new string[4] { "Blur Image", "Corrupt Choices", "Lessen Time","Black Screen" };

        string tempstr = "!@#$%^&*()_+-=~";
        symbols = tempstr.ToCharArray();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetPowerup("", "Corrupt Choices");
        }
    }

    public void useItem()
    {
        if (isTargetingPlayer)
        {
            useButton.GetComponent<Button>().interactable = false;
            useButton.transform.GetChild(0).GetComponent<Text>().text = "Use";
            targetPanel.SetActive(false);
            isTargetingPlayer = false;
            for(int l = 0; l < targetPanel.transform.GetChild(0).childCount; l++)
            {
                Destroy(targetPanel.transform.GetChild(0).GetChild(l).gameObject);
            }
            if (currentItemName.Equals("Steal Skip"))
            {
                for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("sendStealSkip", PhotonNetwork.PlayerList[i],PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Steal Powerup"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("sendStealPowerup", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Negate"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveNegate", PhotonNetwork.PlayerList[i],PhotonNetwork.NickName);
                    }
                }

            }
            else if (currentItemName.Equals("Reveal Powerups"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("sendRevealPowerups", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Reveal Answer"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("sendRevealAns", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Blur Image"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveBlurImage", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Corrupt Choices"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveCorruption", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Lessen Time"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveLessTime", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
            else if (currentItemName.Equals("Black Screen"))
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveBlackScreen", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,false);
                    }
                }
            }
        }else if (isNegated)
        {
            isNegated = false;
            powerupLimit = 0;
            Destroy(inventory.transform.GetChild(useItemChildIndex).gameObject);
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, PhotonNetwork.NickName+"'s powerup is negated");
        }
        else if (powerupLimit != 0)
        {
            powerupLimit--;
            currentItemName = inventory.transform.GetChild(useItemChildIndex).GetComponent<Powerup>().powerupName;
            if (currentItemName.Equals("Skip"))
            {
                gm.usedSkip = true;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, PhotonNetwork.NickName+" has used a skip");
            }else if (currentItemName.Equals("50/50"))
            {
                gm.use5050(PhotonNetwork.NickName);
            }
            else if (currentItemName.Equals("Reverse"))
            {
                hasReverse = true;
            }
            else if (currentItemName.Equals("Boost"))
            {
                isBoosted = true;
            }
            else if (currentItemName.Equals("Anime Name"))
            {
                changeItemPanel(gm.currentQuestionData[0]);
            }
            else if (currentItemName.Equals("More Time"))
            {
                if (isBoosted)
                {
                    gm.currentTime = gm.currentTime + (gm.gameTime-(gm.gameTime / 3));
                }
                else
                {
                    gm.currentTime = gm.currentTime + (gm.gameTime / 2);
                }
               
            }
            else if (currentItemName.Equals("Barrier"))
            {
                hasBarrier = true;
            }
            else if (currentItemName.Equals("Steal Skip"))
            {
                targetPlayer("Steal Skip");
            }
            else if (currentItemName.Equals("Steal Powerup"))
            {
                targetPlayer("Steal Powerup");
            }
            else if (currentItemName.Equals("Negate"))
            {
                targetPlayer("Negate");
            }
            else if (currentItemName.Equals("Reveal Powerups"))
            {
                targetPlayer("Reveal Powerups");
            }
            else if (currentItemName.Equals("Reveal Answer"))
            {
                targetPlayer("Reveal Answer");
            }
            else if (currentItemName.Equals("Blur Image"))
            {
                targetPlayer("Blur Image");
            }
            else if (currentItemName.Equals("Corrupt Choices"))
            {
                targetPlayer("Corrupt Choices");
            }
            else if (currentItemName.Equals("Lessen Time"))
            {
                targetPlayer("Lessen Time");
            }
            else if (currentItemName.Equals("Black Screen"))
            {
                targetPlayer("Black Screen");
            }

            Destroy(inventory.transform.GetChild(useItemChildIndex).gameObject);
        }
    }

    public void use5050(string user,int correctAnsIndex)
    {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName.Equals(user))
            {
                photonView.RPC("send5050", PhotonNetwork.PlayerList[i],correctAnsIndex);
                break;
            }
        }
        
    }
    [PunRPC]
    public void send5050(int correctAnsIndex)
    {
        int temp = Random.Range(0, 3);
        while (temp == correctAnsIndex)
        {
            temp = Random.Range(0, 3);
        }
        
        for (int k = 0; k < 4; k++) {
            if (k == temp || k == correctAnsIndex)
            {
                gm.answerButtons[k].GetComponent<Button>().interactable = true;
            }
            else
            {
                gm.answerButtons[k].GetComponent<Button>().interactable = false;
            }
        }
    }
    [PunRPC]
    public void sendStealSkip(string username,bool isReversed)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
            {
                if (hasBarrier)
                {
                    photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Steal Skip on " + PhotonNetwork.NickName+" but was protected by Barrier");
                    hasBarrier = false;
                }
                else if (hasReverse)
                {
                    photonView.RPC("sendStealSkip", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,true);
                    photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Steal Skip on " + PhotonNetwork.NickName);
                    hasReverse = false;
                }else if (isReversed)
                {
                    photonView.RPC("receiveStealSkip", PhotonNetwork.PlayerList[i], gm.usedSkip);
                    photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " has reversed Steal Skip to " + PhotonNetwork.NickName);
                    gm.usedSkip = false;
                }
                else
                {
                    photonView.RPC("receiveStealSkip", PhotonNetwork.PlayerList[i], gm.usedSkip);
                    photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Steal Skip on " + PhotonNetwork.NickName);
                    gm.usedSkip = false;
                }

            }
        }
    }
    [PunRPC]
    public void receiveStealSkip(bool usedSkip)
    {
        gm.usedSkip = usedSkip;
    }

    [PunRPC]
    public void sendStealPowerup(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Steal Powerup on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (inventory.transform.childCount != 0)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                        if (hasReverse)
                        {
                            photonView.RPC("sendStealPowerup", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName,true);
                            hasReverse = false;
                        }
                        else if (isReversed)
                        {
                            int rand = Random.Range(0, inventory.transform.childCount);
                            photonView.RPC("receiveStealPowerup", PhotonNetwork.PlayerList[i], inventory.transform.GetChild(rand).GetComponent<Powerup>().powerupName);
                            Destroy(inventory.transform.GetChild(rand).gameObject); photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used a Reverse amd stole " + PhotonNetwork.NickName + "'s powerup");
                            gm.usedSkip = false;
                        }
                        else
                        {
                            int rand = Random.Range(0, inventory.transform.childCount);
                            photonView.RPC("receiveStealPowerup", PhotonNetwork.PlayerList[i], inventory.transform.GetChild(rand).GetComponent<Powerup>().powerupName);
                            Destroy(inventory.transform.GetChild(rand).gameObject); photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " stole " + PhotonNetwork.NickName + "'s powerup");
                            gm.usedSkip = false;
                        }

                    }
                }
            }
        }
    }
    [PunRPC]
    public void receiveNegate(string username)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Negate on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (hasReverse)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                        photonView.RPC("receiveNegate", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName);
                        photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, PhotonNetwork.NickName + " has reversed Negate");
                        gm.usedSkip = false;
                    }
                }
                hasReverse = false;
            }
            else
            {
                isNegated = true;
            }
        }
    }

    [PunRPC]
    public void receiveStealPowerup(string powerupName)
    {
        GetPowerup("", powerupName);
    }

    [PunRPC]
    public void sendRevealPowerups(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reveal Powerup on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                {
                    if (hasReverse)
                    {
                        hasReverse = false;
                        photonView.RPC("sendRevealPowerups", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, true);
                    }else if (isReversed)
                    {
                        string one = "";
                        string two = "";
                        for (int h = 0; h < inventory.transform.childCount; h++)
                        {
                            if (h == 0)
                            {
                                one = inventory.transform.GetChild(h).GetComponent<Powerup>().powerupName;
                            }
                            else if (h == 1)
                            {
                                two = inventory.transform.GetChild(h).GetComponent<Powerup>().powerupName;
                            }
                        }
                        photonView.RPC("receiveRevealPowerups", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, one, two);
                        photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered,username+ " used Reverse and looked at some powerups of " + PhotonNetwork.NickName);
                    }
                    else
                    {
                        string one = "";
                        string two = "";
                        for (int h = 0; h < inventory.transform.childCount; h++)
                        {
                            if (h == 0)
                            {
                                one = inventory.transform.GetChild(h).GetComponent<Powerup>().powerupName;
                            }
                            else if (h == 1)
                            {
                                two = inventory.transform.GetChild(h).GetComponent<Powerup>().powerupName;
                            }
                        }
                        photonView.RPC("receiveRevealPowerups", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, one, two);
                        photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " looked at some of " + PhotonNetwork.NickName + "'s powerup");
                    }
                    
                }
            }
        }
    }
    [PunRPC]
    public void receiveRevealPowerups(string username,string firstpowerup, string secondpowerup)
    {
        if (firstpowerup.Equals("") && secondpowerup.Equals(""))
        {
            changeItemPanel(username +" doesn't have any powerups");
        }
        else if(secondpowerup.Equals(""))
        {
            changeItemPanel(username+" has a "+firstpowerup);
        }
        else
        {
            changeItemPanel(username + " has a " + firstpowerup+" and a "+secondpowerup);
        }
    }

    [PunRPC]
    public void sendRevealAns(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reveal Answer on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                    if (hasReverse)
                    {
                        hasReverse = false;
                        photonView.RPC("sendRevealAns", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, false);
                    }else if (isReversed)
                    {
                        photonView.RPC("receiveRevealAns", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, gm.selectedIndex);
                        photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reverse and peeked at " + PhotonNetwork.NickName + "'s answer");
                    }
                    else
                    {
                        photonView.RPC("receiveRevealAns", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, gm.selectedIndex);
                        photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " peeked at " + PhotonNetwork.NickName + "'s answer");
                    }
                   
                    }
                }
        }

    }
    [PunRPC]
    public void receiveRevealAns(string username, int selectedIndex)
    {
        if (selectedIndex == -1)
        {
            changeItemPanel(username + " hasn't asnwered yet. Check the scoreboard on the left if the player's border is blue before using Reveal Answer");
        }
        else
        {
            changeItemPanel(username + " has picked " + gm.answerButtonTexts[selectedIndex].text);
        }
    }

    [PunRPC]
    public void receiveBlurImage(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Blur Image on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (hasReverse)
            {
                hasReverse = false;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                        photonView.RPC("receiveBlurImage", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, true);
                    }
                }
            }
            else if (isReversed)
            {
                gm.blurNextRound = true;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reverse and will blur " + PhotonNetwork.NickName + "'s image in the next round");
            }
            else
            {
                gm.blurNextRound = true;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " will blur " + PhotonNetwork.NickName + "'s image in the next round");
            }
        }

    }
    public IEnumerator blurimage(float time)
    {
        blurPanel.SetActive(true);
        yield return new WaitForSeconds(time-(time / 3));
        blurPanel.SetActive(false);
    }

    [PunRPC]
    public void receiveLessTime(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Less Time on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (hasReverse)
            {
                hasReverse = false;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(targetPlayerName))
                    {
                        photonView.RPC("receiveLessTime", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, true);
                    }
                }
            }
            else if (isReversed)
            {
                gm.currentTime = gm.currentTime / 2;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reverse and decreased " + PhotonNetwork.NickName + "'s time");
            }
            else
            {
                gm.currentTime = gm.currentTime / 2;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " decreased " + PhotonNetwork.NickName + "'s time");
            }
        }

    }

    [PunRPC]
    public void receiveBlackScreen(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Black Screen on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (hasReverse)
            {
                hasReverse = false;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                        photonView.RPC("receiveBlackScreen", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, true);
                    }
                }
            }
            else if (isReversed)
            {
                StartCoroutine(BlackScreen());
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reverse and blocked " + PhotonNetwork.NickName + "'s choices for a few seconds");
            }
            else
            {
                StartCoroutine(BlackScreen());
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " has blocked " + PhotonNetwork.NickName + "'s choices for a few seconds");
            }
        }

    }

    public IEnumerator BlackScreen()
    {
        Color temp = blackPanel.color;
        temp.a = 255;
        blackPanel.color = temp;
        yield return new WaitForSeconds(gm.currentTime-(gm.currentTime/3));
        temp.a = 0;
        blackPanel.color = temp;
    }

    [PunRPC]
    public void receiveCorruption(string username,bool isReversed)
    {
        if (hasBarrier)
        {
            photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Corruption on " + PhotonNetwork.NickName + " but was protected by Barrier");
            hasBarrier = false;
        }
        else
        {
            if (hasReverse)
            {
                hasReverse = false;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].NickName.Equals(username))
                    {
                        photonView.RPC("receiveCorruption", PhotonNetwork.PlayerList[i], PhotonNetwork.NickName, true);
                    }
                }
            }
            else if (isReversed)
            {
                gm.corruptNextRound = true;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " used Reverse and will corrupt some letters in " + PhotonNetwork.NickName + "'s choices next round");
            }
            else
            {
                gm.corruptNextRound = true;
                photonView.RPC("AnnounceInfo", RpcTarget.AllBuffered, username + " will corrupt some letters in " + PhotonNetwork.NickName + "'s choices next round");
            }
        }
        
    }
    public void Corrupt()
    {
        for (int r = 0; r < gm.answerButtonTexts.Length; r++)
        {
            string temp = gm.answerButtonTexts[r].text;
            gm.answerButtonTexts[r].text = "";
            for (int i = 0; i < temp.Length; i++)
            {
                if(!temp[i].Equals(' '))
                {
                    if (Random.Range(1, 10) <= 2)
                    {
                        gm.answerButtonTexts[r].text = gm.answerButtonTexts[r].text + symbols[Random.Range(0, symbols.Length)];
                    }
                    else
                    {
                        gm.answerButtonTexts[r].text = gm.answerButtonTexts[r].text + temp[i];
                    }
                }
                else
                {
                    gm.answerButtonTexts[r].text = gm.answerButtonTexts[r].text + temp[i];
                }
            }
        }
    }

    public void dropLoot(List<PlayerAns> playerAns)
    {
        int players = PhotonNetwork.PlayerList.Length;
        int correctPlayers = 0;
        for (int i = 0; i < playerAns.Count; i++)
        {
            if (playerAns[i].isCorrect == true)
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
        else if (players == 3)
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
        else if (players == 2)
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
        int i = Random.Range(1, 10);
        if (isBoosted)
        {
            i = Random.Range(1, 7);
            isBoosted = false;
        }
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
        if (isBoosted)
        {
            i = Random.Range(1, 6);
            isBoosted = false;
        }
        if (i <= 1)
        {
            return (powerup[0][0]);
        }
        else if (i <= 3)
        {
            return (powerup[1][Random.Range(0, 2)]);
        }
        else if (i <= 10)
        {
            return (powerup[2][Random.Range(0, 2)]);
        }
        else return "";
    }

    [PunRPC]
    public void GetPowerup(string username, string powerup)
    {
        if (inventory.transform.childCount < 5)
        {
            GameObject temp = Instantiate(powerupObject, inventory.transform);
            for (int i = 0; i < allPowerupDetails.Count; i++)
            {
                if (allPowerupDetails[i].powerupName.Equals(powerup))
                {
                    temp.GetComponent<Powerup>().powerupName = allPowerupDetails[i].powerupName;
                    temp.GetComponent<Powerup>().powerupDescription = allPowerupDetails[i].powerupDesc;
                    temp.GetComponent<Powerup>().powerupType = allPowerupDetails[i].powerupType.ToString();
                    temp.GetComponent<Powerup>().powerupImage = allPowerupDetails[i].powerupImage;
                }
            }
        }
    }
    [PunRPC]
    public void AnnounceInfo(string info)
    {
        if (ann != null)
        {
            StopCoroutine(ann);
        }
        ann = AnnouncePopout(info);
        StartCoroutine(ann);
    }
    public IEnumerator AnnouncePopout(string info)
    {
        announcePanel.SetActive(true);
        announcePanel.transform.GetChild(0).GetComponent<Text>().text = info;
        yield return new WaitForSeconds(5);
        announcePanel.SetActive(false);
        ann = null;
    }

    public void changeItemPanel(string desc)
    {
        itemPanel.SetActive(true);
        itemPanel.transform.GetChild(0).GetComponent<Text>().text = desc;
    }

    public void closeItemPanel()
    {
        itemPanel.SetActive(false);
    }

    public void targetPlayer(string powerupName)
    {
        isTargetingPlayer = true;
        targetPanel.SetActive(true);
        for(int i=0;i< PhotonNetwork.PlayerList.Length; i++)
        {
            if (!pash.playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text.Equals(PhotonNetwork.NickName))
            {
                GameObject temp = Instantiate(targetPlayerObject, targetPanel.transform.GetChild(0).transform);
                temp.GetComponent<targetPlayerScript>().setTargetPlayer(pash.playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text, pash.playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite);
            }
        }
    }
}
[System.Serializable]
public class PowerupDetails
{
    public string powerupName;
    public string powerupDesc;
    public PowerupType powerupType;
    public enum PowerupType
    {
        spell,
        trap
    }
    public Sprite powerupImage;

}
