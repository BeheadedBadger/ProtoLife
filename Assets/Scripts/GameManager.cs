using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool BuildMode;
    public bool DemolishMode;

    public float LifeCoins = new();
    public int Amber = new();

    public Inventory inventory;
    [SerializeField] public List<HexTile> hexTiles;

    public BuildModeObject selectedObj;
    public SoilObject selectedSoil;
    public HexTile selectedHex;

    public float turnsSinceStart;
    float turnSpeed = 0.5f;
    public int days = 1;
    public int months = 1;
    public int years = 1;
    public DateTime currentDate = new();
    int nextTimeUpdate = 24;

    void Start()
    {
        Load();
    }

    void Update()
    {
        if (days == 0)
        { days = 1; }

        if (months == 0)
        { months = 1; }

        if (years == 0)
        { years = 1; }

        turnsSinceStart += (turnSpeed * Time.deltaTime);

        if (LifeCoins < 0)
        { LifeCoins = 0; }

        if (nextTimeUpdate < turnsSinceStart)
        {
            currentDate = currentDate.AddDays(1);
            nextTimeUpdate = Mathf.RoundToInt(turnsSinceStart) + 24;

            days = currentDate.Day;
            months = currentDate.Month;
            years = currentDate.Year;

            Save();
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

    public void Save()
    {
        List<Data.LifeformData> AllLifeformData = new();
        foreach (BuildModeObject obj in inventory.objects)
        {
            Data.LifeformData lifeformData = new(obj.title, obj.unlocked);
            AllLifeformData.Add(lifeformData);
        }

        List<Data.TileData> AllTileData = new();
        foreach (HexTile tile in hexTiles)
        {
            if (tile != null)
            {
                List<float> gridPosition = new List<float> { tile.tileBasicPosition.x, tile.tileBasicPosition.y, tile.tileBasicPosition.z };
                List<float> soilPosition = new List<float> { tile.soilBasicPosition.x, tile.soilBasicPosition.y, tile.soilBasicPosition.z };
                Data.TileData tileData = new(gridPosition.ToArray(), soilPosition.ToArray(), tile.soilFill.thisSoilType.ToString(), tile.soilFill.nutrientScore, tile.soilFill.waterScore, tile.GetInstanceID());
                AllTileData.Add(tileData);
            }
        }

        Data data = new Data(Mathf.RoundToInt(LifeCoins), Amber, days, months, years, AllTileData.ToArray(), AllLifeformData.ToArray());
        Data.SaveGame(data);
    }

    public void Load()
    {
        Data data = Data.LoadGame();

        if (data != null)
        {
            LifeCoins = data.LifeCoins;
            Amber = data.Amber;
            currentDate = new DateTime(data.Year, data.Month, data.Day);

            foreach (BuildModeObject obj in inventory.objects)
            {
                Data.LifeformData lifedata = data.Lifeforms.Where(x => x.Name == obj.title).FirstOrDefault();
                obj.unlocked = lifedata.Unlocked;
            }

            foreach (HexTile tile in hexTiles)
            {
                if (tile != null)
                {
                    Data.TileData tiledata = data.Tiles.Where(x => x.ID == tile.GetInstanceID()).FirstOrDefault();
                    tile.grid.transform.position = new Vector3(tiledata.PositionGrid[0], tiledata.PositionGrid[1], tiledata.PositionGrid[2]);
                    tile.soilFill.transform.position = new Vector3(tiledata.PositionSoil[0], tiledata.PositionSoil[1], tiledata.PositionSoil[2]);
                    tile.soilFill.nutrientScore = tiledata.NutrientScore;
                    tile.soilFill.waterScore = tiledata.HumidityScore;

                    tile.FindNeighbors(tile.grid.transform.position);

                    SoilObject.SoilType type = (SoilObject.SoilType)Enum.Parse(typeof(SoilObject.SoilType), tiledata.SoilType);
                    tile.soilFill.SetSoilType(type, tile);
                    if (type != SoilObject.SoilType.Ash)
                    { tile.soilFilled = true; }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
