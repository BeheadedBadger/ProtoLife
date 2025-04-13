using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] GameObject amber;
    [SerializeField] Button button;

    //SoilTypes
    [SerializeField] Image Sand;
    [SerializeField] Image Loam;
    [SerializeField] Image Clay;
    [SerializeField] Image Silt;
    [SerializeField] Image Water;
    [SerializeField] Sprite disabled;

    //Water requirements
    [SerializeField] List<Image> waterRequirements;

    Vector3 activeRotation = new(0, 0, -10);
    Vector3 inactiveRotation = new(0, 0, 0);

    void FixedUpdate()
    {
        if (!infoSet)
        {
            SetInfo();
        }

        if (infoSet)
        {
            if (gameManager.selectedObj == associatedObj && associatedObj.selected == false)
            {
                SetToSelected();
            }

            if (gameManager.selectedObj != associatedObj && associatedObj.selected == true)
            {
                SetToUnselected();
            }
        }
    }

    public void SetToUnselected()
    {
        StartCoroutine(LerpRotation(this.gameObject, activeRotation, inactiveRotation, 0.2f, false));
        associatedObj.selected = false;
        //gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void SetToSelected()
    {
        StartCoroutine(LerpRotation(this.gameObject, inactiveRotation, activeRotation, 0.2f, true));
        associatedObj.selected = true;
        //gameObject.transform.eulerAngles = new Vector3(0, 0, -5);
    }

    public void OnUnlock()
    {
        associatedObj.unlocked = true;
        SetInfo();
    }

    public void OnClick()
    {
        if (!associatedObj.unlocked && gameManager.Amber >= associatedObj.unlockCost )
        {
            gameManager.Amber -= associatedObj.unlockCost;
            OnUnlock();
        }

        if (associatedObj.unlocked)
        {
            gameManager.selectedObj = associatedObj;
            if (associatedObj.objType == BuildModeObject.ObjectType.Soil)
            {
                gameManager.SelectSoil();
            }
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
        amber.SetActive(false);

        if (associatedObj.objType != BuildModeObject.ObjectType.Soil)
        {
            generates.text = (associatedObj.lifeCoinGeneration * 24).ToString("N1");
            SetSoilTypes();
            SetWaterRequirements();
        }

        else
        {
            generates.text = "";

            if (associatedObj.title != "Loam")
            {
                Loam.sprite = disabled;
            }
            if (associatedObj.title != "Sand")
            {
                Sand.sprite = disabled;
            }
            if (associatedObj.title != "Clay")
            {
                Clay.sprite = disabled;
            }
            if (associatedObj.title != "Water")
            {
                Water.sprite = disabled;
            }
            if (associatedObj.title != "Silt")
            {
                Silt.sprite = disabled;
            }

            noWaterNeed();
        }
    }

    private void SetLocked()
    {
        cost.text = "";
        generates.text = associatedObj.unlockCost.ToString();
        icon.sprite = lockedIcon;
        amber.SetActive(true);
        noWaterNeed();
        noSoilTypes();
    }

    private void noWaterNeed()
    {
        foreach (Image water in waterRequirements)
        {
            water.color = new Color(0, 0, 0, 0);
        }
    }

    private void noSoilTypes()
    {
        Sand.color = new Color(0, 0, 0, 0);
        Loam.color = new Color(0, 0, 0, 0);
        Clay.color = new Color(0, 0, 0, 0);
        Silt.color = new Color(0, 0, 0, 0);
        Water.color = new Color(0, 0, 0, 0);
    }

    private void SetSoilTypes()
    {
        Sand.color = new Color(1, 1, 1, 1);
        Loam.color = new Color(1, 1, 1, 1);
        Clay.color = new Color(1, 1, 1, 1);
        Silt.color = new Color(1, 1, 1, 1);
        Water.color = new Color(1, 1, 1, 1);

        if (!associatedObj.soilTypes.Contains(SoilObject.SoilType.Sand))
        {
            Sand.sprite = disabled;
        }

        if (!associatedObj.soilTypes.Contains(SoilObject.SoilType.Loam))
        {
            Loam.sprite = disabled;
        }

        if (!associatedObj.soilTypes.Contains(SoilObject.SoilType.Clay))
        {
            Clay.sprite = disabled;
        }

        if (!associatedObj.soilTypes.Contains(SoilObject.SoilType.Silt))
        {
            Silt.sprite = disabled;
        }

        if (!associatedObj.soilTypes.Contains(SoilObject.SoilType.Water))
        {
            Water.sprite = disabled;
        }
    }

    void SetWaterRequirements()
    {
        if (associatedObj.waterNeed < 9)
        {
            waterRequirements[4].sprite = disabled;
        }

        if (associatedObj.waterNeed < 7)
        {
            waterRequirements[3].sprite = disabled;
        }

        if (associatedObj.waterNeed < 5)
        {
            waterRequirements[2].sprite = disabled;
        }

        if (associatedObj.waterNeed < 3)
        {
            waterRequirements[1].sprite = disabled;
        }

        if (associatedObj.waterNeed < 1)
        {
            waterRequirements[0].sprite = disabled;
        }
    }

    IEnumerator LerpRotation(GameObject obj, Vector3 startRotation, Vector3 targetRotation, float duration, bool setActive)
    {
        float time = 0;
        while (time < duration)
        {
           obj.transform.rotation = Quaternion.Lerp(Quaternion.Euler(startRotation), Quaternion.Euler(targetRotation), time);
           time += Time.deltaTime;
           yield return null;
        }

        obj.transform.rotation = Quaternion.Euler(targetRotation);
    }
}
