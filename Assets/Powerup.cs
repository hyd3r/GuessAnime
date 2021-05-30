using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{
    public string powerupName;
    public string powerupDescription;
    public Sprite powerupImage;
    private Button button;
    public string powerupType;
    public GameObject detailsTab;
    public GameObject gm;
    public GameObject useButton;
    public GameObject powerupManager;

    void Start()
    {
        button = this.gameObject.GetComponent<Button>();
        button.onClick.AddListener(ViewDetails);
        detailsTab = GameObject.Find("PowerupDetailsTab").gameObject;
        gm = GameObject.Find("GameManager").gameObject;
        useButton = GameObject.Find("UsePowerupButton").gameObject;
        powerupManager = GameObject.Find("PowerupManager").gameObject;
    }

    void Update()
    {
        if (powerupType.Equals("spell"))
        {
            if (gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.trap)
            {
                button.interactable = false;
            }
            else if(gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.guessing)
            {
                button.interactable = true;
            }
        }
        else if (powerupType.Equals("trap"))
        {
            if (gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.guessing)
            {
                button.interactable = false;
            }
            else if (gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.trap)
            {
                button.interactable = true;
            }
        }
    }

    void ViewDetails()
    {
        powerupManager.GetComponent<PowerupManager>().useItemChildIndex = this.gameObject.transform.GetSiblingIndex();
        powerupManager.GetComponent<PowerupManager>().useButton.GetComponent<Button>().interactable = true;
        detailsTab.transform.GetChild(0).gameObject.SetActive(true);
        detailsTab.transform.GetChild(0).GetComponent<Image>().sprite = powerupImage;
        detailsTab.transform.GetChild(1).GetComponent<Text>().text = powerupName;
        detailsTab.transform.GetChild(2).GetComponent<Text>().text = powerupDescription;
        detailsTab.transform.GetChild(3).GetComponent<Text>().text = powerupType;


        if (gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.guessing)
        {
            if (powerupType.Equals("spell"))
            {
                useButton.GetComponent<Button>().interactable = true;
            }
            else if (powerupType.Equals("trap"))
            {
                useButton.GetComponent<Button>().interactable = false;
            }
        }else if(gm.GetComponent<GameManager>().gameState == GameManager.gameStateType.trap)
        {
            if(powerupType.Equals("spell"))
            {
                useButton.GetComponent<Button>().interactable = false;
            }
            else if (powerupType.Equals("trap"))
            {
                useButton.GetComponent<Button>().interactable = true;
            }
        }
    }
  }


