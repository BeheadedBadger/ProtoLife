using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Soil : MonoBehaviour
{
    public GameObject soilModel;
    public SoilObject.SoilType thisSoilType;
    public int waterScore;
    public int nutrientScore;
    public HexTile parentHex;

    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;

    //Replace with better options later:
    //Colours
    List<Color> colours = new List<Color> {
    new(0.23f, 0.23f, 0.23f, 1), //Ash
    new(0.77f, 0.76f, 0.54f, 1), //Sand
    new(0.52f, 0.42f, 0.42f, 1), //Loam
    };

    [SerializeField] List<Material> materials;

    public void ChangeSoilType(SoilObject.SoilType soilType, HexTile hex)
    {
        thisSoilType = soilType;
        parentHex = hex;
        soilBasicPosition = hex.soilBasicPosition;
        soilSelectedPosition = hex.soilSelectedPosition;
        soilModel = hex.soil;
        CalculateWaterScore();

        if (soilType != SoilObject.SoilType.Water)
        {
            StartCoroutine(SoilAnimationLerp(soilModel, new Vector3(1, 1, 1), new Vector3(0.75f, 0, 0.75f), 0.25f));
            int matNumber = (int)soilType;
            soilModel.transform.GetChild(1).GetComponent<Renderer>().material = materials[matNumber];
        }
        else
        {
            //GameObject water = hex.water;
            //water.gameObject.transform.localScale = new Vector3(0.75f, 0, 0.75f);
            //water.gameObject.SetActive(true);
            //StartCoroutine(LerpSize(water, new Vector3(0.75f, 0, 0.75f), new Vector3(1, 1, 1), 0.25f));
            soilModel.gameObject.SetActive(false);
        }
    }

    public void CalculateWaterScore()
    {
        if (this.thisSoilType == SoilObject.SoilType.Ash)
        {
            waterScore = 0;
        }

        if (this.thisSoilType == SoilObject.SoilType.Water)
        {
            waterScore = 10;
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            { 
                if (neighbour.soilFill.waterScore < this.waterScore - 1 && (this.waterScore - 1) > 1)
                {
                    neighbour.soilFill.waterScore = this.waterScore - 1;
                    neighbour.soilFill.CalculateWaterScore();
                }
            }
        }

        else foreach (HexTile neighbour in parentHex.neighboringHexTiles)
        {
            if (neighbour.soilFill.waterScore - 1 > this.waterScore)
            {
                this.waterScore = neighbour.soilFill.waterScore - 1;
            }
            if (neighbour.soilFill.waterScore < this.waterScore - 1 && (this.waterScore - 1) > 1 )
            {
                neighbour.soilFill.waterScore = this.waterScore - 1;
                neighbour.soilFill.CalculateWaterScore();
            }
        }
    }

    IEnumerator SoilAnimationLerp(GameObject obj, Vector3 largerScale, Vector3 smallerScale, float duration)
    {
        float time = 0;
        Vector3 overshootRange = new Vector3(largerScale.x * 1.25f, largerScale.y * 1.25f, largerScale.z * 1.25f);

        //Shrink Old Tile
        while (time < (duration / 3))
        {
            obj.transform.localScale = Vector3.Lerp(largerScale, smallerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //Overshoot first
        time = 0;
        while (time < duration)
        {
            obj.transform.localScale = Vector3.Lerp(smallerScale, overshootRange, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //Set to New Tile Size
        time = 0;
        while (time / 4 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(overshootRange, largerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = largerScale;
    }

    IEnumerator LerpSize(GameObject obj, Vector3 startScale, Vector3 targetScale, float duration)
    {
        float time = 0;
        Vector3 overshootRange = new Vector3(targetScale.x * 1.25f, targetScale.y * 1.25f, targetScale.z * 1.25f);

        //Overshoot first
        while (time < duration)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, overshootRange, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time / 4 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(overshootRange, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    IEnumerator LerpColour(Material obj, Color startValue, Color targetValue, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            obj.color = Color.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        obj.color = targetValue;
    }
}
