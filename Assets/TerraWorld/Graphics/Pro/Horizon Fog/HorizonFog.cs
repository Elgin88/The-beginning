using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;

namespace TerraUnity.Runtime
{
    public struct HorizonFogParams
    {
        // Generic
        public bool hasHorizonFog;
        public bool autoColor;
        public float coneHeight;
        public float coneAngle;
        public HorizonFog.HorizonBlendMode horizonBlendMode;
        [XmlIgnore] public Material material;

#if UNITY_EDITOR
        public string MaterialPath { get => AssetDatabase.GetAssetPath(material); set => material = AssetDatabase.LoadAssetAtPath<Material>(value); }
#endif

        // Physcially based auto coloring
        public float visibility;
        public float startOffset;
        public float endOffset;
        public float strength;

        // User-defined coloring
        public Color horizonFogVolumeColor;
        public Color horizonFogVolumeColorAuto;
        public float visibilityUser;
        public float startOffsetUser;
        public float endOffsetUser;
        public float strengthUser;
    }


    [ExecuteAlways]
    public class HorizonFog : VolumetricObjectBase
    {
        public static bool Enabled { get => Script.gameObject.activeSelf; set => Script.gameObject.SetActive(value); }
        private static HorizonFog Script { get => GetScript(); }
        private static HorizonFog _script;
        public static string horizonMaterialName = "Horizon Material.mat";

        private static HorizonFog GetScript()
        {
            if (_script == null)
            {
                foreach (Transform t in TTerraWorldManager.SceneSettingsGO1.GetComponentsInChildren(typeof(Transform), true))
                {
                    HorizonFog script = t.GetComponent<HorizonFog>();

                    if (script != null)
                        _script = script;
                }

                if (_script == null)
                {
                    horizonFogGameObject = new GameObject("Horizon Fog");
                    horizonFogGameObject.transform.parent = TTerraWorldManager.SceneSettingsGO1.transform;
                    horizonFogGameObject.transform.position = new Vector3(0, -90000f, 0);
                    horizonFogGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
                    horizonFogGameObject.AddComponent<MeshFilter>();
                    horizonFogGameObject.AddComponent<MeshRenderer>();
                    horizonFogGameObject.AddComponent<CameraXZ>();
                    _script = horizonFogGameObject.AddComponent<HorizonFog>();
                    _script.texture = null;
                    _script.textureScale = 1;
                    _script.textureMovement = Vector3.zero;
                    _script.coneHeight = 100000f;
                    _script.coneAngle = 60f;
                }
            }

#if UNITY_EDITOR
#if TERRAWORLD_PRO
            if (_script.volumetricMaterial == null)
            {
                Material mat = AssetDatabase.LoadAssetAtPath(TTerraWorldManager.WorkDirectoryLocalPath + horizonMaterialName, typeof(Material)) as Material;

                if (mat != null)
                    _script.volumetricMaterial = mat;
                else
                {
                    TResourcesManager.LoadHorizonFogResources();
                    _script.volumetricMaterial = TResourcesManager.volumetricHorizonMaterial;
                }
            }
#endif
#endif

            return _script;
        }

        public enum HorizonBlendMode
        {
            Normal,
            Underlay
        }
        public HorizonBlendMode horizonBlendMode = HorizonBlendMode.Normal;

        //public Material material = null;
        public bool autoColor = true;
        public float coneHeight = 100000f;
        public float coneAngle = 60f;
        public float startOffset = 0f;
        public float startOffsetUser = 0f;
        public float endOffset = 53500f;
        public float endOffsetUser = 80000f;

        public static GameObject horizonFogGameObject;

        private static HorizonFogParams _parameters;

        public static HorizonFogParams GetParams()
        {
            _parameters.hasHorizonFog = Enabled;
            _parameters.autoColor = Script.autoColor;
            _parameters.material = Script.volumetricMaterial;
            _parameters.coneHeight = Script.coneHeight;
            _parameters.coneAngle = Script.coneAngle;
            _parameters.horizonBlendMode = Script.horizonBlendMode;

            // Physcially based auto coloring
            _parameters.visibility = Script.visibility;
            _parameters.startOffset = Script.startOffset;
            _parameters.endOffset = Script.endOffset;
            _parameters.strength = Script.strength;

            // User-defined coloring
            _parameters.horizonFogVolumeColor = Script.horizonFogVolumeColor;
            _parameters.horizonFogVolumeColorAuto = Script.horizonFogVolumeColorAuto;
            _parameters.visibilityUser = Script.visibilityUser;
            _parameters.startOffsetUser = Script.startOffsetUser;
            _parameters.endOffsetUser = Script.endOffsetUser;
            _parameters.strengthUser = Script.strengthUser;

            return _parameters;
        }

        public static void SetParams(HorizonFogParams parameters, bool updateAtmosphere = true)
        {
            _parameters = parameters;

            Enabled = parameters.hasHorizonFog;
            Script.autoColor = parameters.autoColor;
            Script.volumetricMaterial = parameters.material;
            Script.coneHeight = parameters.coneHeight;
            Script.coneAngle = parameters.coneAngle;
            Script.horizonBlendMode = parameters.horizonBlendMode;

            // Physcially based auto coloring
            Script.visibility = parameters.visibility;
            Script.startOffset = parameters.startOffset;
            Script.endOffset = parameters.endOffset;
            Script.strength = parameters.strength;

            // User-defined coloring
            Script.horizonFogVolumeColor = parameters.horizonFogVolumeColor;
            Script.horizonFogVolumeColorAuto = parameters.horizonFogVolumeColorAuto;
            Script.visibilityUser = parameters.visibilityUser;
            Script.startOffsetUser = parameters.startOffsetUser;
            Script.endOffsetUser = parameters.endOffsetUser;
            Script.strengthUser = parameters.strengthUser;

            Script.SetHorizonBlendMode(parameters.horizonBlendMode);
            Script.UpdateParams(updateAtmosphere);
            //Script.UpdateResources();
        }

        private void UpdateResources()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            if (Application.isPlaying) return;
            string worldPath = TTerraWorldManager.WorkDirectoryLocalPath + horizonMaterialName;

            if (Script.volumetricMaterial == null)
            {
                if (!File.Exists(worldPath))
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.volumetricHorizonMaterial), worldPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            Script.volumetricMaterial = AssetDatabase.LoadAssetAtPath(worldPath, typeof(Material)) as Material;
#endif
#endif
        }

        private void SetHorizonBlendMode(HorizonBlendMode _horizonBlendMode)
        {
            if (volumetricMaterial == null) return;

            if (autoColor)
            {
                volumetricMaterial.SetOverrideTag("Queue", "Transparent");
                volumetricMaterial.SetOverrideTag("IgnoreProjector", "True");
                volumetricMaterial.SetOverrideTag("RenderType", "Transparent");
                volumetricMaterial.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                switch (_horizonBlendMode)
                {
                    case HorizonBlendMode.Normal:
                        volumetricMaterial.SetOverrideTag("Queue", "Overlay-1");
                        volumetricMaterial.SetOverrideTag("IgnoreProjector", "True");
                        volumetricMaterial.SetOverrideTag("RenderType", "Transparent");
                        volumetricMaterial.renderQueue = (int)RenderQueue.Overlay - 1;
                        break;
                    case HorizonBlendMode.Underlay:
                        volumetricMaterial.SetOverrideTag("Queue", "Transparent");
                        volumetricMaterial.SetOverrideTag("IgnoreProjector", "True");
                        volumetricMaterial.SetOverrideTag("RenderType", "Transparent");
                        volumetricMaterial.renderQueue = (int)RenderQueue.Transparent;
                        break;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //material = volumetricMaterial;
        }

        public override void UpdateVolume(bool updateAtmosphere = true)
        {
            if (updateAtmosphere && !Application.isPlaying)
                TimeOfDay.Apply();

            float angleRads = coneAngle * Mathf.Deg2Rad;
            float bottomRadius = Mathf.Tan(angleRads) * coneHeight;
            float bottomRadiusHalf = bottomRadius * 0.5f;
            Vector3 halfBoxSize = new Vector3(bottomRadius, coneHeight, bottomRadius);

            if (meshInstance)
            {
                ScaleMesh(meshInstance, halfBoxSize, -Vector3.up * coneHeight * 0.5f);

                // Set bounding volume so modified vertices don't get culled
                Bounds bounds = new Bounds();
                bounds.SetMinMax(-halfBoxSize, halfBoxSize);
                meshInstance.bounds = bounds;
            }

            if (materialInstance)
            {
                if (autoColor)
                {
                    materialInstance.SetVector("_ConeData", new Vector4(bottomRadiusHalf, coneHeight, startOffset, Mathf.Cos(angleRads)));
                    materialInstance.SetFloat("_Visibility", visibility);
                    materialInstance.SetColor("_Color", horizonFogVolumeColorAuto);
                }
                else
                {
                    materialInstance.SetVector("_ConeData", new Vector4(bottomRadiusHalf, coneHeight, startOffsetUser, Mathf.Cos(angleRads)));
                    materialInstance.SetFloat("_Visibility", visibilityUser);
                    materialInstance.SetColor("_Color", horizonFogVolumeColor);
                }
                
                materialInstance.SetVector("_TextureData", new Vector4(-textureMovement.x, -textureMovement.y, -textureMovement.z, (1f / textureScale)));
                materialInstance.SetTexture("_MainTex", texture);
            }
        }
    }
}

