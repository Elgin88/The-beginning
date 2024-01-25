using UnityEngine;
using System;
using System.Collections.Generic;
#if TERRAWORLD_PRO
#if UNITY_EDITOR
using TerraUnity.Edittime;
#endif
#endif

namespace TerraUnity.Runtime
{
    public struct SnowManagerParams
    {
        public bool hasSnow;
        [Range(-1000f, 10000f)] public float snowHeight;
        [Range(0f, 10000f)] public float snowFalloff;
        public Color snowColor;
        [HideInInspector] [Range(0f, 1f)] public float snowThickness;
        [HideInInspector] [Range(0f, 1f)] public float snowDamping;
    }

    [ExecuteAlways]
    public class SnowManager : MonoBehaviour
    {
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static SnowManager Script { get => GetScript(); }
        private static SnowManager _script;

        private static SnowManager GetScript()
        {
            if (_script == null)
            {
                SnowManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<SnowManager>();

                if (script != null)
                    _script = script;
                else
                {
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<SnowManager>();
                    _script.Init();
                }
            }

            return _script;
        }

        public bool hasSnow = true;
        [Range(-1000f, 10000f)] public float snowHeight = 5000f;
        [Range(0f, 100f)] public float snowFalloff = 100f;
        public Color snowColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        [HideInInspector] [Range(0f, 1f)] public float snowThickness = 0f;
        [HideInInspector] [Range(0f, 1f)] public float snowDamping = 0f;

        private static SnowManagerParams _parameters;

        public static SnowManagerParams GetParams()
        {
            _parameters.hasSnow = Script.hasSnow;
            _parameters.snowHeight = Script.snowHeight;
            _parameters.snowFalloff = Script.snowFalloff;
            _parameters.snowColor = Script.snowColor;
            _parameters.snowThickness = Script.snowThickness;
            _parameters.snowDamping = Script.snowDamping;

            return _parameters;
        }

        public static void SetParams(SnowManagerParams parameters)
        {
             _parameters = parameters;

            Script.hasSnow = parameters.hasSnow;
            Script.snowHeight = parameters.snowHeight;
            Script.snowFalloff = parameters.snowFalloff;
            Script.snowColor = parameters.snowColor;
            Script.snowThickness = parameters.snowThickness;
            Script.snowDamping = parameters.snowDamping;

            ApplySettings();
        }

        public void Init()
        {
#if TERRAWORLD_PRO
#if UNITY_EDITOR
            ApplySettings();
#endif
#endif
        }

        ////TODO: Needed for builds but should be removed after debugging
        //private void Start()
        //{
        //    if (Application.isPlaying)
        //        ApplySettings();
        //}

        private void OnEnable()
        {
            ApplySettings();
        }

        private void OnValidate()
        {
            // Only update if user is changing parameters in inspector and not through code calls
            //if (Event.current == null) return;
            ApplySettings();
        }

        private static void ApplySettings()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;

#if TERRAWORLD_PRO
#if UNITY_EDITOR
            // Set terrain material's hasSnow state
            TerrainRenderingManager.isProceduralSnow = Script.hasSnow;
            TerrainRenderingManager.isProceduralSnowBG = Script.hasSnow;
            //TerrainRenderingParams terrainRenderingParams = TerrainRenderingManager.GetParams();
            //terrainRenderingParams.snowColorR = Script.snowColor.r;
            //terrainRenderingParams.snowColorG = Script.snowColor.g;
            //terrainRenderingParams.snowColorB = Script.snowColor.b;
            //TerrainRenderingManager.SetParams(terrainRenderingParams);
#endif
#endif

            // Set all materials' snow settings including terrain material(s)
            List<Material> materials = GetSceneMaterials.SceneMaterialsList();

            foreach (Material m in materials)
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_PROCEDURALSNOW"))
                {
                    if (m.HasProperty("_SnowState"))
                    {
                        m.SetFloat("_SnowState", Convert.ToInt32(Script.hasSnow));

                        if (Script.hasSnow)
                        {
                            if (m.HasProperty("_SnowStartHeight")) m.SetFloat("_SnowStartHeight", Script.snowHeight);
                            if (m.HasProperty("_HeightFalloff")) m.SetFloat("_HeightFalloff", Script.snowFalloff);
                            if (m.HasProperty("_SnowColor")) m.SetColor("_SnowColor", Script.snowColor);
                            if (m.HasProperty("_SnowThickness")) m.SetFloat("_SnowThickness", Script.snowThickness);
                            if (m.HasProperty("_SnowDamping")) m.SetFloat("_SnowDamping", Script.snowDamping);
                        }
                    }
                }
            }

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }
    }
}

