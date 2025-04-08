using UnityEngine;

namespace Assets.Scripts.LifeForms
{   
    public enum GeneExpression { On = 0, Recessive = 1, Off = 2 }

    public class GeneInfo : MonoBehaviour
    {
        public string name;
        public GeneExpression expression;
    }
}
