using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public GameManager gameManager;
    public BuildModeObject associatedObj;
    bool infoSet;
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI generates;
    [SerializeField] Image icon;
    [SerializeField] Sprite lockedIcon;
    [SerializeField] Button button;

    //SoilTypes
    [SerializeField] GameObject Sand;
    [SerializeField] GameObject Loam;
    [SerializeField] GameObject Clay;
    [SerializeField] GameObject Silt;
    [SerializeField] GameObject Water;

    //Water requirements
    [SerializeField] List<Image> waterRequirements;
    [SerializeField] Sprite waterFilled;

    void Update()
    {
        if (!infoSet)
        {
            SetInfo();
        }
    }

    public void OnUnlock() 
    {
        SetInfo();
    }

    public void OnClick()
    {
        gameManager.selectedObj = associatedObj;
        if (associatedObj.objType == BuildModeObject.ObjectType.Soil)
        {
            gameManager.selectSoil();
        }
    }

    private void SetInfo()
    {
        if (!associatedObj.unlocked)
        {
            SetLocked();
        }

        else
        {
            SetUnlocked();
        }

        infoSet = true;
    }

    private void SetUnlocked()
    {
        button.enabled = true;
        cost.text = associatedObj.cost.ToString();
        icon.sprite = associatedObj.sprite;
       
        if (associatedObj.objType != BuildModeObject.ObjectType.Soil)
        {
            generates.text = associatedObj.lifeCoinGeneration.ToString();
            SetSoilTypes();
            SetWaterRequirements();
            waterNeed();
        }

        else
        {
            generates.text = "";
            noWaterNeed();
        }
    }

    private void SetLocked()
    {
        cost.text = "";
        generates.text = "";
        icon.sprite = lockedIcon;
        button.enabled = false;
        noWaterNeed();
    }

    private void noWaterNeed()
    {
        foreach (Image water in waterRequirements)
        {
            water.color = new Color(0, 0, 0, 0);
        }
    }

    private void waterNeed() 
    {
        foreach (Image water in waterRequirements)
        {
            water.color = new Color(0.34f, 0.34f, 0.34f, 1);
        }
    }

    private void SetSoilTypes()
    {
        if (associatedObj.soilTypes.Contains(SoilObject.SoilType.Sand))
        {
            Sand.SetActive(true);
        }

        if (associatedObj.soilTypes.Contains(SoilObject.SoilType.Loam))
        {
            Loam.SetActive(true);
        }

        if (associatedObj.soilTypes.Contains(SoilObject.SoilType.Clay))
        {
            Clay.SetActive(true);
        }

        if (associatedObj.soilTypes.Contains(SoilObject.SoilType.Silt))
        {
            Silt.SetActive(true);
        }

        if (associatedObj.soilTypes.Contains(SoilObject.SoilType.Water))
        {
            Water.SetActive(true);
        }
    }

    void SetWaterRequirements()
    {
        for (int i = 0; i < associatedObj.waterNeed; i++)
        {
            waterRequirements[i].sprite = waterFilled;
        }
    }
}
