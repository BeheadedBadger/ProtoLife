using Assets.Scripts.LifeForms;
using System.Collections.Generic;
using UnityEngine;

public class PlantGenes : MonoBehaviour
{
    public enum GeneName { NoChlo = 0, LowChlo = 1, AnthoIncrease = 2, CaroIncease = 3 }

    public List<GeneInfo> GeneticInfo;
    //Chance out of 1000
    public int mutationChance = 3;

    public List<GeneInfo> SetGenes(List<GeneInfo> parent1, List<GeneInfo> parent2)
    {
        List<GeneInfo> childGenetics = new();
        //Asexual reproduction. Only variants through mutation.
        if (parent2 == null)
        {
            childGenetics.AddRange(CalculateAsexualGenes(parent1));
        }

        else
        {
            //GeneticInfo = CalculateSexualGenes(parent1, parent2);
        }

        return childGenetics;
    }

    private List<GeneInfo> CalculateAsexualGenes(List<GeneInfo> parent1)
    {
        bool mutationOccurred = false;
        int percentageRoll = Random.Range(0, 100);
        List<GeneInfo> childGenetics = new();

        foreach (GeneInfo gene in parent1)
        {
            GeneInfo newGene = gameObject.AddComponent(typeof(GeneInfo)) as GeneInfo;
            newGene.Name = gene.Name;
            newGene.Expression = gene.Expression;
            childGenetics.Add(newGene);
        }

        for (int i = 0; i < parent1.Count; i++)
        {
            percentageRoll = Random.Range(0, 1000);
            if (percentageRoll <= mutationChance && !mutationOccurred)
            {
                childGenetics[i].Expression = (GeneExpression)Random.Range(0, 2);
                mutationOccurred = true;
                Debug.Log($"A mutation has occurred on the {childGenetics[i].Name}! ({childGenetics[i].Expression})");
            }
            else
            {
                childGenetics[i].Expression = parent1[i].Expression;
            }
        }

        //Turn LowChlo off if NoChlo is active.
        if (childGenetics[1].Expression == GeneExpression.On && childGenetics[0].Expression == GeneExpression.On)
        {
            childGenetics[1].Expression = GeneExpression.Off;
        }

        return childGenetics;
    }

    public int? CheckMat(List<GeneInfo> genes)
    {
        //Normal
        if ((genes[0].Expression == GeneExpression.Off || genes[0].Expression == GeneExpression.Recessive)
            && (genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 0;
        }

        //NoChlo
        if (genes[0].Expression == GeneExpression.On)
        {
            return 1;
        }

        //LowChlo
        if ((genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 2;
        }

        //AnthoIncrease
        if ((genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
             && (genes[2].Expression == GeneExpression.On))
        { 
            return 3;
        }

        //LowChlo + AnthoIncrease
        if (genes[1].Expression == GeneExpression.On && genes[2].Expression == GeneExpression.On)
        {
            return 4;
        }

        //CaroIncrease
        if ((genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 5;
        }

        //LowChlo + CaroIncrease
        if (genes[1].Expression == GeneExpression.On && genes[3].Expression == GeneExpression.On)
        {
            return 6;
        }

        return null;
    }
}
