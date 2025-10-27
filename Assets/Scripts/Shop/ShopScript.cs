using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;

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

    // setting up loot table .json file
    [System.Serializable]
    public class Item
    {
        public string name;
        public string displayName;
        public int rarity; // lower number = less rare
        public int cost;
    }

    [System.Serializable]
    public class ItemList
    {
        public Item[] items;
    }

    [SerializeField]
    public ItemList allItemsList = new ItemList();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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


        // manually printing every item at the end
        /*
        for (int i = 0; i < allItemsList.items.Length; i++) { 
            print(allItemsList.items[i].name);
        }
        */

    }

    // called at a level end, creates the shop's stock and shows the shop hud.
    public void RefreshShop()
    {
        // generate the shop's stock using RNG to determine the rarity of each item

        // fire it to the shopui so it can display the text for every slot


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
        print("clicked repair");
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

    // compares the current held scrap with the cost of the item
    // if the item can be afforded then do the appropriate thing with it.
    // otherwise do nothing.
    public void BuyItem(int slot)
    {
        print("buying item in slot " + slot);

    }

}
