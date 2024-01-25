using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class LightManagerFogLights : LightManager<FogLight>
    {
#if UNITY_STANDALONE_WIN
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static LightManagerFogLights Script { get => GetScript(); }
        private static LightManagerFogLights _script;
        private static float _fogLightIntensity = 1;
        private static float _fogLightRange = 1;

        private static LightManagerFogLights GetScript()
        {
            if (_script == null)
            {
                LightManagerFogLights script = TTerraWorldManager.SceneSettingsGO1.GetComponent<LightManagerFogLights>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<LightManagerFogLights>();

                AddFogToAllLights();
            }

            return _script;
        }

        private void Awake()
        {
            AddFogToAllLights();
        }

        private void OnEnable()
        {
            AddFogToAllLights();
        }

        public static void AddFogToAllLights()
        {
            if (!VolumetricFog.Enabled) return;

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.GetComponent<Light>() != null && go.GetComponent<Light>().type != LightType.Directional)
                    {
                        FogLight fogLight = go.GetComponent<FogLight>();

                        if (fogLight != null)
                        {
                            _fogLightIntensity = fogLight.m_IntensityMult;
                            _fogLightRange = fogLight.m_RangeMult;
                            //MonoBehaviour.DestroyImmediate(fogLight);
                        }
                        else
                        {
                            fogLight = go.AddComponent<FogLight>();
                            fogLight.m_IntensityMult = _fogLightIntensity;
                            fogLight.m_RangeMult = _fogLightRange;
                        }
                    }
                }
            }
        }
#endif
    }
}

