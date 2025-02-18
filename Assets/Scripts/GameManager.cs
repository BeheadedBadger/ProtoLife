using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool BuildMode;
    public bool DemolishMode;
    public int LifeCoins;

    // Start is called before the first frame update
    void Start()
    {
        //Get amount of coins from memory
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
