using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeForm : MonoBehaviour
{
    [SerializeField] HexTile parentHex;
    [SerializeField] LifeFormObject lifeFormObject;
    [SerializeField] public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = parentHex.gameManager;
    }

    void createLifeForm(HexTile parent)
    {
        parentHex = parent;
        gameManager = parentHex.gameManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
