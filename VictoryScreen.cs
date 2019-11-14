using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// Things to add:
/// + Skip animation if the player presses space
/// + Right now the player can press space and not get the exp 
/// + Improve background
/// + Change the awful victory text
/// </summary>


public class VictoryScreen : MonoBehaviour
{
    //Instances
    private BattleManager btlManager;

    //Fading in
    private bool screenActive;
    private bool fadingIn;
    private bool initiatedFadeOut = false; //Turns true the first time the player presses Space
    public Fade fadePanelToWorldMap;

    //General Victory Screen elements
    public Image thisImage;
    public Text victoryTextFore;
    public Text victoryTextBack;
    private Color panelItemsColor;

    //Fargas
    public Text fargasLevelUpBack;
    public Text fargasLevelUpFore;
    public Image fargasPortrait;
    public Image fargasPortraitBack;
    public Image fargasHP;
    public Image fargasHPBack;
    public Text fargasCurrentHPText;
    public Text fargasMaxHPText;
    public Image fargasMP;
    public Image fargasMPBack;
    public Text fargasCurrentMPText;
    public Text fargasMaxMPText;
    public Image fargasExp;
    public Image fargasExpBack;
    public Text fargasCurrentExpText;
    public Text fargasMaxExpText;
    private float expStep;
    private float fargasCurrentExp;
    private float fargasMaxExp;
    private float fargasExpStep; //Used to know by how much to increase the exp bar == 1/maxExp
    private int fargasExpGain;
    private bool fargasAddinExp; //Used in update to increase EXP bar
    private bool startFadingFargasIn;

    //Frea
    public Text freaLevelUpBack;
    public Text freaLevelUpFore;
    public Image freaPortrait;
    public Image freaPortraitBack;
    public Image freaHP;
    public Image freaHPBack;
    public Text freaCurrentHPText;
    public Text freaMaxHPText;
    public Image freaMP;
    public Image freaMPBack;
    public Text freaCurrentMPText;
    public Text freaMaxMPText;
    public Image freaExp;
    public Image freaExpBack;
    public Text freaCurrentExpText;
    public Text freaMaxExpText;
    private float freaCurrentExp;
    private float freaMaxExp;
    private float freaExpStep;
    private int freaExpGain;
    private bool freaAddinExp;
    private bool startFadingFreaIn;

    //Arcelus
    public Text arcelusLevelUpBack;
    public Text arcelusLevelUpFore;
    public Image arcelusPortrait;
    public Image arcelusPortraitBack;
    public Image arcelusHP;
    public Image arcelusHPBack;
    public Text arcelusCurrentHPText;
    public Text arcelusMaxHPText;
    public Image arcelusMP;
    public Image arcelusMPBack;
    public Text arcelusCurrentMPText;
    public Text arcelusMaxMPText;
    public Image arcelusExp;
    public Image arcelusExpBack;
    public Text arcelusCurrentExpText;
    public Text arcelusMaxExpText;
    private float arcelusCurrentExp;
    private float arcelusMaxExp;
    private float arcelusExpStep;
    private int arcelusExpGain;
    private bool arcelusAddinExp;
    private bool startFadingArcelusIn;

    //Oberon
    public Text oberonLevelUpBack;
    public Text oberonLevelUpFore;
    public Image oberonPortrait;
    public Image oberonPortraitBack;
    public Image oberonHP;
    public Image oberonHPBack;
    public Text oberonCurrentHPText;
    public Text oberonMaxHPText;
    public Image oberonMP;
    public Image oberonMPBack;
    public Text oberonCurrentMPText;
    public Text oberonMaxMPText;
    public Image oberonExp;
    public Image oberonExpBack;
    public Text oberonCurrentExpText;
    public Text oberonMaxExpText;
    private float oberonCurrentExp;
    private float oberonMaxExp;
    private float oberonExpStep;
    private int oberonExpGain;
    private bool oberonAddinExp;
    private bool startFadingOberonIn;

    //Gold
    public Text goldText;


    void Start()
    {
        screenActive = false;
        fadingIn = false;
        btlManager = BattleManager.instance;
        expStep = 0.0f;
        goldText.gameObject.SetActive(false);
        fargasLevelUpBack.gameObject.SetActive(false);
        fargasLevelUpFore.gameObject.SetActive(false);
        freaLevelUpBack.gameObject.SetActive(false);
        freaLevelUpFore.gameObject.SetActive(false);
        arcelusLevelUpBack.gameObject.SetActive(false);
        arcelusLevelUpFore.gameObject.SetActive(false);
        oberonLevelUpBack.gameObject.SetActive(false);
        oberonLevelUpFore.gameObject.SetActive(false);

        //Everything starts faded out
        startFadingFargasIn = startFadingArcelusIn = startFadingFreaIn = startFadingOberonIn = false;

        panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
        panelItemsColor.a = 0.0f;

        fargasPortrait.color = fargasPortraitBack.color = fargasHP.color = fargasCurrentHPText.color = fargasMaxHPText.color =  fargasMP.color = fargasCurrentMPText.color = fargasMaxMPText.color =
        fargasExp.color = fargasHPBack.color = fargasMPBack.color = fargasExpBack.color =  fargasCurrentExpText.color = fargasMaxExpText.color = 

        freaPortrait.color = freaPortraitBack.color = freaHP.color = freaMP.color = freaExp.color = freaHPBack.color = freaMPBack.color = freaExpBack.color = 
        freaCurrentHPText.color = freaMaxHPText.color = freaCurrentMPText.color = freaMaxMPText.color = freaCurrentExpText.color = freaMaxExpText.color =

        oberonPortrait.color = oberonPortraitBack.color = oberonHP.color = oberonMP.color = oberonExp.color = oberonHPBack.color = oberonMPBack.color = oberonExpBack.color = 
        oberonCurrentHPText.color = oberonMaxHPText.color = oberonCurrentMPText.color = oberonMaxMPText.color = oberonCurrentExpText.color = oberonMaxExpText.color = 

        arcelusPortrait.color = arcelusPortraitBack.color = arcelusHP.color = arcelusMP.color = arcelusExp.color = arcelusHPBack.color = arcelusMPBack.color = arcelusExpBack.color = 
        arcelusCurrentHPText.color = arcelusMaxHPText.color = arcelusCurrentMPText.color = arcelusMaxMPText.color = arcelusCurrentExpText.color = arcelusMaxExpText.color =

        victoryTextBack.color = victoryTextFore.color = panelItemsColor;

        // Clear out the consumables list
        //MainInventory.invInstance.consumableInv.Clear();
    }

    void Update()
    {
        //Screen active is set to true by the Fade object that transitions into the victory screen
        if (screenActive)
        {
            //Start fading in the victory panel itself and the defeat text
            if(thisImage.fillAmount<1.0f && fadingIn)
            {
                thisImage.fillAmount += 0.05f;
                //Fade in the Victory text
                panelItemsColor.a += 0.08f;
                victoryTextBack.color = panelItemsColor;
                panelItemsColor.r = 0.479175f;
                panelItemsColor.g = 0.1119616f;
                panelItemsColor.b = 0.6415094f;
                victoryTextFore.color = panelItemsColor;
                panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
            }
            else if(fadingIn)
            {
                //If we've completed the fade in, reset the panel items color and get the stats of the player
                panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
                panelItemsColor.a = 0.0f;
                fadingIn = false;
                goldText.text = "Gold: " + btlManager.goldGain.ToString(); //Update the gold;
                goldText.gameObject.SetActive(true);
                GetNewStats();
            }
            //We still haven't done anybody's EXP yet, so we start with Fargas
            //Fargas
            if (startFadingFargasIn)
            {
                //Fade in the character's portrait and bars
                if(FadeInItems(ref fargasPortrait, ref fargasPortraitBack, ref fargasHP, ref fargasHPBack, ref fargasCurrentHPText, ref fargasMaxHPText, ref fargasMP, ref fargasMPBack, ref fargasCurrentMPText, ref fargasMaxMPText, ref fargasExp, ref fargasExpBack, ref fargasCurrentExpText, ref fargasMaxExpText))
                {
                    //After the fade in is complete start adding the exp
                    fargasAddinExp = true;
                    startFadingFargasIn = false;
                }               
            }
            else if (fargasAddinExp)
            {
                IncreaseExp(ref fargasExp, ref fargasCurrentExp, ref fargasMaxExp, ref fargasExpGain,ref fargasExpStep, ref fargasCurrentExpText , ref fargasMaxExpText,  0);
            }
            //Oberon
            else if (startFadingOberonIn)
            {
                if (FadeInItems(ref oberonPortrait, ref oberonPortraitBack, ref oberonHP, ref oberonHPBack, ref oberonCurrentHPText, ref oberonMaxHPText, ref oberonMP, ref oberonMPBack, ref oberonCurrentMPText, ref oberonMaxMPText, ref oberonExp, ref oberonExpBack, ref oberonCurrentExpText, ref oberonMaxExpText))
                {
                    oberonAddinExp = true;
                    startFadingOberonIn = false;
                }
            }

            else if (oberonAddinExp)
            {
                IncreaseExp(ref oberonExp, ref oberonCurrentExp, ref oberonMaxExp, ref oberonExpGain, ref oberonExpStep, ref oberonCurrentExpText, ref oberonMaxExpText, 1);
            }
            //Frea
            else if (startFadingFreaIn)
            {
                if (FadeInItems(ref freaPortrait, ref freaPortraitBack, ref freaHP, ref freaHPBack, ref freaCurrentHPText, ref freaMaxHPText, ref freaMP, ref freaMPBack, ref freaCurrentMPText, ref freaMaxMPText, ref freaExp, ref freaExpBack, ref freaCurrentExpText, ref freaMaxExpText))
                {
                    freaAddinExp = true;
                    startFadingFreaIn = false;
                }
            }

            else if (freaAddinExp)
            {
                IncreaseExp(ref freaExp, ref freaCurrentExp, ref freaMaxExp, ref freaExpGain, ref freaExpStep, ref freaCurrentExpText, ref freaMaxExpText, 2);
            }
            //Arcelus
            else if (startFadingArcelusIn)
            {
                if (FadeInItems(ref arcelusPortrait, ref arcelusPortraitBack, ref arcelusHP, ref arcelusHPBack, ref arcelusCurrentHPText, ref arcelusMaxHPText, ref arcelusMP, ref arcelusMPBack, ref arcelusCurrentMPText, ref arcelusMaxMPText, ref arcelusExp, ref arcelusExpBack, ref arcelusCurrentExpText, ref arcelusMaxExpText))
                {
                    arcelusAddinExp = true;
                    startFadingArcelusIn = false;
                }
            }

            if (arcelusAddinExp && !fargasAddinExp && !oberonAddinExp && !freaAddinExp)
            {
                IncreaseExp(ref arcelusExp, ref arcelusCurrentExp, ref arcelusMaxExp, ref arcelusExpGain, ref arcelusExpStep, ref arcelusCurrentExpText, ref arcelusMaxExpText, 3);
            }
            else if (Input.GetButtonDown("Confirm") && !fargasAddinExp && !startFadingFargasIn && !freaAddinExp && !startFadingFreaIn && !oberonAddinExp && !startFadingOberonIn && !arcelusAddinExp && !startFadingArcelusIn && !initiatedFadeOut)
            {
                initiatedFadeOut = true;
                fadePanelToWorldMap.TransitionBackToWorldMapFromBattle();
            }
        }
    }
    
    public void GetNewStats()
    {
        //Must check if all the players are still alive but for now they are
        //Update the players' stats


        fargasHP.fillAmount = btlManager.players[0].playerReference.hpImage.fillAmount;
        fargasCurrentHPText.text = Mathf.RoundToInt (btlManager.players[0].currentHP).ToString() + " / ";
        fargasMaxHPText.text = btlManager.players[0].maxHP.ToString();
        fargasMP.fillAmount = btlManager.players[0].playerReference.currentMP / btlManager.players[0].playerReference.maxMP;
        fargasCurrentMPText.text = Mathf.RoundToInt(btlManager.players[0].currentMP).ToString() + " / ";
        fargasMaxMPText.text = btlManager.players[0].maxMP.ToString();
        fargasCurrentExp = btlManager.players[0].exp;
        fargasMaxExp = btlManager.players[0].expNeededForNextLevel;
        fargasExp.fillAmount = btlManager.players[0].exp / fargasMaxExp;
        fargasCurrentExpText.text = btlManager.players[0].exp.ToString() + " / ";
        fargasMaxExpText.text = btlManager.players[0].expNeededForNextLevel.ToString();
        fargasExpStep = 1.0f / fargasMaxExp;
        fargasExpGain = btlManager.expGain;


        oberonHP.fillAmount = btlManager.players[1].playerReference.hpImage.fillAmount;
        oberonCurrentHPText.text = Mathf.RoundToInt(btlManager.players[1].currentHP).ToString() + " / ";
        oberonMaxHPText.text = btlManager.players[1].maxHP.ToString();
        oberonMP.fillAmount = btlManager.players[1].playerReference.currentMP / btlManager.players[1].playerReference.maxMP;
        oberonCurrentMPText.text = Mathf.RoundToInt(btlManager.players[1].currentMP).ToString() + " / ";
        oberonMaxMPText.text = btlManager.players[1].maxMP.ToString();
        oberonCurrentExp = btlManager.players[1].exp;
        oberonMaxExp = btlManager.players[1].expNeededForNextLevel;
        oberonExp.fillAmount = btlManager.players[1].exp / oberonMaxExp;
        oberonCurrentExpText.text = btlManager.players[1].exp.ToString() + " / ";
        oberonMaxExpText.text = btlManager.players[1].expNeededForNextLevel.ToString();
        oberonExpStep = 1.0f / oberonMaxExp;
        oberonExpGain = btlManager.expGain;


        freaHP.fillAmount = btlManager.players[2].playerReference.hpImage.fillAmount;
        freaCurrentHPText.text = Mathf.RoundToInt(btlManager.players[2].currentHP).ToString() + " / ";
        freaMaxHPText.text = btlManager.players[2].maxHP.ToString();
        freaMP.fillAmount = btlManager.players[2].playerReference.currentMP / btlManager.players[2].playerReference.maxMP;
        freaCurrentMPText.text = Mathf.RoundToInt(btlManager.players[2].currentMP).ToString() + " / ";
        freaMaxMPText.text = btlManager.players[2].maxMP.ToString();
        freaCurrentExp = btlManager.players[2].exp;
        freaMaxExp = btlManager.players[2].expNeededForNextLevel;
        freaExp.fillAmount = btlManager.players[2].exp / freaMaxExp;
        freaCurrentExpText.text = btlManager.players[2].exp.ToString() + " / ";
        freaMaxExpText.text = btlManager.players[2].expNeededForNextLevel.ToString();
        freaExpStep = 1.0f / freaMaxExp;
        freaExpGain = btlManager.expGain;


        arcelusHP.fillAmount = btlManager.players[3].playerReference.hpImage.fillAmount;
        arcelusCurrentHPText.text = Mathf.RoundToInt(btlManager.players[3].currentHP).ToString() + " / ";
        arcelusMaxHPText.text = btlManager.players[3].maxHP.ToString();
        arcelusCurrentMPText.text = Mathf.RoundToInt(btlManager.players[3].currentMP).ToString() + " / ";
        arcelusMaxMPText.text = btlManager.players[3].maxMP.ToString();
        arcelusMP.fillAmount = btlManager.players[3].playerReference.currentMP / btlManager.players[3].playerReference.maxMP;
        arcelusCurrentExp = btlManager.players[3].exp;
        arcelusMaxExp = btlManager.players[3].expNeededForNextLevel;
        arcelusExp.fillAmount = btlManager.players[3].exp / arcelusMaxExp;
        arcelusCurrentExpText.text = btlManager.players[3].exp.ToString() + " / ";
        arcelusMaxExpText.text = btlManager.players[3].expNeededForNextLevel.ToString();
        arcelusExpStep = 1.0f / arcelusMaxExp;
        arcelusExpGain = btlManager.expGain;

        startFadingFargasIn = true;
    }

    private bool FadeInItems(ref Image portrait, ref Image portraitBack, ref Image hp, ref Image hpBack, ref Text currentHPText, ref Text maxHPText, ref Image mp, ref Image mpBack, ref Text currentMPText, ref Text maxMPText, ref Image exp, ref Image expBack, ref Text currentExpText, ref Text maxExpText)
    {
        panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
        panelItemsColor.a += 0.05f;
        portrait.color = portraitBack.color = hp.color = hpBack.color = currentHPText.color = maxHPText.color = mp.color = mpBack.color = currentMPText.color = maxMPText.color = exp.color = expBack.color = currentExpText.color = maxExpText.color = panelItemsColor;

        //Texts

        //HP
        panelItemsColor.r = 0.3960785f;
        panelItemsColor.b = 0.2705882f;
        panelItemsColor.g = 0.8078432f;
        currentHPText.color = maxHPText.color = panelItemsColor;

        //MP
        panelItemsColor.r = 0.2705882f;
        panelItemsColor.b = 0.8078432f;
        panelItemsColor.g = 0.5764706f;

        currentMPText.color = maxMPText.color = panelItemsColor;

        //EXP
        panelItemsColor.r = 0.5568628f;
        panelItemsColor.b = 0.5058824f;
        panelItemsColor.g = 0.2901961f;
        currentExpText.color = maxExpText.color = panelItemsColor;

        if (panelItemsColor.a >= 1.0f)
        {
            panelItemsColor.a = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }


    private void IncreaseExp(ref Image expBar, ref float currentExp, ref float maxExp, ref int expGain, ref float expStep, ref Text expText, ref Text maxExpText , int playerIndex)
    {
        if (Input.GetButtonDown("Confirm"))
        {
            if (expGain + currentExp >= maxExp)
            {
                expGain = expGain - Mathf.RoundToInt(maxExp - currentExp);
                currentExp = maxExp;
                expBar.fillAmount = 1.0f;
                btlManager.players[playerIndex].exp = Mathf.RoundToInt(currentExp);
            }
            else if (expGain + currentExp < maxExp)
            {
                currentExp += expGain;
                expGain = 0;
                expBar.fillAmount = currentExp / maxExp;
                expText.text = currentExp.ToString() + " / ";
            }
        }
        else
        {
            //Keep increasing the current exp by 1 and decreasing the exp gain by 1
            if (expBar.fillAmount < 1.0f && currentExp < maxExp && expGain > 0)
            {
                currentExp++;
                expText.text = currentExp.ToString() + " / ";
                expGain--;
                expBar.fillAmount += expStep;
            }
            //If you surpass the max exp, level up
            //also get the new max exp
            else if (currentExp >= maxExp || expBar.fillAmount >= 1.0f)
            {
                btlManager.players[playerIndex].exp = Mathf.RoundToInt(currentExp); //Update the battle manager
                btlManager.LevelUp(playerIndex);
                currentExp = btlManager.players[playerIndex].exp;
                maxExp = btlManager.players[playerIndex].expNeededForNextLevel;
                expText.text = currentExp.ToString() + " / ";
                maxExpText.text = maxExp.ToString();
                expStep = 1.0f / maxExp;
                expBar.fillAmount = 0.0f;

                switch (playerIndex)
                {
                    case 0:
                        fargasLevelUpBack.gameObject.SetActive(true);
                        fargasLevelUpFore.gameObject.SetActive(true);
                        break;
                    case 1:
                        oberonLevelUpBack.gameObject.SetActive(true);
                        oberonLevelUpFore.gameObject.SetActive(true);
                        break;
                    case 2:
                        freaLevelUpBack.gameObject.SetActive(true);
                        freaLevelUpFore.gameObject.SetActive(true);
                        break;
                    case 3:
                        arcelusLevelUpBack.gameObject.SetActive(true);
                        arcelusLevelUpFore.gameObject.SetActive(true);
                        break;
                }

            }
            //If we have reached the exp gain, stop
            else if (expGain <= 0)
            {
                //Update the party stats with the new exp. The battle manager will get them next time a battle starts
                //Normally the victory screen should not directly communicate with the PartyStats but as this is a simple write, going through the btlmanager seems and over complication
                btlManager.players[playerIndex].exp = Mathf.RoundToInt(currentExp);
                PartyStats.chara[playerIndex].currentExperience = Mathf.RoundToInt(currentExp);
                switch (playerIndex)
                {
                    case 0:
                        startFadingOberonIn = true;
                        fargasAddinExp = false;
                        break;
                    case 1:
                        startFadingFreaIn = true;
                        oberonAddinExp = false;
                        break;
                    case 2:
                        startFadingArcelusIn = true;
                        freaAddinExp = false;
                        break;
                    case 3:
                        arcelusAddinExp = false;
                        break;
                }
            }
        }
    }

    //Called from the Fade object transitioning into victory
    public void VictoryFadeIn()
    {
        screenActive = true;
        fadingIn = true;
    }


}
