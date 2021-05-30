using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class targetPlayerScript : MonoBehaviour
{
    private Button button;
    public GameObject powerupManager;
    public string username;
    public Sprite image;

    void Start()
    {
        button = this.gameObject.GetComponent<Button>();
        button.onClick.AddListener(selectPlayer);
        powerupManager = GameObject.Find("PowerupManager").gameObject;
    }
    public void setTargetPlayer(string user, Sprite sprite)
    {
        username = user;
        this.gameObject.transform.GetChild(0).GetComponent<Text>().text = username;
        image = sprite;
        this.gameObject.GetComponent<Image>().sprite = image;
    }

   public void selectPlayer()
    {
        powerupManager.GetComponent<PowerupManager>().targetPlayerName = username;
        for(int i = 0; i < this.transform.parent.childCount; i++)
        {
            this.transform.parent.GetChild(i).GetComponent<Button>().interactable = true;
        }
        button.interactable = false;
        powerupManager.GetComponent<PowerupManager>().useButton.transform.GetChild(0).GetComponent<Text>().text = "Target " + username;
    }
}
