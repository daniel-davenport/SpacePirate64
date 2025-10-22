using UnityEngine;

public class ShopScript : MonoBehaviour
{
    [Header("References")]
    public GameObject shopUI;
    public LevelDirector levelDirector;
    public ShopUIEvents shopUIEvents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shopUIEvents = shopUI.GetComponent<ShopUIEvents>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // called at a level end, creates the shop's stock and shows the shop hud.
    public void RefreshShop()
    {



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
    }

    // compares the current held scrap with the cost of the item
    // if the item can be afforded then do the appropriate thing with it.
    // otherwise do nothing.
    public void BuyItem(int slot)
    {
        print("buying item in slot " + slot);

    }

}
