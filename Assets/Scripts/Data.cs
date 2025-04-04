using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Data
{
    public int LifeCoins;
    public int Amber;
    public TileData[] Tiles;
    public LifeformData[] Lifeforms;
    public int Day;
    public int Month;
    public int Year;

    [Serializable]
    public class TileData
    {
        public string SoilType;
        public int NutrientScore;
        public int HumidityScore;
        public int ID;

        //Lifeforms
        public string Cover;
        public int coverAge;
        public int coverHealth;

        public string Stationary;
        public int stationaryAge;
        public int stationaryHealth;

        public string Mobile;
        public int mobileAge;
        public int mobileHealth;

        public TileData(string soilType, int nutrientScore, int humidityScore, int id)
        {
            ID = id;
            SoilType = soilType;
            NutrientScore = nutrientScore;
            HumidityScore = humidityScore;
        }
    }

    [Serializable]
    public class LifeformData
    {
        public string Name;
        public bool Unlocked;

        public LifeformData(string name, bool unlocked)
        {
            Name = name;
            Unlocked = unlocked;
        }
    }

    public Data(int coins, int amber, int day, int month, int year, TileData[] tiles, LifeformData[] lifeForms)
    {
        LifeCoins = coins;
        Amber = amber;
        Day = day;
        Month = month;
        Year = year;
        Tiles = tiles;
        Lifeforms = lifeForms;
    }

    public static void SaveGame(Data data)
    {
        BinaryFormatter formatter = new();
        string path = Application.persistentDataPath + "/Data.data";
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
    }

    public static Data LoadGame()
    {
        string path = Application.persistentDataPath + "/Data.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new();
            using (FileStream stream = new(path, FileMode.Open))
            {
                Data data = formatter.Deserialize(stream) as Data;
                return data;
            }
        }
        else 
        {
            return null;
        }
    }
}
