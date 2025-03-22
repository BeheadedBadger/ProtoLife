using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeForm : MonoBehaviour
{
    [SerializeField] public HexTile parentHex;
    [SerializeField] LifeFormObject lifeFormObject;
    [SerializeField] public GameManager gameManager;

    DateTime procreationTime;
    DateTime coinGenerationTime;
    DateTime feedingTime;
    DateTime deathTime;

    bool InitializationCompleted = false;
    int health;

    public void createLifeForm(HexTile parent)
    {
        List<int> rotation = new List<int>() { 0, 60, 120, 180, 240, 300 };
        this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, rotation[UnityEngine.Random.Range(0, 5)], 0));
        
        //if cover
        StartCoroutine(CoverInitAnimation(this.gameObject, new Vector3(1, 1, 1), new Vector3(0, 0, 0), 0.5f));

        parentHex = parent;
        gameManager = parentHex.gameManager;
        health = lifeFormObject.maxHealth;

        procreationTime = gameManager.currentDate.AddDays(lifeFormObject.procreationTime);
        coinGenerationTime = gameManager.currentDate.AddDays(lifeFormObject.lifeCoinGeneration);
        deathTime = gameManager.currentDate.AddDays(lifeFormObject.lifeSpan);
        feedingTime = gameManager.currentDate.AddDays(lifeFormObject.feedingRate);

        InitializationCompleted = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (InitializationCompleted)
        {
            CheckTimeBasedEvents();
        }
    }

    private void CheckTimeBasedEvents()
    {
        if (deathTime < gameManager.currentDate || health <= 0)
        {
            Death();
        }

        if (procreationTime < gameManager.currentDate)
        {
            AttemptProcreation();
            procreationTime.AddDays(lifeFormObject.procreationTime);
        }

        if (coinGenerationTime < gameManager.currentDate)
        {
            gameManager.LifeCoins += lifeFormObject.lifeCoinGeneration;
            coinGenerationTime.AddDays(1);
        }

        if (feedingTime < gameManager.currentDate)
        {
            Feed();
        }
    }

    private void Feed()
    {
        if (lifeFormObject.lifeType == LifeFormObject.LifeType.Stationary)
        {
            if (lifeFormObject.foodSources.Contains(LifeFormObject.Kingdom.Nitrates))
            {
                if (parentHex.soilFill.nutrientScore > 0)
                {
                    parentHex.soilFill.nutrientScore -= 1;
                }
            }

            //else if() //CHECK FOR OTHER FOOD SOURCES

            else
            {
                health -= 1;
            }
        }
    }

    private void Death()
    {
        parentHex.soilFill.nutrientScore += 1;
        //If cover
        StartCoroutine(CoverDestroyAnimation(this.gameObject, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.25f));
        
        Destroy(this.gameObject);
    }

    private void AttemptProcreation()
    {
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Asexual)
        {
            List<HexTile> possiblePlacement = new();

            foreach (GameObject neighbor in parentHex.neighboringTiles)
            {
                HexTile tile = neighbor.GetComponent<HexTile>();
                bool canPlace = tile.CheckIfPlacementIsPossible(lifeFormObject);
                if (canPlace) 
                { 
                    possiblePlacement.Add(tile); 
                }
            }

            if (possiblePlacement.Count > 0)
            {
                for (int placed = 0; placed < lifeFormObject.procreationRate; placed++)
                {
                    if (possiblePlacement.Count > 0)
                    {
                        HexTile random = possiblePlacement[UnityEngine.Random.Range(0, possiblePlacement.Count - 1)];
                        GameObject prefab = Instantiate(lifeFormObject.prefab, random.coverContainer.transform);
                        if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover)
                        {
                            random.coverFilled = true;
                        }
                        //Add other fills

                        createLifeForm(random);
                        possiblePlacement.Remove(random);
                    }
                }
            }
        }
    }

    public void DamageLifeform(int damage)
    {
        health -= damage;
    }

    IEnumerator CoverDestroyAnimation(GameObject obj, Vector3 smallerScale, Vector3 largerScale, float duration)
    {
        float time = 0;
        while (time / 20 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(largerScale, smallerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = smallerScale;
    }

    IEnumerator CoverInitAnimation(GameObject obj, Vector3 largerScale, Vector3 smallerScale, float duration)
    {
        float time = 0;
        Vector3 overshootRange = new Vector3(largerScale.x * 1.25f, largerScale.y * 1.25f, largerScale.z * 1.25f);

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
        while (time / 20 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(overshootRange, largerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = largerScale;
    }
}
