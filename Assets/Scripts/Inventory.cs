using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Custom/Inventory")]
public class Inventory : ScriptableObject
{
    public List<SoilSlot> SoilItems;
    public List<InventorySlot> InventoryItems;

    public void AddSoil(SoilObject _soil)
    {
        if (_soil.unlocked == false) 
        {
            SoilItems.Add(new SoilSlot(_soil));
        }
    }

    public void AddLifeForm(LifeFormObject _lifeform, int _amount)
    { 
        bool hasItem = false;
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i].lifeform == _lifeform)
            {
                InventoryItems[i].AddAmount(_amount);
                hasItem = true;
                break;
            }
        }
        if (!hasItem)
        {
            InventoryItems.Add(new InventorySlot(_lifeform, _amount));
        }
    }
}

[System.Serializable]
public class SoilSlot 
{
    public SoilObject soil;

    public SoilSlot(SoilObject _soil)
    {
        soil = _soil;
    }
}

[System.Serializable]
public class InventorySlot 
{
    public LifeFormObject lifeform;
    public int amount;

    public InventorySlot(LifeFormObject _lifeform, int _amount)
    {
        lifeform = _lifeform;
        amount = _amount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}
