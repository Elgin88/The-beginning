#if TERRAWORLD_PRO
#if UNITY_EDITOR

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class TTerraMeshLayer : TMeshLayer
    {
        public TMeshResolution meshResolution;
        public float offsetFalloff = 0.001f;
        public float endCurve = 0.001f;
    }
#endif
}
#endif
#endif

