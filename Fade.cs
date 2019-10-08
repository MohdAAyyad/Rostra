using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    private Image thisImage;
    public WMEnemy enemyHolder;
    private bool fadeOut;
    private bool transitionToBattle;
    private bool transitionToVictory;
    private bool transitionToDefeat;
    private bool transitionToWorldMap;

    public VictoryScreen victoryPanel;
    public GameObject defeatPanel;
    private UIBTL uiBtl;
    
    void Start()
    {
        thisImage = gameObject.GetComponent<Image>();
        fadeOut = false;
        transitionToBattle = false;
        transitionToVictory = false;
        transitionToDefeat = false;
        transitionToWorldMap = false;
        uiBtl = UIBTL.instance;
    }

    // Update is called once per frame
    void Update()
    {
        //Fadeout or Fadein?
        if (!fadeOut)
        {
            thisImage.fillAmount -= 0.02f;
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
                else if(transitionToVictory)
                {
                    transitionToVictory = false;
                    TransitionIntoVictory();
                    uiBtl.StartShowingEndScreen(true); //Show the victory screen stats now
                }
                else if(transitionToDefeat)
                {
                    transitionToDefeat = false;
                    TransitionIntoDefeat();
                    uiBtl.StartShowingEndScreen(false); //Show the defeat screen
                }
                else if(transitionToWorldMap)
                {
                    transitionToWorldMap = false;
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Queue Scene"));
                }
                
            }
        }
    }

    public void FlipFadeToBattle( WMEnemy enemyCollidingWithPlayer)
    {
        enemyHolder = enemyCollidingWithPlayer;
        fadeOut = true;
        transitionToBattle = true;
    }
    //Two version of flip fade, one for controlled situations where the WM is assigned from the editor and one for the world map
    public void FlipFadeToBattle()
    {
        fadeOut = !fadeOut;
        transitionToBattle = true;
    }

    public void FlipFadeToVictory()
    {
        fadeOut = !fadeOut;
        transitionToVictory = true;        
    }

    public void FlipFadeToDefeat()
    {
        fadeOut = !fadeOut;
        transitionToDefeat = true;

    }

    public void TransitionIntoBattle()
    {
        enemyHolder.TransitionIntoBattle();
    }
    
    public void TransitionIntoVictory()
    {
        victoryPanel.VictoryFadeIn();
    }
    
    public void TransitionIntoDefeat()
    {
        defeatPanel.gameObject.SetActive(true);
    }

    public void TransitionBackToWorldMapFromBattle()
    {
        fadeOut = !fadeOut;
        transitionToWorldMap = true;
    }
}
