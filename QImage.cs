using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QImage : MonoBehaviour
{
    private UIBTL uiBTL;
    private BattleManager btlManager;
    public int imageIndex;

    void Start()
    {
        uiBTL = UIBTL.instance;
        btlManager = BattleManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag.Equals("ImageRecycler"))
        {
            uiBTL.ImageRecycle(imageIndex);
            btlManager.NextOnQueue(); //When the Q stops moving, start the next turn
        }
    }
}
