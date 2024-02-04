#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class TPolygonMeshLayer : TMeshLayer
    {
        public List<T2DObject> MeshArea;

        public TPolygonMeshLayer()
        {
            MeshArea = new List<T2DObject>();
        }
    }
#endif
}
#endif
#endif

