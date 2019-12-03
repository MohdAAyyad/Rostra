using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : ConversationTrigger
{
    public ITEM_ID[] itemToBeObtained;
    private bool collected = false;

    public override void TriggerConvo() //Override Trigger Convo so it works as it's supposed to for merchants
    {
        if (!collected)
        {
            for (int i = 0; i < itemToBeObtained.Length; i++)
            {
                MainInventory.invInstance.AddItem((int)itemToBeObtained[i], 1);
            }
            base.TriggerConvo();
            collected = true;
            AudioManager.instance.PlayThisEffect("collect");
            gameObject.SetActive(false);
        }
    }
}
