#if UNITY_EDITOR
namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public struct TObjectiveLayerData
    {
        public string unityLayerName;
        public int layerMask;
        public float renderDistance;
        public bool hasShadowCasting;
        public bool hasCollider;
        public bool hasPhysics;
    }

    public class TObjectiveLayer : TLayer
    {
        private TObjectiveLayerData _objectiveData;
        public string UnityLayerName { get => _objectiveData.unityLayerName; set => _objectiveData.unityLayerName = value; }
        public int UnityLayerMask { get => _objectiveData.layerMask; set => _objectiveData.layerMask = value; }
        public bool HasShadowCasting { get => _objectiveData.hasShadowCasting; set => _objectiveData.hasShadowCasting = value; }
        public bool HasCollider { get => _objectiveData.hasCollider; set => _objectiveData.hasCollider = value; }
        public bool HasPhysics { get => _objectiveData.hasPhysics; set => _objectiveData.hasPhysics = value; }
    }
#endif
}
#endif

