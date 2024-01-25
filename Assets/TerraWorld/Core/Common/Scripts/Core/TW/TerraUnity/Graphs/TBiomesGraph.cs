#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;


namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum BiomeMasks
    {
        Biome_Type_Filter,
        Area_Mixer
    }

    public enum BiomeScatters
    {
        Terrain_Tree_Scatter,
        Object_Scatter,
        GPU_Instance_Scatter,
        Grass_Scatter
    }

    public enum BiomeMeshGenerators
    {
        Water_Generator,
        Terrain_Mesh_Generator
    }

    public enum BiomeTypes
    {
        Waters,
        Lakes,
        Sea,
        River,
        Trees,
        Wood,
        Meadow,
        Orchard,
        Grass,
        Greenfield,
        Beach,
        Wetland,
        Bay
    }


    // Entry
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TWaterModules : TNode
    {
        [XmlIgnore] public TLakeLayer _lakeLayer;
        [XmlIgnore] public TRiverLayer _riverLayer;
        [XmlIgnore] public TOceanLayer _oceanLayer;

        public TWaterModules() : base()
        {
        }
    }

    public abstract class TGridModules : TNode
    {
        [XmlIgnore] public TGridLayer _gridLayer;

        public TGridModules() : base()
        {
        }
    }

    public abstract class TScatteredObjectModules : TNode
    {
        [XmlIgnore] public TObjectScatterLayer _objectScatterLayer;

        public TScatteredObjectModules() : base()
        {
        }
    }

    public abstract  class TScatteredInstanceModules : TNode
    {
        [XmlIgnore] public TInstanceScatterLayer _instanceScatterLayer;

        public TScatteredInstanceModules() : base()
        {
        }
    }

    public abstract class TScatteredGrassModules : TNode
    {
        [XmlIgnore] public TGrassScatterLayer _grassScatterLayer;

        public TScatteredGrassModules() : base()
        {
        }
    }


    // Extractors
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("BiomeExtractor")]
    //public class BiomeExtractor : TMultiMaskModules
    public class BiomeExtractor : TNode
    {
        public BiomeTypes biomeType = BiomeTypes.Lakes;
        public bool bordersOnly = false;
        public int edgeSize = 1;
        public float riverWidth = 100;
        public float scaleFactor = 1f;
        public string XMLMaskData;
        public float MinSize = 2000;
        public bool FixWithImage = false;
        

        [XmlIgnore] private TMap _currentMap;

        public BiomeExtractor() : base()
        {
         //   type = typeof(BiomeExtractor).FullName;
            Data.moduleType = ModuleType.Extractor;
            Data.name = "Biome Filter ";
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;

            _currentMap = CurrentMap;
            OutMasks.Clear();

            if (isActive)
            {

                OutMasks = TModuleActions.BiomeExtractor(CurrentMap, biomeType, MinSize, bordersOnly, edgeSize, scaleFactor, riverWidth);

                _progress = 1;
            }

            isDone = true;
        }
    }


    // Scatters
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("TreeScatter")]
    public class TreeScatter : TScatteredObjectModules
    {
        public string prefabName;
        public int seedNo;
        //public int densityResolutionPerKilometer = 500;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public int priority = 0;
        //public Vector3 objectScale = Vector3.One;
        public float minRange = 0;
        public float maxRange = 1;
        //public string unityLayerName = "Default";
        public int maskLayer = ~0;
        //public string layerName;
        //public bool isWorldOffset = true;

        public float averageDistance = 10f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;

        public TreeScatter() : base()
        {
           // type = typeof(TreeScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "Terrain Tree Scatter" ;
            //layerName = Data.name;
            seedNo = Data.ID;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.ObjectScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(prefabName)) result.Add(prefabName);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            _objectScatterLayer = null;

            if (isActive)
            {
                List<TMask> masks = null;
                if (string.IsNullOrEmpty(prefabName) || !File.Exists(Path.GetFullPath(prefabName))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");
                
                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                    masks = preNode?.OutMasks;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _objectScatterLayer = TModuleActions.TreeScatter
                            (
                                prefabName,
                                seedNo,
                                bypassLakes,
                                underLakes,
                                underLakesMask,
                                onLakes,
                                minRotationRange,
                                maxRotationRange,
                                positionVariation,
                                scaleMultiplier,
                                minScale,
                                maxScale,
                                minSlope,
                                maxSlope,
                                priority,
                                minRange,
                                maxRange,
                                maskLayer,
                                averageDistance,
                                checkBoundingBox,
                                maxElevation,
                                minElevation,
                                masks,
                                Data.name

                            );
                _progress = 1;
            }

            isDone = true;
        }
    }


    [XmlType("ObjectScatter")]
    public class ObjectScatter : TScatteredObjectModules
    {
        public List<string> prefabNames;
        public int seedNo;
        public bool rotation90Degrees = false;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public bool hasCollider = true;
        public bool hasPhysics = false;
        public string unityLayerName = "Default";
        public int maskLayer = ~0;
        public string layerName;
        public float minSlope = 0;
        public float maxSlope = 90;
        public Vector3 positionOffset = Vector3.Zero;
        public Vector3 rotationOffset = Vector3.Zero;
        public int priority = 0;
        public List<ObjectBounds> bounds;
        public List<Vector3> objectScales;
        public float minRange = 0;
        public float maxRange = 1;
        public float averageDistance = 10f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;
        public bool placeSingleItem = false;

        public ObjectScatter() : base()
        {
            //type = typeof(ObjectScatter).FullName;
            Data.name = "Object Scatter";
            prefabNames = new List<string>();
            //prefabNames.Add("");
            layerName = Data.name;
            seedNo = Data.ID;
            Data.moduleType = ModuleType.Scatter;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.ObjectScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();

            if (prefabNames.Count == 0)
                result.Add(null);
            else
            {
                for (int i = 0; i < prefabNames.Count; i++)
                {
                    if (!string.IsNullOrEmpty(prefabNames[i]) && !string.IsNullOrEmpty(prefabNames[i])) result.Add(prefabNames[i]);
                    else result.Add(null);
                }
            }

            return result;
        }

        public override void ModuleAction (TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            _objectScatterLayer = null;

            if (isActive)
            {
                List<TMask> masks = null;
                if (prefabNames.Count < 1) throw new Exception("No Prefab selected for " + Data.name + "\n\n Please Check The Node.");

                for (int i = 0; i < prefabNames.Count; i++)
                    if (string.IsNullOrEmpty(prefabNames[i]) || !File.Exists(Path.GetFullPath(prefabNames[i]))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                    masks = preNode?.OutMasks;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _objectScatterLayer = TModuleActions.ObjectScatter
                    (
                         prefabNames,
                         seedNo,
                         rotation90Degrees,
                         bypassLakes,
                         underLakes,
                         underLakesMask,
                         onLakes,
                         lockYRotation,
                         getSurfaceAngle,
                         minRotationRange,
                         maxRotationRange,
                         positionVariation,
                         scaleMultiplier,
                         minScale,
                         maxScale,
                         hasCollider,
                         hasPhysics,
                         unityLayerName,
                         maskLayer,
                         Data.name,
                         minSlope,
                         maxSlope,
                         positionOffset,
                         rotationOffset,
                         priority,
                         bounds,
                         objectScales,
                         minRange,
                         maxRange,
                         averageDistance,
                         checkBoundingBox,
                         maxElevation,
                         minElevation,
                         placeSingleItem,
                         masks
                    );


                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("InstanceScatter")]
    public class InstanceScatter : TScatteredInstanceModules
    {
        public string prefabName;
        public int seedNo;
        public float averageDistance = 10f;
        //public int gridResolution = 100;
        public bool rotation90Degrees = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public string unityLayerName = "Default";
        public int maskLayer = ~0;
        public string layerName;
        public float minSlope = 0;
        public float maxSlope = 90;
        public Vector3 positionOffset = Vector3.Zero;
        public Vector3 rotationOffset = Vector3.Zero;
        public int priority = 0;
        public List<ObjectBounds> bounds;
        public float minRange = 0;
        public float maxRange = 1;
        public bool receiveShadows = true;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public float LODMultiplier = 1.0f;
        [XmlIgnore] private List<TMask> _masks = new List<TMask>();
        public bool isWorldOffset = true; //TODO: Ali should implement this
        public bool prefabHasCollider = false;
        public float maxDistance = 2000f;
        public float frustumMultiplier = 1.1f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;
        public bool occlusionCulling = false;

        public InstanceScatter() : base()
        {
            //type = typeof(InstanceScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "GPU Instance Scatter";
            layerName = Data.name;
            seedNo = Data.ID;
            //lastNumber++;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.InstanceScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(prefabName) ) result.Add(prefabName);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            _instanceScatterLayer = null;

            if (isActive )
            {
                List<TMask> masks = null;
                if (string.IsNullOrEmpty(prefabName) || !File.Exists(Path.GetFullPath(prefabName))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                    masks = preNode?.OutMasks;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _instanceScatterLayer = TModuleActions.InstanceScatter
                (
                    prefabName,
                    seedNo,
                    averageDistance,
                    //gridResolution,
                    rotation90Degrees,
                    lockYRotation,
                    getSurfaceAngle,
                    minRotationRange,
                    maxRotationRange,
                    positionVariation,
                    scaleMultiplier,
                    minScale,
                    maxScale,
                    unityLayerName,
                    maskLayer,
                    layerName,
                    minSlope,
                    maxSlope,
                    positionOffset,
                    rotationOffset,
                    priority,
                    bounds,
                    minRange,
                    maxRange,
                    receiveShadows,
                    bypassLakes,
                    underLakes,
                    underLakesMask,
                    onLakes,
                    shadowCastingMode,
                    LODMultiplier,
                    _masks,
                    isWorldOffset,
                    prefabHasCollider,
                    maxDistance,
                    frustumMultiplier,
                    checkBoundingBox,
                    maxElevation,
                    minElevation,
                    occlusionCulling,
                    masks,
                    Data.name
                );

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("GrassScatter")]
    public class GrassScatter : TScatteredGrassModules
    {
        [XmlIgnore] private TMaterial material;
        public int maxParallelJobCount = 50;
        public Vector2 scale = Vector2.One;
        public float radius = 300f;
        public float gridSize = 50f;
        public float slant = 0.2f;
        public float groundOffset = 0;
        public int amountPerBlock = 2000;
        public string unityLayerName = "Default";
        public string layerName;
        public float alphaMapThreshold = 0.2f;
        public float densityFactor = 0.2f;
        public BuilderType builderType = BuilderType.Quad;
        public NormalType normalType = NormalType.Up;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public int seedNo;
        public float minRange = 0;
        public float maxRange = 1;
        public float minSlope = 0;
        public float maxSlope = 90;
        public float maxElevation = 100000;
        public float minElevation = -100000;
        public float maskDamping = 7;

        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }
        public string Modelpath;
        public string MeshName;

        public bool layerBasedPlacement = false;
        public bool bypassWater = true;
        public bool underWater = false;
        public bool onWater = false;
        public int maskLayer = ~0;

        public GrassScatter() : base()
        {
          //  type = typeof(GrassScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "Grass Scatter";
            layerName = Data.name;
            seedNo = Data.ID;
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.GrassScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            
            if (!string.IsNullOrEmpty(Materialpath)) result.Add(Materialpath);
            else result.Add(null);

            if (!string.IsNullOrEmpty(Modelpath) && builderType != BuilderType.Quad) result.Add(Modelpath);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            _grassScatterLayer = null;

            if (isActive)
            {
                List<TMask> masks = null;

                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name + "\n\n Please Check The Node.");

                if (builderType == BuilderType.FromMesh && (string.IsNullOrEmpty(Modelpath) || !File.Exists(Path.GetFullPath(Modelpath))))
                    throw new Exception("No Mesh Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                    masks = preNode?.OutMasks;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _grassScatterLayer = TModuleActions.GrassScatter
                (
                    material,
                    maxParallelJobCount,
                    scale,
                    radius,
                    gridSize,
                    slant,
                    groundOffset,
                    amountPerBlock,
                    unityLayerName,
                    layerName,
                    alphaMapThreshold,
                    densityFactor,
                    builderType,
                    normalType,
                    shadowCastingMode,
                    seedNo,
                    minSlope,
                    maxSlope,
                    minElevation,
                    maxElevation,
                    minRange,
                    maxRange,
                    Modelpath,
                    MeshName,
                    layerBasedPlacement,
                    bypassWater,
                    underWater,
                    onWater,
                    maskLayer,
                    masks,
                    Data.name,
                    maskDamping
                );

                _progress = 1;
            }

            isDone = true;
        }
    }

    // Mesh Generators
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("WaterGenerator")]
    public class WaterGenerator : TWaterModules
    {
        [XmlIgnore] private TMaterial material;
        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }

        public string layerName;
        public float lodCulling = 25f;
        public int AroundPointsDensity;
        public float AroundVariation;
        public string unityLayerName;
        public string XMLMaskData;
        public Vector3 positionOffset = Vector3.Zero;
        public int priority = 0;
        public bool GenerateLakes = true;
        public bool GenerateOceans = true;
        public bool GenerateRiver = true;
        public float RiverWidthInMeter = 150;
        public float Depth = 0.5f;
        public float LakeMinSizeInM2 = 20000;
        public List<Vector2> boundingPoints;
        public bool smoothOperation = true;
        public float deformAngle = 10f;
        
        [XmlIgnore] private TMap _currentMap;
        [XmlIgnore] public Bitmap lakeAroundMask;
        [XmlIgnore] public Bitmap maskImage;

        public WaterGenerator() : base()
        {
            //type = typeof(WaterGenerator).FullName;
            Data.moduleType = ModuleType.TerraMesh;
            Data.name = "Water Generator";
            layerName = "Water";
            unityLayerName = "Water";
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Lakes;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(Materialpath) ) result.Add(Materialpath);
            else result.Add(null);
            return result;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;
            _currentMap = CurrentMap;
            _lakeLayer = null;
            _riverLayer = null;
            _oceanLayer = null;
            Vector2 centerPointNormalized = Vector2.Zero;
            float deformAngleRadian = deformAngle * (float)Math.PI / 180f;

            if (isActive)
            {
                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name + "\n\n Please Check The Node.");

                if (GenerateLakes)
                {
                    _lakeLayer = TModuleActions.GetLakes
                    (
                        CurrentMap.LandcoverXML,
                        material,
                        lodCulling,
                        AroundPointsDensity,
                        AroundVariation,
                        unityLayerName,
                        positionOffset,
                        priority,
                        Depth,
                        LakeMinSizeInM2,
                        deformAngle,
                        _currentMap,
                        Data.name
                    );
                }
               
                if (GenerateOceans)
                {
                    _oceanLayer = TModuleActions.GetOceans
                    (
                        CurrentMap.LandcoverXML,
                        material,
                        lodCulling,
                        unityLayerName,
                        positionOffset,
                        priority,
                        Depth,
                        deformAngle,
                        _currentMap,
                        Data.name
                    );
                }

                if (GenerateRiver)
                {
                    _riverLayer = TModuleActions.GetRivers
                    (
                        CurrentMap.LandcoverXML,
                        material,
                        lodCulling,
                        unityLayerName,
                        positionOffset,
                        priority,
                        RiverWidthInMeter,
                        Depth,
                        smoothOperation,
                        _currentMap,
                        Data.name
                    );
                }

                if (_riverLayer?.WaterMasks?.Count > 0 || _lakeLayer?.WaterMasks?.Count > 0 || _oceanLayer?.WaterMasks?.Count > 0)
                {
                    //  THeightmapProcessors.DeformByMask(ref _currentMap._refTerrain.Heightmap.heightsData, Deformmask, Depth * 1.5f, false, null);
                    if (smoothOperation)
                    {
                        _currentMap._refTerrain.Heightmap.heightsData = THeightmapProcessors.SmoothHeightmap
                        ( 
                            _currentMap._refTerrain.Heightmap.heightsData, 1, 0, THeightmapProcessors.Neighbourhood.Moore
                        );
                    }
                }

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("MeshGenerator")]
    public class MeshGenerator : TGridModules
    {
        //private string materialpath;
        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }
        public int densityResolutionPerKilometer = 5;
        public int density = 90;
        public float edgeCurve = -1;
        public int gridCount = 16;
        public float lodCulling = 25f;
        public Vector3 scale = Vector3.One;
        public string unityLayerName = "Default";
        public bool hasShadowCasting = false;
        public bool hasCollider = false;
        public bool hasPhysics = false;
        public bool SeperatedObject = false;
        public string layerName;
        public Vector3 positionOffset = Vector3.Zero;
        public int priority = 0;
        [XmlIgnore] private TMaterial material;
        [XmlIgnore] public List<TMask> _masks;

        public MeshGenerator() : base()
        {
            //type = typeof(MeshGenerator).FullName;
            Data.moduleType = ModuleType.TerraMesh;
            Data.name = "Terrain Mesh Generator";
            _masks = new List<TMask>();
            layerName = Data.name;
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.Mesh;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(Materialpath) ) result.Add(Materialpath);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            _gridLayer = null;

            if (isActive)
            {
                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name);

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                        _masks = preNode.OutMasks;

                    if (_masks.Count < 1) return;
                }

                _gridLayer = TModuleActions.GetMeshLayer
                (
                    densityResolutionPerKilometer,
                    density,
                    edgeCurve,
                    gridCount,
                    lodCulling,
                    scale,
                    unityLayerName,
                    hasShadowCasting,
                    hasCollider,
                    hasPhysics,
                    SeperatedObject,
                    layerName,
                    positionOffset,
                    priority,
                    material,
                    _masks,
                    currentMap,
                    Data.name
                );

                _progress = 1;
            }

            isDone = true;
        }
    }


    // Graph
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    public class TBiomesGraph : TGraph
    {
        //TODO: Check ConnectionDataType
        public TBiomesGraph() : base(ConnectionDataType.Lakes, "BIOMES Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {

            worldGraph = terraWorldGraph;
            _title = "BIOMES";
        }

        public void AddNode(BiomeMasks biomeMasks)
        {
            if (biomeMasks.Equals(BiomeMasks.Biome_Type_Filter))
            {
                BiomeExtractor node = new BiomeExtractor();
                node.Init(this);
                node.Data.moduleType = ModuleType.Extractor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            if (biomeMasks.Equals(BiomeMasks.Area_Mixer))
            {
                MaskBlendOperator node = new MaskBlendOperator();
                node.Init(this);
                node.Data.moduleType = ModuleType.Operator;
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }

            UpdateConnections();
        }

        public void AddNode(BiomeScatters biomeScatters)
        {
            if (biomeScatters.Equals(BiomeScatters.Object_Scatter))
            {
                ObjectScatter node = new ObjectScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.Terrain_Tree_Scatter))
            {
                TreeScatter node = new TreeScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.GPU_Instance_Scatter))
            {
                InstanceScatter node = new InstanceScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.Grass_Scatter))
            {
                GrassScatter node = new GrassScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
        }

        public void AddNode(BiomeMeshGenerators biomeMeshGenerators)
        {
            if (biomeMeshGenerators.Equals(BiomeMeshGenerators.Water_Generator))
            {
                WaterGenerator node = new WaterGenerator();
                node.Init(this);
                node.Data.moduleType = ModuleType.TerraMesh;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeMeshGenerators.Equals(BiomeMeshGenerators.Terrain_Mesh_Generator))
            {
                MeshGenerator node = new MeshGenerator();
                node.Init(this);
                node.Data.moduleType = ModuleType.TerraMesh;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
        }

        public bool AnyLandCoverDataNode ()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetType() == typeof(WaterGenerator)) return true;
                if (nodes[i].GetType() == typeof(BiomeExtractor)) return true;
            }

            return false;
        }
    }
#endif
}
#endif
#endif
