using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Life Object", menuName = "Custom/LifeFormObject")]
public class LifeFormObject : ScriptableObject
{
    public enum LifeType { Cover = 0, Stationary = 1, Mobile = 2 }
    public enum Kingdom { Fungi = 0, Animal = 1, Protist = 2, Plant = 3 }
    public bool unlocked;
    public float lifeCoinGeneration; //ranges from 0,1 to 1 and determines how often lifecoins are generated

    public GameObject lifeForm;
    public GameObject fossil; //Can be collected and used to unlock this lifeform
    public GameObject startingForm; //Egg, spore, seed, etc. Can be stored and used later
    public int inStock;
    public Sprite sprite; //Inventory sprite

    //Cover and Stationary are always 0, mobile ranges from 0,1 to 1
    public float mobility;
    public int maxHealth;
    public int health;

    //Requirements
    public List<LifeFormObject.Kingdom> foodSources;
    //Water need ranges from 1 to 10, based on what the lifeform can extract enough water from.
    //10 is just water tiles, 1 is low humidity soil tiles.
    public int waterNeed;
    //SoilTypes that the lifeform can thrive on and/or move to
    public List<SoilObject.SoilType> soilTypes;
    public int lifeSpan;
    public int age;

    //Procreation
    //Asexual lifeforms duplicate themselves without the need of others.
    //Sexual ones require another lifeform in a certain vicinity to reproduce.
    public enum procreationType { Asexual = 0, Sexual = 1 }
    //procreationTime determines how often the lifeform can reproduce, ranging from 0,1 to 1
    public float procreationTime; 
    //procreationRate determines how many new lifeforms can be created per procreation event.
    public int procreationRate;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Instantiate Lifeform

    //Destroy Lifeform / harvest
}
