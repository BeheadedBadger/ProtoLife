using Assets.Scripts.LifeForms;
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
    public GameObject selectedHex;

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

            CreatureController();
        }
    }

    private void CreatureController()
    {
        foreach (HexTile tile in hexTiles)
        {
            if (tile.cover != null)
            {
                tile.cover.CheckTimeBasedEvents();
            }

            if (tile.stationary != null)
            {
                tile.stationary.CheckTimeBasedEvents();
            }

            if (tile.mobile != null)
            {
                tile.mobile.CheckTimeBasedEvents();
            }

            tile.LifeFormCheck();
        }
    }

    public void SelectSoil()
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
                    foreach (GeneInfo gene in tile.cover.genes)
                    {
                        if (gene.Expression == GeneExpression.On)
                        { tileData.coverGenetics.Add(2); }
                        if (gene.Expression == GeneExpression.Recessive)
                        { tileData.coverGenetics.Add(1); }
                        if (gene.Expression == GeneExpression.Off)
                        { tileData.coverGenetics.Add(0); }
                    }   
                }

                if (tile.stationary != null && tile.stationaryFilled)
                {
                    tileData.Stationary = tile.stationary.lifeFormObject.title;
                    tileData.stationaryAge = tile.stationary.age;
                    tileData.stationaryHealth = tile.stationary.health;
                    foreach (GeneInfo gene in tile.stationary.genes)
                    {
                        if (gene.Expression == GeneExpression.On)
                        { tileData.stationaryGenetics.Add(2); }
                        if (gene.Expression == GeneExpression.Recessive)
                        { tileData.stationaryGenetics.Add(1); }
                        if (gene.Expression == GeneExpression.Off)
                        { tileData.stationaryGenetics.Add(0); }
                    }
                }

                if (tile.mobile != null && tile.mobileFilled)
                {
                    tileData.Mobile = tile.mobile.lifeFormObject.title;
                    tileData.mobileAge = tile.mobile.age;
                    tileData.mobileHealth = tile.mobile.health; 
                    foreach (GeneInfo gene in tile.mobile.genes)
                    {
                        if (gene.Expression == GeneExpression.On)
                        { tileData.mobileGenetics.Add(2); }
                        if (gene.Expression == GeneExpression.Recessive)
                        { tileData.mobileGenetics.Add(1); }
                        if (gene.Expression == GeneExpression.Off)
                        { tileData.mobileGenetics.Add(0); }
                    }
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
            currentDate = new DateTime();
            currentDate = currentDate.AddDays(data.Day);
            currentDate = currentDate.AddMonths(data.Month);
            currentDate = currentDate.AddYears(data.Year);
            days = data.Day;
            months = data.Month;
            years = data.Year;

            foreach (BuildModeObject obj in inventory.objects)
            {
                Data.LifeformData lifedata = data.Lifeforms.Where(x => x.Name == obj.title).FirstOrDefault();
                if (lifedata != null)
                {
                    obj.unlocked = lifedata.Unlocked;
                }
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
                    tile.SetHeight(type);

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

                                GetGenes(tiledata, lifeform);

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

                                GetGenes(tiledata, lifeform);

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

                                GetGenes(tiledata, lifeform);

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

    private void GetGenes(Data.TileData tiledata, LifeForm lifeform)
    {
        foreach (GeneInfo gene in lifeform.lifeFormObject.standardGenetics)
        {
            GeneInfo newGene = gameObject.AddComponent(typeof(GeneInfo)) as GeneInfo;
            newGene.Name = gene.Name;
            newGene.Expression = gene.Expression;
            lifeform.genes.Add(newGene);
        }

        for (int i = 0; i < tiledata.coverGenetics.Count; i++)
        {
            if (tiledata.coverGenetics[i] == 2)
            { lifeform.genes[i].Expression = GeneExpression.On; }
            if (tiledata.coverGenetics[i] == 1)
            { lifeform.genes[i].Expression = GeneExpression.Recessive; }
            if (tiledata.coverGenetics[i] == 0)
            { lifeform.genes[i].Expression = GeneExpression.Off; }
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
