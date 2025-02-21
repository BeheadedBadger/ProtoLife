using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool BuildMode;
    public bool DemolishMode;

    public int LifeCoins;

    public Inventory inventory;

    public SoilObject selectedSoil;
    //public ScriptableObject selectedItem;
    //public List<ScriptableObject> SoilLibrary;
    //public List<ScriptableObject> LifeformLibrary;


    void Start()
    {
        //Get amount of coins from memory
    }

    void Update()
    {
        
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
