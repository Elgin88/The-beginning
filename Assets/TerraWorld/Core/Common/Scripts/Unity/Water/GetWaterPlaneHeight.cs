using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GetWaterPlaneHeight : MonoBehaviour
    {
        public string searchTag = "Respawn";
        public float adoptionSpeed = 20f;
        public float yOffset = 0f;
        public float rejectionUnits = 2;
        private PlanarReflection planarReflection;
        private Vector3 cachedCameraPosition = Vector3.zero;
        private Vector3 cachedClosestPointPosition = Vector3.zero;
        private int layerMask;
        private RaycastHit hit;
        private Transform parent;

        private void Start()
        {
            Initialize();
        }

        void LateUpdate()
        {
            GetDistances();
        }

        private void Initialize()
        {
            if (planarReflection == null) planarReflection = GetComponent<PlanarReflection>();
            layerMask = 1 << LayerMask.NameToLayer("Water");
        }

        private void GetDistances()
        {
            if (!Application.isPlaying) Initialize();
            if (planarReflection == null) return;
            if (planarReflection.m_ReflectionCamera == null) return;
            if (cachedCameraPosition == planarReflection.m_ReflectionCamera.transform.position) return;

            cachedCameraPosition = planarReflection.m_ReflectionCamera.transform.position;

            if (rejectionUnits > 0)
            {
                float dist = (cachedClosestPointPosition - TCameraManager.CurrentCamera.transform.position).sqrMagnitude;
                if (dist <= rejectionUnits * rejectionUnits) return;
            }

            Vector3 origin = TCameraManager.CurrentCamera.transform.position + (Vector3.up * 10000) + (TCameraManager.CurrentCamera.transform.forward * 20);

            if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                float offset = 0;
                parent = transform;

                while (parent != null)
                {
                    offset += parent.transform.position.y;
                    parent = parent.transform.parent;
                }

                float waterPlaneHeight = hit.point.y - offset;
                planarReflection.clipPlaneOffset = Mathf.Lerp(planarReflection.clipPlaneOffset, waterPlaneHeight + yOffset, Time.deltaTime * adoptionSpeed);
            }

            if (rejectionUnits > 0) cachedClosestPointPosition = TCameraManager.CurrentCamera.transform.position;
        }
    }
}

