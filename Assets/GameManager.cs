using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphQlClient.Core;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Text syncButtonText;
    public Button syncButton;
    public InputField syncInput;
    public GraphApi aniListAPI;
    public List<AnimeList> animelist = new List<AnimeList>();
    public List<AnimeList> charaList;
    public string[] currentQuestionData = new string[8];
    //public Root myDeserializedClass = new Root();
    public List<Root> parseList = new List<Root>();
    PhotonView photonView;
    public GameObject gameScreen;
    public Text guessTimeText;
    public Text questionAmountText;
    public Slider timer;
    public float gameTime;
    private float currentTime = 10f;
    private bool playTimer = false;
    public int questions=10;
    public int currentQuestion = 1;
    public Image gameImage;



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
        //PhotonNetwork.OfflineMode = true;
    }

    void Update()
    {
        if (playTimer)
        {
            currentTime -= Time.deltaTime;
            timer.value = currentTime / gameTime;
        }
    }
    public void SyncAnilist()
    {
        if (syncInput.text.Length == 0) animelist.Clear();
        else
        {
            syncButton.interactable = false;
            syncButtonText.text = "Fetching data...";
            photonView.RPC("GetAnimeLists", RpcTarget.MasterClient, syncInput.text, PhotonNetwork.NickName);
        }
    }

    [PunRPC]
    public async void GetAnimeLists(string inputname, string playernn)
    {
        GraphApi.Query createUser = aniListAPI.GetQueryByName("GetAnimeLists", GraphApi.Query.Type.Query);
        createUser.SetArgs(new { userName = inputname, type = MediaType.ANIME, sort = MediaListSort.SCORE_DESC, status = MediaListStatus.COMPLETED, forceSingleCompletedList = true });
        UnityWebRequest request = await aniListAPI.Post(createUser);
        parseList.Add(JsonConvert.DeserializeObject<Root>(request.downloadHandler.text));
        int index = 99;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName.Equals(playernn))
            {
                index = i;
            }
        }
        if (request.responseCode == 200)
        {
            photonView.RPC("SyncFin", PhotonNetwork.PlayerList[index], true);
            //for (int i = 0; i < myDeserializedClass.Data.MediaListCollection.Lists[0].Entries.Count; i++)
            
        }
        else photonView.RPC("SyncFin", PhotonNetwork.PlayerList[index], false);
    }
    [PunRPC]
    private void SyncFin(bool success)
    {
        if (success)
        {
            syncInput.text = "";
            syncButton.interactable = true;
            syncButtonText.text = "Success";
        }
        else syncButtonText.text = "Invalid";
    }

    public void changeSyncButton()
    {
        if (syncInput.text.Length != 0)
        {
            syncButtonText.text = "Add";
        }
        else
        {
            syncButtonText.text = "Clear";
        }
    }
    
    public void IncDecGuessTime(bool toInc)
    {
        if (toInc) gameTime++;
        else gameTime--;
        photonView.RPC("changeGuessTime", RpcTarget.AllBuffered,gameTime);
    }
    public void IncDecQuestions(bool toInc)
    {
        if (toInc) questions++;
        else questions--;
        photonView.RPC("changeQuestionAmount", RpcTarget.AllBuffered, questions);
    }

    [PunRPC]
    void changeGuessTime(float newTime)
    {
        gameTime = newTime;
        guessTimeText.text = newTime.ToString();
    }

    [PunRPC]
    void changeQuestionAmount(int newQuestionAmount)
    {
        questions = newQuestionAmount;
        questionAmountText.text = newQuestionAmount.ToString();
    }

    public void StartButton()
    {
        charaList = new List<AnimeList>(questions);
        for (int i = 0; i < questions; i++)
        {
            int randomNum1 = Random.Range(0, parseList.Count);
            MediaListCollection mediacol = parseList[randomNum1].Data.MediaListCollection;
            int randomNum2 = Random.Range(0, mediacol.Lists[0].Entries.Count);
            Media parse = mediacol.Lists[0].Entries[randomNum2].Media;
            int randomCharacter = Random.Range(0, parse.Characters.Nodes.Count);
            while (parse.Characters.Nodes[randomCharacter].Description == null|| parse.BannerImage == null)
            {
                randomCharacter = Random.Range(0, parse.Characters.Nodes.Count);
            }
            charaList.Add(new AnimeList(parse.Title.English, parse.Description, parse.CoverImage.Large, parse.BannerImage, parse.Genres, parse.Characters.Nodes[randomCharacter].Name.Full, parse.Characters.Nodes[randomCharacter].Description, parse.Characters.Nodes[randomCharacter].Image.Large));
        }
        AddQuestionData();
        photonView.RPC("StartGame", RpcTarget.All, currentQuestionData);
    }

    public void AddQuestionData()
    {
        currentQuestionData = new string[8];
        currentQuestionData[0] = charaList[currentQuestion].title;
        currentQuestionData[1] = charaList[currentQuestion].description;
        currentQuestionData[2] = charaList[currentQuestion].coverImage;
        currentQuestionData[3] = charaList[currentQuestion].bannerImage;
        currentQuestionData[4] = charaList[currentQuestion].genre;
        currentQuestionData[5] = charaList[currentQuestion].charaName;
        currentQuestionData[6] = charaList[currentQuestion].charaDescription;
        currentQuestionData[7] = charaList[currentQuestion].charaImage;
    }


    [PunRPC]
    public void StartGame(string[] currentQuestionData)
    {
        this.currentQuestionData = new string[8];
        this.currentQuestionData = currentQuestionData;
        StartCoroutine(DownloadImage(currentQuestionData[7]));
        gameScreen.SetActive(true);
        currentTime = gameTime;
        playTimer = true;
    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            // ImageComponent.texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

            Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
            gameImage.SetNativeSize();
            gameImage.overrideSprite = sprite;
        }
    }


}
///////////////////////////////
///////////////////////////
///////////////////////
///
[System.Serializable]
public class AnimeList
{
    public string title;
    public string description;
    public string coverImage;
    public string bannerImage;
    public string genre;
    public string charaName;
    public string charaDescription;
    public string charaImage;

    public AnimeList(string title, string description, string coverImage, string bannerImage, List<string> genre, string charaName,string charaDescription, string charaImage)
    {
        this.title = title;
        this.description = description;
        this.coverImage = coverImage;
        this.bannerImage = bannerImage;
        for (int i = 0; i < genre.Count; i++)
        {
            this.genre += genre[i]+" ";
        }
        this.charaName = charaName;
        this.charaDescription = charaDescription;
        this.charaImage = charaImage;
    }

}


/////////////////////////////
/////////////////////////////
///

public class CoverImage
{
    [JsonProperty("large")]
    public string Large;
}

public class Title
{
    [JsonProperty("english")]
    public string English;
}

public class Name
{
    [JsonProperty("full")]
    public string Full;
}

public class Imagedata
{
    [JsonProperty("large")]
    public string Large;
}

public class Node
{
    [JsonProperty("name")]
    public Name Name;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("image")]
    public Imagedata Image;
}

public class Characters
{
    [JsonProperty("nodes")]
    public List<Node> Nodes;
}

public class Media
{
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("coverImage")]
    public CoverImage CoverImage;

    [JsonProperty("bannerImage")]
    public string BannerImage;

    [JsonProperty("genres")]
    public List<string> Genres;

    [JsonProperty("title")]
    public Title Title;

    [JsonProperty("characters")]
    public Characters Characters;
}

public class Entry
{
    [JsonProperty("media")]
    public Media Media;
}

public class Listdata
{
    [JsonProperty("entries")]
    public List<Entry> Entries;
}

public class MediaListCollection
{
    [JsonProperty("lists")]
    public List<Listdata> Lists;
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

