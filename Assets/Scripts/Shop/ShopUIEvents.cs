using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopUIEvents : MonoBehaviour
{
    [Header("References")]
    public ShopScript shopScript;
    public List<Button> shopButtons = new List<Button>();

    private UIDocument document;
    private Button repairButton;
    private Button closeButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        document = GetComponent<UIDocument>();
        repairButton = document.rootVisualElement.Q("RepairButton") as Button;
        closeButton = document.rootVisualElement.Q("CloseButton") as Button;

        repairButton.RegisterCallback<ClickEvent>(OnRepairClick);
        closeButton.RegisterCallback<ClickEvent>(OnCloseClick);

        // list of all buttons
        shopButtons = document.rootVisualElement.Query<Button>().ToList();

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

        // disabling the shop so it doesnt appear
        //gameObject.SetActive(false);

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
        Debug.Log("clicked repair");
    }

    private void OnCloseClick(ClickEvent ce)
    {
        Debug.Log("clicked close");
    }

    // try to buy the item that's held in the slot
    private void TryItemBuy(int slotNum)
    {
        shopScript.BuyItem(slotNum);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(true);
    }
}
