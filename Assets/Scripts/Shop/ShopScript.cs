using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;
using static ShopScript;

public class ShopScript : MonoBehaviour
{
    [Header("References")]
    public GameObject shopUI;
    public PlayerController playerController;
    public LevelDirector levelDirector;
    public ShopUIEvents shopUIEvents;
    public BombScript bombScript;

    [Header("Stats")]
    public int repairCost;
    public int bombCost;
    public int resupplyCost; // how much restocking the shop costs
    public int baseResupplyCost; 
    public float resupplyModifier; // how much the price increases per restock

    private TextAsset itemListJson;
    
    public int maxWeapons = 3;
    public int maxItems = 1;

    // setting up loot table .json file
    [System.Serializable]
    public class Item
    {
        public string name;
        public string displayName;
        public string description;
        public string itemType; // weapon, bomb, item?
        public int tier; // lower number = less rare
        public int cost;
    }

    [System.Serializable]
    public class ItemList
    {
        // dynamic, has to be a list.
        public List<Item> items;
    }

    [SerializeField]

    // weapon list
    public ItemList allItemsList = new ItemList();
    public Item[] sellingWeapons = new Item[3];
    public string[] sellingWeaponDisplayNames = new string[3];
    public string[] sellingWeaponDescriptions = new string[3];
    public int[] sellingWeaponDisplayCosts = new int[3];
    public int[] sellingWeaponDisplayTiers = new int[3];

    // item list
    public Item[] sellingItems = new Item[3];
    public string[] sellingItemDisplayNames = new string[3];
    public string[] sellingItemDescriptions = new string[3];
    public int[] sellingItemDisplayCosts = new int[3];
    public int[] sellingItemDisplayTiers = new int[3];

    // item tiers
    public ItemList[] weaponTierTables = new ItemList[3];
    public ItemList[] itemTierTables = new ItemList[3];

    // item buying
    public bool confirmed = false;
    public bool cancelled = false;
    public int changedSlot = -1; // default to -1 to not throw anything 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // setting the size based on whatever is specified
        sellingWeapons = new Item[maxWeapons];
        sellingWeaponDisplayNames = new string[maxWeapons];
        sellingWeaponDescriptions = new string[maxWeapons];
        sellingWeaponDisplayCosts = new int[maxWeapons];
        sellingWeaponDisplayTiers = new int[maxWeapons];

        sellingItems = new Item[maxItems];
        sellingItemDisplayNames = new string[maxItems];
        sellingItemDescriptions = new string[maxItems];
        sellingItemDisplayCosts = new int[maxItems];
        sellingItemDisplayTiers = new int[maxItems];


        shopUIEvents = shopUI.GetComponent<ShopUIEvents>();
        shopUIEvents.playerController = playerController;

        LoadItemJson();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void LoadItemJson()
    {
        // reading enemy info from the physical json instead of a compiled one
        string itemFilePath = Path.Combine(Application.streamingAssetsPath, "ItemData.json");
        if (File.Exists(itemFilePath))
        {
            string fileContent = File.ReadAllText(itemFilePath);
            itemListJson = new TextAsset(fileContent);

            allItemsList = JsonUtility.FromJson<ItemList>(itemListJson.text);
        }
        else
        {
            print("ERROR: ITEM DATA FILE NOT FOUND.");
        }


        // adding each item to its item loot table
        for (int i = 0; i < allItemsList.items.Count; i++)
        {
            // making sure it's 0-index
            int itemTier = allItemsList.items[i].tier - 1;

            // add it to the end
            if (allItemsList.items[i].itemType == "weapon")
            {
                weaponTierTables[itemTier].items.Add(allItemsList.items[i]);
            }
            //else if (allItemsList.items[i].itemType == "item")
            else
            {
                itemTierTables[itemTier].items.Add(allItemsList.items[i]);
            }
            
        }

        // manually printing every item at the end
        /*
        for (int i = 0; i < allItemsList.items.Count; i++) { 
            print(allItemsList.items[i].name);
        }
        */

    }


    // getting a random item in the tiered list
    private void GetRandomItemByTier(int slot, int tier, string type)
    {
        if (type == "weapon")
        {
            // making sure it's 0-index
            tier = tier - 1;

            // getting a random item in the list
            int randomIndex = Random.Range(0, weaponTierTables[tier].items.Count);

            // adding it to the list
            sellingWeapons[slot] = weaponTierTables[tier].items[randomIndex];
            sellingWeaponDisplayNames[slot] = weaponTierTables[tier].items[randomIndex].displayName;
            sellingWeaponDescriptions[slot] = weaponTierTables[tier].items[randomIndex].description;
            sellingWeaponDisplayCosts[slot] = weaponTierTables[tier].items[randomIndex].cost;
            sellingWeaponDisplayTiers[slot] = weaponTierTables[tier].items[randomIndex].tier;

            // note: later maybe consider when there's more content to exclude same-type weapons?
            // or keep them since you got 2
        }
        else
        {
            // making sure it's 0-index
            tier = tier - 1;

            // getting a random item in the list
            int randomIndex = Random.Range(0, itemTierTables[tier].items.Count);

            // adding it to the list
            sellingItems[slot] = itemTierTables[tier].items[randomIndex];
            sellingItemDisplayNames[slot] = itemTierTables[tier].items[randomIndex].displayName;
            sellingItemDescriptions[slot] = itemTierTables[tier].items[randomIndex].description;
            sellingItemDisplayCosts[slot] = itemTierTables[tier].items[randomIndex].cost;
            sellingItemDisplayTiers[slot] = itemTierTables[tier].items[randomIndex].tier;

        }



    }





    // generates the stock based on tiers
    private void GenerateStock(bool reroll)
    {
        // rng logic:
        // weapons are tiered levels 1-3
        // level 1 chance: 50%
        // level 2 chance: 30%
        // level 3 chance: 20%
        print("generating shop");

        // resetting the resupply cost only if you didn't reroll
        if (reroll == false)
            resupplyCost = baseResupplyCost;

        // generating a weapon for each slot
        for (int i = 0; i < maxWeapons; i++)
        {
            int itemTier = 0;
            int rng = Random.Range(0, 100);

            if (rng < 50)
            {
                itemTier = 1;
            }
            else if (rng < 80)
            {
                itemTier = 2;
            }
            else if (rng <= 100)
            {
                itemTier = 3;
            }


            // find a random item of that tier
            GetRandomItemByTier(i, itemTier, "weapon");

            //print(sellingWeapons[i].name);
        }


        // generating an item for each slot
        for (int i = 0; i < maxItems; i++)
        {
            int itemTier = 0;
            int rng = Random.Range(0, 100);

            // NOTE: THERE CURRENTLY ARE ONLY TIER 1 ITEMS, CHANGE THIS LATER WHEN YOU ADD MORE
            itemTier = 1;
            /*
            if (rng < 50)
            {
                itemTier = 1;
            }
            else if (rng < 80)
            {
                itemTier = 2;
            }
            else if (rng <= 100)
            {
                itemTier = 3;
            }
            */

            // find a random item of that tier
            GetRandomItemByTier(i, itemTier, "item");

            print(sellingItems[i].name);
        }

    }


    // called at a level end, creates the shop's stock and shows the shop hud.
    public void RefreshShop(bool reroll = false)
    {
        // generate the shop's stock using RNG to determine the rarity of each item
        GenerateStock(reroll);

        // fire it to the shopui so it can display the text for every slot
        shopUIEvents.UpdateDisplayWeapons(sellingWeaponDisplayNames, sellingWeaponDisplayCosts);
        shopUIEvents.UpdateDisplayItems(sellingItemDisplayNames, sellingItemDisplayCosts);

        // at the end, show the shop ui
        shopUIEvents.ShowDocument();

    }


    public void CloseShop()
    {
        // fire to the level director that the shop was closed
        levelDirector.ShopClosed();

        // close the shop
        shopUIEvents.HideDocument();
    }


    public void RepairShip()
    {
        int playerHP = playerController.playerHealth;
        int playerMaxHP = playerController.maxHealth;
        int playerHeldScrap = playerController.heldScrap;

        // they're allowed to heal
        if (playerHP < playerMaxHP)
        {
            // they have enough to buy a repair
            if (playerHeldScrap >= repairCost)
            {
                print("can repair");
                // increase their HP and reduce their scrap by the repair cost
                playerController.playerHealth += 1;
                playerController.heldScrap -= repairCost;

            } 
            else
            {
                // too poor
                print("poor");
            }


        }
        else
        {
            print("health already max");
        }

    }


    // restocking bombs specifically
    public void BuyBomb()
    {
        // check if they can afford a bomb, then give them another one
        int playerBombs = bombScript.heldBombs;
        int playerMaxBombs = bombScript.maxBombs;
        int playerHeldScrap = playerController.heldScrap;

        // they're allowed to heal
        if (playerBombs < playerMaxBombs)
        {
            // they have enough to buy a repair
            if (playerHeldScrap >= bombCost)
            {
                print("bought bomb");
                // increase their HP and reduce their scrap by the repair cost
                bombScript.heldBombs += 1;
                playerController.heldScrap -= bombCost;

            }
            else
            {
                // too poor
                print("poor");
            }


        }
        else
        {
            print("bombs max");
        }

    }


    // check if you have enough to bribe the shop
    // if you do, RefreshShop() and increase the cost of bribing by the multiplier
    // if you don't, do nothing
    public void BribeShop()
    {
        if (playerController.heldScrap >= resupplyCost)
        {
            print("can bribe shop");
            // reducing money + increasing resupply cost
            playerController.heldScrap -= resupplyCost;
            resupplyCost = Mathf.CeilToInt(resupplyCost * resupplyModifier);

            // restocking the shop
            RefreshShop(true);

        }
        else
        {
            print("poor");
        }
    }

    // buying items (bombs, later other items)
    // note: due to time constraint (and my desire to not do it) there will be no confirmation, you just click and buy it.
    public void BuyItem(int slot)
    {
        // making sure it's 0-indexed
        slot = slot - 1;

        // get the item stored in the slot
        Item slotItem = sellingItems[slot];
        int itemCost = slotItem.cost;

        // buy the item
        if (playerController.heldScrap >= itemCost)
        {
            // double checking that you can afford it
            playerController.heldScrap -= itemCost;

            //print("bought item " + slotItem.displayName + " / " + slotItem.name);

            // tell that one button to sell out
            shopUIEvents.BoughtObject("item", slot);

            if (slotItem.itemType == "bomb")
            {
                // update their bombscript
                bombScript.equippedBomb = slotItem.name;
            }

        }


    }


    // compares the current held scrap with the cost of the item
    // if the item can be afforded then do the appropriate thing with it.
    // otherwise do nothing.
    public void BuyWeapon(int slot)
    {
        // making sure it's 0-indexed
        slot = slot - 1;

        // get the item stored in the slot
        Item slotItem = sellingWeapons[slot];
        int itemCost = slotItem.cost;


        // coroutine checking for confirmation
        IEnumerator WaitForConfirmation()
        {

            // waiting until the player confirms
            yield return new WaitUntil(() => confirmed == true || cancelled == true);
            yield return new WaitUntil(() => changedSlot != -1 || cancelled == true);

            if (confirmed == true)
            {
                //print("buying item in slot " + (slot + 1) + ", which is a: " + sellingWeapons[slot].displayName);

                // buy the item
                if (playerController.heldScrap >= itemCost)
                {
                    // double checking that you can afford it
                    playerController.heldScrap -= itemCost;

                    // seeing what slot to replace it in
                    playerController.weaponHandler.equippedWeaponNames[changedSlot] = slotItem.name;

                    // reload the weapons
                    playerController.weaponHandler.LoadWeaponData();

                    // tell that one button to sell out
                    shopUIEvents.BoughtObject("weapon", slot);

                    //print("bought weapon " + slotItem.displayName);
                }

                // hide the confirmation window 
                shopUIEvents.HideConfirmationWindow();
            }
            else
            {
                // the buy was cancelled
                print("cancelled");
            }

            // resetting variables
            confirmed = false;
            cancelled = false;
            changedSlot = -1;

        }


        if (playerController.heldScrap >= itemCost) {
            // you can afford it

            // resetting variables (just in case)
            confirmed = false;
            cancelled = false;
            changedSlot = -1;

            // figure out what slot to add it to
            // show the confirmation window
            shopUIEvents.ShowConfirmationWindow(slotItem.displayName, slotItem.cost);

            // wait for a confirmation
            StartCoroutine(WaitForConfirmation());
        }

    }

}
