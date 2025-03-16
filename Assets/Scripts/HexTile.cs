using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexTile : MonoBehaviour
{
    //SceneManager
    [SerializeField] public GameManager gameManager;

    //Grid
    [SerializeField] GameObject grid;
    Material gridColour;
    Vector3 tileBasicPosition;
    Vector3 tileSelectedPosition;
    bool highlighted;
    bool placementPossible;

    float speed = 0.2f;

    //Fill
    public Soil soilFill;
    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;
    bool soilFilled;
    [SerializeField] public GameObject soil;
    [SerializeField] public GameObject water;

    bool coverFilled;

    //Neighbors
    public List<GameObject> neighboringTiles;
    public List<HexTile> neighboringHexTiles;

    void OnMouseEnter()
    {
        if (gameManager.BuildMode == true)
        {
            highlighted = true;
        }
    }

    void OnMouseExit()
    {
        if (gameManager.BuildMode == true)
        {
            highlighted = false;
        }
    }

    void Start()
    {
        gridColour = grid.GetComponent<Renderer>().material;
        gridColour.color = Color.grey;

        tileBasicPosition = grid.transform.position;
        tileSelectedPosition = new Vector3(tileBasicPosition.x, tileBasicPosition.y + 0.4f, tileBasicPosition.z);

        soilBasicPosition = soil.transform.position;
        soilSelectedPosition = new Vector3(soilBasicPosition.x, soilBasicPosition.y - 0.2f, soilBasicPosition.z);
        if (!soilFilled)
        {
            soilFill.ChangeSoilType(SoilObject.SoilType.Ash, this);
        }

        FindNeighbors(tileBasicPosition);
    }

    private void FindNeighbors(Vector3 tileBasicPosition)
    {
        RaycastHit[] neighbourRays = Physics.SphereCastAll(tileBasicPosition, 1, new Vector3(0, 0.1f, 0));
        foreach (RaycastHit neighbourRay in neighbourRays)
        {
            if (neighbourRay.transform.position != tileBasicPosition)
            {
                neighboringTiles.Add(neighbourRay.transform.gameObject);
                neighboringHexTiles.Add(neighbourRay.transform.gameObject.GetComponent<HexTile>());
            }
        }
    }

    void Update()
    {
        if (!gameManager.BuildMode)
        {
            grid.gameObject.SetActive(false);
        }

        if (gameManager.BuildMode)
        {
            grid.gameObject.SetActive(true);

            if (highlighted)
            {
                if (grid.transform.position != tileSelectedPosition)
                {
                    StartCoroutine(LerpPosition(grid, grid.transform.position, tileSelectedPosition, speed));
                }

                if (soil.transform.position != soilSelectedPosition)
                {
                    StartCoroutine(LerpPosition(soil, soil.transform.position, soilSelectedPosition, speed));
                }

                CheckIfPlacementIsPossible();

                if (placementPossible && gridColour.color != Color.grey)
                {
                    StartCoroutine(LerpColour(gridColour, gridColour.color, Color.grey, speed));
                }

                if (!placementPossible && gridColour.color != Color.red)
                {
                    StartCoroutine(LerpColour(gridColour, gridColour.color, Color.red, speed));
                }
            }

            if (!highlighted)
            {
                if (grid.transform.position != tileBasicPosition)
                {
                    StartCoroutine(LerpPosition(grid, grid.transform.position, tileBasicPosition, speed));
                }

                if (soil.transform.position != soilBasicPosition)
                {
                    StartCoroutine(LerpPosition(soil, soil.transform.position, soilBasicPosition, speed));
                }

                if (gridColour.color != Color.grey)
                {
                    StartCoroutine(LerpColour(gridColour, gridColour.color, Color.grey, speed));
                }
            }

            //Handle input
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //If the cursor is on the UI, don't place object
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (highlighted && placementPossible && gameManager.selectedObj.objType == BuildModeObject.ObjectType.Soil)
                {
                    soilFill.ChangeSoilType(gameManager.selectedSoil.soilType, this);

                    grid.SetActive(false);
                    soilFilled = true;
                }

                if (highlighted && placementPossible && gameManager.selectedObj.objType == BuildModeObject.ObjectType.Cover)
                {
                    GameObject cover = Instantiate(gameManager.selectedObj.prefab, transform);
                    coverFilled = true;
                }
            }
        }
    }

    private void CheckIfPlacementIsPossible()
    {
        if (gameManager.selectedObj.objType == BuildModeObject.ObjectType.Soil &&
            !soilFilled && this.soilFill.thisSoilType == SoilObject.SoilType.Ash)
        {
            placementPossible = true;
        }

        else if (gameManager.selectedObj.soilTypes.Contains(soilFill.thisSoilType) &&
            !coverFilled && soilFill.waterScore - 2 <= gameManager.selectedObj.waterNeed &&
            soilFill.waterScore + 2 >= gameManager.selectedObj.waterNeed)
        {
            placementPossible = true;
        }

        else 
        { 
            placementPossible = false; 
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
        while (time/4 < duration)
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
