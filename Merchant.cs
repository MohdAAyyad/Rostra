using UnityEngine;

// Code Written By:     Christopher Brine
// Last Updated:        October 10th, 2019

enum MERCHANT { // Minimum Value -- 0x0000, Maximum Value -- 0xFFFF (65535 possible merchant types)
    GENERAL =       0x0000,     // This merchant sells a random assortment of items, but not any specialty or high-quality ones (Ex. basic potions, basic weapons)
    WEAPONS =       0x0001,     // This merchant specializes in weaponry -- both low-end and high-end variants
    ARMOR =         0x0002,     // This merchant specializes in armor and protection -- both low-end and high-end variants
    POTIONS =       0x0003,     // This merchant sells potions that can heal, restore magic, as well as boost stats during battle and other such effects
    STAT_BOOSTS =   0x0004,     // This merchant sells items that can permanently boost a player character's stats
};

public class Merchant : MonoBehaviour {
    public int merchantType = (int)MERCHANT.GENERAL;    // The item types that this merchant specializes in (Armor, weapons, general items, potions, etc.)
    public int[] itemsToSell;                           // The list of items that the merchant has to sell to the player

    [HideInInspector]
    public int[] itemsToPurchase;                        // The list of items that the merchant is able to purchase from the player

    private bool canShop = false;                       // When the player enters the tigger area, this will become true and let the player enter the shop
    private ItemShop shop = null;                       // Holds the instance of the ItemShop that was created upon interaction with the merchant

    public void Awake() {
        switch (merchantType) {
            case (int)MERCHANT.GENERAL: // A generic merchant will purchase just about every type of item (Except for any stat boosting items)
                itemsToPurchase = new int[] {
                    (int)ITEM_CLASS.WEAPON,
                    (int)ITEM_CLASS.ARMOR,
                    (int)ITEM_CLASS.POTIONS,
                    (int)ITEM_CLASS.GENERIC,
                };
                break;
            case (int)MERCHANT.WEAPONS: // A weapons merchant will purchase any manner of weaponry from the player at a slightly higher price than normal
                itemsToPurchase = new int[] {
                    (int)ITEM_CLASS.WEAPON,
                    (int)ITEM_CLASS.GENERIC,
                };
                break;
            case (int)MERCHANT.ARMOR: // An armor merchant will purchase any manner of armor from the player at a slightly higher price than normal
                itemsToPurchase = new int[] {
                    (int)ITEM_CLASS.ARMOR,
                    (int)ITEM_CLASS.GENERIC,
                };
                break;
            case (int)MERCHANT.POTIONS:
            case (int)MERCHANT.STAT_BOOSTS:
                itemsToPurchase = new int[] {
                    (int)ITEM_CLASS.POTIONS,
                    (int)ITEM_CLASS.STAT_BOOSTS,
                    (int)ITEM_CLASS.GENERIC,
                };
                break;
        }
    }

    public void Update() {
        if (shop == null) {
            bool keyInteract = Input.GetKeyDown(KeyCode.Space);

            if (keyInteract) {
                shop = new ItemShop(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            canShop = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            canShop = false;
        }
    }
}