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

    // Update is called once per frame
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
            cost.text = "";
            generates.text = "";
            icon.sprite = lockedIcon;
            button.enabled = false;
        }

        else
        {
            button.enabled = true;
            cost.text = associatedObj.cost.ToString();
            icon.sprite = associatedObj.sprite;
            if (associatedObj.lifeCoinGeneration <= 0)
            {
                generates.text = "";
            }
            else
            {
                generates.text = associatedObj.lifeCoinGeneration.ToString();
            }
        }

        infoSet = true;
    }
}
