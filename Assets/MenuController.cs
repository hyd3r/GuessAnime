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
    public GameObject soloButton;
    public Button[] joinCreateButtons;


    private void Awake()
    {
 
    }

    void Start()
    {
        usernameMenu.SetActive(true);
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        info.text = "Connected to the Server";
        joinCreateButtons[0].interactable = true;
        joinCreateButtons[1].interactable = true;
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
            soloButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
            soloButton.SetActive(false);
        }
    }

    public void SetUserName(bool isOnlineMode)
    {
        PhotonNetwork.NickName = usernameInput.text;
        if (isOnlineMode)
        {
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.ConnectUsingSettings();
            usernameMenu.SetActive(false);
        }
        else
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom("Single Player");
        }

    }


    public void CreateGame()
    {
        PhotonNetwork.CreateRoom(createGameInput.text, new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public void JoinGame()
    {
        if (joinGameInput.text.Equals(""))
        {
            info.text = "Join input field empty.";
        }
        else
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 5;
            roomOptions.PublishUserId = true;
            roomOptions.CleanupCacheOnLeave = false;
            PhotonNetwork.JoinOrCreateRoom(joinGameInput.text, roomOptions, TypedLobby.Default);
        }

    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainGame");
    }


}
