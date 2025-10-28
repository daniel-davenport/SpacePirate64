using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class ShopUIEvents : MonoBehaviour
{
    [Header("References")]
    public ShopScript shopScript;
    public PlayerController playerController;
    public List<Button> shopButtons = new List<Button>();
    public List<Label> itemTexts = new List<Label>();

    private UIDocument document;
    private Button repairButton;
    private Button closeButton;

    private Label repairCost;
    private Label hullHealthLabel;
    private Label scrapAmount;

    // confirmation window
    private VisualElement confirmationWindow;
    private Button slot1ConfirmButton;
    private Label slot1Equipped;
    private Button slot2ConfirmButton;
    private Label slot2Equipped;
    private Button cancelConfirmButton;
    private Label equippingTitle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        document = GetComponent<UIDocument>();
        repairButton = document.rootVisualElement.Q("RepairButton") as Button;
        closeButton = document.rootVisualElement.Q("CloseButton") as Button;

        // getting labels
        hullHealthLabel = document.rootVisualElement.Q("HullHealth") as Label;
        scrapAmount = document.rootVisualElement.Q("ScrapAmount") as Label;
        repairCost = document.rootVisualElement.Q("RepairCost") as Label;

        repairButton.RegisterCallback<ClickEvent>(OnRepairClick);
        closeButton.RegisterCallback<ClickEvent>(OnCloseClick);

        // getting the confirmation window
        confirmationWindow = document.rootVisualElement.Q("ConfirmationWindow") as VisualElement;
        slot1ConfirmButton = document.rootVisualElement.Q("Slot1Button") as Button;
        slot1Equipped = slot1ConfirmButton.Q("EquippedLabel") as Label;
        slot2ConfirmButton = document.rootVisualElement.Q("Slot2Button") as Button;
        slot2Equipped = slot2ConfirmButton.Q("EquippedLabel") as Label;
        cancelConfirmButton = document.rootVisualElement.Q("CancelConfirmButton") as Button;
        equippingTitle = document.rootVisualElement.Q("EquippingTitle") as Label;


        // list of all buttons
        shopButtons = document.rootVisualElement.Query<Button>().ToList();

        // list of all item texts
        itemTexts = document.rootVisualElement.Query<Label>().ToList();


        int slot = 1;
        foreach (Button button in shopButtons)
        {
            // only counting buybuttons
            // note: it goes hierarchically, meaning the top button will be slot 1, and the bottom one will be whatever the max is
            if (button.name == "BuyButton")
            {
                int index = slot;
                button.clicked += () => TryItemBuy(index);
                slot++;
            }

        }

        // iterating through itemtexts backwards to remove any non-itemtext labels
        for (int i = itemTexts.Count - 1; i >= 0; i--)
        {
            if (itemTexts[i].name != "ItemText")
            {
                itemTexts.RemoveAt(i);
            } else
            {
                // test case to show it really exists (remember that the order in the UI Builder matters)
                itemTexts[i].text = (i).ToString();
            }
        }



        // disabling the shop so it doesnt appear
        HideDocument();

    }

    private void OnEnable()
    {
        //print("enabled");

        if (repairButton != null && closeButton != null)
        {
            repairButton.RegisterCallback<ClickEvent>(OnRepairClick);
            closeButton.RegisterCallback<ClickEvent>(OnCloseClick);
            cancelConfirmButton.RegisterCallback<ClickEvent>(OnCancelClick);
        }
        
    }

    private void OnDisable()
    {
        //print("disabled");

        if (repairButton != null && closeButton != null)
        {
            repairButton.UnregisterCallback<ClickEvent>(OnRepairClick);
            closeButton.UnregisterCallback<ClickEvent>(OnCloseClick);
            cancelConfirmButton.UnregisterCallback<ClickEvent>(OnCancelClick);
        }
            
    }

    private void OnRepairClick(ClickEvent ce)
    {
        shopScript.RepairShip();
    }

    private void OnCloseClick(ClickEvent ce)
    {
        shopScript.CloseShop();
    }

    private void OnCancelClick(ClickEvent ce)
    {
        HideConfirmationWindow();
    }


    // showing/hiding the document to avoid using enable/disable
    public void ShowDocument()
    {
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    
    public void HideDocument()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
        HideConfirmationWindow();
    }


    // updating the visual display names
    public void UpdateDisplayItems(string[] displayNames)
    {
        for (int i = 0; i < displayNames.Length; i++) {
            if (itemTexts[i] != null)
            {
                itemTexts[i].text = displayNames[i];
            }
        }

    }

    // show/hiding the confirm window
    public void ShowConfirmationWindow()
    {
        confirmationWindow.style.display = DisplayStyle.Flex;

    }

    public void HideConfirmationWindow()
    {
        confirmationWindow.style.display = DisplayStyle.None;

    }

    // TODO:
    /*
     * function for when you click confirm slot 1 or 2 that fires to the shop script that it was confirmed 
     * plugs similarly to the cancel/close buttons n stuff 
     * 
     * 
     */



    // try to buy the item that's held in the slot
    private void TryItemBuy(int slotNum)
    {
        shopScript.BuyItem(slotNum);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController != null)
        {
            // shop stats
            repairCost.text = "[" + shopScript.repairCost + "] SCRAP";

            // player stats
            hullHealthLabel.text = "HULL: " + playerController.playerHealth + "/" + playerController.maxHealth;
            scrapAmount.text = playerController.heldScrap.ToString();

            // player's weapons
            slot1Equipped.text = playerController.weaponHandler.equippedWeaponNames[0];
            slot2Equipped.text = playerController.weaponHandler.equippedWeaponNames[1];

        }
    }
}
