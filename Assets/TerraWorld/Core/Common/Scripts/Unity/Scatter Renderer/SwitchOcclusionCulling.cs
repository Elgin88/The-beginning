#if TERRAWORLD_PRO
using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class SwitchOcclusionCulling : MonoBehaviour
    {
        public bool occlusionCulling = false;
        [Range(0.1f, 3f)] public float frustumMultiplier = 1.1f;
        private List<TScatterParams> GPULayers = null;

        void Start()
        {
            GetGPULayers();
            SwitchOcclusion();
        }

        private void OnValidate()
        {
            GetGPULayers();
            SwitchOcclusion(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                occlusionCulling = !occlusionCulling;
                SwitchOcclusion();
            }
        }

        private void GetGPULayers()
        {
            if (GPULayers != null && GPULayers.Count != 0) return;
            GPULayers = new List<TScatterParams>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go.activeSelf && go.GetComponent<TScatterParams>() != null)
                        GPULayers.Add(go.GetComponent<TScatterParams>());
        }

        private void SwitchOcclusion (bool debugLog = true)
        {
            if (GPULayers == null || GPULayers.Count == 0) return;

            for (int i = 0; i < GPULayers.Count; i++)
            {
                if (GPULayers[i] == null) continue;
                GPULayers[i].occlusionCulling = occlusionCulling;
                GPULayers[i].frustumMultiplier = frustumMultiplier;

#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    GPULayers[i].SetCullingLODEditor(true);
#endif
            }

            if (Application.isPlaying && debugLog)
            {
                if (occlusionCulling) Debug.Log("Occlusion Culling: ON");
                else Debug.Log("Occlusion Culling: OFF");
            }
        }
    }
}
#endif

