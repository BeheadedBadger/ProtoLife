using UnityEngine;

[CreateAssetMenu(fileName = "New Soil Object", menuName = "Custom/SoilObject")]
public class SoilObject : BuildModeObject
{
    public enum SoilType { Ash = 0, Sand = 1, Loam = 2, Clay = 3, Silt = 4, Water = 5 };
    public SoilType soilType;

    public void Awake()
    {
        objType = ObjectType.Soil;
    }
}
