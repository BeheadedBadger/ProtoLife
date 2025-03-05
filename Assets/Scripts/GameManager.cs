using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool BuildMode;
    public bool DemolishMode;
    public bool pointerOverUi;

    public int LifeCoins;

    public Inventory inventory;

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

    public void selectSoil(SoilObject soil)
    {
        selectedSoil = soil;
    }

    public void EnableBuildMode() 
    {
        BuildMode = true; 
        DemolishMode = false;  
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

    public void selectSoilType(SoilObject.SoilType soilType)
    {
        //selectedSoilType = soilType;
    } 
}
