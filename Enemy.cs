using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Enemy : MonoBehaviour
{
    public int enemyIndexInBattleManager;
    private BattleManager battleManager;
    private UIBTL uiBTL;
    public float eMana;
    public float eAttack;
    public float eAgility;
    public float eDefence;
    public float eStrength;
    public float eSpeed;
    public float eBaseLevel;
    public int eCurrentLevel;
    public string eName;
    public float eRange;
    public float eCritical;
    public string enemyClassType;
    public string enemyType;// Determins which player enemy will attack
    public bool hit;
    float minPlayerHp;
    public Sprite qImage;
    Player attackThisPlayer;

    private SpriteRenderer spriteRenderer;
    private Color spriteColor;
    private Animator animator;

    public Image HP;
    public float maxHP;
    public float currentHP;
    public GameObject enemyCanvas;

    private bool haveAddedMyself;
    public bool dead;

    private GameObject demoEffect;
    private ObjectPooler objPooler;


    private void Start()
    {
        battleManager = BattleManager.instance;
        objPooler = ObjectPooler.instance;
        uiBTL = UIBTL.instance;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteColor = spriteRenderer.color;
        animator = gameObject.GetComponent<Animator>();

        haveAddedMyself = false;
        hit = false;
        dead = false;

    }

    private void Update()
    {
        //If the enemy is yet to add itself to the Q and the btl manager is currently adding enemies, then add this enemy to the Q
        if(!haveAddedMyself&&battleManager.addEnemies)
        {
            AddEnemyToBattle();
            haveAddedMyself = true;
        }
    }

    //Every enemy scales differently based on its warrior class (DPS,Tanks, Support)
    public void IncreaseStatsBasedOnLevel(int enemyCurrentLevel)
    {
        
        eCurrentLevel = enemyCurrentLevel;
        //eHP increase is still temporary until we agree how much each class'es HP increases with leveling up
            float skillPoints = enemyCurrentLevel - eBaseLevel;
            switch (enemyClassType)
            {
                case "DPS":
                    eAttack = Mathf.CeilToInt(eAttack += (skillPoints * 0.4f));
                    eAgility = Mathf.CeilToInt(eAgility += (skillPoints * 0.2f));
                    eSpeed = Mathf.CeilToInt(eSpeed += (skillPoints * 0.2f));
                    currentHP += skillPoints * 35.0f * 0.5f;                    

                break;

                case "Tank":
                    eAttack = Mathf.CeilToInt(eAttack += (skillPoints * 0.1f));
                    eDefence = Mathf.CeilToInt(eDefence += (skillPoints * 0.6f));
                    eSpeed = Mathf.CeilToInt(eSpeed += (skillPoints * 0.2f));
                    currentHP += skillPoints * 60.0f * 0.5f;
                break;

                case "Support":
                    eSpeed = Mathf.CeilToInt(eAttack += (skillPoints * 0.4f));
                    eAgility = Mathf.CeilToInt(eAttack += (skillPoints * 0.4f));
                    eDefence = Mathf.CeilToInt(eDefence += (skillPoints * 0.2f));
                    currentHP += skillPoints * 85.0f * 0.5f;
                break;
            }

        maxHP = currentHP;
    }

    public void AddEnemyToBattle()
    {
        battleManager.AddEnemy(enemyIndexInBattleManager, Mathf.RoundToInt(eAgility), Mathf.RoundToInt(eStrength), Mathf.RoundToInt(eCritical), Mathf.RoundToInt(eSpeed), this, name);
    }

    public void EnemyTurn()
    {
        if (!dead)
        {
            switch (enemyType)
            {
                case "":
                    DumbAttack();
                    break;

                case "Opportunistic":

                    break;

                case "Assassin":
                    AttackLowHp();
                    break;

                case "Bruiser":

                    break;

                case "Healer":

                    break;

                case "Heal-Support":

                    break;

                case "Straegist":

                    break;
                case "Demo":
                    DumbAttack();
                    break;

            }
        }
        else
        {
            uiBTL.EndTurn();
        }
    }

    //Calculate whether the attack is a hit or a miss
    private void CalculateHit()
    {
        //20 sided die + str <? enemy agility
        if (Random.Range(0.0f, 20.0f) + eStrength < attackThisPlayer.agi)
        {
            hit = false;
        }
        else
        {
            hit = true;
        }

        Debug.Log("Enemy Hit is " + hit);
    }

    private float CalculateCrit()
    {
        return Random.Range(0.0f, 100.0f);
    }

    void DumbAttack()
    {
        attackThisPlayer = battleManager.players[Random.Range(0, 4)].playerReference;
        //if the player is dead, try again
        if (attackThisPlayer.currentHP <= 0.0f)
        {
            DumbAttack();
        }
        else
        {
            //Run the animation
            CalculateHit();
            animator.SetBool("Attack", true);
        }
        
    }

    private void DemoAttackEffect()
    {
        demoEffect = objPooler.SpawnFromPool("DemoAttack", attackThisPlayer.gameObject.transform.position, gameObject.transform.rotation);
    }

    //Called from the animator once the attack anaimation ends
    private void CompleteAttack()
    {
        if (hit)
        {
            if(CalculateCrit() <= eCritical)
            {
                Debug.Log("Critical Hit from Enemy");
                attackThisPlayer.TakeDamage(eAttack * 1.2f);
            }
            else
            {
                attackThisPlayer.TakeDamage(eAttack);
            }           
        }
        else
        {
            Debug.Log("Enemy has missed");
        }
        animator.SetBool("Attack", false);
        uiBTL.EndTurn();
    }

   
    void AttackLowHp()
    {

    }


    //returns a int between 0 and 4 
    public int GetRandomNumber()
    {
        int randomNumber;
        return randomNumber = Random.Range(0, 4);
    }

    public float LowestStat()
    {
        float lowestHp;
        float[] health = new float[4];

        for (int i = 0; i < 4; i++)
        {
            health[i] = battleManager.players[i].playerReference.currentHP;
            Debug.Log(battleManager.players[i].playerReference.name + " Health is " + health[i]);
        }

        lowestHp = Mathf.Min(health);
        return lowestHp;
    }

    public void becomeLessVisbile() //Called from UIBTL when this enemy is NOT chosen for attack
    {
        spriteColor.a = 0.5f;
        spriteRenderer.color = spriteColor;
    }

    public void resetVisibility() //Called from UIBTL when this enemy is NOT chosen for attack, and either the player doesn't attack or the attack finishes
    {
        spriteColor.a = 1.0f;
        spriteRenderer.color = spriteColor;
    }

    //Calcualte the damage
    public void TakeDamage(float playerAttack)
    {
        float damage = playerAttack - ((eDefence / (20.0f + eDefence)) * playerAttack);
        currentHP -= damage;
        battleManager.enemies[enemyIndexInBattleManager].currentHP = currentHP; //Update the BTL manager with the new health
        HP.fillAmount = currentHP / maxHP;
        animator.SetBool("Hit", true);

        if(currentHP<=0.0f)
        {
            Death();
        }
    }

    public void EndHitAnimation()
    {
        animator.SetBool("Hit", false);
    }


    private void Death()
    {
        spriteRenderer.enabled = false;
        enemyCanvas.SetActive(false);
        dead = true;
        uiBTL.EnemyIsDead(enemyIndexInBattleManager);
    }


}
