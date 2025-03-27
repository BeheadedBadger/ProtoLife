using System.Collections.Generic;
using UnityEngine;

public abstract class BuildModeObject : ScriptableObject 
{
    public enum ObjectType { Soil = 0, Cover = 1, Stationary = 2, Mobile = 3 };

    [TextArea(1, 12)] public string title;
    [TextArea(15, 20)] public string description;
    public ObjectType objType;
    public bool unlocked;
    public GameObject prefab;
    public Sprite sprite;
    public int cost;
    public float lifeCoinGeneration;

    public int waterNeed;
    public List<SoilObject.SoilType> soilTypes;

    public bool selected;
}
