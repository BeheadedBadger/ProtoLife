using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeForm : MonoBehaviour
{
    [SerializeField] public HexTile parentHex;
    [SerializeField] public LifeFormObject lifeFormObject;
    [SerializeField] public GameManager gameManager;

    DateTime procreationTime;
    DateTime coinGenerationTime;
    DateTime feedingTime;
    DateTime deathTime;

    bool InitializationCompleted;
    DateTime previousUpdate;
    public int health;
    public int age;

    public int feedingDesperation;
    public int procreationDesperation;

    private void Awake()
    {
        InitializationCompleted = false;
    }

    public void createLifeForm(HexTile parent)
    {
        List<int> rotation = new List<int>() { 0, 60, 120, 180, 240, 300 };
        this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, rotation[UnityEngine.Random.Range(0, 5)], 0));

        if ((lifeFormObject.objType == BuildModeObject.ObjectType.Cover || lifeFormObject.objType == BuildModeObject.ObjectType.Stationary) && this.gameObject.activeInHierarchy)
        {
            StartCoroutine(CoverOrStationaryInitAnimation(this.gameObject, new Vector3(1, 1, 1), new Vector3(0, 0, 0), 0.5f));    
        }

        parentHex = parent;
        gameManager = parentHex.gameManager;

        if (health == 0)
        {
            health = lifeFormObject.maxHealth;
        }

        procreationTime = gameManager.currentDate.AddDays(lifeFormObject.procreationTime);
        coinGenerationTime = gameManager.currentDate.AddDays(lifeFormObject.lifeCoinGeneration);

        if (age > 0)
        {
            int lifespanRemaining = lifeFormObject.lifeSpan - age;
            if (lifespanRemaining > 0)
            {
                deathTime = gameManager.currentDate.AddDays(lifespanRemaining);
            }
            else Death();
        }
        else 
        {
            deathTime = gameManager.currentDate.AddDays(lifeFormObject.lifeSpan);
        }

        feedingTime = gameManager.currentDate.AddDays(lifeFormObject.feedingRate);
        previousUpdate = gameManager.currentDate;

        InitializationCompleted = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (InitializationCompleted && gameManager.currentDate > previousUpdate)
        {
            previousUpdate = gameManager.currentDate;
            CheckTimeBasedEvents();
        }
    }

    private void CheckTimeBasedEvents()
    {
        if (gameManager.currentDate.Day > previousUpdate.Day)
        {
            age++;
        }

        if (procreationTime < gameManager.currentDate)
        {
            AttemptProcreation();
            procreationTime = procreationTime.AddDays(lifeFormObject.procreationTime);
        }

        if (deathTime < gameManager.currentDate || health <= 0)
        {
            Death();
        }

        if (coinGenerationTime < gameManager.currentDate)
        {
            gameManager.LifeCoins += lifeFormObject.lifeCoinGeneration;
            int days = (gameManager.currentDate - coinGenerationTime).Days;
            coinGenerationTime = coinGenerationTime.AddDays(1);
        }

        if (feedingTime < gameManager.currentDate)
        {
            Feed();
            feedingTime = feedingTime.AddDays(lifeFormObject.feedingRate);
        }
    }

    private void Feed()
    {
        if (lifeFormObject.lifeType == LifeFormObject.LifeType.Stationary || lifeFormObject.lifeType == LifeFormObject.LifeType.Cover)
        {
            if (lifeFormObject.foodSources.Contains(LifeFormObject.Kingdom.Nitrates))
            {
                if (parentHex.soilFill.nutrientScore > 0)
                {
                    parentHex.soilFill.nutrientScore -= 1;
                    if (health < lifeFormObject.maxHealth)
                    {
                        health += 1;
                    }
                    return;
                }
            }

            else
            {
                foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                {
                    if (parentHex.mobileFilled && parentHex.mobile != null)
                    {
                        LifeForm prey = parentHex.mobile;

                        if (prey.lifeFormObject.kingdom == foodSource)
                        {
                            int appeal = PreyAppeal(prey);
                            if (appeal > 0)
                            {
                                prey.DamageLifeform(lifeFormObject.damage);
                                if (health < lifeFormObject.maxHealth)
                                {
                                    health += 1;
                                }
                                return;
                            }
                        }
                    }
                }

                if (lifeFormObject.objType != BuildModeObject.ObjectType.Stationary)
                {
                    foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                    {
                        if (parentHex.stationaryFilled)
                        {
                            LifeForm prey = parentHex.stationary; 
                            if (prey.lifeFormObject.kingdom == foodSource)
                            {
                                int appeal = PreyAppeal(prey);
                                if (appeal > 0)
                                {
                                    FeedOn(prey);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (lifeFormObject.objType != BuildModeObject.ObjectType.Cover)
                {
                    foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                    {
                        if (parentHex.coverFilled && parentHex.cover != null)
                        {
                            LifeForm prey = parentHex.cover;
                            if (prey.lifeFormObject.kingdom == foodSource)
                            {
                                int appeal = PreyAppeal(prey);
                                if (appeal > 0)
                                {
                                    FeedOn(prey);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            health -= 1;
            feedingDesperation += 1;
        }

        else if (lifeFormObject.lifeType == LifeFormObject.LifeType.Mobile)
        {
            foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
            {
                //Check for food in own tile
                if (parentHex.coverFilled && parentHex.cover != null)
                {
                    LifeForm prey = parentHex.cover;
                    if (prey.lifeFormObject.kingdom == foodSource)
                    {
                        int appeal = PreyAppeal(prey);
                        if (appeal > 0)
                        {
                            FeedOn(prey);
                            return;
                        }
                    }
                }

                if (parentHex.stationaryFilled && parentHex.stationary != null)
                { 
                    LifeForm prey = parentHex.stationary;
                    if (prey.lifeFormObject.kingdom == foodSource)
                    {
                        int appeal = PreyAppeal(prey);
                        if (appeal > 0)
                        {
                            FeedOn(prey);
                            return;
                        }
                    }
                }
            }

            foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
            {
                //Check for food in neighbour tiles if no food has been found in own tile
                foreach (HexTile neighbour in parentHex.neighboringHexTiles)
                {
                    if (neighbour.coverFilled && neighbour.cover != null)
                    {
                        LifeForm prey = neighbour.cover;
                        if (prey.lifeFormObject.kingdom == foodSource)
                        {
                            int appeal = PreyAppeal(prey);
                            if (appeal > 0)
                            {
                                FeedOn(prey);
                                return;
                            }
                        }
                    }

                    if (neighbour.stationaryFilled)
                    {
                        LifeForm prey = neighbour.stationary;
                        if (prey.lifeFormObject.kingdom == foodSource)
                        {
                            int appeal = PreyAppeal(prey);
                            if (appeal > 0)
                            {
                                FeedOn(prey);
                                return;
                            }
                        }
                    }

                    if (neighbour.mobileFilled && neighbour.mobile != null)
                    {
                        LifeForm prey = neighbour.mobile;
                        if (prey.lifeFormObject.kingdom == foodSource)
                        {
                            int appeal = PreyAppeal(prey);
                            if (appeal > 0)
                            {
                                FeedOn(prey);
                                return;
                            }
                        }
                    }
                }
            }

            foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
            {
                //Check even further away and move to other tile if no food was found yet
                List<HexTile> path = new();
                foreach (HexTile neighbour in parentHex.neighboringHexTiles)
                {
                    foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                    {
                        if (secondaryNeighbour.coverFilled && secondaryNeighbour.cover != null)
                        {
                            LifeForm prey = secondaryNeighbour.cover;
                            if (prey.lifeFormObject.kingdom == foodSource)
                            {
                                int appeal = PreyAppeal(prey);
                                if (appeal > 0)
                                {
                                    path.Add(neighbour);
                                    MoveTo(path, "feed");
                                    return;
                                }
                            }
                        }

                        if (secondaryNeighbour.stationaryFilled && secondaryNeighbour.stationary != null)
                        {
                            LifeForm prey = secondaryNeighbour.stationary;
                            if (prey.lifeFormObject.kingdom == foodSource)
                            {
                                int appeal = PreyAppeal(prey);
                                if (appeal > 0)
                                {
                                    path.Add(neighbour);
                                    MoveTo(path, "feed");
                                    return;
                                }
                            }
                        }

                        if (secondaryNeighbour.mobileFilled && secondaryNeighbour.mobile != null)
                        {
                            LifeForm prey = secondaryNeighbour.mobile; 
                            if (prey.lifeFormObject.kingdom == foodSource) 
                            {
                                int appeal = PreyAppeal(prey);
                                if (appeal > 0)
                                {
                                    path.Add(neighbour);
                                    MoveTo(path, "feed");
                                    return;
                                }
                            }
                        }
                    }
                }

                //Check even further away and move to other tiles if no food was found yet
                foreach (HexTile neighbour in parentHex.neighboringHexTiles)
                {
                    foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                    {
                        foreach (HexTile trinaryNeighbour in secondaryNeighbour.neighboringHexTiles)
                        {
                            if (trinaryNeighbour.coverFilled)
                            {
                                LifeForm prey = trinaryNeighbour.cover;
                                if (prey.lifeFormObject.kingdom == foodSource)
                                {
                                    int appeal = PreyAppeal(prey);
                                    if (appeal > 0)
                                    {
                                        path.Add(neighbour);
                                        path.Add(secondaryNeighbour);
                                        MoveTo(path, "feed");
                                        return;
                                    }
                                }
                            }

                            if (trinaryNeighbour.stationaryFilled)
                            {
                                LifeForm prey = trinaryNeighbour.stationary;
                                if (prey.lifeFormObject.kingdom == foodSource)
                                {
                                    int appeal = PreyAppeal(prey);
                                    if (appeal > 0)
                                    {
                                        path.Add(neighbour);
                                        path.Add(secondaryNeighbour);
                                        MoveTo(path, "feed");
                                        return;
                                    }
                                }
                            }

                            if (trinaryNeighbour.mobileFilled && trinaryNeighbour.mobile != null)
                            {
                                LifeForm prey = trinaryNeighbour.mobile;
                                if (prey.lifeFormObject.kingdom == foodSource)
                                {
                                    int appeal = PreyAppeal(prey);
                                    if (appeal > 0)
                                    {
                                        path.Add(neighbour);
                                        path.Add(secondaryNeighbour);
                                        MoveTo(path, "feed"); ;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                //Move in a random direction and try again
                List<HexTile> options = new();
                foreach (HexTile hexTile in parentHex.neighboringHexTiles)
                {
                    if (hexTile.mobileFilled == false)
                    {
                        options.Add(hexTile);
                    }
                }
                if (options.Count >= 1)
                {
                    path = new() { options[UnityEngine.Random.Range(0, options.Count)] };
                    MoveTo(path, "feed");
                }

                health -= 1;
                feedingDesperation += 1;
            }
        }
    }

    private int PreyAppeal(LifeForm prey)
    {
        bool sameSpecies = (prey.lifeFormObject.title == this.lifeFormObject.title);
        int sameSpeciesScore = 0;
        if (sameSpecies == true)
        {
            sameSpeciesScore = -5;
        }
        int risk = 0;
        if (prey.health - lifeFormObject.damage > 0)
        {
            risk += 1;
        }
        if (health - prey.lifeFormObject.damage < 0)
        {
            risk += 2;
        }
        if (prey.lifeFormObject.mobility > lifeFormObject.mobility)
        {
            risk += 2;
        }

        return (feedingDesperation + sameSpeciesScore) - risk;
    }

    private void FeedOn(LifeForm prey)
    {
        prey.DamageLifeform(lifeFormObject.damage);
        if (health < lifeFormObject.maxHealth)
        {
            health += 1;
        }
    }

    private void Death()
    {
        parentHex.soilFill.nutrientScore += 1;

        if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover)
        { 
            parentHex.coverFilled = false;
        }

        if (lifeFormObject.objType == BuildModeObject.ObjectType.Stationary)
        { 
            parentHex.stationaryFilled = false;   
        }

        if (lifeFormObject.objType == BuildModeObject.ObjectType.Mobile)
        { 
            parentHex.mobileFilled = false;
            GameObject.Destroy(this.gameObject);
        }

        if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover || lifeFormObject.objType == BuildModeObject.ObjectType.Stationary)
        {
            StartCoroutine(CoverOrStationaryDestroyAnimation(this.gameObject, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.25f));
        }
    }

    private void AttemptProcreation()
    {
        //Asexual
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Asexual)
        {
            Procreate();
        }

        //Sexual and immobile
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Sexual && (lifeFormObject.objType == LifeFormObject.ObjectType.Stationary || lifeFormObject.objType == BuildModeObject.ObjectType.Cover))
        {
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                if (neighbour.mobileFilled)
                {
                    LifeForm neighbourLifeform = neighbour.mobile;
                    if (neighbourLifeform.lifeFormObject.title == lifeFormObject.title)
                    {
                        Procreate();
                    }
                }
            }
        }
        
        //Sexual and mobile
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Sexual && lifeFormObject.objType == LifeFormObject.ObjectType.Mobile)
        {
            List<HexTile> path = new();

            foreach(HexTile neighbour in parentHex.neighboringHexTiles) 
            {
                if (neighbour.mobileFilled && neighbour.mobile != null)
                {
                    LifeForm neighbourLifeform = neighbour.mobile;
                    if (neighbourLifeform.lifeFormObject.title == lifeFormObject.title)
                    {
                        Procreate();
                        return;
                    }
                }
            }

            //Try one tile further away if no potential mates can be found
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                {
                    if (secondaryNeighbour.mobileFilled && secondaryNeighbour.mobile != null)
                    {
                        LifeForm neighbourLifeform = secondaryNeighbour.mobile;
                        if (neighbourLifeform.lifeFormObject.title == lifeFormObject.title && neighbour.mobileFilled == false)
                        {
                            path.Add(neighbour);
                            MoveTo(path, "procreation");
                            return;
                        }
                    }
                }
            }

            //Try one tile even further away if none can be found even still
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                if (neighbour.mobileFilled == false)
                {
                    foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                    {
                        if (secondaryNeighbour.mobileFilled == false)
                        {
                            foreach (HexTile tritaryNeighbour in secondaryNeighbour.neighboringHexTiles)
                            {
                                if (tritaryNeighbour.mobileFilled && tritaryNeighbour.mobile != null)
                                {
                                    LifeForm neighbourLifeform = tritaryNeighbour.mobile;
                                    if (neighbourLifeform.lifeFormObject.title == lifeFormObject.title)
                                    {
                                        path.Add(neighbour);
                                        path.Add(secondaryNeighbour);
                                        MoveTo(path, "procreation");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Move in a random direction and try again
            List<HexTile> options = new();
            foreach (HexTile hexTile in parentHex.neighboringHexTiles)
            {
                if (hexTile.mobileFilled == false && hexTile.CheckIfPlacementIsPossible(this.lifeFormObject))
                {
                    options.Add(hexTile);
                }
            }
            if (options.Count >= 1)
            {
                path = new() { options[UnityEngine.Random.Range(0, options.Count)] };
                MoveTo(path, "procreation");
            }
        }
    }

    private void Procreate()
    {
        List<HexTile> possiblePlacement = new();

        foreach (HexTile neighbor in parentHex.neighboringHexTiles)
        {
            bool canPlace = neighbor.CheckIfPlacementIsPossible(lifeFormObject);
            if (canPlace)
            {
                possiblePlacement.Add(neighbor);
            }
        }

        if (possiblePlacement.Count > 0)
        {
            for (int placed = 0; placed < lifeFormObject.procreationRate; placed++)
            {
                if (possiblePlacement.Count > 0)
                {
                    HexTile random = possiblePlacement[UnityEngine.Random.Range(0, possiblePlacement.Count - 1)];
                    LifeForm prefab;

                    if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover && random.coverFilled == false)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.coverContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.cover = prefab;
                        random.coverFilled = true;
                        prefab.createLifeForm(random);
                    }
                    else if (lifeFormObject.objType == BuildModeObject.ObjectType.Stationary && random.stationaryFilled == false)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.stationaryContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.stationary = prefab;
                        random.stationaryFilled = true;
                        prefab.createLifeForm(random);
                    }
                    else if (lifeFormObject.objType == BuildModeObject.ObjectType.Mobile && random.mobileFilled == false)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.mobileContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.mobile = prefab;
                        random.mobileFilled = true;
                        prefab.createLifeForm(random);
                    }

                    possiblePlacement.Remove(random);
                }
            }
        }
    }

    private void MoveTo(List<HexTile> tiles, string Goal)
    {
        if (tiles.Count == 1)
        {
            this.gameObject.transform.SetParent(tiles[0].mobileContainer.transform, false);
            tiles[0].mobileFilled = true;
            this.parentHex = tiles[0];
        }
        //Add some sort of delay
        if (tiles.Count == 2)
        {
            this.gameObject.transform.SetParent(tiles[1].mobileContainer.transform, false);
            tiles[1].mobileFilled = true;
            this.parentHex = tiles[1];
        }
        if (Goal == "procreation")
        {
            AttemptProcreation();
        }
        if (Goal == "feed")
        {
            Feed();
        }
    }

    public void DamageLifeform(int damage)
    {
        health -= damage;
        if (health > 0)
        { 
            //aggressor.DamageLifeform(lifeformobject.damage)     Damage aggressor
        }
    }

    public void Poop(int nitrateScore) 
    {
        parentHex.soilFill.nutrientScore += nitrateScore;
    }

    IEnumerator CoverOrStationaryDestroyAnimation(GameObject obj, Vector3 smallerScale, Vector3 largerScale, float duration)
    {
        float time = 0;
        while (time / 20 < duration)
        {
            obj.transform.localScale = Vector3.Lerp(largerScale, smallerScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = smallerScale;
        GameObject.Destroy(this.gameObject);
    }

    IEnumerator CoverOrStationaryInitAnimation(GameObject obj, Vector3 largerScale, Vector3 smallerScale, float duration)
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
