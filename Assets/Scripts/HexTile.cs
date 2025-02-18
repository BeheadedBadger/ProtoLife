using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    //SceneManager
    [SerializeField] GameManager gameManager;

    //Grid
    [SerializeField] GameObject grid;
    Material gridColour;
    Vector3 tileBasicPosition;
    Vector3 tileSelectedPosition;
    bool active;
    float speed = 0.2f;
    bool animationDone = true;

    //Fill
    Material soilColour;
    [SerializeField] GameObject soil;
    [SerializeField] GameObject water;
    Vector3 soilBasicPosition;
    Vector3 soilSelectedPosition;
    bool filled;

    //Colours
    Color DeadSoilColour = new Color(0.23f, 0.23f, 0.23f, 1);
    Color LoamColour = new Color(0.52f, 0.42f, 0.42f, 1);

    //TODO: Finding neigbors
    //Neighbors
    public List<GameObject> neighboringTiles;

    void OnMouseEnter()
    {
        if (gameManager.BuildMode == true)
        {
            //animationDone = true;
            //gridColour.color = Color.grey;
            //grid.transform.position = tileBasicPosition;
            //soil.transform.position = soilBasicPosition;
            active = true;
            animationDone = false;
        }
    }

    void OnMouseExit()
    {
        if (gameManager.BuildMode == true)
        {
            //animationDone = true;
            //gridColour.color = Color.white;
            //grid.transform.position = tileSelectedPosition;
            //soil.transform.position = soilSelectedPosition;
            active = false;
            animationDone = false;
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
        soilColour = soil.GetComponent<Renderer>().material;
        if (!filled)
        {
            soilColour.color = DeadSoilColour;
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

            if (active && animationDone == false)
            {
                StartCoroutine(LerpPosition(grid, tileBasicPosition, tileSelectedPosition, speed));
                StartCoroutine(LerpPosition(soil, soilBasicPosition, soilSelectedPosition, speed));

                if (!filled)
                {
                    StartCoroutine(LerpColour(gridColour, Color.gray, Color.white, speed));
                }
                if (filled)
                {
                    StartCoroutine(LerpColour(gridColour, Color.gray, Color.red, speed));
                }
            }

            if (!active && animationDone == false)
            {
                StartCoroutine(LerpPosition(grid, tileSelectedPosition, tileBasicPosition, speed));
                StartCoroutine(LerpPosition(soil, soilSelectedPosition, soilBasicPosition, speed));
                StartCoroutine(LerpColour(gridColour, Color.white, Color.gray, speed));
            }
            
            //TODO: Move soil to it's own gameobject and script
            //Do pass along that it has been placed on this tile
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (active && !filled)
                {
                    grid.gameObject.SetActive(false);
                    //soil.gameObject.SetActive(true);
                    filled = true;
                    StartCoroutine(SoilAnimationLerp(soil, new Vector3(1,1,1), new Vector3(0.75f, 0, 0.75f), 0.25f));
                    StartCoroutine(LerpColour(soilColour, DeadSoilColour, LoamColour, 0.3f));
                    //deadSoil.gameObject.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (active && !filled)
                {
                    grid.gameObject.SetActive(false);
                    water.gameObject.transform.localScale = new Vector3(0.75f, 0, 0.75f);
                    water.gameObject.SetActive(true);
                    filled = true;
                    StartCoroutine(LerpSize(water, new Vector3(0.75f, 0, 0.75f), new Vector3(1, 1, 1), 0.25f));
                    soil.gameObject.SetActive(false);
                }
            }
            //
        }
    }

    IEnumerator SoilAnimationLerp(GameObject obj, Vector3 largerScale, Vector3 smallerScale, float duration)
    {
        float time = 0;
        Vector3 overshootRange = new Vector3(largerScale.x * 1.25f, largerScale.y * 1.25f, largerScale.z * 1.25f);

        //Shrink Old Tile
        while (time < (duration/3))
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
        animationDone = true;
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
        animationDone = true;
    }
}
