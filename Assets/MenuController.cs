using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject usernameMenu;
    [SerializeField] private GameObject connectPanel;
    
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private InputField createGameInput;
    [SerializeField] private InputField joinGameInput;
    [SerializeField] private Text info;

    [SerializeField] private GameObject startButton;


    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    void Start()
    {
        usernameMenu.SetActive(true);
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        info.text = "Connected to the Server";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        info.text = message;
    }


    public void ChangedUserNameInput()
    {
        if(usernameInput.text.Length >= 3)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void SetUserName()
    {
        usernameMenu.SetActive(false);
        PhotonNetwork.NickName = usernameInput.text;
    }

    public void CreateGame()
    {
        PhotonNetwork.CreateRoom(createGameInput.text, new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public void JoinGame()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        roomOptions.PublishUserId = true;
        roomOptions.CleanupCacheOnLeave = false;
        PhotonNetwork.JoinOrCreateRoom(joinGameInput.text, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainGame");
    }


}
