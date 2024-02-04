#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class TObjectScatterLayer : TPointLayer
    {
        public bool bypassLake;
        public bool underLake;
        public bool underLakeMask;
        public bool onLake;
        public List<string> prefabNames;
        public float[,] maskData;
        public float averageDistance;
        public bool checkBoundingBox;
        public bool placeSingleItem;

        public TObjectScatterLayer(bool TreeType) : base()
        {
            if (TreeType) layerType = LayerType.ScatteredTrees; else layerType = LayerType.ScatteredObject;
            prefabNames = new List<string>();
        }
    }
#endif
}
#endif

