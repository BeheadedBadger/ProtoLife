using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Custom/Inventory")]
public class Inventory : ScriptableObject
{
    public List<BuildModeObject> objects;
}
