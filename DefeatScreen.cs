using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefeatScreen : MonoBehaviour
{
    //Fading in
    public Fade fade;
    private bool screenActive;
    private bool fadingIn;

    //General Defeat Screen elements
    public Image thisImage;
    private string defeatHopeString; //Stores the words defeat and hope
    private bool isActive;
    public Text defeatTextFore;
    public Text defeatTextBack;
    public Text quote;
    public Text author;
    private int textCounter; //Used to move one letter at a time
    private float timeToNextLetter;
    private Color panelItemsColor;
    private string[] quotesPool;
    private string[] authorsPool;
    private int quoteIndex = 0;

    //Highlighter
    public GameObject highlighter;
    public GameObject[] hPos;
    public GameObject menuOptions;
    private int menuIndex = 0;
    private bool menuActive = false; //Used to know when the highlighter is active
    private bool choiceMade = false; //Used to know when to change the text

    void Start()
    {
        screenActive = false;
        fadingIn = false;
        quotesPool = new string[2];
        authorsPool = new string [2];
        panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
        panelItemsColor.a = 0.0f;
        defeatTextBack.color = defeatTextFore.color = panelItemsColor;
        quotesPool[0] = "\" Watching the wolves devour the child, Hagir came to a simple conclusion. To live is to invite our inevitable demise. \"";
        authorsPool[0] = "On Matters of Life and Death. Chapter Three";
        quotesPool[1] = "\"It is not those who wield great power that will succeed in our world, but those with the resolve to triumph over adversity\"";
        authorsPool[1] = "High Cleric Alastra";
        defeatHopeString = "Defeat!";
        textCounter = 0;
        timeToNextLetter = 0.02f;

        highlighter.gameObject.SetActive(false);
        menuOptions.gameObject.SetActive(false);
        // Clear out the consumables list
        //MainInventory.invInstance.consumableInv.Clear();
    }

    void Update()
    {
        if(screenActive)
        {
            if(thisImage.fillAmount < 1.0f && fadingIn)
            {
                //Fade in the Defeat text
                thisImage.fillAmount += 0.05f;
                panelItemsColor.a += 0.08f;
                defeatTextBack.color = panelItemsColor;
                panelItemsColor.r = 0.7058824f;
                panelItemsColor.b = 0.08294977f;
                panelItemsColor.g = 0.07843137f;
                defeatTextFore.color = panelItemsColor;
                panelItemsColor.r = panelItemsColor.g = panelItemsColor.b = 1.0f;
            }
            else if(fadingIn)
            { 
                //Once the fade in is done, start writing the quote
                StartCoroutine(WriteQuote());
                fadingIn = false;
            }
            else if(menuActive && !choiceMade)
            {
                switch(menuIndex)
                {
                    case 0: //Player is at load game
                        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                        {
                            highlighter.transform.position = hPos[1].transform.position;
                            menuIndex = 1;
                        }
                        else if(Input.GetButtonDown("Confirm"))
                        {
                            quoteIndex = 1; //Show life text
                            panelItemsColor.r = 0.9630859f;
                            panelItemsColor.g = 1.0f;
                            panelItemsColor.b = 0.5424528f;
                            panelItemsColor.a = 1.0f;
                            quote.color = author.color = panelItemsColor;
                            quote.text = "";
                            author.text = "";
                            StartCoroutine(WriteQuote());
                            choiceMade = true;
                        }
                        break;
                    case 1: //Return to main menu
                        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                        {
                            highlighter.transform.position = hPos[0].transform.position;
                            menuIndex = 0;
                        }
                        else if (Input.GetButtonDown("Confirm"))
                        {
                            choiceMade = true;
                            fade.TransitionBackToMainMenu();
                        }
                        break;
                }
            }
        }
    }

    public void DefeatFadeIn()
    {
        screenActive = true;
        fadingIn = true;
    }

    private  IEnumerator WriteQuote()
    {
        quote.text = quotesPool[quoteIndex].Substring(0, textCounter);
        yield return new WaitForSeconds(timeToNextLetter);
        if (textCounter + 1 < quotesPool[quoteIndex].Length)
        {
            textCounter++;
            StartCoroutine(WriteQuote());
            //Recursive call until the string is finished
        }
        else
        {
            textCounter = 0;
            StartCoroutine(WriteAuthor());
            //Once the string is finished, start writing the author
        }
        
    }

    private IEnumerator WriteAuthor()
    {
        author.text = authorsPool[quoteIndex].Substring(0, textCounter);
        yield return new WaitForSeconds(timeToNextLetter);
        if (textCounter < authorsPool[quoteIndex].Length)
        {
            textCounter++;
            StartCoroutine(WriteAuthor());
        }
        else
        {
            if (!menuActive)
            {
                //If the menu is inactive, that means we're still at the death quote
                //Activate the menu and reset the text counter
                highlighter.gameObject.SetActive(true);
                menuOptions.gameObject.SetActive(true);
                menuActive = true;
                menuIndex = 0;
                textCounter = 0;
            }
            else if(choiceMade) //If the player chose loadgame
            {
                fade.TransitionBackToWorldMapFromBattle(); // -->Placeholder for loadgame
            }
        }

    }
}
