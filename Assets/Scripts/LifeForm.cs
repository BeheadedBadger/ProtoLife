using Assets.Scripts.LifeForms;
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
    DateTime previousUpdate;
    public int health;
    public int age;

    public int feedingDesperation;
    public int procreationDesperation;

    public List<GameObject> LifeStages;
    public Material material;
    public List<GeneInfo> genes = new();
    public List<Renderer> materialsToSet;

    public FungiGenes fungiGenes;
    public PlantGenes plantGenes;

    public int lifespan = new();
    //TODO other genes

    public bool dead = false;

    public void createLifeForm(HexTile parent)
    {
        lifespan = lifeFormObject.lifeSpan;
        if (genes.Count == 0)
        {
            foreach (GeneInfo gene in lifeFormObject.standardGenetics)
            {
                GeneInfo newGene = gameObject.AddComponent(typeof(GeneInfo)) as GeneInfo;
                newGene.Name = gene.Name;
                newGene.Expression = gene.Expression;
                genes.Add(newGene);
            }

            material = lifeFormObject.materials[lifeFormObject.standardColouration];
        }
        bool standardColouration = true;
        foreach (GeneInfo gene in genes)
        {
            if (gene.Expression != lifeFormObject.standardGenetics[genes.IndexOf(gene)].Expression)
            { standardColouration = false; }
        }
        if (standardColouration == false) 
        {
            CheckMaterial();
            CheckEffects();
        }

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

        procreationTime = gameManager.currentDate.AddHours(lifeFormObject.procreationTime);
        coinGenerationTime = gameManager.currentDate.AddHours(lifeFormObject.lifeCoinGeneration);

        if (age > 0)
        {
            int lifespanRemaining = lifespan - age;
            if (lifespanRemaining > 0)
            {
                if (LifeStages.Count > 0)
                { CheckStage(); }
            }
            else Death();
        }

        feedingTime = gameManager.currentDate.AddHours(lifeFormObject.feedingRate);
        previousUpdate = gameManager.currentDate;
    }

    private void CheckEffects()
    {
        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Fungi)
        { }

        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Plant)
        {
            //Short lifespan
            foreach (GeneInfo gene in genes)
            {
                if (gene.Name == "NoChlo" && gene.Expression == GeneExpression.On)
                { 
                    lifespan = (lifeFormObject.lifeSpan / 2);
                }
            }
        }
    }

    private void CheckMaterial()
    {
        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Fungi)
        {
            int? fungiMat = fungiGenes.CheckMat(genes);
            if (fungiMat != null)
            {
                material = lifeFormObject.materials[(int)fungiMat];
            }
            else
            {
                material = lifeFormObject.materials[lifeFormObject.standardColouration];
                genes = lifeFormObject.standardGenetics;
            }
        }
        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Plant)
        {
            int? plantMat = plantGenes.CheckMat(genes);
            if (plantMat != null)
            {
                material = lifeFormObject.materials[(int)plantMat];
            }
            else
            {
                material = lifeFormObject.materials[lifeFormObject.standardColouration];
                genes = lifeFormObject.standardGenetics;
            }
        }
        for (int i = 0; i < materialsToSet.Count; i++)
        { materialsToSet[i].material = material; }
    }

    public void CheckTimeBasedEvents()
    {
        if (!dead)
        {
            if (age > lifespan || health <= 0)
            {
                Death();
                return;
            }

            if (gameManager.currentDate.Hour > previousUpdate.Hour)
            {
                age++;
                if (LifeStages.Count > 0)
                {
                    CheckStage();
                }
            }

            if (procreationTime < gameManager.currentDate)
            {
                AttemptProcreation();
            }

            if (coinGenerationTime < gameManager.currentDate)
            {
                gameManager.LifeCoins += lifeFormObject.lifeCoinGeneration;
                coinGenerationTime = coinGenerationTime.AddHours(1);
            }

            if (feedingTime < gameManager.currentDate)
            {
                if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Animal)
                {
                    //ToDo: Adjust value when we assign sizes to lifeforms
                    Poop(1);
                }
                Feed();
            }

            previousUpdate = gameManager.currentDate;
        }

        if (dead)
        {
            if (gameManager.currentDate.Hour > previousUpdate.Hour)
            {
                age++;
            }

            previousUpdate = gameManager.currentDate;
            if (health == 0 || age >= (lifeFormObject.lifeSpan * 1.1))
            {
                StartCoroutine(CoverOrStationaryDestroyAnimation(this.gameObject, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.3f));
            }
        }
    }

    private void CheckStage()
    {
        float durationOfStage = lifeFormObject.lifeSpan / LifeStages.Count - 1;
        for (int i = 1; i < LifeStages.Count - 1; i++)
        {
            if (age > (durationOfStage * i) && age < (durationOfStage * (i+1))) 
            {
                if (LifeStages[i].activeSelf == false)
                {
                    foreach (GameObject lifestage in LifeStages)
                    { lifestage.SetActive(false); }
                    StartCoroutine(LifeFormGrowAnimation(LifeStages[i - 1], LifeStages[i], 0.2f));
                }
            }
        }
    }

    private void Feed()
    {
        //Attempt to feed on soil
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

        //Attempt to feed on lifeforms it share a tile with, or the ones directly next to it
        List<HexTile> tilesToCheck = new();
        List<LifeForm> potentialPrey = new();

        tilesToCheck.Add(parentHex);
        foreach (HexTile tile in parentHex.neighboringHexTiles)
        {
            tilesToCheck.Add(tile);
        }

        foreach (HexTile tile in tilesToCheck)
        {
            foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
            {
                if (tile.mobile != null && tile.mobile.lifeFormObject.kingdom == foodSource)
                {
                    potentialPrey.Add(parentHex.mobile);
                }

                if (tile.cover != null && tile.cover.lifeFormObject.kingdom == foodSource)
                {
                    potentialPrey.Add(parentHex.cover);
                }

                if (tile.stationary != null && tile.stationary.lifeFormObject.kingdom == foodSource)
                {
                    potentialPrey.Add(parentHex.stationary);
                }
            }
        }

        if (potentialPrey.Count > 0)
        {
            foreach (LifeForm prey in potentialPrey)
            {
                int appeal = PreyAppeal(prey);
                if (appeal > 0)
                {
                    FeedOn(prey);
                    return;
                }
            }
        }

        //If the lifeform can't move, it has exhausted it's options and failed to feed, try again later.
        if (lifeFormObject.lifeType != LifeFormObject.LifeType.Mobile)
        {
            feedingDesperation += 1;
            feedingTime = feedingTime.AddHours(lifeFormObject.feedingRate/feedingDesperation);

            //hunger damage
            if (feedingDesperation > 4)
            {
                health =-1;
            }

            return;
        }

        if (lifeFormObject.lifeType == LifeFormObject.LifeType.Mobile)
        {
            potentialPrey = new();
            List<HexTile> path = new();

            //Find targets
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                {
                    foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                    {
                        if (secondaryNeighbour.mobile != null && secondaryNeighbour.mobile.lifeFormObject.kingdom == foodSource)
                        {
                            potentialPrey.Add(secondaryNeighbour.mobile);
                        }

                        if (secondaryNeighbour.cover != null && secondaryNeighbour.cover.lifeFormObject.kingdom == foodSource)
                        {
                            potentialPrey.Add(secondaryNeighbour.cover);
                        }

                        if (secondaryNeighbour.stationary != null && secondaryNeighbour.stationary.lifeFormObject.kingdom == foodSource)
                        {
                            potentialPrey.Add(secondaryNeighbour.stationary);
                        }
                    }

                    if (potentialPrey.Count > 0)
                    {
                        foreach (LifeForm prey in potentialPrey)
                        {
                            int appeal = PreyAppeal(prey);
                            if (appeal >= 0)
                            {
                                if (neighbour.CheckIfPlacementIsPossible(lifeFormObject))
                                {
                                    path.Add(neighbour);
                                    MoveTo(path, "feeding");
                                    return;
                                }
                            }
                        }
                    }

                    if (lifeFormObject.mobility > 2)
                    {
                        foreach (HexTile tertiaryNeighbour in secondaryNeighbour.neighboringHexTiles)
                        {
                            foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                            {
                                if (tertiaryNeighbour.mobile != null && tertiaryNeighbour.mobile.lifeFormObject.kingdom == foodSource)
                                {
                                    potentialPrey.Add(tertiaryNeighbour.mobile);
                                }

                                if (tertiaryNeighbour.cover != null && tertiaryNeighbour.cover.lifeFormObject.kingdom == foodSource)
                                {
                                    potentialPrey.Add(tertiaryNeighbour.cover);
                                }

                                if (tertiaryNeighbour.stationary != null && tertiaryNeighbour.stationary.lifeFormObject.kingdom == foodSource)
                                {
                                    potentialPrey.Add(tertiaryNeighbour.stationary);
                                }
                            }

                            if (potentialPrey.Count > 0)
                            {
                                foreach (LifeForm prey in potentialPrey)
                                {
                                    int appeal = PreyAppeal(prey);
                                    if (appeal > 0)
                                    {
                                        if (neighbour.CheckIfPlacementIsPossible(lifeFormObject) && secondaryNeighbour.CheckIfPlacementIsPossible(lifeFormObject))
                                        {
                                            path.Add(neighbour);
                                            path.Add(secondaryNeighbour);
                                            MoveTo(path, "feeding");
                                            return;
                                        }
                                    }
                                }
                            }

                            else if (lifeFormObject.mobility > 3)
                            {
                                foreach (HexTile quartaryNeighbour in tertiaryNeighbour.neighboringHexTiles)
                                {
                                    foreach (LifeFormObject.Kingdom foodSource in lifeFormObject.foodSources)
                                    {
                                        if (quartaryNeighbour.mobile != null && quartaryNeighbour.mobile.lifeFormObject.kingdom == foodSource)
                                        {
                                            potentialPrey.Add(quartaryNeighbour.mobile);
                                        }

                                        if (quartaryNeighbour.cover != null && quartaryNeighbour.cover.lifeFormObject.kingdom == foodSource)
                                        {
                                            potentialPrey.Add(quartaryNeighbour.cover);
                                        }

                                        if (quartaryNeighbour.stationary != null && quartaryNeighbour.stationary.lifeFormObject.kingdom == foodSource)
                                        {
                                            potentialPrey.Add(quartaryNeighbour.stationary);
                                        }
                                    }

                                    if (potentialPrey.Count > 0)
                                    {
                                        foreach (LifeForm prey in potentialPrey)
                                        {
                                            int appeal = PreyAppeal(prey);
                                            if (appeal > 0)
                                            {
                                                path.Add(neighbour);
                                                path.Add(secondaryNeighbour);
                                                path.Add(tertiaryNeighbour);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Failed to find a path to a suitable prey item. Move in random direction and try again later.
            HexTile tile = parentHex.neighboringHexTiles[UnityEngine.Random.Range(0, parentHex.neighboringHexTiles.Count)];
            if (tile.CheckIfPlacementIsPossible(lifeFormObject))
            {
                MoveTo(new List<HexTile> { tile }, "feedingFailed");
            }
        }
    }

    private int PreyAppeal(LifeForm prey)
    {
        if (prey != null)
        {
            bool sameSpecies = (prey.lifeFormObject?.title == lifeFormObject.title);
            int sameSpeciesScore = 0;

            if (sameSpecies == true)
            {
                sameSpeciesScore = -5;
            }
            int risk = 0;
            if (((prey.health - lifeFormObject.damage) > 0) && prey.dead == false)
            {
                risk += 1;
            }
            if (((health - prey.lifeFormObject.damage) < 0) && prey.dead == false)
            {
                risk += 2;
            }
            if (((prey.lifeFormObject.mobility) > lifeFormObject.mobility) && prey.dead == false)
            {
                risk += 2;
            }

            return (feedingDesperation + sameSpeciesScore) - risk;
        }
        return -1;
    }

    private void FeedOn(LifeForm prey)
    {
        if (prey == null)
        {
            return;
        }

        feedingDesperation = 0;
        feedingTime = feedingTime.AddDays(lifeFormObject.feedingRate);

        prey.DamageLifeform(lifeFormObject.damage, this);

        //restore predator health
        if (health < lifeFormObject.maxHealth)
        {
            health += 1;

            if (health > lifeFormObject.maxHealth)
            { health = lifeFormObject.maxHealth; }
        }
    }

    private void Death()
    {
        parentHex.soilFill.nutrientScore += 1;
        for (int i = 0; i < LifeStages.Count; i++)
        {
            if (LifeStages[i].activeSelf == true)
            {
                StartCoroutine(LifeFormGrowAnimation(LifeStages[i], LifeStages[^1], 0.2f));
            }
        }

        dead = true;
        
        health = lifeFormObject.maxHealth;
      /*
        LifeStages[LifeStages.Count].SetActive(true);
        if (lifeFormObject.objType == BuildModeObject.ObjectType.Mobile)
        {
            GameObject.Destroy(this.gameObject);
        }
        
        if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover || lifeFormObject.objType == BuildModeObject.ObjectType.Stationary)
        {
            StartCoroutine(CoverOrStationaryDestroyAnimation(this.gameObject, new Vector3(0, 0, 0), new Vector3(1, 1, 1), 0.25f));
        }*/
    }

    private void AttemptProcreation()
    {
        //ToDo: procreative maturity

        //Asexual
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Asexual)
        {
            Procreate(null);
            return;
        }

        //Sexual and immobile
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Sexual)
        {
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                if (neighbour.stationary != null && neighbour.stationary.lifeFormObject.title == lifeFormObject.title)
                {
                    Procreate(neighbour.stationary);
                    return;
                }
                if (neighbour.cover != null && neighbour.cover.lifeFormObject.title == lifeFormObject.title)
                {
                    Procreate(neighbour.cover);
                    return;
                }
                if (neighbour.mobile != null && neighbour.mobile.lifeFormObject.title == lifeFormObject.title)
                {
                    Procreate(neighbour.mobile);
                    return;
                }
                
            }
        }

        //Sexual and mobile
        if (lifeFormObject.procreationType == LifeFormObject.ProcreationType.Sexual && lifeFormObject.objType == LifeFormObject.ObjectType.Mobile)
        {
            List<HexTile> path = new();

            //Find targets
            foreach (HexTile neighbour in parentHex.neighboringHexTiles)
            {
                foreach (HexTile secondaryNeighbour in neighbour.neighboringHexTiles)
                {
                    if (secondaryNeighbour.mobile != null && secondaryNeighbour.mobile.lifeFormObject.title == lifeFormObject.title)
                    {
                        if (neighbour.CheckIfPlacementIsPossible(lifeFormObject))
                        {
                            path.Add(neighbour);
                            MoveTo(path, "procreation");
                            return;
                        }
                    }

                    if (lifeFormObject.mobility > 2)
                    {
                        foreach (HexTile tertiaryNeighbour in secondaryNeighbour.neighboringHexTiles)
                        {
                            if (tertiaryNeighbour.mobile != null && tertiaryNeighbour.mobile.lifeFormObject.title == lifeFormObject.title)
                            {
                                if (neighbour.CheckIfPlacementIsPossible(lifeFormObject) && secondaryNeighbour.CheckIfPlacementIsPossible(lifeFormObject))
                                {
                                    path.Add(neighbour);
                                    path.Add(secondaryNeighbour);
                                    MoveTo(path, "procreation");
                                    return;
                                }
                            }

                            if (lifeFormObject.mobility > 3)
                            {
                                foreach (HexTile quartaryNeighbour in tertiaryNeighbour.neighboringHexTiles)
                                {
                                    if (quartaryNeighbour.mobile != null && quartaryNeighbour.mobile.lifeFormObject.title == lifeFormObject.title)
                                    {
                                        if (neighbour.CheckIfPlacementIsPossible(lifeFormObject) &&
                                        secondaryNeighbour.CheckIfPlacementIsPossible(lifeFormObject) &&
                                        tertiaryNeighbour.CheckIfPlacementIsPossible(lifeFormObject))
                                        {
                                            path.Add(neighbour);
                                            path.Add(secondaryNeighbour);
                                            path.Add(tertiaryNeighbour);
                                            MoveTo(path, "procreation");
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Failed to find a path to a suitable mate. Move in random direction and try again later.
            HexTile tile = parentHex.neighboringHexTiles[UnityEngine.Random.Range(0, parentHex.neighboringHexTiles.Count)];
            if (tile.CheckIfPlacementIsPossible(lifeFormObject))
            {
                MoveTo(new List<HexTile> { tile }, "procreationFailed");
            }
        }
    }

    private void Procreate(LifeForm mate)
    {
        procreationDesperation = 0;
        procreationTime = procreationTime.AddHours(lifeFormObject.procreationTime);

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
                    //Calculate genes
                    List<GeneInfo> childGenes = new();

                    if (mate != null)
                    {
                        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Fungi)
                        {
                            childGenes = fungiGenes.SetGenes(genes, mate.genes);
                        }

                        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Plant)
                        {
                            childGenes = plantGenes.SetGenes(genes, mate.genes);
                        }
                    }

                    if (mate == null)
                    {
                        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Fungi)
                        {
                            childGenes = fungiGenes.SetGenes(genes, null);
                        }

                        if (lifeFormObject.kingdom == LifeFormObject.Kingdom.Plant)
                        {
                            childGenes = plantGenes.SetGenes(genes, null);
                        }
                    }

                    //Place
                    HexTile random = possiblePlacement[UnityEngine.Random.Range(0, possiblePlacement.Count - 1)];
                    LifeForm prefab;

                    if (lifeFormObject.objType == BuildModeObject.ObjectType.Cover && random.cover == null)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.coverContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.cover = prefab;
                        prefab.genes = childGenes;
                        prefab.createLifeForm(random);
                    }
                    else if (lifeFormObject.objType == BuildModeObject.ObjectType.Stationary && random.stationary == null)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.stationaryContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.stationary = prefab;
                        prefab.genes = childGenes;
                        prefab.createLifeForm(random);
                    }
                    else if (lifeFormObject.objType == BuildModeObject.ObjectType.Mobile && random.mobile == null)
                    {
                        var instantiated = Instantiate(lifeFormObject.prefab, random.mobileContainer.transform);
                        prefab = instantiated.GetComponent<LifeForm>();
                        random.mobile = prefab;
                        prefab.genes = childGenes;
                        prefab.createLifeForm(random);
                    }

                    possiblePlacement.Remove(random);
                }
            }
        }
    }

    public void MoveTo(List<HexTile> tiles, string Goal)
    {
        StartCoroutine(MovementToTile(this.gameObject, this.parentHex.transform.position, tiles, 0.5f));
        this.parentHex = tiles[tiles.Count - 1];

        if (Goal == "procreation")
        {
            procreationTime = gameManager.currentDate.AddHours(1);
        }
        if (Goal == "procreationFailed")
        {
            if (procreationDesperation < 5)
            {
                procreationDesperation += 1;
            }
            procreationTime = gameManager.currentDate.AddHours((lifeFormObject.procreationTime / 3) / procreationDesperation);
        }
        if (Goal == "feeding")
        {
            feedingTime = gameManager.currentDate.AddHours(1);
        }
        if (Goal == "feedingFailed")
        {
            feedingDesperation += 1;
            feedingTime = gameManager.currentDate.AddHours(lifeFormObject.feedingRate / feedingDesperation);

            //hunger damage
            if (feedingDesperation > 4)
            {
                health = -1;
            }
        }
    }

    public void DamageLifeform(int damage, LifeForm aggressor)
    {
        health -= damage;
        //ToDo: Indicate that the lifeform is being damaged. Maybe with an animation.

        if (health > 0 && aggressor != null)
        {
            aggressor.DamageLifeform(lifeFormObject.damage, null);
        }
    }

    public void Poop(int nitrateScore) 
    {
        parentHex.soilFill.nutrientScore += nitrateScore;
    }

    IEnumerator LifeFormGrowAnimation(GameObject oldObj, GameObject newObj, float duration)
    {
        float time = 0;
        oldObj.SetActive(true);
        while (time < duration)
        {
            oldObj.transform.localScale = Vector3.Lerp(new(1, 1, 1), new Vector3(0,0,0), time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        oldObj.SetActive(false);

        time = 0;
        newObj.SetActive(true);
        while (time < duration)
        {
            newObj.transform.localScale = Vector3.Lerp(new(0, 0, 0), new(1, 1, 1), time / duration);
            time += Time.deltaTime;
            yield return null;
        }
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
        Vector3 overshootRange = new Vector3(largerScale.x * 1.1f, largerScale.y * 1.1f, largerScale.z * 1.1f);

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

    IEnumerator MovementToTile(GameObject obj, Vector3 startingPosition, List<HexTile> targetPosition, float duration)
    {
        for (int i = 0; i < targetPosition.Count; i++)
        {
            if (targetPosition[i].CheckIfPlacementIsPossible(lifeFormObject))
            {
                this.gameObject.transform.SetParent(targetPosition[targetPosition.Count - 1].mobileContainer.transform, true);

                float time = 0;
                while (time < duration)
                {
                    obj.transform.position = Vector3.Lerp(this.gameObject.transform.position, targetPosition[i].transform.position, time / duration);
                    time += Time.deltaTime;
                    yield return null;
                }
            }

            this.gameObject.transform.SetParent(targetPosition[targetPosition.Count - 1].mobileContainer.transform, false);
        }
    }
}
