using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[Header("Change this to change scene")]
	public string SceneToLoad;
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
        controls,
        contactUs
    };
    private MainMenuState currentState;

    //HowToPlay

    public GameObject howToPlayPanel;
    public Sprite[] howToPlayImagesPool;
    public Image howToPlayImage;
    public Image[] howToPlayIndicators;
    private Color howToPlayImageIndicatorColor;
    private AudioManager audioManager;

    //Controls
    public GameObject controlsPanel;

    //ContactUs
    public GameObject contactUsPanel;

	//Input
	private bool InDw_Down => Input.GetButtonDown("Down");
	private bool InDW_Up => Input.GetButtonDown("Up");
	private bool InDW_Left => Input.GetButtonDown("Left");
	private bool InDW_Right => Input.GetButtonDown("Right");
	private bool InDW_Confirm => Input.GetButtonDown("Confirm");
	private bool InDW_Cancel => Input.GetButtonDown("Cancel");


    private void Awake()
    {
        Instantiate(gameManager, gameManager.transform.position, gameManager.transform.rotation);
    }
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

        AudioListener.volume = 1.0f;
        audioManager = AudioManager.instance;
        audioManager.PlayThisClip("TitleTheme");
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
            case MainMenuState.contactUs:
                ContactUs();
                break;
        }
       
    }

    private void Main()
    {
        switch (menuIndex)
        {
            case 0:
                if (InDw_Down)
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[1].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Up)
                {
                    menuIndex = 3;
                    hilighter.transform.localPosition = hPos[4].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Confirm) //Player has chosen start game
                {
                    audioManager.PlayThisEffect("uiConfirm");
                    AudioManager.instance.PlayThisClip("WorldMapMusic1");
                    UIBTL.conversationAfterBattle = false;
                    BattleManager.battleInProgress = false;
                    startFading = true;
                }
                break;
            case 1:
                if (InDw_Down)
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[2].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Up)
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[0].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Confirm) //Chosen How To Play
                {
                    audioManager.PlayThisEffect("uiConfirm");
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
                if (InDw_Down)
                {
                    menuIndex++;
                    hilighter.transform.localPosition = hPos[3].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Up)
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[1].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Confirm) //Player has chosen controls
                {
                    audioManager.PlayThisEffect("uiConfirm");
                    controlsPanel.gameObject.SetActive(true);
                    currentState = MainMenuState.controls;
                }
                break;
            case 3:
                if (InDw_Down)
                {
                    menuIndex = 4;
                    hilighter.transform.localPosition = hPos[4].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Up)
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[2].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Confirm) //Player has chosen to quit
                {
                    audioManager.PlayThisEffect("uiConfirm");
                    contactUsPanel.gameObject.SetActive(true);
                    Cursor.visible = true;
                    currentState = MainMenuState.contactUs;
                }
                break;
            case 4:
                if (InDw_Down)
                {
                    menuIndex = 0;
                    hilighter.transform.localPosition = hPos[0].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Up)
                {
                    menuIndex--;
                    hilighter.transform.localPosition = hPos[3].transform.localPosition;
                    audioManager.PlayThisEffect("uiScroll");

                }
                else if (InDW_Confirm) //Player has chosen to quit
                {
                    audioManager.PlayThisEffect("uiConfirm");
                    Application.Quit();
                }
                break;
        }

        if (startFading)
        {
            fade.fillAmount += 0.02f;
            if (fade.fillAmount >= 1.0f)
            {
                SceneManager.LoadScene(SceneToLoad);
            }
        }
    }

    //How To Play Screen
    private void HowToPlay()
    {
        if (InDW_Right)
        {
            if (menuIndex < 6)
            {
                audioManager.PlayThisEffect("uiScroll");
                menuIndex++;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[menuIndex - 1].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
            else if (menuIndex >= 6)
            {
                audioManager.PlayThisEffect("uiScroll");
                menuIndex = 0;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[6].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
        }
        else if (InDW_Left)
        {
            if (menuIndex > 0)
            {
                audioManager.PlayThisEffect("uiScroll");
                menuIndex--;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[menuIndex + 1].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
            else if (menuIndex <= 0)
            {
                audioManager.PlayThisEffect("uiScroll");
                menuIndex = 6;
                howToPlayImage.sprite = howToPlayImagesPool[menuIndex];
                howToPlayIndicators[0].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 1.0f;
                howToPlayIndicators[menuIndex].color = howToPlayImageIndicatorColor;
                howToPlayImageIndicatorColor.a = 0.5f;
            }
        }
        else if(InDW_Cancel)
        {
            audioManager.PlayThisEffect("uiCancel");
            menuIndex = 1;
            howToPlayPanel.gameObject.SetActive(false);
            currentState = MainMenuState.main;
        }
    }

    private void Controls()
    {
        if (InDW_Cancel)
        {
            audioManager.PlayThisEffect("uiCancel");
            menuIndex = 2;
            controlsPanel.gameObject.SetActive(false);
            currentState = MainMenuState.main;
        }
    }

    private void ContactUs()
    {
        if (InDW_Cancel)
        {
            audioManager.PlayThisEffect("uiCancel");
            menuIndex = 3;
            contactUsPanel.gameObject.SetActive(false);
            currentState = MainMenuState.main;
            Cursor.visible = false;
        }
    }
}
