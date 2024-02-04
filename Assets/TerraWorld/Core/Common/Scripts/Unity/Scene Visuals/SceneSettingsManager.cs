#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEditor;
using UnityStandardAssets.ImageEffects;
using System;
using System.IO;
using System.Xml.Serialization;
using TerraUnity.Runtime;
using TerraUnity.Utils;
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class SceneSettingsManager
    {
        //private static bool worldIsInitialized;

        public struct VFXData
        {
            public bool Actived;
            public bool Enabled;
            public TimeOfDayParams timeOfDayParams;
            public CrepuscularParams crepuscularParams;
            public WaterManagerParams waterManagerParams;
            public CloudsManagerParams cloudsManagerParams;
            //public AtmosphericScatteringParams atmosphericScatteringParams;
            public VolumetricFogParams volumetricFogParams;
            public WindManagerParams windManagerParams;
            public HorizonFogParams horizonFogParams;
            public SnowManagerParams snowManagerParams;
            public FlatShadingParams flatShadingParams;
            public ColormapBlendingParams colormapBlendingParams;

#if UNITY_POST_PROCESSING_STACK_V2
            public bool isPostProcessing;
            public PostProcessingParams postProcessingParams;
#endif 

            public void ApplyOldfxParams(FXParams fxParams)
            {
                Actived = true;

                if (fxParams.selectionIndexVFX == 0)
                    Enabled = true;
                else
                    Enabled = false;

                // Time of day settings
                timeOfDayParams.dayNightControl = (TimeOfDayControls)fxParams.dayNightControl; // 0 = off, 1 = Manual, 2 = Auto = true;
                timeOfDayParams.elevation = fxParams.elevation;
                timeOfDayParams.azimuth = fxParams.azimuth;
                timeOfDayParams.dayNightSpeed = fxParams.globalSpeed / 4f;

                // Crepuscular Rays settings
                crepuscularParams.hasGodRays = fxParams.hasGodRays;
                crepuscularParams.godRaySamples = fxParams.godRaySamples;
                crepuscularParams.godRayDensity = fxParams.godRayDensity;
                crepuscularParams.godRayWeight = fxParams.godRayWeight;
                crepuscularParams.godRayDecay = fxParams.godRayDecay;
                crepuscularParams.godRayExposure = fxParams.godRayExposure;

                // Clouds settings
                cloudsManagerParams.hasClouds = fxParams.hasClouds;
                cloudsManagerParams.castShadows = fxParams.cloudShadows;
                cloudsManagerParams.density = fxParams.cloudsDensity;
                cloudsManagerParams.cloudSize = fxParams.cloudSize;
                cloudsManagerParams.altitude = fxParams.cloudsAltitude;
                cloudsManagerParams.tintColor = TUtils.Vector4ToUnityColor(fxParams.cloudColor);
                cloudsManagerParams.isCustomShape = fxParams.isCustomShape;
                cloudsManagerParams.cloudMeshNormalOffset = fxParams.cloudNormalOffset;
                //cloudsManagerParams.CloudMeshTexturePath = fxParams.cloudMeshTexturePath;
                cloudsManagerParams.hasRotation = fxParams.hasRotation;
                cloudsManagerParams.hasFog = fxParams.hasFog;

                // Atmospheric Scattering Fog settings
                //atmosphericScatteringParams.hasAtmosphericScattering = false;
                //atmosphericScatteringParams.hasAtmosphericScattering = fxParams.hasAtmosphericScattering;
                //atmosphericScatteringParams.heightRayleighIntensity = fxParams.volumetricLightIntensity;
                //atmosphericScatteringParams.worldMieColorIntensity = fxParams.atmosphericFogIntensity;
                //atmosphericScatteringParams.heightRayleighDensity = fxParams.atmosphericFogDensity;
                //atmosphericScatteringParams.heightDistance = fxParams.atmosphericFogDistance;

                // Volumetric Fog settings
                volumetricFogParams.hasVolumetricFog = fxParams.hasVolumetricFog;
                volumetricFogParams.m_GlobalDensityMult = fxParams.fogStrength;
                volumetricFogParams.m_NearClip = fxParams.fogNearClip;
                volumetricFogParams.m_FarClipMax = fxParams.fogFarClip;
                volumetricFogParams.m_ConstantFog = fxParams.volumetricFogDensity;
                volumetricFogParams.m_NoiseFogAmount = fxParams.fogNoiseAmount;
                volumetricFogParams.m_NoiseFogScale = fxParams.fogNoiseScale;
                volumetricFogParams.m_AmbientLightColor = TUtils.Vector4ToUnityColor(fxParams.volumetricFogColor);

                // Wind settings
                windManagerParams.hasWind = fxParams.hasWind;
                //TODO: 
                //windManagerParams. = windTime;
                //windManagerParams.hasWind = windSpeed;
                //windManagerParams.hasWind = windBending;

                // Weather settings
                snowManagerParams.hasSnow = fxParams.hasSnow;
                snowManagerParams.snowHeight = fxParams.snowStartHeight;
                snowManagerParams.snowThickness = fxParams.snowThickness; // 1f;
                snowManagerParams.snowDamping = fxParams.snowDamping; // 0.5f;
                snowManagerParams.snowFalloff = fxParams.heightFalloff;
                //TODO :
                //snowManagerParams.hasSnow = BGTerrainHasSnow;

                // Water settings
                waterManagerParams.waterQuality = fxParams.waterQuality;
                waterManagerParams.waterBaseColor = TUtils.Vector4ToUnityColor(fxParams.waterBaseColor);
                waterManagerParams.waterReflectionColor = TUtils.Vector4ToUnityColor(fxParams.waterReflectionColor);
                waterManagerParams.hasReflection = fxParams.hasReflection; // Real-time Reflections settings
                waterManagerParams.reflectionQuality = fxParams.reflectionQuality;
                waterManagerParams.edgeBlend = fxParams.edgeBlend;
                waterManagerParams.specularLighting = fxParams.specularLighting;
                waterManagerParams.gerstnerWaves = fxParams.GerstnerWaves;
                //waterManagerParams.reflectionDistance = fxParams.reflectionDistance; // Not affecting rendering

                // Post Processing settings
#if UNITY_POST_PROCESSING_STACK_V2
                if (fxParams.isPostProcessing == 0)
                    isPostProcessing = true;
                else
                    isPostProcessing = false;
#endif

                // Horizon Fog settings
                horizonFogParams.hasHorizonFog = fxParams.hasHorizonFog;
                horizonFogParams.autoColor = fxParams.autoColor;
                horizonFogParams.horizonFogVolumeColor = TUtils.Vector4ToUnityColor(fxParams.horizonFogColor);
                //horizonFogParams.hasHorizonFog = horizonFogDensityAuto;
                //horizonFogParams.strength = horizonFogStrengthAuto;
                //horizonFogParams.hasHorizonFog = horizonFogStartHeightAuto;
                //horizonFogParams.hasHorizonFog = horizonBlendModeAuto;
                horizonFogParams.horizonBlendMode = fxParams.horizonBlendMode;
                //horizonFogParams.hasHorizonFog = horizonFogEndHeightAuto;
                //horizonFogParams.hasHorizonFog = horizonFogDensityManual;
                horizonFogParams.strength = fxParams.horizonFogStrengthManual;
                horizonFogParams.startOffset = fxParams.horizonFogStartHeightManual;
                horizonFogParams.endOffset = fxParams.horizonFogEndHeightManual;

                // Flat Shading settings
                flatShadingParams.isFlatShadingObjects = fxParams.isFlatShading;

                // Colormap Blending settings
                colormapBlendingParams.hasColormapBlendingGPU = fxParams.hasColormapBlendingGPU;
                colormapBlendingParams.hasColormapBlendingGrass = fxParams.hasColormapBlendingGrass;
                colormapBlendingParams.blendingStrengthGPU = fxParams.blendingStrengthGPU;
                colormapBlendingParams.blendingStrengthGrass = fxParams.blendingStrengthGrass;
                colormapBlendingParams.excludeOpaqueMaterialsGPU = fxParams.excludeOpaqueMaterialsGPU;
                colormapBlendingParams.excludeOpaqueMaterialsGrass = fxParams.excludeOpaqueMaterialsGrass;
            }

            public void ApplyOldTimeOfDayParams(OldTimeOfDayParams _oldtimeOfDayParams)
            {
                // TODO : these params form oldtimeofday struc shuold convert to new one
                /*
                -  enableTimeOfDay;
                - sunPosNormalized;
                - volumetricFogColor;
                - cloudsColor;
                - cloudsEmissionColor;
                - maxIntensity;
                - minIntensity;
                - minPoint;
                - maxAmbient;
                - minAmbient;
                -  minAmbientPoint;
                gradualEnvironmentLightingUpdate;
                */

                timeOfDayParams.dayNightSpeed = _oldtimeOfDayParams.dayRotateSpeed.x;
                timeOfDayParams.nightSpeedRatio = _oldtimeOfDayParams.nightRotateSpeed.x / _oldtimeOfDayParams.dayRotateSpeed.x;
                timeOfDayParams.nightDayColor = _oldtimeOfDayParams.nightDayColor;
                timeOfDayParams.heightRayleighColor = _oldtimeOfDayParams.heightRayleighColor;
                timeOfDayParams.worldRayleighColorDay = _oldtimeOfDayParams.worldRayleighColorDay;
                timeOfDayParams.worldRayleighColorNight = _oldtimeOfDayParams.worldRayleighColorNight;
                timeOfDayParams.worldRayleighColorIntensity = _oldtimeOfDayParams.worldRayleighColorIntensity;
                timeOfDayParams.dayAtmosphereThickness = _oldtimeOfDayParams.dayAtmosphereThickness;
                timeOfDayParams.nightAtmosphereThickness = _oldtimeOfDayParams.nightAtmosphereThickness / 4f;
                timeOfDayParams.dayMaxExposureStrength = _oldtimeOfDayParams.dayMaxExposureStrength;
                timeOfDayParams.dayMinExposureStrength = _oldtimeOfDayParams.dayMinExposureStrength;
                timeOfDayParams.nightMaxExposureStrength = _oldtimeOfDayParams.nightMaxExposureStrength;
                timeOfDayParams.daySkyTint = _oldtimeOfDayParams.daySkyTint;
                timeOfDayParams.dayGroundColor = _oldtimeOfDayParams.dayGroundColor;
                timeOfDayParams.nightSkyTint = _oldtimeOfDayParams.nightSkyTint;
                timeOfDayParams.nightGroundColor = _oldtimeOfDayParams.nightGroundColor;
                timeOfDayParams.starsRendererNormilizedSunAngle = _oldtimeOfDayParams.starsRendererNormilizedSunAngle;
                timeOfDayParams.sunIntensity = _oldtimeOfDayParams.maxIntensity;
                timeOfDayParams.moonIntensity = _oldtimeOfDayParams.minIntensity;
                timeOfDayParams.sunHorizonPoint = _oldtimeOfDayParams.minPoint;
                timeOfDayParams.ambientDay = _oldtimeOfDayParams.maxAmbient;
                timeOfDayParams.ambientNight = _oldtimeOfDayParams.minAmbient;
                timeOfDayParams.ambientHorizonPoint = _oldtimeOfDayParams.minAmbientPoint;
            }

            //TODO: Get data from Scene Settings prefab without instantiating it
            public VFXData GetDefault()
            {
                VFXData _VFXData = new VFXData();


                return _VFXData;
            }

            public List<string> GetResourcePaths()
            {
                List<string> result = new List<string>();
                // if (!string.IsNullOrEmpty(timeOfDayParams.SkyMaterialPath)) result.Add(timeOfDayParams.SkyMaterialPath);
                //  if (!string.IsNullOrEmpty(timeOfDayParams.StarsPrefabPath)) result.Add(timeOfDayParams.StarsPrefabPath);
                //  if (!string.IsNullOrEmpty(crepuscularParams.MaterialPath)) result.Add(crepuscularParams.MaterialPath);
                // if (!string.IsNullOrEmpty(cloudsManagerParams.CloudPrefabPath)) result.Add(cloudsManagerParams.CloudPrefabPath);
                // if (!string.IsNullOrEmpty(cloudsManagerParams.CloudMeshPath)) result.Add(cloudsManagerParams.CloudMeshPath);
                if (!string.IsNullOrEmpty(cloudsManagerParams.CloudMeshUserPath)) result.Add(cloudsManagerParams.CloudMeshUserPath);
                // if (!string.IsNullOrEmpty(horizonFogParams.MaterialPath)) result.Add(horizonFogParams.MaterialPath);

                return result;
            }
        }

        //private static VFXData _VFXData;

        public static VFXData GetVFXData()
        {
            VFXData _VFXData = TTerraWorldManager.WorldGraph.VFXDATA;

            if (Active)
            {
                _VFXData.Actived = true;
                _VFXData.Enabled = TTerraWorldManager.SceneSettingsGO1.activeSelf;

                if (_VFXData.Enabled)
                {
                    _VFXData.timeOfDayParams = TimeOfDay.GetParams();
                    _VFXData.crepuscularParams = Crepuscular.GetParams();
                    _VFXData.cloudsManagerParams = CloudsManager.GetParams();
                    _VFXData.waterManagerParams = WaterManager.GetParams();
                    _VFXData.windManagerParams = WindManager.GetParams();
                    _VFXData.horizonFogParams = HorizonFog.GetParams();
                    _VFXData.snowManagerParams = SnowManager.GetParams();
                    _VFXData.flatShadingParams = FlatShadingManager.GetParams();
                    _VFXData.colormapBlendingParams = ColormapBlendingManager.GetParams();

#if UNITY_POST_PROCESSING_STACK_V2
                    _VFXData.isPostProcessing = PostProcessingManager.IsPostProcessing();

                    if (_VFXData.isPostProcessing)
                        _VFXData.postProcessingParams = PostProcessingManager.GetParams();
#endif

#if UNITY_STANDALONE_WIN
                    //_VFXData.atmosphericScatteringParams = AtmosphericScattering.GetParams();
                    _VFXData.volumetricFogParams = VolumetricFog.GetParams();
#endif
                }
            }
            else
                _VFXData.Actived = false;

            TTerraWorldManager.WorldGraph.VFXDATA = _VFXData;

            return TTerraWorldManager.WorldGraph.VFXDATA;
        }

        public static void SetVFXData(VFXData data)
        {
            // Avoid performing this function during builds
            if (BuildPipeline.isBuildingPlayer) return;

            TTerraWorldManager.WorldGraph.VFXDATA = data;
            Active = TTerraWorldManager.WorldGraph.VFXDATA.Actived;

            if (Active)
            {
                if (TTerraWorldManager.WorldGraph.VFXDATA.Enabled)
                    EnableVisualFX();
                else
                    DisableVisualFX(false);
            }
            else
                DisableVisualFX(true);

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
            TTerraWorldManager.WorldGraph.VFXDATA = GetVFXData();
            TTerraWorldManager.SaveOldGraph();
        }

        public static void Destroyed()
        {
            //TODO: Check if the following lines are needed!
            //VFXData _VFXData = TTerraWorldManager.WorldGraph.VFXDATA;
            //_VFXData.Actived = false;
            //TTerraWorldManager.WorldGraph.VFXDATA = _VFXData;
            DisableVisualFX(true);
        }

        public static bool Active
        {
            get
            {
                if (TTerraWorldManager.SceneSettingsGO1 != null) return true;
                else return false;
            }
            set
            {
                if (value && !Active) InstantiateNewSceneSettings();
                if (!value && Active) MonoBehaviour.DestroyImmediate(TTerraWorldManager.SceneSettingsGO1);
            }
        }

        public static void EnableVisualFX()
        {
            TTerraWorldManager.SceneSettingsGO1.SetActive(true);
            VFXData _VFXData = TTerraWorldManager.WorldGraph.VFXDATA;

            // Set "Scene Settings" components
            TimeOfDay.SetParams(_VFXData.timeOfDayParams);
            Crepuscular.SetParams(_VFXData.crepuscularParams);
            CloudsManager.SetParams(_VFXData.cloudsManagerParams);
            WaterManager.SetParams(_VFXData.waterManagerParams);
            WindManager.SetParams(_VFXData.windManagerParams);
            HorizonFog.SetParams(_VFXData.horizonFogParams);
            SnowManager.SetParams(_VFXData.snowManagerParams);
            FlatShadingManager.SetParams(_VFXData.flatShadingParams);
            ColormapBlendingManager.SetParams(_VFXData.colormapBlendingParams);

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessingManager.SetPostProcessing(_VFXData.isPostProcessing);

            if (_VFXData.isPostProcessing)
                PostProcessingManager.SetParams(_VFXData.postProcessingParams);
#endif

#if UNITY_STANDALONE_WIN
            //AtmosphericScattering.SetParams(_VFXData.atmosphericScatteringParams);
            VolumetricFog.SetParams(_VFXData.volumetricFogParams);
#endif

            // Enable camera VFX components
            Crepuscular.Enabled = _VFXData.crepuscularParams.hasGodRays;

#if UNITY_STANDALONE_WIN
            //AtmosphericScatteringDeferred.Enabled = false;
            //AtmosphericScatteringDeferred.Enabled = _VFXData.atmosphericScatteringParams.hasAtmosphericScattering;
            VolumetricFog.Enabled = _VFXData.volumetricFogParams.hasVolumetricFog;
#endif
        }

        public static void DisableVisualFX(bool destroyComponents)
        {
            if (Active)
                TTerraWorldManager.SceneSettingsGO1.SetActive(false);

            if (TTerraWorldManager.SceneSettingsGO1 != null)
            {
                SnowManagerParams snowManagerParams = SnowManager.GetParams();
                snowManagerParams.hasSnow = false;
                SnowManager.SetParams(snowManagerParams);
            }

            if (destroyComponents) // Destroy camera VFX components
            {
                if (Camera.main == null) return;
                Crepuscular crepuscular = TCameraManager.MainCamera.GetComponent<Crepuscular>();
                if (crepuscular != null) MonoBehaviour.DestroyImmediate(crepuscular);

#if UNITY_STANDALONE_WIN
                //AtmosphericScatteringDeferred atmosphericScatteringDeferred = TCameraManager.MainCamera.GetComponent<AtmosphericScatteringDeferred>();
                //if (atmosphericScatteringDeferred != null) MonoBehaviour.DestroyImmediate(atmosphericScatteringDeferred);

                VolumetricFog volumetricFog = TCameraManager.MainCamera.GetComponent<VolumetricFog>();
                if (volumetricFog != null) MonoBehaviour.DestroyImmediate(volumetricFog);
#endif

#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessLayer postProcessLayer = TCameraManager.MainCamera.GetComponent<PostProcessLayer>();
                if (postProcessLayer != null) MonoBehaviour.DestroyImmediate(postProcessLayer);
#endif

                GlobalFogExtended fog = TCameraManager.MainCamera.GetComponent<GlobalFogExtended>();
                if (fog != null) MonoBehaviour.DestroyImmediate(fog);
            }
            else // Disable camera VFX components
            {
                Crepuscular.Enabled = false;

#if UNITY_STANDALONE_WIN
                //AtmosphericScatteringDeferred.Enabled = false;
                VolumetricFog.Enabled = false;
#endif

#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessingManager.SetPostProcessing(false);
#endif

                GlobalFogExtended fog = TCameraManager.MainCamera.GetComponent<GlobalFogExtended>();
                if (fog != null) fog.enabled = false;
            }

#if UNITY_EDITOR
            if (Application.isEditor) RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
#endif

            try
            {
                if (TTerraWorldManager.Sun != null)
                {
                    if (TTerraWorldManager.SunLight != null)
                        TTerraWorldManager.SunLight.intensity = 1;

                    //if (TTerraWorldManager.Sun.GetComponent<AtmosphericScatteringSun>() != null)
                    //MonoBehaviour.DestroyImmediate(TTerraWorldManager.Sun.GetComponent<AtmosphericScatteringSun>());
                }
            }
            catch { }
        }

#if UNITY_POST_PROCESSING_STACK_V2
        private static PostProcessVolume postProcessVolume { get => PostProcessingManager.PostProcessVolumeScript; }
#endif

        public static void ResetVFXSettings()
        {
            SetVFXData(new VFXData());
        }

        public static void MoveResourcesToWorkDirectory()
        {
            //worldIsInitialized = true;
            AssetDatabase.Refresh();

            // Copy & duplicate resource assets into corresponding world directory
            TResourcesManager.LoadAllResources();

            //TODO: Set all resource references in _sceneSettingsGO to TTerraWorld.WorkDirectoryLocalPath and remove the following lines
            //_sceneSettingsGO.GetComponent<TimeOfDay>().skyMaterial = AssetDatabase.LoadAssetAtPath(skyMaterialPath, typeof(Material)) as Material;

            string skyMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TimeOfDay.skyMaterialName;
            if (!File.Exists(skyMaterialPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.skyMat), skyMaterialPath);

            string starsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + TimeOfDay.starsPrefabName;
            if (!File.Exists(starsPrefabPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab), starsPrefabPath);

            TimeOfDayParams timeofDayParams = TimeOfDay.GetParams();
            timeofDayParams.skyMaterial = AssetDatabase.LoadAssetAtPath(skyMaterialPath, typeof(Material)) as Material;
            timeofDayParams.starsPrefab = AssetDatabase.LoadAssetAtPath(starsPrefabPath, typeof(GameObject)) as GameObject;
            TimeOfDay.SetParams(timeofDayParams);

            string cloudsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + CloudsManager.cloudsPrefabName;
            if (!File.Exists(cloudsPrefabPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.cloudPrefab), cloudsPrefabPath);

            CloudsManagerParams cloudsManagerParams = CloudsManager.GetParams();
            cloudsManagerParams.cloudMesh = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(TResourcesManager.cloudMesh), typeof(Mesh)) as Mesh;
            cloudsManagerParams.cloudPrefab = AssetDatabase.LoadAssetAtPath(cloudsPrefabPath, typeof(GameObject)) as GameObject;
            CloudsManager.SetParams(cloudsManagerParams);

            string godRaysMaterialPath = TTerraWorld.WorkDirectoryLocalPath + Crepuscular.godRaysMaterialName;
            if (!File.Exists(godRaysMaterialPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.godRaysMaterial), godRaysMaterialPath);

            CrepuscularParams crepuscularParams = Crepuscular.GetParams();
            crepuscularParams.material = AssetDatabase.LoadAssetAtPath(godRaysMaterialPath, typeof(Material)) as Material;
            Crepuscular.SetParams(crepuscularParams);

            string horizonMaterialPath = TTerraWorld.WorkDirectoryLocalPath + HorizonFog.horizonMaterialName;
            if (!File.Exists(horizonMaterialPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.volumetricHorizonMaterial), horizonMaterialPath);

            HorizonFogParams horizonFogParams = HorizonFog.GetParams();
            horizonFogParams.material = AssetDatabase.LoadAssetAtPath(horizonMaterialPath, typeof(Material)) as Material;
            HorizonFog.SetParams(horizonFogParams);

#if UNITY_POST_PROCESSING_STACK_V2
            string postProcessingProfilePath = TTerraWorld.WorkDirectoryLocalPath + PostProcessingManager.postProcessingProfileName;

            if (!File.Exists(postProcessingProfilePath))
            {
                TResourcesManager.LoadPostProcessingResources();
                PostProcessProfile postProcessingAsset = TResourcesManager.postProcessProfile;
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(postProcessingAsset), postProcessingProfilePath);
            }

            postProcessVolume.profile = AssetDatabase.LoadAssetAtPath(postProcessingProfilePath, typeof(PostProcessProfile)) as PostProcessProfile;
            EditorUtility.SetDirty(postProcessVolume.profile);
#endif

            AssetDatabase.SaveAssets();
            //worldIsInitialized = false;
        }

        public static void ReplaceRefrences(ref SceneSettingsManager.VFXData fXData)
        {
            string skyMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TimeOfDay.skyMaterialName;
            if (!File.Exists(skyMaterialPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.skyMat), skyMaterialPath);

            string starsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + TimeOfDay.starsPrefabName;
            if (!File.Exists(starsPrefabPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab), starsPrefabPath);

            fXData.timeOfDayParams.skyMaterial = AssetDatabase.LoadAssetAtPath(skyMaterialPath, typeof(Material)) as Material;
            fXData.timeOfDayParams.starsPrefab = AssetDatabase.LoadAssetAtPath(starsPrefabPath, typeof(GameObject)) as GameObject;

            string cloudsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + CloudsManager.cloudsPrefabName;
            if (!File.Exists(cloudsPrefabPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.cloudPrefab), cloudsPrefabPath);

            fXData.cloudsManagerParams.cloudMesh = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(TResourcesManager.cloudMesh), typeof(Mesh)) as Mesh;
            fXData.cloudsManagerParams.cloudPrefab = AssetDatabase.LoadAssetAtPath(cloudsPrefabPath, typeof(GameObject)) as GameObject;

            string godRaysMaterialPath = TTerraWorld.WorkDirectoryLocalPath + Crepuscular.godRaysMaterialName;
            if (!File.Exists(godRaysMaterialPath))
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.godRaysMaterial), godRaysMaterialPath);

            fXData.crepuscularParams.material = AssetDatabase.LoadAssetAtPath(godRaysMaterialPath, typeof(Material)) as Material;

            string horizonMaterialPath = TTerraWorld.WorkDirectoryLocalPath + HorizonFog.horizonMaterialName;
            if (!File.Exists(horizonMaterialPath))

                fXData.horizonFogParams.material = AssetDatabase.LoadAssetAtPath(horizonMaterialPath, typeof(Material)) as Material;

#if UNITY_POST_PROCESSING_STACK_V2
            string postProcessingProfilePath = TTerraWorld.WorkDirectoryLocalPath + PostProcessingManager.postProcessingProfileName;

            if (!File.Exists(postProcessingProfilePath))
            {
                TResourcesManager.LoadPostProcessingResources();
                PostProcessProfile postProcessingAsset = TResourcesManager.postProcessProfile;
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(postProcessingAsset), postProcessingProfilePath);
            }

            //fXData.postProcessingParams.postProcessProfile = AssetDatabase.LoadAssetAtPath(postProcessingProfilePath, typeof(PostProcessProfile)) as PostProcessProfile;
            //EditorUtility.SetDirty(postProcessVolume.profile);
#endif

            AssetDatabase.SaveAssets();
        }

        public static void InstantiateNewSceneSettings()
        {
            GameObject TerraWorldGO = TTerraWorldManager.IsMainTerraworldGameObject;

            if (TerraWorldGO == null)
                throw new Exception("No TerraWorld Game Object Found!");

            if (TTerraWorldManager.SceneSettingsGO1 != null)
                MonoBehaviour.DestroyImmediate(TTerraWorldManager.SceneSettingsGO1);

            TResourcesManager.LoadSceneSettingsPrefab();
            GameObject _sceneSettingsGO = MonoBehaviour.Instantiate(TResourcesManager.sceneSettingsPrefab);
            _sceneSettingsGO.name = "Scene Settings";
            _sceneSettingsGO.transform.parent = TerraWorldGO.transform;
            _sceneSettingsGO.layer = LayerMask.NameToLayer("TransparentFX");
            MoveResourcesToWorkDirectory();

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessingManager.SetPostProcessing(true);
#endif
        }


    }
#endif
}
#endif
#endif

