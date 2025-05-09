using Assets.Scripts.LifeForms;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Life Object", menuName = "Custom/LifeFormObject")]
public class LifeFormObject : BuildModeObject
{
    public enum LifeType { Cover = 0, Stationary = 1, Mobile = 2 }
    public LifeType lifeType;
    public enum Kingdom { Fungi = 0, Animal = 1, Protist = 2, Plant = 3, Nitrates = 4 }
    public Kingdom kingdom;

    public GameObject lifeForm;

    public int mobility;
    public int damage;
    public int maxHealth;

    //Requirements
    public float feedingRate;
    public List<LifeFormObject.Kingdom> foodSources;

    public int lifeSpan;

    //Procreation
    public enum ProcreationType { Asexual = 0, Sexual = 1 }
    public ProcreationType procreationType;
    public float procreationTime; 
    public int procreationRate;

    //Genetics
    public List<Material> materials;
    public List<GeneInfo> standardGenetics;
    public int standardColouration;

    public void Awake()
    {
        if (lifeType == LifeType.Cover)
        {
            objType = ObjectType.Cover;
        }
        if (lifeType == LifeType.Stationary)
        {
            objType = ObjectType.Stationary;
        }
        if (lifeType == LifeType.Mobile) 
        {
            objType = ObjectType.Mobile; 
        }
    }
}
