using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soil : MonoBehaviour
{
    public enum SoilType { Ash = 0, Sand = 1, Loam = 2, Clay = 3, Silt = 4, Water = 5 };

    public SoilType soilType; 
    public int waterScore;
    public int nutrientScore;
    public HexTile parentHex;

    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;

    public void changesoilType()
    { 
        //if soil, do the soil animation thingy from hextile here
        //if water, something else, also in hextile right now
        //TODO
    }

    public void CalculateWaterScore()
    {
        if (this.soilType == SoilType.Ash)
        {
            waterScore = 0;
        }

        if (this.soilType == SoilType.Water)
        {
            waterScore = 10;
        }

        else foreach (HexTile neighbour in parentHex.neighboringHexTiles)
        {
            if (neighbour.soilFill.waterScore - 1 > this.waterScore)
            {
                this.waterScore = neighbour.soilFill.waterScore - 1;
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
