using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;

public class ShopScript : MonoBehaviour
{
    [Header("References")]
    public GameObject shopUI;
    public PlayerController playerController;
    public LevelDirector levelDirector;
    public ShopUIEvents shopUIEvents;

    [Header("Stats")]
    public int repairCost;
    private TextAsset itemListJson;
    
    public int maxItems = 3;

    // setting up loot table .json file
    [System.Serializable]
    public class Item
    {
        public string name;
        public string displayName;
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

    // item list
    public ItemList allItemsList = new ItemList();
    public Item[] sellingItems = new Item[3];
    public string[] sellingItemDisplayNames = new string[3];

    // item tiers
    public ItemList[] tierTables = new ItemList[3];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sellingItems = new Item[maxItems];

        shopUIEvents = shopUI.GetComponent<ShopUIEvents>();
        shopUIEvents.playerController = playerController;

        LoadWeaponJson();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void LoadWeaponJson()
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
            tierTables[itemTier].items.Add(allItemsList.items[i]);
        }

        // manually printing every item at the end
        /*
        for (int i = 0; i < allItemsList.items.Count; i++) { 
            print(allItemsList.items[i].name);
        }
        */

    }


    // getting a random item in the tiered list
    private void GetRandomItemByTier(int slot, int tier)
    {
        // making sure it's 0-index
        tier = tier - 1;

        // getting a random item in the list
        int randomIndex = Random.Range(0, tierTables[tier].items.Count);

        // adding it to the list
        sellingItems[slot] = tierTables[tier].items[randomIndex];
        sellingItemDisplayNames[slot] = tierTables[tier].items[randomIndex].displayName;


        // note: later maybe consider when there's more content to exclude same-type weapons?
        // or keep them since you got 2

    }

    // generates the stock based on tiers
    private void GenerateStock()
    {
        // rng logic:
        // weapons are tiered levels 1-3
        // level 1 chance: 70%
        // level 2 chance: 20%
        // level 3 chance: 10%
        print("generating shop");

        // generating an item for each slot
        for (int i = 0; i < maxItems; i++)
        {
            int itemTier = 0;
            int rng = Random.Range(0, 100);

            if (rng < 70)
            {
                itemTier = 1;
            }
            else if (rng < 90)
            {
                itemTier = 2;
            }
            else if (rng <= 100)
            {
                itemTier = 3;
            }


            // find a random item of that tier
            GetRandomItemByTier(i, itemTier);


        }

    }


    // called at a level end, creates the shop's stock and shows the shop hud.
    public void RefreshShop()
    {
        // generate the shop's stock using RNG to determine the rarity of each item
        GenerateStock();

        // fire it to the shopui so it can display the text for every slot
        shopUIEvents.UpdateDisplayItems(sellingItemDisplayNames);

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



    // the player clicked one of the confirmation buttons, buy and equip the item into that slot
    public void ConfirmPurchase(int weaponSlot)
    {

    }



    // compares the current held scrap with the cost of the item
    // if the item can be afforded then do the appropriate thing with it.
    // otherwise do nothing.
    public void BuyItem(int slot)
    {
        // making sure it's 0-indexed
        slot = slot - 1;
        print("buying item in slot " + (slot + 1) + ", which is a: " + sellingItems[slot].displayName);

        // get the item stored in the slot
        Item slotItem = sellingItems[slot];
        int itemCost = slotItem.cost;

        if (playerController.heldScrap >= itemCost) {
            // you can afford it

            // figure out what slot to add it to

            // show the confirmation window
            shopUIEvents.ShowConfirmationWindow();

        }


        // note: make sure to have a way to specify what slot it'll be equipping to
        // either a selector or a drag n drop or smth

    }

}
