﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farea : Enemy
{
    private float attackChance = 0; //Used to determine whether a skil or a normal attack will be used.
    private bool isThereADeadPlayer = false; //One of the Farea's skills relies on one of the players being dead
    private int bossPhase = 1; //Used to know which skills are available for the boss to use at which phase
    private float totalDamageSustained = 0.0f; //Used for Mother's Pain Skill. Becomes zero after the skill is used
    private float totalDamageThreshold = 400.0f; //When the Farea has sustained 300 or more damage and is in phase 2, it will do Mother's Pain Next
    private float skillChanceModifier = 1.0f; //Used in phase 2 to prioritize certain skills depending on the situation
    private Player thisPlayerIsDead; //Used as a reference for You Are Not Mine
    private Vector2 mothersPainInitialLoc = new Vector2(24.24f, -2.35f);

    private bool youAreNotMineUsedOnceThisFight = false;



    private enum fareaSkills //Only the skills that will require waiting for a number of turns before executing
    {
        none,
        wails,
        lullabyOfDepsair,
        mothersPain,
        youAreNotMine

    }
    private fareaSkills chosenSkill = fareaSkills.none;

    //J&W --> (The official competitor of A&W)
    public GameObject jObj;
    public GameObject wObj;

    //Wail
    public GameObject wailWait;
    private string[] statToDebuff; //Used to store all possible stats that WoF can debuff
    private string chosenStat; //Used to store the choice of the stat to debuff

    //Lullaby
    public GameObject lullWait;

    //MothersPain
    public GameObject mothersPainWait;

    //You Are Not Mine
    public GameObject youAreNotMineObject;


    protected override void Start()
    {
        battleManager = BattleManager.instance;
        objPooler = ObjectPooler.instance;
        uiBTL = UIBTL.instance;
        audioManager = AudioManager.instance;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteColor = spriteRenderer.color;
        animator = gameObject.GetComponent<Animator>();

        damageText.gameObject.SetActive(false);
        healText.gameObject.SetActive(false);

        haveAddedMyself = false;
        hit = false;
        dead = false;

        currentState = EnemyState.idle;
        waitTurnsText.gameObject.SetActive(false);
        jObj.gameObject.SetActive(false);
        wObj.gameObject.SetActive(false);
        wailWait.gameObject.SetActive(false);
        lullWait.gameObject.SetActive(false);
        mothersPainWait.gameObject.SetActive(false);
        deadlyTiesObject.gameObject.SetActive(false);
        chain.gameObject.SetActive(false);
        youAreNotMineObject.gameObject.SetActive(false);

        statToDebuff = new string[2];
        statToDebuff[0] = "Defense";
        statToDebuff[1] = "Attack";


        if (chainedSymbol)
        {
            chainedSymbol.gameObject.SetActive(false);
        }
        if (primaryChainedSymbol)
        {
            primaryChainedSymbol.gameObject.SetActive(false);
        }
        if (ralliedSymbol)
        {
            ralliedSymbol.gameObject.SetActive(false);
        }
        if (burnSymbol)
        {
            burnSymbol.gameObject.SetActive(false);
        }
        if (debuffArrow)
        {
            debuffArrow.gameObject.SetActive(false);
        }
        if (atkBuffArrowIndicator)
        {
            atkBuffArrowIndicator.gameObject.SetActive(false);
        }
        if (strBuffArrowIndicator)
        {
            strBuffArrowIndicator.gameObject.SetActive(false);
        }
        if (defBuffArrowIndicator)
        {
            defBuffArrowIndicator.gameObject.SetActive(false);
        }
        if (agiBuffArrowIndicator)
        {
            agiBuffArrowIndicator.gameObject.SetActive(false);
        }
        if (healthObject)
        {
            healthObject.gameObject.SetActive(false);
        }
        if (waitTurnsText)
        {
            waitTurnsText.gameObject.SetActive(false);
        }
        if (atkBuffEffect)
        {
            atkBuffEffect.gameObject.SetActive(false);
        }
        if (defBuffEffect)
        {
            defBuffEffect.gameObject.SetActive(false);
        }
        if (agiBuffEffect)
        {
            agiBuffEffect.gameObject.SetActive(false);
        }
        if (strBuffEffect)
        {
            strBuffEffect.gameObject.SetActive(false);
        }
    }


    protected override void Update()
    {
        base.Update();
    }

    public override void TakeDamage(float playerAttack, int numberOfAttacks)
    {
        if (playerAttack > 0.0f)
        {
            if (tieThisPlayer != null)
            {
                tieThisPlayer.TakeDamage(playerAttack); //Damage the tied player should you get damaged
                tiedTimer--;
                if (tiedTimer <= 0)
                {
                    Untie();
                }
            }
            float damage = playerAttack - ((eDefence / (20.0f + eDefence)) * playerAttack);
            currentHP -= damage;
            totalDamageSustained += damage;
            damageText.gameObject.SetActive(true);
            damageText.text = Mathf.RoundToInt(damage).ToString();
            battleManager.enemies[enemyIndexInBattleManager].currentHP = currentHP; //Update the BTL manager with the new health
            HP.fillAmount = currentHP / maxHP;

            if (currentHP <= 1.0f)
            {
                if (bossPhase == 1)
                {
                    //Reset and get ready for phase 2
                    animator.SetBool("Phase2", true);
                    currentState = EnemyState.idle;
                    waitTime = 0;
                    waitTurnsText.text = "0";
                    chosenSkill = fareaSkills.none;
                    lullWait.gameObject.SetActive(false);
                    wailWait.gameObject.SetActive(false);

                }
                animator.SetBool("Death", true);
            }
            else
            {
                if (numberOfAttacks <= 0)
                {
                    uiBTL.EndTurn(); //Only end the turn after the damage has been taken
                }
            }

            if (currentState != EnemyState.waiting)
            {
                animator.SetBool("Hit", true);
            }

            if (currentHP < 0.5f * maxHP)
            {
                //If the current health is less than half the max health, then make it more probable to use You Are Not Mine
                //If you have not used it yet
                if (!youAreNotMineUsedOnceThisFight)
                {
                    //Debug.Log("Farea current HP: " + currentHP);
                    //Debug.Log("Farea max HP: " + maxHP);

                    skillChanceModifier = 2.0f;
                }
            }
        }
        else
        {
            if (missText != null)
            {
                missText.gameObject.SetActive(true);
            }
        }

    }

    public override void TakeDamage(float playerAttack, int numberOfAttacks, int debuffIndex, float debuffValuePercent, int debuffTimer, string debuffSubIndex, EnemyStatusAilment ailment)
    {
        if (playerAttack > 0.0f) //Don't need to calcualte damage if the incoming attack is debuff only
        {
            //Didn't recall the original function cause the "Hit" animation ends the turn
            //Debug.Log("Received player attack: " + playerAttack);
            float damage = playerAttack - ((actualDEF / (20.0f + actualDEF)) * playerAttack);
            damage *= ralliedDamageModifier; //Increase damage if rallied;
            currentHP -= damage;
            damageText.gameObject.SetActive(true);
            damageText.text = Mathf.RoundToInt(damage).ToString();
            battleManager.enemies[enemyIndexInBattleManager].currentHP = currentHP; //Update the BTL manager with the new health
            HP.fillAmount = currentHP / maxHP;
        }
        if (playerAttack >= 0.0f) //if incoming damage is negative, then it was a miss
        {
            switch (debuffIndex)
            {
                case 0: // Ailment

                    switch (ailment)
                    {
                        case EnemyStatusAilment.chained:
                            break;
                        case EnemyStatusAilment.rallied:
                            if (currentStatusAilment0 != EnemyStatusAilment.rallied && currentStatusAilment1 != EnemyStatusAilment.rallied) //Cannot rally an enemy that's already been rallied against
                            {
                                ralliedWaitTime = debuffTimer;
                                ralliedDamageModifier = 2.0f;
                                ralliedSymbol.gameObject.SetActive(true);
                                uiBTL.EndTurn(); //Rally doesn't do any damage
                            }
                            break;
                        case EnemyStatusAilment.burn:
                            if (currentStatusAilment0 != EnemyStatusAilment.burn && currentStatusAilment1 != EnemyStatusAilment.burn) //Cannot burn an enemy that's already been burned
                            {
                                burnedWaitTime = debuffTimer;
                                burnSymbol.gameObject.SetActive(true);
                                uiBTL.EndTurn(); //Rally doesn't do any damage
                            }
                            break;
                    }
                    //Check which of the two ailments is set to none.
                    // If both of them are filled, overwrite the 0 spot
                    if (currentStatusAilment0 == EnemyStatusAilment.none)
                    {
                        currentStatusAilment0 = ailment;
                    }
                    else if (currentStatusAilment1 == EnemyStatusAilment.none)
                    {
                        currentStatusAilment1 = ailment;
                    }
                    else if (currentStatusAilment0 == EnemyStatusAilment.rallied)
                    {
                        NoLongerRallied();
                        currentStatusAilment0 = ailment;
                    }
                    break;
                case 1: //Debuff
                    BuffStats(debuffSubIndex, -debuffValuePercent, debuffTimer);
                    break;
            }

            animator.SetBool("Hit", true);

            if (currentHP < 1.0f) //Avoid near zero
            {
                if (bossPhase == 1)
                {
                    //Reset and get ready for phase 2
                    animator.SetBool("Phase2", true);
                    currentState = EnemyState.idle;
                    waitTime = 0;
                    waitTurnsText.text = "0";
                    chosenSkill = fareaSkills.none;
                    lullWait.gameObject.SetActive(false);
                    wailWait.gameObject.SetActive(false);

                }
                animator.SetBool("Death", true);
            }
            else
            {
                if (numberOfAttacks <= 0)
                {
                   uiBTL.EndTurn();
                }
            }
        }
        else
        {
            if (missText != null)
            {
                missText.gameObject.SetActive(true);
            }
        }
    }

    private void StartPhase2()
    {
        //Debug.Log("Start Phase 2");
        currentState = EnemyState.idle;
        uiBTL.EndTurn(); //End the player's turn
        bossPhase = 2;
        maxHP *= 1.5f;
        currentHP = maxHP;
        eAttack *= 1.2f;
        eDefence *= 1.2f;
        eAgility *= 1.2f;
        eStrength *= 1.2f;

        HP.fillAmount = 1.0f;
        animator.SetBool("Hit", false);
        animator.SetBool("Death", false);
        animator.SetInteger("WaitingIndex", 0);
        waitTurnsText.gameObject.SetActive(false);
    }

    public override void EnemyTurn()
    {
        uiBTL.backdropHighlighter.gameObject.SetActive(true);
        uiBTL.DisableActivtyText();
        CheckForAilments();
        CheckForBuffs();
        //Check if we're waiting on a skill first
        if (currentState == EnemyState.waiting)
        {
            waitQTurns--;
            waitTurnsText.text = waitQTurns.ToString(); //Update the UI
            if (waitQTurns <= 0)
            {
                waitTurnsText.gameObject.SetActive(false); //Turn off the text. Don't forget to enable it when the enemy goes to waiting state
                currentState = EnemyState.idle; //Change the state
                animator.SetInteger("WaitingIndex", 1); //After waiting, the index is always going to be 1
            }
            else
            {
                //End the turn
                uiBTL.EndTurn();
            }

        }
        else
        {
            //Only update the attackChance when no skill is on the waiting list
            attackChance = Random.Range(0.0f, 100.0f);
            //attackChance = 50; //Testing

            if (bossPhase == 1)
            {
                if (attackChance >= 0.0f && attackChance < 10.0f) //Normal attack
                {
                    DumbAttack();
                }
                else if (attackChance >= 10.0f && attackChance < 40.0f) //Wails of Frailty
                {
                    GoToWaitState(fareaSkills.wails, 1, 2);
                    //Summon skill effect
                    wailWait.gameObject.SetActive(true);
                    uiBTL.UpdateActivityText("Wails of Frailty");
                }
                else if (attackChance >= 40.0f && attackChance < 70.0f) //Judgment and Wrath
                {
                    attackThisPlayer = battleManager.players[Random.Range(0, 3)].playerReference;
                    animator.SetBool("JudgementAndWrath", true);
                    uiBTL.UpdateActivityText("Judgement & Wrath");
                }
                else if (attackChance >= 70.0f && attackChance <= 100.0f) // Lullaby Of Despair
                {
                    GoToWaitState(fareaSkills.lullabyOfDepsair, 1, 2);
                    //Summon skill effect
                    lullWait.gameObject.SetActive(true);
                    uiBTL.UpdateActivityText("Lullaby Of Despair");
                }

            }
            else if (bossPhase == 2)
            {
                //Debug.Log("Boss is in fact in phase 2");
                if (totalDamageSustained >= totalDamageThreshold)
                {
                    //If the total damage sustained is over the threshold, Mother's Pain must be done in 2 turns rathe than 3
                    GoToWaitState(fareaSkills.mothersPain, 1, 3);
                    //Summon skill effect
                    mothersPainWait.gameObject.SetActive(true);
                    uiBTL.UpdateActivityText("Mother's Pain");
                    totalDamageThreshold -= 50; //If the Farya used MP that means she's suffered a lot of damage, make it easier to use MP 
                }
                else
                {
                    //Check if one of the players is dead --> if yes, there's a chance to use You Are Not Mine
                    for (int i = 0; i < uiBTL.playersDead.Length; i++)
                    {
                        if (uiBTL.playersDead[i] == true)
                        {
                            isThereADeadPlayer = true;
                            thisPlayerIsDead = battleManager.players[i].playerReference;
                            break;
                        }
                        else
                        {
                            isThereADeadPlayer = false; // In case the Farea does not choose to use You Are Not Mine and the player gets revived before her next turn
                            thisPlayerIsDead = null;
                        }
                    }
                    attackChance = Random.Range(0.0f, 100.0f * skillChanceModifier);
                    //Debug.Log("Attack chance is: " + attackChance);
                    //attackChance = 50; //Testing
                    if (attackChance >= 0.0f && attackChance < 30.0f * skillChanceModifier && isThereADeadPlayer)
                    {
                        //You are not mine
                        GoToWaitState(fareaSkills.youAreNotMine, 1, 2);
                        //Summon skill effect
                        youAreNotMineObject.gameObject.SetActive(true);
                        uiBTL.UpdateActivityText("You Are Not Mine");
                    }
                    else if (attackChance >= 30.0f && attackChance < 60.0f * skillChanceModifier)
                    {
                        if (tieThisPlayer == null)
                        {
                            //Deadly Ties
                            deadlyTiesObject.gameObject.SetActive(true);
                            chain.gameObject.SetActive(true);
                            TieAPlayer();
                            tiedTimer = 4;
                            animator.SetBool("DeadlyTies", true);
                            uiBTL.UpdateActivityText("DeadlyTies");
                        }
                        else
                        {
                            EnemyTurn(); //If you're already tied to a player, try again
                        }
                    }
                    else if (attackChance >= 60.0f && attackChance < 70.0f) //Wails of Frailty
                    {
                        GoToWaitState(fareaSkills.wails, 1, 2);
                        //Summon skill effect
                        wailWait.gameObject.SetActive(true);
                        uiBTL.UpdateActivityText("Wails of Frailty");
                    }
                    else if (attackChance >= 70.0f && attackChance < 80.0f) //Judgment and Wrath
                    {
                        attackThisPlayer = battleManager.players[Random.Range(0, 3)].playerReference;
                        animator.SetBool("JudgementAndWrath", true);
                        uiBTL.UpdateActivityText("Judgement & Wrath");
                    }
                    else if (attackChance >=80.0f && attackChance <= 90.0f) // Lullaby Of Despair
                    {
                        GoToWaitState(fareaSkills.lullabyOfDepsair, 1, 2);
                        //Summon skill effect
                        lullWait.gameObject.SetActive(true);
                        uiBTL.UpdateActivityText("Lullaby Of Despair");
                    }
                    else  //Normal attack
                    {
                        DumbAttack();
                    }
                }
            }
        }
    }


    //-----------------------------Skills------------------------------//
    private void GoToWaitState(fareaSkills skill, int turnsToWait, int waitingIndex)
    {
        chosenSkill = skill;
        waitQTurns = turnsToWait;
        waitTurnsText.gameObject.SetActive(true);
        waitTurnsText.text = waitQTurns.ToString();
        animator.SetInteger("WaitingIndex", waitingIndex);
        currentState = EnemyState.waiting;
        audioManager.PlayThisEffect("faryaWait");
        uiBTL.EndTurn();
    }


    //Judgement & Wrath
    private void Judgement()
    {
        //Disable J and apply the damage.
        //Should change this to use animations
        audioManager.PlayThisEffect("Judge");
        objPooler.SpawnFromPool("JudgeAttack", attackThisPlayer.gameObject.transform.position, gameObject.transform.rotation);
        jObj.gameObject.SetActive(false);
        attackThisPlayer.TakeDamage(eAttack);
    }

    private void Wrath()
    {
        //Disable W and apply the damage.
        //Should change this to use animations
        audioManager.PlayThisEffect("Wrath");
        objPooler.SpawnFromPool("WrathAttack", attackThisPlayer.gameObject.transform.position, gameObject.transform.rotation);
        wObj.gameObject.SetActive(false);
        attackThisPlayer.TakeDamage(eAttack * 1.2f); //Wrath does a little more damage
        animator.SetBool("JudgementAndWrath", false);
        uiBTL.DisableActivtyText();
        uiBTL.EndTurn();
    }

    private void JudgementAndWrathEffect()
    {
        jObj.gameObject.SetActive(true);
        wObj.gameObject.SetActive(true);
    }

    private void DeadlyTies()
    {
        //Affect the player and tie them
        audioManager.PlayThisEffect("Ties");
        tieThisPlayer.TakeDamage(0.0f, 0, "", Player.playerAilments.tied, this, 0.0f, tiedTimer, false);
        animator.SetBool("DeadlyTies", false);
        deadlyTiesObject.gameObject.SetActive(false);
        uiBTL.EndTurn();
    }

    //I know the below two functions are basically the same, I can't use recurisve calls with a return function cause it needs you to return something 
    //Deadly ties chooser
    private void TieAPlayer()
    {
        tieThisPlayer = battleManager.players[Random.Range(0, 3)].playerReference;
        if (tieThisPlayer.dead)
        {
            TieAPlayer(); //Make sure whatever player you choose is alive
        }
    }
    //Lullaby of Despair chooser
    private void ScareAPlayer()
    {
        attackThisPlayer = battleManager.players[Random.Range(0, 3)].playerReference;
        if (attackThisPlayer.dead)
        {
            ScareAPlayer(); //Make sure whatever player you choose is alive
        }
    }

    //Called from the animator to check which skill to execute after waiting

    private void SkillEffect()
    {
        switch (chosenSkill)
        {
            case fareaSkills.wails:
                audioManager.PlayThisEffect("Wails");
                //Choose at random whether you want to damage attack or defense
                chosenStat = statToDebuff[Random.Range(0, statToDebuff.Length)];
                //Update the UI
                uiBTL.UpdateActivityText("Wails of Frailty");
                //Damage all players
                for (int i = 0; i < battleManager.players.Length; i++)
                {
                    if (!battleManager.players[i].playerReference.dead)
                    {
                        battleManager.players[i].playerReference.TakeDamage(eAttack, 1, chosenStat, Player.playerAilments.none, null, 0.3f, 3, true);
                        //Summon debuff object
                        objPooler.SpawnFromPool("WailAttack", battleManager.players[i].playerReference.transform.position, gameObject.transform.rotation);
                    }
                }
                wailWait.gameObject.SetActive(false);

                break;
            case fareaSkills.lullabyOfDepsair:
                //Update the UI
                uiBTL.UpdateActivityText("Lullaby Of Despair");
                //Choose a player at random
                ScareAPlayer();

                //If the player has no ailments, then affect thme with fear
                if (attackThisPlayer.currentAilment == Player.playerAilments.none) //Make sure you target 
                {
                    audioManager.PlayThisEffect("Despair");
                    attackThisPlayer.TakeDamage(0.0f, 0, "", Player.playerAilments.fear, null, 0, 3, false);
                }
                else
                {
                    //Otherwise just damage the player
                    attackThisPlayer.TakeDamage(eAttack * 1.5f);
                }
                lullWait.gameObject.SetActive(false);

                break;
            case fareaSkills.mothersPain:
                audioManager.PlayThisEffect("MothersPain");
                for (int i = 0; i < battleManager.players.Length; i++)
                {
                    if (!battleManager.players[i].playerReference.dead)
                    {
                        battleManager.players[i].playerReference.TakeDamage(0.5f * totalDamageSustained); //Damage the players with half the totalDamageSustained

                        objPooler.SpawnFromPool("MothersPain", mothersPainInitialLoc, gameObject.transform.rotation);
                    }
                }
                totalDamageSustained = 0.0f; //Reset the variable
                mothersPainWait.gameObject.SetActive(false);
                break;
            case fareaSkills.youAreNotMine:
                if (thisPlayerIsDead.dead)
                {
                    audioManager.PlayThisEffect("YouAreNotMine");
                    youAreNotMineObject.gameObject.SetActive(false);
                    objPooler.SpawnFromPool("YouAreNotMine", thisPlayerIsDead.transform.position, gameObject.transform.rotation);
                    Heal(thisPlayerIsDead.maxHP * 0.7f);
                    youAreNotMineUsedOnceThisFight = true; //The skill check modifier will not change the next time the Farea drops below max health
                    skillChanceModifier = 1.0f;
                    thisPlayerIsDead = null;
                }
                else
                {
                    youAreNotMineObject.gameObject.SetActive(false);
                    thisPlayerIsDead.TakeDamage(eAttack * 1.5f);
                }
                break;
        }

        //Reset the skill and the animator
        chosenSkill = fareaSkills.none;
        animator.SetInteger("WaitingIndex", 0);
        uiBTL.EndTurn();

    }
}