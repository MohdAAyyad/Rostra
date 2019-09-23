using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;
    public Enemy[] enemiesToSpawn = new Enemy[5];
    public int[] enemyLevels = new int[5];
    public GameObject[] enemyPos = new GameObject[5]; //Provided by the BTL Manager before the battle starts
    private Enemy enemySpawned;
    public int numberOfEnemies = 0;

    #region singleton
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    //Called from the World Map
    public void AddEnemyToSpawn(Enemy enemyToAdd, int index, int currentEnemyLevel)
    {

        enemiesToSpawn[index] = enemyToAdd;
        enemiesToSpawn[index].enemyIndexInBattleManager = index;
        enemyLevels[index] = currentEnemyLevel;
        numberOfEnemies++;
    }

    //Called from BTL Manager
    public void AddPos(GameObject pos, int index)
    {
        enemyPos[index] = pos;
    }

    //Called from BTL Manager
    public void SpawnEnemies()
    {

        for(int i=0;i<enemiesToSpawn.Length;i++)
        {
            if (enemiesToSpawn[i] != null)
            {
                enemySpawned = Instantiate(enemiesToSpawn[i], enemyPos[i].transform.position, gameObject.transform.rotation);
                enemySpawned.enemyIndexInBattleManager = i;
                enemySpawned.IncreaseStatsBasedOnLevel(enemyLevels[i]);
            }
        }
    }
}
