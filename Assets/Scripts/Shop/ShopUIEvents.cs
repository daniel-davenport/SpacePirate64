using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Button = UnityEngine.UIElements.Button;
using Unity.VisualScripting;

public class ShopUIEvents : MonoBehaviour
{
    [Header("References")]
    public ShopScript shopScript;
    public PlayerController playerController;
    public BombScript bombScript;
    public List<Button> shopButtons = new List<Button>();
    private List<Label> itemList = new List<Label>();
    private List<Label> itemTexts = new List<Label>();
    private List<Label> weaponTexts = new List<Label>();

    private UIDocument document;
    private Button repairButton;
    private Button closeButton;
    private Button bombRestockButton;
    private VisualElement hoverTooltipWindow;

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

    private Label hoverTitle;
    private Label hoverDescription;
    private bool hoveringOverWeapon = false;
    private bool hoveringOverItem = false;
    private bool hoveringLabel = false;
    private int hoveringSlot = -1;

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

        // getting tooltip
        hoverTooltipWindow = document.rootVisualElement.Q("DescriptionTooltip") as VisualElement;

        slot1ConfirmButton = document.rootVisualElement.Q("Slot1Button") as Button;
        slot1Equipped = slot1ConfirmButton.Q("EquippedLabel") as Label;

        slot2ConfirmButton = document.rootVisualElement.Q("Slot2Button") as Button;
        slot2Equipped = slot2ConfirmButton.Q("EquippedLabel") as Label;

        cancelConfirmButton = document.rootVisualElement.Q("CancelConfirmButton") as Button;
        equippingTitle = document.rootVisualElement.Q("EquippingTitle") as Label;


        // list of all buttons
        shopButtons = document.rootVisualElement.Query<Button>().ToList();

        // list of all item texts
        itemList = document.rootVisualElement.Query<Label>().ToList();

        // getting the hover tooltip
        hoverTitle = hoverTooltipWindow.Q("Title") as Label;
        hoverDescription = hoverTooltipWindow.Q("Description") as Label;

        int weaponSlot = 1;
        int itemSlot = 1;
        foreach (Button button in shopButtons)
        {
            // only counting buybuttons
            // note: it goes hierarchically, meaning the top button will be slot 1, and the bottom one will be whatever the max is
            if (button.name == "BuyButton")
            {
                int index = weaponSlot;
                button.clicked += () => TryWeaponBuy(index);
                weaponSlot++;
            }
            else if (button.name == "BuyItemButton")
            {
                int index = itemSlot;
                button.clicked += () => TryItemBuy(index);
                itemSlot++;
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


        // this has to go backwards because of the removals
        int weaponLabelSlot = shopScript.maxWeapons - 1;
        int itemLabelSlot = shopScript.maxItems - 1;
        // iterating through itemList backwards to remove any non-itemtext labels
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (itemList[i].name == "WeaponText")
            {
                // inserting it at 0 to shift everything over later (remember that the order in the UI Builder matters)
                weaponTexts.Insert(0, itemList[i]); // (since we're iterating backwards, inserting at 0 makes sense because the first item should be last, technically)

                // table to pass more args than unity allows
                int[] slotTable = new int[2];
                slotTable[0] = 0; // 0 = weapon
                slotTable[1] = weaponLabelSlot;

                itemList[i].RegisterCallback<MouseEnterEvent, int[]>(ItemMouseEnter, slotTable);
                itemList[i].RegisterCallback<MouseLeaveEvent, int[]>(ItemMouseExit, slotTable);
                weaponLabelSlot--;
            }
            else if (itemList[i].name == "ItemText")
            {
                // test case to show it really exists (remember that the order in the UI Builder matters)
                itemTexts.Insert(0, itemList[i]);

                // table to pass more args than unity allows
                int[] slotTable = new int[2];
                slotTable[0] = 1; // 1 = item
                slotTable[1] = itemLabelSlot;

                itemList[i].RegisterCallback<MouseEnterEvent, int[]>(ItemMouseEnter, slotTable);
                itemList[i].RegisterCallback<MouseLeaveEvent, int[]>(ItemMouseExit, slotTable);
                itemLabelSlot--;
            }
            else
            {
                itemList.RemoveAt(i);
            }

        }

        // starting the tooltip hover routine
        StartCoroutine(SetTooltipWindow());


        // disabling the shop so it doesnt appear
        HideDocument();

    }


    private IEnumerator SetTooltipWindow()
    {
        while (true)
        {
            Vector2 screenPosition = Input.mousePosition;

            //print(hoveringOverText);
            hoverTooltipWindow.visible = hoveringLabel;

            //print(screenPosition);
            // inverting Y-axis which fixes some issues
            screenPosition.y = -screenPosition.y;

            hoverTooltipWindow.transform.position = screenPosition;

            yield return null;
        }
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


    // showing description tooltips
    // slot 0 is a 0 or 1 to determine if it's a weapon or item
    // slot 1 is the actual slot
    private void ItemMouseEnter(MouseEnterEvent me, int[] slotInfo)
    {
        if (slotInfo[0] == 0)
            hoveringOverWeapon = true;
        else
            hoveringOverItem = true;

        hoveringLabel = true;
        hoveringSlot = slotInfo[1];
    }

    private void ItemMouseExit(MouseLeaveEvent me, int[] slotInfo)
    {
        hoveringLabel = false;
        hoveringSlot = -1;
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
    public void UpdateDisplayWeapons(string[] displayNames, int[] displayCosts)
    {

        for (int i = 0; i < displayNames.Length; i++) {
            if (weaponTexts[i] != null)
            {
                weaponTexts[i].text = displayNames[i];

                // setting the cost
                Label itemCost = weaponTexts[i].Q("CostLabel") as Label;
                itemCost.text = displayCosts[i].ToString();
            }
        }

    }

    // same as above
    public void UpdateDisplayItems(string[] displayNames, int[] displayCosts)
    {
        //print(displayNames.Length + " " + displayCosts.Length);

        for (int i = 0; i < displayNames.Length; i++)
        {
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
    private void TryWeaponBuy(int slotNum)
    {
        shopScript.BuyWeapon(slotNum);
    }

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

            // player's weapons
            if (playerController.weaponHandler.weaponInfoArr[0] != null && playerController.weaponHandler.weaponInfoArr[1] != null)
            {
                // shows the equipped weapon plus its current level
                slot1Equipped.text = playerController.weaponHandler.weaponInfoArr[0].weaponDisplayName + " [" + playerController.weaponHandler.weaponLevels[0] + "]";
                slot2Equipped.text = playerController.weaponHandler.weaponInfoArr[1].weaponDisplayName + " [" + playerController.weaponHandler.weaponLevels[1] + "]";
            }

            if (hoveringOverWeapon == true)
            {
                if (hoveringSlot >= 0)
                {
                    hoverTitle.text = shopScript.sellingWeaponDisplayNames[hoveringSlot] + " - Tier " + shopScript.sellingWeaponDisplayTiers[hoveringSlot];
                    hoverDescription.text = shopScript.sellingWeaponDescriptions[hoveringSlot];
                }
            }
            else if (hoveringOverItem == true)
            {
                if (hoveringSlot >= 0)
                {
                    hoverTitle.text = shopScript.sellingItemDisplayNames[hoveringSlot];
                    hoverDescription.text = shopScript.sellingItemDescriptions[hoveringSlot];
                }
            }

        }
    }
}
