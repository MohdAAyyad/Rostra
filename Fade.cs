using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    private Image thisImage;
    public WMEnemy enemyHolder;
    public WMEnemy secondBossHolder; //Bad code, have to. 
    public bool tutorial;
    private bool fadeOut;
    private bool transitionToBattle;
    private bool transitionToSecondBossFight; //Bad code, have to. 
    private bool transitionToVictory;
    private bool transitionToDefeat;
    private bool transitionToWorldMap;
    private bool transitionToEndTest;
    private bool transitionIntoACutscene;
    private bool transitionToMainMenu;
    private CutsceneTrigger cutsceneTriggerRef;
    private bool transitionOutOfACutscene;
    private bool canGoToSurvey;


    public VictoryScreen victoryPanel;
    public DefeatScreen defeatPanel;
    public GameObject endTestPanel;
    private UIBTL uiBtl;
    private AudioManager audioManager;
    private int bossCounter = -1; //Checks if which boss we're fighitng


    void Start()
    {
        thisImage = gameObject.GetComponent<Image>();
        fadeOut = false;
        transitionToBattle = false;
        transitionToSecondBossFight = false;
        transitionToVictory = false;
        transitionToDefeat = false;
        transitionToWorldMap = false;
        transitionToEndTest = false;
        transitionToMainMenu = false;
        canGoToSurvey = false;
        uiBtl = UIBTL.instance;
        audioManager = AudioManager.instance;

        if (endTestPanel)
        {
            endTestPanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canGoToSurvey)
        {
            Application.OpenURL("https://docs.google.com/forms/d/1YV2OpAa3DlDGKMnicz97-1V5Shs8iR6eQgThn0fjRzE");
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            FlipFadeToBattle();
        }

        //Fadeout or Fadein?
        if (!fadeOut)
        {
            thisImage.fillAmount -= 0.02f;

            if(thisImage.fillAmount <= 0.0f)
            {
                if(transitionIntoACutscene) //When it's a cutscene, fade in call cutscene, fade out
                {
                    transitionIntoACutscene = false;
                }
            }
        }
        else
        {
            thisImage.fillAmount += 0.02f;

            if (thisImage.fillAmount >= 1.0f)
            {
                if (transitionToBattle)
                {
                    transitionToBattle = false;
                    TransitionIntoBattle();
                    fadeOut = false;
                }
                else if(transitionToSecondBossFight)
                {
                    if(transitionOutOfACutscene == true) //If we're transitioning into another boss, chances are we've come from a cutscene
                    {
                        transitionOutOfACutscene = false;
                    }
                    transitionToSecondBossFight = false;
                    TransitionToSecondBossFight();
                    fadeOut = false;
                }
                else if (transitionToVictory)
                {
                    transitionToVictory = false;
                    TransitionIntoVictory();
                   // uiBtl.StartShowingEndScreen(true); //Show the victory screen stats now
                }
                else if (transitionToDefeat)
                {
                    transitionToDefeat = false;
                    TransitionIntoDefeat();
                    //uiBtl.StartShowingEndScreen(false); //Show the defeat screen
                }
                else if (transitionToWorldMap)
                {
                    //Debug.Log("Transition is now falseee");
                    transitionToWorldMap = false;
                    if (WMEnemy.startTutorial) //Is the fight we're in a tutorial?
                    {
                        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Queue Scene 2"));
                        WMEnemy.startTutorial = false;
                    }
                    else
                    {
                        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Queue Scene"));
                    }
                    audioManager.PlayThePreviousSong();
                    NewWMEnemy.isActive = true;
                    CutsceneManager.instance.isActive = false;
                }
                else if (transitionToEndTest)
                {
                    transitionToEndTest = false;
                    endTestPanel.gameObject.SetActive(true);
                    canGoToSurvey = true;
                }
                else if(transitionIntoACutscene)
                {
                    fadeOut = !fadeOut; //Fade out again
					//Debug.Log("Transition into a cutscene works");
					if (cutsceneTriggerRef != null)
                    {
						Debug.Log("Working");
                        cutsceneTriggerRef.TriggerCutscene(); //Load the cutscene while fading out
                        audioManager.PlayThisClip("Cutscene1");
                        cutsceneTriggerRef = null;
					}
                }
                else if(transitionOutOfACutscene)
                {
                    fadeOut = !fadeOut;
                    CutsceneManager.instance.End();
                    transitionOutOfACutscene = false;
                    audioManager.PlayThePreviousSong();
                }
                else if(transitionToMainMenu)
                {
                    GameManager.instance.DestoryUndestroyables();
                    transitionToMainMenu = false;
                    SceneManager.LoadScene("Main Menu");
                }
            }
        }
    }

    public void FlipFadeToBattle(WMEnemy enemyCollidingWithPlayer)
    {
		enemyHolder = enemyCollidingWithPlayer;
        fadeOut = true;
        transitionToBattle = true;
    }
    //Two version of flip fade, one for controlled situations where the WM is assigned from the editor and one for the world map
    public void FlipFadeToBattle()
    {
        if(!BattleManager.battleInProgress)
        BattleManager.battleInProgress = true; //A battle has just started

        if(EnemySpawner.instance.isBoss)
        {
            bossCounter++;
        }
        //Debug.Log("Flip Fade to battle and counter is " + bossCounter);

        if (bossCounter == 1 && enemyHolder!=null) //Boss counter 0 is Farea, 1 is Grendol
        {
            Debug.Log("Hit");
            fadeOut = true;
            transitionToSecondBossFight = true;
        }
        else
        {
            Debug.Log("miss");
            fadeOut = true;
            transitionToBattle = true;
        }
    }

    public void FlipFadeToVictory()
    {
        audioManager.PlayThisClip("VictoryMusic1");
        fadeOut = !fadeOut;
        transitionToVictory = true;
    }

    public void FlipFadeToDefeat()
    {
        audioManager.PlayThisClip("DefeatMusic1");
        fadeOut = !fadeOut;
        transitionToDefeat = true;
    }

    public void TransitionIntoACutscene(CutsceneTrigger cutTrigger)
    {
        cutsceneTriggerRef = cutTrigger;
		fadeOut = !fadeOut;
		transitionIntoACutscene = true;
		//Debug.Log("Transition into a cutscene called");
	}
    public void TransitionOutOfACutscene()
    {
        transitionOutOfACutscene = true;
        fadeOut = true;
       
    }
    public void TransitionIntoBattle()
    {
        enemyHolder.TransitionIntoBattle();
    }

    public void TransitionToSecondBossFight() //Bad code. Have to
    {
        secondBossHolder.TransitionIntoBattle();
    }

    public void TransitionIntoVictory()
    {
        victoryPanel.VictoryFadeIn();
    }

    public void TransitionIntoDefeat()
    {
        defeatPanel.DefeatFadeIn();
    }

    public void TransitionBackToWorldMapFromBattle()
    {
        fadeOut = !fadeOut;
        transitionToWorldMap = true;
    }
    public void TransitionBackToMainMenu()
    {
        fadeOut = !fadeOut;
        transitionToMainMenu = true;
    }

    public void FlipToEndTest()
    {
        fadeOut = !fadeOut;
        transitionToEndTest = true;
    }

}