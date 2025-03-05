using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    //SceneManager
    [SerializeField] public GameManager gameManager;

    //Grid
    [SerializeField] GameObject grid;
    Material gridColour;
    Vector3 tileBasicPosition;
    Vector3 tileSelectedPosition;
    bool active;
    float speed = 0.2f;
    bool animationDone = true;

    //Fill
    public Soil soilFill;
    public Vector3 soilBasicPosition;
    public Vector3 soilSelectedPosition;
    bool soilFilled;
    [SerializeField] public GameObject soil;
    [SerializeField] public GameObject water;

    //Neighbors
    public List<GameObject> neighboringTiles;
    public List<HexTile> neighboringHexTiles;

    void OnMouseEnter()
    {
        if (gameManager.BuildMode == true)
        { 
            active = true;
            animationDone = false;
        }
    }

    void OnMouseExit()
    {
        if (gameManager.BuildMode == true)
        {
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
        if (!soilFilled)
        {
            soilFill.changesoilType(SoilObject.SoilType.Ash, this);
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

            if (active && animationDone == false)
            {
                StartCoroutine(LerpPosition(grid, tileBasicPosition, tileSelectedPosition, speed));
                StartCoroutine(LerpPosition(soil, soilBasicPosition, soilSelectedPosition, speed));

                if (!soilFilled)
                {
                    StartCoroutine(LerpColour(gridColour, Color.gray, Color.white, speed));
                }
                if (soilFilled)
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
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (active && !soilFilled)
                {
                    soilFill.changesoilType(gameManager.selectedSoil.type, this);

                    grid.gameObject.SetActive(false);
                    soilFilled = true;
                }
            }
        }
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
