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
    public string[] equippedSkills = new string [4];
    public int range; //Range of player standard attack
    public int initialPos; //Position of the player 0 being Frontline and -1 being Ranged
    public bool dead;

    //Queue
    public Sprite qImage;
    private int QCounter; //Used to count turns since the player went in rage or decided to go in waiting state.

    //Components
    private Animator playerAnimator;

    //Skills
    private int chosenSkill;
    private int skillTarget;
    private float mpCost;

    //Rage
    public float currentRage;
    public float maxRage;
    public GameObject rageModeIndicator;

    //Guard
    public GameObject guardIcon;

    //UI
    public Image hpImage;
    public Image rageImage;

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
        rageModeIndicator.gameObject.SetActive(false);
        canRage = false;

        //Guard
        guardIcon.gameObject.SetActive(false);

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
        uiBTL.EndTurn();

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

        playerAnimator.SetBool("Hit", true);
    }

    private void EndHit()
    {
        playerAnimator.SetBool("Hit", false);
    }

    //Heal function. Different heal skills will heal the player by different percentages
    public void Heal(float percentage)
    {
        float healAmount = percentage * maxHP;
        currentHP += healAmount;
        battleManager.players[playerIndex].currentHP = currentHP;

        if(currentHP>maxHP)
        {
            currentHP = maxHP;
        }
        //If the player could rage, now they could not since they healed
        if(canRage)
        {
            canRage = false;
            uiBTL.RageOptionTextColor();
        }

        currentRage -= healAmount * 1.5f; //Rage goes down by 20% more than the health gained

        if(currentRage < 0.0f)
        {
            currentRage = 0.0f;
        }

        //Update the UI
        hpImage.fillAmount = currentHP / maxHP;
        rageImage.fillAmount = currentRage / maxRage;
        uiBTL.UpdatePlayerHPControlPanel();
        PartyStats.chara[playerIndex].rage = currentRage; //Update the party stats
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
    public int SkillSearch(int skillID)
    {
        Debug.Log("Skill ID");
        chosenSkill = skillID;
        mpCost = skills.SkillStats(chosenSkill)[5]; //Get the MP cost

        //0: Target one enemy
        //1: Target all enemies
        //2: Target row of enemies
        //4: Target one player
        //5: Target all players


            switch (skillID)
            {
                case (int)SKILLS.TEST_SKILL1:
                    return skillTarget = 0;
                case (int)SKILLS.TEST_SKILL2:
                    return skillTarget = 1;
                case (int)SKILLS.TEST_SKILL3:
                    return skillTarget = 4;
                case (int)SKILLS.TEST_SKILL4:
                    return skillTarget = 4;
                default:
                    return skillTarget = 0;
            }
    }

    public void UseSkillOnPlayer(Player playerReference)
    {
        if(skillTarget == 5)
        {
            //Affect all players
        }
        else
        {
            //Affect the playerReference
        }
    }

    public void UseSkillOnEnemy(Enemy enemyReference)
    {
        Debug.Log("Skill Target " + skillTarget);
        if(skillTarget  == 1)
        {

        }
        else if(skillTarget  == 2)
        {

        }
        else
        {
            Debug.Log("HIT");
            playerAnimator.SetBool("ASkill", true);
           // playerAnimator.SetBool("Turn", false);
            attackingThisEnemy = enemyReference;
        }
    }

    public void SkillDamageEnemy()
    {
        if (skillTarget == 0) // Damaging one enemy
        {
            Debug.Log("Damage enemy");
            attackingThisEnemy.TakeDamage(0.5f * actualATK + skills.SkillStats(chosenSkill)[0]); //Damage is the half the player's attack stat and the skill's attack stat
            playerAnimator.SetBool("ASkill", false);
            currentMP -= mpCost;
            uiBTL.UpdatePlayerMPControlPanel();
            uiBTL.EndTurn(); 
        }
    }
}
