using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QImage : MonoBehaviour
{
    private UIBTL uiBTL;
    private BattleManager btlManager;
    public int imageIndex;
    public GameObject deathSkull;

    void Start()
    {
        uiBTL = UIBTL.instance;
        btlManager = BattleManager.instance;
       if(deathSkull!=null) deathSkull.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag.Equals("ImageRecycler"))
        {
            btlManager.NextOnQueue(); //When the Q stops moving, start the next turn
            uiBTL.ImageRecycle(imageIndex);           
        }
    }

    public void EnableSkull()
    {
        deathSkull.gameObject.SetActive(true);
    }

    public void DisableSkull()
    {
        deathSkull.gameObject.SetActive(false);
    }
}
