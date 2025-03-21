using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Handler : MonoBehaviour
{
    BuildModeObject.ObjectType selectedType;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject buildModePanel;
    [SerializeField] GameObject buildModeIconPanel;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] TextMeshProUGUI time;

    public void SwitchType(BuildModeObject.ObjectType selection)
    { 
        selectedType = selection;
        loadSelection(selectedType);
    }

    private void Awake()
    {
        loadSelection(selectedType);
    }

    void Update()
    {
        if (gameManager.BuildMode)
        {
            buildModePanel.SetActive(true);
        }

        else {
            buildModePanel.SetActive(false);
        }
        time.text = $"Day: {gameManager.days}  Month: {gameManager.months}  Year:{gameManager.years}";
    }

    public void loadSelection(BuildModeObject.ObjectType selection) 
    {
        selectedType = selection;
        foreach (Transform child in buildModeIconPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (BuildModeObject obj in gameManager.inventory.objects)
        {
            if (obj.objType == selectedType)
            {
                GameObject createdIcon = Instantiate(itemPrefab, buildModeIconPanel.transform);
                ItemSlot itemSlot = createdIcon.GetComponent<ItemSlot>();
                itemSlot.associatedObj = obj;
                itemSlot.gameManager = gameManager;
            }
        } 
    }
}
