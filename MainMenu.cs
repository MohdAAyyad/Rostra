using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject hilighter;
    public GameObject[] hPos;
    public GameManager gameManager;

    private int menuIndex;
    public Image fade;
    private bool startFading;
    private enum MainMenuState
    {
        main,
        howToPlay,
        controls
    };
    private MainMenuState currentState;

    //HowToPlay

    public GameObject howToPlayPanel;
    public Sprite[] howToPlayImagesPool;
    public Image howToPlayImage;
    public Image[] howToPlayIndicators;
    private Color howToPlayImageIndicatorColor;

    //Controls
    public GameObject controlsPanel;


    void Start()
    {
        Cursor.visible = false;
        hilighter.transform.localPosition = hPos[0].transform.localPosition;
        menuIndex = 0;
        fade.fillAmount = 0.0f;
        startFading = false;
        currentState = MainMenuState.main;

        howToPlayImageIndicatorColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        for (int i = 1;i < howToPlayIndicators.Length;i++) //Only the first one is fully visible
        {
            howToPlayIndicators[i].color = howToPlayImageIndicatorColor;
        }

        howToPlayPanel.gameObject.SetActive(false);
        controlsPanel.gameObject.SetActive(false);

        Instantiate(gameManager, gameManager.transform.position, gameManager.transform.rotation);
    }

    void Update()
    {
        switch(currentState)
        {
            case MainMenuState.main:
                Main();
                break;
            case MainMenuState.howToPlay:
                HowToPlay();
                break;
            case MainMenuState.controls:
                Controls();
                break;
        }
       
    }

    private void Main()
    {
        switch (menuIndex)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[1].transform.localPosition;

                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    menuIndex = 3;
                    hilighter.transform.localPosition = hPos[3].transform.localPosition;

                }
                else if (Input.GetButtonDown("Confirm")) //Player has chosen start game
                {
                    UIBTL.conversationAfterBattle = false;
                    BattleManager.battleInProgress = false;
                    startFading = true;
                }
                break;
            case 1:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[2].transform.localPosition;

                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[0].transform.localPosition;

                }
                else if (Input.GetButtonDown("Confirm")) //Chosen How To Play
                {
                    menuIndex = 0;
                    howToPlayPanel.gameObject.SetActive(true);
                    howToPlayImage.sprite = howToPlayImagesPool[0];

                    howToPlayImageIndicatorColor.a = 0.5f;

                    for (int i = 1; i < howToPlayIndicators.Length; i++) //Only the first one is fully visible
                    {
                        howToPlayIndicators[i].color = howToPlayImageIndicatorColor;
                    }
                    currentState = MainMenuState.howToPlay;

                }
                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[3].transform.localPosition;

                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[1].transform.localPosition;

                }
                else if (Input.GetButtonDown("Confirm")) //Player has chosen controls
                {
                    controlsPanel.gameObject.SetActive(true);
                    currentState = MainMenuState.controls;
                }
                break;
            case 3:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    menuIndex = 0;
                    hilighter.transform.localPosition = hPos[0].transform.localPosition;

                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[2].transform.localPosition;

                }
                else if (Input.GetButtonDown("Confirm")) //Player has chosen to quit
                {
                    Application.Quit();
                }
                break;
        }

        if (startFading)
        {
            fade.fillAmount += 0.02f;
            if (fade.fillAmount >= 1.0f)
            {
                SceneManager.LoadScene("Playtest1");
            }
        }
    }

    //How To Play Screen
    private void HowToPlay()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (menuIndex < 6)
            {
                menuIndex++;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[menuIndex - 1].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
            else if (menuIndex >= 6)
            {
                menuIndex = 0;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[6].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (menuIndex > 0)
            {
                menuIndex--;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[menuIndex + 1].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
            else if (menuIndex <= 0)
            {
                menuIndex = 6;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[0].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            menuIndex = 1;
            howToPlayPanel.gameObject.SetActive(false);
            currentState = MainMenuState.main;
        }
    }

    private void Controls()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            menuIndex = 2;
            controlsPanel.gameObject.SetActive(false);
            currentState = MainMenuState.main;
        }
    }
}
