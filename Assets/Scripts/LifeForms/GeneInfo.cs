using UnityEngine;

namespace Assets.Scripts.LifeForms
{   
    public enum GeneExpression { On = 0, Recessive = 1, Off = 2 }

    public class GeneInfo : MonoBehaviour
    {
        public string Name;
        public GeneExpression Expression;

        public GeneInfo Gene(string name, GeneExpression expression)
        {
            GeneInfo gene = new();
            gene.Name = name;
            gene.Expression = expression;

            return gene;
        }
    }
}
