using UnityEngine;

[CreateAssetMenu(fileName = "New Soil Object", menuName = "Custom/SoilObject")]
public class SoilObject : ScriptableObject
{
    public enum SoilType { Ash = 0, Sand = 1, Loam = 2, Clay = 3, Silt = 4, Water = 5 };

    [TextArea(1, 12)] public string title; 
    public bool unlocked;
    public Sprite icon;
    public Material material;
    public SoilType type;
    [TextArea(15,20)]public string description;
}
