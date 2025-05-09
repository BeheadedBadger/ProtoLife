using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexTile : MonoBehaviour
{
    //SceneManager
    [SerializeField] public GameManager gameManager;

    //Grid
    [SerializeField] public GameObject grid;
    Color gridInactive = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    Material gridColour;
    public Vector3 tileBasicPosition;
    public Vector3 tileSelectedPosition;
    bool highlighted;
    bool placementPossible;

    float speed = 0.2f;

    //Fill
    [SerializeField] public GameObject obscured;
    [SerializeField] public GameObject revealed;
    public bool isRevealed;

    public Soil soilFill;
    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;
    public bool soilFilled;
    [SerializeField] public GameObject soil;

    [SerializeField] public GameObject coverContainer;
    public LifeForm cover;
    public bool coverFilled;

    [SerializeField] public GameObject stationaryContainer;
    public LifeForm stationary;
    public bool stationaryFilled;

    [SerializeField] public GameObject mobileContainer;
    public LifeForm mobile;
    public bool mobileFilled;

    //Neighbors
    public List<GameObject> neighboringTiles;
    public List<HexTile> neighboringHexTiles;

    public void OnMouseOver()
    {
        gameManager.selectedHex = this.gameObject;
        //Highlight();
    }

    /* public void OnMouseExit()
     {
         if (gameManager.selectedHex == this)
         { gameManager.selectedHex = null; } 

         //DeactivateHighlight();
     }*/

    void Start()
    {
        gridColour = grid.GetComponent<Renderer>().material;
        gridColour.color = gridInactive;

        SetPositions(grid.transform.position, soil.transform.position);
        if (soilFill.thisSoilType != SoilObject.SoilType.Ash)
        { Reveal(); }
    }

    public void SetPositions(Vector3 basicPosition, Vector3 soilPosition)
    {
        tileBasicPosition = basicPosition;
        tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);

        soilBasicPosition = soilPosition;
        soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);

        FindNeighbors(basicPosition);
    }

    public void FindNeighbors(Vector3 tileBasicPosition)
    {
        neighboringHexTiles = new();
        neighboringTiles = new();

        RaycastHit[] neighbourRays = Physics.SphereCastAll(tileBasicPosition, 1, new Vector3(0, 0.1f, 0));
        foreach (RaycastHit neighbourRay in neighbourRays)
        {
            neighbourRays = neighbourRays.Distinct().ToArray();
            if (neighbourRay.transform.position != this.transform.position)
            {
                neighboringTiles.Add(neighbourRay.transform.gameObject);
                if (neighbourRay.transform.gameObject.TryGetComponent<HexTile>(out var neighbourHex))
                {
                    neighboringHexTiles.Add(neighbourHex);
                }
            }
        }
    }

    void Update()
    {
        //Check for mismatches
        if ((isRevealed == false) && (soilFill.thisSoilType != SoilObject.SoilType.Ash))
        {
            isRevealed = true;
        }
        if (isRevealed && obscured.activeSelf)
        {
            if (soilFilled)
            {
                Reveal();
            }
            else
            {
                obscured.SetActive(false);
                revealed.SetActive(true);
            }
        }

        if (gameManager.selectedHex == this.gameObject)
        {
            if (!highlighted || soil.transform.position == soilBasicPosition || grid.transform.position == tileBasicPosition)
            { Highlight(); }
        }

        else
        {
            if (highlighted || soil.transform.position == soilSelectedPosition || grid.transform.position == tileSelectedPosition)
            { 
                DeactivateHighlight();
            }

            gridColour.color = gridInactive;
        }

        //Deactivate grid for view mode
        if (!gameManager.BuildMode && !gameManager.DemolishMode && grid.activeSelf)
        {
            grid.gameObject.SetActive(false);
        }

        else if (gameManager.DemolishMode && isRevealed)
        {
            if (grid.activeSelf == false)
            { grid.gameObject.SetActive(true); }

            //Handle input
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //If the cursor is on the UI, ignore click.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                //Else, destroy all lifeforms on this tile.
                if (highlighted)
                {
                    if (this.cover != null)
                    {
                        Destroy(cover.gameObject);
                    }
                    if (this.stationary != null)
                    {
                        Destroy(stationary.gameObject);
                    }
                    if (this.mobile != null)
                    {
                        Destroy(mobile.gameObject);
                    }
                }
            }
        }

        else if (gameManager.BuildMode && isRevealed)
        {
            if (grid.activeSelf == false)
            { grid.gameObject.SetActive(true); }

            if (highlighted)
            {
                CheckIfPlacementIsPossible();

                if (placementPossible && gridColour.color != gridInactive)
                {
                    StartCoroutine(LerpColour(gridColour, gridColour.color, gridInactive, speed));
                }

                if (!placementPossible && gridColour.color != Color.red)
                {
                    StartCoroutine(LerpColour(gridColour, gridColour.color, Color.red, speed));
                }

                //Handle input
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    //If the cursor is on the UI, don't place object
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    else if (!placementPossible)
                    {
                        SoundManager.PlaySound(soundType.VeryShortFail);
                    }

                    else if ((gameManager.selectedObj != null) && placementPossible)
                    {
                        bool canPay = CanPay();

                        if (canPay == false)
                        {
                            SoundManager.PlaySound(soundType.VeryShortFail);
                            return;
                        }

                        if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Soil && gameManager.selectedSoil != null)
                        {
                            SoundManager.PlaySound(soundType.VeryShortSuccess);
                            gameManager.LifeCoins -= gameManager.selectedObj.cost;

                            SetHeight(gameManager.selectedSoil.soilType);
                            grid.SetActive(false);
                            soilFilled = true;
                            Reveal();
                        }

                        else if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Cover)
                        {
                            SoundManager.PlaySound(soundType.VeryShortSuccess);
                            gameManager.LifeCoins -= gameManager.selectedObj.cost;
                            GameObject coverGO = Instantiate(gameManager.selectedObj.prefab, coverContainer.transform.transform);
                            cover = coverGO.GetComponent<LifeForm>();
                            cover.createLifeForm(this);
                            coverFilled = true;
                        }

                        else if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Stationary)
                        {
                            SoundManager.PlaySound(soundType.VeryShortSuccess);
                            gameManager.LifeCoins -= gameManager.selectedObj.cost;
                            GameObject stationaryGO = Instantiate(gameManager.selectedObj.prefab, stationaryContainer.transform.transform);
                            stationary = stationaryGO.GetComponent<LifeForm>();
                            stationary.createLifeForm(this);
                            stationaryFilled = true;
                        }

                        else if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Mobile)
                        {
                            SoundManager.PlaySound(soundType.VeryShortSuccess);
                            gameManager.LifeCoins -= gameManager.selectedObj.cost;
                            GameObject mobileGO = Instantiate(gameManager.selectedObj.prefab, mobileContainer.transform.transform);
                            mobile = mobileGO.GetComponent<LifeForm>();
                            mobile.createLifeForm(this);
                            mobileFilled = true;
                        }

                        else
                        {
                            SoundManager.PlaySound(soundType.VeryShortFail);
                        }
                    }
                }
            }
        }

        SoilCheck();
    }

    private void SoilCheck()
    {
        if (soilFill.thisSoilType == SoilObject.SoilType.Loam && soilBasicPosition.y != -0f
            || soilFill.thisSoilType == SoilObject.SoilType.Loam && tileBasicPosition.y != 0.75f)
        {
            soilBasicPosition.y = -0f;
            soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);

            tileBasicPosition.y = 0.75f;
            tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);
            grid.transform.position = tileBasicPosition;
        }
        if (soilFill.thisSoilType == SoilObject.SoilType.Clay && soilBasicPosition.y != -0.25f
            || soilFill.thisSoilType == SoilObject.SoilType.Clay && tileBasicPosition.y != 0.5f)
        {
            soilBasicPosition.y = -0.25f;
            soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);

            tileBasicPosition.y = 0.5f;
            tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);
            grid.transform.position = tileBasicPosition;
        }
        if (soilFill.thisSoilType == SoilObject.SoilType.Sand && soilBasicPosition.y != -0.5f
            || soilFill.thisSoilType == SoilObject.SoilType.Sand && tileBasicPosition.y != 0.25f)
        {
            soilBasicPosition.y = -0.5f;
            soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);

            tileBasicPosition.y = 0.25f;
            tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);
            grid.transform.position = tileBasicPosition;
        }
        if (soilFill.thisSoilType == SoilObject.SoilType.Silt && soilBasicPosition.y != -0.75f
            || soilFill.thisSoilType == SoilObject.SoilType.Silt && tileBasicPosition.y != 0f)
        {
            soilBasicPosition.y = -0.75f;
            soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);

            tileBasicPosition.y = 0f;
            tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);
            grid.transform.position = tileBasicPosition;
        }
    }

    public void SetHeight(SoilObject.SoilType soilType)
    {
        tileBasicPosition = grid.transform.position;
        soilBasicPosition = soil.transform.position;

        Vector3 heighten = new();
        if (soilType == SoilObject.SoilType.Loam)
        {
            heighten = new Vector3(0, 1f, 0);
        }
        if (soilType == SoilObject.SoilType.Clay)
        {
            heighten = new Vector3(0, 0.75f, 0);
        }
        if (soilType == SoilObject.SoilType.Sand)
        {
            heighten = new Vector3(0, 0.5f, 0);
        }
        if (soilType == SoilObject.SoilType.Silt)
        {
            heighten = new Vector3(0, 0.25f, 0);
        }

        tileBasicPosition += heighten;
        tileSelectedPosition += heighten;
        soilBasicPosition += heighten;
        soilSelectedPosition += heighten;

        soilFill.ChangeSoilType(soilType);
        StartCoroutine(LerpPosition(soil, soil.transform.position, soilBasicPosition, speed));
    }

    public void LifeFormCheck()
    {
        coverFilled = (cover != null);
        stationaryFilled = (stationary != null);
        mobileFilled = (mobile != null);
    }

    private void Highlight()
    {
        if (grid.transform.position != tileSelectedPosition)
        {
            StartCoroutine(LerpPosition(grid, grid.transform.position, tileSelectedPosition, speed));
        }

        if (soil.transform.position != soilSelectedPosition)
        {
            StartCoroutine(LerpPosition(soil, soil.transform.position, soilSelectedPosition, speed));
        }

        highlighted = true;
    }

    private void DeactivateHighlight()
    {
        gridColour.color = gridInactive;

        if (grid.transform.position != tileBasicPosition)
        {
            StartCoroutine(LerpPosition(grid, grid.transform.position, tileBasicPosition, speed));
        }

        if (soil.transform.position != soilBasicPosition)
        {
            StartCoroutine(LerpPosition(soil, soil.transform.position, soilBasicPosition, speed));
        }

        highlighted = false;
        gridColour.color = gridInactive;
    }

    private void Reveal()
    {
        foreach (HexTile neighbor in neighboringHexTiles)
        {
            if (!neighbor.isRevealed)
            { neighbor.isRevealed = true; }
        }

        obscured.SetActive(false);
        revealed.SetActive(true);
        isRevealed = true;
    }

    private void Hide()
    { }

    private bool CanPay()
    {
        if (gameManager.LifeCoins - gameManager.selectedObj.cost < 0)
        {
            //Handle can't afford in GameManager
            return false;
        }

        return true;
    }

    private void CheckIfPlacementIsPossible()
    {
        if (gameManager.selectedObj != null && gameManager.selectedObj.objType == BuildModeObject.ObjectType.Soil &&
            !soilFilled && this.soilFill.thisSoilType == SoilObject.SoilType.Ash)
        {
            placementPossible = true;
            return;
        }

        else if (gameManager.selectedObj != null && gameManager.selectedObj.soilTypes.Contains(soilFill.thisSoilType))
        {
            if (soilFill.waterScore - 2 <= gameManager.selectedObj.waterNeed && soilFill.waterScore + 2 >= gameManager.selectedObj.waterNeed)
            {
                if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Cover && !coverFilled)
                {
                    placementPossible = true;
                    return;
                }

                if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Stationary && !stationaryFilled)
                {
                    placementPossible = true;
                    return;
                }
            }

            if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Mobile && !mobileFilled)
            {
                placementPossible = true;
                return;
            }

            else
            {
                placementPossible = false;
                return;
            }
        }

        placementPossible = false;
    }

    public bool CheckIfPlacementIsPossible(LifeFormObject lifeform)
    {
        if (lifeform.soilTypes.Contains(soilFill.thisSoilType))
        {
            if (soilFill.waterScore - 2 <= lifeform.waterNeed && soilFill.waterScore + 2 >= lifeform.waterNeed)
            {
                if (lifeform.objType == BuildModeObject.ObjectType.Cover && !coverFilled)
                {
                    return true;
                }
                if (lifeform.objType == BuildModeObject.ObjectType.Stationary && !stationaryFilled)
                {
                    return true;
                }
            }

            if (lifeform.objType == BuildModeObject.ObjectType.Mobile && !mobileFilled)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        else
        {
            return false;
        }
    }

    //Animations
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

    IEnumerator LerpPosition(GameObject obj, Vector3 startPosition, Vector3 targetPosition, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = targetPosition;
    }
}
