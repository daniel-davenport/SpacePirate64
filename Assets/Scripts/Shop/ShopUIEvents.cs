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
        print("enabled");

        if (repairButton != null && closeButton != null)
        {
            repairButton.RegisterCallback<ClickEvent>(OnRepairClick);
            closeButton.RegisterCallback<ClickEvent>(OnCloseClick);
        }
        
    }

    private void OnDisable()
    {
        print("disabled");

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


    // showing/hiding the document to avoid using enable/disable
    public void ShowDocument()
    {
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    
    public void HideDocument()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
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

            // player stats
            hullHealthLabel.text = "HULL: " + playerController.playerHealth + "/" + playerController.maxHealth;
            scrapAmount.text = playerController.heldScrap.ToString();
        }
    }
}
