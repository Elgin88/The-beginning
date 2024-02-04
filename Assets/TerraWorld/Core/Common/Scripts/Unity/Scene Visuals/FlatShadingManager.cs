using UnityEngine;
using System;
using System.Collections.Generic;
using TerraUnity.Edittime;
using Mewlist.MassiveGrass;

namespace TerraUnity.Runtime
{
    public struct FlatShadingParams
    {
        public bool isFlatShadingTerrain;
        public bool isFlatShadingObjects;
        public bool isFlatShadingClouds;
    }

    [ExecuteAlways]
    public class FlatShadingManager : MonoBehaviour
    {
#if TERRAWORLD_PRO
#if UNITY_EDITOR
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static FlatShadingManager Script { get => GetScript(); }
        private static FlatShadingManager _script;

        private static FlatShadingManager GetScript()
        {
            if (_script == null)
            {
                FlatShadingManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<FlatShadingManager>();

                if (script != null)
                    _script = script;
                else
                {
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<FlatShadingManager>();
#if TERRAWORLD_PRO
#if UNITY_EDITOR
                    _script.Init();
#endif
#endif
                }
            }

            return _script;
        }

        public bool isFlatShadingTerrain = false;
        public bool isFlatShadingObjects = false;
        public bool isFlatShadingClouds = false;

        private static FlatShadingParams _parameters;

        public static FlatShadingParams GetParams()
        {
            _parameters.isFlatShadingTerrain = Script.isFlatShadingTerrain;
            _parameters.isFlatShadingObjects = Script.isFlatShadingObjects;
            _parameters.isFlatShadingClouds = Script.isFlatShadingClouds;

            return _parameters;
        }

        public static void SetParams(FlatShadingParams parameters)
        {
             _parameters = parameters;

            Script.isFlatShadingTerrain = parameters.isFlatShadingTerrain;
            Script.isFlatShadingObjects = parameters.isFlatShadingObjects;
            Script.isFlatShadingClouds = parameters.isFlatShadingClouds;

            Script.Apply();
        }

        public void OnValidate()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;

            //// Only update if user is changing parameters in inspector and not through code calls
            //if (Event.current == null) return;
            
            Apply();
        }

        private void Apply()
        {
            List<Material> materials = GetSceneMaterials.SceneMaterialsList();
            Material mainTerrainMaterial = TerrainRenderingManager.TerrainMaterial;
            Material BGTerrainMaterial = TerrainRenderingManager.TerrainMaterialBG;

            foreach (Material m in materials)
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                // GameObjects
                if (m != mainTerrainMaterial && m != BGTerrainMaterial)
                    if (m.IsKeywordEnabled("_FLATSHADING"))
                        if (m.HasProperty("_FlatShadingState"))
                            m.SetFloat("_FlatShadingState", Convert.ToInt32(isFlatShadingObjects));
            }

            // Terrains
            TerrainRenderingParams terrainRenderingParams = TerrainRenderingManager.GetParams();
            terrainRenderingParams.isFlatShading = isFlatShadingTerrain;
            TerrainRenderingManager.SetParams(terrainRenderingParams);
         
            // Clouds
            CloudsManager.SetFlatShading(isFlatShadingClouds);

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        public void Init ()
        {
            isFlatShadingTerrain = TerrainRenderingManager.GetParams().isFlatShading;
            CloudsManagerParams cloudsManagerParams = CloudsManager.GetParams();
            isFlatShadingClouds = cloudsManagerParams.isFlatShading;
            //isFlatShadingObjects = isFlatShadingTerrain;
        }
#endif
#endif
    }
}

