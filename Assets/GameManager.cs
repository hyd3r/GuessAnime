using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphQlClient.Core;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Text syncButton;
    public InputField syncInput;
    public GraphApi aniListAPI;
    public List<int> animelist;
    public Root myDeserializedClass = new Root();
    PhotonView photonView;
    public GameObject gameScreen;
    public GameObject startButton;

    enum MediaType
        {
            ANIME,
            MANGA
        }
    enum MediaListSort
    {
        MEDIA_ID,
MEDIA_ID_DESC,
SCORE,
SCORE_DESC,
STATUS,
STATUS_DESC,
PROGRESS,
PROGRESS_DESC,
PROGRESS_VOLUMES,
PROGRESS_VOLUMES_DESC,
REPEAT,
REPEAT_DESC,
PRIORITY,
PRIORITY_DESC,
STARTED_ON,
STARTED_ON_DESC,
FINISHED_ON,
FINISHED_ON_DESC,
ADDED_TIME,
ADDED_TIME_DESC,
UPDATED_TIME,
UPDATED_TIME_DESC,
MEDIA_TITLE_ROMAJI,
MEDIA_TITLE_ROMAJI_DESC,
MEDIA_TITLE_ENGLISH,
MEDIA_TITLE_ENGLISH_DESC,
MEDIA_TITLE_NATIVE,
MEDIA_TITLE_NATIVE_DESC,
MEDIA_POPULARITY,
MEDIA_POPULARITY_DESC

    }
    enum MediaListStatus
    {
        CURRENT,
PLANNING,
COMPLETED,
DROPPED,
PAUSED,
REPEATING

    }

    void Start()
        {
        photonView = PhotonView.Get(this);
        //animelist = new List<int>();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false)''
        }
    }

        void Update()
        {
           
        }
    public void SyncAnilist()
    {
        if (syncInput.text.Length == 0) animelist.Clear();
        else
        {
            photonView.RPC("GetAnimeLists", RpcTarget.MasterClient,syncInput.text,PhotonNetwork.NickName);
        }
    }

    [PunRPC]
    public async void GetAnimeLists(string inputname,string playernn)
        {
        GraphApi.Query createUser = aniListAPI.GetQueryByName("GetAnimeLists", GraphApi.Query.Type.Query);
        createUser.SetArgs(new { userName = inputname, type = MediaType.ANIME, sort = MediaListSort.SCORE_DESC, status = MediaListStatus.COMPLETED, forceSingleCompletedList = true });
        UnityWebRequest request = await aniListAPI.Post(createUser);
        myDeserializedClass = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
        int index=99;
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName.Equals(playernn))
            {
                index = i;
            }
        }
        if (request.responseCode == 200)
        {
            photonView.RPC("SyncFin", PhotonNetwork.PlayerList[index], true);
            int num;
            for (int i = 0; i < myDeserializedClass.Data.MediaListCollection.Lists[0].Entries.Count; i++)
            {
                num = myDeserializedClass.Data.MediaListCollection.Lists[0].Entries[i].MediaId;
                if (!animelist.Contains(num))
                    animelist.Add(num);
            }
        }
        else photonView.RPC("SyncFin", PhotonNetwork.PlayerList[index], false);
    }
    [PunRPC]
    private void SyncFin(bool success)
    {
            if (success)
            {
                syncInput.text = "";
                syncButton.text = "Success";
            }
            else syncButton.text = "Invalid";
    }
    
    public void changeSyncButton()
    {
        if (syncInput.text.Length != 0)
        {
            syncButton.text = "Add";
        }
        else
        {
            syncButton.text = "Clear";
        }
    }

    public void StartGame()
    {
        gameScreen.SetActive(true);

    }


    }


public class Entry
{
    [JsonProperty("mediaId")]
    public int MediaId;
}

public class Lists
{
    [JsonProperty("entries")]
    public List<Entry> Entries;
}

public class MediaListCollection
{
    [JsonProperty("lists")]
    public List<Lists> Lists;
}

public class Data
{
    [JsonProperty("MediaListCollection")]
    public MediaListCollection MediaListCollection;
}

public class Root
{
    [JsonProperty("data")]
    public Data Data;
}





