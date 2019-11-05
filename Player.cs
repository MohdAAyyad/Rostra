///Note: This player script contains information that is used by all 4 player characters. The reason for this is
///that the UIBTL needs to talk to the players. Making it a unified script seemed easier when we started rather than attempting to know
///which of the 4 player scripts should be made active.
///This is not the corect way to do it alas it's too late to change it now as it works. 
///Will need to build a hierarchy and rely on virtual functions instead of cramming everything onto one huge script
///My bad - Ayyad


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
    public string nameOfCharacter;
    public int range; //Range of player standard attack
    public int initialPos; //Position of the player 0 being Frontline and -1 being Ranged
    public bool dead;

    //Status ailments
    public enum playerAilments
    {
        none,
        fear,
        tied,
    }
    public playerAilments currentAilment = playerAilments.none; //You an only be affected by one ailment at a time

    //Fear
    private int fearTimer = 0; //Used to disable the ailment after a certain number of turns have been skipped due to fear
    public GameObject fearSymbol;
    private int fearChance = 0; //If a player is inflicted with Fear, see if they should skip their turn
    private bool affectedByFear = false;

    //Tied
    private Enemy tiedToThisEnemy = null; //Should the player be tied to an enemy, the enemy should be healed by half the heal amount the player recieves when healed
    public GameObject tiedSymbol;


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
    private string skillObjectForObjPooler; //In case you're summong objects alongside the effects
    private string skillNameForObjPooler; // Skill effect
    private int skillWaitingIndex = 0;
    private string skillAnimatorName = "";
    private int enemyRowIndicator = 0; //Used to know the enemy row the player is attacking
    private bool showSkillNameAfterWait = false; //If the player waits before executing the attack, make sure to show the name when the skill is executed
    private int numberOfAttacks = 0; //Some skills have the character attack twice

    //Buffs
    //Booleans are used in case the player's stats were debuffed by an enemy and when buffed, the debuff effects will be negated. The Q counter will not be affected
    private bool defenseBuffed = false;
    private bool attackBuffed = false;
    private bool agilityBuffed = false;
    private bool strBuffed = false;
    private bool drainEye = false;
    private float drainEyeModifier = 0.2f; //Used to calculate the heal percentage
    private float defenseBuffSkillQCounter = 0; //How many turns until the defense buff is reversed. Need three counters as multiple stats could be buffed/debuffed at the same time
    private float attackBuffSkillQCounter = 0;
    private float agilityBuffSkillQCounter = 0;
    private float strBuffSkillQCounter = 0;
    private float drainEyeSkillQCounter = 0;

    //Rage
    public float currentRage;
    public float maxRage;


    //Effects
    //Guard
    public GameObject guardIcon;
    //Rage
    public GameObject rageModeIndicator;
    //Heal
    public GameObject healEffect;
    //MPHeal
    public GameObject mpEffect;
    //Revive
    public GameObject hopeEffect;
    //Buffs
    public GameObject defBuffEffect;
    public GameObject atkBuffEffect;
    public GameObject agiBuffEffect;
    public GameObject strBuffEffect;
    public GameObject drainEyeBuffEffect;
    //Debuffs
    public SpriteRenderer debuffArrow;
    public GameObject atkBuffArrowIndicator;
    public GameObject defBuffArrowIndicator;
    public GameObject agiBuffArrowIndicator;
    public GameObject strBuffArrowIndicator;
    private Quaternion arrowRotator;
    private Color debuffColor;


    //UI
    public Image hpImage;
    public Image rageImage;
    public Image mpImage;
    public Text damageText;
    public Text healText;
    public Text mpText;
    public Text skillText;
    private string skillTextValue = "";
    public Text waitTimeText;

    //Camera
    public BattleCamera btlCam;

    //Actual stats --> Stats after they've been modified in battle
    private float actualATK;
    private float actualDEF;
    private float actualAgi;
    private float actualCRIT;
    private float actualSTR;
    public bool healable; //False when dead or in rage mode

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
        mpEffect.gameObject.SetActive(false);
        hopeEffect.gameObject.SetActive(false);
        defBuffEffect.gameObject.SetActive(false);
        atkBuffEffect.gameObject.SetActive(false);
        agiBuffEffect.gameObject.SetActive(false);
        strBuffEffect.gameObject.SetActive(false);
        drainEyeBuffEffect.gameObject.SetActive(false);
        //MP Heal

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
        mpImage.fillAmount = currentMP / maxMP;
        rageImage.fillAmount = currentRage / maxRage;
        damageText.gameObject.SetActive(false);
        healText.gameObject.SetActive(false);
        skillText.gameObject.SetActive(false);
        mpText.gameObject.SetActive(false);
        waitTimeText.gameObject.SetActive(false);
        debuffArrow.gameObject.SetActive(false);
        debuffColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        arrowRotator = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        atkBuffArrowIndicator.gameObject.SetActive(false);
        defBuffArrowIndicator.gameObject.SetActive(false);
        agiBuffArrowIndicator.gameObject.SetActive(false);
        strBuffArrowIndicator.gameObject.SetActive(false);

        //Ailments
        fearSymbol.gameObject.SetActive(false);
        tiedSymbol.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //Testing the damage formula and rage calculations
            TakeDamage(30.0f);
        }

        if (Input.GetKeyDown(KeyCode.H))
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
        battleManager.players[playerIndex].name = nameOfCharacter;
        battleManager.numberOfPlayers--;

        //Update skills
    }

    //Called from the UIBTL to turn on the animation
    public void PlayerTurn()
    {
        if (!dead)
        {
            playerAnimator.SetBool("Turn", true);

            CheckForAilments();

            //If the it's my turn again, and I have been guarding, end the guard since guarding only lasts for 1 turn
            if (currentState == playerState.Guard)
            {
                EndGuard();
            }
            else if (currentState == playerState.Waiting && !affectedByFear)
            {
                skillWaitTime--;

                if (skillWaitTime <= 0)
                {
                    waitTimeText.gameObject.SetActive(false);
                    switch (skillTarget) //use the skill target to know which function to call
                    {
                        case 0:
                            UseSkillOnOneEnemy(chosenSkill, mpCost, 0, attackingThisEnemy);
                            break;
                        case 1:
                            UseSkillOnEnemyRow(chosenSkill, mpCost, 0, enemyRowIndicator);
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
                            UseSkillOnAllPlayers(chosenSkill, mpCost, 0);
                            break;
                        case 8:
                            UseSkillOnOnePlayer(chosenSkill, mpCost, 0, healThisPlayer);
                            break;
                        case 9:
                            UseSkillOnAllPlayers(chosenSkill, mpCost, 0);
                            break;
                    }

                    uiBTL.UpdateActivityText(skills.SkillName(chosenSkill));

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

    #region attack
    //Called from the UI
    public void Attack(Enemy enemyRef)
    {
        numberOfAttacks = 0; //Just a precaution
        chosenSkill = (int)SKILLS.NO_SKILL; //It's a normal attack, make sure CompleteAttack works correctly
        playerAnimator.SetBool("Turn", false);
        playerAnimator.SetBool("Attack", true);
        attackingThisEnemy = enemyRef;
    }
    //Called from the animator
    private void CompleteAttack()
    {
        numberOfAttacks--;
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
                if (chosenSkill == (int)SKILLS.NO_SKILL) //In case the player's skill uses the same animation as the normal attack animation
                {
                    attackingThisEnemy.TakeDamage(actualATK * 1.2f, numberOfAttacks);
                    if (drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * (drainEyeModifier * (1.2f * actualATK)));
                    }
                }
                else
                {
                    attackingThisEnemy.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks);
                    if (drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                    }
                }
            }
            else
            {
                if (chosenSkill == (int)SKILLS.NO_SKILL)
                {
                    attackingThisEnemy.TakeDamage(actualATK, numberOfAttacks);
                    if (drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * 0.1f * actualATK);
                    }
                }
                else
                {
                    attackingThisEnemy.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks);
                    if (drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * (drainEyeModifier * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0])));
                    }
                }
            }

        }
        else
        {
            Debug.Log("Player attack has missed");
            if (numberOfAttacks <= 0)
            {
                uiBTL.EndTurn();
            }
        }

        //If the player is in rage state, they can only attack so it makes sense to check if we were in rage mode when attacking
        if (currentState == playerState.Rage)
        {
            QCounter++;
            if (QCounter >= 3) //If it's been 3 or more turns since the player raged out, reset rage mode
            {
                ResetPlayerRage();
            }
        }

        if (numberOfAttacks <= 0) //Check that all attacks have been done
        {
            playerAnimator.SetBool("Attack", false);
        }

    }
    #endregion

    #region hit and crit
    //Calculate whether the attack is a hit or a miss
    private void CalculateHit()
    {
        //20 sided die + str <? enemy agility
        if (Random.Range(0.0f, 20.0f) + str < attackingThisEnemy.eAgility)
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

    private void EndHit()
    {
        playerAnimator.SetBool("Hit", false);
    }

    #endregion

    #region Guard
    //Guard and End Guard are called from the UI. End Guard is called when the player's turn returns
    public void Guard()
    {
        actualDefBeforeGuard = actualDEF; //Store the current defense before multiplying it
        actualDEF = actualDefBeforeGuard * 1.5f;
        currentState = playerState.Guard;
        playerAnimator.SetBool("Turn", false);
        guardIcon.gameObject.SetActive(true);
        uiBTL.EndTurn();
    }
    public void EndGuard()
    {
        actualDEF = actualDefBeforeGuard;
        currentState = playerState.Idle;
        guardIcon.gameObject.SetActive(false);
    }

    #endregion

    #region take damage
    public void TakeDamage(float enemyATK)
    {
        btlCam.CameraShake();
        float damage = enemyATK - ((def / (20.0f + def)) * enemyATK);
        currentHP -= damage;
        damageText.gameObject.SetActive(true);
        damageText.text = Mathf.RoundToInt(damage).ToString();
        battleManager.players[playerIndex].currentHP = currentHP; //Update the BTL manager with the new health
        PartyStats.chara[playerIndex].hitpoints = currentHP; //Update the party stats

        if (currentHP <= 0.0f)
        {
            currentHP = 0.0f;
            battleManager.players[playerIndex].currentHP = currentHP; //Update the BTL manager with the new health
            PartyStats.chara[playerIndex].hitpoints = currentHP; //Update the party stats
            hpImage.fillAmount = 0.0f;
            dead = true;
            DisableAllBuffs(); //BUffs should not continue beyond death
            playerAnimator.SetBool("Dead", true);
            //If you die, you lose your RAGE
            if (currentState == playerState.Waiting) //If the player is waiting on a skill, reset everything and die
            {
                Debug.Log("Huh?");
                chosenSkill = (int)SKILLS.NO_SKILL;
                skillWaitTime = 0;
                skillWaitingIndex = 0;
                waitTimeText.gameObject.SetActive(false);
                playerAnimator.SetInteger("WaitingIndex", 0);

            }
            ResetPlayerRage();
            uiBTL.PlayerIsDead(playerIndex);
        }
        else
        {
            hpImage.fillAmount = currentHP / maxHP;
        }
        if (currentRage < maxRage && currentState != playerState.Rage) //If there's still capacity for rage while we're not actually in rage, increase the rage meter
        {
            currentRage += damage * 1.05f; //Rage amount is always 5% more than the health lost
            rageImage.fillAmount = currentRage / maxRage;

            if (currentRage >= maxRage)
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

    //Take Damage override to include debuffs
    public void TakeDamage(float enemyAtk,int debuffIndex, string debuffSubIndex, playerAilments ailment, Enemy enemyReference, float debuffValuePercent, int debuffTimer, bool affectNonGuardOnly)
    {
        if (enemyAtk > 0)
        {
            TakeDamage(enemyAtk); //Call the original one since the player needs to lose health first should the attack passed on be higher than zero
        }

        switch(debuffIndex) //Is it a regular debuff or an ailment?
        {
            case 0: //Ailment
                switch(ailment)
                {
                    case playerAilments.fear:
                        //Get the fear timer and activate the object
                        currentAilment = playerAilments.fear;
                        fearTimer = debuffTimer;
                        Debug.Log(nameOfCharacter + "IS SPOOKED");
                        fearSymbol.gameObject.SetActive(true);
                        break;
                    case playerAilments.tied:
                        currentAilment = playerAilments.tied;
                        tiedToThisEnemy = enemyReference;
                        tiedSymbol.gameObject.SetActive(true);
                        break;
                }
                break;
            case 1: //Regular
                        if(affectNonGuardOnly) //If this boolean is true, make sure to only affect characters in a non-guard state
                        {
                            if (currentState != playerState.Guard)
                            {
                                BuffStats(debuffSubIndex, -debuffValuePercent, debuffTimer);
                            }
                        }
                        else
                        {

                            BuffStats(debuffSubIndex, -debuffValuePercent, debuffTimer);
                        }
                break;
        }
    }

    #endregion

    #region RAGE
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
    #endregion
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
        mpImage.fillAmount = currentMP / maxMP;
        rageImage.fillAmount = currentRage / maxRage;

        if (currentRage >= maxRage)
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
    public void UseSkillOnOnePlayer(int skillID, float manaCost, float waitTime, Player playerReference)
    {

        skillWaitTime = waitTime;
        chosenSkill = skillID;
        mpCost = manaCost;
        healThisPlayer = playerReference;

        //Check if the skill is immediate or if the player needs to wait a number of turns
        if (skillID == (int)SKILLS.Ar_Heal) //Heal
        {
            skillTarget = 6;//Single player heal
            skillAnimatorName = "Heal";
            skillWaitingIndex = 1;
        }
        else if(skillID == (int)SKILLS.Ar_ManaCharge)
        {
            skillTarget = 6;//Single player heal
            skillAnimatorName = "Heal";
            skillWaitingIndex = 2; //Mana heal wait is 2 per the animator
        }
        else if(skillID == (int)SKILLS.Ar_LullabyOfHope)
        {
            skillTarget = 6;//Single player heal
            skillAnimatorName = "Heal";
            skillWaitingIndex = 3; //Lullaby of Hope wait is 3 per the animator
            skillTextValue = "Don't give up!";
        }
        else if(skillID == (int)SKILLS.Ar_DrainEye)
        {
            skillTarget = 8; //Single player buff
            skillAnimatorName = "DrainEye";
            skillWaitingIndex = 1; //No wait for draineye
        }
        else if (skillID == (int)SKILLS.Ob_ShieldAlly) //Buff defense skill
        {
            skillTarget = 8; //Single player buff
            skillAnimatorName = "BuffDef";
            skillWaitingIndex = 1;
        }
        else if (skillID == (int)SKILLS.Fr_IDontMiss) //I Don't Miss special skill
        {
            skillTarget = 8; //Single player buff
            BuffStats("Strength", skills.SkillStats(chosenSkill)[0], 3);
            chosenSkill = (int)SKILLS.NO_SKILL;
            currentMP -= mpCost;
            mpImage.fillAmount = currentMP / maxMP;
            battleManager.players[playerIndex].currentMP = currentMP;
            PartyStats.chara[playerIndex].magicpoints = currentMP;
            uiBTL.UpdatePlayerMPControlPanel();
            currentState = playerState.Idle;


        }

        //If there's waiting time, go to wait state and end the turn 
        if (waitTime <= 0)
        {
            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", 0);
            if (skillAnimatorName != "")
            {
                playerAnimator.SetBool(skillAnimatorName, true);
            }
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

    public void UseSkillOnAllPlayers(int skillID, float manaCost, float waitTime)
    {
        skillWaitTime = waitTime;
        chosenSkill = skillID;
        mpCost = manaCost;

        //Placeholders, these skills will be a single character heal/buff
        //Check if the skill is immediate or if the player needs to wait a number of turns
        if (skillID == (int)SKILLS.Ar_HealingAura) //Heal 
        {
            skillTarget = 7;//All player heal
            skillAnimatorName = "Heal";
            skillWaitingIndex = 1;
        }
        else if (skillID == (int)SKILLS.Ob_ShieldAllAllies) //Buff defense skill
        {
            skillTarget = 9; //All player buff
            skillAnimatorName = "BuffDef";
            skillWaitingIndex = 1;
        }

        if (waitTime <= 0)
        {
            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", -1); //-1 is (All) animation
            playerAnimator.SetBool(skillAnimatorName, true);
        }
        else
        {
            Debug.Log("Now");
            waitTimeText.gameObject.SetActive(true);
            waitTimeText.text = skillWaitTime.ToString();
            playerAnimator.SetInteger("WaitingIndex", skillWaitingIndex);
            currentState = playerState.Waiting;
            uiBTL.EndTurn();
        }

    }

    public void UseSkillOnOneEnemy(int skillID, float manaCost, float waitTime, Enemy enemyReference)
    {
        Debug.Log("Skill ID is: " + skillID);
        skillWaitTime = waitTime;
        skillTarget = 0;
        chosenSkill = skillID;
        mpCost = manaCost;
        attackingThisEnemy = enemyReference;

        switch(skillID)
        {
            case (int)SKILLS.Fr_DoubleShot: //Double shot is a normal attack done twice
                Debug.Log("Inside HITTT ");
                numberOfAttacks = 2;
                playerAnimator.SetBool("Turn", false);
                skillAnimatorName = "Attack";
                currentMP -= mpCost;
                mpImage.fillAmount = currentMP / maxMP;
                battleManager.players[playerIndex].currentMP = currentMP;
                PartyStats.chara[playerIndex].magicpoints = currentMP;
                break;
            case (int)SKILLS.Fr_PiercingShot:
                skillNameForObjPooler = "FFSkill1";
                skillAnimatorName = "ASkill";
                skillWaitingIndex = 1; //Should there be waiting time, this index is used to know which waiting animation to go to
                break;
        }

        //Do we have to wait?
        if (waitTime <= 0)
        {
            Debug.Log("Further Inside HITTT ");
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
        switch (skillID)
        {
            case (int)SKILLS.Fr_ArrowRain:
                skillObjectForObjPooler = "ArrowRain";
                skillNameForObjPooler = "ArrowImpact";
                skillAnimatorName = "ASkill";
                skillWaitingIndex = 1;
                break;
            case (int)SKILLS.Fr_NeverAgain:
                skillObjectForObjPooler = "NeverAgain";
                skillNameForObjPooler = "ArrowImpactNG";
                skillAnimatorName = "ASkill";
                skillTextValue = "Die!";
                skillWaitingIndex = -2;
                break;
            case (int)SKILLS.Ar_Armageddon:
                skillObjectForObjPooler = "ArmFire";
                skillNameForObjPooler = "ArmImpact";
                skillAnimatorName = "Heal";
                skillTextValue = "Burn!";
                skillWaitingIndex = 4; //Armageddon is 4
                break;
        }

        if (waitTime <= 0)
        {
            skillWaitingIndex = 0;
            playerAnimator.SetInteger("WaitingIndex", -1); // (All attacks) run when the index is -1
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

    public void UseSkillOnEnemyRow(int skillID, float manaCost, float waitTime, int rowIndicator)
    {
        skillWaitTime = waitTime;
        skillTarget = 1;
        chosenSkill = skillID;
        mpCost = manaCost;
        enemyRowIndicator = rowIndicator;

        //Check which skill to know which animation to run
        if (skillID == (int)SKILLS.Fa_SwiftStrike) 
        {
            Debug.Log("HIT");
            skillNameForObjPooler = "FFSkill1";
            skillAnimatorName = "ASkill";
            skillWaitingIndex = 1;
        }

        if (waitTime <= 0)
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

        //Check if there's a skill text
        if(skillTextValue != "")
        {
            skillText.gameObject.SetActive(true);
            skillText.text = skillTextValue;
            skillTextValue = "";
        }
        if (skillTarget == 0) // Damaging one enemy
        {
            Debug.Log("Damage enemy");
            CalculateHitForSkill();
            if (hit)
            {
                objPooler.SpawnFromPool(skillNameForObjPooler, attackingThisEnemy.gameObject.transform.position, gameObject.transform.rotation);
                Debug.Log("Skill hit");
                //Summon effect here
                btlCam.CameraShake();
                if (CalculateCrit() <= crit)
                {
                    Debug.Log("Skill Crit");
                    attackingThisEnemy.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                    if(drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                    }
                }
                else
                {
                    Debug.Log("No Skill Crit");
                    attackingThisEnemy.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                    if (drainEye) //Check if Drain Eye is active
                    {
                        Heal(0.01f * (drainEyeModifier * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0])));
                    }
                }

            }
            else
            {
                Debug.Log("Skill miss");
                uiBTL.EndTurn();
            }


            playerAnimator.SetBool(skillAnimatorName, false);

        }
        else if (skillTarget == 1) //Full row attack
        {
            uiBTL.UpdateNumberOfEndTurnsNeededToEndTurn(enemyRowIndicator); //Tell the UIBTL which row you're attacking
            if (enemyRowIndicator == 0)
            {
                for (int i = 0; i < 3; i++) //Front row
                {
                    if (battleManager.enemies[i].enemyReference != null)
                    {
                        if (!battleManager.enemies[i].enemyReference.dead)
                        {
                            CalculateHitForSkill(battleManager.enemies[i].enemyReference);
                            if (hit)
                            {
                                objPooler.SpawnFromPool(skillNameForObjPooler, battleManager.enemies[i].enemyReference.gameObject.transform.position, gameObject.transform.rotation);
                                Debug.Log("Skill hit");
                                //Summon effect here
                                btlCam.CameraShake();
                                if (CalculateCrit() <= crit)
                                {
                                    Debug.Log("Skill Crit");
                                    battleManager.enemies[i].enemyReference.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                                    if (drainEye) //Check if Drain Eye is active
                                    {
                                        Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                    }
                                }
                                else
                                {
                                    Debug.Log("No Skill Crit");
                                    battleManager.enemies[i].enemyReference.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                                    if (drainEye) //Check if Drain Eye is active
                                    {
                                        Heal(0.01f * (drainEyeModifier * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Skill miss");
                                uiBTL.EndTurn();
                            }
                        }
                    }
                }
                playerAnimator.SetBool(skillAnimatorName, false);
                playerAnimator.SetInteger("WaitingIndex", 0);
            }
            else if (enemyRowIndicator == 1) //Ranged row
            {
                for (int i = 3; i < 5; i++)
                {
                    if (battleManager.enemies[i].enemyReference != null)
                    {
                        if (!battleManager.enemies[i].enemyReference.dead)
                        {
                            CalculateHitForSkill(battleManager.enemies[i].enemyReference);
                            if (hit)
                            {
                                objPooler.SpawnFromPool(skillNameForObjPooler, battleManager.enemies[i].enemyReference.gameObject.transform.position, gameObject.transform.rotation);
                                Debug.Log("Skill hit");
                                //Summon effect here
                                btlCam.CameraShake();
                                if (CalculateCrit() <= crit)
                                {
                                    Debug.Log("Skill Crit");
                                    battleManager.enemies[i].enemyReference.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                                    if (drainEye) //Check if Drain Eye is active
                                    {
                                        Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                    }
                                }
                                else
                                {
                                    Debug.Log("No Skill Crit");
                                    battleManager.enemies[i].enemyReference.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                                    if (drainEye) //Check if Drain Eye is active
                                    {
                                        Heal(0.01f * (drainEyeModifier * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Skill miss");
                                uiBTL.EndTurn();

                            }
                        }
                    }
                }
            }
            playerAnimator.SetBool(skillAnimatorName, false);
            playerAnimator.SetInteger("WaitingIndex", 0);
        }
        else if (skillTarget == 2) //All enemies attack
        {
            uiBTL.UpdateNumberOfEndTurnsNeededToEndTurn(2);
            for (int i = 0; i < battleManager.enemies.Length; i++)
            {
                if (battleManager.enemies[i].enemyReference != null)
                {
                    if (!battleManager.enemies[i].enemyReference.dead)
                    {
                        CalculateHitForSkill(battleManager.enemies[i].enemyReference);
                        if (hit)
                        {
                            if (skillObjectForObjPooler != "")
                            {
                                objPooler.SpawnFromPool(skillObjectForObjPooler, battleManager.enemies[i].enemyReference.gameObject.transform.position, gameObject.transform.rotation);
                            }
                            objPooler.SpawnFromPool(skillNameForObjPooler, battleManager.enemies[i].enemyReference.gameObject.transform.position, gameObject.transform.rotation);
                            Debug.Log("Skill hit");
                            //Summon effect here
                            btlCam.CameraShake();
                            if (CalculateCrit() <= crit)
                            {
                                Debug.Log("Skill Crit");
                                battleManager.enemies[i].enemyReference.TakeDamage(0.7f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is half the player's attack stat and the skill's attack stat
                                if (drainEye) //Check if Drain Eye is active
                                {
                                    Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                }
                            }
                            else
                            {
                                Debug.Log("No Skill Crit");
                                battleManager.enemies[i].enemyReference.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0], numberOfAttacks); //Damage is the half the player's attack stat and the skill's attack stat
                                if (drainEye) //Check if Drain Eye is active
                                {
                                    Heal(0.01f * (drainEyeModifier * (0.7f * actualATK + skills.SkillStats(chosenSkill)[0])));
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Skill miss");
                            uiBTL.EndTurn();
                        }
                    }
                }
            }
            playerAnimator.SetBool(skillAnimatorName, false);
            playerAnimator.SetInteger("WaitingIndex", 0);
        }
        else if (skillTarget == 6) //Heal one player
        {
            if (chosenSkill == (int)SKILLS.Ar_Heal)
            {
                healThisPlayer.Heal(0.01f * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0])); //Passing in a percentage
                playerAnimator.SetBool("Heal", false);
            }
            else if(chosenSkill == (int)SKILLS.Ar_ManaCharge)
            {
                healThisPlayer.ManaCharge(0.01f * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0]));
                playerAnimator.SetBool("Heal", false);
            }
            else if(chosenSkill == (int)SKILLS.Ar_LullabyOfHope)
            {
                healThisPlayer.RevivePlayer(0.5f); //Revive a player with half their HP
                playerAnimator.SetBool("Heal", false);
            }
        }
        else if (skillTarget == 7) //Heal all players
        {
            if (chosenSkill == (int)SKILLS.Ar_HealingAura)
            {
                for (int i = 0; i < battleManager.players.Length; i++)
                {
                    if (battleManager.players[i].playerReference != null)
                    {
                        if (!battleManager.players[i].playerReference.dead && battleManager.players[i].playerReference.currentState != playerState.Rage)
                        {
                            battleManager.players[i].playerReference.Heal(0.01f * (0.5f * actualATK + skills.SkillStats(chosenSkill)[0]));
                        }
                    }
                }
                playerAnimator.SetBool("Heal", false);
            }
        }
        else if (skillTarget == 8) //Buff single player
        {
            if (chosenSkill == (int)SKILLS.Ob_ShieldAlly)//Defense Buff
            {
                healThisPlayer.BuffStats("Defense", skills.SkillStats(chosenSkill)[0], 3);
                playerAnimator.SetBool("BuffDef", false);
            }
            else if(chosenSkill == (int)SKILLS.Ar_DrainEye)
            {
                healThisPlayer.BuffStats("DrainEye", skills.SkillStats(chosenSkill)[0], 3);
                playerAnimator.SetBool("DrainEye", false);
            }

        }
        else if (skillTarget == 9) //Buff all players
        {
            if (chosenSkill == (int)SKILLS.Ob_ShieldAllAllies) //Defense buff // Placeholder
            {
                for (int i = 0; i < battleManager.players.Length; i++)
                {
                    if (battleManager.players[i].playerReference != null)
                    {
                        //Make sure the character being buffed is alive and not in RAGE mode
                        if (!battleManager.players[i].playerReference.dead && battleManager.players[i].playerReference.currentState != playerState.Rage)
                        {
                            battleManager.players[i].playerReference.BuffStats("Defense", skills.SkillStats(chosenSkill)[0], 3);
                        }
                    }
                }
                playerAnimator.SetBool("BuffDef", false);
            }
        }
        //Claculate the new MP and reset the player's state
        chosenSkill = (int)SKILLS.NO_SKILL;
        currentMP -= mpCost;
        mpImage.fillAmount = currentMP / maxMP;
        battleManager.players[playerIndex].currentMP = currentMP;
        PartyStats.chara[playerIndex].magicpoints = currentMP;
        uiBTL.UpdatePlayerMPControlPanel();
        currentState = playerState.Idle;
    }

    //Heal function. Different heal skills will heal the player by different percentages
    public void Heal(float percentage)
    {
        EnableEffect("Heal", 0);
        Debug.Log("Percentageeeee " + percentage);
        float healAmount = percentage * maxHP;
        Debug.Log("Heal amountttt +++ " + healAmount);
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

        if(currentAilment == playerAilments.tied)
        {
            tiedToThisEnemy.HealDueToTied(healAmount);
        }

        //Update the UI
        hpImage.fillAmount = currentHP / maxHP;
        rageImage.fillAmount = currentRage / maxRage;
        uiBTL.UpdatePlayerHPControlPanel();
        PartyStats.chara[playerIndex].rage = currentRage; //Update the party stats
        PartyStats.chara[playerIndex].hitpoints = currentHP;
        uiBTL.EndTurn(); //End the turn of the current player (i.e. the healer) after the healing is done
    }

    //Increase the player's MP by a percentage.
    public void ManaCharge(float percentage)
    {
        EnableEffect("MP", 0);
        float mpAmount = percentage * maxMP;
        currentMP += mpAmount;
        mpImage.fillAmount = currentMP / maxMP;
        mpText.gameObject.SetActive(true);
        mpText.text = Mathf.RoundToInt(mpAmount).ToString();
        battleManager.players[playerIndex].currentMP = currentMP;

        if (currentMP > maxMP)
        {
            currentMP = maxMP;
        }

        uiBTL.UpdatePlayerMPControlPanel();
        PartyStats.chara[playerIndex].magicpoints = currentMP;
        uiBTL.EndTurn(); //End the turn of the current player (i.e. the healer) after the healing is done
    }

    public void RevivePlayer(float percentage) //Heal percentage after revival. Used for skills
    {
        EnableEffect("Revival", 0); //Enable the effect
        Heal(percentage);
        dead = false;
        playerAnimator.SetBool("Dead", false);
        playerAnimator.SetBool("Hit", false);
        currentState = playerState.Idle; //Reset the player
        chosenSkill = (int)SKILLS.NO_SKILL;
        currentAilment = playerAilments.none;
        DisableAllBuffs();
    }

    #region buffs and debuffs

    public void BuffStats(string statToBuff, float precentage, float lastsNumberOfTurns)
    {
        Debug.Log("Stat to buff: " + statToBuff);
        lastsNumberOfTurns++; //Add one more turn since the system should count the number of turns based on the caster not the receiver. This way ensures that the queue goes around equal to the number of turns it the buff/debuff is supposed to last
        switch (statToBuff)
        {
            case "Defense":
                if (precentage > 0)
                {
                    EnableEffect("DefBuff", 0);
                }
                else
                {
                    EnableEffect("DefDebuff", 0);
                }
                if (defenseBuffed && ((actualDEF < def && precentage > 0) || (actualDEF > def && precentage < 0))) //Check for debuffs first
                {
                    actualDEF = def;
                    defenseBuffSkillQCounter = 0; //Negate the debuff completely
                    defBuffArrowIndicator.gameObject.SetActive(false);
                }
                else if (defenseBuffed && ((actualDEF > def && precentage > 0) || (actualDEF < def && precentage < 0))) //If defense has already been buffed, update the Q counter
                {
                    defenseBuffSkillQCounter = lastsNumberOfTurns;                   
                }
                else if (!defenseBuffed) //No buffs or debuffs have occurred so far
                {
                    Debug.Log("Actual defense before for " + nameOfCharacter + " is: " + actualDEF);
                    defenseBuffed = true;
                    actualDEF = def + def * precentage;
                    defenseBuffSkillQCounter = lastsNumberOfTurns;

                    Debug.Log("Actual defense now for " + nameOfCharacter + " is: " + actualDEF);
                    Debug.Log("Counter: " + defenseBuffSkillQCounter);
                }
                break;
            case "Attack":
                if (precentage > 0)
                {
                    EnableEffect("AtkBuff", 0);
                }
                else
                {
                    EnableEffect("AtkDebuff", 0);
                }
                if (attackBuffed && ((actualATK < atk && precentage > 0) || (actualATK > atk && precentage < 0))) //Check if we'er being debuffed after being buffed or vice versa, if so, reset the attack
                {
                    actualATK = atk;
                    attackBuffSkillQCounter = 0; //Negate the debuff completely
                    atkBuffArrowIndicator.gameObject.SetActive(false);
                }
                else if (attackBuffed && ((actualATK > atk && precentage > 0) || (actualATK < atk && precentage < 0))) //Check if the buff or debuff is being extended
                {
                    attackBuffSkillQCounter = lastsNumberOfTurns;
                   
                }
                else if (!attackBuffed) //No buffs or debuffs have occurred so far
                {
                    
                    attackBuffed = true;
                    actualATK = atk + atk * precentage;
                    attackBuffSkillQCounter = lastsNumberOfTurns;
                }
                break;
            case "Agility":
                if (precentage > 0)
                {
                    EnableEffect("AgiBuff", 0);
                }
                else
                {
                    EnableEffect("AgiDebuff", 0);
                }
                if (agilityBuffed && ((actualAgi < agi && precentage > 0) || (actualAgi > agi && precentage < 0))) //Check for debuffs first
                {
                    actualAgi = agi;
                    agilityBuffSkillQCounter = 0; //Negate the debuff completely
                    agiBuffArrowIndicator.gameObject.SetActive(false);
                }
                else if (agilityBuffed && ((actualAgi > agi && precentage > 0) || (actualAgi < agi && precentage < 0))) //If agility has already been buffed, update the Q counter
                {
                    agilityBuffSkillQCounter = lastsNumberOfTurns;
                   
                }
                else if (!agilityBuffed) //No buffs or debuffs have occurred so far
                {
                   
                    agilityBuffed = true;
                    actualAgi = agi + agi * precentage;
                    agilityBuffSkillQCounter = lastsNumberOfTurns;
                }
                break;
            case "Strength":
                if (precentage > 0)
                {
                    EnableEffect("StrBuff", 0);
                }
                else
                {
                    EnableEffect("StrDebuff", 0);
                }
                if (strBuffed && ((actualSTR < str && precentage > 0) || (actualSTR > str && precentage < 0))) //Check for debuffs first
                {
                    actualSTR = str;
                    strBuffSkillQCounter = 0; //Negate the debuff completely
                    strBuffArrowIndicator.gameObject.SetActive(false);
                }
                else if (strBuffed && ((actualSTR > str && precentage > 0) || (actualSTR < str && precentage < 0))) //If str has already been buffed, update the Q counter
                {
                    strBuffSkillQCounter = lastsNumberOfTurns;
                    
                }
                else if (!strBuffed) //No buffs or debuffs have occurred so far
                {
                   
                    strBuffed = true;
                    actualSTR = str + str * precentage;
                    Debug.Log(nameOfCharacter + " Strength is: " + actualSTR);
                    strBuffSkillQCounter = lastsNumberOfTurns;
                }
                break;
            case "DrainEye":
                EnableEffect("DrainEye", 0);
                if (drainEye) //Check to see if Drain Eye is active, if yes, just extend it
                {
                    drainEyeSkillQCounter = lastsNumberOfTurns;
                }
                else if (!drainEye) //No buffs or debuffs have occurred so far
                {

                    drainEye = true;
                    drainEyeSkillQCounter = lastsNumberOfTurns;
                }
                break;
        }

       
    }

    private void CheckForBuffs()
    {
        if (defenseBuffed && defenseBuffSkillQCounter > 0)
        {
            defenseBuffSkillQCounter--;
            if (defenseBuffSkillQCounter <= 0)
            {
                defenseBuffSkillQCounter = 0;
                defenseBuffed = false;
                actualDEF = def;
                Debug.Log("Buff has ended");
                uiBTL.UpdateActivityText("DEF is back to normal");
                defBuffArrowIndicator.gameObject.SetActive(false);
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
                uiBTL.UpdateActivityText("ATK is back to normal");
                atkBuffArrowIndicator.gameObject.SetActive(false);
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
                uiBTL.UpdateActivityText("AGI is back to normal");
                agiBuffArrowIndicator.gameObject.SetActive(false);
            }
        }

        if (strBuffed && strBuffSkillQCounter > 0)
        {
            strBuffSkillQCounter--;
            if (strBuffSkillQCounter <= 0)
            {
                strBuffSkillQCounter = 0;
                strBuffed = false;
                actualSTR = str;
                uiBTL.UpdateActivityText("STR is back to normal");
                strBuffArrowIndicator.gameObject.SetActive(false);
            }
        }

        if(drainEye && drainEyeSkillQCounter > 0)
        {
            drainEyeSkillQCounter--;
            if (drainEyeSkillQCounter <= 0)
            {
                drainEyeSkillQCounter = 0;
                drainEye = false;
                uiBTL.UpdateActivityText("Drain Eye has shut");
                drainEyeBuffEffect.gameObject.SetActive(false);
            }
        }
    }

    public void EnableEffect(string effectName, int value) //Value is used by items as they add static amounts rather than percentages. Skills will pass value as zero.
    {
        switch (effectName)
        {
            case "Heal":
                healEffect.gameObject.SetActive(true);
                if (value > 0) //Check if the function call coming from an item.
                {
                    healText.gameObject.SetActive(true);
                    healText.text = value.ToString();

                    if(currentAilment == playerAilments.tied) //If the player is tied to an enemy, that enemy should be healed as well
                    {
                        tiedToThisEnemy.HealDueToTied(value);
                    }
                }
                break;
            case "MP":
                mpEffect.gameObject.SetActive(true);
                if (value > 0)
                {
                    mpText.gameObject.SetActive(true);
                    mpText.text = value.ToString();
                }
                break;
            case "DefBuff":
                defBuffEffect.gameObject.SetActive(true);
                defBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator= Quaternion.Euler(0.0f, 0.0f, 180.0f);
                defBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "DefDebuff":
                debuffColor.r = 0.4958386f;
                debuffColor.g = 0.1921569f;
                debuffColor.b = 0.8588235f;
                debuffArrow.gameObject.SetActive(true);
                debuffArrow.color = debuffColor;
                defBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                defBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "AtkBuff":
                atkBuffEffect.gameObject.SetActive(true);
                atkBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                atkBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "AtkDebuff":
                debuffColor.r = 0.8584906f;
                debuffColor.g = 0.1903257f;
                debuffColor.b = 0.1903257f;
                debuffArrow.gameObject.SetActive(true);
                debuffArrow.color = debuffColor;
                atkBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                atkBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "AgiBuff":
                agiBuffEffect.gameObject.SetActive(true);
                agiBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                agiBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "AgiDebuff":
                debuffColor.r = 0.03582038f;
                debuffColor.g = 0.3113208f;
                debuffColor.b = 0.01909043f;
                debuffArrow.gameObject.SetActive(true);
                debuffArrow.color = debuffColor;
                agiBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator= Quaternion.Euler(0.0f, 0.0f, 0.0f);
                agiBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "StrBuff":
                strBuffEffect.gameObject.SetActive(true);
                strBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                strBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "StrDebuff":
                debuffColor.r = 0.896f;
                debuffColor.g = 0.4148713f;
                debuffColor.b = 0.1940637f;
                debuffArrow.gameObject.SetActive(true);
                debuffArrow.color = debuffColor;
                strBuffArrowIndicator.gameObject.SetActive(true);
                arrowRotator = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                strBuffArrowIndicator.transform.rotation = arrowRotator;
                break;
            case "Revival":
                hopeEffect.gameObject.SetActive(true);
                if(value>0) //Used for Items
                {
                    healEffect.gameObject.SetActive(true);
                    healText.gameObject.SetActive(true);
                    healText.text = value.ToString();
                    dead = false;
                    playerAnimator.SetBool("Dead", false);
                    playerAnimator.SetBool("Hit", false);
                    currentState = playerState.Idle; //Reset the player
                    chosenSkill = (int)SKILLS.NO_SKILL;
                }
                break;
            case "DrainEye":
                drainEyeBuffEffect.gameObject.SetActive(true);
                break;
        }

        uiBTL.EndTurn(); //End the turn of the current player (i.e. the buffer) when the buffing is done
    }
    
    private void DisableAllBuffs() //Called when the player dies. Disables all buffs.
    {
        if(attackBuffed)
        {
            actualATK = atk;
            attackBuffed = false;
            attackBuffSkillQCounter = 0;
        }
        if (defenseBuffed)
        {
            actualDEF = def;
            defenseBuffed = false;
            defenseBuffSkillQCounter = 0;
        }
        if (agilityBuffed)
        {
            actualAgi = agi;
            agilityBuffed = false;
            agilityBuffSkillQCounter = 0;
        }
        if(strBuffed)
        {
            actualSTR = str;
            strBuffed = false;
            strBuffSkillQCounter = 0;
        }
        if(drainEye)
        {
            drainEye = false;
            drainEyeBuffEffect.gameObject.SetActive(false);
            drainEyeSkillQCounter = 0;
        }
    }

    #endregion

    #region ailments

    private void CheckForAilments()
    {
        switch(currentAilment)
        {
            case playerAilments.fear:
                if(fearTimer > 0)
                {
                    Debug.Log("Fear timer is larger than zero");
                    fearChance = Random.Range(0, 10);
                    Debug.Log("Fear chance is: " + fearChance);

                    if(fearChance>=0 && fearChance <=3)
                    {
                        affectedByFear = false;
                    }
                    else
                    {
                        Debug.Log(nameOfCharacter + " is scared and ended the turn");
                        affectedByFear = true;
                        fearTimer--; //Only decrease the timer if affected by fear ends up being true
                        uiBTL.UpdateActivityText(nameOfCharacter + " is too afraid...");
                        uiBTL.EndTurn(); //Skip the turn if affected by fear
                    }
                }
                else
                {
                    currentAilment = playerAilments.none;
                    fearSymbol.gameObject.SetActive(false);
                    uiBTL.UpdateActivityText(nameOfCharacter + " is no longer afraid");
                }
                break;
            case playerAilments.tied:
                break;
            default:
                break;
        }
    }

    public void Untie()
    {
        tiedSymbol.gameObject.SetActive(false);
        currentAilment = playerAilments.none;
    }

    #endregion
}