using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIBTL : MonoBehaviour
{
    //Instances
    private BattleManager btlManager;
    private MainInventory inventory;
    private SkillsInventory skills;

    public GameObject playerTurnIndicator;
    public GameObject chooseEnemyArrow;
    public GameObject[] chooseEnemyArrowForSelectAll;
    public GameObject playerIndicatorPos0;
    public GameObject playerIndicatorPos1;
    public GameObject playerIndicatorPos2;
    public GameObject playerIndicatorPos3;
    public GameObject[] enemyIndicatorPosArray = new GameObject[5];
    public GameObject controlsPanel; //Needs to be disabled after choosing a command and re-enabled when it's a player's turn
    public GameObject rageModeIndicator1;
    public GameObject rageModeIndicator2;
    public GameObject highlighter;
    public GameObject[] highlighiterPos;
    private int controlsIndicator; //Used to know which command has been chosen
    private int enemyIndicatorIndex;//Used to know which enemy is being chosen to be attacked
    private int activeRange; // Are we using the player's standard range of a skill's range?
    [HideInInspector]
    public int currentPlayerTurnIndex; //Updated from the btl manager to know which player turn it is

    //Control Panel
    public Text rageText; //Needs to be disabled when it's not usable
    public Text playerName;
    public Text attackText;
    public Text skillsText;
    public Text itemsText;
    public Text guardText;
    public Text currentHP;
    public Text maxHP;
    public Image hpBar;
    public Text currentMP;
    public Text maxMP;
    public Image mpBar;

    //Skills Control Panel

    public Image skillsPanel;
    public GameObject[] skillsHPos;
    public GameObject skillsHighlighter;
    public Text[] skillNames;
    public Text[] mpCosts;
    public Text skillDescription;
    public Text skillAtkValueText;
    public Text skillWaitValueText;
    private bool firstTimeOpenedSkillsPanel; //Used to make sure we get the skills information only once per player per turn

    //Items Control Panel
    public GameObject[] choosePlayerPos;
    public GameObject choosePlayerArrow;
    public GameObject itemsPanel;
    public Text itemDescription;
    public Sprite[] itemIcons; //Stores the icon of each item --> Temporary until the itemIcons array is ready in the MainInventory
    public Image[] itemIconsInPanel; //Stores references to the icons inside the panel --> They change if we scroll down
    public Text[] itemNames; //Displayes item name
    public Text[] itemCount; //Displays item count
    public Image itemsHighlighter; //Moves with the itemsPanelIndex to tell which item the player is currently using
    public GameObject[] itemsHPos;
    private int itemHPosIndex;
    public GameObject upArrow;
    public GameObject downArrow;
    private int itemsPanelIndex; //Which item are we at now? Also represents the item ID since items will be ordered based on their IDs

    //Q UI Images

    private List<Sprite> imagesQ; //Filled by the BTL manager
    public Image[] images = new Image[9];
    private Vector2 imageRecyclePos; //To which position do images go when recycled?
    private Vector2 targetPos; //Used to calculate the distance each image travels in the Q
    private float imageMovementSpeed;
    private float imageMaxDistance; //Distance to be moved by each image
    private bool moveImagesNow; //Toggled on end turn and when the first image hits the recycler

    //States
    private enum btlUIState
    {
        choosingBasicCommand, //Player still choosing which command to use
        choosingSkillsCommand, //Player chooses between skills
        choosingItemsCommand, //Player chooses items
        choosingEnemy, //Player has chosen an offense command that targets one enemy
        choosingAllEnemies,
        choosingRowOfEnemies,
        choosingPlayer, //Player has chosen a supporting command
        choosingAllPlayers,
        battleEnd
    }

    private btlUIState currentState;
    private btlUIState previousState; //Needed to know if we're choosing a player to use an item or use a skill

    //Enemies
    public Enemy[] enemies;
    public bool [] enemiesDead;
    public int numberOfEnemies; //Populated by the battle manager at the start of the battle
    private int numberOfDeadEnemies = 0;
    private bool moveRangedRowToFrontRoW = false; //Turend true when there are 4 or more enemies and 0,1,2 are dead


    //Players
    private Player playerInControl;
    private int playerIndicatorIndex; //Used to keep track where the playerindicator is when using items or skills

    //Activity Text
    public GameObject activtyTextBack;
    public Text activityText;

    //End Battle Screen
    public Fade fadePanel;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    private bool battleHasEnded;
    

    //Dialogue after battle
    public static bool conversationAfterBattle = false; //If true, a conversation will start right after the battle ends
    private DialogueManager dialogueManager;
    private DialogueContainer dialogueContainer;

    #region singleton
    public static UIBTL instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


    }
    #endregion

    void Start()
    {
        btlManager = BattleManager.instance;
        inventory = MainInventory.invInstance;
        skills = SkillsInventory.invInstance;

        enemies = new Enemy[5]; //Filled by the BTL Manager in Add Enemy
        enemiesDead = new bool[5]; //Every entry is turned to true by the enemy that dies
        controlsIndicator = 0; //Start at Attack
        highlighter.transform.position = highlighiterPos[0].transform.position;
       previousState = currentState = btlUIState.choosingBasicCommand;
        playerName.text = "";
        chooseEnemyArrow.gameObject.SetActive(false);
        playerTurnIndicator.SetActive(false);
        controlsPanel.gameObject.SetActive(false);
        enemyIndicatorIndex = 0;
        activeRange = 0;

        imagesQ = new List<Sprite>();
        imageRecyclePos = images[8].gameObject.transform.localPosition;
        targetPos = images[0].transform.localPosition;

        imageMovementSpeed = 250.0f;
        imageMaxDistance = 149.0f;
        moveImagesNow = false;

        //Rage mode is not available when the battle starts
        rageText.color = Color.gray;
        rageModeIndicator1.gameObject.SetActive(false);
        rageModeIndicator2.gameObject.SetActive(false);

        //All the enemies are alive at the beginning
        for(int i=0;i<enemiesDead.Length;i++)
        {
            enemiesDead[i] = false;
        }

        //End Battle
        battleHasEnded = false;

        //Activity Text
        activityText.text = "";
        activtyTextBack.gameObject.SetActive(false);


        //Skills
        skillsPanel.gameObject.SetActive(false);
        firstTimeOpenedSkillsPanel = false;

        //Items
        itemsPanel.gameObject.SetActive(false);
        itemHPosIndex = 0;
        choosePlayerArrow.gameObject.SetActive(false);
        chooseEnemyArrow.gameObject.SetActive(false);

        playerIndicatorIndex = 0;

        //Dialogue after battle
        dialogueManager = DialogueManager.instance;
        dialogueContainer = DialogueContainer.instance;

        for (int i = 0; i < chooseEnemyArrowForSelectAll.Length; i++)
        {
            chooseEnemyArrowForSelectAll[i].gameObject.SetActive(false);
        }
    }


    void Update()
    {
        if(moveImagesNow)
        {
            //Called on End Turn
            MoveQImages();
            
        }

        switch(currentState)
        {
            case btlUIState.choosingBasicCommand:
                ChoosingBasicCommand();
                break;
            case btlUIState.choosingSkillsCommand:
                ChoosingSkillsCommand();
                break;
            case btlUIState.choosingItemsCommand:
                ChoosingItemsCommand();
                break;
            case btlUIState.choosingEnemy:
                ChoosingEnemy();
                break;
            case btlUIState.choosingAllEnemies:
                ChoosingAllEnemies();
                break;
            case btlUIState.choosingRowOfEnemies:
                break;
            case btlUIState.choosingPlayer:
                ChoosingPlayer();
                break;
            case btlUIState.choosingAllPlayers:
                break;
            case btlUIState.battleEnd:
                EndBattleUI();
                break;
        }
    }

    public void AddImageToQ(Sprite nextOnQImage)
    {
        //Called from the BTL manager when adding characters to the Q
        imagesQ.Add(nextOnQImage);
    }

    public void QueueIsReady()
    {
        //Called from the BTL manager when the Q has been built
        Debug.Log("Images count " + imagesQ.Count);
        //Fill up the Q until its of size 9. Only 6 will be on screen at a time however.
        switch(imagesQ.Count)
        {
            //Minimum size of Q is 5 since we will not be removing the images when characters die
            //Change the image recycler position depending on the size of the Q
            case 5:
                imageRecyclePos = images[4].transform.localPosition;
                images[5].gameObject.SetActive(false);
                images[6].gameObject.SetActive(false);
                images[7].gameObject.SetActive(false);
                images[8].gameObject.SetActive(false);

                for(int i =0;i<5;i++)
                {
                    //0 - 4
                    images[i].sprite = imagesQ[i];
                }


                break;
            case 6:
                imageRecyclePos = images[5].transform.localPosition;
                images[6].gameObject.SetActive(false);
                images[7].gameObject.SetActive(false);
                images[8].gameObject.SetActive(false);

                for (int i = 0; i < 6; i++)
                {
                    //0 - 5
                    images[i].sprite = imagesQ[i];
                }
                break;
            case 7:
                imageRecyclePos = images[6].transform.localPosition;
                images[7].gameObject.SetActive(false);
                images[8].gameObject.SetActive(false);

                for (int i = 0; i < 7; i++)
                {
                    //0 - 6
                    images[i].sprite = imagesQ[i];
                }


                break;
            case 8:
                imageRecyclePos = images[7].transform.localPosition;
                images[8].gameObject.SetActive(false);
                for (int i = 0; i < 8; i++)
                {
                    //0 - 7
                    images[i].sprite = imagesQ[i];
                }
                break;
            case 9:
                for (int i = 0; i < 9; i++)
                {
                    //0 - 8
                    images[i].sprite = imagesQ[i];
                }
                break;

        }

    }

    public void MoveQImages()
    {

        //Once the images start moving, turn off the indicator next to the "RAGE" word and return the text color to normal if the previous player was in rage
        if (playerInControl.currentState == Player.playerState.Rage)
        {
            rageModeIndicator1.gameObject.SetActive(false);
            rageModeIndicator2.gameObject.SetActive(false);
            skillsText.color = Color.white;
            itemsText.color = Color.white;
            guardText.color = Color.white;
        }

        //Move all the images an amount of imageMaxDistance to the right

        targetPos.x = images[0].transform.localPosition.x + imageMaxDistance;
        images[0].transform.localPosition = Vector2.MoveTowards(images[0].transform.localPosition, targetPos, imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[1].transform.localPosition.x + imageMaxDistance;
        images[1].transform.localPosition = Vector2.MoveTowards(images[1].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[2].transform.localPosition.x + imageMaxDistance;
        images[2].transform.localPosition = Vector2.MoveTowards(images[2].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[3].transform.localPosition.x + imageMaxDistance;
        images[3].transform.localPosition = Vector2.MoveTowards(images[3].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[4].transform.localPosition.x + imageMaxDistance;
        images[4].transform.localPosition = Vector2.MoveTowards(images[4].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[5].transform.localPosition.x + imageMaxDistance;
        images[5].transform.localPosition = Vector2.MoveTowards(images[5].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[6].transform.localPosition.x + imageMaxDistance;
        images[6].transform.localPosition = Vector2.MoveTowards(images[6].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[7].transform.localPosition.x + imageMaxDistance;
        images[7].transform.localPosition = Vector2.MoveTowards(images[7].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

        targetPos.x = images[8].transform.localPosition.x + imageMaxDistance;
        images[8].transform.localPosition = Vector2.MoveTowards(images[8].transform.localPosition, targetPos , imageMovementSpeed * Time.deltaTime);

    }

    //Called when the image at the far right of the Q collides with the recycle image collider
    public void ImageRecycle(int imageIndex)
    {
        //We've hit the recycler, stop moving!
        moveImagesNow = false;

        switch (imageIndex) //Which image hit the recycler?
        {
            case 0:
                images[0].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 1:
                images[1].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 2:
                images[2].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 3:
                images[3].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 4:
                images[4].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 5:
                images[5].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 6:
                images[6].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 7:
                images[7].gameObject.transform.localPosition = imageRecyclePos;
                break;
            case 8:
                images[8].gameObject.transform.localPosition = imageRecyclePos;
                break;

        }
    }

    //Called from the BTL Manager to update the UI based on which player's turn it is
    public void ShowThisPlayerUI(int playerIndex, string name, Player playerReference)
    {
        DisableActivtyText();
        playerInControl = playerReference;
        if (playerReference.currentState != Player.playerState.Waiting && !battleHasEnded)
        {
            currentState = btlUIState.choosingBasicCommand;

            playerTurnIndicator.SetActive(true);
            controlsPanel.gameObject.SetActive(true);

            playerName.text = name;
            UpdatePlayerHPControlPanel();
            UpdatePlayerMPControlPanel();
            RageOptionTextColor();
        }

        playerInControl.PlayerTurn();

        //Turn on the indicator if the player is in rage mode
        if (playerInControl.currentState==Player.playerState.Rage)
        {
            rageModeIndicator1.gameObject.SetActive(true);
            rageModeIndicator2.gameObject.SetActive(true);
            skillsText.color = Color.grey;
            itemsText.color = Color.grey;
            guardText.color = Color.grey;
            rageText.color = Color.yellow;
        }
        else
        {
            rageModeIndicator1.gameObject.SetActive(false);
            rageModeIndicator2.gameObject.SetActive(false);
            skillsText.color = Color.white;
            itemsText.color = Color.white;
            guardText.color = Color.white;
            RageOptionTextColor();
        }

        switch (playerIndex)
        {
            case 0:
                playerTurnIndicator.transform.position = playerIndicatorPos0.transform.position;
                break;
            case 1:
                playerTurnIndicator.transform.position = playerIndicatorPos1.transform.position;
                break;
            case 2:
                playerTurnIndicator.transform.position = playerIndicatorPos2.transform.position;
                break;
            case 3:
                playerTurnIndicator.transform.position = playerIndicatorPos3.transform.position;
                break;
        }
    }

    private void ChoosingBasicCommand()
    {
        if (!moveImagesNow) //Don't allow the player to choose a command until the Q has settled down
        {
            switch (controlsIndicator)
            {
                //If the player is in rage mode, only "Attack" can be chosen
                case 0://Highlighter is at attack
                    if (Input.GetKeyDown(KeyCode.DownArrow) && playerInControl.currentState!=Player.playerState.Rage)
                    {
                        controlsIndicator = 1;
                        highlighter.transform.position = highlighiterPos[1].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow) && playerInControl.currentState != Player.playerState.Rage)
                    {
                        controlsIndicator = 2;
                        highlighter.transform.position = highlighiterPos[2].transform.position;
                    }
                    else if(Input.GetKeyDown(KeyCode.LeftArrow) && playerInControl.canRage && playerInControl.currentState != Player.playerState.Rage)
                    {
                        controlsIndicator = 4;
                        highlighter.transform.position = highlighiterPos[4].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.Space)) //Player has chosen attack
                    {
                        previousState = btlUIState.choosingBasicCommand;
                        currentState = btlUIState.choosingEnemy;
                        chooseEnemyArrow.SetActive(true);
                        activeRange = playerInControl.range;

                        MoveEnemyIndicatorToFirstAliveEnemy();

                    }
                    break;

                case 1://Highlighter is at Guard
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        controlsIndicator = 0;
                        highlighter.transform.position = highlighiterPos[0].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        controlsIndicator = 3;
                        highlighter.transform.position = highlighiterPos[3].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.Space)) //Player has chosen Guard
                    {
                        UpdateActivityText("Guard");
                        playerInControl.Guard();
                    }
                    break;

                case 2:
                    //Highlighter is at Skills
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        controlsIndicator = 3;
                        highlighter.transform.position = highlighiterPos[3].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        controlsIndicator = 0;
                        highlighter.transform.position = highlighiterPos[0].transform.position;
                    }
                    else if(Input.GetKeyDown(KeyCode.RightArrow) && playerInControl.canRage)
                    {
                        controlsIndicator = 4;
                        highlighter.transform.position = highlighiterPos[4].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.Space)) //Player has chosen Skills
                    {
                        previousState = btlUIState.choosingBasicCommand;
                        if (!firstTimeOpenedSkillsPanel)
                        {
                            //Get the name of the skills and thier MP costs if this is the first time this player opens the skills panel
                            for (int i = 0; i < 4; i++)
                            {
                                skillNames[i].text = skills.SkillName(PartySkills.skills[playerInControl.playerIndex].equippedSkills[i]);
                                mpCosts[i].text = "MP: " + skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[i])[5].ToString();
                            }
                            firstTimeOpenedSkillsPanel = true;
                        }

                        skillsPanel.gameObject.SetActive(true);
                        currentState = btlUIState.choosingSkillsCommand;
                        skillsHighlighter.gameObject.transform.position = skillsHPos[0].transform.position;
                        skillDescription.text = skills.SkillDescription(PartySkills.skills[playerInControl.playerIndex].equippedSkills[0]).ToString();
                        skillAtkValueText.text = skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[0])[0].ToString();
                        skillWaitValueText.text = "Wait: " + skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[0])[2].ToString();
                        controlsIndicator = 0;
                    }
                    break;
                case 3:
                    //Highlighter is at Items
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        controlsIndicator = 2;
                        highlighter.transform.position = highlighiterPos[2].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        controlsIndicator = 1;
                        highlighter.transform.position = highlighiterPos[1].transform.position;
                    }
                    else if(Input.GetKeyDown(KeyCode.RightArrow) && playerInControl.canRage)
                    {
                        controlsIndicator = 4;
                        highlighter.transform.position = highlighiterPos[4].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.Space)) //Player has chosen Items
                    {
                        previousState = btlUIState.choosingBasicCommand;
                        //Reset the items panel
                        itemsPanel.gameObject.SetActive(true);
                        itemsHighlighter.gameObject.SetActive(true);
                        itemsHighlighter.transform.position = itemsHPos[0].transform.position;
                        itemDescription.text = inventory.ItemDescription(inventory.invItem[inventory.consumableInv[0], 0]);
                        itemsHighlighter.gameObject.transform.position = itemsHPos[0].transform.position;
                        itemHPosIndex = 0;
                        upArrow.gameObject.SetActive(false);
                        downArrow.gameObject.SetActive(true);
                        itemsPanelIndex = 0;
                        //Show three the first three items in the inventory
                        for(int i =0;i<3 && i<inventory.consumableInv.Count;i++)
                        {
                                itemIconsInPanel[i].sprite = itemIcons[inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 0]];
                                itemNames[i].text = inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 0]);
                                itemCount[i].text = inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 1].ToString();
                        }
                        currentState = btlUIState.choosingItemsCommand;
                    }
                    break;
                case 4://Hilighter is at RAGE
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        controlsIndicator = 3;
                        highlighter.transform.position = highlighiterPos[3].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        controlsIndicator = 2;
                        highlighter.transform.position = highlighiterPos[2].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        controlsIndicator = 0;
                        highlighter.transform.position = highlighiterPos[0].transform.position;
                    }
                    else if (Input.GetKeyDown(KeyCode.Space)) //Player has chosen Rage
                    {
                        rageText.color = Color.yellow;
                        skillsText.color = Color.gray;
                        itemsText.color = Color.gray;
                        guardText.color = Color.gray;
                        playerInControl.Rage(); //Go into rage mode
                        rageModeIndicator1.gameObject.SetActive(true);
                        rageModeIndicator2.gameObject.SetActive(true);
                        highlighter.transform.position = highlighiterPos[0].transform.position;
                        controlsIndicator = 0;
                    }
                    break;
            }
        }
    }

    private void ChoosingSkillsCommand()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            DisableActivtyText();
            skillsPanel.gameObject.SetActive(false);
            controlsIndicator = 2; //Back to skills in the choosingbasicommands
            currentState = btlUIState.choosingBasicCommand;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && controlsIndicator < 3)
        {
            //Make sure there's a skill below the one highlighted right now
            if (PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator + 1] != (int)SKILLS.NO_SKILL)
            {
                controlsIndicator++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && controlsIndicator >= 3)
        {
            controlsIndicator = 0;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow) && controlsIndicator > 0)
        {
            controlsIndicator--;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && controlsIndicator <= 0)
        {
            //Make sure the player goes to the last equipped skills, not necessarily number 3
            for(int i = 3; i>0; i--)
            {
                if (PartySkills.skills[playerInControl.playerIndex].equippedSkills[i] != (int)SKILLS.NO_SKILL)
                {
                    controlsIndicator = i;
                    break;
                }
            }            
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator] != (int)SKILLS.NO_SKILL)
            {
                //Choose skill, make sure you have enough MP to use it, then check if it targets players or enemies       
                if (playerInControl.currentMP >= skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[5])
                {
                    skillsPanel.gameObject.SetActive(false);
                    switch (skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[4])
                    {
                        case (float)SKILL_TYPE.SINGLE_TARGET_ATK:
                            MoveEnemyIndicatorToFirstAliveEnemy();
                            chooseEnemyArrow.SetActive(true);
                            previousState = btlUIState.choosingSkillsCommand;
                            currentState = btlUIState.choosingEnemy;
                            break;
                        case (float)SKILL_TYPE.SINGLE_TARGET_DEBUFF:
                            MoveEnemyIndicatorToFirstAliveEnemy();
                            chooseEnemyArrow.SetActive(true);
                            previousState = btlUIState.choosingSkillsCommand;
                            currentState = btlUIState.choosingEnemy;
                            break;
                        case (float)SKILL_TYPE.ALL_TARGETS_ATK:
                            //Activate the indicators for alive enemies only
                            for(int i = 0;i<enemiesDead.Length;i++)
                            {
                                if (enemiesDead[i] == false && btlManager.enemies[i].enemyReference!=null)
                                {
                                    chooseEnemyArrowForSelectAll[i].gameObject.SetActive(true);
                                }
                            }
                            previousState = btlUIState.choosingSkillsCommand;
                            currentState = btlUIState.choosingAllEnemies;
                            break;
                        case (float)SKILL_TYPE.ALL_TARGETS_DEBUFF:
                            break;
                        case (float)SKILL_TYPE.FULL_ROW_ATK:
                            break;
                        case (float)SKILL_TYPE.FULL_ROW_DEBUFF:
                            break;
                        case (float)SKILL_TYPE.SINGLE_PLAYER_HEAL:
                            choosePlayerArrow.gameObject.SetActive(true);
                            playerIndicatorIndex = 0;
                            choosePlayerArrow.transform.position = choosePlayerPos[0].transform.position;
                            previousState = btlUIState.choosingSkillsCommand;
                            currentState = btlUIState.choosingPlayer;
                            break;
                        case (float)SKILL_TYPE.ALL_PLAYER_HEAL:
                            break;
                        case (float)SKILL_TYPE.SINGLE_PLAYER_BUFF:
                            choosePlayerArrow.gameObject.SetActive(true);
                            playerIndicatorIndex = 0;
                            choosePlayerArrow.transform.position = choosePlayerPos[0].transform.position;
                            previousState = btlUIState.choosingSkillsCommand;
                            currentState = btlUIState.choosingPlayer;
                            break;
                        case (float)SKILL_TYPE.ALL_PLAYER_BUFF:
                            break;
                    }
                }
            }
        }

        skillsHighlighter.gameObject.transform.position = skillsHPos[controlsIndicator].transform.position;
        skillDescription.text = skills.SkillDescription(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator]).ToString();
        skillAtkValueText.text = "ATK: " + skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[0].ToString();
        skillWaitValueText.text = "Wait: " + skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[2].ToString();

    }

    private void ChoosingItemsCommand()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableActivtyText();
            itemsPanel.gameObject.SetActive(false);
            controlsIndicator = 3; //Back to items in the choosingbasicommands
            itemsPanelIndex = 0;
            currentState = btlUIState.choosingBasicCommand;
        }
        else if(Input.GetKeyDown(KeyCode.Space))//Player has chosen an item
        {
            //Make sure you choose an item that is usable and not equipable
            previousState = btlUIState.choosingItemsCommand; //Needed to know what to reference when choosing the player
            choosePlayerArrow.transform.position = choosePlayerPos[0].transform.position; //Move the player indicator on top of Fargas for now
            playerIndicatorIndex = 0;
            itemsPanel.gameObject.SetActive(false);
            choosePlayerArrow.gameObject.SetActive(true);
            currentState = btlUIState.choosingPlayer;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow) && itemsPanelIndex < inventory.consumableInv.Count)
        {
            //Keep track of where the highlighter is
            if (itemsPanelIndex + 1 < inventory.consumableInv.Count)
            {
                itemHPosIndex++;

                //Know which items to display
                itemsPanelIndex++;
            }

            if(itemHPosIndex==1)
            {
                itemsHighlighter.transform.position = itemsHPos[1].transform.position;
            }
            else if(itemHPosIndex==2)
            {
                itemsHighlighter.transform.position = itemsHPos[2].transform.position;
            }
            else
            {
                itemHPosIndex = 0;
                itemsHighlighter.transform.position = itemsHPos[0].transform.position;
            }


            itemDescription.text = inventory.ItemDescription(inventory.invItem[inventory.consumableInv[itemsPanelIndex],0]);
            if(itemsPanelIndex%3 == 0)
            {
                //We're mirroring the order of the items in the inventory UI and only showing usable items
                for (int i = 0; i < 3; i++)
                {
                    //Make sure we don't go out of bounds
                    if (itemsPanelIndex + i < inventory.consumableInv.Count)
                    {
                        itemIconsInPanel[i].sprite = itemIcons[inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 0]];
                        itemNames[i].text = inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 0]);
                        itemCount[i].text = inventory.invItem[inventory.consumableInv[itemsPanelIndex + i], 1].ToString();
                    }
                    else
                    {
                        itemIconsInPanel[i].sprite = itemIcons[0];
                        itemNames[i].text = "---";
                        itemCount[i].text = "---";

                    }
                }

                //Check if we're at the last three items -->This should be changed to work with the inventory count. Will change once we decide on the inventory count
                if (itemsPanelIndex >= inventory.consumableInv.Count - 3)
                {
                    downArrow.gameObject.SetActive(false);
                    upArrow.gameObject.SetActive(true);
                }
                else
                {
                    downArrow.gameObject.SetActive(true);
                    upArrow.gameObject.SetActive(true);
                }
            }

        }
        else if(Input.GetKeyDown(KeyCode.UpArrow) && itemsPanelIndex > 0)
        {
            if (itemsPanelIndex - 1 >= 0)
            {
                itemHPosIndex--;
                //Know which items to display
                itemsPanelIndex--;
            }



            itemDescription.text = inventory.ItemDescription(inventory.invItem[inventory.consumableInv[itemsPanelIndex],0]);
            //Check if we're back to the previous 3 items
            //Will turn it into a for loop when I'm smart enough
            if (itemsPanelIndex % 3 != 0 && itemHPosIndex < 0)
            {
                    itemIconsInPanel[0].sprite = itemIcons[inventory.invItem[inventory.consumableInv[itemsPanelIndex - 2], 0]];
                    itemNames[0].text = inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex - 2], 0]);
                    itemCount[0].text = inventory.invItem[inventory.consumableInv[itemsPanelIndex - 2], 1].ToString();

                    itemIconsInPanel[1].sprite = itemIcons[inventory.invItem[inventory.consumableInv[itemsPanelIndex - 1], 0]];
                    itemNames[1].text = inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex - 1], 0]);
                    itemCount[1].text = inventory.invItem[inventory.consumableInv[itemsPanelIndex - 1], 1].ToString();
             
                    itemIconsInPanel[2].sprite = itemIcons[inventory.invItem[inventory.consumableInv[itemsPanelIndex], 0]];
                    itemNames[2].text = inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex], 0]);
                    itemCount[2].text = inventory.invItem[inventory.consumableInv[itemsPanelIndex], 1].ToString();

                itemsHighlighter.transform.position = itemsHPos[2].transform.position;
                itemHPosIndex = 2;

                //Check if we're back to the very first three
                if (itemsPanelIndex == 2)
                {
                    downArrow.gameObject.SetActive(true);
                    upArrow.gameObject.SetActive(false);
                }
                else
                {
                    downArrow.gameObject.SetActive(true);
                    upArrow.gameObject.SetActive(true);
                }
            }
            else if (itemHPosIndex == 1)
            {
                itemsHighlighter.transform.position = itemsHPos[1].transform.position;
            }
            else if (itemHPosIndex == 0)
            {
                itemsHighlighter.transform.position = itemsHPos[0].transform.position;
            }
            
        }
    }

    private void ChoosingPlayer()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            DisableActivtyText();
            switch (playerInControl.playerIndex)
            {
                case 0:
                    playerTurnIndicator.transform.position = playerIndicatorPos0.transform.position;
                    break;
                case 1:
                    playerTurnIndicator.transform.position = playerIndicatorPos1.transform.position;
                    break;
                case 2:
                    playerTurnIndicator.transform.position = playerIndicatorPos2.transform.position;
                    break;
                case 3:
                    playerTurnIndicator.transform.position = playerIndicatorPos3.transform.position;
                    break;
            }
            if (previousState == btlUIState.choosingItemsCommand)
            {
                itemsPanel.gameObject.SetActive(true);
                currentState = btlUIState.choosingItemsCommand;
            }
            else if (previousState == btlUIState.choosingSkillsCommand)
            {
                skillsPanel.gameObject.SetActive(true);
                currentState = btlUIState.choosingSkillsCommand;
            }
        }
        if(playerIndicatorIndex == 0)
        {
            if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                playerIndicatorIndex = 1;//Go to Oberon
                choosePlayerArrow.transform.position = choosePlayerPos[1].transform.position;

            }
            else if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                playerIndicatorIndex = 3;//Go to Arcelus
                choosePlayerArrow.transform.position = choosePlayerPos[3].transform.position;
            }

        }
        else if (playerIndicatorIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                playerIndicatorIndex = 0;//Go to Fargas
                choosePlayerArrow.transform.position = choosePlayerPos[0].transform.position;

            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                playerIndicatorIndex = 2;//Go to Frea
                choosePlayerArrow.transform.position = choosePlayerPos[2].transform.position;
            }
        }
        else if (playerIndicatorIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                playerIndicatorIndex = 3;//Go to Arcelus
                choosePlayerArrow.transform.position = choosePlayerPos[3].transform.position;

            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                playerIndicatorIndex = 1;//Go to Oberon
                choosePlayerArrow.transform.position = choosePlayerPos[1].transform.position;
            }
        }
        else if (playerIndicatorIndex == 3)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                playerIndicatorIndex = 2;//Go to Frea
                choosePlayerArrow.transform.position = choosePlayerPos[2].transform.position;

            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                playerIndicatorIndex = 0;//Go to Fargas
                choosePlayerArrow.transform.position = choosePlayerPos[0].transform.position;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            choosePlayerArrow.gameObject.SetActive(false);
            if (previousState == btlUIState.choosingItemsCommand)
            {
                if (btlManager.players[playerIndicatorIndex].playerReference.currentState != Player.playerState.Rage)
                {
                    UpdateActivityText(inventory.ItemName(inventory.invItem[inventory.consumableInv[itemsPanelIndex], 0]));
                    inventory.ItemUseFunction(inventory.invItem[inventory.consumableInv[itemsPanelIndex], 0], inventory.consumableInv[itemsPanelIndex], playerIndicatorIndex);
                    btlManager.players[playerIndicatorIndex].playerReference.EnableEffect("Heal", inventory.itemAddAmount); //Update the heal text
                    itemCount[itemHPosIndex].text = inventory.invItem[inventory.consumableInv[0], 1].ToString();
                    itemsPanelIndex = 0; //Reset the itemsPanelIndex
                    btlManager.players[playerIndicatorIndex].playerReference.UpdatePlayerStats();
                    playerInControl.ForcePlayerTurnAnimationOff();
                    EndTurn();
                }
            }
            else if(previousState == btlUIState.choosingSkillsCommand)
            {
                if (btlManager.players[playerIndicatorIndex].playerReference.currentState != Player.playerState.Rage)
                {
                    UpdateActivityText((skills.SkillName(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])));
                    playerInControl.UseSkillOnOnePlayer(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator],
                                                        skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[5],
                                                        skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[2],
                                                        btlManager.players[playerIndicatorIndex].playerReference);
                }
            }
        }
    }

    private void ChoosingEnemy()
    {
        //Leave choosing enemy
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            DisableActivtyText();
            if (previousState == btlUIState.choosingBasicCommand)
            {
                currentState = btlUIState.choosingBasicCommand;
            }
            else if (previousState == btlUIState.choosingSkillsCommand)
            {
                skillsPanel.gameObject.SetActive(true);
                currentState = btlUIState.choosingSkillsCommand;
            }

        }

        switch (enemyIndicatorIndex)
        {
            case 0:
                if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (enemies[1] != null && enemiesDead[1]==false)
                    {
                        enemyIndicatorIndex = 1;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[1].transform.position;
                    }
                }
                else if((Input.GetKeyDown(KeyCode.RightArrow)|| Input.GetKeyDown(KeyCode.LeftArrow)) && (activeRange + playerInControl.initialPos >= 2))
                {
                    if (enemies[3] != null && enemiesDead[3] == false)
                    {
                        enemyIndicatorIndex = 3;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[3].transform.position;
                    }
                }
                else if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (enemies[2] != null && enemiesDead[2] == false)
                    {
                        enemyIndicatorIndex = 2;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[2].transform.position;
                    }
                }
                break;
            case 1:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (enemies[2] != null && enemiesDead[2] == false)
                    {
                        enemyIndicatorIndex = 2;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[2].transform.position;
                    }
                }
                else if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) && (activeRange + playerInControl.initialPos >= 2))
                {
                    if (enemies[4] != null && enemiesDead[4] == false)
                    {
                        enemyIndicatorIndex = 4;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[4].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (enemies[0] != null && enemiesDead[0] == false)
                    {
                        enemyIndicatorIndex = 0;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[0].transform.position;
                    }
                }
                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (enemies[0] != null && enemiesDead[0] == false)
                    {
                        enemyIndicatorIndex = 0;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[0].transform.position;
                    }
                }
                else if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) && (activeRange + playerInControl.initialPos >= 2))
                {
                    if (enemies[3] != null && enemiesDead[3] == false)
                    {
                        enemyIndicatorIndex = 3;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[3].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (enemies[1] != null && enemiesDead[1] == false)
                    {
                        enemyIndicatorIndex = 1;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[1].transform.position;
                    }
                }
                break;
            case 3:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (enemies[4] != null && enemiesDead[4] == false)
                    {
                        enemyIndicatorIndex = 4;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[4].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (enemies[0] != null && enemiesDead[0] == false)
                    {
                        enemyIndicatorIndex = 0;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[0].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (enemies[4] != null  && enemiesDead[4] == false)
                    {
                        enemyIndicatorIndex = 4;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[4].transform.position;
                    }
                }
                break;
            case 4:
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (enemies[3] != null && enemiesDead[3] == false)
                    {
                        enemyIndicatorIndex = 3;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[3].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (enemies[2] != null && enemiesDead[2] == false)
                    {
                        enemyIndicatorIndex = 2;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[2].transform.position;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (enemies[3] != null && enemiesDead[3] == false)
                    {
                        enemyIndicatorIndex = 3;
                        chooseEnemyArrow.transform.position = enemyIndicatorPosArray[3].transform.position;
                    }
                }
                break;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            chooseEnemyArrow.gameObject.SetActive(false);
            if (previousState == btlUIState.choosingBasicCommand)
            {
                UpdateActivityText("Attack");
                playerInControl.Attack(enemies[enemyIndicatorIndex]);
            }
            else if(previousState == btlUIState.choosingSkillsCommand)
            {
                Debug.Log("Enemy has been chosen");
                UpdateActivityText(skills.SkillName(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator]));
                playerInControl.UseSkillOnOneEnemy(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator],
                                                   skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[5],
                                                   skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[2],
                                                   enemies[enemyIndicatorIndex]);
            }
        }
    }

    private void ChoosingAllEnemies()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableActivtyText();
            if (previousState == btlUIState.choosingBasicCommand)
            {
                currentState = btlUIState.choosingBasicCommand;
            }
            else if (previousState == btlUIState.choosingSkillsCommand)
            {
                skillsPanel.gameObject.SetActive(true);
                currentState = btlUIState.choosingSkillsCommand;
            }

            for (int i = 0; i < chooseEnemyArrowForSelectAll.Length; i++)
            {
                chooseEnemyArrowForSelectAll[i].gameObject.SetActive(false);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            UpdateActivityText(skills.SkillName(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator]));
            playerInControl.UseSkillOnAllEnemies(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator],
                                               skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[5],
                                               skills.SkillStats(PartySkills.skills[playerInControl.playerIndex].equippedSkills[controlsIndicator])[2]);

            for (int i = 0; i < chooseEnemyArrowForSelectAll.Length; i++)
            {
                    chooseEnemyArrowForSelectAll[i].gameObject.SetActive(false);
            }
        }
    }

    public void RageOptionTextColor()
    {
        if (playerInControl.canRage)
        {
            rageText.color = Color.white;
        }
        else
        {
            rageText.color = Color.gray;
        }

    }

    public void UpdatePlayerHPControlPanel()
    {
        currentHP.text = Mathf.RoundToInt(playerInControl.currentHP).ToString();
        maxHP.text = " / " + Mathf.RoundToInt(playerInControl.maxHP).ToString();
        hpBar.fillAmount = playerInControl.hpImage.fillAmount;
    }

    public void UpdatePlayerMPControlPanel()
    {
        currentMP.text = Mathf.RoundToInt(playerInControl.currentMP).ToString();
        maxMP.text = " / " + Mathf.RoundToInt(playerInControl.maxMP).ToString();
        mpBar.fillAmount = playerInControl.currentMP / playerInControl.maxMP;
    }

    public void EndTurn()
    {
        if(playerInControl.currentState==Player.playerState.Rage)
        {
            //Make sure to turn off the indicators at the end of the turn, this is to make sure the end screen does not show the indicators
            rageModeIndicator1.gameObject.SetActive(false);
            rageModeIndicator2.gameObject.SetActive(false);
        }
        playerTurnIndicator.SetActive(false);
        chooseEnemyArrow.SetActive(false);
        controlsPanel.gameObject.SetActive(false);
        itemsPanel.gameObject.SetActive(false);
        moveImagesNow = true;
        firstTimeOpenedSkillsPanel = false; //Get ready for the next player in case they want to use thier skills
        controlsIndicator = 0;
        highlighter.gameObject.transform.position = highlighiterPos[0].transform.position;
        playerInControl.ForcePlayerTurnAnimationOff();
    }

    public void EnemyIsDead(int enemyIndex)
    {
        enemiesDead[enemyIndex] = true;
        numberOfDeadEnemies++;

        if(numberOfDeadEnemies >=numberOfEnemies)
        {
            fadePanel.FlipFadeToVictory();

            //Make sure to turn off the indicators at the end of the turn, this is to make sure the end screen does not show the indicators
            rageModeIndicator1.gameObject.SetActive(false);
            rageModeIndicator2.gameObject.SetActive(false);
            battleHasEnded = true;
            btlManager.EndOfBattle();
        }

       /* for(int i = 0, j=0; j<enemies.Length;j++)
        {
            if(enemies[j]!=null && enemiesDead[j]==true)
            {
                i++;
                if(i>=numberOfEnemies)
                {
                    Debug.Log("End it ");
                    //Start fading in the end battle screen
                    fadePanel.FlipFadeToVictory();
                    
                    //Make sure to turn off the indicators at the end of the turn, this is to make sure the end screen does not show the indicators
                    rageModeIndicator1.gameObject.SetActive(false);
                    rageModeIndicator2.gameObject.SetActive(false);
                    battleHasEnded = true;
                    btlManager.EndOfBattle();
                }
            }
        }
       */
    }

    private void EndBattleUI()
    {
            //If there's a conversation right after the battle, invoke the function in the dialogue manager
           // if(conversationAfterBattle)
           // {
           //     dialogueManager.StartConversation(dialogueContainer.doneFight);
           // }
       
    }

    public void StartShowingEndScreen(bool isVictory)
    {
        if(isVictory)
        {
            currentState = btlUIState.battleEnd;         
        }
        else
        {

        }
    }

    private void UpdatePlayerStats(int playerIndex)
    {
        btlManager.UpdatePlayerStats(playerIndex);
        playerInControl.ForcePlayerTurnAnimationOff();
    }

   
    private void MoveEnemyIndicatorToFirstAliveEnemy()
    {
        //Make sure the indicator starts at an alive enemy
        for (int i = 0; i < enemiesDead.Length; i++)
        {
            if (enemiesDead[i] == false)
            {
                chooseEnemyArrow.transform.position = enemyIndicatorPosArray[i].transform.position;
                enemyIndicatorIndex = i;
                break;
            }
        }
    }

    //Update the activity text
    public void UpdateActivityText(string activity)
    {
        Debug.Log("Update Text : " + activity);
        activtyTextBack.gameObject.SetActive(true);
        activityText.text = activity;
    }

    public void DisableActivtyText()
    {
        activityText.text = "";
        activtyTextBack.gameObject.SetActive(false);
        choosePlayerArrow.gameObject.SetActive(false);
        chooseEnemyArrow.gameObject.SetActive(false);
    }

}
