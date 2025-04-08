using Assets.Scripts.LifeForms;
using System.Collections.Generic;
using UnityEngine;

public class FungiGenes : MonoBehaviour
{
    //ybrm, YBrm, YbRm, yBRm, YBRm, YBrM, YbRM, yBRM
    public enum GeneName { Melanin = 0, Carotenoids = 1, Azulenes = 2, Astaxanthin = 3 }

    public List<GeneInfo> GeneticInfo;
    public int mutationChance = 3;


    public List<GeneInfo> SetGenes(List<GeneInfo> parent1, List<GeneInfo> parent2)
    {
        //Asexual reproduction. Only variants through mutation.
        if (parent2 == null)
        {
            GeneticInfo = CalculateAsexualGenes(parent1);
        }

        else 
        {
            GeneticInfo = CalculateSexualGenes(parent1, parent2);
        }

        return GeneticInfo;
    }

    private List<GeneInfo> CalculateSexualGenes(List<GeneInfo> parent1, List<GeneInfo> parent2)
    {
        bool mutationOccurred = false;
        int percentageRoll = Random.Range(0, 100);

        for (int i = 0; i < parent1.Count; i++)
        {
            GeneticInfo[i].name = parent1[i].name;
            percentageRoll = Random.Range(1, 100);
            if (percentageRoll <= mutationChance && !mutationOccurred)
            {
                GeneticInfo[i].expression = (GeneExpression)Random.Range(0, 2);
                mutationOccurred = true;
            }
            else
            {
                if (parent1[i].expression == GeneExpression.On && parent2[i].expression == GeneExpression.On)
                {
                    GeneticInfo[i].expression = GeneExpression.On;
                }

                if (parent1[i].expression == GeneExpression.Off && parent2[i].expression == GeneExpression.On ||
                   parent1[i].expression == GeneExpression.On && parent2[i].expression == GeneExpression.Off)
                {
                    GeneticInfo[i].expression = GeneExpression.Recessive;
                }

                if (parent1[i].expression == GeneExpression.On && parent2[i].expression == GeneExpression.Recessive ||
                    parent1[i].expression == GeneExpression.Recessive && parent2[i].expression == GeneExpression.On)
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 50)
                    {
                        GeneticInfo[i].expression = GeneExpression.On;
                    }
                    else
                    {
                        GeneticInfo[i].expression = GeneExpression.Recessive;
                    }
                }

                if (parent1[i].expression == GeneExpression.Recessive && parent2[i].expression == GeneExpression.Recessive)
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 66)
                    {
                        GeneticInfo[i].expression = GeneExpression.On;
                    }
                    else if (percentageRoll >= 33 && percentageRoll < 66)
                    {
                        GeneticInfo[i].expression = GeneExpression.Recessive;
                    }
                    else
                    {
                        GeneticInfo[i].expression = GeneExpression.Off;
                    }
                }

                if (parent1[i].expression == GeneExpression.Off && parent2[i].expression == GeneExpression.Recessive ||
                    parent1[i].expression == GeneExpression.Recessive && parent2[i].expression == GeneExpression.Off)
                {
                    percentageRoll = Random.Range(1, 100);
                    if (percentageRoll >= 50)
                    {
                        GeneticInfo[i].expression = GeneExpression.Recessive;
                    }
                    else
                    {
                        GeneticInfo[i].expression = GeneExpression.Off;
                    }
                }

                if (parent1[i].expression == GeneExpression.Off && parent2[i].expression == GeneExpression.Off)
                {
                    GeneticInfo[i].expression = GeneExpression.Off;
                }
            }
        }

        //If two colour genes are not producing pigment, colour pigment production is shut down.
        if (GeneticInfo[1].expression == GeneExpression.Off && GeneticInfo[2].expression == GeneExpression.Off
            || GeneticInfo[1].expression == GeneExpression.Off && GeneticInfo[3].expression == GeneExpression.Off
            || GeneticInfo[2].expression == GeneExpression.Off && GeneticInfo[3].expression == GeneExpression.Off)
        {
            GeneticInfo[1].expression = GeneExpression.Off;
            GeneticInfo[2].expression = GeneExpression.Off;
            GeneticInfo[3].expression = GeneExpression.Off;
        }
        
        return GeneticInfo;
    }

    private List<GeneInfo> CalculateAsexualGenes(List<GeneInfo> parent1)
    {
        bool mutationOccurred = false;
        int colourGenes = 0;
        int percentageRoll = Random.Range(0, 100);

        for (int i = 0; i < parent1.Count; i++)
        {
            GeneticInfo[i].name = parent1[i].name;
            percentageRoll = Random.Range(0, 100);
            if (percentageRoll >= 97 && !mutationOccurred)
            {
                GeneticInfo[i].expression = (GeneExpression)Random.Range(0, 2);
                mutationOccurred = true;
            }
            else
            {
                GeneticInfo[i].expression = parent1[i].expression;
            }
        }

        //If two colour genes are not producing pigment, colour pigment production is shut down.
        if (GeneticInfo[1].expression == GeneExpression.Off && GeneticInfo[2].expression == GeneExpression.Off
            || GeneticInfo[1].expression == GeneExpression.Off && GeneticInfo[3].expression == GeneExpression.Off
            || GeneticInfo[2].expression == GeneExpression.Off && GeneticInfo[3].expression == GeneExpression.Off)
        {
            GeneticInfo[1].expression = GeneExpression.Off;
            GeneticInfo[2].expression = GeneExpression.Off;
            GeneticInfo[3].expression = GeneExpression.Off;
        }

        return GeneticInfo;
    }

    public int CheckMat(List<GeneInfo> genes)
    {
        int materialNr = 0;

        //as az c M
        if ((genes[0].expression == GeneExpression.Off || genes[0].expression == GeneExpression.Recessive)
            && (genes[1].expression == GeneExpression.Off || genes[1].expression == GeneExpression.Recessive)
            && (genes[2].expression == GeneExpression.Off || genes[2].expression == GeneExpression.Recessive)
            && (genes[3].expression == GeneExpression.On))
        {
            materialNr = 0;
        }

        //AS AZ c m
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.Off || genes[2].expression == GeneExpression.Recessive)
            && (genes[3].expression == GeneExpression.Off || genes[3].expression == GeneExpression.Recessive))
        {
            materialNr = 1;
        }

        //AS AZ c M
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.Off || genes[2].expression == GeneExpression.Recessive)
            && (genes[3].expression == GeneExpression.On))
        {
            materialNr = 2;
        }

        //as AZ C m
        if ((genes[0].expression == GeneExpression.Off || genes[0].expression == GeneExpression.Recessive)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.Off || genes[3].expression == GeneExpression.Recessive))
        {
            materialNr = 3;
        }

        //as AZ C M
        if ((genes[0].expression == GeneExpression.Off || genes[0].expression == GeneExpression.Recessive)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.On))
        {
            materialNr = 4;
        }

        //AS az C m
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.Off || genes[1].expression == GeneExpression.Recessive)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.Off || genes[3].expression == GeneExpression.Recessive))
        {
            materialNr = 5;
        }

        //AS AZ C m
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.Off || genes[3].expression == GeneExpression.Recessive))
        {
            materialNr = 6;
        }

        //AS AZ C M
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.On)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.On))
        {
            materialNr = 7;
        }

        //AS az C M
        if ((genes[0].expression == GeneExpression.On)
            && (genes[1].expression == GeneExpression.Off || genes[1].expression == GeneExpression.Recessive)
            && (genes[2].expression == GeneExpression.On)
            && (genes[3].expression == GeneExpression.On))
        {
            materialNr = 8;
        }

        //as az c m
        if ((genes[0].expression == GeneExpression.Off || genes[0].expression == GeneExpression.Recessive)
            && (genes[1].expression == GeneExpression.Off || genes[1].expression == GeneExpression.Recessive)
            && (genes[2].expression == GeneExpression.Off || genes[2].expression == GeneExpression.Recessive)
            && (genes[3].expression == GeneExpression.Off || genes[3].expression == GeneExpression.Recessive))
        {
            materialNr = 9;
        }

        return materialNr;
    }
}