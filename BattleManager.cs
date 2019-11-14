using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    //Player information is updated first from the CharacterStatus struct at start
    [System.Serializable]
    public class PlayerInformtion
    {
        public int playerIndex;
        public float currentHP;
        public float maxHP;
        public float currentMP;
        public float maxMP;
        public string[] skills = new string[4];
        public float atk;
        public float def;
        public float agi;
        public float str;
        public float crit;
        public float speed;
        public string name;
        public int exp;
        public int expNeededForNextLevel;
        public Player playerReference;
        public Enemy enemyReference;
    }

    public static BattleManager instance;

    private UIBTL uiBtl;
    private EnemySpawner enemySpawner;
    private ExpManager expManager;

    public PlayerInformtion[] players;
    public PlayerInformtion[] enemies;
    private List<PlayerInformtion> battleQueue;

    private List<float> pSpeeds;
    private List<float> eSpeeds;

    private float maxPlayerSpeed = 0;
    private int maxPlayerIndex = 0;
    private int[] removedPlayerIndexes; //We need to keep track of which players and enemies have been accounted for in the queue
    private float maxEnemySpeed = 0;
    private int maxEnemyIndex = 0;
    private int[] removedEnemyIndexes;
    public int numberOfEnemies; // Updated from the world map. Need to make sure all enemies are added before building the Q
    private bool allEnemiesAdded = false; //Used to make sure that enemies and players are added before building the Q
    private bool allPlayersAdded = false;
    public int numberOfPlayers = 4; //Should be private. Public for testing purposes as its updated from the player's side

    public GameObject[] enemyPos = new GameObject[5]; //Used to inform the spawner where to place the enemies

    public bool addEnemies; //Turned true after all the players have been added
    //Temp
    private int totalLevels;//The sum of the enemies level
    public int expGain; //Determined by the enemy levels
    public int goldGain;

    //Battle progress
    public bool battleHasEnded;
    public static bool battleInProgress = false;

    private float dummyTimer = 5.0f;

    //At the beginning of each battle, each player and enemy will use the singleton to update their stats
    #region singleton

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        players = new PlayerInformtion[4]; //max 4 players
        enemies = new PlayerInformtion[5];//max 5 enemies
        pSpeeds = new List<float>();
        eSpeeds = new List<float>();
        battleQueue = new List<PlayerInformtion>();
        removedPlayerIndexes = new int[4];
        removedEnemyIndexes = new int[5];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerInformtion();
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i] = new PlayerInformtion();
        }

        for (int i = 0; i < removedPlayerIndexes.Length; i++)
        {
            removedPlayerIndexes[i] = -1;
        }
        for (int i = 0; i < removedEnemyIndexes.Length; i++)
        {
            removedEnemyIndexes[i] = -1;
        }
        addEnemies = false;

        expGain = 0;
        totalLevels = 0;
        
    }

    #endregion

    protected virtual void Start()
    {
       // //Debug.Log("BTL Start");
        uiBtl = UIBTL.instance;
        enemySpawner = EnemySpawner.instance;
        expManager = ExpManager.instance;
        uiBtl.numberOfEnemies = numberOfEnemies = enemySpawner.numberOfEnemies;
        ////Debug.Log("____ " + numberOfEnemies);
        for(int i =0;i<5;i++)
        {
            enemySpawner.AddPos(enemyPos[i], i);
        }
        //Each player and enemy would have an index that stores their information

        //Get player stats from partystats file
        for(int i=0;i<players.Length;i++)
        {
            UpdatePlayerStats(i);
        }
    }


    protected virtual void Update()
    {
       // //Debug.Log(numberOfPlayers);
        //Number of players is decreased by the player scripts
        if(numberOfPlayers<=0 && allPlayersAdded == false)
        {
            allPlayersAdded = true;
            addEnemies = true;
            battleHasEnded = false;
            for (int i = 0; i < 5; i++)
            {
                enemySpawner.AddPos(enemyPos[i],i);
            }
            enemySpawner.SpawnEnemies();

        }

        //Only start the battle when all players and enemies have been added
        if(allPlayersAdded && allEnemiesAdded)
        {
            //Debug.Log("Start Battle");
            StartBattle();
            allPlayersAdded = false;
            allEnemiesAdded = false;
            numberOfPlayers = 4; //Ready for the next battle
            addEnemies = false;
        }

    }

    //Called at the beginning of the battle to store references to current enemies. Needed to be able to update the queue
    public virtual void AddEnemy(int enemyIndex, int agi, int str, int crit, int speed, int currentHp, int maxHp, Enemy enemyRef, string name)
    {
            enemies[enemyIndex].playerIndex = enemyIndex;
            enemies[enemyIndex].agi = agi;
            enemies[enemyIndex].speed = speed;
            enemies[enemyIndex].str = str;
            enemies[enemyIndex].crit = crit;
            enemies[enemyIndex].currentHP = currentHp;
            enemies[enemyIndex].maxHP = maxHp;
            enemies[enemyIndex].enemyReference = enemyRef;
            enemies[enemyIndex].name = name;

        //Temp code
        totalLevels += enemyRef.eCurrentLevel;


        numberOfEnemies--; //Update the number of enemies after adding an enemy. This number is obtained from the WM

        if (numberOfEnemies<=0)
        {
            allEnemiesAdded = true;
            //Temp code
            expGain = 10 * totalLevels;
            goldGain = 5 * totalLevels;
            //Debug.Log("EXP GAINNN " + expGain);
            //Debug.Log("Gold GAINNN " + goldGain);
        }

        //This will probably need to change to avoid race conditions between startbattle and build Q
        uiBtl.enemies[enemyIndex] = enemyRef; //Update the UI system with the enemy
    }

    public virtual void StartBattle()
    {

        //Store and sort the agilities of the players and enemies in ascending order

        foreach (PlayerInformtion e in enemies)
        {
            if (e.enemyReference != null) //Make sure all the entries have enemies (i.e. what if we have less than 5 enemies)
            {
                eSpeeds.Add(e.speed);
            }
        }

        eSpeeds.Sort();

        foreach (PlayerInformtion p in players)
        {
            if (p.playerReference != null)//Make sure all the entries have players (i.e. what if we have less than 4 players)
            {
                pSpeeds.Add(p.speed);
            }
        }

        pSpeeds.Sort();


        BuildQueue();

        //Update the consumable inventory
        TellInventoryToUpdateConsumables();

        battleInProgress = true;

        if (!battleHasEnded)
        {
            NextOnQueue();
        }
    }

    public virtual void NextOnQueue()
    {
        //Check if the next on Q is a player or an enemy and call the correct function
        if (battleQueue[0].playerReference != null && battleQueue[0].enemyReference == null)
        {
            uiBtl.ShowThisPlayerUI(battleQueue[0].playerIndex, battleQueue[0].name, battleQueue[0].playerReference);
			DOM_playerTurn();

            //Add it to the end of the Q
            battleQueue.Add(battleQueue[0]);
            ////Debug.Log("I've added "  + battleQueue[battleQueue.Count - 1].name);
            //Remove it from the start of the Q 
            battleQueue.RemoveAt(0);
        }
        else if (battleQueue[0].playerReference == null && battleQueue[0].enemyReference != null)
        {
           // if (!battleQueue[0].enemyReference.dead)
           // {
                battleQueue[0].enemyReference.EnemyTurn();
                DOM_enemyTurn();

                //Add it to the end of the Q
                battleQueue.Add(battleQueue[0]);
                ////Debug.Log("I've added "  + battleQueue[battleQueue.Count - 1].name);
                //Remove it from the start of the Q 
                battleQueue.RemoveAt(0);
           // }
           // else
           // {
          //      //Add it to the end of the Q
               // battleQueue.Add(battleQueue[0]);
                ////Debug.Log("I've added "  + battleQueue[battleQueue.Count - 1].name);
                //Remove it from the start of the Q 
               // battleQueue.RemoveAt(0);
               // NextOnQueue();
          //  }
        }

    }

	protected virtual void DOM_playerTurn() {

	}
	
	protected virtual void DOM_enemyTurn() {

	}

    public virtual void BuildQueue()
    {
        //Compare the player with the highest speed to the enemy with the highest speed
        if (pSpeeds.Count > 0)
        {
            //Since the list is sorted, the last element has the highest speed
            maxPlayerSpeed = pSpeeds[pSpeeds.Count - 1];

            foreach (PlayerInformtion e in players)
            {
                //Is this the player that has the highest speed?
                if (maxPlayerSpeed == e.speed)
                {
                    //What if two players have the same speed? Make sure you are checking a different player each time
                    if (e.playerIndex == removedPlayerIndexes[0] || e.playerIndex == removedPlayerIndexes[1]
                        || e.playerIndex == removedPlayerIndexes[2] || e.playerIndex == removedPlayerIndexes[3])
                    {
                        //If it is a player we've already added to the queue, move on...
                        continue;
                        
                        
                    }
                    else
                    {
                        //Otherwise, this player is potentially the next on Q (still need to compare with the enemies)
                        maxPlayerIndex = e.playerIndex;                        
                    }
                    
                }
            }
        }

        if (eSpeeds.Count > 0)
        {
            //Enemy list is sorted. Last element has the highest speed
            maxEnemySpeed = eSpeeds[eSpeeds.Count - 1];

            foreach (PlayerInformtion e in enemies)
            {
                //Is this the enemy with the highest speed?
                if (maxEnemySpeed == e.speed)
                {
                    //What if two enemies have the same speed? Make sure we don't add the same enemy to the Q twice
                    if (e.playerIndex == removedEnemyIndexes[0] || e.playerIndex == removedEnemyIndexes[1] 
                        || e.playerIndex == removedEnemyIndexes[2] || e.playerIndex == removedEnemyIndexes[3] 
                        || e.playerIndex == removedEnemyIndexes[4])
                    {
                        //This enemy has already been added, move on
                        continue;
                    }
                    else
                    {
                        //New enemy to be added to the Q
                        maxEnemyIndex = e.playerIndex;
                    }
                   
                }
                
            }
        }

        //If both player agility and enemy agility lists are not empty, see which of them has the character with the highest agility
        if (pSpeeds.Count > 0 && eSpeeds.Count > 0)
        {
            if (maxPlayerSpeed >= maxEnemySpeed)
            {
               //The player has a higher agility
               battleQueue.Add(players[maxPlayerIndex]);
               //Add the player's image to the UI
               uiBtl.AddImageToQ(players[maxPlayerIndex].playerReference.qImage, players[maxPlayerIndex].playerIndex, true);
               //Remove the player's agility from the list
               pSpeeds.RemoveAt(pSpeeds.Count - 1);
               //Add the player's index to the array of removed players
               removedPlayerIndexes[maxPlayerIndex] = maxPlayerIndex;

            }
            else
            {
                //The enemy has the higher agility
                battleQueue.Add(enemies[maxEnemyIndex]);
                //Add the enemy's image to the UI
                uiBtl.AddImageToQ(enemies[maxEnemyIndex].enemyReference.qImage, enemies[maxEnemyIndex].playerIndex, false);
                //Debug.Log(enemies[maxEnemyIndex].enemyReference.qImage);
                //Remove the enemy's agility from the list
                eSpeeds.RemoveAt(eSpeeds.Count - 1);
                //Add the enemy's index to the array of removed enemy
                removedEnemyIndexes[maxEnemyIndex] = maxEnemyIndex;

            }
        }
        //If all the enemies have been added to the Q already, add the remaining players to the Q directly
        else if(pSpeeds.Count>0 && eSpeeds.Count<=0)
        {
            battleQueue.Add(players[maxPlayerIndex]);
            //Add the player's image to the UI
            uiBtl.AddImageToQ(players[maxPlayerIndex].playerReference.qImage, players[maxPlayerIndex].playerIndex, true);
            pSpeeds.RemoveAt(pSpeeds.Count - 1);
            removedPlayerIndexes[maxPlayerIndex] = maxPlayerIndex;
        }
        //If all the players have already been added to the Q, add the remaining enemies to the Q directly.
        else if(pSpeeds.Count<=0 && eSpeeds.Count>0)
        {
            battleQueue.Add(enemies[maxEnemyIndex]);
            //Add the enemy's image to the UI
            uiBtl.AddImageToQ(enemies[maxEnemyIndex].enemyReference.qImage, enemies[maxEnemyIndex].playerIndex,false);
            //Debug.Log(enemies[maxEnemyIndex].enemyReference.qImage);
            eSpeeds.RemoveAt(eSpeeds.Count - 1);
            removedEnemyIndexes[maxEnemyIndex] = maxEnemyIndex;
        }

        //If either of the agility lists isn't empty, run the function again
        if (pSpeeds.Count>0 || eSpeeds.Count>0)
        {
            BuildQueue();
        }
        //Otherwise, tell the UI system to show the Q
        else
        {
            uiBtl.QueueIsReady();
        }
    }


    //Called by each player at the end of each battle
    public virtual void EndOfBattle(bool victory)
    {
        battleHasEnded = true;
        battleInProgress = false;
        if (victory)
        { //Only if it's a victory, update the party stats
            for (int i = 0; i < 4; i++)
            {
                //Update the remaining HP of players in the btl manager and the partystats
                if(players[i].playerReference.currentHP<=0.0f)
                {
                    players[i].playerReference.currentHP = 10.0f; //If a player dies, they leave the battle with 10 hp
                }
                players[i].currentHP = PartyStats.chara[i].hitpoints = players[i].playerReference.currentHP;
                players[i].currentMP = PartyStats.chara[i].magicpoints = players[i].playerReference.currentMP;
                //If the player ended the battle in rage mode, reset the rage to 0
                if (players[i].playerReference.currentState == Player.playerState.Rage)
                {
                    PartyStats.chara[i].rage = 0.0f;
                    players[i].playerReference.canRage = false;
                }
                
            }
        }
        enemySpawner.numberOfEnemies = 0; //Reset the enemy spawner to get ready for the next battle

        //Clear out the enemies array
        for (int i =0;i<5;i++)
        {
            if(enemies[i].enemyReference != null)
            Destroy(enemies[i].enemyReference.gameObject);
            enemies[i] = null;
        }
    }

    //Called by the exp manager on awake and when the player's level changes
    public virtual void UpdateFromExp(int playerIndex, int currentExp, int maxExp)
    {
        players[playerIndex].exp = currentExp;
        players[playerIndex].expNeededForNextLevel = maxExp;

        //Update UI for max EXP
    }
    

    public virtual void LevelUp(int playerIndex)
    {
        //Called from the Victory Screen
        //The new EXP is what remains after reaching the new level
        players[playerIndex].exp = players[playerIndex].exp - players[playerIndex].expNeededForNextLevel;
        //Update the party stats
        PartyStats.chara[playerIndex].currentExperience = players[playerIndex].exp;
        //Update UI and do level up audio

        //Call the EXP manager to level up --> This should update the expNeededForNextLevel
        expManager.LevelUp(playerIndex);
        //Update the needed exp for next levelup
        players[playerIndex].expNeededForNextLevel = PartyStats.chara[playerIndex].neededExperience;
        //Debug.Log("You need this much to level up again! " + players[playerIndex].expNeededForNextLevel);
    }

    public virtual void UpdatePlayerStats(int playerIndex)
    {
        players[playerIndex].currentHP = PartyStats.chara[playerIndex].hitpoints;
        players[playerIndex].maxHP = PartyStats.chara[playerIndex].TotalMaxHealth;
        players[playerIndex].currentMP = PartyStats.chara[playerIndex].magicpoints;
        players[playerIndex].maxMP = PartyStats.chara[playerIndex].TotalMaxMana;
        players[playerIndex].atk = PartyStats.chara[playerIndex].TotalAttack;
        players[playerIndex].def = PartyStats.chara[playerIndex].TotalDefence;
        players[playerIndex].agi = PartyStats.chara[playerIndex].TotalAgility;
        players[playerIndex].crit = PartyStats.chara[playerIndex].TotalCritical;
        players[playerIndex].str = PartyStats.chara[playerIndex].TotalStrength;
        players[playerIndex].speed = PartyStats.chara[playerIndex].TotalSpeed;
        players[playerIndex].exp = PartyStats.chara[playerIndex].currentExperience;
        players[playerIndex].expNeededForNextLevel = PartyStats.chara[playerIndex].neededExperience;
    }

    protected virtual void TellInventoryToUpdateConsumables()
    {

        var length = MainInventory.INVENTORY_SIZE;
        for (int i = 0; i < length; i++)
        {
            if (MainInventory.invInstance.ItemType(MainInventory.invInstance.invItem[i, 0]) == (int)ITEM_TYPE.CONSUMABLE)
            {
                MainInventory.invInstance.consumableInv.Add(i);
            }
        }
        //Debug.Log("Consumables Count is: " + MainInventory.invInstance.consumableInv.Count);
    }
}
