#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using TerraUnity.Utils;
using TerraUnity.Runtime;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
#endif

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class TTerrain
    {
        private TArea _area;
        private TMap map;
        private TTerraWorld _terraWorldRef;
        private Action<TTerrain> _lastActions;
        public TDetailTextureCollection detailTextureCollection;
        public TDetailTextureCollection colorMapTextureCollection;
        private List<TOceanLayer> _oceanLayers = new List<TOceanLayer>();
        private List<TLakeLayer> _lakeLayers = new List<TLakeLayer>();
        private List<TRiverLayer> _riverLayers = new List<TRiverLayer>();
        private List<TGridLayer> _gridsLayers = new List<TGridLayer>();
        private List<TObjectScatterLayer> _objectScatterLayers = new List<TObjectScatterLayer>();
        private List<TInstanceScatterLayer> _instanceScatterLayers = new List<TInstanceScatterLayer>();
        private List<TGrassScatterLayer> _grassScatterLayers = new List<TGrassScatterLayer>();
        private THeightmap _heightmap;
        private string _ID;
        private float _graphProgress;
        private Exception exception = null;
        public TMask overallWaterMask = null;
        private int maximumElevationResolution = 4096;
        public float[,] bgHeightmap = null;
        public Bitmap bgImage = null;
        public int PixelError = 5;
        public bool splatmapResolutionBestFit = true;
        public int splatmapResolution = 1024;
        public int splatmapSmoothness = 1;

        // Background Terrain Settings
        //-----------------------------------------------------------------------
        public bool BGMountains;
        public int BGTerrainScaleMultiplier;
        public int BGTerrainHeightmapResolution;
        public int BGTerrainSatelliteImageResolution;
        public int BGTerrainPixelError;
        public float BGTerrainOffset;

        public float Progress
        {
            get
            {
                if (Map != null && Map.Progress > 0)
                    return (float)(_graphProgress * 0.7 + 0.3 * Map.Progress);
                else
                    return (float)(_graphProgress * 0.7 + 0.3);
            }
        }

        public TArea Area { get => _area; }
        public TMap Map { get => map; set => map = value; }
        public TTerraWorld TerraWorld { get => _terraWorldRef; }
        public Action<TTerrain> LastActions { get => _lastActions; set => _lastActions = value; }
        public List<TLakeLayer> LakeLayer { get => _lakeLayers; }
        public List<TOceanLayer> OceanLayer { get => _oceanLayers; }
        public List<TRiverLayer> RiverLayer { get => _riverLayers; }
        public THeightmap Heightmap { get => _heightmap; set => _heightmap = value; }
        public List<TGridLayer> GridsLayers { get => _gridsLayers; }
        public List<TObjectScatterLayer> ObjectScatterLayers { get => _objectScatterLayers; }
        public List<TInstanceScatterLayer> InstanceScatterLayers { get => _instanceScatterLayers; }
        public List<TGrassScatterLayer> GrassScatterLayers { get => _grassScatterLayers; }

        public static TMap currentMap;

        //Bitmap aspectMap, curvatureMap, flowMap, normalMap, slopeMap;

        public TTerrain(double top, double left, double bottom, double right, TTerraWorld terraWorld)
        {
            _area = new TArea(top, left, bottom, right);
            _terraWorldRef = terraWorld;
            _ID = TTerraWorldGraph.GetNewID().ToString();
            detailTextureCollection = new TDetailTextureCollection(this);
            colorMapTextureCollection = new TDetailTextureCollection(this);
            _oceanLayers = new List<TOceanLayer>();
            _lakeLayers = new List<TLakeLayer>();
            _riverLayers = new List<TRiverLayer>();
            Heightmap = new THeightmap();
        }

        public void UpdateTerrain()
        {
            TDebug.TraceMessage();
            TTerrainGenerator.SetStatusToOnProgress();
            //bool requestElevationData = false;
            //bool requestImageData = false;
            bool requestLandcoverData = false;
            HeightmapSource heightmapSource = TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource();
            SatelliteImage satelliteImage = TTerraWorld.WorldGraph.colormapGraph.SatelliteImage();

            if (heightmapSource != null)
            {
                TerraWorld.ElevationSource = heightmapSource._source;
                TerraWorld.ElevationZoomLevel = heightmapSource.highestResolution ? TMap.GetZoomLevel(maximumElevationResolution, _area) : TMap.GetZoomLevel(heightmapSource._resolution, _area);
                if (TerraWorld.ElevationZoomLevel > 16) TerraWorld.ElevationZoomLevel = 16;
                //requestElevationData = true;
            }
            else
                TerraWorld.ElevationZoomLevel = -1;

            if (satelliteImage != null)
            {
                TerraWorld.ImagerySource = satelliteImage._source;
                TerraWorld.ImageZoomLevel = TMap.GetZoomLevel(satelliteImage.resolution, _area);
                //requestImageData = true;
            }
            else
                TerraWorld.ImageZoomLevel = -1;

            if (TTerraWorld.WorldGraph.biomesGraph.AnyLandCoverDataNode())
                requestLandcoverData = true;

#if TERRAWORLD_XPRO
            TXRealWorldSourceNode XRWSourceNode = TTerraworldGenerator.XGraph.GetRealWorldSourceNode();

            if (XRWSourceNode != null)
            {
                TerraWorld.ImagerySource = XRWSourceNode.ImagerySource;
                TerraWorld.ImageZoomLevel = TMap.GetZoomLevel(XRWSourceNode.ImageryResolution, _area);
                TerraWorld.ElevationSource = XRWSourceNode.HeightmapSource;
                TerraWorld.ElevationZoomLevel = XRWSourceNode.highestResolution ? TMap.GetZoomLevel(maximumElevationResolution, _area) : TMap.GetZoomLevel(XRWSourceNode.HeightmapResolution, _area);
                requestLandcoerData = true;
            }
#endif

            Map = new TMap(_area.Top, _area.Left, _area.Bottom, _area.Right, this, TerraWorld.ImageZoomLevel, TerraWorld.ElevationZoomLevel, requestLandcoverData, TerraWorld.LandcoverImageryZoomLevel);
            Map.SaveTilesImagery = TTerraWorld.CacheData;
            Map.SaveTilesElevation = TTerraWorld.CacheData;
            Map.SaveTilesLandcover = TTerraWorld.CacheData;
            Map._mapElevationSource = TerraWorld.ElevationSource;
            Map._mapImagerySource = TerraWorld.ImagerySource;
            Map._mapLandcoverSource = TerraWorld.LandcoverSource;
            //Map.RequestElevationData = requestElevationData;
            //Map.RequestImageData = requestImageData;

            Map.UpdateMap(TerrainAnalysis);
            currentMap = Map;
        }

        private void TerrainAnalysis(TMap CurrentMap)
        {
            TDebug.TraceMessage();

            //Main Thread Option
            //RunAllModules(CurrentMap._refTerrain);
            //WhenAllDone();

             TTerraWorldGraph terraWorldGraph = TTerraWorld.WorldGraph;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RunAllModulesAsync(CurrentMap._refTerrain, terraWorldGraph);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task RunAllModulesAsync(TTerrain terrain, TTerraWorldGraph terraWorldGraph)
        {
            TDebug.TraceMessage();
            exception = null;

#if TERRAWORLD_XPRO
            //RunAllModulesASyncPerform(terrain);
            await Task.Run(() => RunAllModulesASyncPerform(terrain));
#else
            await Task.Run(() => RunAllModulesASyncPerform(terrain, terraWorldGraph));
#endif

            if (exception == null)
            {
                if (TTerrainGenerator.WorldInProgress)
                    WhenAllDone();
            }
            else
                TDebug.LogErrorToUnityUI(exception);
        }

        public void RunAllModulesASyncPerform(TTerrain terrain, TTerraWorldGraph terraWorldGraph)
        {
            TDebug.TraceMessage();

            try
            {
                RunAllModules(terrain, terraWorldGraph);
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        public void RunAllModules(TTerrain terrain, TTerraWorldGraph terraWorldGraph)
        {
            TDebug.TraceMessage();
            if (terrain.map.Heightmap.heightsData == null) throw new Exception("Internal Error : 02");

            _graphProgress = 5;
            overallWaterMask = new TMask(terrain.map.Heightmap.heightsData.GetLength(0), terrain.map.Heightmap.heightsData.GetLength(1));
            terrain.Heightmap.heightsData = terrain.map.Heightmap.heightsData;

            // Heightmap Interpreter
            //---------------------------------------------------------------------------------------------------------------------------------------------------

#if TERRAWORLD_XPRO
            TXTerrainNode XTerrainNode = TTerraworldGenerator.XGraph.GetTerrainNode();
            if (!TTerrainGenerator.WorldInProgress) return;
            if (XTerrainNode == null) throw new Exception("There is no terrain generator node in graph. Insert a \"Terrain Generator Node\" ");

            terrain.PixelError = XTerrainNode.PixelError;
            terrain.BGMountains = XTerrainNode.BGMountains;
            terrain.BGTerrainScaleMultiplier = XTerrainNode.BGTerrainScaleMultiplier;
            terrain.BGTerrainHeightmapResolution = XTerrainNode.BGTerrainHeightmapResolution;
            terrain.BGTerrainSatelliteImageResolution = XTerrainNode.BGTerrainSatelliteImageResolution;
            terrain.BGTerrainPixelError = XTerrainNode.BGTerrainPixelError;
            terrain.BGTerrainOffset = XTerrainNode.BGTerrainOffset;

            terrain.Heightmap.heightsData = XTerrainNode.GetProceededHeightMap(terrain);
            TDebug.TraceMessage("Heightmap Node : " + XTerrainNode.NodeName);
#else
            List<TNode> HeightmapMaster = terraWorldGraph.heightmapGraph.RunGraph(terrain, ConnectionDataType.HeightmapMaster);
            if (!TTerrainGenerator.WorldInProgress) return;
            if (HeightmapMaster.Count > 1) throw new Exception("More than one end node detected on heightmap graph!");
            if (HeightmapMaster.Count < 1) throw new Exception("There is no end node for heightmap graph.");

            for (int i = 0; i < HeightmapMaster.Count; i++)
            {
                terrain.PixelError = ((HeightmapMaster)HeightmapMaster[0])._pixelError;
                THeightmapModules lastnode = (THeightmapModules)HeightmapMaster[i];

                if (lastnode._heightmapData != null)
                    terrain.Heightmap.heightsData = lastnode._heightmapData;

                TDebug.TraceMessage("heightmapGraph result : " + lastnode.Data.name);
            }
#endif
            _graphProgress = 20;
            PerformGC();

            // Lakes Interpreter
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXWaterGeneratorNode> waterGeneratorNodes = TTerraworldGenerator.XGraph.GetWaterGeneratorNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < waterGeneratorNodes.Count; i++)
            {
                TXWaterGeneratorNode lastnode = waterGeneratorNodes[i];
                TLakeLayer newlakesLayer = lastnode.GetLakes(terrain);
                TRiverLayer newriversLayer = lastnode.GetRivers(terrain);
                TOceanLayer newoceanLayer = lastnode.GetOceans(terrain);
                if (newlakesLayer != null) terrain.AddLakeLayer(newlakesLayer);
                if (newriversLayer != null) terrain.AddRiverLayer(newriversLayer);
                if (newoceanLayer != null) terrain.AddOceanLayer(newoceanLayer);
                TDebug.TraceMessage("Water Node : " + lastnode.NodeName);

            }
#else
            List<TNode> Lakes = terraWorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.Lakes);
            if (!TTerrainGenerator.WorldInProgress) return;


            for (int i = 0; i < Lakes.Count; i++)
            {
                TWaterModules lastnode = (TWaterModules)Lakes[i];
                TLakeLayer newlakesLayer = lastnode._lakeLayer;
                TRiverLayer newriversLayer = lastnode._riverLayer;
                TOceanLayer newoceanLayer = lastnode._oceanLayer;
                if (newlakesLayer != null) terrain.AddLakeLayer(newlakesLayer);
                if (newriversLayer != null) terrain.AddRiverLayer(newriversLayer);
                if (newoceanLayer != null) terrain.AddOceanLayer(newoceanLayer);
                TDebug.TraceMessage("Water result : " + lastnode.Data.name);
            }
#endif
            _graphProgress = 40;
            PerformGC();

            // ColorMap
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            if (!TTerrainGenerator.WorldInProgress) return;
            TDetailTexture XcolorMap = XTerrainNode.GetColorMap(terrain);

            if (XcolorMap != null )
            {
                terrain.colorMapTextureCollection.Add(XcolorMap);
            }
#else
            List<TNode> ColormapMaster = terraWorldGraph.colormapGraph.RunGraph(terrain, ConnectionDataType.ColormapMaster);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < ColormapMaster.Count; i++)
            {
                TImageModules lastnode = (TImageModules)ColormapMaster[i];
                TDetailTexture colorMapDetailTexture = lastnode._detailTexture;

                if (colorMapDetailTexture != null)
                {
                    terrain.colorMapTextureCollection.Add(colorMapDetailTexture);
                    TDebug.TraceMessage("Graph result (Colormap): " + lastnode.Data.name);
                }
            }

#endif
            _graphProgress = 50;
            PerformGC();

            // TerrainLayers
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TDetailTexture> terrainLayers = XTerrainNode.GetDetailedTextures(terrain);
            if (!TTerrainGenerator.WorldInProgress) return;
            terrain.splatmapResolutionBestFit = XTerrainNode.splatmapResolutionBestFit;
            terrain.splatmapResolution = XTerrainNode.splatmapResolution;
            terrain.splatmapSmoothness = XTerrainNode.splatmapSmoothness;

            for (int i = 0; i < terrainLayers.Count; i++)
            {
                if (terrainLayers[i] != null)
                {
                    terrain.detailTextureCollection.Add(terrainLayers[i]);
                }
            }
#else
            List<TNode> DetailTextureMasters = terraWorldGraph.colormapGraph.RunGraph(terrain, ConnectionDataType.DetailTextureMaster);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < DetailTextureMasters.Count; i++)
            {
                TImageModules lastnode = (TImageModules)DetailTextureMasters[i];
                TDetailTexture detailTexture = lastnode._detailTexture;

                if (detailTexture != null)
                {
                    terrain.detailTextureCollection.Add(detailTexture);
                    TDebug.TraceMessage("Graph result (DetailTexture): " + lastnode.Data.name);
                }
            }
#endif

            _graphProgress = 60;
            PerformGC();

            // Meshes
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXMeshGeneratorNode> meshGeneratorNodes = TTerraworldGenerator.XGraph.GetMeshGeneratorNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < meshGeneratorNodes.Count; i++)
            {
                TXMeshModules lastnode = meshGeneratorNodes[i];
                TGridLayer newGridLayer = lastnode.GetMeshLayer(terrain);
                if (newGridLayer != null) terrain.AddGridLayer(newGridLayer);
            }
#else
            List<TNode> Mesh = terraWorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.Mesh);
            if (!TTerrainGenerator.WorldInProgress) return;
        
            for (int i = 0; i < Mesh.Count; i++)
            {
                TGridModules lastnode = (TGridModules)Mesh[i];
                TGridLayer newGridLayer = lastnode._gridLayer;
                if (newGridLayer != null) terrain.AddGridLayer(newGridLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif
            _graphProgress = 70;
            PerformGC();

            // Object Scatters
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXObjectScatterNode> ObjectScattersNodes = TTerraworldGenerator.XGraph.GetObjectScatterNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < ObjectScattersNodes.Count; i++)
            {
                TXObjectScatterModules lastnode = ObjectScattersNodes[i];
                TObjectScatterLayer newobjectsLayer = lastnode.GetObjectsLayer(terrain);
                if (newobjectsLayer != null) terrain.AddObjectScatterLayer(newobjectsLayer);
            }
#else
            List<TNode> ObjectScatters = terraWorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.ObjectScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < ObjectScatters.Count; i++)
            {
                TScatteredObjectModules lastnode = (TScatteredObjectModules)ObjectScatters[i];
                TObjectScatterLayer newobjectsLayer = lastnode._objectScatterLayer;
                if (newobjectsLayer != null) terrain.AddObjectScatterLayer(newobjectsLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif
            _graphProgress = 80;
            PerformGC();

            // Instance Scatters
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXInstanceScatterNode> InstanceScattersNodes = TTerraworldGenerator.XGraph.GetInstanceScatterNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < InstanceScattersNodes.Count; i++)
            {
                TXInstanceScatterModules lastnode = InstanceScattersNodes[i];
                TInstanceScatterLayer newInstanceLayer = lastnode.GetInstanceLayer(terrain);
                if (newInstanceLayer != null) terrain.AddInstanceScatterLayer(newInstanceLayer);
            }
#else
            List<TNode> InstanceScatters = terraWorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.InstanceScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < InstanceScatters.Count; i++)
            {
                TScatteredInstanceModules lastnode = (TScatteredInstanceModules)InstanceScatters[i];
                TInstanceScatterLayer newInstancesLayer = lastnode._instanceScatterLayer;
                if (newInstancesLayer != null) terrain.AddInstanceScatterLayer(newInstancesLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif

            _graphProgress = 90;
            PerformGC();

            // Grass Scatters
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXGrassScatterNode> GrassScattersNodes = TTerraworldGenerator.XGraph.GetGrassScatterNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < GrassScattersNodes.Count; i++)
            {
                TXGrassScatterModules lastnode = GrassScattersNodes[i];
                TGrassScatterLayer newGrassLayer = lastnode.GetGrassLayer(terrain);
                if (newGrassLayer != null) terrain.AddGrassScatterLayer(newGrassLayer);
            }
#else
            List<TNode> GrassScatters = terraWorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.GrassScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < GrassScatters.Count; i++)
            {
                TScatteredGrassModules lastnode = (TScatteredGrassModules)GrassScatters[i];
                TGrassScatterLayer newInstancesLayer = lastnode._grassScatterLayer;
                if (newInstancesLayer != null) terrain.AddGrassScatterLayer(newInstancesLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif

            _graphProgress = 100;
            PerformGC();
        }

        private void PerformGC()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
                throw new ArgumentException("The type must be serializable.", "source");

            // Don't serialize a null object, simply return the default for that object
            if (System.Object.ReferenceEquals(source, null))
                return default(T);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();

            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(stream);
            }
        }

        private bool IsAnyCollision(TPointObject pointObject)
        {
            for (int i = 0; i < _lakeLayers.Count; i++)
                for (int j = 0; j < _lakeLayers[i].LakesList.Count; j++)
                {
                    if (TUtils.PointInPolygon(_lakeLayers[i].LakesList[j].AroundPoints, pointObject.GeoPosition))
                        return true;
                }

            return false;
        }

        public void AddLakeLayer(TLakeLayer lakeLayer)
        {
            TMask mask = TMask.MergeMasks(lakeLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _lakeLayers.Add(lakeLayer);
        }

        public void AddRiverLayer(TRiverLayer riverLayer)
        {
            TMask mask = TMask.MergeMasks(riverLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _riverLayers.Add(riverLayer);
        }

        public void AddOceanLayer(TOceanLayer oceanLayer)
        {
            TMask mask = TMask.MergeMasks(oceanLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _oceanLayers.Add(oceanLayer);
        }

        public void AddGridLayer(TGridLayer GridLayer)
        {
            _gridsLayers.Add(GridLayer);
        }

        public void AddObjectScatterLayer(TObjectScatterLayer ObjectScatterLayer)
        {
            //if (!ObjectScatterLayer.underLake)
            //{
            //    for (int i = 0; i < ObjectScatterLayer.points.Count; i++)
            //    {
            //        TPointObject Object = ObjectScatterLayer.points[i];
            //
            //        if (IsAnyCollision(Object))
            //        {
            //            ObjectScatterLayer.points.Remove(Object);
            //            i--;
            //        }
            //    }
            //}

            _objectScatterLayers.Add(ObjectScatterLayer);
        }

        public void AddInstanceScatterLayer(TInstanceScatterLayer instanceScatterLayer)
        {
            _instanceScatterLayers.Add(instanceScatterLayer);
        }

        public void AddGrassScatterLayer(TGrassScatterLayer grassScatterLayer)
        {
            _grassScatterLayers.Add(grassScatterLayer);
        }

        public Vector3 GetWorldPositionWithHeight(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            Vector2 XZ = map.GetWorldPosition(geoPoint);
            float height = (float)_heightmap.GetInterpolatedHeight(latlonDeltaNormalized.Y, latlonDeltaNormalized.X);
            return new Vector3((float)XZ.X, height, (float)XZ.Y);
        }

        public Vector3 GetAngle(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            return _heightmap.GetInterpolatedNormal(latlonDeltaNormalized.X, latlonDeltaNormalized.Y);
        }

        public Vector2 GetNormalPositionN(TGlobalPoint geoPoint)
        {
            return Map.GetLatLongNormalizedPositionN(geoPoint);
        }

        public float GetSteepness(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            float result = _heightmap.GetSteepness(latlonDeltaNormalized.Y, latlonDeltaNormalized.X, map._area.AreaSizeLat * 1000, map._area.AreaSizeLon * 1000);
            return result;
        }

        public void WhenAllDone()
        {
            TDebug.TraceMessage();
            TerraWorld.EachTerrainDone(this);
        }
    }
#endif
}
#endif
#endif

