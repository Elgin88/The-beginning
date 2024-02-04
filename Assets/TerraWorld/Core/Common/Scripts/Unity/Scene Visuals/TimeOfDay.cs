using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityStandardAssets.ImageEffects;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace TerraUnity.Runtime
{
    public enum TimeOfDayControls
    {
        Bypass = 0,
        Static = 1,
        Dynamic = 2,
        Manual = 3
    }

    public enum GlobalFogMode
    {
        Linear = 0,
        Exponential = 1,
        ExponentialSquared = 2
    }

    public struct OldTimeOfDayParams
    {
        public bool enableTimeOfDay;
        public Gradient nightDayColor;
        public float sunPosNormalized;
        public Color heightRayleighColor;
        public Color volumetricFogColor;
        public Color cloudsColor;
        public Color worldRayleighColorDay;
        public Color worldRayleighColorNight;
        public float worldRayleighColorIntensity;
        public Color cloudsEmissionColor;
        public float maxIntensity;
        public float minIntensity;
        public float minPoint;
        public float maxAmbient;
        public float minAmbient;
        public float minAmbientPoint;
        public float dayAtmosphereThickness;
        public float nightAtmosphereThickness;
        public Vector3 dayRotateSpeed;
        public Vector3 nightRotateSpeed;
        public float dayMaxExposureStrength;
        public float dayMinExposureStrength;
        public float nightMaxExposureStrength;
        public Color daySkyTint;
        public Color dayGroundColor;
        public Color nightSkyTint;
        public Color nightGroundColor;
        public float starsRendererNormilizedSunAngle;
    }

    public struct TimeOfDayParams
    {
        public TimeOfDayControls dayNightControl;
        public float elevation;
        public float azimuth;
        public float dayNightSpeed;
        public float nightSpeedRatio;
        public Gradient nightDayColor;
        public Color heightRayleighColor;
        public Color worldRayleighColorDay;
        public Color worldRayleighColorNight;
        public float worldRayleighColorIntensity;
        public float sunIntensity;
        public float moonIntensity;
        public float sunHorizonPoint;
        public float ambientHorizonPoint;
        public float ambientDay;
        public float ambientNight;
        public float dayAtmosphereThickness;
        public float nightAtmosphereThickness;
        public float dayMaxExposureStrength;
        public float dayMinExposureStrength;
        public float nightMaxExposureStrength;
        public float nightMinExposureStrength;
        public Color daySkyTint;
        public Color dayGroundColor;
        public Color nightSkyTint;
        public Color nightGroundColor;
        public float starsRendererNormilizedSunAngle;
        [XmlIgnore] public Material skyMaterial;
#if UNITY_EDITOR
        public string SkyMaterialPath { get => AssetDatabase.GetAssetPath(skyMaterial); set => skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(value); }
        public string StarsPrefabPath { get => AssetDatabase.GetAssetPath(starsPrefab); set => starsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(value); }
#endif
        [XmlIgnore] public GameObject starsPrefab;
        public bool enableFog;
        public Gradient fogColor;
        public float fogLuminance;
        public float fogDensity;
        public GlobalFogMode globalFogMode;
        public float linearFogStartDistance;
        public float linearFogEndDistance;
    }

    [ExecuteAlways]
    public class TimeOfDay : MonoBehaviour
    {
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static TimeOfDay Script { get => GetScript(); }
        private static TimeOfDay _script;
        public static string skyMaterialName = "Sky Material.mat";
        public static string starsPrefabName = "Stars Prefab.prefab";
        private const string starsGOName = "Night Stars";
        private float _SunSteps = 0;

        public static TimeOfDayParams Params { get => GetParams(); }

        private GameObject StarsGO 
        { 
            get 
            {
                if (Script.starsPrefab == null)
                {
#if UNITY_EDITOR && TERRAWORLD_PRO
                    string starsPrefabPath = TTerraWorldManager.WorkDirectoryLocalPath + starsPrefabName;

                    if (!File.Exists(starsPrefabPath))
                    {
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab), starsPrefabPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    Script.starsPrefab = AssetDatabase.LoadAssetAtPath(starsPrefabPath, typeof(GameObject)) as GameObject;
#endif
                }

                if (_starsGO == null)
                {
                    foreach (Transform t in TTerraWorldManager.SceneSettingsGO1.GetComponentsInChildren(typeof(Transform), true))
                        if (t.GetComponent<ParticleSystem>() != null && t.name.Equals(starsGOName))
                        {
                            _starsGO = t.gameObject;
                            break;
                        }

                    if (_starsGO == null && starsPrefab != null)
                    {
                        _starsGO = Instantiate(Script.starsPrefab);
                        _starsGO.name = starsGOName;
                        _starsGO.transform.parent = TTerraWorldManager.SceneSettingsGO1.transform;
                        _starsGO.transform.localPosition = Vector3.zero;
                    }
                }

                if (_starsGO != null && _starsRenderer == null)
                {
                    _starsRenderer = _starsGO.GetComponent<ParticleSystemRenderer>();

#if UNITY_EDITOR
                    if (_starsRenderer != null)
                    {
                        PreviewAllParticlesInEditorScene();
                        //ParticleSystem particleSystem = starsGO.GetComponent<ParticleSystem>();
                        //particleSystem?.Simulate(2);
                        //particleSystem?.Play();
                    }
                    else
                        throw new Exception("Assigned Star prefab does not contain any Particle Systems on it!");
#endif
                }

                return _starsGO;
            }
            set
            {
                _starsGO = value;
            }
        }

        private static TimeOfDay GetScript()
        {
            if (_script == null)
            {
                TimeOfDay script = TTerraWorldManager.SceneSettingsGO1.GetComponent<TimeOfDay>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<TimeOfDay>();
            }

            return _script;
        }

#if UNITY_EDITOR
        private static void PreviewAllParticlesInEditorScene()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Editor));
            if (assembly == null) return;

            Type particleSystemEditorUtilsType = assembly.GetType("UnityEditor.ParticleSystemEditorUtils");
            if (particleSystemEditorUtilsType == null) return;

            PropertyInfo previewLayers = particleSystemEditorUtilsType.GetProperty("previewLayers", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (previewLayers == null) return;

            uint allLayers = Convert.ToUInt32(uint.MaxValue);
            previewLayers.SetValue(null, allLayers);
        }
#endif

        [Header("Sun Controls"), Space(5)]
        public TimeOfDayControls dayNightControl;
        [Range(0f, 359f)] public float elevation = 45f;
        [Range(0f, 359f)] public float azimuth = 60f;
        [Range(1f, 12f)] public float dayNightCycleSpeed = 9f;
        [Range(0.1f, 10f)] public float nightSpeedRatio = 2f;

        [Header("Resources"), Space(10)]
        public Material skyMaterial;
        public GameObject starsPrefab;
        private ParticleSystemRenderer _starsRenderer;
        private GameObject _starsGO;

        [Header("Scene Colors"), Space(10)]
        public Gradient nightDayColor;
        [Range(0f, 1f)] private float sunPosAmbient;
        public Color heightRayleighColor = new Color(0.3843137f, 0.5117647f, 0.6392157f, 1);
        public Color worldRayleighColorDay = Color.white;
        public Color worldRayleighColorNight = Color.white;

        [Header("Fog Settings"), Space(10)]
        public bool enableFog = true;
        public Gradient fogColor;
        [Range(1f, 2.5f)] public float fogLuminance = 1.75f;
        public GlobalFogMode globalFogMode = GlobalFogMode.Exponential;
        public float linearFogStartDistance = 0f;
        public float linearFogEndDistance = 10000f;
        [Range(0.00005f, 0.0005f)] public float exponentialFogDensity = 0.00015f;

        [Header("Scene Lighting & Ambient"), Space(10)]
        public float worldRayleighColorIntensity = 12f;
        public float sunIntensity = 1f;
        public float moonIntensity = 1f;
        public float sunHorizonPoint = -0.2f;
        public float ambientHorizonPoint = -0.2f;
        public float ambientDay = 1f;
        public float ambientNight = 1f;

        [Header("Sky Settings"), Space(10)]
        [Range(0f, 10f)] public float dayAtmosphereThickness = 1f;
        [Range(0f, 1f)] public float nightAtmosphereThickness = 0.333f;
        [Range(0f, 80f)] public float dayMaxExposureStrength = 1f;
        [Range(0f, 80f)] public float dayMinExposureStrength = 0.85f;
        [Range(0f, 80f)] public float nightMaxExposureStrength = 0.5f;
        [Range(0f, 80f)] public float nightMinExposureStrength = 0.3f;
        public Color daySkyTint = new Color(0.25f, 0.25f, 0.25f, 1f);
        public Color dayGroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color nightSkyTint = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color nightGroundColor = new Color(0.1f, 0.125f, 0.15f, 1f);

        [Header("Misc"), Space(10)]
        [Range(0f, 80f)] public float starsRendererNormilizedSunAngle = 0.25f;

        private float exposure;
        private float hue, saturation, value;
        private float minCloudBrightness = 0.2f;
        private Scene scene;
        private Vector3 sunForwardDirection;
        private DateTime lastUpdate;

        private static TimeOfDayParams _parameters;

        public static TimeOfDayParams GetParams()
        {
            _parameters.elevation = Script.elevation;
            _parameters.azimuth = Script.azimuth;
            _parameters.dayNightSpeed = Script.dayNightCycleSpeed;
            _parameters.nightSpeedRatio = Script.nightSpeedRatio;
            _parameters.dayNightControl = Script.dayNightControl;
            _parameters.nightDayColor = Script.nightDayColor;
            _parameters.heightRayleighColor = Script.heightRayleighColor;
            _parameters.worldRayleighColorDay = Script.worldRayleighColorDay;
            _parameters.worldRayleighColorNight = Script.worldRayleighColorNight;
            _parameters.worldRayleighColorIntensity = Script.worldRayleighColorIntensity;
            _parameters.sunIntensity = Script.sunIntensity;
            _parameters.moonIntensity = Script.moonIntensity;
            _parameters.sunHorizonPoint = Script.sunHorizonPoint;
            _parameters.ambientHorizonPoint = Script.ambientHorizonPoint;
            _parameters.ambientDay = Script.ambientDay;
            _parameters.ambientNight = Script.ambientNight;
            _parameters.dayAtmosphereThickness = Script.dayAtmosphereThickness;
            _parameters.nightAtmosphereThickness = Script.nightAtmosphereThickness;
            _parameters.dayMaxExposureStrength = Script.dayMaxExposureStrength;
            _parameters.dayMinExposureStrength = Script.dayMinExposureStrength;
            _parameters.nightMaxExposureStrength = Script.nightMaxExposureStrength;
            _parameters.nightMinExposureStrength = Script.nightMinExposureStrength;
            _parameters.daySkyTint = Script.daySkyTint;
            _parameters.dayGroundColor = Script.dayGroundColor;
            _parameters.nightSkyTint = Script.nightSkyTint;
            _parameters.nightGroundColor = Script.nightGroundColor;
            _parameters.starsRendererNormilizedSunAngle = Script.starsRendererNormilizedSunAngle;
            _parameters.enableFog = Script.enableFog;
            _parameters.fogColor = Script.fogColor;
            _parameters.fogLuminance = Script.fogLuminance;
            _parameters.fogDensity = Script.exponentialFogDensity;
            _parameters.globalFogMode = Script.globalFogMode;
            _parameters.linearFogStartDistance = Script.linearFogStartDistance;
            _parameters.linearFogEndDistance = Script.linearFogEndDistance;

            // Get Resources
            _parameters.skyMaterial = Script.skyMaterial;
            _parameters.starsPrefab = Script.starsPrefab;

            return _parameters;
        }

        public static void SetParams(TimeOfDayParams parameters)
        {
            _parameters = parameters;

            Script.elevation = parameters.elevation;
            Script.azimuth = parameters.azimuth;
            Script.dayNightCycleSpeed = parameters.dayNightSpeed;
            Script.nightSpeedRatio = parameters.nightSpeedRatio;
            Script.dayNightControl = parameters.dayNightControl;
            Script.nightDayColor = parameters.nightDayColor;
            Script.heightRayleighColor = parameters.heightRayleighColor;
            Script.worldRayleighColorDay = parameters.worldRayleighColorDay;
            Script.worldRayleighColorNight = parameters.worldRayleighColorNight;
            Script.worldRayleighColorIntensity = parameters.worldRayleighColorIntensity;
            Script.sunIntensity = parameters.sunIntensity;
            Script.moonIntensity = parameters.moonIntensity;
            Script.sunHorizonPoint = parameters.sunHorizonPoint;
            Script.ambientHorizonPoint = parameters.ambientHorizonPoint;
            Script.ambientDay = parameters.ambientDay;
            Script.ambientNight = parameters.ambientNight;
            Script.dayAtmosphereThickness = parameters.dayAtmosphereThickness;
            Script.nightAtmosphereThickness = parameters.nightAtmosphereThickness;
            Script.dayMaxExposureStrength = parameters.dayMaxExposureStrength;
            Script.dayMinExposureStrength = parameters.dayMinExposureStrength;
            Script.nightMaxExposureStrength = parameters.nightMaxExposureStrength;
            Script.nightMinExposureStrength = parameters.nightMinExposureStrength;
            Script.daySkyTint = parameters.daySkyTint;
            Script.dayGroundColor = parameters.dayGroundColor;
            Script.nightSkyTint = parameters.nightSkyTint;
            Script.nightGroundColor = parameters.nightGroundColor;
            Script.starsRendererNormilizedSunAngle = parameters.starsRendererNormilizedSunAngle;
            Script.enableFog = parameters.enableFog;
            Script.fogColor = parameters.fogColor;
            Script.fogLuminance = parameters.fogLuminance;
            Script.exponentialFogDensity = parameters.fogDensity;
            Script.globalFogMode = parameters.globalFogMode;
            Script.linearFogStartDistance = parameters.linearFogStartDistance;
            Script.linearFogEndDistance = parameters.linearFogEndDistance;

            // Set Resources
            Script.skyMaterial = parameters.skyMaterial;
            Script.starsPrefab = parameters.starsPrefab;

            Script.UpdateResources();
            Apply();
        }

        public static void Apply()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            if (TTerraWorldManager.Sun == null) return;
            Script.scene = SceneManager.GetActiveScene();
            Script.SetEnvironmentReflections();
//#if UNITY_EDITOR
            Script.SetDayNightControl();
//#endif
            Script.SetupFog();
            Script.UpdateAtmosphere();
        }

        private void UpdateResources()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            string worldPath = TTerraWorldManager.WorkDirectoryLocalPath + skyMaterialName;

            if (Script.skyMaterial == null)
            {
                if (!File.Exists(worldPath))
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.skyMat), worldPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                Script.skyMaterial = AssetDatabase.LoadAssetAtPath(worldPath, typeof(Material)) as Material;
            }
            else
            {
                string localPath = AssetDatabase.GetAssetPath(Script.skyMaterial);

                if (!localPath.Equals(worldPath))
                {
                    if (File.Exists(Path.GetFullPath(worldPath))) File.Delete(Path.GetFullPath(worldPath));
                    AssetDatabase.Refresh();

                    if (!File.Exists(worldPath))
                    {
                        AssetDatabase.CopyAsset(localPath, worldPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    Script.skyMaterial = AssetDatabase.LoadAssetAtPath(worldPath, typeof(Material)) as Material;
                }
            }

            RenderSettings.skybox = Script.skyMaterial;
            worldPath = TTerraWorldManager.WorkDirectoryLocalPath + starsPrefabName;

            if (Script.starsPrefab == null)
            {
                if (!File.Exists(worldPath))
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab), worldPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                Script.starsPrefab = AssetDatabase.LoadAssetAtPath(worldPath, typeof(GameObject)) as GameObject;
            }
            else
            {
                string localPath = AssetDatabase.GetAssetPath(Script.starsPrefab);

                if (!localPath.Equals(worldPath))
                {
                    if (File.Exists(Path.GetFullPath(worldPath))) File.Delete(Path.GetFullPath(worldPath));
                    AssetDatabase.Refresh();

                    if (!File.Exists(worldPath))
                    {
                        AssetDatabase.CopyAsset(localPath, worldPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    Script.starsPrefab = AssetDatabase.LoadAssetAtPath(worldPath, typeof(GameObject)) as GameObject;
                }
            }

            // Needed to update stars parent gameobject
            StarsGO = null;
            DestroyImmediate(StarsGO);
#endif
#endif
        }

        // Needed for proper World's Abmient Lighting and Reflections syncing with Day/Night Cycle in builds
        private void SetEnvironmentReflections()
        {
            if (dayNightControl == TimeOfDayControls.Dynamic)
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        }

//#if UNITY_EDITOR
        private void SetDayNightControl()
        {
            if (dayNightControl == TimeOfDayControls.Bypass || dayNightControl == TimeOfDayControls.Manual)
                this.enabled = false;
            else if (dayNightControl == TimeOfDayControls.Static)
                this.enabled = true;
            else if (dayNightControl == TimeOfDayControls.Dynamic)
            {
                this.enabled = true;

                // Needed for proper World's Abmient Lighting and Reflections syncing with Day/Night Cycle in builds
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            }

            // Update scene's Ambient Lighting and Reflections
            DynamicGI.UpdateEnvironment();
        }
//#endif

        private void SetupFog ()
        {
            if (enableFog && fogColor != null)
            {
                GlobalFogExtended fog = TCameraManager.MainCamera.GetComponent<GlobalFogExtended>();
                if (fog == null) fog = TCameraManager.MainCamera.gameObject.AddComponent<GlobalFogExtended>();
                fog.enabled = true;
                RenderSettings.fogMode = (FogMode)((int)globalFogMode + 1);
                RenderSettings.fogStartDistance = linearFogStartDistance;
                RenderSettings.fogEndDistance = linearFogEndDistance;
                RenderSettings.fogDensity = exponentialFogDensity;
            }
            else
            {
                GlobalFogExtended fog = TCameraManager.MainCamera.GetComponent<GlobalFogExtended>();
                if (fog != null) fog.enabled = false;
            }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Apply();
        }
#endif

        private void Start()
        {
            // Update scene's Ambient Lighting and Reflections
            DynamicGI.UpdateEnvironment();

            if (Application.isPlaying)
            {
                Script.scene = SceneManager.GetActiveScene();
                Script.SetEnvironmentReflections();
                //#if UNITY_EDITOR
                Script.SetDayNightControl();
                //#endif
                Script.SetupFog();
                Script.UpdateAtmosphere();
            }
        }

        void Update()
        {
            if (dayNightControl == TimeOfDayControls.Dynamic)
            {
                if (dayNightCycleSpeed < 1) dayNightCycleSpeed = 1;
                float updateIntervalInSeconds = 234858f / Mathf.Exp(dayNightCycleSpeed);

                if ((DateTime.Now - lastUpdate).TotalMilliseconds > updateIntervalInSeconds)
                {
                    if (IsDay) _SunSteps += (float)((DateTime.Now - lastUpdate).TotalMilliseconds / updateIntervalInSeconds);
                    else _SunSteps += (float)((DateTime.Now - lastUpdate).TotalMilliseconds / updateIntervalInSeconds) * nightSpeedRatio;
                    if (_SunSteps < 1) _SunSteps = 1;
                    if (_SunSteps > 1440) _SunSteps = 1;
                    lastUpdate = DateTime.Now;
                    UpdateAtmosphere();
                }
            }
        }

        private void UpdateAtmosphere()
        {
            if (scene == null || !scene.isLoaded) return;

            if (dayNightControl == TimeOfDayControls.Dynamic)
                elevation = 0.25f * _SunSteps;

            TTerraWorldManager.Sun.transform.localEulerAngles = new Vector3(elevation, azimuth, 0);

            // Invert sun position at night to act as Moon
            TTerraWorldManager.Moon.transform.localEulerAngles = new Vector3
            (
                360 - TTerraWorldManager.Sun.transform.localEulerAngles.x,
                360 - TTerraWorldManager.Sun.transform.localEulerAngles.y,
                0
            );

            sunForwardDirection = TTerraWorldManager.Sun.transform.forward;
            float sunDotDay = Vector3.Dot(sunForwardDirection, Vector3.down);
            float sunDotNight = Vector3.Dot(sunForwardDirection, Vector3.up);
            sunPosAmbient = Mathf.Clamp01((sunDotDay - ambientHorizonPoint) / (1 - ambientHorizonPoint));

            float sunPos = Mathf.Clamp01((sunDotDay - sunHorizonPoint) / (1 - sunHorizonPoint));
            float moonPos = Mathf.Clamp01((sunDotNight - sunHorizonPoint) / (1 - sunHorizonPoint));

            //float sunPower = ((sunIntensity - moonIntensity) * sunPos) + moonIntensity;
            float sunPower = sunIntensity * sunPos;
            float moonPower = moonIntensity * moonPos;

            // Set sun & moon light intensity based on day/night lighting
            TTerraWorldManager.SunLight.intensity = sunPower + (2f * (sunPos + (0.5f * (1 - sunPos))));

            // Add some moon lighting power to simulate night ambient lighting
            TTerraWorldManager.MoonLight.intensity = moonPower + 30f;

            // Set reflection intensity based on day/night lighting
            RenderSettings.reflectionIntensity = sunPos;

            // Set sun/moon shadow strength based on day/night lighting
            TTerraWorldManager.SunLight.shadowStrength = Mathf.Clamp((1 - sunPos) + 0.4f, 0.875f, 0.925f);
            TTerraWorldManager.MoonLight.shadowStrength = 0.9f;

            // Switch between Sun & Moon in scene
            if (IsDay)
            {
                TTerraWorldManager.SunLight.enabled = true;
                TTerraWorldManager.MoonLight.enabled = false;
            }
            else
            {
                TTerraWorldManager.SunLight.enabled = false;
                TTerraWorldManager.MoonLight.enabled = true;
            }

            float ambientIntensity = ((ambientDay - ambientNight) * sunPosAmbient) + ambientNight + 1.5f;
            RenderSettings.ambientIntensity = ambientIntensity;

            if (nightDayColor != null)
            {
                TTerraWorldManager.SunLight.color = nightDayColor.Evaluate(sunPosAmbient);
                TTerraWorldManager.MoonLight.color = nightDayColor.Evaluate(sunPosAmbient);
            }

            if (enableFog && fogColor != null)
            {
                if (IsDay)
                    RenderSettings.fogColor = fogColor.Evaluate(sunPosAmbient) * TTerraWorldManager.SunLight.color * RenderSettings.ambientSkyColor * fogLuminance;
                else
                    RenderSettings.fogColor = RenderSettings.ambientSkyColor * 3.333f;
            }

            RenderSettings.ambientLight = TTerraWorldManager.SunLight.color;

            //float fullCycleInSeconds = 360f / daySpeed.x / 120f;

            // Get external components data
            CloudsManagerParams cloudsManagerParams = CloudsManager.GetParams();

#if UNITY_STANDALONE_WIN
            VolumetricFogParams volumetricFogParams = VolumetricFog.GetParams();

            if (VolumetricFog.Enabled)
            {
                if (volumetricFogParams.autoColor)
                {
                   Color volumetricFogColor = new Color(0.633f, 0.766f, 0.815f, 1f);
                   Color.RGBToHSV(volumetricFogColor, out hue, out saturation, out value);
                    volumetricFogParams.m_AmbientLightColor = Color.HSVToRGB(hue, saturation, value * sunPosAmbient * 1.25f);
                }
            }
#endif

            if (IsDay) // We are in Day
            {
                if (skyMaterial != null)
                {
                    float intensityDay = ((dayAtmosphereThickness - nightAtmosphereThickness) * (sunPosAmbient * 0.33f)) + nightAtmosphereThickness;
                    skyMaterial.SetFloat("_AtmosphereThickness", intensityDay);
                    exposure = ((dayMaxExposureStrength - dayMinExposureStrength) * sunPosAmbient) + dayMinExposureStrength;
                    skyMaterial.SetFloat("_Exposure", exposure);
                    skyMaterial.SetColor("_SkyTint", Color.Lerp(daySkyTint, nightSkyTint, 1 - sunPosAmbient));
                    skyMaterial.SetColor("_GroundColor", Color.Lerp(dayGroundColor, nightGroundColor, 1 - sunPosAmbient));
                }

                Color.RGBToHSV(cloudsManagerParams.tintColor, out hue, out saturation, out value);
                CloudsManager.SetTintColor(Color.HSVToRGB(hue, saturation, Mathf.Clamp(value * sunPosAmbient, minCloudBrightness, 0.9f)));
                Color.RGBToHSV(cloudsManagerParams.emissionColor, out hue, out saturation, out value);
                CloudsManager.SetEmissionColor(Color.HSVToRGB(hue, saturation, Mathf.Clamp((value * Mathf.Clamp(sunPosAmbient, 0.5f, 1f)) + (sunPosAmbient * 0.25f), 0f, 0.9f)));
            }
            else // We are at Night
            {
                float sunPosNormalizedNight = Mathf.Clamp01((sunDotNight - ambientHorizonPoint) / (1 - ambientHorizonPoint));

                if (skyMaterial != null)
                {
                    float intensityNight = nightAtmosphereThickness;
                    skyMaterial.SetFloat("_AtmosphereThickness", intensityNight);
                    exposure = ((nightMaxExposureStrength - nightMinExposureStrength) * sunPosNormalizedNight) + nightMinExposureStrength;
                    skyMaterial.SetFloat("_Exposure", exposure);
                    skyMaterial.SetColor("_SkyTint", Color.Lerp(nightSkyTint, daySkyTint, sunPosNormalizedNight));
                    skyMaterial.SetColor("_GroundColor", Color.Lerp(nightGroundColor, dayGroundColor, sunPosNormalizedNight));
                }

                //Color.RGBToHSV(cloudsManagerParams.tintColor, out hue, out saturation, out value);
                //CloudsManager.SetTintColor(Color.HSVToRGB(hue, saturation, Mathf.Clamp(value * sunPosNormalizedNight - 0.2f, 0f, 0.1f)));
                CloudsManager.SetTintColor(Color.black);

                Color.RGBToHSV(cloudsManagerParams.emissionColor, out hue, out saturation, out value);
                CloudsManager.SetEmissionColor(Color.HSVToRGB(hue, saturation, Mathf.Clamp(value * sunPosNormalizedNight, 0f, 0.35f)));
                //CloudsManager.SetEmissionColor(Color.black);
            }

            //Set external components data
#if UNITY_STANDALONE_WIN
            VolumetricFog.SetParams(volumetricFogParams, false);
#endif

            UpdateStars();
            UpdateWaterColors();
            UpdateHorizonFogVolumeColor();

            // Update scene's Ambient Lighting and Reflections
            DynamicGI.UpdateEnvironment();
        }

        private void UpdateStars ()
        {
            if (StarsGO == null) return;
            StarsGO.transform.rotation = TTerraWorldManager.Sun.transform.rotation;

            if (_starsRenderer != null)
            {
                if (sunPosAmbient > starsRendererNormilizedSunAngle) // && sunPosAmbient < starsRendererEndAngle)
                {
                    _starsRenderer.enabled = false;
                    //ParticleSystem particleSystem = StarsGO.GetComponent<ParticleSystem>();
                    //particleSystem?.Simulate(2);
                    //particleSystem?.Play();
                }
                else
                {
                    _starsRenderer.enabled = true;
                    //ParticleSystem particleSystem = StarsGO.GetComponent<ParticleSystem>();
                    //particleSystem?.Stop();
                }
            }
        }

        public static void UpdateWaterColors ()
        {
            if (WaterManager.water4UMaterial == null) return;

            WaterManagerParams waterManagerParams = WaterManager.GetParams();

            Color.RGBToHSV(waterManagerParams.waterBaseColor, out Script.hue, out Script.saturation, out Script.value);
            Color waterColor = Color.HSVToRGB(Script.hue, Script.saturation, Script.value * Script.sunPosAmbient);
            waterColor.a = waterManagerParams.waterBaseColor.a;
            WaterManager.water4UMaterial.SetColor("_BaseColor", waterColor);
            
            Color.RGBToHSV(waterManagerParams.waterReflectionColor, out Script.hue, out Script.saturation, out Script.value);
            waterColor = Color.HSVToRGB(Script.hue, Script.saturation, Script.value * Script.sunPosAmbient);
            waterColor.a = waterManagerParams.waterReflectionColor.a;
            WaterManager.water4UMaterial.SetColor("_ReflectionColor", waterColor);
            
            if (TTerraWorldManager.SunLight != null)
                WaterManager.water4UMaterial.SetColor("_SpecularColor", TTerraWorldManager.SunLight.color);
        }

        private void UpdateHorizonFogVolumeColor()
        {
            GameObject volume = HorizonFog.horizonFogGameObject;

            if (volume == null || HorizonFog.horizonFogGameObject == null)
            {
                foreach (Transform t in TTerraWorldManager.SceneSettingsGO1.GetComponentsInChildren(typeof(Transform), true))
                    if (t.name.Equals("Horizon Fog"))
                    {
                        volume = t.gameObject;
                        HorizonFog.horizonFogGameObject = t.gameObject;
                        break;
                    }
            }

            if (volume == null) return;

            HorizonFogParams horizonFogParams = HorizonFog.GetParams();

            if (TTerraWorldManager.SunLight != null)
            {
                //coneHeight = (Camera.main.farClipPlane / 2);
                horizonFogParams.coneHeight = 100000f;

                // Physcially based auto coloring
                if (horizonFogParams.autoColor)
                {
                    volume.transform.position = new Vector3(volume.transform.position.x, -horizonFogParams.coneHeight + horizonFogParams.endOffset - (horizonFogParams.coneHeight / 2), volume.transform.position.z);
                    //float _sunPosNormalized = Mathf.Clamp(Vector3.Dot(TTerraWorldManager.SunLight.transform.forward, Vector3.down), Mathf.Epsilon, 0.55f);

                    if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_GroundColor"))
                    {
                        if (IsDay)
                        {
                            horizonFogParams.horizonFogVolumeColorAuto = RenderSettings.skybox.GetColor("_GroundColor");
                            horizonFogParams.horizonFogVolumeColorAuto *= horizonFogParams.strength * sunPosAmbient * 2f;
                            horizonFogParams.horizonFogVolumeColorAuto += RenderSettings.ambientGroundColor;
                        }
                        else
                            horizonFogParams.horizonFogVolumeColorAuto = RenderSettings.ambientGroundColor;

                        //if (TTerraWorldManager.SunLight.transform.eulerAngles.x == 0 || TTerraWorldManager.SunLight.transform.eulerAngles.x >= 350 || Script.sunPosAmbient > 0)
                        //{
                        //    horizonFogParams.horizonFogVolumeColorAuto *= Script.sunPosAmbient + (horizonFogParams.strength * Script.sunPosAmbient);
                        //    //horizonFogParams.horizonFogVolumeColorAuto *= Script.sunPosAmbient + (horizonFogParams.strength * Script.sunPosAmbient) + 0.25f;
                        //    //horizonFogParams.horizonFogVolumeColorAuto += Color.Lerp(TTerraWorldManager.SunLight.color, horizonFogParams.horizonFogVolumeColorAuto, Mathf.Clamp01(sunPosAmbient));
                        //    horizonFogParams.horizonFogVolumeColorAuto += Color.Lerp(RenderSettings.ambientGroundColor * 1, horizonFogParams.horizonFogVolumeColorAuto, Mathf.Clamp01(Script.sunPosAmbient));
                        //}
                        //else
                        //    horizonFogParams.horizonFogVolumeColorAuto *= 0.5f;
                    }
                    else
                        horizonFogParams.horizonFogVolumeColorAuto = RenderSettings.ambientGroundColor;
                }
                // User-defined coloring
                else
                {
                    volume.transform.position = new Vector3(volume.transform.position.x, -horizonFogParams.coneHeight + horizonFogParams.endOffsetUser - (horizonFogParams.coneHeight / 2), volume.transform.position.z);
                    horizonFogParams.horizonFogVolumeColor.a = horizonFogParams.strengthUser;
                }
            }

            HorizonFog.SetParams(horizonFogParams, false);
        }

        public static bool IsDay
        {
            get
            {
                if (Script.sunPosAmbient > 0) return true;
                else return false;
            }
        }

        //TODO: Debug following lines later
        //private void ResetSkyMaterial()
        //{
        //    if (TTerraWorldManager.SunLight != null) TTerraWorldManager.SunLight.intensity = 1;
        //
        //    if (skyMaterial != null)
        //    {
        //        skyMaterial.SetFloat("_AtmosphereThickness", 1.2f);
        //        skyMaterial.SetFloat("_Exposure", 1.56f);
        //    }
        //}
        //
        //void OnDestroy()
        //{
        //    ResetSkyMaterial();
        //}
    }
}

