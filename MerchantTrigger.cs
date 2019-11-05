using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantTrigger : ConversationTrigger
{
    private bool interactedWith = false;

    public enum merchantState
    {
        active,
        idle
    }

    public merchantState currentState = merchantState.idle; //State is switched from the active Shop UI
    public bool canTalkAgain = true; //Make sure that if the player chooses "Exit" in the Shop UI, the Confirm button is not read to start the conversation again until the UI is closed

    private void Update()
    {
        switch(currentState)
        {
            case merchantState.idle:
                if (!DialogueManager.instance.isActive && interactedWith)
                {
                    
                    ItemShopUI.OpenItemShop();
                    currentState = merchantState.active;
                    interactedWith = false;
                }
                break;
            case merchantState.active:
                break;
        }

    }

    public override void TriggerConvo() //Override Trigger Convo so it works as it's supposed to for merchants
    {
        if(!canTalkAgain)
        {
            canTalkAgain = true;
        }
        else if (currentState == merchantState.idle)
        {
            interactedWith = true;
            ItemShopUI.Singleton.activeMerchant = this;
            base.TriggerConvo();
        }
    }

}
