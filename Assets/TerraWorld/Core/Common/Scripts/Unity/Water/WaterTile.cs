using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class WaterTile : MonoBehaviour
    {
        public PlanarReflection reflection;
        public WaterBase waterBase;
        public bool debugBounds = false;

        private Bounds bounds;

        public void Start()
        {
            AcquireComponents();
        }

        void AcquireComponents()
        {
            if (!reflection)
            {
                if (transform.parent)
                    reflection = transform.parent.GetComponent<PlanarReflection>();
                else
                    reflection = transform.GetComponent<PlanarReflection>();
            }

            if (!waterBase)
            {
                if (transform.parent)
                    waterBase = transform.parent.GetComponent<WaterBase>();
                else
                    waterBase = transform.GetComponent<WaterBase>();
            }

            if (debugBounds)
                bounds = GetComponent<Renderer>().bounds;
        }

        public void Update()
        {
            if (Application.isEditor)
                AcquireComponents();
        }

        public void OnWillRenderObject()
        {
            //if (Camera.current == Camera.main || Camera.current.name == "SceneCamera")
            if (reflection && reflection.enabled)
                reflection.WaterTileBeingRendered(transform, TCameraManager.CurrentCamera);

            if (waterBase)
                waterBase.WaterTileBeingRendered(transform, TCameraManager.CurrentCamera);
        }

        private void OnDrawGizmos()
        {
            if (!debugBounds) return;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.DrawWireSphere(bounds.center, 0.3f);
        }
    }
}

