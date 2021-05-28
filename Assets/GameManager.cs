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
    public List<string> charaList = new List<string>();
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
    public int currentQuestion = 0;
    public Image gameImage;
    public Text[] answerButtonTexts = new Text[4];
    public int correctAnsIndex;
    private string[] answerList = new string[4];
    public GameObject pash;
    public List<PlayerAns> playerAns = new List<PlayerAns>();
    private bool isWaitingforNextRound = false;
    public Color[] colors;
    private bool answered = false;
    private int selectedIndex;
    public GameObject[] answerButtons;


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
            if (timer.value <= 0f)
            {
                SelectAnswer(99);
                playTimer = false;
                answered = true;
            }
        }
        if (answered)
        {
            for (int l = 0; l <answerButtons.Length;l++)
            {
                answerButtons[l].GetComponent<Button>().interactable = false;
                if (l == selectedIndex)
                {
                    answerButtons[l].GetComponent<Image>().color = colors[3];
                }
            }
        }
        if (isWaitingforNextRound)
        {
            currentTime += Time.deltaTime;
            timer.value = currentTime / 5;

            if (selectedIndex == correctAnsIndex)
            {
                answerButtons[selectedIndex].GetComponent<Image>().color = colors[2];
            }
            else
            {
                answerButtons[correctAnsIndex].GetComponent<Image>().color = colors[2];
                if (selectedIndex != 99) 
                {
                    answerButtons[selectedIndex].GetComponent<Image>().color = colors[1];
                }
            }

            if (timer.value >= 1)
            {
                isWaitingforNextRound = false;
                if (PhotonNetwork.IsMasterClient)
                {
                    currentQuestion++;
                    if (currentQuestion != questions)
                    {
                        this.currentQuestionData = new string[8];
                        this.answerList = new string[4];
                        AddQuestionData();
                        correctAnsIndex = Random.Range(0, 3);
                        answerList[correctAnsIndex] = currentQuestionData[5];
                        for (int i = 0; i < 4; i++)
                        {
                            if (i != correctAnsIndex)
                            {
                                answerList[i] = charaList[Random.Range(0, charaList.Count - 1)];
                            }
                        }
                    }
                    photonView.RPC("StartNextRound", RpcTarget.AllBuffered,currentQuestion, currentQuestionData, answerList);
                }
            }
        }
    }
    public void SyncAnilist()
    {
        if (syncInput.text.Length == 0) parseList.Clear();
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
                Debug.Log(PhotonNetwork.PlayerList[i].NickName);
                index = i;
            }
        }
        if (request.responseCode == 200)
        {
            photonView.RPC("SyncFin", PhotonNetwork.PlayerList[index], true);
            AddtoCharaList(parseList.Count-1);
            
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

    public void AddtoCharaList(int index)
    {
        for(int i=0; i < parseList[index].Data.MediaListCollection.Lists[0].Entries.Count-1; i++)
        {
            for (int w = 0; w < parseList[index].Data.MediaListCollection.Lists[0].Entries[i].Media.Characters.Nodes.Count-1; w++)
            {
                charaList.Add(parseList[index].Data.MediaListCollection.Lists[0].Entries[i].Media.Characters.Nodes[w].Name.Full);
            }
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
        animelist = new List<AnimeList>(questions);
        for (int i = 0; i < questions; i++)
        {
            int randomNum1 = Random.Range(0, parseList.Count-1);
            MediaListCollection mediacol = parseList[randomNum1].Data.MediaListCollection;
            int randomNum2 = Random.Range(0, mediacol.Lists[0].Entries.Count-1);
            Media parse = mediacol.Lists[0].Entries[randomNum2].Media;
            int randomCharacter = Random.Range(0, parse.Characters.Nodes.Count-1);
            if(parse.Characters.Nodes[randomCharacter].Description == null|| parse.BannerImage == null) 
            {
                i--;
            }
            else { 
            animelist.Add(new AnimeList(parse.Title.English, parse.Description, parse.CoverImage.Large, parse.BannerImage, parse.Genres, parse.Characters.Nodes[randomCharacter].Name.Full, parse.Characters.Nodes[randomCharacter].Description, parse.Characters.Nodes[randomCharacter].Image.Large));
            }
        }
        AddQuestionData();
        correctAnsIndex = Random.Range(0, 3);
        answerList[correctAnsIndex] = currentQuestionData[5];
        for (int i = 0; i < 4; i++)
        {
            if (i != correctAnsIndex)
            {
                answerList[i] = charaList[Random.Range(0, charaList.Count - 1)];
            }
        }
        photonView.RPC("StartGame", RpcTarget.AllBuffered, currentQuestionData,answerList);
    }

    public void AddQuestionData()
    {
        currentQuestionData = new string[8];
        currentQuestionData[0] = animelist[currentQuestion].title;
        currentQuestionData[1] = animelist[currentQuestion].description;
        currentQuestionData[2] = animelist[currentQuestion].coverImage;
        currentQuestionData[3] = animelist[currentQuestion].bannerImage;
        currentQuestionData[4] = animelist[currentQuestion].genre;
        currentQuestionData[5] = animelist[currentQuestion].charaName;
        currentQuestionData[6] = animelist[currentQuestion].charaDescription;
        currentQuestionData[7] = animelist[currentQuestion].charaImage;
    }


    [PunRPC]
    public void StartGame(string[] currentQuestionData,string[] answerList)
    {
        this.currentQuestionData = new string[8];
        this.currentQuestionData = currentQuestionData;
        this.answerList = new string[4];
        this.answerList = answerList;
        StartCoroutine(DownloadImage(currentQuestionData[7]));
        for(int x = 0; x < 4; x++)
        {
            answerButtonTexts[x].text = answerList[x];
        }
        
        gameScreen.SetActive(true);
        currentTime = gameTime;
        playTimer = true;
        pash.SetActive(true);
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

    public void SelectAnswer(int buttonIndex)
    {
        if (playTimer)
        {
            selectedIndex = buttonIndex;
            answered = true;
            photonView.RPC("SendAnsToPASH", RpcTarget.MasterClient, PhotonNetwork.NickName, buttonIndex);
        }
    }

    [PunRPC]
    void SendAnsToPASH(string user, int buttonIndex)
    {
        if (correctAnsIndex == buttonIndex) playerAns.Add(new PlayerAns(user, true));
        else playerAns.Add(new PlayerAns(user, false));

        if (PhotonNetwork.PlayerList.Length == playerAns.Count)
        {
            pash.GetComponent<PlayerAndScoreHandler>().UpdateScoreboard(playerAns);
            playerAns.Clear();
            photonView.RPC("WaitforNextRound", RpcTarget.AllBuffered,correctAnsIndex);
        }
    }
    [PunRPC]
    void WaitforNextRound(int correctAns)
    {
        timer.value = 0;
        currentTime = 0;
        isWaitingforNextRound = true;
        playTimer = false;
        answered = false;
        this.correctAnsIndex = correctAns;
    }

    [PunRPC]
    void StartNextRound(int currentQuestionNum, string[] currentQuestionData, string[] answerList)
    {
        currentQuestion = currentQuestionNum;
        if (currentQuestionNum == questions)
        {
            gameScreen.SetActive(false);
            playTimer = false;
            pash.SetActive(false);
            isWaitingforNextRound = false;
            currentQuestion = 0;
        }
        else
        {
            this.currentQuestionData = new string[8];
            this.currentQuestionData = currentQuestionData;
            this.answerList = new string[4];
            this.answerList = answerList;
            StartCoroutine(DownloadImage(currentQuestionData[7]));
            for (int t = 0; t < answerButtons.Length; t++)
            {
                answerButtons[t].GetComponent<Button>().interactable = true;
                answerButtons[t].GetComponent<Image>().color = colors[0];
                answerButtonTexts[t].text = answerList[t];
            }
            playTimer = true;
            currentTime = gameTime;
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

