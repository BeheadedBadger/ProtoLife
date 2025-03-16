using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool BuildMode;
    public bool DemolishMode;

    public int LifeCoins;

    public Inventory inventory;

    public BuildModeObject selectedObj;
    public SoilObject selectedSoil;

    public float turnsSinceStart;
    float turnSpeed = 0.5f;
    public int days = 0;
    public int months = 0;
    public int years = 0;

    //public ScriptableObject selectedItem;
    //public List<ScriptableObject> SoilLibrary;
    //public List<ScriptableObject> LifeformLibrary;


    void Start()
    {
        //Get amount of coins from memory
    }

    void Update()
    {
        turnsSinceStart += (turnSpeed * Time.deltaTime);
        days = Mathf.RoundToInt(turnsSinceStart / 24);

        if (days > 30)
        { 
            days -= 30;
            months += 1;
        }
        if (months > 12)
        {
            months -= 12;
            years += 1;
        }
    }

    public void selectSoil()
    {
        selectedSoil = (SoilObject)selectedObj;
    }

    public void EnableBuildMode() 
    {
        BuildMode = true; 
        DemolishMode = false;
        if (selectedObj == null)
        {
            selectedObj = inventory.objects[0];
        }
    }

    public void EnableDemolishMode()
    {
        BuildMode = false;
        DemolishMode = true;
    }

    public void EnableViewMode() 
    {
        BuildMode = false;
        DemolishMode = false;
    }
}
