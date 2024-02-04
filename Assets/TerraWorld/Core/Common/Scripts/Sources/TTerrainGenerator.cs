#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using Mewlist.MassiveGrass;
using TerraUnity.Runtime;
using TerraUnity.Utils;
using TerraUnity.UI;
using System.Linq;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
#endif

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    [ExecuteAlways]
    public static class TTerrainGenerator
    {
        public static int progressID = -1;
        private static float percentageProgress = 0;

        private enum GeneratorStatus
        {
            Idle,
            InProgress,
            Error,
            ErrorInGraph
        }

        private struct GeneratorSteps
        {
            public bool MainTerrainsPath;
            public bool BGTerrainPath;
            public void Reset()
            {
                MainTerrainsPath = false;
                BGTerrainPath = false;
            }
        }

        private static string mainTerrainGameObjectName = "Terrain 0-0";
        public static float everestHeight = 8848;
        public static float offsetForNegetiveHeightHandling = 500;
        private static TTerraWorld _terraWorld;
        private static float baseMapDistance = 100000;
        private static GeneratorStatus _status = GeneratorStatus.Idle;
        private static GeneratorSteps _steps;
        public static float worldMinElevation = 0;
        public static float worldMaxElevation = 0;
        public static int _NodesCount = 0;
        //public static int xNodesCount = 0;
        //private static TMap mapRequest = null;

        /// <summary>
        /// Start Debug Placement
        /// </summary>
        //static System.Numerics.Vector3 p1 = System.Numerics.Vector3.Zero;
        //static System.Numerics.Vector3 p2 = System.Numerics.Vector3.Zero;
        //static System.Numerics.Vector3 p3 = System.Numerics.Vector3.Zero;
        /// <summary>
        /// End Debug Placement
        /// </summary>

        private static Material terrainMaterial;
        private static Action<TTerrain, Terrain, List<GameObject>, List<GameObject>> _OnFinished;
        public static string devTeamMessage = "";

        // public static TTerraWorld TerraWorld { get => _terraWorld; set => _terraWorld = value; }

        public static bool WorldInProgress
        {
            get
            {
                if (_status == GeneratorStatus.InProgress) return true;
                else return false;
            }
        }

        public static bool Idle
        {
            get
            {
                if (_status == GeneratorStatus.Idle) return true;
                else return false;
            }
        }

        public static bool Error
        {
            get
            {
                if (_status == GeneratorStatus.Error) return true;
                else return false;
            }
        }

        public static bool ErrorInGraph
        {
            get
            {
                if (_status == GeneratorStatus.ErrorInGraph) return true;
                else return false;
            }
        }

        public static void SetStatusToError()
        {
            _status = GeneratorStatus.Error;
            CloseProgressWindow(progressID);
        }

        public static void SetStatusToErrorInGraph()
        {
            _status = GeneratorStatus.ErrorInGraph;
            CloseProgressWindow(progressID);
        }

        public static void SetStatusToOnProgress()
        {
            _status = GeneratorStatus.InProgress;
        }

        public static void SetStatusToIdle()
        {
            _status = GeneratorStatus.Idle;
            CloseProgressWindow(progressID);
        }

        static float creationProgress = 0;
        public static float? Progress
        {
            get
            {
                if (_terraWorld != null && _status == GeneratorStatus.InProgress)
                {
                    creationProgress = Mathf.Lerp(creationProgress, _terraWorld.ProgressPersentage, Time.deltaTime * 0.1f);
                    if (creationProgress > _terraWorld.ProgressPersentage) creationProgress = _terraWorld.ProgressPersentage;
                    return (creationProgress);
                }
                else
                {
                    return null;
                }
            }
        }

        public static string ProgressDetails
        {
            get
            {
                if (Progress != null && Progress > 0 && Progress < 30)
                    return "Fetching Raw Data From Servers ...";
                else if (Progress != null && Progress > 30 && Progress < 95)
                    return "Analyzing Graph ...";
                else
                    return string.Empty;
            }
        }

        //public static bool isWorldUpdateAndReplace { get { return TProjectSettings.IsReplaceAndUpdate; } }
        public static TMapManager.ElevationMapServer _elevationSource = TMapManager.ElevationMapServer.ESRI;
        public static TMapManager.ImageryMapServer _imagerySource = TMapManager.ImageryMapServer.ESRI;
        public static TMapManager.LandcoverMapServer _landcoverSource = TMapManager.LandcoverMapServer.OSM;

        public static GameObject BGTerrainGO;
        //private static TerrainRenderingParams renderingParams;
        // private static bool BGTerrainGenerated = false;
        private static TArea BGArea;
        private static TerrainData BGTData;
        private static float BGAreaWidth;
        private static float BGAreaLength;
        private static TImage bgImage = null;

        // Just for Preview Terrain
        // public static void CreatePreviewWorld(TTerraWorld TW)
        // {
        //     TW.UpdateTerraWorld(ApplyOnTerrain);
        // }
        //
        // public static void OnIdle()
        // {
        //     ResetNodesProgress();
        // }

        public static void ResetNodesProgress()
        {
            for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
                for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
                    TTerraWorld.WorldGraph.graphList[i].nodes[j].Progress = 0;
        }

        public static void CreateWorld(Action<TTerrain, Terrain, List<GameObject>, List<GameObject>> onFinished)
        {
            try
            {
                TDebug.Reset();
                TDebug.TraceMessage();
#if TERRAWORLD_XPRO
                _NodesCount = TTerraworldGenerator.XGraph.GetNodesCount();

                if (_NodesCount > 0)
                {
                    TTerraworldGenerator.XGraph.ResetNodesStatus();
                    TTerraworldGenerator.XGraph.CheckConnections(false);
                }
#else
                _NodesCount = TTerraWorld.WorldGraph.GetNodeCounts();
                if (_NodesCount > 0)
                {
                    TTerraWorld.WorldGraph.ResetGraphsStatus();
                    TTerraWorld.WorldGraph.CheckConnections();
                }
#endif

                if (_NodesCount == 0)
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "There are no modules / nodes in the graph to generate world!", "Ok");
                    return;
                }

                SetStatusToOnProgress();
                _OnFinished = onFinished;
                TDebug.TraceMessage("Fetching Data From Servers...");
                _steps.Reset();
                InitBGTerrain();
                //InitMainTerrains();
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
        }

        private static void InitBGTerrain()
        {
            TDebug.TraceMessage();
            _steps.BGTerrainPath = false;
            bgImage = null;

#if TERRAWORLD_XPRO
            TXTerrainNode TerrainNode = TTerraworldGenerator.XGraph.GetTerrainNode();

            if (TerrainNode != null)
            {
                if (TerrainNode.BGMountains)
                {
                    _steps.BGTerrainDone = false;
                    BGArea = new TArea(TTerraWorld.WorldArea.Top, TTerraWorld.WorldArea.Left, TTerraWorld.WorldArea.Bottom, TTerraWorld.WorldArea.Right);
                    BGArea.Update(BGArea.AreaSizeLat * TerrainNode.BGTerrainScaleMultiplier, BGArea.AreaSizeLon * TerrainNode.BGTerrainScaleMultiplier);
                    int BGTZoomLevelImagery = TMap.GetZoomLevel(TerrainNode.BGTerrainSatelliteImageResolution, BGArea);
                    int BGTZoomLevelElevation = TMap.GetZoomLevel(TerrainNode.BGTerrainHeightmapResolution, BGArea);
                    BGAreaWidth = BGArea.AreaSizeLon * 1000;
                    BGAreaLength = BGArea.AreaSizeLat * 1000;
                    TMap BGMap = new TMap(CreateBackgroundTerrainData, BGArea, BGTZoomLevelImagery, BGTZoomLevelElevation);
                }
                else
                {
                    _steps.BGTerrainDone = true;
                    //BGTerrainGenerated = true;
                }
            }
#else
            if (TTerraWorld.WorldGraph.RenderingDATA.BGMountains)
            {
                //BGTerrainGenerated = false;
                TerrainRenderingParams renderingParams = TTerraWorld.WorldGraph.RenderingDATA;
                _steps.BGTerrainPath = false;
                BGArea = new TArea(TTerraWorld.WorldArea.Top, TTerraWorld.WorldArea.Left, TTerraWorld.WorldArea.Bottom, TTerraWorld.WorldArea.Right);
                //BGArea.AreaSizeLat *= renderingParams.BGTerrainScaleMultiplier;
                BGArea.AreaSizeLon *= renderingParams.BGTerrainScaleMultiplier;
                //BGArea.Update(BGArea.AreaSizeLat * renderingParams.BGTerrainScaleMultiplier, BGArea.AreaSizeLon * renderingParams.BGTerrainScaleMultiplier);
                int BGTZoomLevelImagery = TMap.GetZoomLevel(renderingParams.BGTerrainSatelliteImageResolution, BGArea);
                int BGTZoomLevelElevation = TMap.GetZoomLevel(renderingParams.BGTerrainHeightmapResolution, BGArea);
                BGAreaWidth = BGArea.AreaSizeLon * 1000;
                BGAreaLength = BGArea.AreaSizeLat * 1000;
                TMap BGMap = new TMap(CreateBackgroundTerrainData, BGArea, BGTZoomLevelImagery, BGTZoomLevelElevation);
            }
            else
            {
                //_steps.BGTerrainPath = true;
                InitMainTerrains();
            }
#endif
        }

        private static void InitMainTerrains()
        {
            TDebug.TraceMessage();
            _steps.MainTerrainsPath = false;
            _terraWorld = new TTerraWorld();
            _terraWorld.UpdateTerraWorld(ApplyOnTerrain);
        }

        private static void CreateBackgroundTerrainData(TMap BGMap)
        {
            TDebug.TraceMessage();
            float[,] heightmap = BGMap.Heightmap.heightsData;
            // Create TerrainData
            BGTData = new TerrainData();

#if TERRAWORLD_XPRO
            TXTerrainNode TerrainNode = TTerraworldGenerator.XGraph.GetTerrainNode();

            if (TerrainNode != null)
            {
                BGTData.heightmapResolution = TerrainNode.BGTerrainHeightmapResolution;
                heightmap = THeightmapProcessors.ResampleHeightmap(heightmap, THeightmapProcessors.ResampleMode.DOWN, TerrainNode.BGTerrainHeightmapResolution + 1);
                float[,] _heightmap = THeightmapProcessors.NormalizeHeightmap(heightmap, -offsetForNegetiveHeightHandling, everestHeight);
                BGTData.SetHeights(0, 0, _heightmap);
                SetTerrainSize(BGAreaWidth, BGAreaLength, BGTData, TTerraWorldGraph.scaleFactor);
                bgImage = new TImage(TImageProcessors.ResetResolution(BGMap.Image.Image, TerrainNode.BGTerrainSatelliteImageResolution));
            }
#else
            TerrainRenderingParams renderingParams = TTerraWorld.WorldGraph.RenderingDATA;
            BGTData.heightmapResolution = renderingParams.BGTerrainHeightmapResolution;
            heightmap = THeightmapProcessors.ResampleHeightmap(heightmap, THeightmapProcessors.ResampleMode.DOWN, renderingParams.BGTerrainHeightmapResolution + 1);
            float[,] _heightmap = THeightmapProcessors.NormalizeHeightmap(heightmap, -offsetForNegetiveHeightHandling, everestHeight);
            BGTData.SetHeights(0, 0, _heightmap);
            SetTerrainSize(BGAreaWidth, BGAreaLength, BGTData, TTerraWorldGraph.scaleFactor);
            bgImage = new TImage(TImageProcessors.ResetResolution(BGMap.Image.Image, renderingParams.BGTerrainSatelliteImageResolution));
#endif
            _steps.BGTerrainPath = true;
            //CreateBackgroundTerrainObject();
            InitMainTerrains();
        }

        private static void CreateBackgroundTerrainObject()
        {
            TDebug.TraceMessage();
            if (!_steps.MainTerrainsPath || !_steps.BGTerrainPath) return;
            if (BGTData == null) return;

            if (File.Exists(_terraWorld.HeightmapPathBackground)) AssetDatabase.DeleteAsset(_terraWorld.HeightmapPathBackground);
            AssetDatabase.CreateAsset(BGTData, _terraWorld.HeightmapPathBackground);
            Texture2D satImage = bgImage.GetTextureRGBAAndSaveOnDisk("Background Terrain Layer Diffuse");

            TerrainLayer[] TL = new TerrainLayer[1];
            TL[0] = new TerrainLayer();
            TL[0].diffuseTexture = satImage;
            TL[0].tileSize = new Vector2(BGTData.size.x, BGTData.size.z);
            if (File.Exists(TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer BG.terrainlayer")) AssetDatabase.DeleteAsset(TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer BG.terrainlayer");
            AssetDatabase.CreateAsset(TL[0], TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer BG.terrainlayer");
            BGTData.terrainLayers = TL;

            BGTData.alphamapResolution = 32;
            float[,,] splatmap = TDetailTextureCollection.GetFilledAplphaMap(BGTData.alphamapResolution, 0);
            BGTData.SetAlphamaps(0, 0, splatmap);

            AssetDatabase.Refresh();

            //if (mainTerrainHeightmapData!= null) THeightmapProcessors.GetMinMaxElevationFromHeights(mainTerrainHeightmapData, out minElevation, out maxElevation);

            // Carve terrain center
            int fromCenterX = (int)(TUtils.InverseLerp(0, BGArea.AreaSizeLon, TTerraWorld.WorldArea.AreaSizeLon) * BGTData.heightmapResolution) - 2;
            int fromCenterY = (int)(TUtils.InverseLerp(0, BGArea.AreaSizeLat, TTerraWorld.WorldArea.AreaSizeLat) * BGTData.heightmapResolution) - 2;
            float[,] deformedBGHeightmap = new float[fromCenterX - 2, fromCenterY - 2];

            //for (int i = 0; i < fromCenterX - 2; i++)
            //    for (int j = 0; j < fromCenterY - 2; j++)
            //        deformedBGHeightmap[i, j] = (worldMinElevation + (offsetForNegetiveHeightHandling * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration) - 20) / ((everestHeight + offsetForNegetiveHeightHandling) * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration);

            for (int i = 0; i < fromCenterX - 2; i++)
                for (int j = 0; j < fromCenterY - 2; j++)
                    deformedBGHeightmap[i, j] = (worldMinElevation + offsetForNegetiveHeightHandling - 20) / (everestHeight + offsetForNegetiveHeightHandling);

            BGTData.SetHeights((int)(BGTData.heightmapResolution / 2 - fromCenterX / 2) + 1, (int)(BGTData.heightmapResolution / 2 - fromCenterY / 2) + 1, deformedBGHeightmap);
            SetTerrainSize(BGAreaWidth, BGAreaLength, BGTData, TTerraWorldGraph.scaleFactor);

            GameObject oldWorld = TTerraWorldManager.BackgroundTerrainGO;
            if (oldWorld != null) UnityEngine.Object.DestroyImmediate(oldWorld);

            // Create Terrain Object
            BGTerrainGO = Terrain.CreateTerrainGameObject(BGTData);
            BGTerrainGO.name = "Background Terrain";

            // Set Static flag for this object so that CullingGroup will take occluders into account
            // Disable Lightmap Static to bypass lightmapping
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(BGTerrainGO);

#if UNITY_2019_2_OR_NEWER
            flags &= ~(StaticEditorFlags.ContributeGI | StaticEditorFlags.OccludeeStatic);
#else
            flags &= ~(StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic);
#endif

            GameObjectUtility.SetStaticEditorFlags(BGTerrainGO, flags);

            // Reference to the Terrain System
            Terrain BGTerrain = BGTerrainGO.GetComponent<Terrain>();
            TerrainRenderingManager.SetTerrainMaterialBGDefault(BGTerrain);

#if UNITY_2019_1_OR_NEWER
            BGTerrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
#else
            BGTerrain.castShadows = true;
#endif

#if TERRAWORLD_XPRO
            TXTerrainNode TerrainNode = TTerraworldGenerator.XGraph.GetTerrainNode();

            BGTerrain.heightmapPixelError = TerrainNode.BGTerrainPixelError;
            BGTerrain.basemapDistance = baseMapDistance;

            BGTerrain.GetComponent<TerrainCollider>().enabled = false;
            BGTerrain.Flush();
            BGTerrain.drawHeightmap = true;

            BGTerrainGO.transform.parent = TTerraWorldManager.MainTerrainGO.transform;
            BGTerrainGO.transform.position = new Vector3(-(BGTerrain.terrainData.size.x / 2f), TerrainNode.BGTerrainOffset, -(BGTerrain.terrainData.size.z / 2f));
#else
            TerrainRenderingParams renderingParams = TTerraWorld.WorldGraph.RenderingDATA;

            BGTerrain.heightmapPixelError = renderingParams.BGTerrainPixelError;
            BGTerrain.basemapDistance = baseMapDistance;

            BGTerrain.GetComponent<TerrainCollider>().enabled = false;
            BGTerrain.Flush();
            BGTerrain.drawHeightmap = true;

            BGTerrainGO.transform.parent = TTerraWorldManager.MainTerrainGO.transform;
            BGTerrainGO.transform.position = new Vector3(-(BGTerrain.terrainData.size.x / 2f), renderingParams.BGTerrainOffset, -(BGTerrain.terrainData.size.z / 2f));
#endif
        }

        private static void SetTerrainSize(float areaWidthMeters, float areaLengthMeters, TerrainData tData, float scaleFactor)
        {
            //Vector3 terrainSize = new Vector3(areaWidthMeters, (everestHeight + offsetForNegetiveHeightHandling) * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration, areaLengthMeters) * scaleFactor;
            Vector3 terrainSize = new Vector3(areaWidthMeters, everestHeight + offsetForNegetiveHeightHandling, areaLengthMeters) * scaleFactor;
            tData.size = terrainSize;
        }

        public static void CancelProgress()
        {
            _status = GeneratorStatus.Idle;
            CloseProgressWindow(progressID);
        }

        public static void ApplyOnTerrain(TTerraWorld TW)
        {
            TDebug.TraceMessage();

            try
            {
                _terraWorld = TW;

                for (int i = 0; i < _terraWorld._terrains.Count; i++)
                {
                    TTerrain t = _terraWorld._terrains[i];
                    Terrain ter = GenerateTerrain(t, i);
                }
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
        }

        private static Terrain GenerateTerrain(TTerrain t, int index)
        {
            TDebug.TraceMessage();
            Terrain UnityTerrain = InitializeTerrain(t, index);
            ApplyTerrrain(t, UnityTerrain);

            return UnityTerrain;
        }

        private static Terrain InitializeTerrain(TTerrain TWTerrainData, int index)
        {
            TDebug.TraceMessage();

            //if (isWorldUpdateAndReplace)
            //{
            //string oldWorldPath = Path.GetDirectoryName(_terraWorld.HeightmapPath);
            GameObject oldWorld = TTerraWorldManager.MainTerrainGO;
            if (oldWorld != null) UnityEngine.Object.DestroyImmediate(oldWorld);
            //if (Directory.Exists(oldWorldPath)) Directory.Delete(oldWorldPath, true);
            //}

            TerrainData tData = new TerrainData();
            if (File.Exists(_terraWorld.HeightmapPath)) AssetDatabase.DeleteAsset(_terraWorld.HeightmapPath);

            AssetDatabase.CreateAsset(tData, _terraWorld.HeightmapPath);
            GameObject _mainTerrainGO = Terrain.CreateTerrainGameObject(tData);
            //worldObject.transform.position -= new Vector3(0, offsetForNegetiveHeightHandling, 0);
            _mainTerrainGO.transform.parent = TTerraWorldManager.CreateAndGetTerraworldGameObject.transform;
            _mainTerrainGO.name = mainTerrainGameObjectName;
            _mainTerrainGO.AddComponent<TTerraWorldTerrainManager>();
            _mainTerrainGO.AddComponent<DetectTerrainChanges>();
            _mainTerrainGO.AddComponent<HookTerrainMaterialMasks>();

            // Set Static flag for this object so that CullingGroup will take occluders into account
            // Disable Lightmap Static to bypass lightmapping
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(_mainTerrainGO);
#if UNITY_2019_2_OR_NEWER
            flags &= ~(StaticEditorFlags.ContributeGI | StaticEditorFlags.OccludeeStatic);
#else
            flags &= ~(StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic);
#endif
            GameObjectUtility.SetStaticEditorFlags(_mainTerrainGO, flags);

            // We can expose these general parameters for terrain in UI later!
            Terrain terrain = _mainTerrainGO.GetComponent<Terrain>();
            TerrainRenderingManager.SetTerrainMaterialMAINDefault(terrain);
            terrain.heightmapPixelError = TWTerrainData.PixelError;

#if UNITY_2019_1_OR_NEWER
            terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
#else
            terrain.castShadows = true;
#endif

            terrain.bakeLightProbesForTrees = false; // This is essential for GPU Instancing on terrain vegetation and better performance overall
            terrain.preserveTreePrototypeLayers = true;
            terrain.GetComponent<TerrainCollider>().enabled = true;
            //terrain.heightmapPixelError = TerrainRenderingManager.TerrainPixelError;

            return terrain;
        }

        public static void ShowProgressWindow(string action, float percentage, int ID)
        {
            if (percentage > percentageProgress) percentageProgress = percentage;
            TProgressBar.DisplayProgressBar("TERRAWORLD", action, percentageProgress, ID);
        }

        public static void CloseProgressWindow(int ID)
        {
            percentageProgress = 0;
            TProgressBar.RemoveProgressBar(ID);
        }

        private static void ApplyTerrrain(TTerrain tTerrainData, Terrain terrain)
        {
            TDebug.TraceMessage("Generating Terrain");
            _steps.MainTerrainsPath = true;
            terrain.heightmapPixelError = tTerrainData.PixelError;

            //ShowProgressWindow("Environmental Settings...", 0.5f, progressID);
            //SceneSettingsManager.SwitchFX(TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams);

            ApplyLakeOnTerrain(terrain, tTerrainData, out List<GameObject> lakeList);

            ShowProgressWindow("Generating Terrain...", 0.5f, progressID);
            UpdateTerrainHeightmap(terrain, tTerrainData);

            if (_steps.BGTerrainPath)
            {
                ShowProgressWindow("Generating Background Terrain...", 0.55f, progressID);
                CreateBackgroundTerrainObject();
            }

            ShowProgressWindow("Generating Splatmaps...", 0.6f, progressID);
            UpdateTerrainImagery(tTerrainData, terrain);

            ShowProgressWindow("Generating Meshes...", 0.65f, progressID);
            ApplyRiversOnTerrain(terrain, tTerrainData, out List<GameObject> riverList);
            ApplyGridsOnTerrain(terrain, tTerrainData);

            ShowProgressWindow("Generating Objects & Instances...", 0.675f, progressID);
            ApplyObjectsOnTerrain(terrain, tTerrainData);

            //WorldSnapshot.GetTerrainSnapshot(terrain, Path.GetFullPath(TTerraWorld.WorkDirectoryLocalPath + "World Snapshot.png"), 1024, ~0);

            ShowProgressWindow("Generating Grass & Plants...", 0.7f, progressID);
            ApplyGrassOnTerrain(terrain, tTerrainData);

            _OnFinished.Invoke(tTerrainData, terrain, lakeList, riverList);
        }

        public static void UpdateTerrainHeightmap(Terrain terrain, TTerrain tTerrainData)
        {
            TDebug.TraceMessage("UpdateTerrainHeightmap");
            float[,] heightmap = tTerrainData.Heightmap.heightsData;
            float areaSizeX = tTerrainData.Map._area.AreaSizeLon * 1000;
            float areaSizeZ = tTerrainData.Map._area.AreaSizeLat * 1000;
            TerrainData tData = terrain.terrainData;
            tData.heightmapResolution = heightmap.GetLength(0);

            // Get Min & Max real-world elevations from heightmap
            THeightmapProcessors.GetMinMaxElevationFromHeights(heightmap, out worldMinElevation, out worldMaxElevation);

            // Normalize real-world elevation values between 0 & 1
            float[,] _heightmap = THeightmapProcessors.NormalizeHeightmap(heightmap, -offsetForNegetiveHeightHandling, everestHeight);
            tData.SetHeights(0, 0, _heightmap);
            SetTerrainSize(areaSizeX, areaSizeZ, tData, TTerraWorldGraph.scaleFactor);

            terrain.basemapDistance = baseMapDistance;
            terrain.transform.position = new Vector3(-(terrain.terrainData.size.x / 2f), 0, -(terrain.terrainData.size.z / 2f));
            terrain.Flush();
        }

        public static void UpdateTerrainImagery(TTerrain tTerrainData, Terrain terrain)
        {
            TDebug.TraceMessage("UpdateTerrainImagery");

            List<TDetailTexture> detailTextureCollection = tTerrainData.detailTextureCollection._textures;
            //List<TDetailTexture> detailTextures = new List<TDetailTexture>();

            // Generating Colormap
            TDetailTexture _colormap = tTerrainData.colorMapTextureCollection.GetColorMap();
            TerrainData tData = terrain.terrainData;
            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

            // Create TerrainLayer for the Colormap
            if (_colormap != null && _colormap.DiffuseMap != null)
            {
                //_colormap.DiffuseMap.ObjectPath = ;
                string colorMapPath = TTerraWorld.WorkDirectoryLocalPath + "ColorMap.jpg";
                _colormap.DiffuseMap.SaveObject(colorMapPath);
                TerrainRenderingManager.ColormapTexture = AssetDatabase.LoadMainAssetAtPath(colorMapPath) as Texture2D;
                //Texture2D diffuse = _colormap.DiffuseMap.GetObject() as Texture2D;
                //AssetDatabase.CreateAsset(diffuse, _terraWorld.HeightmapPathBackground);

                ////string texturePath = colormap.DiffuseMap.ObjectPath;
                ////TImage diffuseImage = colormap.DiffuseMap;
                ////TImage normalImage = colormap.NormalMap;
                ////TImage maskmapImage = colormap.MaskMap;
                //colormap.DiffuseMap.ObjectPath
                //Texture2D diffuse = colormap.DiffuseMap.GetObject() as Texture2D;
                ////Texture2D normal = null;
                ////Texture2D maskmap = null;
                //
                //if (TerrainRenderingManager.isModernRendering)
                //{
                //    terrainMaterial = AssetDatabase.LoadAssetAtPath(TTerraWorld.GeneratedTerrainsPath + "Terrain.mat", typeof(Material)) as Material;
                //    terrainMaterial?.SetTexture("_ColorMap", diffuse);
                //
                //    if (TerrainRenderingManager.isColormapBlending)
                //    {
                //        terrainMaterial?.SetFloat("_BlendingDistance", terrain.terrainData.size.x);
                //        terrainMaterial?.SetFloat("_Blend", 0.01f);
                //    }
                //    else
                //    {
                //        terrainMaterial?.SetFloat("_BlendingDistance", 1000000);
                //        terrainMaterial?.SetFloat("_Blend", 0f);
                //    }
                //}
                //else
                //{
                //    //    diffuse = diffuseImage.GetObject() as Texture2D;
                //    //    if (normalImage != null) normal = normalImage.GetObject() as Texture2D;
                //    //    if (maskmapImage != null) maskmap = maskmapImage.GetObject() as Texture2D;
                //    //
                //    //    TerrainLayer terrainLayer = new TerrainLayer();
                //    //    terrainLayer.diffuseTexture = diffuse;
                //    //    if (normal != null) terrainLayer.normalMapTexture = normal;
                //    //    if (maskmapImage != null) terrainLayer.maskMapTexture = maskmap;
                //    //
                //    //    terrainLayer.tileSize = new Vector2(tData.size.x, tData.size.z);
                //    //    terrainLayer.tileOffset = Vector2.zero;
                //    //
                //    //    terrainLayer.specular = TUtils.CastToUnityColor(colormap.Specular);
                //    //    terrainLayer.metallic = colormap.Metallic;
                //    //    terrainLayer.smoothness = colormap.Smoothness;
                //    //    terrainLayer.normalScale = colormap.NormalScale;
                //    //    terrainLayers.Add(terrainLayer);
                //    //    AssetDatabase.CreateAsset(terrainLayer, TTerraWorld.WorldDataPath + "Colormap Terrain Layer.terrainlayer");
                //    //    detailTextures.Insert(0, colormap);
                //}
            }

            if (detailTextureCollection.Count == 0 && _colormap != null)
            {
                _colormap.Tiling = new System.Numerics.Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
                detailTextureCollection.Add(_colormap);
            }

            // Create or Assign TerrainLayers for detail textures
            for (int i = 0; i < detailTextureCollection.Count; i++)
            {
                TDetailTexture detailTexture = detailTextureCollection[i];
                //if (detailTexture.IsColorMap) continue;
                //if (detailTextureCollection[i].DiffuseMap == null) continue;
                //detailTextures.Add(detailTexture);

                if (detailTexture.Mode == TDetailTextureMode.TerrainLayer)
                {
                    TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath(detailTexture.TerrainLayerPath, typeof(TerrainLayer)) as TerrainLayer;
                    terrainLayers.Add(terrainLayer);
                }
                else if (detailTexture.Mode == TDetailTextureMode.Deffuse)
                {
                    TerrainLayer terrainLayer = new TerrainLayer();
                    //TImage diffuseImage = detailTexture.DiffuseMap;
                    //TImage normalImage = detailTexture.NormalMap;
                    //TImage maskmapImage = detailTexture.MaskMap;

                    if (detailTexture.DiffuseMap != null)
                    {
                        // string path = TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer " + i + " Diffuse.jpg";
                        // detailTexture.DiffuseMap.SaveObject(path);
                        //terrainLayer.diffuseTexture = AssetDatabase.LoadMainAssetAtPath(detailTexture.DiffuseMap.ObjectPath) as Texture2D;
                        terrainLayer.diffuseTexture = detailTexture.DiffuseMap.GetAsTexture2D();
                    }

                    if (detailTexture.NormalMap != null)
                    {
                        // string path = TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer " + i + " Normalmap.jpg";
                        // detailTexture.NormalMap.SaveObject(path);
                        // terrainLayer.normalMapTexture = AssetDatabase.LoadMainAssetAtPath(detailTexture.NormalMap.ObjectPath) as Texture2D;
                        terrainLayer.normalMapTexture = detailTexture.NormalMap.GetAsTexture2D();
                    }

                    if (detailTexture.MaskMap != null)
                    {
                        // string path = TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer " + i + " Maskmap.jpg";
                        // detailTexture.MaskMap.SaveObject(path);
                        // terrainLayer.maskMapTexture = AssetDatabase.LoadMainAssetAtPath(detailTexture.MaskMap.ObjectPath) as Texture2D;
                        terrainLayer.maskMapTexture = detailTexture.MaskMap.GetAsTexture2D();
                    }

                    //if (diffuseImage != null) terrainLayer.diffuseTexture = diffuseImage.GetTextureRGBAAndSaveOnDisk("Terrain Layer " + i + " Diffuse");
                    //if (normalImage != null) terrainLayer.normalMapTexture = normalImage.GetTextureRGBAAndSaveOnDisk("Terrain Layer " + i + " Normal");
                    //if (maskmapImage != null) terrainLayer.maskMapTexture = maskmapImage.GetTextureRGBAAndSaveOnDisk("Terrain Layer " + i + " Maskmap");

                    terrainLayer.tileSize = TUtils.CastToUnity(detailTexture.Tiling);
                    terrainLayer.tileOffset = TUtils.CastToUnity(detailTexture.TilingOffset);
                    terrainLayer.specular = TUtils.CastToUnityColor(detailTexture.Specular);
                    terrainLayer.metallic = detailTexture.Metallic;
                    terrainLayer.smoothness = detailTexture.Smoothness;
                    terrainLayer.normalScale = detailTexture.NormalScale;
                    string _path = TTerraWorld.WorkDirectoryLocalPath + "Terrain Layer " + i + ".terrainlayer";
                    if (File.Exists(_path)) AssetDatabase.DeleteAsset(_path);
                    AssetDatabase.CreateAsset(terrainLayer, _path);
                    terrainLayer = AssetDatabase.LoadAssetAtPath(_path, typeof(TerrainLayer)) as TerrainLayer;
                    terrainLayers.Add(terrainLayer);
                }
                else
                    throw new Exception("Unknown DetailTexture data!");
            }

            tData.terrainLayers = terrainLayers.ToArray();

#if TERRAWORLD_XPRO
            if (tTerrainData.splatmapResolutionBestFit)
                tData.alphamapResolution = TUtils.nearestPowerOfTwo(tTerrainData.Heightmap.heightsData.GetLength(0) - 1);
            else
                tData.alphamapResolution = TUtils.nearestPowerOfTwo(tTerrainData.splatmapResolution);
            
#else
            if (TTerraWorld.WorldGraph.RenderingDATA.splatmapResolutionBestFit)
                tData.alphamapResolution = TUtils.nearestPowerOfTwo(tTerrainData.Heightmap.heightsData.GetLength(0) - 1);
            else
                tData.alphamapResolution = TUtils.nearestPowerOfTwo(TTerraWorld.WorldGraph.RenderingDATA.splatmapResolution);
#endif

            //tTerrainData.detailTextureCollection._textures = detailTextures;
            float[,,] splatmap = tTerrainData.detailTextureCollection.GetAplphaMaps(tData.alphamapResolution);
            terrain.terrainData.SetAlphamaps(0, 0, splatmap);
            AssetDatabase.Refresh();
        }

        static List<double> RemoveHighVarianceElements(List<double> numbers, bool upperElements, double tolerance)
        {
            double mean = numbers.Average();
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < numbers.Count; i++)
            {
                if (upperElements == true)//remove higher
                {
                    if ((numbers[i] - mean) / mean > tolerance)
                        numbers.RemoveAt(i);
                }
                else
                {
                    if ((mean - numbers[i]) / mean > tolerance)
                        numbers.RemoveAt(i);
                }
            }

            return numbers;
        }

        public static void ApplyLakeOnTerrain(Terrain terrain, TTerrain tTerrainData, out List<GameObject> lakeList)
        {
            TDebug.TraceMessage("ApplyLakeOnTerrain");
            TerrainData data = terrain.terrainData;
            List<TLakeLayer> _lakeLayers = tTerrainData.LakeLayer;
            List<TOceanLayer> _oceanLayers = tTerrainData.OceanLayer;
            lakeList = new List<GameObject>();
            List<T2DObject> lakes = new List<T2DObject>();
            TGlobalPoint centerPos;
            System.Numerics.Vector3 centerWorldPos;
            Material waterMaterial = null;
            GameObject waterLayer;
            //float _minHeight;
            int outerCount;
            string waterTag = "Respawn";
            int lakeLayerIndex = LayerMask.NameToLayer("Water");

            if (tTerrainData.overallWaterMask != null)
                TerrainRenderingManager.WaterMaskTexture = tTerrainData.overallWaterMask.GetTexture("WaterMask");
            else
                TerrainRenderingManager.WaterMaskTexture = null;

            //TODO
            if (_lakeLayers?.Count != 0)
            {
                //TTerraWorldTerrainManager terrainManagerScript = terrain.GetComponent<TTerraWorldTerrainManager>();
                //if (terrainManagerScript == null) throw new Exception("Terrain Detection Error.");

                //InitDebugBorders(lakes);
                foreach (TLakeLayer lakeLayer in _lakeLayers)
                {
                    //if (lakeLayer.xMaterial == null)
                    waterMaterial = lakeLayer.material.GetMaterial();
                    //else
                    //    waterMaterial = lakeLayer.xMaterial;

                    waterLayer = GameObject.Find(lakeLayer.LayerName);
                    if (waterLayer == null) waterLayer = new GameObject(lakeLayer.LayerName);
                    waterLayer.transform.parent = terrain.transform;

                    for (int i = 0; i < lakeLayer.LakesList.Count; i++)
                    {
                        T2DObject lake = lakeLayer.LakesList[i];
                        lake.Center();

                        //GetCenterPositionOfAroundPoints(lake.AroundPoints, ref lake.center);
                        centerWorldPos = _terraWorld.GetWorldPosition(lake.center);
                        //_minHeight = centerWorldPos.Y;

                        string name = string.IsNullOrEmpty(lake.name) ? "No Name" : lake.name;
                        GameObject lakeObject = new GameObject(name);
                        Vector3 pivotPosition = TUtils.CastToUnity(centerWorldPos + lakeLayer.Offset);
                        //pivotPosition.y = lakeLayer.Offset.Y + lakeLayer.WaterMasks[i].minHeight - 1.5f + (offsetForNegetiveHeightHandling * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration);
                        //pivotPosition.y = lakeLayer.Offset.Y + lakeLayer.WaterMasks[i].minHeight - 1.5f + offsetForNegetiveHeightHandling;

                        Vector3 minimumHeight = new Vector3(0, 1000000, 0);
                        List<double> heights = new List<double>();

                        for (int j = 0; j < lake.AroundPoints.Count; j++)
                        {
                            Vector3 worldPosition = TUtils.CastToUnity(_terraWorld.GetWorldPosition(new TGlobalPoint(lake.AroundPoints[j].latitude, lake.AroundPoints[j].longitude)));
                            heights.Add(worldPosition.y);

                            if (minimumHeight.y > worldPosition.y)
                                minimumHeight = worldPosition;
                        }

                        heights = RemoveHighVarianceElements(heights, true, 20);
                        double meanHeight = heights.Average();

                        //pivotPosition.y = lakeLayer.Offset.Y + lakeLayer.WaterMasks[0].minHeight - 1.5f + offsetForNegetiveHeightHandling;
                        pivotPosition.y = lakeLayer.Offset.Y + (float)meanHeight + offsetForNegetiveHeightHandling;

                        // Offset created water mesh's Y position to an extra 1 meter downwards to match with terrain deformation and avoid sticking out of the edges.
                        pivotPosition.y -= 1f;

                        lakeObject.transform.position = pivotPosition;

                        // Add Mesh Editor To Lakes
                        lakeObject.AddComponent<MeshTools>();

                        // Set renderer & material
                        MeshRenderer renderer = lakeObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                        renderer.sharedMaterial = waterMaterial;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.receiveShadows = true;

                        // Set mesh filter and assign generated mesh
                        MeshFilter filter = lakeObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
                        lakes.Clear();
                        lakes.Add(lake);
                        TMesh lakemesh = InitMesh_TriangleNet(lakes, centerWorldPos);
                        filter.mesh = lakemesh.Mesh;
                        lakeObject.transform.parent = waterLayer.transform;

                        // Add Mesh Collider To Lakes
                        MeshCollider lakeCollider = lakeObject.gameObject.AddComponent<MeshCollider>();
                        lakeCollider.sharedMesh = null;
                        lakeCollider.sharedMesh = lakemesh.Mesh;

                        lakeObject.tag = waterTag;
                        lakeObject.layer = lakeLayerIndex;

                        // Automatically add LOD Group to each Lake
                        LODGroup group = lakeObject.AddComponent<LODGroup>();
                        Renderer[] renderers = new Renderer[1];
                        renderers[0] = lakeObject.GetComponent<Renderer>();
                        LOD[] lods = new LOD[1];
                        lods[0] = new LOD(lakeLayer.LODCulling / 100f, renderers);
                        group.SetLODs(lods);
                        group.RecalculateBounds();

                        // Add water4U scripts
                        WaterManager.AddWater4MaterialComponents(lakeObject);

                        lakeList.Add(waterLayer);
                    }
                }

                if (_oceanLayers?.Count != 0)
                {
                    foreach (TOceanLayer oceanLayer in _oceanLayers)
                    {
                        // Makes Oceans with Islands
                        //_minHeight = float.MaxValue;
                        //if (oceanLayer.xMaterial == null)
                        waterMaterial = oceanLayer.material.GetMaterial();
                        //else
                        //    waterMaterial = oceanLayer.xMaterial;

                        waterLayer = GameObject.Find(oceanLayer.LayerName);
                        if (waterLayer == null) waterLayer = new GameObject(oceanLayer.LayerName);
                        waterLayer.transform.parent = terrain.transform;
                        List<T2DObject> oceans = new List<T2DObject>();

                        foreach (T2DObject Coastline in oceanLayer.Coastlines)
                            if (Coastline.property != TProperty.None)
                                oceans.Add(Coastline);

                        outerCount = 0;
                        centerPos = new TGlobalPoint();
                        centerPos.longitude = 0;
                        centerPos.latitude = 0;

                        for (int i = 0; i < oceans.Count; i++)
                        {
                            if (oceans[i].property == TProperty.Outer)
                            {
                                oceans[i].Center();
                                centerPos.longitude += oceans[i].center.longitude;
                                centerPos.latitude += oceans[i].center.latitude;
                                outerCount++;
                            }
                        }

                        if (outerCount > 0)
                        {
                            centerPos.longitude /= outerCount;
                            centerPos.latitude /= outerCount;
                            centerWorldPos = _terraWorld.GetWorldPosition(centerPos);

                            string name = "No Name";
                            for (int i = 0; i < oceans.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(oceans[i].name))
                                    name = oceans[i].name;
                            }
                            GameObject ocean = new GameObject(name);

                            Vector3 pivotPosition = TUtils.CastToUnity(centerWorldPos + oceanLayer.Offset);
                            //pivotPosition.y = oceans[0].minHeight + oceanLayer.Offset.Y + offsetForNegetiveHeightHandling + oceanLayer.depth;
                            //pivotPosition.y = oceanLayer.Offset.Y + oceanLayer.WaterMasks[0].minHeight - 1.5f + (offsetForNegetiveHeightHandling * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration);
                            //  pivotPosition.y = oceanLayer.Offset.Y + oceanLayer.WaterMasks[0].minHeight - 1.5f + offsetForNegetiveHeightHandling;
                            Vector3 minimumHeight = new Vector3(0, 1000000, 0);

                            for (int j = 0; j < oceans[0].AroundPoints.Count; j++)
                            {
                                Vector3 xxx = TUtils.CastToUnity(_terraWorld.GetWorldPosition(new TGlobalPoint(oceans[0].AroundPoints[j].latitude, oceans[0].AroundPoints[j].longitude)));

                                if (minimumHeight.y > xxx.y)
                                    minimumHeight = xxx;
                            }

                            //pivotPosition.y = oceanLayer.Offset.Y + oceanLayer.WaterMasks[0].minHeight - 1.5f + offsetForNegetiveHeightHandling;
                            pivotPosition.y = oceanLayer.Offset.Y + minimumHeight.y + offsetForNegetiveHeightHandling;
                            ocean.transform.position = pivotPosition;

                            // Add Mesh Editor To Lakes
                            ocean.AddComponent<MeshTools>();

                            // Set renderer & material
                            MeshRenderer renderer = ocean.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                            renderer.sharedMaterial = waterMaterial;
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            renderer.receiveShadows = true;

                            // Set mesh filter and assign generated mesh
                            MeshFilter filter = ocean.AddComponent(typeof(MeshFilter)) as MeshFilter;
                            TMesh lakemesh = InitMesh_TriangleNet(oceans, centerWorldPos);
                            filter.mesh = lakemesh.Mesh;
                            ocean.transform.parent = waterLayer.transform;

                            // Add Mesh Collider To Lakes
                            MeshCollider lakeCollider = ocean.gameObject.AddComponent<MeshCollider>();
                            lakeCollider.sharedMesh = null;
                            lakeCollider.sharedMesh = lakemesh.Mesh;

                            ocean.tag = waterTag;
                            ocean.layer = lakeLayerIndex;

                            // Automatically add LOD Group to each Lake
                            LODGroup group = ocean.AddComponent<LODGroup>();
                            Renderer[] renderers = new Renderer[1];
                            renderers[0] = ocean.GetComponent<Renderer>();
                            LOD[] lods = new LOD[1];
                            lods[0] = new LOD(oceanLayer.LODCulling / 100f, renderers);
                            group.SetLODs(lods);
                            group.RecalculateBounds();

                            // Add water4U scripts
                            WaterManager.AddWater4MaterialComponents(ocean);

                            lakeList.Add(waterLayer);
                        }
                    }
                }
            }
        }

        public static void ApplyRiversOnTerrain(Terrain terrain, TTerrain tTerrainData, out List<GameObject> riverList)
        {
            TDebug.TraceMessage("ApplyRiversOnTerrain");
            TerrainData data = terrain.terrainData;
            List<TRiverLayer> _riverLayers = tTerrainData.RiverLayer;
            riverList = new List<GameObject>();
            if (_riverLayers == null || _riverLayers.Count == 0) return;
            string waterTag = "Respawn";
            int lakeLayerIndex = LayerMask.NameToLayer("Water");
            Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
            Vector4 mapBounds = new Vector4((float)TTerraWorld.WorldArea.Top, (float)TTerraWorld.WorldArea.Left, (float)TTerraWorld.WorldArea.Bottom, (float)TTerraWorld.WorldArea.Right);
            float widthFactor = terrainSize.x / Math.Abs(mapBounds.w - mapBounds.y);
            float heightFactor = terrainSize.y / Math.Abs(mapBounds.x - mapBounds.z);

            //Material waterMaterial = TTerraWorldManager.WaterManagerScript.waterMaterial;
            Material waterMaterial = null;

            foreach (var riverlayer in _riverLayers)
            {
                //if (riverlayer.xMaterial == null)
                waterMaterial = riverlayer.material.GetMaterial();
                //else
                //    waterMaterial = riverlayer.xMaterial;

                GameObject riverLayerObject = GameObject.Find(riverlayer.LayerName);
                if (riverLayerObject == null) riverLayerObject = new GameObject(riverlayer.LayerName);
                riverLayerObject.transform.parent = terrain.transform;

                for (int i = 0; i < riverlayer.RiversList.Count; i++)
                {
                    TRiver river = new TRiver(riverlayer.RiversList[i], widthFactor, heightFactor, terrainSize, mapBounds, terrain, tTerrainData);
                    List<Vector3> nodes = river.getNodes();
                    int nodeCount = nodes.Count;

                    GameObject riverGameObject = new GameObject(river.riverName);
                    MeshFilter mf = riverGameObject.gameObject.AddComponent<MeshFilter>();
                    riverGameObject.gameObject.AddComponent<MeshRenderer>();
                    //riverGameObject.transform.position = new Vector3 (0, riverlayer.depth ,0);
                    riverGameObject.isStatic = false;

                    riverGameObject.transform.parent = riverLayerObject.transform;
                    riverGameObject.tag = waterTag;
                    riverGameObject.layer = lakeLayerIndex;

                    riverGameObject.gameObject.AddComponent<TRiverMeshGen>();
                    TRiverMeshGen riverMeshGen = riverGameObject.gameObject.GetComponent<TRiverMeshGen>();
                    //TRiverMeshGen riverMeshGen = new TRiverMeshGen();

                    riverMeshGen.SetVertexCount(nodeCount);
                    Vector3 offset = TUtils.CastToUnity(riverlayer.Offset);
                    offset.y += riverlayer.depth + 2;

                    for (int j = 0; j < nodeCount; j++)
                    {
                        riverMeshGen.SetPosition(j, river.getNode(j) + offset);

                        //GameObject dummy1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //dummy1.name = "Index - "+j;
                        //dummy1.transform.position = river.getNode(j);
                        //dummy1.transform.localScale = new Vector3(2, 2, 2);
                        //dummy1.transform.parent = riverLayerObject.transform;
                    }

                    // Add Mesh Editor To Rivers
                    riverGameObject.AddComponent<MeshTools>();

                    Renderer riverRenderer = riverGameObject.GetComponent<Renderer>();
                    riverRenderer.sharedMaterial = waterMaterial;
                    riverRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    riverRenderer.receiveShadows = true;

                    Mesh riverMesh = riverMeshGen.GetMesh(riverlayer._width);

                    mf.sharedMesh = riverMesh;
                    mf.sharedMesh.RecalculateBounds();

                    riverGameObject.GetComponent<MeshFilter>().sharedMesh = riverMesh;

                    // Add Mesh Collider To Rivers
                    if (riverMesh.vertexCount >= 3)
                    {
                        MeshCollider riverCollider = riverGameObject.gameObject.AddComponent<MeshCollider>();
                        riverCollider.sharedMesh = null;
                        riverCollider.sharedMesh = riverMesh;
                    }

                    // Automatically add LOD Group to each River
                    LODGroup group = riverGameObject.AddComponent<LODGroup>();
                    Renderer[] renderers = new Renderer[1];
                    renderers[0] = riverGameObject.GetComponent<Renderer>();
                    LOD[] lods = new LOD[1];
                    lods[0] = new LOD(riverlayer.LODCulling / 100f, renderers);
                    group.SetLODs(lods);
                    group.RecalculateBounds();

                    // Add water4U scripts
                    WaterManager.AddWater4MaterialComponents(riverGameObject);
                    riverList.Add(riverGameObject);
                }
            }
        }

        public static bool GetCenterPositionOfAroundPoints(List<TGlobalPoint> aroundPoints, ref TGlobalPoint centerPos)
        {
            bool centerPointFlag = false;
            TGlobalPoint pointLB = new TGlobalPoint();
            TGlobalPoint pointRT = new TGlobalPoint();
            List<T2DPoint> intersectPoints1 = new List<T2DPoint>();
            List<T2DPoint> intersectPoints2 = new List<T2DPoint>();

            if (!TUtils.PointInPolygon(aroundPoints, centerPos))
            {
                TUtils.PolygonRectangle(aroundPoints, ref pointLB, ref pointRT);

                for (int i = 0; i < 4; i++)
                {
                    TLine line;
                    intersectPoints2.Clear();

                    switch (i)
                    {
                        case 0:
                            line = new TLine(centerPos.longitude, centerPos.latitude, pointRT.longitude * 1.01, centerPos.latitude);
                            break;
                        case 1:
                            line = new TLine(centerPos.longitude, centerPos.latitude, pointRT.longitude, -centerPos.latitude * 1.01);
                            break;
                        case 2:
                            line = new TLine(-centerPos.longitude * 1.01, centerPos.latitude, pointRT.longitude, centerPos.latitude);
                            break;
                        case 3:
                            line = new TLine(centerPos.longitude, centerPos.latitude, pointRT.longitude, centerPos.latitude * 1.01);
                            break;
                        default:
                            line = new TLine(centerPos.longitude, centerPos.latitude, pointRT.longitude * 1.01, centerPos.latitude);
                            break;
                    }

                    for (int j = 0; j < aroundPoints.Count - 1; j++)
                    {
                        TLine linePolygon = new TLine(aroundPoints[j].longitude, aroundPoints[j].latitude,
                                                      aroundPoints[j + 1].longitude, aroundPoints[j + 1].latitude);

                        if (line.CalcIntersection(linePolygon, ref intersectPoints1))
                        {
                            if (intersectPoints1.Count == 1)
                                intersectPoints2.Add(intersectPoints1[0]);

                            if (intersectPoints2.Count == 2)
                            {
                                centerPos.longitude = (intersectPoints2[0].x + intersectPoints2[1].x) / 2.0;
                                centerPos.latitude = (intersectPoints2[0].y + intersectPoints2[1].y) / 2.0;
                                centerPointFlag = true;
                                break;
                            }
                        }
                    }

                    if (centerPointFlag == true)
                        break;
                }
            }

            return centerPointFlag;
        }

        public static void ApplyGridsOnTerrain(Terrain terrain, TTerrain tTerrainData)
        {
            TDebug.TraceMessage("ApplyGridsOnTerrain");
            bool gridPassedAllFilters = true;
            TerrainData data = terrain.terrainData;
            List<TGridLayer> _gridsLayers = tTerrainData.GridsLayers;
            if (_gridsLayers == null || _gridsLayers.Count == 0) return;

            foreach (TGridLayer _gridlayer in _gridsLayers)
            {
                if (_gridlayer == null || _gridlayer.GridsList.Count == 0) continue;
                GameObject gridLayer = new GameObject(_gridlayer.LayerName);
                gridLayer.transform.position = terrain.transform.position;
                gridLayer.transform.parent = terrain.transform;

                foreach (T2DGrid _grid in _gridlayer.GridsList)
                {
                    if (_grid.TrianglesList.Count < 1) continue;
                    gridPassedAllFilters = true;

                    if (gridPassedAllFilters)
                    {
                        TGlobalPoint centerPos = _grid.Center();
                        System.Numerics.Vector3 centerWorldPos = (_terraWorld.GetWorldPosition(centerPos));

                        GameObject grid = new GameObject("gr_" + _grid.ID.ToString());
                        Vector3 pivot = TUtils.CastToUnity(centerWorldPos + _gridlayer.Offset);
                        //pivot.y += offsetForNegetiveHeightHandling * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;
                        pivot.y += offsetForNegetiveHeightHandling;
                        grid.transform.parent = gridLayer.transform;
                        grid.transform.localPosition = pivot;
                        grid.layer = LayerMask.NameToLayer(_gridlayer.UnityLayerName);

                        // Add Mesh Editor To Grid
                        grid.AddComponent<MeshTools>();

                        // Set renderer & material
                        MeshRenderer renderer = grid.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                        Material material = null;

                        //if (_gridlayer.xMaterial == null)
                        material = _gridlayer.material.GetMaterial();
                        //else
                        //    material = _gridlayer.xMaterial;

                        renderer.sharedMaterial = material;

                        if (_gridlayer.HasShadowCasting)
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        else
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                        // Set mesh filter and assign generated mesh
                        MeshFilter filter = grid.AddComponent(typeof(MeshFilter)) as MeshFilter;
                        TMesh gridMesh = GenerateMesh(_grid);
                        filter.mesh = gridMesh.Mesh;

                        CurveMesh(filter.sharedMesh, _gridlayer.EdgeCurve);

                        if (_gridlayer.HasCollider)
                        {
                            MeshCollider meshCollider = grid.AddComponent<MeshCollider>();
                            //meshCollider.convex = _gridlayer.IsConvex;
                            meshCollider.sharedMesh = null;
                            meshCollider.sharedMesh = filter.sharedMesh;

                            if (_gridlayer.HasPhysics)
                                grid.AddComponent<Rigidbody>();
                        }

                        // Automatically add LOD Group to each TerraMesh
                        LODGroup group = grid.AddComponent<LODGroup>();
                        Renderer[] renderers = new Renderer[1];
                        renderers[0] = grid.GetComponent<Renderer>();
                        LOD[] lods = new LOD[1];
                        lods[0] = new LOD(_gridlayer.LODCulling / 100f, renderers);
                        group.SetLODs(lods);
                        group.RecalculateBounds();
                    }
                }
            }
        }

        private static void CurveMesh(Mesh mesh, float edgeCurve)
        {
            Vector3[] vertices = mesh.vertices;
            //List<MeshEdges.Edge> boundaryPath = MeshEdges.GetEdges(mesh.triangles).FindBoundary().SortEdges();
            List<MeshEdges.Edge> boundaryPath = MeshEdges.GetEdges(mesh.triangles).FindBoundary();

            for (int i = 0; i < boundaryPath.Count; i++)
                vertices[boundaryPath[i].v1].y += edgeCurve;

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public static void ApplyObjectsOnTerrain(Terrain terrain, TTerrain tTerrainData)
        {
            TDebug.TraceMessage("ApplyObjectsOnTerrain");
            List<TObjectScatterLayer> _objectLayers = tTerrainData.ObjectScatterLayers;
            if (_objectLayers == null || _objectLayers.Count == 0) return;

            //Dictionary<string, UnityEngine.Object> Prefabslib = new Dictionary<string, UnityEngine.Object>();
            //UnityEngine.Object Objectprefab = null;
            //bool objectPassedAllFilters = true;
            TerrainData data = terrain.terrainData;
            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);

            foreach (TObjectScatterLayer _objectlayer in _objectLayers)
            {
                ObjectLayer OSP = null;

                if (_objectlayer.layerType == LayerType.ScatteredObject)
                {
                    GameObject objectLayer = new GameObject(_objectlayer.LayerName);
                    objectLayer.transform.position = terrain.transform.position;
                    objectLayer.transform.parent = terrain.transform;

                    //TODO: Add editing tools and brush painting features to scattered objects
                    // Create ObjectLayerEditor.cs to override GUI
                    OSP = objectLayer.AddComponent<ObjectLayer>();
                }
                else if (_objectlayer.layerType == LayerType.ScatteredTrees)
                {
                    GameObject Treeprefab = (GameObject)AssetDatabase.LoadAssetAtPath(_objectlayer.prefabNames[0], typeof(GameObject));
                    List<TreePrototype> newPrototypes = new List<TreePrototype>(terrain.terrainData.treePrototypes);
                    TreePrototype newP = new TreePrototype { prefab = Treeprefab };
                    //int TreePrototypeIndex = newPrototypes.Count;
                    newPrototypes.Add(newP);

                    terrain.terrainData.treePrototypes = newPrototypes.ToArray();
                    terrain.terrainData.RefreshPrototypes();
                    terrain.Flush();

                    //TODO: Add editing tools and brush painting features to scattered objects
                    // Create ObjectLayerEditor.cs to override GUI
                    OSP = terrain.gameObject.AddComponent<ObjectLayer>();
                }

                OSP.averageDistance = _objectlayer.averageDistance;
                OSP.scale = TUtils.CastToUnity(_objectlayer.ScaleMultiplier);
                OSP.minScale = _objectlayer.MinScale;
                OSP.maxScale = _objectlayer.MaxScale;
                OSP.positionVariation = _objectlayer.PositionVariation;
                OSP.lock90DegreeRotation = _objectlayer.rotation90Degrees;
                OSP.lockYRotation = _objectlayer.lockYRotation;
                OSP.getSurfaceAngle = _objectlayer.getSurfaceAngle;
                OSP.seedNo = _objectlayer.SeedNo;
                OSP.priority = _objectlayer.Priority;
                OSP.unityLayerName = _objectlayer.UnityLayerName;
                OSP.unityLayerMask = _objectlayer.UnityLayerMask;
                OSP.positionVariation = _objectlayer.PositionVariation;
                OSP.positionVariation = _objectlayer.PositionVariation;
                OSP.positionOffset = TUtils.CastToUnity(_objectlayer.Offset);
                OSP.rotationOffset = TUtils.CastToUnity(_objectlayer.RotationOffset);
                OSP.minRotationRange = _objectlayer.MinRotationRange;
                OSP.maxRotationRange = _objectlayer.MaxRotationRange;
                OSP.minAllowedAngle = _objectlayer.MinSlope;
                OSP.maxAllowedAngle = _objectlayer.MaxSlope;
                OSP.minAllowedHeight = _objectlayer.MinElevation;
                OSP.maxAllowedHeight = _objectlayer.MaxElevation;
                OSP.bypassLake = _objectlayer.bypassLake;
                OSP.underLake = _objectlayer.underLake;
                OSP.onLake = _objectlayer.onLake;
                OSP.checkBoundingBox = _objectlayer.checkBoundingBox;
                OSP.prefabNames = _objectlayer.prefabNames;
                OSP.layerType = _objectlayer.layerType;
                OSP.placeSingleItem = _objectlayer.placeSingleItem;

                OSP.maskDataFast = new TScatterLayer.MaskDataFast[_objectlayer.maskData.GetLength(0)];

                for (int i = 0; i < _objectlayer.maskData.GetLength(0); i++)
                {
                    OSP.maskDataFast[i].row = new float[_objectlayer.maskData.GetLength(1)];

                    for (int j = 0; j < _objectlayer.maskData.GetLength(1); j++)
                        OSP.maskDataFast[i].row[j] = _objectlayer.maskData[i, j];
                }

                OSP.SaveMaskData();

                //foreach (TPointObject _object in _objectlayer.points)
                //{
                //    objectPassedAllFilters = true;
                //    Vector3 normal = new Vector3(0, 1, 0);
                //    Vector3 worldPosition = TUtils.CastToUnity(_terraWorld.GetWorldPosition(_object.GeoPosition));
                //
                //    // Local space
                //    Vector3 origin = worldPosition;
                //    origin.x = origin.x + _objectlayer.Offset.X + terrain.transform.position.x;
                //    origin.y = 100000;
                //    origin.z = origin.z + _objectlayer.Offset.Z + terrain.transform.position.z;
                //
                //    Ray ray = new Ray(origin, Vector3.down);
                //    RaycastHit hit;
                //
                //    if (!Raycasts.RaycastNonAllocSorted(ray, _objectlayer.bypassLake, _objectlayer.underLake, out hit, _objectlayer.UnityLayerMask))
                //        continue;
                //
                //    origin = hit.point;
                //    normal = hit.normal;
                //
                //    if (Vector3.Angle(normal, Vector3.up) >= _objectlayer.MinSlope && Vector3.Angle(normal, Vector3.up) <= _objectlayer.MaxSlope)
                //    {
                //        //if ((origin.y) >= _objectlayer.MinElevation && (origin.y) <= _objectlayer.MaxElevation)
                //        //    objectPassedAllFilters = true;
                //        //else
                //        //    objectPassedAllFilters = false;
                //
                //        objectPassedAllFilters = true;
                //    }
                //    else
                //        objectPassedAllFilters = false;
                //
                //    if (objectPassedAllFilters)
                //    {
                //        if (_objectlayer.layerType == LayerType.ScatteredObject)
                //        {
                //            int randomObjectIndex = rand.Next(0, _objectlayer.prefabNames.Count);
                //            string prefabName = _objectlayer.prefabNames[randomObjectIndex];
                //            Objectprefab = AssetDatabase.LoadAssetAtPath(prefabName, typeof(UnityEngine.Object));
                //            GameObject go = GameObject.Instantiate(Objectprefab) as GameObject;
                //            go.name = _object.Id.ToString();
                //            go.transform.localScale = TUtils.CastToUnity(_object.scale);
                //            go.transform.parent = objectLayer.transform;
                //
                //            foreach (Transform t in go.GetComponentsInChildren(typeof(Transform), false))
                //            {
                //                t.gameObject.layer = LayerMask.NameToLayer(_objectlayer.UnityLayerName);
                //            }
                //
                //            // --- rotation
                //            if (_objectlayer.getSurfaceAngle)
                //            {
                //                Vector3 rotationOffset = TUtils.CastToUnity(_objectlayer.RotationOffset);
                //                Vector3 finalRotation = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
                //                Quaternion surfaceRotation = Quaternion.Euler(finalRotation);
                //
                //                if (!_objectlayer.lockYRotation)
                //                {
                //                    float rotationY = UnityEngine.Random.Range(_objectlayer.MinRotationRange, _objectlayer.MaxRotationRange);
                //                    surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                //                }
                //                else
                //                {
                //                    float rotationY = Mathf.Round(UnityEngine.Random.Range(0f, 4)) * 90;
                //                    surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                //                    surfaceRotation.eulerAngles = new Vector3(surfaceRotation.eulerAngles.x, rotationY, surfaceRotation.eulerAngles.z);
                //                }
                //
                //                go.transform.rotation = surfaceRotation * Quaternion.Euler(rotationOffset);
                //            }
                //            else
                //                go.transform.eulerAngles = TUtils.CastToUnity(_object.rotation);
                //
                //            // Local space
                //            Vector3 offsetPos = origin;
                //            offsetPos += (go.transform.right * _objectlayer.Offset.X);
                //            offsetPos += (go.transform.up * _objectlayer.Offset.Y);
                //            offsetPos += (go.transform.forward * _objectlayer.Offset.Z);
                //            go.transform.position = offsetPos;
                //        }
                //
                //        if (_objectlayer.layerType == LayerType.ScatteredTrees)
                //        {
                //            TreeInstance tree = new TreeInstance();
                //            tree.prototypeIndex = TreePrototypeIndex;
                //            tree.widthScale = _object.scale.X;
                //            tree.heightScale = _object.scale.Y;
                //            tree.rotation = _object.rotation.Y * Mathf.Deg2Rad;
                //
                //            // Local space
                //            Vector3 offsetPos = origin;
                //            offsetPos = offsetPos - terrain.transform.position;
                //            tree.position = new Vector3(offsetPos.x * 1.0f / terrain.terrainData.size.x, 0, offsetPos.z * 1.0f / terrain.terrainData.size.z);
                //            terrain.AddTreeInstance(tree);
                //        }
                //    }
                //}
                //
                //if (_objectlayer.layerType == LayerType.ScatteredTrees)
                //{
                //    terrain.terrainData.RefreshPrototypes();
                //    terrain.Flush();
                //}
            }

            UpdateObjectsLayer(terrain);
            Physics.autoSimulation = true;
        }

        private static void UpdateObjectsLayer(Terrain terrain)
        {
            try
            {
                foreach (Transform t in terrain?.GetComponentsInChildren(typeof(Transform), true))
                {
                    if (t == null) continue;
                    ObjectLayer objectLayer = t?.GetComponent<ObjectLayer>();
                    if (objectLayer != null) objectLayer.UpdateLayer();
                }
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
        }

        public static void ApplyGrassOnTerrain(Terrain terrain, TTerrain tTerrainData)
        {
            TDebug.TraceMessage("ApplyGrassOnTerrain");
            List<TGrassScatterLayer> _grassLayers = tTerrainData.GrassScatterLayers;
            if (_grassLayers == null || _grassLayers.Count == 0) return;
            //terrain.gameObject.AddComponent<MassiveGrassTerrain>();
            GameObject massiveGrass = new GameObject("Grass & Plants");
            massiveGrass.transform.parent = terrain.transform;
            MassiveGrass MG = massiveGrass.AddComponent<MassiveGrass>();

            MG.maxParallelJobCount = 4;
            //MG.maxParallelJobCount = _grassLayers[0].maxParallelJobCount;

            //MG.profiles = new List<MassiveGrassProfile>();
            //MG.masks = new Texture2D[_grassLayers.Count];
            int counter = 0;

            foreach (TGrassScatterLayer _grassLayer in _grassLayers)
            {
                GameObject child = new GameObject(_grassLayer.LayerName);
                child.transform.parent = massiveGrass.transform;
                GrassLayer grassLayer = child.AddComponent<GrassLayer>();

                grassLayer.MGP = ScriptableObject.CreateInstance(typeof(MassiveGrassProfile)) as MassiveGrassProfile;
                grassLayer.MGP.Material = _grassLayer.material.GetMaterial();
                grassLayer.MGP.Mesh = TMesh.GetMeshObject(_grassLayer.modelPath, _grassLayer.meshName);
                grassLayer.MGP.Scale = TUtils.CastToUnity(_grassLayer.scale);
                grassLayer.MGP.Radius = _grassLayer.radius;
                grassLayer.MGP.GridSize = _grassLayer.gridSize;
                grassLayer.MGP.Slant = _grassLayer.slant;
                grassLayer.MGP.AmountPerBlock = _grassLayer.amountPerBlock;
                grassLayer.MGP.AlphaMapThreshold = _grassLayer.alphaMapThreshold;
                grassLayer.MGP.DensityFactor = _grassLayer.densityFactor;
                grassLayer.MGP.BuilderType = (Mewlist.MassiveGrass.BuilderType)_grassLayer.builderType;
                grassLayer.MGP.NormalType = (Mewlist.MassiveGrass.NormalType)_grassLayer.normalType;
                grassLayer.MGP.GroundOffset = _grassLayer.groundOffset;

                if (_grassLayer.shadowCastingMode.Equals(TShadowCastingMode.On) || _grassLayer.shadowCastingMode.Equals(TShadowCastingMode.TwoSided))
                    grassLayer.MGP.CastShadows = true;
                else
                    grassLayer.MGP.CastShadows = false;

                grassLayer.MGP.Seed = _grassLayer.seedNo;
                grassLayer.MGP.Layer = GetMaskValue(_grassLayer.unityLayerName);
                grassLayer.MGP.minAllowedAngle = _grassLayer.MinSlope;
                grassLayer.MGP.maxAllowedAngle = _grassLayer.MaxSlope;
                grassLayer.MGP.minAllowedHeight = _grassLayer.MinElevation;
                grassLayer.MGP.maxAllowedHeight = _grassLayer.MaxElevation;

                //grassLayer.MGP.terrain = terrain;

                //grassLayer.MGP.mask = _grassLayer.mask.GetTexture(_grassLayer.layerName + "_Mask");
                grassLayer.MGP.maskDataFast = new TScatterLayer.MaskDataFast[_grassLayer.maskData.GetLength(0)];

                for (int i = 0; i < _grassLayer.maskData.GetLength(0); i++)
                {
                    grassLayer.MGP.maskDataFast[i].row = new float[_grassLayer.maskData.GetLength(1)];

                    for (int j = 0; j < _grassLayer.maskData.GetLength(1); j++)
                        grassLayer.MGP.maskDataFast[i].row[j] = _grassLayer.maskData[i, j];
                }

                grassLayer.SaveMaskData();

                grassLayer.MGP.layerBasedPlacement = _grassLayer.layerBasedPlacement;
                grassLayer.MGP.bypassWater = _grassLayer.bypassWater;
                grassLayer.MGP.underWater = _grassLayer.underWater;
                grassLayer.MGP.onWater = _grassLayer.onWater;
                grassLayer.MGP.unityLayerMask = _grassLayer.UnityLayerMask;
                grassLayer.MGP.brushDamping = _grassLayer.maskDamping;

                string _path = TTerraWorld.WorkDirectoryLocalPath + _grassLayer.LayerName + "_Profile.asset";
                if (File.Exists(_path)) AssetDatabase.DeleteAsset(_path);
                AssetDatabase.CreateAsset(grassLayer.MGP, _path);

                //MG.profiles.Add(grassLayer.MGP);
                //MG.masks[counter] = _grassLayer.mask.GetTexture("GrassMask_" + (counter + 1));

                if (grassLayer.MGP.Material.HasProperty("_IsGrass"))
                {
                    grassLayer.MGP.Material.SetFloat("_IsGrass", 1f);
                    grassLayer.MGP.Material.EnableKeyword("_IS_GRASS");

                    if (grassLayer.MGP.Material.HasProperty("_IsLeaves"))
                    {
                        grassLayer.MGP.Material.SetFloat("_IsLeaves", 0f);
                        grassLayer.MGP.Material.DisableKeyword("_IS_LEAVES");
                    }

                    if (grassLayer.MGP.Material.HasProperty("_IsBark"))
                    {
                        grassLayer.MGP.Material.SetFloat("_IsBark", 0f);
                        grassLayer.MGP.Material.DisableKeyword("_IS_BARK");
                    }
                }

                counter++;
            }

            AssetDatabase.Refresh();

            List<MassiveGrassProfile> profiles = new List<MassiveGrassProfile>();
            foreach (GrassLayer GL in massiveGrass.transform.GetComponentsInChildren(typeof(GrassLayer), true)) profiles.Add(GL.MGP);
            MG.profiles = profiles;
            MG.Refresh();
        }

        private static int GetMaskValue(string layerName)
        {
            int result = 0;
            List<string> layers = new List<string>();
            int index = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    if (LayerMask.LayerToName(i) == layerName)
                    {
                        result = index;
                        return result;
                    }

                    index++;
                }
            }

            return result;
        }

        public static TMesh InitMesh(T2DObject flattenObject)
        {
            TDebug.TraceMessage();
            System.Numerics.Vector3[] vertices = new System.Numerics.Vector3[flattenObject.AroundPoints.Count - 1];
            flattenObject.Center();
            System.Numerics.Vector3 centerWorldPos = (_terraWorld.GetWorldPosition(flattenObject.center));

            for (int i = 0; i < flattenObject.AroundPoints.Count - 1; i++)
            {
                TGlobalPoint node = flattenObject.AroundPoints[i];
                System.Numerics.Vector3 WorldPos = (_terraWorld.GetWorldPosition(node));
                vertices[i] = WorldPos - centerWorldPos;
                vertices[i].Y = 0;
            }

            TMesh mesh = new TMesh();
            mesh.GenerateMesh(vertices);

            return mesh;
        }

        public static TMesh InitMesh_TriangleNet(List<T2DObject> flattenObject, System.Numerics.Vector3 centerWorldPos)
        {
            TDebug.TraceMessage();
            TMesh mesh = new TMesh();
            List<PolygonVertices> polygonVertices = new List<PolygonVertices>();

            if (flattenObject.Count > 0)
            {
                for (int i = 0; i < flattenObject.Count; i++)
                {
                    PolygonVertices polygon = new PolygonVertices(flattenObject[i].AroundPoints.Count);
                    polygon.property = flattenObject[i].property;

                    for (int j = 0; j < flattenObject[i].AroundPoints.Count; j++)
                    {
                        polygon.vertices[j] = _terraWorld.GetWorldPosition(flattenObject[i].AroundPoints[j]) - centerWorldPos;
                        polygon.vertices[j].Y = 0;
                    }

                    polygonVertices.Add(polygon);
                }

                mesh.GenerateMesh_TriangleNet(polygonVertices);
            }

            return mesh;
        }

        public static TMesh GenerateMesh(T2DGrid grid)
        {
            TDebug.TraceMessage();
            System.Numerics.Vector3[] vertices = new System.Numerics.Vector3[grid.VerticesList.Count];
            TGlobalPoint centerPos = grid.Center();
            System.Numerics.Vector3 centerWorldPos = (_terraWorld.GetWorldPosition(centerPos));

            for (int i = 0; i < grid.VerticesList.Count; i++)
            {
                TGlobalPoint node = grid.VerticesList[i];
                System.Numerics.Vector3 WorldPos = (_terraWorld.GetWorldPosition(node));
                vertices[i] = WorldPos - centerWorldPos;
                vertices[i].Y += 0.2f;
            }

            TMesh mesh = new TMesh();
            Vector3[] verticesUnity = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                verticesUnity[i] = TUtils.CastToUnity(vertices[i]);

            int[] indices = grid.GetIndices();
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < uvs.Length; i++)
                uvs[i] = new Vector2(vertices[i].X, vertices[i].Z);

            Mesh unityMesh = new Mesh();
            unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            unityMesh.vertices = verticesUnity;
            unityMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            unityMesh.uv = uvs;
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateBounds();
            mesh.Mesh = unityMesh;

            return mesh;
        }

        public static float GetProgressSlice()
        {
            float progressSlide = 100 / ((_NodesCount * _terraWorld._terrains.Count) + 1);
            return progressSlide;
        }

        public static void ClearMemory()
        {
            TDebug.TraceMessage();

            AssetDatabase.Refresh();
            Resources.UnloadUnusedAssets();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();

            Resources.UnloadUnusedAssets();
            SceneView.RepaintAll();

            AssetDatabase.Refresh();
        }

        public static void ResetCodes()
        {
            TDebug.TraceMessage();

            string tempClass =
@"public class TempCancelDate
{
    string dateTime = " +
"\"" + DateTime.Now.ToString() + "\"" + @";
}";

            using (StreamWriter file = File.CreateText("Assets/TempCancelDate.cs"))
            {
                file.Write(tempClass);
            }

            AssetDatabase.Refresh();
        }
    }
#endif
}
#endif
#endif

