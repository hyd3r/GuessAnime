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
    public List<Root> parseList = new List<Root>();
    PhotonView photonViewRef;
    public GameObject gameScreen;
    public Text guessTimeText;
    public Text questionAmountText;
    public Slider timer;
    public float gameTime;
    private float currentTime = 10f;
    public int questions=10;
    public int currentQuestion = 0;
    public Image gameImage;
    public Text[] answerButtonTexts = new Text[4];
    public int correctAnsIndex;
    private string[] answerList = new string[4];
    public GameObject pash;
    public List<PlayerAns> playerAns = new List<PlayerAns>();
    public Color[] colors;
    private int selectedIndex;
    public GameObject[] answerButtons;
    public gameStateType gameState = gameStateType.stop;
    public RoomController rc;
    
    public enum gameStateType
    {
        stop,
        playing,
        answered,
        waiting
    }

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
        photonViewRef = PhotonView.Get(this);
    }

    void Update()
    {
        switch (gameState)
        {
            case gameStateType.stop:
                break;
            case gameStateType.playing:
                currentTime -= Time.deltaTime;
                timer.value = currentTime / gameTime;
                if (timer.value <= 0f)
                {
                    SelectAnswer(99);
                }
                break;
            case gameStateType.answered:
                currentTime -= Time.deltaTime;
                timer.value = currentTime / gameTime;
                for (int l = 0; l < 4; l++)
                {
                    answerButtons[l].GetComponent<Button>().interactable = false;
                    if (l == selectedIndex)
                    {
                        answerButtons[l].GetComponent<Image>().color = colors[3];
                    }
                    else
                    {
                        answerButtons[l].GetComponent<Image>().color = colors[0];
                    }
                }
                break;
            case gameStateType.waiting:
                currentTime += Time.deltaTime;
                timer.value = currentTime / 5;

                if (selectedIndex == correctAnsIndex)
                {
                    answerButtons[selectedIndex].GetComponent<Image>().color = colors[2];
                }
                else
                {
                    for(int a = 0; a < 4; a++)
                    {
                        if (a == correctAnsIndex)
                        {
                            answerButtons[a].GetComponent<Image>().color = colors[2];
                        }else if (a == selectedIndex)
                        {
                            answerButtons[a].GetComponent<Image>().color = colors[1];
                        }
                        else
                        {
                            answerButtons[a].GetComponent<Image>().color = colors[0];
                        }
                    }
                }

                if (timer.value >= 1)
                {
                    gameState = gameStateType.stop;
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
                        photonViewRef.RPC("StartNextRound", RpcTarget.AllBuffered, currentQuestion, currentQuestionData, answerList);
                    }
                }
                break;
            default:
                break;
        }
    }
    public void SyncAnilist()
    {
        if (syncInput.text.Length == 0) parseList.Clear();
        else
        {
            syncButton.interactable = false;
            syncButtonText.text = "Fetching data...";
            rc.SendMessageToChat(" Fetching "+syncInput.text+"'s AniList data for "+PhotonNetwork.NickName+". Please wait...",Message.MessageType.info);
            photonViewRef.RPC("GetAnimeLists", RpcTarget.MasterClient, syncInput.text, PhotonNetwork.NickName);
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
            photonViewRef.RPC("SyncFin", PhotonNetwork.PlayerList[index], true);
            AddtoCharaList(parseList.Count-1);
            
        }
        else photonViewRef.RPC("SyncFin", PhotonNetwork.PlayerList[index], false);
    }
    [PunRPC]
    public void SyncFin(bool success)
    {
        if (success)
        {
            rc.SendMessageToChat(" "+PhotonNetwork.NickName + "'s Sync successfull!", Message.MessageType.info);
            syncInput.text = "";
            syncButton.interactable = true;
            syncButtonText.text = "Success";
        }
        else
        {
            rc.SendMessageToChat(" "+PhotonNetwork.NickName + "'s Sync failed!", Message.MessageType.info);
            syncButtonText.text = "Invalid";
        }
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
        photonViewRef.RPC("changeGuessTime", RpcTarget.AllBuffered,gameTime);
    }
    public void IncDecQuestions(bool toInc)
    {
        if (toInc) questions++;
        else questions--;
        photonViewRef.RPC("changeQuestionAmount", RpcTarget.AllBuffered, questions);
    }

    [PunRPC]
    public void changeGuessTime(float newTime)
    {
        gameTime = newTime;
        guessTimeText.text = newTime.ToString();
    }

    [PunRPC]
    public void changeQuestionAmount(int newQuestionAmount)
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
            if(parse.Title.English==null|| parse.Description==null|| parse.CoverImage.Large==null|| parse.BannerImage == null || parse.Genres==null|| parse.Characters.Nodes[randomCharacter].Name.Full==null || parse.Characters.Nodes[randomCharacter].Description == null|| parse.Characters.Nodes[randomCharacter].Image.Large==null) 
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
        photonViewRef.RPC("StartGame", RpcTarget.AllBuffered, currentQuestionData,answerList);
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
        gameState = gameStateType.playing;
        pash.SetActive(true);
        for(int f = 0; f < 4; f++)
        {
            answerButtons[f].GetComponent<Button>().interactable = true;
            answerButtons[f].GetComponent<Image>().color = colors[0];
        }
    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
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
        selectedIndex = buttonIndex;
        gameState = gameStateType.answered;
        photonViewRef.RPC("SendAnsToPASH", RpcTarget.MasterClient, PhotonNetwork.NickName, buttonIndex);
    }

    [PunRPC]
    public void SendAnsToPASH(string user, int buttonIndex)
    {
        if (correctAnsIndex == buttonIndex) playerAns.Add(new PlayerAns(user, true));
        else playerAns.Add(new PlayerAns(user, false));

        if (PhotonNetwork.PlayerList.Length == playerAns.Count)
        {
            StartCoroutine(SendDelay());
        }
    }
    IEnumerator SendDelay()
    {
        yield return new WaitForSeconds(1);
        pash.GetComponent<PlayerAndScoreHandler>().PreUpdateScoreboard(playerAns);
        playerAns.Clear();
        photonViewRef.RPC("WaitforNextRound", RpcTarget.AllBuffered, correctAnsIndex);
    }

    [PunRPC]
    public void WaitforNextRound(int correctAns)
    {
        timer.value = 0;
        currentTime = 0;
        gameState = gameStateType.waiting;
        correctAnsIndex = correctAns;
    }

    [PunRPC]
    public void StartNextRound(int currentQuestionNum, string[] currentQuestionData, string[] answerList)
    {
        currentQuestion = currentQuestionNum;
        if (currentQuestionNum == questions)
        {
            gameScreen.SetActive(false);
            gameState = gameStateType.stop;
            currentQuestion = 0;
            if (PhotonNetwork.IsMasterClient)
            {
                rc.SendMessageToChat(" " + pash.GetComponent<PlayerAndScoreHandler>().playerScoreBoard[0].name + " wins the game!", Message.MessageType.info);
            }
            pash.SetActive(false);
        }
        else
        {
            this.currentQuestionData = new string[8];
            this.currentQuestionData = currentQuestionData;
            this.answerList = new string[4];
            this.answerList = answerList;
            StartCoroutine(DownloadImage(currentQuestionData[7]));
            for (int t = 0; t < 4; t++)
            {
                answerButtons[t].GetComponent<Button>().interactable = true;
                answerButtons[t].GetComponent<Image>().color = colors[0];
                answerButtonTexts[t].text = answerList[t];
            }
            gameState = gameStateType.playing;
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
[System.Serializable]
public class CoverImage
{
    [JsonProperty("large")]
    public string Large;
}
[System.Serializable]
public class Title
{
    [JsonProperty("english")]
    public string English;
}
[System.Serializable]
public class Name
{
    [JsonProperty("full")]
    public string Full;
}
[System.Serializable]
public class Imagedata
{
    [JsonProperty("large")]
    public string Large;
}
[System.Serializable]
public class Node
{
    [JsonProperty("name")]
    public Name Name;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("image")]
    public Imagedata Image;
}
[System.Serializable]
public class Characters
{
    [JsonProperty("nodes")]
    public List<Node> Nodes;
}
[System.Serializable]
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
[System.Serializable]
public class Entry
{
    [JsonProperty("media")]
    public Media Media;
}
[System.Serializable]
public class Listdata
{
    [JsonProperty("entries")]
    public List<Entry> Entries;
}
[System.Serializable]
public class MediaListCollection
{
    [JsonProperty("lists")]
    public List<Listdata> Lists;
}
[System.Serializable]
public class Data
{
    [JsonProperty("MediaListCollection")]
    public MediaListCollection MediaListCollection;
}
[System.Serializable]
public class Root
{
    [JsonProperty("data")]
    public Data Data;
}

