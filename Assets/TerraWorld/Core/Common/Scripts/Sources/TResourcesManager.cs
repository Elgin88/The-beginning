﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using TerraUnity.Runtime;

public static class TResourcesManager
{
    //TerraWorld
    private static string pathCommon = "GUI/Buttons/Common/";
    private static string pathPersonal = "GUI/Buttons/Personal/";
    private static string pathProfessional = "GUI/Buttons/Professional/";
    private static string pathCurrent;

    public static Texture2D[] platformIcons, progressIcons, templateIcons, biomeIcons;
    public static Texture2D editIcon, mobileIcon, tabletIcon, pcIcon, consoleIcon, farmIcon, graphIcon, customGraphIcon;
    public static Texture2D areaIcon, heightmapIcon, colormapIcon, biomesIcon, renderingIcon, FXIcon, globalSettingsIcon, runtimeIcon,graph2Icon;
    public static Texture2D black, logoLite, logoPro, logoBG, moduleSettingsIcon, nextIcon, addIcon, deleteIcon, subtractIcon, directIcon, stopIcon, bgWhite;
    public static Texture2D searchIcon, icon3D, homeIcon, saveIcon, loadIcon, newIcon, locationIcon, exportPackageIcon, hoverLeftIcon, hoverRightIcon, syncIcon;
    public static Texture2D enableIcon, removeIcon, arrangeIcon, clearIcon, onIcon, offIcon;
    public static Texture2D watchIcon, resetIcon;
    public static Texture2D launchIcon, pauseIcon;
    public static Texture2D processorIcon, operatorIcon, maskOperatorIcon, extractorIcon, scatterIcon, terraMeshIcon;
    public static Texture2D heightmapIconMaster, colormapIconMaster, terrainLayerIcon;
    public static Texture2D progressMain, progress1, progress2, progress3, progress4, progress5, progress6, progress7, progress8, progress9, progress10, progress11, progress12;
    public static Texture2D VFXIcon, timeOfDayIcon, godRaysIcon, cloudsIcon, fogIcon, atmosphericScatteringIcon, snowIcon, windIcon, horizonIcon, waterIcon, postProcessingIcon, flatShadingIcon, colormapBlendingIcon;
    //Texture2D fxSunRise, fxSunSet;
    public static Texture2D worldIcon, terrainIcon;
    //Texture2D proceduralTemplateIcon, GISTemplateIcon;
    public static Texture2D forestIcon, grasslandsIcon, desertIcon, tundraIcon, blankTemplateIcon;
    public static Texture2D selectIcon;
    public static Texture2D sadFaceIcon;
    public static Texture2D mountainsIcon;
    public static Texture2D facebookIcon, youtubeIcon, linkedinIcon, redditIcon, twitterIcon, websiteIcon, discordIcon;
    public static Texture2D versionUpdateIcon,SupportIcon;
    public static Font digitalFont;

    //HelperUI
    public static Texture2D cautionIcon,MinMax;
    public static Texture2D worldViewIcon, centerViewIcon, topDownViewIcon;
    //public static Texture2D alertIcon, boundsIcon;

    //InteractiveMap
    public static Texture2D OSMLegend;
    public static Texture2D centerCross;
    public static Material mat;

    //ProjectLauncher
    public static Texture2D logo1;
    public static Texture2D introImage;
    //public static UnityEngine.Object videoObject;

    //SearchResultsDisplay
    public static Texture2D BG;
    public static Texture2D placeMarker;

    ////BoundingBox
    //public static Material boundingBoxMaterial;

    //TerrainGenerator
    public static Texture2D snowAlbedo;
    public static Texture2D snowNormalmap;
    public static Texture2D snowMaskmap;
    public static Texture2D noise;

    //TimeOfDay
    public static GameObject nightLightsPrefab;


    // World Resources
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    // Scene Settings Manager
    public static GameObject sceneSettingsPrefab;

    // Clouds Manager
    public static Material cloudsMaterial;
    public static GameObject cloudPrefab;
    public static Mesh cloudMesh;
    public static Mesh cloudMeshCustom;
    public static string cloudMeshCustomModelPath;
    public static List<string> cloudsMeshNames;
    public static List<string> cloudsMeshPaths;
    //public static GameObject subEmitterPrefab;
    // Time Of Day
    public static Material skyMat;
    public static GameObject starsPrefab;
    // God Rays
    public static Material godRaysMaterial;
    // Horizon Fog
    public static Material volumetricHorizonMaterial;
    // Terrain
    public static Material TerraFormerMaterial { get => GetDefaultTerrainMaterial(); }
    public static Material TerraFormerMaterialBG { get => GetDefaultTerrainMaterialBG(); }
    // Post Processing
#if UNITY_POST_PROCESSING_STACK_V2
    public static PostProcessProfile postProcessProfile;
#endif

    public static List<string> LoadAllResources ()
    {
        List<string> result = new List<string>();

        try
        {
            LoadTerraWorldUIResources();
            LoadSceneSettingsPrefab();
            LoadSnowResources();
            LoadCloudsResources();
            //LoadBoundingBoxResources();
            //LoadHelperUIResources();
            LoadTimeOfDayResources();
            LoadGodRaysResources();
            LoadHorizonFogResources();

#if UNITY_POST_PROCESSING_STACK_V2
            LoadPostProcessingResources();
#endif
            LoadInteractiveMapResources();
            LoadProjectLauncherResources();
            LoadSearchResultResources();

#if TERRAWORLD_XPRO
            LoadXGgraphResources();
#endif
}
        catch (Exception e)
        {
            result.Add(e.Message);
        }

        return result;
    }

    public static void LoadTerraWorldUIResources()
    {
        // Common
        if (digitalFont == null) digitalFont = Resources.Load<Font>("GUI/Fonts/DS-DIGI");
        if (logoLite == null) logoLite = Resources.Load("GUI/Logo/TW-Lite-Logo") as Texture2D;
        if (logoPro == null) logoPro = Resources.Load("GUI/Logo/TW-Pro-Logo") as Texture2D;
        if (logoBG == null) logoBG = Resources.Load("GUI/Logo/logo_BG") as Texture2D;
        if (black == null) black = Resources.Load("GUI/Logo/Black") as Texture2D;

        if (icon3D == null) icon3D = Resources.Load(pathCommon + "3DIcon") as Texture2D;
        if (cautionIcon == null) cautionIcon = Resources.Load(pathCommon + "Caution") as Texture2D;
        if (MinMax == null) MinMax = Resources.Load(pathCommon + "minmax") as Texture2D;
        if (launchIcon == null) launchIcon = Resources.Load(pathCommon + "Launch") as Texture2D;
        if (mountainsIcon == null) mountainsIcon = Resources.Load(pathCommon + "Mountains") as Texture2D;
        if (onIcon == null) onIcon = Resources.Load(pathCommon + "On") as Texture2D;
        if (offIcon == null) offIcon = Resources.Load(pathCommon + "Off") as Texture2D;
        if (stopIcon == null) stopIcon = Resources.Load(pathCommon + "Stop") as Texture2D;
        if (directIcon == null) directIcon = Resources.Load(pathCommon + "Direct") as Texture2D;
        if (removeIcon == null) removeIcon = Resources.Load(pathCommon + "Remove") as Texture2D;
        if (heightmapIconMaster == null) heightmapIconMaster = Resources.Load(pathCommon + "HeightmapOut") as Texture2D;
        if (colormapIconMaster == null) colormapIconMaster = Resources.Load(pathCommon + "ColormapOut") as Texture2D;
        if (terrainLayerIcon == null) terrainLayerIcon = Resources.Load(pathCommon + "LayerOut") as Texture2D;
        if (worldViewIcon == null) worldViewIcon = Resources.Load(pathCommon + "World View") as Texture2D;
        if (centerViewIcon == null) centerViewIcon = Resources.Load(pathCommon + "Center View") as Texture2D;
        if (topDownViewIcon == null) topDownViewIcon = Resources.Load(pathCommon + "TopDown View") as Texture2D;

#if UNITY_EDITOR
        if (UnityEditor.EditorGUIUtility.isProSkin)
            pathCurrent = pathProfessional;
        else
            pathCurrent = pathPersonal;
#endif

        // Tabs
        if (areaIcon == null) areaIcon = Resources.Load(pathCurrent + "Tabs/Area") as Texture2D;
        if (heightmapIcon == null) heightmapIcon = Resources.Load(pathCurrent + "Tabs/Heightmap") as Texture2D;
        if (colormapIcon == null) colormapIcon = Resources.Load(pathCurrent + "Tabs/Colormap") as Texture2D;
        if (biomesIcon == null) biomesIcon = Resources.Load(pathCurrent + "Tabs/Biomes") as Texture2D;
        if (renderingIcon == null) renderingIcon = Resources.Load(pathCurrent + "Tabs/Rendering") as Texture2D;
        if (FXIcon == null) FXIcon = Resources.Load(pathCurrent + "Tabs/FX") as Texture2D;
        if (globalSettingsIcon == null) globalSettingsIcon = Resources.Load(pathCurrent + "Tabs/Global Settings") as Texture2D;
        if (runtimeIcon == null) runtimeIcon = Resources.Load(pathCurrent + "Tabs/Runtime") as Texture2D;

        // Area
        if (searchIcon == null) searchIcon = Resources.Load(pathCurrent + "Area/Search") as Texture2D;
        if (locationIcon == null) locationIcon = Resources.Load(pathCurrent + "Area/Location") as Texture2D;
        if (saveIcon == null) saveIcon = Resources.Load(pathCurrent + "Area/Save") as Texture2D;
        if (loadIcon == null) loadIcon = Resources.Load(pathCurrent + "Area/Load") as Texture2D;
        if (newIcon == null) newIcon = Resources.Load(pathCurrent + "Area/New") as Texture2D;
        if (exportPackageIcon == null) exportPackageIcon = Resources.Load(pathCurrent + "Area/ExportPackage") as Texture2D;
        if (sadFaceIcon == null) sadFaceIcon = Resources.Load(pathCurrent + "Area/SadFace") as Texture2D;
        if (hoverLeftIcon == null) hoverLeftIcon = Resources.Load(pathCurrent + "Area/Left") as Texture2D;
        if (hoverRightIcon == null) hoverRightIcon = Resources.Load(pathCurrent + "Area/Right") as Texture2D;
        if (syncIcon == null) syncIcon = Resources.Load(pathCurrent + "Area/Sync") as Texture2D;
        if (placeMarker == null) placeMarker = Resources.Load(pathCurrent + "Area/Marker") as Texture2D;

        // Footer
        if (versionUpdateIcon == null) versionUpdateIcon = Resources.Load(pathCurrent + "Footer/New_Version") as Texture2D;
        if (SupportIcon == null) SupportIcon = Resources.Load(pathCurrent + "Footer/Support") as Texture2D;

        // FX
        if (VFXIcon == null) VFXIcon = Resources.Load(pathCurrent + "FX/VFX") as Texture2D;
        if (timeOfDayIcon == null) timeOfDayIcon = Resources.Load(pathCurrent + "FX/DayNight") as Texture2D;
        if (godRaysIcon == null) godRaysIcon = Resources.Load(pathCurrent + "FX/GodRays") as Texture2D;
        if (cloudsIcon == null) cloudsIcon = Resources.Load(pathCurrent + "FX/Clouds") as Texture2D;
        if (fogIcon == null) fogIcon = Resources.Load(pathCurrent + "FX/Fog") as Texture2D;
        if (atmosphericScatteringIcon == null) atmosphericScatteringIcon = Resources.Load(pathCurrent + "FX/Atmosphere") as Texture2D;
        if (snowIcon == null) snowIcon = Resources.Load(pathCurrent + "FX/Snow") as Texture2D;
        if (windIcon == null) windIcon = Resources.Load(pathCurrent + "FX/Wind") as Texture2D;
        if (horizonIcon == null) horizonIcon = Resources.Load(pathCurrent + "FX/Horizon") as Texture2D;
        if (waterIcon == null) waterIcon = Resources.Load(pathCurrent + "FX/Water") as Texture2D;
        if (postProcessingIcon == null) postProcessingIcon = Resources.Load(pathCurrent + "FX/PostProcessing") as Texture2D;
        if (flatShadingIcon == null) flatShadingIcon = Resources.Load(pathCurrent + "FX/FlatShading") as Texture2D;
        if (colormapBlendingIcon == null) colormapBlendingIcon = Resources.Load(pathCurrent + "FX/ColormapBlending") as Texture2D;
        if (editIcon == null) editIcon = Resources.Load(pathCurrent + "FX/Edit") as Texture2D;
        if (resetIcon == null) resetIcon = Resources.Load(pathCurrent + "FX/Reset") as Texture2D;

        // Graph
        if (extractorIcon == null) extractorIcon = Resources.Load(pathCurrent + "Graph/Extractor") as Texture2D;
        if (scatterIcon == null) scatterIcon = Resources.Load(pathCurrent + "Graph/Scatter") as Texture2D;
        if (terraMeshIcon == null) terraMeshIcon = Resources.Load(pathCurrent + "Graph/MeshGen") as Texture2D;
        if (processorIcon == null) processorIcon = Resources.Load(pathCurrent + "Graph/Processor") as Texture2D;
        if (operatorIcon == null) operatorIcon = Resources.Load(pathCurrent + "Graph/Operator") as Texture2D;
        if (maskOperatorIcon == null) maskOperatorIcon = Resources.Load(pathCurrent + "Graph/MaskOperator") as Texture2D;
        if (clearIcon == null) clearIcon = Resources.Load(pathCurrent + "Graph/Clear") as Texture2D;
        if (watchIcon == null) watchIcon = Resources.Load(pathCurrent + "Graph/Watch") as Texture2D;

        // Social
        if (facebookIcon == null) facebookIcon = Resources.Load(pathCurrent + "Social/Facebook") as Texture2D;
        if (youtubeIcon == null) youtubeIcon = Resources.Load(pathCurrent + "Social/Youtube") as Texture2D;
        if (redditIcon == null) redditIcon = Resources.Load(pathCurrent + "Social/Reddit") as Texture2D;
        if (linkedinIcon == null) linkedinIcon = Resources.Load(pathCurrent + "Social/Linkedin") as Texture2D;
        if (twitterIcon == null) twitterIcon = Resources.Load(pathCurrent + "Social/Twitter") as Texture2D;
        if (websiteIcon == null) websiteIcon = Resources.Load(pathCurrent + "Social/Website") as Texture2D;
        if (discordIcon == null) discordIcon = Resources.Load(pathCurrent + "Social/Discord") as Texture2D;

        // Terrain Rendering
        if (resetIcon == null) resetIcon = Resources.Load(pathCurrent + "Terrain Rendering/Reset") as Texture2D;
        if (terrainIcon == null) terrainIcon = Resources.Load(pathCurrent + "Terrain Rendering/Terrain") as Texture2D;

        //if (forestIcon == null) forestIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/Biome_Forest") as Texture2D;
        //if (grasslandsIcon == null) grasslandsIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/Biome_Grasslands") as Texture2D;
        //if (desertIcon == null) desertIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/Biome_Desert") as Texture2D;
        //if (tundraIcon == null) tundraIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/Biome_Tundra") as Texture2D;
        //if (blankTemplateIcon == null) blankTemplateIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/Biome_New") as Texture2D;
        //if (selectIcon == null) selectIcon = Resources.Load("Editor/GUI/Buttons/Area/Personal/SelectBiome") as Texture2D;
        //if ( moduleSettingsIcon         == null )  moduleSettingsIcon          = Resources.Load("Editor/GUI/Buttons/Settings") as Texture2D;
        //if ( nextIcon                   == null )  nextIcon                    = Resources.Load("Editor/GUI/Buttons/Next") as Texture2D;
        //if ( addIcon                    == null )  addIcon                     = Resources.Load("Editor/GUI/Buttons/Add") as Texture2D;
        //if ( deleteIcon                 == null )  deleteIcon                  = Resources.Load("Editor/GUI/Buttons/Delete") as Texture2D;
        //if ( subtractIcon               == null )  subtractIcon                = Resources.Load("Editor/GUI/Buttons/Subtract") as Texture2D;
        //if ( bgWhite                    == null )  bgWhite                     = Resources.Load("Editor/GUI/Background/BG_White") as Texture2D;
        //if ( enableIcon                 == null )  enableIcon                  = Resources.Load("Editor/GUI/Buttons/Enable") as Texture2D;
        //if ( arrangeIcon                == null )  arrangeIcon                 = Resources.Load("Editor/GUI/Buttons/Arrange") as Texture2D;
        //if ( pauseIcon                  == null )  pauseIcon                   = Resources.Load("Editor/GUI/Buttons/Pause") as Texture2D;
        //if ( progress1                  == null )  progress1                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress1") as Texture2D;
        //if ( progress2                  == null )  progress2                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress2") as Texture2D;
        //if ( progress3                  == null )  progress3                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress3") as Texture2D;
        //if ( progress4                  == null )  progress4                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress4") as Texture2D;
        //if ( progress5                  == null )  progress5                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress5") as Texture2D;
        //if ( progress6                  == null )  progress6                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress6") as Texture2D;
        //if ( progress7                  == null )  progress7                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress7") as Texture2D;
        //if ( progress8                  == null )  progress8                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress8") as Texture2D;
        //if ( progress9                  == null )  progress9                   = Resources.Load("Editor/GUI/Buttons/Loading/Progress9") as Texture2D;
        //if ( progress10                 == null )  progress10                  = Resources.Load("Editor/GUI/Buttons/Loading/Progress10") as Texture2D;
        //if ( progress11                 == null )  progress11                  = Resources.Load("Editor/GUI/Buttons/Loading/Progress11") as Texture2D;
        //if ( progress12                 == null )  progress12                  = Resources.Load("Editor/GUI/Buttons/Loading/Progress12") as Texture2D;
        //if ( progressIcons              == null )  progressIcons               = new Texture2D[12] { progress1, progress2, progress3, progress4, progress5, progress6, progress7, progress8, progress9, progress10, progress11, progress12 };
        //if ( worldIcon                  == null )  worldIcon                   = Resources.Load("Editor/GUI/Buttons/Footer/World") as Texture2D;
        //if (mobileIcon == null) mobileIcon = Resources.Load("Editor/GUI/Buttons/Platforms/mobile") as Texture2D;
        //if (tabletIcon == null) tabletIcon = Resources.Load("Editor/GUI/Buttons/Platforms/tablet") as Texture2D;
        //if (pcIcon == null) pcIcon = Resources.Load("Editor/GUI/Buttons/Platforms/pc") as Texture2D;
        //if (consoleIcon == null) consoleIcon = Resources.Load("Editor/GUI/Buttons/Platforms/console") as Texture2D;
        //if (farmIcon == null) farmIcon = Resources.Load("Editor/GUI/Buttons/Platforms/farm") as Texture2D;
        //if (graphIcon == null) graphIcon = Resources.Load("Editor/GUI/Buttons/Platforms/graph") as Texture2D;
        //if (customGraphIcon == null) customGraphIcon = Resources.Load("Editor/GUI/Buttons/Platforms/customGraph") as Texture2D;
        //if ( platformIcons              == null )  platformIcons               = new Texture2D[6] { mobileIcon, tabletIcon, pcIcon, consoleIcon, farmIcon, graphIcon };
        //if (platformIcons == null) platformIcons = new Texture2D[2] { pcIcon, mobileIcon };
        //if (homeIcon == null) homeIcon = Resources.Load("Editor/GUI/Buttons/Home") as Texture2D;
    }

    private static UnityEngine.Object GetResource (string fileName, Type type)
    {
        UnityEngine.Object result = null;
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssetPaths)
        {
            if (!assetPath.Contains("TResource Manager")) continue;
            if (!assetPath.Contains(fileName)) continue;
            UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)) as UnityEngine.Object;

            if (resource != null)
                if (resource.GetType() == type)
                    return resource;
        }

        return result;
    }

    public static void LoadSceneSettingsPrefab ()
    {
        if (sceneSettingsPrefab == null) sceneSettingsPrefab = GetResource("Scene Settings_250", typeof(GameObject)) as GameObject;
    }

    public static void LoadSnowResources()
    {
        if (snowAlbedo == null )  snowAlbedo = GetResource("Snow_Albedo_250", typeof(Texture2D)) as Texture2D;
        if (snowNormalmap == null )  snowNormalmap = GetResource("Snow_Normal_250", typeof(Texture2D)) as Texture2D;
        if (snowMaskmap == null )  snowMaskmap = GetResource("Snow_Mask_250", typeof(Texture2D)) as Texture2D;
        if (noise == null) noise = GetResource("Snow_Height_250", typeof(Texture2D)) as Texture2D;
        //if (noise == null) noise = GetResource("Noise_Smooth_250", typeof(Texture2D)) as Texture2D;
    }

    public static void LoadCloudsResources()
    {
        if (cloudsMaterial == null) cloudsMaterial = GetResource("Clouds_250", typeof(Material)) as Material;
        if (cloudPrefab == null) cloudPrefab = GetResource("Clouds_250", typeof(GameObject)) as GameObject;
        //if (subEmitterPrefab == null) subEmitterPrefab = GetResource("Snow Particles", typeof(GameObject)) as GameObject;

#if UNITY_EDITOR
        if (cloudMesh == null)
        {
            string fullPath = Path.GetFullPath(Application.dataPath);
            string[] modelPath = Directory.GetFiles(fullPath, "CloudShape.fbx", SearchOption.AllDirectories);

            if (modelPath != null && modelPath.Length > 0 && !string.IsNullOrEmpty(modelPath[0]))
                cloudMesh = GetMeshObject(TAddresses.GetProjectPath(modelPath[0]), "Cloud3");
                //cloudMesh = GetMeshObject(modelPath[0].Substring(modelPath[0].LastIndexOf("Assets")), "Cloud3");
            else
            {
                modelPath = Directory.GetFiles(fullPath, "CloudShape.FBX", SearchOption.AllDirectories);

                if (modelPath != null && modelPath.Length > 0 && !string.IsNullOrEmpty(modelPath[0]))
                    cloudMesh = GetMeshObject(TAddresses.GetProjectPath(modelPath[0]), "Cloud3");
                    //cloudMesh = GetMeshObject(modelPath[0].Substring(modelPath[0].LastIndexOf("Assets")), "Cloud3");
            }
        }

        string[] modelsPath = Directory.GetFiles(Path.GetFullPath(Application.dataPath), "Clouds.fbx", SearchOption.AllDirectories);

        if (modelsPath != null && modelsPath.Length > 0 && !string.IsNullOrEmpty(modelsPath[0]))
        {
            cloudMeshCustomModelPath = TAddresses.GetProjectPath(modelsPath[0]);
            cloudsMeshNames = GetMeshNames(cloudMeshCustomModelPath);
            cloudMeshCustom = GetMeshObject(cloudMeshCustomModelPath, "Chicken");
        }
#endif
    }

    private static Material GetDefaultTerrainMaterial()
    {
        return GetResource("TerraFormer_250", typeof(Material)) as Material;
    }

    private static Material GetDefaultTerrainMaterialBG()
    {
        return GetResource("BGTerraFormer_250", typeof(Material)) as Material;
    }

#if UNITY_EDITOR
    private static List<string> GetMeshNames(string modelPath)
    {
        List<string> meshNames = new List<string>();
        cloudsMeshPaths = new List<string>();

        if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

            foreach (UnityEngine.Object obj in assets)
            {
                if (obj is Mesh)
                {
                    Mesh subMesh = (Mesh)obj;
                    meshNames.Add(subMesh.name);
                    cloudsMeshPaths.Add(AssetDatabase.GetAssetPath(subMesh));
                }
            }

            meshNames.Add("Custom");
        }

        return meshNames;
    }

    public static Mesh GetMeshObject(string modelPath, string MeshName)
    {
        Mesh result = null;

        if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath) && !string.IsNullOrEmpty(MeshName))
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);

            foreach (UnityEngine.Object obj in assets)
            {
                if (obj is Mesh)
                {
                    Mesh subMesh = (Mesh)obj;
                    if (MeshName == subMesh.name) result = subMesh;
                }
            }
        }
        else
            result = null;

        return result;
    }
#endif

    //public static void LoadBoundingBoxResources()
    //{
    //    if (boundingBoxMaterial == null) boundingBoxMaterial = Resources.Load("Editor/Materials/Bounding Box") as Material;
    //}

    //public static void LoadHelperUIResources()
    //{
    //    if (alertIcon == null) alertIcon = Resources.Load("Editor/GUI/Buttons/Alert") as Texture2D;
    //    if (boundsIcon == null) boundsIcon = Resources.Load("Editor/GUI/Buttons/Bounds") as Texture2D;
    //}

    public static void LoadTimeOfDayResources()
    {
        if (skyMat == null) skyMat = GetResource("Sky_250", typeof(Material)) as Material;
        if (starsPrefab == null) starsPrefab = GetResource("Stars_250", typeof(GameObject)) as GameObject;
        if (nightLightsPrefab == null) nightLightsPrefab = GetResource("Night Lights_250", typeof(GameObject)) as GameObject;
    }

    public static void LoadGodRaysResources ()
    {
        if (godRaysMaterial == null) godRaysMaterial = GetResource("GodRays_250", typeof(Material)) as Material;
    }

    public static void LoadHorizonFogResources()
    {
        if (volumetricHorizonMaterial == null) volumetricHorizonMaterial = GetResource("VolumetricHorizon_250", typeof(Material)) as Material;
    }

#if UNITY_POST_PROCESSING_STACK_V2
    public static void LoadPostProcessingResources ()
    {
        if (postProcessProfile == null) postProcessProfile = GetResource("TerraWorld Post Processing_250", typeof(PostProcessProfile)) as PostProcessProfile;
    }
#endif

    public static void LoadInteractiveMapResources()
    {
        if (centerCross == null) centerCross = Resources.Load("GUI/Interactive Map/CenterCross") as Texture2D;
        if (OSMLegend == null) OSMLegend = Resources.Load("GUI/Interactive Map/OsmLegend") as Texture2D;
        if (mat == null) mat = (Material)Resources.Load("GUI/Interactive Map/CrossMat");
    }

    public static void LoadProjectLauncherResources()
    {
        //if (logo1 == null) logo1 = Resources.Load("GUI/Logo/logo") as Texture2D;
        if (introImage == null) introImage = Resources.Load("GUI/Launcher/Intro2") as Texture2D;
        //if (videoObject == null) videoObject = Resources.Load("GUI/Launcher/Intro_Movie") as UnityEngine.Object;
    }

    public static void LoadSearchResultResources()
    {
        if (BG == null) BG = Resources.Load("GUI/Background/BG") as Texture2D;
    }

#if TERRAWORLD_XPRO
    public static Texture2D AndImage;
    public static Texture2D OrImage;
    public static Texture2D XorImage;
    public static Texture2D NotImage;
    public static Texture2D SubImage;

    public static void LoadXGgraphResources()
    {
        if (AndImage == null) AndImage = Resources.Load("AndImage") as Texture2D;
        if (OrImage == null)  OrImage = Resources.Load("OrImage") as Texture2D;
        if (XorImage == null) XorImage = Resources.Load("XorImage") as Texture2D;
        if (NotImage == null) NotImage = Resources.Load("NotImage") as Texture2D;
        if (SubImage == null) SubImage = Resources.Load("SubImage") as Texture2D;
    }
#endif
}
#endif

