using UnityEngine;
using UnityEngine.UI;

public class UI_SoilSlot : MonoBehaviour
{
    [SerializeField] public Image sprite;
    [SerializeField] public Text soilName;
    public SoilObject soilObject;
    [SerializeField] public Toggle toggle;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (toggle.isOn && gameManager != null) 
        {
            gameManager.selectSoil(soilObject);
        }
    }
}
