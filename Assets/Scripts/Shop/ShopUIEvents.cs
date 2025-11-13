using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class ShopUIEvents : MonoBehaviour
{
    [Header("References")]
    public ShopScript shopScript;
    public PlayerController playerController;
    public BombScript bombScript;
    public List<Button> shopButtons = new List<Button>();
    public List<Label> itemTexts = new List<Label>();

    private UIDocument document;
    private Button repairButton;
    private Button closeButton;
    private Button bombRestockButton;

    private Label repairCost;
    private Label hullHealthLabel;
    private Label scrapAmount;
    private Label bombRestockCost;
    private Label bombsHeld;

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
        // getting references
        bombScript = shopScript.bombScript;

        document = GetComponent<UIDocument>();
        repairButton = document.rootVisualElement.Q("RepairButton") as Button;
        closeButton = document.rootVisualElement.Q("CloseButton") as Button;
        bombRestockButton = document.rootVisualElement.Q("BombRestockButton") as Button;

        // getting labels
        hullHealthLabel = document.rootVisualElement.Q("HullHealth") as Label;
        scrapAmount = document.rootVisualElement.Q("ScrapAmount") as Label;
        repairCost = document.rootVisualElement.Q("RepairCost") as Label;
        bombRestockCost = document.rootVisualElement.Q("BombCost") as Label;
        bombsHeld = document.rootVisualElement.Q("BombsHeld") as Label;


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


            // counting the slot confirmation buttons
            if (button.name == "Slot1Button")
            {
                button.clicked += () => OnConfirmClick(0);
            }
            else if (button.name == "Slot2Button")
            {
                button.clicked += () => OnConfirmClick(1);
            }
            else if (button.name == "CancelConfirmButton")
            {
                button.clicked += () => OnCancelClick();
            }

            if (button.name == "BombRestockButton")
                button.clicked += () => OnBombRestockClick();


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
            // registering repair and close callbacks
            repairButton.RegisterCallback<ClickEvent>(OnRepairClick);
            closeButton.RegisterCallback<ClickEvent>(OnCloseClick);
        }
        
    }

    private void OnDisable()
    {
        //print("disabled");

        if (repairButton != null && closeButton != null)
        {
            repairButton.UnregisterCallback<ClickEvent>(OnRepairClick);
            closeButton.UnregisterCallback<ClickEvent>(OnCloseClick);
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

    private void OnBombRestockClick()
    {
        shopScript.BuyBomb();
    }

    private void OnCancelClick()
    {
        HideConfirmationWindow();
    }

    private void OnConfirmClick(int slot)
    {
        shopScript.confirmed = true;
        shopScript.changedSlot = slot;
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
    public void UpdateDisplayItems(string[] displayNames, int[] displayCosts)
    {
        // TODO:
        // update this to show the price of the weapon along with the name

        for (int i = 0; i < displayNames.Length; i++) {
            if (itemTexts[i] != null)
            {
                itemTexts[i].text = displayNames[i];

                // setting the cost
                Label itemCost = itemTexts[i].Q("CostLabel") as Label;
                itemCost.text = displayCosts[i].ToString();


            }
        }

    }

    // show/hiding the confirm window
    public void ShowConfirmationWindow(string buyingName, int buyCost)
    {
        confirmationWindow.style.display = DisplayStyle.Flex;
        equippingTitle.text = "BUYING: [" + buyingName + "] FOR " + buyCost + " SCRAP";

    }

    public void HideConfirmationWindow()
    {
        confirmationWindow.style.display = DisplayStyle.None;
        shopScript.cancelled = true;
    }

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
            bombRestockCost.text = "[" + shopScript.bombCost + "] SCRAP";

            // player stats
            hullHealthLabel.text = "HULL: " + playerController.playerHealth + "/" + playerController.maxHealth;
            bombsHeld.text = "HELD: " + bombScript.heldBombs + "/" + bombScript.maxBombs;
            scrapAmount.text = playerController.heldScrap.ToString();

            // TODO:
            // change this to be the weapon display name rather than internal name
            // also optionally show the weapon's level?

            // player's weapons
            if (playerController.weaponHandler.weaponInfoArr[0] != null && playerController.weaponHandler.weaponInfoArr[1] != null)
            {
                // shows the equipped weapon plus its current level
                slot1Equipped.text = playerController.weaponHandler.weaponInfoArr[0].weaponDisplayName + " [" + playerController.weaponHandler.weaponLevels[0] + "]";
                slot2Equipped.text = playerController.weaponHandler.weaponInfoArr[1].weaponDisplayName + " [" + playerController.weaponHandler.weaponLevels[1] + "]";
            }

        }
    }
}
