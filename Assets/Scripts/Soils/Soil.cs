using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Soil : MonoBehaviour
{
    public GameObject soilModel;
    public SoilObject.SoilType thisSoilType;
    public int waterScore;
    public int nutrientScore = 10;
    public HexTile parentHex;

    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;

    [SerializeField] List<Material> materials;
    [SerializeField] GameObject amberPrefab;
    private GameObject amber;

    public void SetSoilType(SoilObject.SoilType soilType, HexTile hex)
    {
        thisSoilType = soilType;
        parentHex = hex;
        soilBasicPosition = hex.soilBasicPosition;
        soilSelectedPosition = hex.soilSelectedPosition;
        soilModel = hex.soil;
        hex.soilFill = this;

        if (soilType != SoilObject.SoilType.Water)
        {
            StartCoroutine(SoilAnimationLerp(soilModel, new Vector3(1, 1, 1), new Vector3(1, 1, 1), 0.25f));
            int matNumber = (int)soilType;
            soilModel.transform.GetChild(1).GetComponent<Renderer>().material = materials[matNumber];
        }
        else
        {
            StartCoroutine(LerpSize(soilModel, new Vector3(1f, 1f, 1f), new Vector3(0, 0, 0), 0.25f));
        }
    }

    public void ChangeSoilType(SoilObject.SoilType soilType)
    {
        if (soilType != SoilObject.SoilType.Ash && thisSoilType == SoilObject.SoilType.Ash)
        {
            amber = Instantiate(amberPrefab, parentHex.transform);
            parentHex.gameManager.Amber += 1;
        }

        thisSoilType = soilType;
        soilBasicPosition = parentHex.soilBasicPosition;
        soilSelectedPosition = parentHex.soilSelectedPosition;
        soilModel = parentHex.soil;
        parentHex.soilFill = this;

        nutrientScore = 10;
        CalculateWaterScore();

        if (soilType != SoilObject.SoilType.Water)
        {
            StartCoroutine(SoilAnimationLerp(soilModel, new Vector3(1, 1, 1), new Vector3(1, 1, 1), 0.25f));
            int matNumber = (int)soilType;
            soilModel.transform.GetChild(1).GetComponent<Renderer>().material = materials[matNumber];
        }
        else
        {
            StartCoroutine(LerpSize(soilModel, new Vector3(1f, 1f, 1f), new Vector3(0, 0, 0), 0.25f));
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
                    neighbour.soilFill.waterScore = (this.waterScore - 1);
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
      /*  while (time < (duration / 3))
        {
            obj.transform.localScale = Vector3.Lerp(largerScale, smallerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }*/

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
        if (amber != null)
        {
            Task.Delay(2);
            Destroy(amber);
        }
        if (thisSoilType == SoilObject.SoilType.Water)
        {
            soilModel.gameObject.SetActive(false);
        }
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
