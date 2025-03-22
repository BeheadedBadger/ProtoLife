using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] GameObject rig;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float movementSpeed;
    [SerializeField] float movementTime;
    [SerializeField] Vector3 zoomAmount;

    Vector3 newPosition;
    Vector3 newZoom;

    void Start()
    {
        newPosition = transform.position;
        newZoom = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleInput();
        HandleBoundries();
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            newPosition += (transform.right * -movementSpeed);
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            newPosition += (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            newPosition += (transform.forward * movementSpeed);
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            newPosition += (transform.forward * -movementSpeed);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            newZoom += zoomAmount;
        }
        else if (Input.GetKey(KeyCode.KeypadPlus))
        {
            newZoom += (zoomAmount / 100);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            newZoom -= zoomAmount;
        }
        else if (Input.GetKey(KeyCode.KeypadMinus))
        {
            newZoom -= (zoomAmount / 100);
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    void HandleBoundries()
    {
        if (rig.transform.position.x < -120) 
        {
            rig.transform.position = new Vector3(-120, rig.transform.position.y, rig.transform.position.z);
            newPosition = rig.transform.position;
        }
        if (rig.transform.position.x > -20)
        {
            rig.transform.position = new Vector3(-20, rig.transform.position.y, rig.transform.position.z);
            newPosition = rig.transform.position;
        }

        if (rig.transform.position.z < -100) 
        {
            rig.transform.position = new Vector3(rig.transform.position.x, rig.transform.position.y, -100);
            newPosition = rig.transform.position;
        }
        if (rig.transform.position.z > -10)
        {
            rig.transform.position = new Vector3(rig.transform.position.x, rig.transform.position.y, -10);
            newPosition = rig.transform.position;
        }

        if (cameraTransform.localPosition.z < -100)
        { 
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, 100, -100);
            newZoom = cameraTransform.localPosition;
        }
        if (cameraTransform.localPosition.z > 60)
        {
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, -60, 60);
            newZoom = cameraTransform.localPosition;
        }
    }
}
