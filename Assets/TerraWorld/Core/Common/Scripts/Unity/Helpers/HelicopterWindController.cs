using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class HelicopterWindController : MonoBehaviour
    {
        public float bendingRange = 4f;
        public float bendingMin = 0.1f;
        public float randomRange = 1f;
        [Range(0.001f, 20f)] public float speed = 1f;

        private BendGrassWhenEnabled bendGrassComponent;
        private float range;
        private float min;

        void Start()
        {
            bendGrassComponent = GetComponent<BendGrassWhenEnabled>();
        }

        void Update()
        {
            //range = Mathf.Lerp(range, Random.Range(bendingRange - randomRange, bendingRange + randomRange), Time.deltaTime);
            //min = Mathf.Lerp(range, Random.Range(bendingMin - randomRange, bendingMin + randomRange), Time.deltaTime);
            //if (range < min) range = min + 0.001f;
            //bendGrassComponent.BendIntensity = min + Mathf.PingPong(Time.time * speed, range - min);
            bendGrassComponent.BendIntensity = bendingRange;
        }
    }
}

