using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Soils : MonoBehaviour
{
    public GameManager manager;
    public Inventory inventory;
    public GameObject displayPrefab;
    Dictionary<SoilSlot, GameObject> itemsDisplayed = new Dictionary<SoilSlot, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        inventory = manager.inventory;
        CreateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDisplay();
    }

    private void CreateDisplay()
    {
        for (int i = 0; i < inventory.SoilItems.Count; i++)
        {
            GameObject obj = Instantiate(displayPrefab, this.transform);
            UI_SoilSlot objscript = obj.GetComponent<UI_SoilSlot>();
            objscript.gameManager = manager;
            objscript.soilName.text = inventory.SoilItems[i].soil.title;
            objscript.sprite.sprite = inventory.SoilItems[i].soil.icon;
            objscript.soilObject = inventory.SoilItems[i].soil;

            itemsDisplayed.Add(inventory.SoilItems[i], obj);

            Toggle toggle = obj.GetComponent<Toggle>();
            toggle.group = this.GetComponent<ToggleGroup>();

            if (i == 0)
            {
                toggle.isOn = true;
                manager.selectSoil(inventory.SoilItems[i].soil);
            }
        }
    }

    private void UpdateDisplay()
    {
        if (itemsDisplayed.Count < inventory.SoilItems.Count)
        {
            for (int i = 0; i < inventory.SoilItems.Count; i++)
            {
                GameObject obj = Instantiate(displayPrefab, this.transform);
                UI_SoilSlot objscript = obj.GetComponent<UI_SoilSlot>();
                objscript.soilName.text = inventory.SoilItems[i].soil.name;
                objscript.sprite.sprite = inventory.SoilItems[i].soil.icon;
            }
        }
    }
}
