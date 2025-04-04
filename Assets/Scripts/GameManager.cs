using System;
using System.Collections.Generic;
using System.Linq;
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
    public int hours = 0;
    public int days = 1;
    public int months = 1;
    public int years = 1;
    public DateTime currentDate = new();
    int nextTimeUpdate = 1;

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
            currentDate = currentDate.AddHours(1);
            nextTimeUpdate = Mathf.RoundToInt(turnsSinceStart) + 1;

            if (days < currentDate.Day)
            {
                Save();
            }

            hours = currentDate.Hour;
            days = currentDate.Day;
            months = currentDate.Month;
            years = currentDate.Year;
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
                Data.TileData tileData = new(tile.soilFill.thisSoilType.ToString(), tile.soilFill.nutrientScore, tile.soilFill.waterScore, tile.GetInstanceID());

                if (tile.cover != null && tile.coverFilled)
                {
                    tileData.Cover = tile.cover.lifeFormObject.title;
                    tileData.coverAge = tile.cover.age;
                    tileData.coverHealth = tile.cover.health;
                }

                if (tile.stationary != null && tile.stationaryFilled)
                {
                    tileData.Stationary = tile.stationary.lifeFormObject.title;
                    tileData.stationaryAge = tile.stationary.age;
                    tileData.stationaryHealth = tile.stationary.health;
                }

                if (tile.mobile != null && tile.mobileFilled)
                {
                    tileData.Mobile = tile.mobile.lifeFormObject.title;
                    tileData.mobileAge = tile.mobile.age;
                    tileData.mobileHealth = tile.mobile.health;
                }

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

                    SoilObject.SoilType type = (SoilObject.SoilType)Enum.Parse(typeof(SoilObject.SoilType), tiledata.SoilType);
                    if (type != SoilObject.SoilType.Ash)
                    {
                        tile.soilFilled = true;
                    }
                    tile.soilFill.nutrientScore = tiledata.NutrientScore;
                    tile.soilFill.waterScore = tiledata.HumidityScore;

                    tile.soilFill.SetSoilType(type, tile);
                    tile.soilFill.soilBasicPosition = tile.soilBasicPosition;
                    tile.soilFill.soilSelectedPosition = tile.soilSelectedPosition;
                    tile.FindNeighbors(tile.grid.transform.position);

                    if (tiledata.Cover != null)
                    {
                        foreach (BuildModeObject obj in inventory.objects)
                        {
                            if (tiledata.Cover == obj.title)
                            {
                                GameObject instantiated = Instantiate(obj.prefab, tile.coverContainer.transform); 
                                LifeForm lifeform = instantiated.GetComponent<LifeForm>();
                                lifeform.age = tiledata.coverAge;
                                lifeform.health = tiledata.coverHealth;

                                lifeform.createLifeForm(tile);
                                tile.coverFilled = true;
                                tile.cover = lifeform;
                            }
                        }
                    }

                    if (tiledata.Stationary != null)
                    {
                        foreach (BuildModeObject obj in inventory.objects)
                        {
                            if (tiledata.Stationary == obj.title)
                            {
                                GameObject instantiated = Instantiate(obj.prefab, tile.stationaryContainer.transform);
                                LifeForm lifeform = instantiated.GetComponent<LifeForm>();
                                lifeform.age = tiledata.stationaryAge;
                                lifeform.health = tiledata.stationaryHealth;

                                lifeform.createLifeForm(tile);
                                tile.stationaryFilled = true;
                                tile.stationary = lifeform;
                            }
                        }
                    }

                    if (tiledata.Mobile != null)
                    {
                        foreach (BuildModeObject obj in inventory.objects)
                        {
                            if (tiledata.Mobile == obj.title)
                            {
                                GameObject instantiated = Instantiate(obj.prefab, tile.mobileContainer.transform);
                                LifeForm lifeform = instantiated.GetComponent<LifeForm>();
                                lifeform.age = tiledata.mobileAge;
                                lifeform.health = tiledata.mobileHealth;

                                lifeform.createLifeForm(tile);
                                tile.mobileFilled = true;
                                tile.mobile = lifeform;
                            }
                        }
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
