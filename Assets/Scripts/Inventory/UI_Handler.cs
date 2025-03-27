using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_Handler : MonoBehaviour
{
    public BuildModeObject.ObjectType selectedType;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject buildModePanel;
    Vector3 buildModePanelScale;
    [SerializeField] GameObject buildModeIconPanel;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] TextMeshProUGUI time;
    [SerializeField] TextMeshProUGUI lifeCoins;

    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI flavourText;

    [SerializeField] UnityEngine.UI.Image ViewMode;
    [SerializeField] Sprite activeViewMode;
    [SerializeField] Sprite inactiveViewMode;

    [SerializeField] UnityEngine.UI.Image BuildMode;
    [SerializeField] Sprite activeBuildMode;
    [SerializeField] Sprite inactiveBuildMode;

    [SerializeField] UnityEngine.UI.Image DemolishMode;
    [SerializeField] Sprite activeDemolishMode;
    [SerializeField] Sprite inactiveDemolishMode;

    public void SwitchType(BuildModeObject.ObjectType selection)
    {
        selectedType = selection;
        loadSelection(selectedType);
    }

    private void Awake()
    {
        loadSelection(selectedType);
        buildModePanelScale = buildModePanel.transform.localScale;
    }

    void Update()
    {
        if (!gameManager.BuildMode && !gameManager.DemolishMode && ViewMode.sprite != activeViewMode)
        {
            BuildMode.sprite = inactiveBuildMode;
            DemolishMode.sprite = inactiveDemolishMode;
            ViewMode.sprite = activeViewMode;
        }

        if (gameManager.BuildMode && BuildMode.sprite != activeBuildMode) 
        {
            BuildMode.sprite = activeBuildMode;
            DemolishMode.sprite = inactiveDemolishMode;
            ViewMode.sprite = inactiveViewMode;
        }

        if (gameManager.DemolishMode && DemolishMode.sprite != activeDemolishMode)
        {
            BuildMode.sprite = inactiveBuildMode;
            DemolishMode.sprite = activeDemolishMode;
            ViewMode.sprite = inactiveViewMode;
        }

        if (gameManager.BuildMode && buildModePanel.activeSelf == false)
        {
            buildModePanel.SetActive(true);
            StartCoroutine(LerpSize(buildModePanel, new Vector3(0, 0, 0), buildModePanelScale, 0.1f, false));
        }

        else if (gameManager.BuildMode == false && buildModePanel.activeSelf )
        {
            StartCoroutine(LerpSize(buildModePanel, buildModePanelScale, new Vector3(0, 0, 0), 0.1f, true));
        }

        if (gameManager.selectedObj != null)
        {
            title.text = gameManager.selectedObj.title;
            flavourText.text = gameManager.selectedObj.description;
        }
        else
        {
            title.text = "";
            flavourText.text = "";
        }

        time.text = $"Day: {gameManager.days}  Month: {gameManager.months}  Year: {gameManager.years}";
        lifeCoins.text = Mathf.RoundToInt(gameManager.LifeCoins).ToString("#,#");
    }

    public void loadSelection(BuildModeObject.ObjectType selection) 
    {
        gameManager.selectedSoil = null;
        gameManager.selectedObj = null;

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

    IEnumerator LerpSize(GameObject obj, Vector3 startScale, Vector3 targetScale, float duration, bool hideAfterwards)
    {
        float time = 0;
        Vector3 overshootRange = new Vector3(targetScale.x * 1.1f, targetScale.y * 1.1f, targetScale.z * 1.1f);

        //Overshoot first
        while (time < duration)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, overshootRange, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time / 4 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(overshootRange, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
        if (hideAfterwards)
        {
            obj.SetActive(false);
        }
    }
}
