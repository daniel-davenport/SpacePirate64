using UnityEngine;
using UnityEngine.UIElements;

public class ControlsPanel : MonoBehaviour
{

    private UIDocument document;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        document = GetComponent<UIDocument>();

        HideControls();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void HideControls()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void ShowControls()
    {
        if (document.rootVisualElement.style.display == DisplayStyle.Flex)
        {
            document.rootVisualElement.style.display = DisplayStyle.None;
        }
        else
        {
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }
            
    }


}
