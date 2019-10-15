using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Instances
    private BattleManager battleManager;
    private UIBTL uiBTL;
    private ObjectPooler objPooler;
    private SkillsInventory skills;

    //Player stats
    public float atk;
    public float def;
    public float currentHP;
    public float maxHP;
    public bool canRage; //Turned true once the current rage reaches the max rage. Used by UIBTL
    public float currentMP;
    public float maxMP;
    public float agi;
    public float str;
    public float crit;
    public float speed;
    public int playerIndex;
    public string name;
    public int range; //Range of player standard attack
    public int initialPos; //Position of the player 0 being Frontline and -1 being Ranged
    public bool dead;

    //Queue
    public Sprite qImage;
    private int QCounter; //Used to count turns since the player went in rage or decided to go in waiting state.

    //Components
    private Animator playerAnimator;

    //Skills

    //Skill target is used when calling skills to know what we are targeting
    //0: Single enemy attack
    //1: Full row enemies attack
    //2: All enemies attack
    //3: Single enemy debuff
    //4: Full row enemies debuff
    //5: All enemies debuff
    //6: Single player heal
    //7: All players heal
    //8: Single player buff
    //9: All players buff

    private int chosenSkill;
    private int skillTarget; 
    private float mpCost;
    private float skillWaitTime;
    private Player healThisPlayer;
    private string skillNameForObjPooler;
    private int skillWaitingIndex = 0;
    private string skillAnimatorName = "";

    //Buffs
    //Booleans are used in case the player's stats were debuffed by an enemy and when buffed, the debuff effects will be negated. The Q counter will not be affected
    private bool defenseBuffed = false;
    private bool attackBuffed = false;
    private bool agilityBuffed = false;
    private float defenseBuffSkillQCounter = 0 ; //How many turns until the defense buff is reversed. Need three counters as multiple stats could be buffed/debuffed at the same time
    private float attackBuffSkillQCounter = 0;
    private float agilityBuffSkillQCounter = 0;

    //Rage
    public float currentRage;
    public float maxRage;


    //Effects
    //Gurad
    public GameObject guardIcon;
    //Rage
    public GameObject rageModeIndicator;
    //Heal
    public GameObject healEffect;
    //Buffs
    public GameObject defBuffEffect;
    public GameObject atkBuffEffect;
    public GameObject agiBuffEffect;

    //UI
    public Image hpImage;
    public Image rageImage;
    public Text damageText;
    public Text healText;
    public Text waitTimeText;

    //Camera
    public BattleCamera btlCam;

    //Actual stats --> Stats after they've been modified in battle
    private float actualATK;
    private float actualDEF;
    private float actualAgi;
    private float actualCRIT;
    private float actualSTR;
    public bool healable; //False when dead and in rage mode

    //Targeted enemy info
    private Enemy attackingThisEnemy;
    private bool hit; //Hit or miss  

    //Guarding
    private float actualDefBeforeGuard; //What if the player uses a def-increasing skill before making this character go to guard?
                                        //Should be updated correctly if a player is guarding and while doing so gets his/her def increased


    public enum playerState
    {
        Idle, //Player has not issued a command
        Guard, //When a player issues a guard command, lasts until the next turn of this character
        Waiting, //When a player issues a skill that takes more than 1 turn to execute
        Dead, //When a character's HP reaches zero, that character's turn is skipped
        Rage //When in rage mode, the player's attack is doubled, the defense halved, and the player becomes unhealable, and cannot use skills or guard.
    }

    public playerState currentState;

    private void Start()
    {
        //Instances
        battleManager = BattleManager.instance;
        uiBTL = UIBTL.instance;
        objPooler = ObjectPooler.instance;
        skills = SkillsInventory.invInstance;

        //States
        currentState = playerState.Idle;

        //Components
        playerAnimator = gameObject.GetComponent<Animator>();

        //Skills
        chosenSkill = 0;

        //Rage
        maxRage = maxHP * 0.65f; //Max rage is 65% of max health
        canRage = false;

        //Effects
        //Guard
        guardIcon.gameObject.SetActive(false);
        //Rage
        rageModeIndicator.gameObject.SetActive(false);
        //Heal
        healEffect.gameObject.SetActive(false);
        defBuffEffect.gameObject.SetActive(false);
        atkBuffEffect.gameObject.SetActive(false);
        agiBuffEffect.gameObject.SetActive(false);

        //Targeted enemy info
        attackingThisEnemy = null;
        hit = false;

        //Get the stats from the party stats function
        StartBattle();


        //Actual stats
        actualATK = atk;
        actualDefBeforeGuard = actualDEF = def;
        actualAgi = agi;
        actualCRIT = crit;
        actualSTR = str;
        healable = false;
        dead = false;

        //UI
        hpImage.fillAmount = currentHP / maxHP;
        rageImage.fillAmount = currentRage / maxRage;
        damageText.gameObject.SetActive(false);
        healText.gameObject.SetActive(false);
        waitTimeText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            //Testing the damage formula and rage calculations
            TakeDamage(30.0f);
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            Heal(0.2f);
        }
    }

    private void StartBattle()
    {

        //Get the information from the party stats file
        UpdatePlayerStats();
        battleManager.players[playerIndex].playerIndex = playerIndex;
        battleManager.players[playerIndex].playerReference = this;
        battleManager.players[playerIndex].name = name;
        battleManager.numberOfPlayers--;

        //Update skills
    }

    //Called from the UIBTL to turn on the animation
    public void PlayerTurn()
    {
        if (!dead)
        {
            playerAnimator.SetBool("Turn", true);

            //If the it's my turn again, and I have been guarding, end the guard since guarding only lasts for 1 turn
            if (currentState == playerState.Guard)
            {
                EndGuard();
            }
            else if(currentState == playerState.Waiting)
            {
                skillWaitTime--;

                if(skillWaitTime<=0)
                {
                    waitTimeText.gameObject.SetActive(false);
                    switch (skillTarget) //use the skill target to know which function to call
                    {
                        case 0:
                            UseSkillOnOneEnemy(chosenSkill, mpCost, 0, attackingThisEnemy);
                            break;
                        case 1:
                            break;
                        case 2:
                            UseSkillOnAllEnemies(chosenSkill, mpCost, 0);
                            break;
                        case 3:
                            break;
                        case 4:
                            break;
                        case 5:
                            break;
                        case 6:
                            UseSkillOnOnePlayer(chosenSkill, mpCost, 0, healThisPlayer);
                            break;
                        case 7:
                            break;
                        case 8:
                            UseSkillOnOnePlayer(chosenSkill, mpCost, 0, healThisPlayer);
                            break;
                        case 9:
                            break;
                    }
                    
                }
                else
                {
                    waitTimeText.text = skillWaitTime.ToString();
                    uiBTL.EndTurn();
                }
            }

            //Check for buffs
            CheckForBuffs();
           
        }
        else
        {
            uiBTL.EndTurn();
        }
    }

    //Called from the UI
    public void Attack(Enemy enemyRef)
    {
        playerAnimator.SetBool("Turn", false);
        playerAnimator.SetBool("Attack", true);
        attackingThisEnemy = enemyRef;
    }
    //Called from the animator
    private void CompleteAttack()
    {
        playerAnimator.SetBool("Attack", false);
        //Check hit or miss
        CalculateHit();
        if (hit)
        {
            //Attack Effect
            objPooler.SpawnFromPool("PlayerNormalAttack", attackingThisEnemy.gameObject.transform.position, gameObject.transform.rotation);
            //Shake the camera
            btlCam.CameraShake();
            //Check for critical hits
            if (CalculateCrit() <= crit)
            {
                attackingThisEnemy.TakeDamage(actualATK * 1.2f);
            }
            else
            {
                attackingThisEnemy.TakeDamage(actualATK);
            }
        }
        //uiBTL.EndTurn();

        //If the player is in rage state, they can only attack so it makes sense to check if we were in rage mode when attacking
        if(currentState==playerState.Rage)
        {
            QCounter++;
            if(QCounter>=3) //If it's been 3 or more turns since the player raged out, reset rage mode
            {
                ResetPlayerRage();
            }
        }

    }

    //Calculate whether the attack is a hit or a miss
    private void CalculateHit()
    {
        //20 sided die + str <? enemy agility
        if(Random.Range(0.0f,20.0f) + str < attackingThisEnemy.eAgility)
        {
            hit = false;
        }
        else
        {
            hit = true;
        }

        Debug.Log("Hit is " + hit);
    }

    private void CalculateHitForSkill()
    {
        //20 sided die + skill accuracy <? enemy agility
        if (Random.Range(0.0f, 20.0f) + skills.SkillStats(chosenSkill)[1] < attackingThisEnemy.eAgility)
        {
            hit = false;
        }
        else
        {
            hit = true;
        }

        Debug.Log("Hit is " + hit);
    }

    //Overloaded function called when targeting all enemies
    private void CalculateHitForSkill(Enemy target)
    {
        //20 sided die + skill accuracy <? enemy agility
        if (Random.Range(0.0f, 20.0f) + skills.SkillStats(chosenSkill)[1] < target.eAgility)
        {
            hit = false;
        }
        else
        {
            hit = true;
        }

        Debug.Log("Hit is " + hit);
    }

    private float CalculateCrit()
    {
        return Random.Range(0.0f, 100.0f);
    }

    //Guard and End Guard are called from the UI. End Guard is called when the player's turn returns
    public void Guard()
    {
        actualDEF = actualDefBeforeGuard * 1.5f;
        currentState = playerState.Guard;
        playerAnimator.SetBool("Turn", false);
        Debug.Log(name + " is Guarding and current def is " + actualDEF);
        guardIcon.gameObject.SetActive(true);
        uiBTL.EndTurn();
    }
    public void EndGuard()
    {
        actualDEF = actualDefBeforeGuard;
        currentState = playerState.Idle;
        guardIcon.gameObject.SetActive(false);
    }

    public void TakeDamage(float enemyATK)
    {
        btlCam.CameraShake();
        float damage = enemyATK - ((def / (20.0f + def)) * enemyATK);
        currentHP -= damage;
        damageText.gameObject.SetActive(true);
        damageText.text = Mathf.RoundToInt(damage).ToString();
        battleManager.players[playerIndex].currentHP = currentHP; //Update the BTL manager with the new health

        if (currentHP <= 0.0f)
        {
            hpImage.fillAmount = 0.0f;
            dead = true;
            playerAnimator.SetBool("Dead", true);
        }
        else
        {
            hpImage.fillAmount = currentHP / maxHP;
        }
        if (currentRage < maxRage && currentState!=playerState.Rage) //If there's still capacity for rage while we're not actually in rage, increase the rage meter
        {
            currentRage += damage * 1.05f; //Rage amount is always 5% more than the health lost
            rageImage.fillAmount = currentRage / maxRage;
            
            if(currentRage>=maxRage)
            {
                currentRage = maxRage;
                canRage = true; //Can now go into rage mode
            }
            PartyStats.chara[playerIndex].rage = currentRage; //Update the party stats
        }

        if (currentState != playerState.Waiting)
        {
            playerAnimator.SetBool("Hit", true);
        }
    }

    private void EndHit()
    {
        playerAnimator.SetBool("Hit", false);
    }


    //Called by the UIBTl when the player chooses to go into rage mode
    public void Rage()
    {
        actualATK = atk * 2.0f;
        actualDEF = def / 2.0f;
        healable = false;
        QCounter = 0; //Reset the QCounter
        rageModeIndicator.gameObject.SetActive(true);
        currentState = playerState.Rage;
    }

    public void ResetPlayerRage()
    {
        Debug.Log("Rage has cooled down");
        currentRage = 0.0f;
        PartyStats.chara[playerIndex].rage = currentRage;
        actualATK = atk;
        actualDEF = def;
        healable = true;
        canRage = false;
        QCounter = 0;
        rageModeIndicator.gameObject.SetActive(false);
        rageImage.fillAmount = 0.0f;  //Update the UI
        currentState = playerState.Idle;
    }

    //Called whenever a player is healed, or stats their stats changed
    public void UpdatePlayerStats()
    {
        currentHP = PartyStats.chara[playerIndex].hitpoints;
        maxHP = PartyStats.chara[playerIndex].TotalMaxHealth;
        currentMP = PartyStats.chara[playerIndex].magicpoints;
        maxMP = PartyStats.chara[playerIndex].TotalMaxMana;
        atk = PartyStats.chara[playerIndex].TotalAttack;
        def = PartyStats.chara[playerIndex].TotalDefence;
        agi = PartyStats.chara[playerIndex].TotalAgility;
        crit = PartyStats.chara[playerIndex].TotalCritical;
        str = PartyStats.chara[playerIndex].TotalStrength;
        speed = PartyStats.chara[playerIndex].TotalSpeed;
        currentRage = PartyStats.chara[playerIndex].rage;

        hpImage.fillAmount = currentHP / maxHP;
        rageImage.fillAmount = currentRage / maxRage;

        if(currentRage>=maxRage)
        {
            currentRage = maxRage;
            canRage = true;
        }
        else
        {
            canRage = false;
        }
    }

    public void ForcePlayerTurnAnimationOff()
    {
        playerAnimator.SetBool("Turn", false);
    }

    //---------------------------------------------------Skills---------------------------------------------------//
    public void UseSkillOnOnePlayer(int skillID, float manaCost , float waitTime, Player playerReference)
    {
        Debug.Log("Skill Target" + skillID);

        skillWaitTime = waitTime;
        chosenSkill = skillID;
        mpCost = manaCost;
        healThisPlayer = playerReference;

        //Check if the skill is immediate or if the player needs to wait a number of turns
        if (skillID == 4) //Heal
        {
            skillTarget = 6;//Single player heal
            skillAnimatorName = "Heal";
            skillWaitingIndex = 1;
        }
        else if (skillID == 3) //Buff defense skill
        {
            skillTarget = 8; //Single player buff
            skillAnimatorName = "BuffDef";
            skillWaitingIndex = 1;
        }

        //If there's waiting time, go to wait state and end the turn 
        if (waitTime <= 0)
        {
            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", 0);
            playerAnimator.SetBool(skillAnimatorName, true);
        }
        else
        {
            waitTimeText.gameObject.SetActive(true);
            waitTimeText.text = skillWaitTime.ToString();
            playerAnimator.SetInteger("WaitingIndex", skillWaitingIndex);
            currentState = playerState.Waiting;
            uiBTL.EndTurn();
        }
    }

    public void UseSkillOnOneEnemy(int skillID, float manaCost, float waitTime, Enemy enemyReference)
    {

        skillWaitTime = waitTime;
        skillTarget = 0;
        chosenSkill = skillID;
        mpCost = manaCost;
        attackingThisEnemy = enemyReference;

        //Check which skill to know which animation to run
        if (skillID == 1 || skillID == 2) //Fargas and Freya basic attack skills
        {
            Debug.Log("HIT");
            skillNameForObjPooler = "FFSkill1";
            skillAnimatorName = "ASkill";
            skillWaitingIndex = 1; //Should there be waiting time, this index is used to know which waiting animation to go to
        }

        //Do we have to wait?
        if (waitTime <= 0)
        {

            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", 0);
            playerAnimator.SetBool(skillAnimatorName, true);
        }
        else
        {
            //If there's waiting time, go to wait state and end the turn 
            waitTimeText.gameObject.SetActive(true);
            waitTimeText.text = skillWaitTime.ToString();
            playerAnimator.SetInteger("WaitingIndex", skillWaitingIndex);
            currentState = playerState.Waiting;
            uiBTL.EndTurn();
        }

    }

    public void UseSkillOnAllEnemies(int skillID, float manaCost, float waitTime)
    {
        skillWaitTime = waitTime;
        skillTarget = 2;
        chosenSkill = skillID;
        mpCost = manaCost;

        //Check which skill to know which animation to run
        if (skillID == 1) //Fargas  basic attack skill --> Placeholder will be changed once we have the actual skill
        {
            Debug.Log("HIT");
            skillNameForObjPooler = "FFSkill1";
            skillAnimatorName = "ASkill";
            skillWaitingIndex = 1;
        }

        if(waitTime<=0)
        {
            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", -1);
            playerAnimator.SetBool(skillAnimatorName, true);
        }
        else
        {
            //If there's waiting time, go to wait state and end the turn 
            waitTimeText.gameObject.SetActive(true);
            waitTimeText.text = skillWaitTime.ToString();
            playerAnimator.SetInteger("WaitingIndex", skillWaitingIndex);
            currentState = playerState.Waiting;
            uiBTL.EndTurn();
        }
    }

    //Called from the animator
    public void SkillEffect()
    {
        //Skill target is used when calling skills to know what we are targeting
        //0: Single enemy attack
        //1: Full row enemies attack
        //2: All enemies attack
        //3: Single enemy debuff
        //4: Full row enemies debuff
        //5: All enemies debuff
        //6: Single player heal
        //7: All players heal
        //8: Single player buff
        //9: All players buff

        if (skillTarget == 0) // Damaging one enemy
        {
            Debug.Log("Damage enemy");
            CalculateHitForSkill();
            if(hit)
            {
                objPooler.SpawnFromPool(skillNameForObjPooler, attackingThisEnemy.gameObject.transform.position, gameObject.transform.rotation);
                Debug.Log("Skill hit");
                //Summon effect here
                btlCam.CameraShake();
                if(CalculateCrit()<=crit)
                {
                    Debug.Log("Skill Crit");
                    attackingThisEnemy.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0]); //Damage is the half the player's attack stat and the skill's attack stat
                }
                else
                {
                    Debug.Log("No Skill Crit");
                    attackingThisEnemy.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0]); //Damage is the half the player's attack stat and the skill's attack stat
                }

            }
            else
            {
                Debug.Log("Skill miss");
            }


            playerAnimator.SetBool(skillAnimatorName, false);

        }
        else if(skillTarget == 2)
        {
            for(int i = 0; i<battleManager.enemies.Length; i++)
            {
                if(battleManager.enemies[i].enemyReference!=null)
                {
                    if(!battleManager.enemies[i].enemyReference.dead)
                    {
                        CalculateHitForSkill(battleManager.enemies[i].enemyReference);
                        if(hit)
                        {
                            objPooler.SpawnFromPool(skillNameForObjPooler, battleManager.enemies[i].enemyReference.gameObject.transform.position, gameObject.transform.rotation);
                            Debug.Log("Skill hit");
                            //Summon effect here
                            btlCam.CameraShake();
                            if (CalculateCrit() <= crit)
                            {
                                Debug.Log("Skill Crit");
                                battleManager.enemies[i].enemyReference.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0]); //Damage is the half the player's attack stat and the skill's attack stat
                            }
                            else
                            {
                                Debug.Log("No Skill Crit");
                                battleManager.enemies[i].enemyReference.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0]); //Damage is the half the player's attack stat and the skill's attack stat
                            }
                        }
                        else
                        {
                            Debug.Log("Skill miss");
                        }
                    }
                }
            }
            playerAnimator.SetBool(skillAnimatorName, false);
        }
        else if(skillTarget == 6)
        {
            healThisPlayer.Heal(0.1f * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0]));
            playerAnimator.SetBool("Heal", false);
        }
        else if(skillTarget == 8) //Buff single player
        {
            if(chosenSkill == 3)//Defense Buff
            {
                healThisPlayer.BuffStats("Defense", skills.SkillStats(chosenSkill)[0], skills.SkillStats(chosenSkill)[2]);
                playerAnimator.SetBool("BuffDef", false);
            }

        }

        currentMP -= mpCost;
        battleManager.players[playerIndex].currentMP = currentMP;
        uiBTL.UpdatePlayerMPControlPanel();
        currentState = playerState.Idle;
    }

    //Heal function. Different heal skills will heal the player by different percentages
    public void Heal(float percentage)
    {
        EnableEffect("Heal", 0);
        float healAmount = percentage * maxHP;
        currentHP += healAmount;
        healText.gameObject.SetActive(true);
        healText.text = Mathf.RoundToInt(healAmount).ToString();
        battleManager.players[playerIndex].currentHP = currentHP;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        //If the player could rage, now they could not since they healed
        if (canRage)
        {
            canRage = false;
            uiBTL.RageOptionTextColor();
        }

        currentRage -= healAmount * 1.5f; //Rage goes down by 20% more than the health gained

        if (currentRage < 0.0f)
        {
            currentRage = 0.0f;
        }

        //Update the UI
        hpImage.fillAmount = currentHP / maxHP;
        rageImage.fillAmount = currentRage / maxRage;
        uiBTL.UpdatePlayerHPControlPanel();
        PartyStats.chara[playerIndex].rage = currentRage; //Update the party stats
        uiBTL.EndTurn(); //End the turn of the current player (i.e. the healer) after the healing is done
    }

    public void BuffStats(string statToBuff, float amount, float lastsNumberOfTurns)
    {
        lastsNumberOfTurns++; //Add one more turn since the system should count the number of turns based on the caster not the receiver. This way ensures that the queue goes around equal to the number of turns it the buff/debuff is supposed to last
        switch(statToBuff)
        {
            case "Defense":
                if(defenseBuffed && actualDEF<def) //Check for debuffs first
                {
                    actualDEF = def;
                    defenseBuffSkillQCounter = 0; //Negate the debuff completely
                }
                else if(defenseBuffed && actualDEF > def) //If defense has already been buffed, update the Q counter
                {
                    defenseBuffSkillQCounter = lastsNumberOfTurns;
                }
                else if(!defenseBuffed) //No buffs or debuffs have occurred so far
                {
                    EnableEffect("DefBuff",0);
                    defenseBuffed = true;
                    actualDEF = def + amount;
                    defenseBuffSkillQCounter = lastsNumberOfTurns;

                    Debug.Log("Actual defense now for " + name + " is: " + actualDEF);
                    Debug.Log("Counter: " + defenseBuffSkillQCounter);
                }
                break;
            case "Attack":
                if (attackBuffed && actualATK < def) //Check for debuffs first
                {
                    actualATK = atk;
                    attackBuffSkillQCounter = 0; //Negate the debuff completely
                }
                else if (attackBuffed && actualATK > def) //If attack has already been buffed, update the Q counter
                {
                    attackBuffSkillQCounter = lastsNumberOfTurns;
                }
                else if (!attackBuffed) //No buffs or debuffs have occurred so far
                {
                    attackBuffed = true;
                    actualATK = atk + amount;
                    attackBuffSkillQCounter = lastsNumberOfTurns;
                }
                break;
            case "Agility":
                if (agilityBuffed && actualAgi < def) //Check for debuffs first
                {
                    actualAgi = agi;
                    agilityBuffSkillQCounter = 0; //Negate the debuff completely
                }
                else if (agilityBuffed && actualAgi > def) //If agility has already been buffed, update the Q counter
                {
                    agilityBuffSkillQCounter = lastsNumberOfTurns;
                }
                else if (!agilityBuffed) //No buffs or debuffs have occurred so far
                {
                    agilityBuffed = true;
                    actualAgi = agi + amount;
                    agilityBuffSkillQCounter = lastsNumberOfTurns;
                }
                break;
        }

        uiBTL.EndTurn(); //End the turn of the current player (i.e. the buffer) when the buffing is done
    }

    private void CheckForBuffs()
    {
        if (defenseBuffed && defenseBuffSkillQCounter > 0)
        {
            defenseBuffSkillQCounter--;
            Debug.Log("Buff counter: " + defenseBuffSkillQCounter);
            if (defenseBuffSkillQCounter <= 0)
            {
                defenseBuffSkillQCounter = 0;
                defenseBuffed = false;
                actualDEF = def;
                Debug.Log("Buff has ended");
            }
        }

        if (attackBuffed && attackBuffSkillQCounter > 0)
        {
            attackBuffSkillQCounter--;
            if (attackBuffSkillQCounter <= 0)
            {
                attackBuffSkillQCounter = 0;
                attackBuffed = false;
                actualATK = atk;
            }
        }

        if (agilityBuffed && agilityBuffSkillQCounter > 0)
        {
            agilityBuffSkillQCounter--;
            if (agilityBuffSkillQCounter <= 0)
            {
                agilityBuffSkillQCounter = 0;
                agilityBuffed = false;
                actualAgi = agi;
            }
        }
    }

    public void EnableEffect(string effectName, int value)
    {
        switch(effectName)
        {
            case "Heal":
                healEffect.gameObject.SetActive(true);
                if(value > 0)
                {
                    healText.gameObject.SetActive(true);
                    healText.text = value.ToString();
                }
                break;
            case "DefBuff":
                defBuffEffect.gameObject.SetActive(true);
                break;
            case "AtkBuff":
                atkBuffEffect.gameObject.SetActive(true);
                break;
            case "AgiBuff":
                agiBuffEffect.gameObject.SetActive(true);
                break;
        }
    }
}
