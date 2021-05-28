using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class RoomController : MonoBehaviourPunCallbacks
{
    private string username;
    public Text roomName;
    public GameObject chatPanel, textObject;
    public GameObject playerPanel, playerObject;
    public InputField chatBox;
    PhotonView photonViewRef;
    public GameObject startButton;
    public Sprite[] images;


    public Color playerMessage, info;

    public const byte playercode = 2;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        username = PhotonNetwork.NickName;
        photonViewRef = PhotonView.Get(this);


        if (PhotonNetwork.IsMasterClient)
        {
            SendMessageToChat(" " + username + " has created the " + roomName.text + " room", Message.MessageType.info);
            
        }else startButton.SetActive(false);

        photonViewRef.RPC("LoadPlayerList", RpcTarget.AllBuffered, username);
    }
    [PunRPC]
    void LoadPlayerList(string nname)
    {
        GameObject newPlayer = PhotonNetwork.Instantiate(playerObject.name, new Vector3(playerPanel.transform.position.x, playerPanel.transform.position.y), Quaternion.identity, 0);
        newPlayer.transform.GetChild(1).GetComponent<Text>().text = nname;
        newPlayer.transform.SetParent(playerPanel.transform);
        newPlayer.transform.localScale = new Vector3(1, 1, 1);
        newPlayer.transform.GetChild(0).GetComponent<Image>().sprite = images[0];
        if (!nname.Equals(username))
        {
            newPlayer.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    public void ChangeImage()
    {
        photonViewRef.RPC("changePlayerImage", RpcTarget.AllBuffered, username);
    }
    [PunRPC]
    void changePlayerImage(string user)
    {
        foreach (Transform child in playerPanel.transform)
        {
            if (child.GetChild(1).GetComponent<Text>().text.Equals(user))
            {
                child.GetChild(0).GetComponent<Image>().sprite = images[child.GetComponent<playerInit>().incIndex()];
            }
        }
    }

    void Update()
    {
        if(chatBox.text != "")
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(" "+username + ": " + chatBox.text, Message.MessageType.playerMessage);
                chatBox.text = "";
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                chatBox.ActivateInputField();
        }
    }
    public void sendButton()
    {
        if (chatBox.text != "")
        {
                SendMessageToChat(" " + username + ": " + chatBox.text, Message.MessageType.playerMessage);
                chatBox.text = "";
        }
    }


    public void SendMessageToChat(string text,Message.MessageType messageType)
    {
        photonViewRef.RPC("LoadChat", RpcTarget.AllBuffered, messageType,text);
    }

    Color MessageTypeColor(Message.MessageType messagetype)
    {
        Color color = info;

        switch (messagetype)
        {
            case Message.MessageType.playerMessage:
                color = playerMessage;
                break;
             
        }
        return color;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
        SendMessageToChat(" "+newPlayer.NickName+" has entered the room",Message.MessageType.info);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SendMessageToChat(" " + otherPlayer.NickName + " has left the room", Message.MessageType.info);
            foreach(Transform child in playerPanel.transform)
            {
                if (child.GetChild(1).GetComponent<Text>().text.Equals(otherPlayer.NickName))
                {
                    PhotonNetwork.Destroy(child.gameObject);
                }
            }
            
        }
    }


    [PunRPC]
    void LoadChat(Message.MessageType msg, string txt)
    {
        GameObject newchat = PhotonNetwork.Instantiate(textObject.name, new Vector3(chatPanel.transform.position.x, chatPanel.transform.position.y), Quaternion.identity, 0);
        newchat.transform.SetParent(chatPanel.transform);
        newchat.GetComponent<Text>().color = MessageTypeColor(msg);
        newchat.GetComponent<Text>().text = txt;
        newchat.transform.localScale = new Vector3(1, 1, 1);
    }

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

}

[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
    public MessageType messageType;

    public enum MessageType
    {
        playerMessage,
        info
    }
}