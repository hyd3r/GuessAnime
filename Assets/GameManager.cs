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
    public List<string> animeNameList = new List<string>();
    public string[] currentQuestionData = new string[8];
    public List<Root> parseList = new List<Root>();
    PhotonView photonViewRef;
    public GameObject gameScreen;
    public Text guessTimeText;
    public Text questionAmountText;
    public Text trapTimeText;
    public Slider timer;
    public float gameTime;
    public float currentTime = 10f;
    public int questions=10;
    public int currentQuestion = 0;
    public Image gameImage;
    public Text[] answerButtonTexts = new Text[4];
    public int correctAnsIndex;
    private string[] answerList = new string[4];
    public GameObject pash;
    public List<PlayerAns> playerAns = new List<PlayerAns>();
    public Color[] colors;
    public int selectedIndex =-1;
    public GameObject[] answerButtons;
    public gameStateType gameState = gameStateType.stop;
    public RoomController rc;
    public Text questionLeft;
    public int trapPhaseTime = 5;
    public PowerupManager pm;
    public Text phaseText;
    public bool usedSkip = false;
    public bool blurNextRound = false;
    public bool corruptNextRound = false;
    public Image slider;
    public GameMode gameMode = GameMode.character;
    public Button[] gameModeButtons;

    public enum GameMode
    {
        character,
        anime
    }

    public enum gameStateType
    {
        stop,
        guessing,
        answered,
        trap
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
            case gameStateType.guessing:
                currentTime -= Time.deltaTime;
                timer.value = currentTime / gameTime;
                if (timer.value <= 0f)
                {
                    if (usedSkip)
                    {
                        SelectAnswer(100);
                        usedSkip = false;
                    }
                    else
                    {
                        SelectAnswer(99);
                    }
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
            case gameStateType.trap:
                currentTime += Time.deltaTime;
                timer.value = currentTime / trapPhaseTime;

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

                            for (int i = 0; i < 4; i++)
                            {
                                if (i != correctAnsIndex)
                                {
                                    if (gameMode == GameMode.character)
                                    {
                                        answerList[i] = charaList[Random.Range(0, charaList.Count - 1)];
                                    }
                                    else if (gameMode == GameMode.anime)
                                    {
                                        answerList[i] = animeNameList[Random.Range(0, animeNameList.Count - 1)];
                                    }
                                }
                                else
                                {
                                    if (gameMode == GameMode.character)
                                    {
                                        answerList[correctAnsIndex] = currentQuestionData[5];
                                    }
                                    else if (gameMode == GameMode.anime)
                                    {
                                        answerList[correctAnsIndex] = currentQuestionData[0];
                                    }
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
                index = i;
            }
        }
        if (request.responseCode == 200)
        {
            photonViewRef.RPC("SyncFin", PhotonNetwork.PlayerList[index], true);
            AddtoCharaList(parseList.Count-1);
            AddtoAnimeNameList(parseList.Count - 1);

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

    public void AddtoAnimeNameList(int index)
    {
        for (int i = 0; i < parseList[index].Data.MediaListCollection.Lists[0].Entries.Count - 1; i++)
        {
              animeNameList.Add(parseList[index].Data.MediaListCollection.Lists[0].Entries[i].Media.Title.English);
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
    public void IncDecTrapTime(bool toInc)
    {
        if (toInc) trapPhaseTime++;
        else trapPhaseTime--;
        photonViewRef.RPC("changeTrapTime", RpcTarget.AllBuffered, trapPhaseTime);
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
    [PunRPC]
    public void changeTrapTime(int newTrapTime)
    {
        trapPhaseTime = newTrapTime;
        trapTimeText.text = newTrapTime.ToString();
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
        for (int i = 0; i < 4; i++)
        {
            if (i != correctAnsIndex)
            {
                if (gameMode == GameMode.character)
                {
                    answerList[i] = charaList[Random.Range(0, charaList.Count - 1)];
                }
                else if (gameMode == GameMode.anime)
                {
                    answerList[i] = animeNameList[Random.Range(0, animeNameList.Count - 1)];
                }
            }
            else
            {
                if (gameMode == GameMode.character)
                {
                    answerList[correctAnsIndex] = currentQuestionData[5];
                }
                else if (gameMode == GameMode.anime)
                {
                    answerList[correctAnsIndex] = currentQuestionData[0];
                }
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

        if (gameMode == GameMode.character)
        {
            StartCoroutine(DownloadImage(currentQuestionData[7]));
        }
        else if (gameMode == GameMode.anime)
        {
            StartCoroutine(DownloadImage(currentQuestionData[2]));
        }

        for(int x = 0; x < 4; x++)
        {
            answerButtonTexts[x].text = answerList[x];
        }
        questionLeft.text = currentQuestion + " / " + questions;
        gameScreen.SetActive(true);
        currentTime = gameTime;
        gameState = gameStateType.guessing;
        phaseText.text = "Guessing Phase";
        pash.SetActive(true);
        for(int f = 0; f < 4; f++)
        {
            answerButtons[f].GetComponent<Button>().interactable = true;
            answerButtons[f].GetComponent<Image>().color = colors[0];
        }
        rc.showChat(false);
        for(int g = 0; g < pm.inventory.transform.childCount; g++)
        {
            Destroy(pm.inventory.transform.GetChild(g).gameObject);
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
        if (correctAnsIndex == buttonIndex|| buttonIndex == 100) playerAns.Add(new PlayerAns(user, true));
        else playerAns.Add(new PlayerAns(user, false));

        pash.GetComponent<PlayerAndScoreHandler>().GetPlayerWhoAnswered(user);

        if (PhotonNetwork.PlayerList.Length == playerAns.Count)
        {
            StartCoroutine(SendDelay());
        }
    }
    IEnumerator SendDelay()
    {
        yield return new WaitForSeconds(1);
        pm.dropLoot(playerAns);
        pash.GetComponent<PlayerAndScoreHandler>().PreUpdateScoreboard(playerAns);
        playerAns.Clear();
        photonViewRef.RPC("WaitforNextRound", RpcTarget.AllBuffered, correctAnsIndex);
    }

    [PunRPC]
    public void WaitforNextRound(int correctAns)
    {
        timer.value = 0;
        currentTime = 0;
        gameState = gameStateType.trap;
        pm.powerupLimit = 1;
        phaseText.text = "Standby Phase";
        correctAnsIndex = correctAns;
        pm.hasBarrier = false;
        pm.hasReverse = false;
        pm.isBoosted = false;
        slider.color = colors[3];
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
                rc.SendMessageToChat(" " + pash.GetComponent<PlayerAndScoreHandler>().playerScoreBoard[0].transform.GetChild(1).GetComponent<Text>().text + " wins the game!", Message.MessageType.info);
            }
            pash.SetActive(false);
        }
        else
        {
            this.currentQuestionData = new string[8];
            this.currentQuestionData = currentQuestionData;
            this.answerList = new string[4];
            this.answerList = answerList;

            if (gameMode == GameMode.character)
            {
                StartCoroutine(DownloadImage(currentQuestionData[7]));
            }
            else if (gameMode == GameMode.anime)
            {
                StartCoroutine(DownloadImage(currentQuestionData[2]));
            }

            for (int t = 0; t < 4; t++)
            {
                answerButtons[t].GetComponent<Button>().interactable = true;
                answerButtons[t].GetComponent<Image>().color = colors[0];
                answerButtonTexts[t].text = answerList[t];
            }
            pash.GetComponent<PlayerAndScoreHandler>().PreNextRoundStart();
            questionLeft.text = currentQuestion + " / " + questions;
            gameState = gameStateType.guessing;
            pm.powerupLimit = 1;
            slider.color = colors[2];
            selectedIndex = -1;
            phaseText.text = "Guessing Phase";
            currentTime = gameTime;
            if (blurNextRound)
            {
                blurNextRound = false;
                StartCoroutine(pm.blurimage(gameTime));
            }
            if (corruptNextRound)
            {
                corruptNextRound = false;
                pm.Corrupt();
            }
        }
    }
    public void ReturnToLobbyButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonViewRef.RPC("ReturnToLobby", RpcTarget.AllBuffered);
        }
        else
        {
            rc.SendMessageToChat(PhotonNetwork.NickName+" wanted to return to lobby", Message.MessageType.info);
        }

    }
    [PunRPC]
    public void ReturnToLobby()
    {
        gameScreen.SetActive(false);
        gameState = gameStateType.stop;
        currentQuestion = 0;
        if (PhotonNetwork.IsMasterClient)
        {
            rc.SendMessageToChat(" " + pash.GetComponent<PlayerAndScoreHandler>().playerScoreBoard[0].transform.GetChild(1).GetComponent<Text>().text + " wins the game!", Message.MessageType.info);
        }
        pash.SetActive(false);
    }
    public void use5050(string user)
    {
        photonViewRef.RPC("send5050", RpcTarget.MasterClient,user);
    }
    [PunRPC]
    public void send5050(string usern)
    {
        pm.use5050(usern,correctAnsIndex);
    }

    public void ChangeGameModeButton(bool isCharaName)
    {
        photonViewRef.RPC("ChangeGameMode", RpcTarget.AllBuffered,isCharaName);
    }

    [PunRPC]
    public void ChangeGameMode(bool isCharaName)
    {
        if (isCharaName)
        {
            gameMode = GameMode.character;
            gameModeButtons[0].interactable = false;
            gameModeButtons[1].interactable = true;
        }
        else
        {
            gameMode = GameMode.anime;
            gameModeButtons[0].interactable = true;
            gameModeButtons[1].interactable = false;
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

