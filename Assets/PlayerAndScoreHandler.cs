using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAndScoreHandler : MonoBehaviourPun, IPunObservable
{
    public GameObject[] playerScoreBoard;
    public GameObject playerList;
    public Sprite[] images;
    public bool isUpdating = false;
    public List<Color> color = new List<Color>();

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (!playerScoreBoard[i].activeInHierarchy)
                {
                    playerScoreBoard[i].SetActive(true);
                    stream.SendNext(playerScoreBoard[i].activeInHierarchy);
                }
                if (playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text.Equals(""))
                {
                    playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
                    stream.SendNext(playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text);
                }
                if (playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite == null)
                {
                    for (int x = 0; x < playerList.transform.childCount; x++)
                    {
                        if (playerList.transform.GetChild(x).GetChild(1).GetComponent<Text>().text.Equals(playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text))
                        {
                            playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite = images[playerList.transform.GetChild(x).GetComponent<playerInit>().imageIndex];
                            stream.SendNext(playerList.transform.GetChild(x).GetComponent<playerInit>().imageIndex);
                        }
                    }
                }
                if (playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text.Equals(""))
                {
                    playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text = "0";
                    stream.SendNext("0");
                }
                stream.SendNext(isUpdating);
                if (isUpdating)
                {
                    for (int k = 0; k < PhotonNetwork.PlayerList.Length; k++)
                    {
                        stream.SendNext(ColorUtility.ToHtmlStringRGBA(playerScoreBoard[k].GetComponent<Image>().color));
                        stream.SendNext(playerScoreBoard[k].transform.GetChild(2).GetComponent<Text>().text);
                    }
                }
            }

        }
        else if (stream.IsReading)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (!playerScoreBoard[i].activeInHierarchy)
                {
                    playerScoreBoard[i].SetActive((bool)stream.ReceiveNext());
                }
                if (playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text.Equals(""))
                {
                    playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text = (string)stream.ReceiveNext();
                }
                if (playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite == null)
                {
                    playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite = images[(int)stream.ReceiveNext()];
                }
                if (playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text.Equals(""))
                {
                    playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text = (string)stream.ReceiveNext();
                }
                isUpdating = (bool)stream.ReceiveNext();
                if (isUpdating)
                {
                    for (int k = 0; k < PhotonNetwork.PlayerList.Length; k++)
                    {
                        Color temp;
                        ColorUtility.TryParseHtmlString((string)stream.ReceiveNext(), out temp);
                        playerScoreBoard[k].GetComponent<Image>().color = temp;
                        playerScoreBoard[k].transform.GetChild(2).GetComponent<Text>().text= (string)stream.ReceiveNext();
                    }
                }
            }

        }
    }

    void OnEnable()
    {
        for (int i = 0; i < 4; i++)
        {
            playerScoreBoard[i].SetActive(false);
            playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text = "";
            playerScoreBoard[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
            playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text = "";
        }
    }

    public void IsWaitingforNextRound()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playerScoreBoard[i].GetComponent<Image>().color = color[0];
        }
    }
    public void UpdateScoreboard(List<PlayerAns> playerAns)
    {
        isUpdating = true;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            for (int x = 0; x < playerAns.Count; x++)
            {
                if (playerAns[x].username.Equals(playerScoreBoard[i].transform.GetChild(1).GetComponent<Text>().text))
                {
                    if (playerAns[x].isCorrect)
                    {
                        playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text = int.Parse(playerScoreBoard[i].transform.GetChild(2).GetComponent<Text>().text)+1+"";
                        playerScoreBoard[i].GetComponent<Image>().color = color[2];
                        if (i != 0)
                        {
                            int curri = i;
                            int tempi = i-1;
                            while (int.Parse(playerScoreBoard[curri].transform.GetChild(2).GetComponent<Text>().text)> int.Parse(playerScoreBoard[tempi].transform.GetChild(2).GetComponent<Text>().text))
                            {
                                GameObject tmp = playerScoreBoard[tempi];
                                playerScoreBoard[tempi] = playerScoreBoard[curri];
                                playerScoreBoard[curri] = tmp;

                                Vector3 tmpPos = playerScoreBoard[tempi].transform.localPosition;
                                playerScoreBoard[tempi].transform.localPosition = playerScoreBoard[curri].transform.localPosition;
                                playerScoreBoard[curri].transform.localPosition = tmpPos;

                                curri--;
                                tempi--;
                                if (curri == 0) break;
                            }
                        }
                    }
                    else
                    {
                        playerScoreBoard[i].GetComponent<Image>().color = color[1];
                    }
                }
            }
        }
    }
}

    public class PlayerAns
    {
        public string username;
        public bool isCorrect;
        public PlayerAns(string username, bool isCorrect)
        {
            this.username = username;
            this.isCorrect = isCorrect;
        }
    }
