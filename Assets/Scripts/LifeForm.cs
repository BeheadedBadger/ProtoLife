using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeForm : MonoBehaviour
{
    [SerializeField] public HexTile parentHex;
    [SerializeField] LifeFormObject lifeFormObject;
    [SerializeField] public GameManager gameManager;

    public void createLifeForm(HexTile parent)
    {
        parentHex = parent;
        gameManager = parentHex.gameManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
