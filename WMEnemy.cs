using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WMEnemy : MonoBehaviour
{
    public Enemy[] enemies;
    public int[] enemyLevels;

    public EnemySpawner enemySpwn;

    private void Start()
    {
        enemySpwn = EnemySpawner.instance;
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag.Equals("Player"))
        {
            TransitionIntoBattle();
        }
    }

    public void TransitionIntoBattle()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemySpwn.AddEnemyToSpawn(enemies[i], i, enemyLevels[i]);
        }
        SceneManager.LoadScene("Queue Scene", LoadSceneMode.Additive);
        gameObject.SetActive(false);
    }
}
