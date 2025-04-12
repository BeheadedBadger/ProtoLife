using Assets.Scripts.LifeForms;
using System.Collections.Generic;
using UnityEngine;

public class FungiGenes : MonoBehaviour
{
    //ybrm, YBrm, YbRm, yBRm, YBRm, YBrM, YbRM, yBRM
    public enum GeneName { Melanin = 0, Carotenoids = 1, Azulenes = 2, Astaxanthin = 3 }

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
            childGenetics = CalculateSexualGenes(parent1, parent2);
        }

        return childGenetics;
    }

    private List<GeneInfo> CalculateSexualGenes(List<GeneInfo> parent1, List<GeneInfo> parent2)
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
            percentageRoll = Random.Range(1, 1000);
            if (percentageRoll <= mutationChance && !mutationOccurred)
            {
                childGenetics[i].Expression = (GeneExpression)Random.Range(0, 2);
                mutationOccurred = true;
                Debug.Log($"A mutation has occurred on the {childGenetics[i].Name}! ({childGenetics[i].Expression})");
            }
            else
            {
                if (parent1[i].Expression == GeneExpression.On && parent2[i].Expression == GeneExpression.On)
                {
                    childGenetics[i].Expression = GeneExpression.On;
                }

                if ((parent1[i].Expression == GeneExpression.Off && parent2[i].Expression == GeneExpression.On) ||
                   (parent1[i].Expression == GeneExpression.On && parent2[i].Expression == GeneExpression.Off))
                {
                    childGenetics[i].Expression = GeneExpression.Recessive;
                }

                if ((parent1[i].Expression == GeneExpression.On && parent2[i].Expression == GeneExpression.Recessive) ||
                    (parent1[i].Expression == GeneExpression.Recessive && parent2[i].Expression == GeneExpression.On))
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 50)
                    {
                        childGenetics[i].Expression = GeneExpression.On;
                    }
                    else
                    {
                        childGenetics[i].Expression = GeneExpression.Recessive;
                    }
                }

                if (parent1[i].Expression == GeneExpression.Recessive && parent2[i].Expression == GeneExpression.Recessive)
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 66)
                    {
                        childGenetics[i].Expression = GeneExpression.On;
                    }
                    else if (percentageRoll >= 33 && percentageRoll < 66)
                    {
                        childGenetics[i].Expression = GeneExpression.Recessive;
                    }
                    else
                    {
                        childGenetics[i].Expression = GeneExpression.Off;
                    }
                }

                if ((parent1[i].Expression == GeneExpression.Off && parent2[i].Expression == GeneExpression.Recessive) ||
                    (parent1[i].Expression == GeneExpression.Recessive && parent2[i].Expression == GeneExpression.Off))
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 50)
                    {
                        childGenetics[i].Expression = GeneExpression.Recessive;
                    }
                    else
                    {
                        childGenetics[i].Expression = GeneExpression.Off;
                    }
                }

                if (parent1[i].Expression == GeneExpression.Off && parent2[i].Expression == GeneExpression.Off)
                {
                    childGenetics[i].Expression = GeneExpression.Off;
                }
            }
        }

        //If two colour genes are not producing pigment, colour pigment production is shut down.
        if (childGenetics[1].Expression == GeneExpression.Off && childGenetics[2].Expression == GeneExpression.Off
            || childGenetics[1].Expression == GeneExpression.Off && childGenetics[3].Expression == GeneExpression.Off
            || childGenetics[2].Expression == GeneExpression.Off && childGenetics[3].Expression == GeneExpression.Off)
        {
            childGenetics[1].Expression = GeneExpression.Off;
            childGenetics[2].Expression = GeneExpression.Off;
            childGenetics[3].Expression = GeneExpression.Off;
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
            }
            else
            {
                childGenetics[i].Expression = parent1[i].Expression;
            }
        }

        //If two colour genes are not producing pigment, colour pigment production is shut down.
        if ((childGenetics[1].Expression == GeneExpression.Off && childGenetics[2].Expression == GeneExpression.Off)
            || (childGenetics[1].Expression == GeneExpression.Off && childGenetics[3].Expression == GeneExpression.Off)
            || (childGenetics[2].Expression == GeneExpression.Off && childGenetics[3].Expression == GeneExpression.Off))
        {
            childGenetics[1].Expression = GeneExpression.Off;
            childGenetics[2].Expression = GeneExpression.Off;
            childGenetics[3].Expression = GeneExpression.Off;
        }

        return childGenetics;
    }

    public int? CheckMat(List<GeneInfo> genes)
    {
        //as az c M
        if ((genes[0].Expression == GeneExpression.Off || genes[0].Expression == GeneExpression.Recessive)
            && (genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 0;
        }

        //AS AZ c m
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 1;
        }

        //AS AZ c M
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 2;
        }

        //as AZ C m
        if ((genes[0].Expression == GeneExpression.Off || genes[0].Expression == GeneExpression.Recessive)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 3;
        }

        //as AZ C M
        if ((genes[0].Expression == GeneExpression.Off || genes[0].Expression == GeneExpression.Recessive)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 4;
        }

        //AS az C m
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 5;
        }

        //AS AZ C m
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 6;
        }

        //AS AZ C M
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.On)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 7;
        }

        //AS az C M
        if ((genes[0].Expression == GeneExpression.On)
            && (genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[2].Expression == GeneExpression.On)
            && (genes[3].Expression == GeneExpression.On))
        {
            return 8;
        }

        //as az c m
        if ((genes[0].Expression == GeneExpression.Off || genes[0].Expression == GeneExpression.Recessive)
            && (genes[1].Expression == GeneExpression.Off || genes[1].Expression == GeneExpression.Recessive)
            && (genes[2].Expression == GeneExpression.Off || genes[2].Expression == GeneExpression.Recessive)
            && (genes[3].Expression == GeneExpression.Off || genes[3].Expression == GeneExpression.Recessive))
        {
            return 9;
        }

        return null;
    }
}