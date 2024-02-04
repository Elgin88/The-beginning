using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;

namespace TerraUnity.Runtime
{
    public struct CrepuscularParams  
    {
        public bool hasGodRays;
        [XmlIgnore] public Material material;
#if UNITY_EDITOR
        public string MaterialPath { get => AssetDatabase.GetAssetPath(material); set => material = AssetDatabase.LoadAssetAtPath<Material>(value); }
#endif
        public float godRaySamples;
        public float godRayDensity;
        public float godRayWeight;
        public float godRayDecay;
        public float godRayExposure;
    }

    [RequireComponent(typeof(Camera)), ExecuteAlways, ImageEffectAllowedInSceneView]
	public class Crepuscular : MonoBehaviour
	{
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static Crepuscular Script { get => GetScript(); }
		private static Crepuscular _script;
        public static string godRaysMaterialName = "GodRays Material.mat";

        private static Crepuscular GetScript()
        {
            if (_script == null)
            {
                Crepuscular script = TCameraManager.MainCamera.GetComponent<Crepuscular>();

                if (script != null)
                    _script = script;
                else
                    _script = TCameraManager.MainCamera.gameObject.AddComponent<Crepuscular>();
            }
            else
            {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                if (_script.material == null)
                {
                    Material mat = AssetDatabase.LoadAssetAtPath(TTerraWorldManager.WorkDirectoryLocalPath + godRaysMaterialName, typeof(Material)) as Material;

                    if (mat != null)
                        _script.material = mat;
                    else
                    {
                        TResourcesManager.LoadGodRaysResources();
                        _script.material = TResourcesManager.godRaysMaterial;
                    }
                }
#endif
#endif
            }

            return _script;
        }

        [Range(8f, 1024f)] public float godRaySamples = 128f;
        [Range(0.01f, 1f)] public float godRayDensity = 1f;
        [Range(0.01f, 1f)] public float godRayWeight = 0.5f;
        [Range(0.01f, 1f)] public float godRayDecay = 1f;
        [Range(0.5f, 1f)] public float godRayExposure = 1f;

        public Material material;
        private static CrepuscularParams _parameters;

        public static CrepuscularParams GetParams()
        {
            _parameters.hasGodRays = Script.enabled;
            _parameters.material = Script.material;
            _parameters.godRaySamples = Script.godRaySamples;
            _parameters.godRayDensity = Script.godRayDensity;
            _parameters.godRayWeight = Script.godRayWeight;
            _parameters.godRayDecay = Script.godRayDecay;
            _parameters.godRayExposure = Script.godRayExposure;

            return _parameters;
        }

        public static void SetParams(CrepuscularParams parameters)
        {
             _parameters = parameters;

            Script.enabled = parameters.hasGodRays;
            Script.material = parameters.material;
            Script.godRaySamples = parameters.godRaySamples;
            Script.godRayDensity = parameters.godRayDensity;
            Script.godRayWeight = parameters.godRayWeight;
            Script.godRayDecay = parameters.godRayDecay;
            Script.godRayExposure = parameters.godRayExposure;

            Script.UpdateResources();
            Script.Apply();
        }

        private void UpdateResources()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            string worldPath = TTerraWorldManager.WorkDirectoryLocalPath + godRaysMaterialName;

            if (Script.material == null)
            {
                if (!File.Exists(worldPath))
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.godRaysMaterial), worldPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            Script.material = AssetDatabase.LoadAssetAtPath(worldPath, typeof(Material)) as Material;
#endif
#endif
        }

        private void Apply ()
        {
            if (material != null)
            {
                material.SetFloat("_NumSamples", Script.godRaySamples);
                material.SetFloat("_Density", Script.godRayDensity);
                material.SetFloat("_Weight", Script.godRayWeight);
                material.SetFloat("_Decay", Script.godRayDecay);
                material.SetFloat("_Exposure", Script.godRayExposure);
            }
        }

        private void OnValidate()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;

            // Only update if user is changing parameters in inspector and not through code calls
            if (Event.current == null) return;
            Apply();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			//if (material == null) material = TResourcesManager.godRaysMaterial;
			if (material == null || TTerraWorldManager.Sun == null) return;

			material.SetVector("_LightPos", GetComponent<Camera>().WorldToViewportPoint(transform.position - TTerraWorldManager.Sun.transform.forward));
			Graphics.Blit(source, destination, material);
		}
	}
}

