using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TerraUnity.Runtime
{
    public struct WaterManagerParams
    {
        public WaterQuality waterQuality;
        public Color waterBaseColor;
        public Color waterReflectionColor;
        public bool hasReflection;
        public float reflectionQuality;
        public bool edgeBlend;
        public bool specularLighting;
        public bool gerstnerWaves;
    }

    [ExecuteAlways]
    public class WaterManager : MonoBehaviour
    {
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static WaterManager Script { get => GetScript(); }
        private static WaterManager _script;
        private const string waterMaterial_shader_name = "TerraUnity/Water4";

        private static WaterManager GetScript()
        {
            if (_script == null)
            {
                WaterManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<WaterManager>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<WaterManager>();
            }

            return _script;
        }

        //Resources
        public static Material water4UMaterial = null;

        public WaterQuality waterQuality = WaterQuality.High;
        public Color waterBaseColor = new Color(0.153709f, 0.2681301f, 0.2985074f, 0.8196079f);
        public Color waterReflectionColor = new Color(0.5794164f, 0.6849238f, 0.761194f, 0.4313726f);
        public bool hasReflection = false;
        [Range(0.1f, 1f)] public float reflectionQuality = 0.5f;
        public bool edgeBlend = true;
        public bool specularLighting = true;
        public bool gerstnerWaves = true;

        private bool isInitialized = false;
        private static bool isCoroutineRunning = false;

        private static WaterManagerParams _parameters;

        public static WaterManagerParams GetParams()
        {
            _parameters.waterQuality = Script.waterQuality;
            _parameters.waterBaseColor = Script.waterBaseColor;
            _parameters.waterReflectionColor = Script.waterReflectionColor;
            _parameters.hasReflection = Script.hasReflection;
            _parameters.reflectionQuality = Script.reflectionQuality;
            _parameters.edgeBlend = Script.edgeBlend;
            _parameters.specularLighting = Script.specularLighting;
            _parameters.gerstnerWaves = Script.gerstnerWaves;

            return _parameters;
        }

        public static void SetParams(WaterManagerParams parameters)
        {
             _parameters = parameters;

            Script.waterQuality = parameters.waterQuality;
            Script.waterBaseColor = parameters.waterBaseColor;
            Script.waterReflectionColor = parameters.waterReflectionColor;
            Script.hasReflection = parameters.hasReflection;
            Script.reflectionQuality = parameters.reflectionQuality;
            Script.edgeBlend = parameters.edgeBlend;
            Script.specularLighting = parameters.specularLighting;
            Script.gerstnerWaves = parameters.gerstnerWaves;

            Script.Apply();
        }

        private WaterManager()
        {
            isInitialized = false;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply(false);
        }

        private void Update()
        {
            if (Application.isPlaying) return;

            if (!isInitialized)
            {
                Apply();
                isInitialized = true;
            }
        }

        private void Apply (bool resetReflectionCamera = true)
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null || !TTerraWorldManager.SceneSettingsGO1.activeSelf) return;
            if (SceneManager.GetActiveScene() == null || !SceneManager.GetActiveScene().isLoaded) return;
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] sceneObjects = scene.GetRootGameObjects();
            Component[] components = null;

            for (int i = 0; i < sceneObjects.Length; i++)
            {
                if (sceneObjects[i] == null) continue;

                components = sceneObjects[i].GetComponentsInChildren(typeof(WaterBase), true);
                if (components != null && components.Length > 0)
                {
                    foreach (WaterBase wbComponent in components)
                    {
                        wbComponent.waterQuality = waterQuality;
                        water4UMaterial = wbComponent.sharedMaterial;

                        if (waterQuality == WaterQuality.Low || waterQuality == WaterQuality.Medium)
                            wbComponent.edgeBlend = false;
                        else
                            wbComponent.edgeBlend = edgeBlend;

                        // Update Water Base
                        wbComponent.WaterTileBeingRendered(wbComponent.transform, TCameraManager.CurrentCamera);
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(SpecularLighting), true);
                if (components != null && components.Length > 0)
                    foreach (SpecularLighting slComponent in components)
                        slComponent.enabled = specularLighting;

                components = sceneObjects[i].GetComponentsInChildren(typeof(PlanarReflection), true);
                if (components != null && components.Length > 0)
                {
                    foreach (PlanarReflection prComponent in components)
                    {
                        if (waterQuality == WaterQuality.Low)
                            prComponent.enabled = false;
                        else
                        {
                            prComponent.enabled = hasReflection;
                            prComponent.quality = reflectionQuality;

                            // Update Planar Reflection
                            if (resetReflectionCamera)
                                prComponent.WaterTileBeingRendered(prComponent.transform, TCameraManager.CurrentCamera);
                        }

                        if (resetReflectionCamera)
                            StartCoroutine(DestroyReflectionCamera(prComponent));
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(GerstnerDisplace), true);
                if (components != null && components.Length > 0)
                {
                    foreach (GerstnerDisplace grComponent in components)
                    {
                        if (waterQuality == WaterQuality.Low)
                            grComponent.enabled = false;
                        else
                            grComponent.enabled = gerstnerWaves;
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(GetWaterPlaneHeight), true);
                if (components != null && components.Length > 0)
                {
                    foreach (GetWaterPlaneHeight wpComponent in components)
                    {
                        if (waterQuality == WaterQuality.Low)
                            wpComponent.enabled = false;
                        else
                            wpComponent.enabled = hasReflection;
                    }
                }
            }

            TimeOfDay.UpdateWaterColors();

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private static void ApplyGlobal()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            if (SceneManager.GetActiveScene() == null || !SceneManager.GetActiveScene().isLoaded) return;
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] sceneObjects = scene.GetRootGameObjects();
            Component[] components = null;

            for (int i = 0; i < sceneObjects.Length; i++)
            {
                if (sceneObjects[i] == null) continue;

                components = sceneObjects[i].GetComponentsInChildren(typeof(WaterBase), true);
                if (components != null && components.Length > 0)
                {
                    foreach (WaterBase wbComponent in components)
                    {
                        wbComponent.waterQuality = Script.waterQuality;
                        water4UMaterial = wbComponent.sharedMaterial;

                        if (Script.waterQuality == WaterQuality.Low || Script.waterQuality == WaterQuality.Medium)
                            wbComponent.edgeBlend = false;
                        else
                            wbComponent.edgeBlend = Script.edgeBlend;

                        // Update Water Base
                        wbComponent.WaterTileBeingRendered(wbComponent.transform, TCameraManager.CurrentCamera);
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(SpecularLighting), true);
                if (components != null && components.Length > 0)
                    foreach (SpecularLighting slComponent in components)
                        slComponent.enabled = Script.specularLighting;

                components = sceneObjects[i].GetComponentsInChildren(typeof(PlanarReflection), true);
                if (components != null && components.Length > 0)
                {
                    foreach (PlanarReflection prComponent in components)
                    {
                        if (Script.waterQuality == WaterQuality.Low)
                            prComponent.enabled = false;
                        else
                        {
                            prComponent.enabled = Script.hasReflection;
                            prComponent.quality = Script.reflectionQuality;

                            // Update Planar Reflection
                            prComponent.WaterTileBeingRendered(prComponent.transform, TCameraManager.CurrentCamera);
                        }
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(GerstnerDisplace), true);
                if (components != null && components.Length > 0)
                {
                    foreach (GerstnerDisplace grComponent in components)
                    {
                        if (Script.waterQuality == WaterQuality.Low)
                            grComponent.enabled = false;
                        else
                            grComponent.enabled = Script.gerstnerWaves;
                    }
                }

                components = sceneObjects[i].GetComponentsInChildren(typeof(GetWaterPlaneHeight), true);
                if (components != null && components.Length > 0)
                {
                    foreach (GetWaterPlaneHeight wpComponent in components)
                    {
                        if (Script.waterQuality == WaterQuality.Low)
                            wpComponent.enabled = false;
                        else
                            wpComponent.enabled = Script.hasReflection;
                    }
                }
            }

            TimeOfDay.UpdateWaterColors();

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private IEnumerator DestroyReflectionCamera (PlanarReflection PR)
        {
            if (isCoroutineRunning) yield break;
            isCoroutineRunning = true;
            yield return new WaitForSeconds(0.2f);
            if (PR.reflectionCamera != null) MonoBehaviour.DestroyImmediate(PR.reflectionCamera);
            isCoroutineRunning = false;
        }

        public static void AddWater4MaterialComponents(GameObject waterGameObject)
        {
            if (waterGameObject == null) return;
            if (waterGameObject.transform.parent == null) throw new System.Exception("No parent found for water gameobject : " + waterGameObject.name);
            Material waterMaterial = waterGameObject.GetComponent<Renderer>().sharedMaterial;
            if (waterMaterial.shader.name != waterMaterial_shader_name) return;

            GameObject g = waterGameObject.transform.parent.gameObject;
            string waterTag = "Respawn";

            if (g.GetComponent<WaterBase>() == null)
            {
                WaterBase waterBase = g.AddComponent<WaterBase>();
                waterBase.sharedMaterial = waterMaterial;
                waterBase.BumpFlowSpeed = 0.025f;
                waterBase.FoamFlowSpeed = 0.025f;
            }

            if (g.GetComponent<SpecularLighting>() == null)
            {
                SpecularLighting specularLighting = g.AddComponent<SpecularLighting>();
                specularLighting.specularLight = TTerraWorldManager.Sun.transform;
            }

            if (g.GetComponent<PlanarReflection>() == null)
            {
                PlanarReflection planarReflection = g.AddComponent<PlanarReflection>();
                planarReflection.reflectionMask = ~0;
                planarReflection.reflectSkybox = true;
                planarReflection.clearColor = Color.white;
            }

            if (g.GetComponent<GerstnerDisplace>() == null)
                g.AddComponent<GerstnerDisplace>();

            if (g.GetComponent<GetWaterPlaneHeight>() == null)
            {
                GetWaterPlaneHeight getWaterPlane = g.AddComponent<GetWaterPlaneHeight>();
                getWaterPlane.searchTag = waterTag;
            }

            foreach (Transform t in g.GetComponentsInChildren<Transform>(true))
            {
                if (t != g.transform && t.gameObject.GetComponent<WaterTile>() == null)
                    t.gameObject.AddComponent<WaterTile>();
            }

            ApplyGlobal();
        }
    }
}

