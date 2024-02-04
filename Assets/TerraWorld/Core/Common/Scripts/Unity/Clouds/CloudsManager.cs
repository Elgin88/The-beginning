using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using System.Xml.Serialization;

namespace TerraUnity.Runtime
{
    public struct CloudsManagerParams
    {
        public bool hasClouds;
        [XmlIgnore] public GameObject player;
        [XmlIgnore] public GameObject cloudPrefab;
        public bool isCustomShape;
        public float cloudMeshNormalOffset;
        [XmlIgnore] public Mesh cloudMesh;
        [XmlIgnore] public Mesh cloudMeshUser;
        public ParticleSystemRenderSpace particleSystemRenderSpace;
        public bool cloudsAroundPlayer;
        public int seed;
        public int cloudCount;
        [Range(0.01f, 100f)] public float density;
        public int maxParticles;
        public float cloudSize;
        public float particleSize;
        [Range(1f, 20f)] public float sizeMultiplier;
        [Range(0f, 1f)] public float randomParticleSize;
        public float altitude;
        public float altitudeRange;
        public bool randomRotation;
        [Range(0f, 360f)] public float rotationVariation;
        public float visibilityDistance;
        [Range(0f, 1f)] public float visibilityFalloff;
        [Range(0.001f, 3f)] public float visibilityFalloffPower;
        public Color tintColor;
        //[ColorUsage(true, true)]
        public Color emissionColor;
        [Range(0f, 1f)] public float cloudOpacity;
        [Range(0f, 3f)] public float textureMipMapBias;
        public bool castShadows;
        public bool windMovement;
        public bool isFlatShading;
        public bool isFlatShadingCustomShape;
        public float windSpeedMultiplier;
        public bool hasRotation;
        public bool hasFog;

#if UNITY_EDITOR
        public string CloudPrefabPath { get => AssetDatabase.GetAssetPath(cloudPrefab); set => cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(value); }
        public string CloudMeshPath { get => AssetDatabase.GetAssetPath(cloudMesh); set => cloudMesh = AssetDatabase.LoadAssetAtPath<Mesh>(value); }
        public string CloudMeshUserPath { get => AssetDatabase.GetAssetPath(cloudMeshUser); set => cloudMeshUser = AssetDatabase.LoadAssetAtPath<Mesh>(value); }
        public string CloudMeshUserName { get => GetMeshName(); set => SetMesh(value); }

        private void SetMesh(string meshName)
        {
            if (!string.IsNullOrEmpty(CloudMeshUserPath))
            {
                cloudMeshUser = TResourcesManager.GetMeshObject(CloudMeshUserPath, meshName);
            }
        }

        private string GetMeshName()
        {
            if (cloudMeshUser != null)
            {
                return cloudMeshUser.name;
            }
            else
                return string.Empty;
        }

#endif

        //[XmlIgnore] public Texture2D cloudMeshTexture;
        //public string CloudMeshTexturePath { get => AssetDatabase.GetAssetPath(cloudMeshTexture); set => cloudMeshTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(value); }
        //public float minWorldHeight;
        //public float maxWorldHeight;
    }

    [ExecuteAlways]
    public class CloudsManager : MonoBehaviour
    {
        private static CloudsManager Script { get => GetScript(); }
        private static CloudsManager _script;
        public static string cloudsPrefabName = "Clouds Prefab.prefab";

        private void OnValidate()
        {
            if (!_parameters.hasClouds) return;
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;

            // Needs regeneration
            if
            (
                HasClouds != _parameters.hasClouds ||
                cloudPrefab != _parameters.cloudPrefab ||
                density != _parameters.density ||
                maxParticles != _parameters.maxParticles ||
                cloudSize != new Vector3(_parameters.cloudSize, _parameters.cloudSize, _parameters.cloudSize) ||
                seed != _parameters.seed ||
                cloudCount != _parameters.cloudCount ||
                particleSize != _parameters.particleSize ||
                sizeMultiplier != _parameters.sizeMultiplier ||
                randomParticleSize != _parameters.randomParticleSize ||
                randomRotation != _parameters.randomRotation ||
                rotationVariation != _parameters.rotationVariation ||
                castShadows != _parameters.castShadows ||
                //minWorldHeight != _parameters.minWorldHeight ||
                //maxWorldHeight != parameters.maxWorldHeight ||
                cloudMesh != _parameters.cloudMesh ||
                cloudMeshUser != _parameters.cloudMeshUser ||
                cloudMeshNormalOffset != _parameters.cloudMeshNormalOffset ||
                isCustomShape != _parameters.isCustomShape ||
                particleSystemRenderSpace != _parameters.particleSystemRenderSpace ||
                hasRotation != _parameters.hasRotation ||
                altitudeRange != _parameters.altitudeRange
            )
                regenerateFlag = RegenerateState.HeavyUpdate;
            else
                regenerateFlag = RegenerateState.LightUpdate;

            Apply();
        }

        private static CloudsManager GetScript()
        {
            if (_script == null)
            {
                CloudsManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<CloudsManager>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<CloudsManager>();
            }
            else
            {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                if (_script.cloudPrefab == null)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath(TTerraWorldManager.WorkDirectoryLocalPath + cloudsPrefabName, typeof(GameObject)) as GameObject;

                    if (go != null)
                        _script.cloudPrefab = go;
                    else
                    {
                        TResourcesManager.LoadCloudsResources();
                        _script.cloudPrefab = TResourcesManager.cloudPrefab;
                    }
                }

                if (_script.cloudMesh == null)
                {
                    TResourcesManager.LoadCloudsResources();
                    _script.cloudMesh = TResourcesManager.cloudMesh;
                }
#endif
#endif
            }

            return _script;
        }

        [Header("Resources"), Space(5)]
        public GameObject player;
        public GameObject cloudPrefab;
        [HideInInspector] public Mesh cloudMesh; // This is being found internally in codes!

        [Header("Custom Shape"), Space(10)]
        public bool isCustomShape = false;
        public Mesh cloudMeshUser;
        public ParticleSystemRenderSpace particleSystemRenderSpace = ParticleSystemRenderSpace.World;
        public bool isFlatShadingCustomShape = false;
        public bool hasRotation = false;
        public bool hasFog = false;
        public float cloudMeshNormalOffset = 1f;

        [Header("Colors"), Space(10)]
        public Color tintColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        //[ColorUsage(true, true)]
        public Color emissionColor = new Color(0.789342f, 0.838732f, 0.898f, 1f);

        [Header("Rendering"), Space(10)]
        public bool castShadows = true;
        [Range(0f, 1f)] public float cloudOpacity = 1f;
        [Range(0f, 3f)] public float textureMipMapBias = 0f;
        public float visibilityDistance = 100000;
        [Range(0f, 1f)] public float visibilityFalloff = 1;
        [Range(0.001f, 3f)] public float visibilityFalloffPower = 1;
        public bool isFlatShading = false;

        [Header("Position"), Space(10)]
        public float altitude = 3500f;
        public float altitudeRange = 2500f;

        [Header("Particle Options"), Space(10)]
        public int seed = 12345;
        public int cloudCount = 40;
        [Range(0.01f, 100f)] public float density = 1;
        public int maxParticles = 40;
        public Vector3 cloudSize = new Vector3(250f, 250f, 250f);
        public float particleSize = 1750f;
        [Range(1f, 20f)] public float sizeMultiplier = 5f;
        [Range(0f, 1f)] public float randomParticleSize = 0.66f;
        public bool randomRotation = true;
        [Range(0f, 360f)] public float rotationVariation = 30f;
        public bool cloudsAroundPlayer = false;

        [Header("Wind Settings"), Space(10)]
        public bool windMovement = true;
        public float windSpeedMultiplier = 1f;

        //public float minWorldHeight = 700f;
        //public float maxWorldHeight = 1500f;
        //public Texture2D cloudMeshTexture;
        //public bool simulateWind = true;

        private GameObject _cloudsParent;
        private Texture cloudTexture;
        private Texture cloudTextureNormal;
        private Material cloudMaterial = null;
        //private bool createAtLevelStart = false;

        enum RegenerateState
        {
            Idle,
            LightUpdate,
            HeavyUpdate,
            Regenerate,
            Destroy
        }
        private RegenerateState regenerateFlag = RegenerateState.Idle;

        private bool HasClouds
        {
            get
            {
                if (regenerateFlag == RegenerateState.Destroy || (regenerateFlag == RegenerateState.Idle && !GetCloudsParent()))
                    return false;
                else
                    return true;
            }
            set
            {
                if (value)
                {
                    if (GetCloudsParent() == null)
                        regenerateFlag = RegenerateState.Regenerate;
                }
                else
                {
                    if (GetCloudsParent() != null)
                        regenerateFlag = RegenerateState.Destroy;
                }
            }
        }

        //TODO: Check the following params later and make them public if still need this feature.
        // Better solution for this, is Runtime FX Spawners around player as we already did.
        //private float subEmitterCheckDistance = 0f;
        //private int subEmitterMaxParticles = 0;
        //[Range(0, 100)] private int emitProbability = 0;
        //private static string subEmitterStr = "_SubEmitter";
        //private float checkDistance2X;
        //private Transform[] particles;
        //private Vector3 lastPosition;

        //private int renderingLayer = 15; //Insert snow/rain layer number
        //private float renderingDistance;

        private static CloudsManagerParams _parameters;

        public static CloudsManagerParams GetParams()
        {
            _parameters.hasClouds = Script.HasClouds;
            _parameters.cloudPrefab = Script.cloudPrefab;
            _parameters.density = Script.density;
            _parameters.maxParticles = Script.maxParticles;
            _parameters.cloudSize = Script.cloudSize.x;
            _parameters.cloudsAroundPlayer = Script.cloudsAroundPlayer;
            _parameters.seed = Script.seed;
            _parameters.cloudCount = Script.cloudCount;
            _parameters.particleSize = Script.particleSize;
            _parameters.sizeMultiplier = Script.sizeMultiplier;
            _parameters.randomParticleSize = Script.randomParticleSize;
            _parameters.altitude = Script.altitude;
            _parameters.altitudeRange = Script.altitudeRange;
            _parameters.randomRotation = Script.randomRotation;
            _parameters.rotationVariation = Script.rotationVariation;
            _parameters.visibilityDistance = Script.visibilityDistance;
            _parameters.visibilityFalloff = Script.visibilityFalloff;
            _parameters.visibilityFalloffPower = Script.visibilityFalloffPower;
            _parameters.tintColor = Script.tintColor;
            _parameters.emissionColor = Script.emissionColor;
            _parameters.cloudOpacity = Script.cloudOpacity;
            _parameters.textureMipMapBias = Script.textureMipMapBias;
            _parameters.castShadows = Script.castShadows;
            //_parameters.minWorldHeight = Script.minWorldHeight;
            //_parameters.maxWorldHeight = Script.maxWorldHeight;
            _parameters.windMovement = Script.windMovement;
            _parameters.isFlatShading = Script.isFlatShading;
            _parameters.isFlatShadingCustomShape = Script.isFlatShadingCustomShape;
            _parameters.cloudMesh = Script.cloudMesh;
            _parameters.cloudMeshUser = Script.cloudMeshUser;
            _parameters.cloudMeshNormalOffset = Script.cloudMeshNormalOffset;
            _parameters.isCustomShape = Script.isCustomShape;
            _parameters.windSpeedMultiplier = Script.windSpeedMultiplier;
            _parameters.particleSystemRenderSpace = Script.particleSystemRenderSpace;
            _parameters.hasRotation = Script.hasRotation;
            _parameters.hasFog = Script.hasFog;

            return _parameters;
        }

        public static void SetParams(CloudsManagerParams parameters)
        {
             _parameters = parameters;

            // Needs regeneration
            if
            (
                Script.HasClouds != parameters.hasClouds ||
                Script.cloudPrefab != parameters.cloudPrefab ||
                Script.density != parameters.density ||
                Script.maxParticles != parameters.maxParticles ||
                Script.cloudSize != new Vector3(parameters.cloudSize, parameters.cloudSize, parameters.cloudSize) ||
                Script.seed != parameters.seed ||
                Script.cloudCount != parameters.cloudCount ||
                Script.particleSize != parameters.particleSize ||
                Script.sizeMultiplier != parameters.sizeMultiplier ||
                Script.randomParticleSize != parameters.randomParticleSize ||
                Script.randomRotation != parameters.randomRotation ||
                Script.rotationVariation != parameters.rotationVariation ||
                Script.castShadows != parameters.castShadows ||
                //Script.minWorldHeight != parameters.minWorldHeight ||
                //Script.maxWorldHeight != parameters.maxWorldHeight ||
                Script.cloudMesh != parameters.cloudMesh ||
                Script.cloudMeshUser != parameters.cloudMeshUser ||
                Script.cloudMeshNormalOffset != parameters.cloudMeshNormalOffset ||
                Script.isCustomShape != parameters.isCustomShape ||
                Script.particleSystemRenderSpace != parameters.particleSystemRenderSpace ||
                Script.hasRotation != parameters.hasRotation ||
                Script.altitudeRange != parameters.altitudeRange
            )
                Script.regenerateFlag = RegenerateState.HeavyUpdate;
            else
                Script.regenerateFlag = RegenerateState.LightUpdate;

            Script.HasClouds = parameters.hasClouds;
            Script.cloudPrefab = parameters.cloudPrefab;
            Script.density = parameters.density;
            Script.maxParticles = parameters.maxParticles;
            Script.cloudSize = new Vector3(parameters.cloudSize, parameters.cloudSize, parameters.cloudSize);
            Script.seed = parameters.seed;
            Script.cloudCount = parameters.cloudCount;
            Script.particleSize = parameters.particleSize;
            Script.sizeMultiplier = parameters.sizeMultiplier;
            Script.randomParticleSize = parameters.randomParticleSize;
            Script.randomRotation = parameters.randomRotation;
            Script.rotationVariation = parameters.rotationVariation;
            Script.visibilityDistance = parameters.visibilityDistance;
            Script.visibilityFalloff = parameters.visibilityFalloff;
            Script.visibilityFalloffPower = parameters.visibilityFalloffPower;
            Script.castShadows = parameters.castShadows;
            //Script.minWorldHeight = parameters.minWorldHeight;
            //Script.maxWorldHeight = parameters.maxWorldHeight;
            Script.cloudMesh = parameters.cloudMesh;
            Script.cloudMeshUser = parameters.cloudMeshUser;
            Script.cloudMeshNormalOffset = parameters.cloudMeshNormalOffset;
            Script.isCustomShape = parameters.isCustomShape;
            Script.particleSystemRenderSpace = parameters.particleSystemRenderSpace;
            Script.hasRotation = parameters.hasRotation;
            Script.hasFog = parameters.hasFog;
            Script.altitudeRange = parameters.altitudeRange;

            // Does not need regeneration
            Script.cloudsAroundPlayer = parameters.cloudsAroundPlayer;
            Script.altitude = parameters.altitude;
            Script.tintColor = parameters.tintColor;
            Script.emissionColor = parameters.emissionColor;
            Script.cloudOpacity = parameters.cloudOpacity;
            Script.textureMipMapBias = parameters.textureMipMapBias;
            Script.windMovement = parameters.windMovement;
            Script.isFlatShading = parameters.isFlatShading;
            Script.isFlatShadingCustomShape = parameters.isFlatShadingCustomShape;
            Script.windSpeedMultiplier = parameters.windSpeedMultiplier;

            Script.UpdateResources();
            Script.Apply();
        }

        private void Apply ()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            UpdateCloudsAltitude();
            SetCloudsTextures();
            SetVisibility();
            SetTextureQuality();
            FlatShadingRenderSwitch();
            SetOpacity();
            TimeOfDay.Apply();
        }

        private void UpdateResources()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            cloudMesh = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(TResourcesManager.cloudMesh), typeof(Mesh)) as Mesh;
            string worldPath = TTerraWorldManager.WorkDirectoryLocalPath + cloudsPrefabName;

            if (Script.cloudPrefab == null)
            {
                if (!File.Exists(worldPath))
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.cloudMesh), worldPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                Script.cloudPrefab = AssetDatabase.LoadAssetAtPath(worldPath, typeof(GameObject)) as GameObject;
            }
            else
            {
                string localPath = AssetDatabase.GetAssetPath(Script.cloudPrefab);

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

                    Script.cloudPrefab = AssetDatabase.LoadAssetAtPath(worldPath, typeof(GameObject)) as GameObject;
                }
            }
#endif
#endif
        }

        void Start()
        {
            InitVariables();
            //if (Application.isPlaying && createAtLevelStart) Update();
        }

        private void InitVariables()
        {
            if (player == null)
                player = TCameraManager.CurrentCamera.gameObject;

            //TResourcesManager.LoadCloudsResources();
        }

        private GameObject GetCloudsParent ()
        {
            if (_cloudsParent == null)
                foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                    if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<OriginShift>() != null)
                        {
                            _cloudsParent = go;
                            break;
                        }

            return _cloudsParent;
        }

        void Update()
        {
            if (regenerateFlag == RegenerateState.LightUpdate)
            {
                UpdateCloudsAltitude();
                SetCloudsTextures();
                SetVisibility();
                SetTextureQuality();
                FlatShadingRenderSwitch();
                regenerateFlag = RegenerateState.Idle;
            }

            if (regenerateFlag == RegenerateState.Regenerate)
            {
                Regenerate();
                regenerateFlag = RegenerateState.LightUpdate;
            }

            if (regenerateFlag == RegenerateState.HeavyUpdate)
            {
                if (Application.isPlaying)
                    Destroy(GetCloudsParent());
                else
                    DestroyImmediate(_cloudsParent);

                regenerateFlag = RegenerateState.Regenerate;
            }

            if (regenerateFlag == RegenerateState.Destroy)
            {
                if (Application.isPlaying)
                    Destroy(GetCloudsParent());
                else
                    DestroyImmediate(_cloudsParent);

                regenerateFlag = RegenerateState.Idle;
            }

            //if (TResourcesManager.subEmitterPrefab != null && lastPosition != player.transform.position && particles != null && player != null && clouds != null)
            //{
            //    DeactivateDistantParticles();
            //    lastPosition = player.transform.position;
            //}
        }

        private Vector3 Center
        {
            get
            {
                Vector3 center;
                if (player == null || cloudPrefab == null) InitVariables();
                
                if (cloudsAroundPlayer) center = player.transform.position;
                else
                {
                    if (TTerraWorldManager.MainTerrainGO != null)
                    {
                        center = new Vector3
                        (
                            TTerraWorldManager.MainTerrainGO.transform.position.x + (TTerraWorldManager.MainTerrain.terrainData.size.x / 2f),
                            TTerraWorldManager.MainTerrainGO.transform.position.y,
                            TTerraWorldManager.MainTerrainGO.transform.position.z + (TTerraWorldManager.MainTerrain.terrainData.size.z / 2f)
                        );
                    }
                    else
                        center = Vector3.zero;
                }
                return center;
            }
        }

        private void Regenerate()
        {
            if (cloudPrefab == null) return;

            if (visibilityDistance == 100000 && TTerraWorldManager.IsTerrainAvailable() && TTerraWorldManager.MainTerrain.terrainData != null)
                visibilityDistance = TTerraWorldManager.MainTerrain.terrainData.size.x * 10;

            cloudCount = (int)(Script.visibilityDistance / 16000f * 20);
            particleSize = Script.cloudSize.x * 7f;

            //checkDistance2X = subEmitterCheckDistance * 2f;
            //SetCloudCullDistance();
            if (GetCloudsParent() == null)
            {
                _cloudsParent = new GameObject("Clouds");
                //clouds.transform.parent = TerraUnity.TTerrainGenerator.MainTerraworldGameObject.transform;
                _cloudsParent.transform.parent = GameObject.Find("Scene Settings").transform;
                Random.InitState(seed);

                for (int i = 0; i < cloudCount * (density * 0.5f); i++)
                {
                    GameObject cloud = Instantiate(cloudPrefab);
                    cloud.name = "Cloud " + (i + 1).ToString();
                    cloud.transform.parent = _cloudsParent.transform;

                    cloud.transform.position = new Vector3
                        (
                            Random.Range(Center.x - visibilityDistance, Center.x + visibilityDistance),
                            Random.Range(-(altitudeRange / 2f), (altitudeRange / 2f)),
                            Random.Range(Center.z - visibilityDistance, Center.z + visibilityDistance)
                        );

                    cloud.transform.eulerAngles = new Vector3
                        (
                            0,
                            Random.Range(-rotationVariation, rotationVariation),
                            0
                        );

                    float randomSizeMultiplier = Random.Range(1f / sizeMultiplier, sizeMultiplier);
                    ParticleSystem cloudSystem = cloud.GetComponent<ParticleSystem>();
                    ParticleSystem.ShapeModule cloudShape = cloudSystem.shape;
                    float scaleX = cloudSize.x * randomSizeMultiplier * 2;
                    float scaleY = cloudSize.y * randomSizeMultiplier * 2;
                    float scaleZ = cloudSize.z * randomSizeMultiplier * 2;

                    ParticleSystemRenderer cloudRenderer = cloud.GetComponent<ParticleSystemRenderer>();
                    ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = cloudSystem.rotationOverLifetime;
                    ParticleSystem.RotationBySpeedModule rotationBySpeedModule = cloudSystem.rotationBySpeed;

                    // Default mesh cloud shape rendering
                    if (!isCustomShape)
                    {
                        if (cloudMesh != null)
                        {
                            cloudShape.shapeType = ParticleSystemShapeType.Mesh;

                            cloudShape.mesh = cloudMesh;
                            cloudShape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
                            cloudShape.normalOffset = 1;
                            cloudShape.texture = null;
                            //cloudMeshTexture = null;

                            cloudRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                            cloudRenderer.mesh = null;

                            // Apply Rotation
                            rotationOverLifetimeModule.enabled = true;
                            rotationBySpeedModule.enabled = true;
                        }
                        else
                            cloudShape.shapeType = ParticleSystemShapeType.Sphere;
                    }
                    // User-defined mesh cloud shape rendering
                    else
                    {
#if UNITY_EDITOR
                        if (cloudMeshUser == null)
                        {
                            TResourcesManager.LoadCloudsResources();
                            cloudMeshUser = TResourcesManager.cloudMesh;
                        }
#endif

                        if (cloudMeshUser != null)
                        {
                            cloudShape.shapeType = ParticleSystemShapeType.Mesh;
                        
                            cloudShape.mesh = cloudMeshUser;
                            cloudShape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
                            cloudShape.meshSpawnMode = ParticleSystemShapeMultiModeValue.Loop;
                            cloudShape.normalOffset = cloudMeshNormalOffset;
                            //cloudMeshTexture = _parameters.cloudMeshTexture;
                            //cloudShape.texture = cloudMeshTexture;
                        
                            cloudRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                            cloudRenderer.mesh = cloudMeshUser;
                        
                            // Render Alignment
                            cloudRenderer.alignment = particleSystemRenderSpace;

                            // Apply Rotation
                            rotationOverLifetimeModule.enabled = hasRotation;
                            rotationBySpeedModule.enabled = hasRotation;

                            SetTextureQuality();
                        }
                        else
                            cloudShape.shapeType = ParticleSystemShapeType.Sphere;
                    }

                    cloudShape.scale = new Vector3(scaleX, scaleY, scaleZ);

                    ParticleSystem.MainModule cloudMain = cloudSystem.main;
                    cloudMain.startSize = particleSize * randomSizeMultiplier * Random.Range(randomParticleSize, 1f);

                    if (!isCustomShape)
                        cloudMain.maxParticles = maxParticles;
                    else
                        cloudMain.maxParticles = 1;

                    //if (TResourcesManager.subEmitterPrefab != null && emitProbability > 0)
                    //{
                    //    int probability = Random.Range(0, 101);
                    //
                    //    if (probability <= emitProbability)
                    //    {
                    //        GameObject subEmitter = Instantiate(TResourcesManager.subEmitterPrefab);
                    //        subEmitter.name = cloud.name + subEmitterStr;
                    //        subEmitter.transform.SetParent(cloud.transform);
                    //        subEmitter.transform.localPosition = Vector3.zero;
                    //
                    //        ParticleSystem subParticleSystem = subEmitter.GetComponent<ParticleSystem>();
                    //
                    //        ParticleSystem.MainModule subMainModule = subParticleSystem.main;
                    //        subMainModule.maxParticles = subEmitterMaxParticles;
                    //        ParticleSystem.EmissionModule emissionModule = subParticleSystem.emission;
                    //        emissionModule.rateOverTime = subEmitterMaxParticles;
                    //        ParticleSystem.ShapeModule subShapeModule = subParticleSystem.shape;
                    //        subShapeModule.shapeType = cloudShape.shapeType;
                    //        subShapeModule.meshShapeType = cloudShape.meshShapeType;
                    //        subShapeModule.mesh = cloudShape.mesh;
                    //        subShapeModule.scale = cloudShape.scale;
                    //
                    //        // Set rotation to zero in order to sync with parent cloud mesh shape
                    //        subEmitter.transform.localEulerAngles = Vector3.zero;
                    //
                    //        // Run cloud simulation
                    //        subParticleSystem.Simulate(1);
                    //        subParticleSystem.Play(true);
                    //    }
                    //}

                    //particles = clouds.GetComponentsInChildren<Transform>(true);

                    if (windMovement)
                    {
                        if (cloud.GetComponent<DynamicSpawner>() == null)
                        {
                            DynamicSpawner dynamicSpawner = cloud.AddComponent<DynamicSpawner>();
                            dynamicSpawner.center = Center;
                            dynamicSpawner.simulationAreaSize = new Vector2(visibilityDistance, visibilityDistance);
                        }
                        else
                        {
                            DynamicSpawner dynamicSpawner = cloud.GetComponent<DynamicSpawner>();
                            dynamicSpawner.center = Center;
                            dynamicSpawner.simulationAreaSize = new Vector2(visibilityDistance, visibilityDistance);
                        }
                    }

                    if (castShadows)
                    {
                        cloudRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                        cloudRenderer.receiveShadows = false;
                    }
                    else
                        //cloudRenderer.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                        cloudRenderer.shadowCastingMode = ShadowCastingMode.Off;

                    FlatShadingRenderSwitch();

                    // Run cloud simulation
                    cloudSystem.Simulate(1);
                    cloudSystem.Play(true);
                }

                OriginShift OS = _cloudsParent.AddComponent<OriginShift>();
                OS.cloudsManager = this;
            }

#if UNITY_EDITOR
            SceneManagement.MarkSceneDirty();
#endif
        }

        private Material GetCloudsMaterial ()
        {
            if (cloudMaterial == null)
                if (cloudPrefab != null && cloudPrefab.GetComponent<ParticleSystem>() != null && cloudPrefab.GetComponent<ParticleSystem>().GetComponent<Renderer>() != null)
                    cloudMaterial = cloudPrefab.GetComponent<ParticleSystem>().GetComponent<Renderer>().sharedMaterial;

            return cloudMaterial;
        }

        private void SetCloudsTextures()
        {
            if (GetCloudsMaterial() == null) return;
            if (GetCloudsMaterial().HasProperty("_MainTex")) cloudTexture = GetCloudsMaterial().GetTexture("_MainTex");
            if (GetCloudsMaterial().HasProperty("_BumpMap")) cloudTextureNormal = GetCloudsMaterial().GetTexture("_BumpMap");
        }

        private void SetVisibility()
        {
            if (GetCloudsMaterial() == null) return;
            if (GetCloudsMaterial().HasProperty("_VisibilityDistance")) GetCloudsMaterial().SetFloat("_VisibilityDistance", visibilityDistance);
            if (GetCloudsMaterial().HasProperty("_VisibilityFalloff")) GetCloudsMaterial().SetFloat("_VisibilityFalloff", visibilityFalloff);
            if (GetCloudsMaterial().HasProperty("_VisibilityFalloffPower")) GetCloudsMaterial().SetFloat("_VisibilityFalloffPower", visibilityFalloffPower);
        }

        private void SetTextureQuality()
        {
            if (!isCustomShape)
            {
                if (cloudTexture != null) cloudTexture.mipMapBias = textureMipMapBias;
                if (cloudTextureNormal != null) cloudTextureNormal.mipMapBias = textureMipMapBias;
            }
            else
            {
                if (cloudTexture != null) cloudTexture.mipMapBias = 10f;
                if (cloudTextureNormal != null) cloudTextureNormal.mipMapBias = 10f;
            }
        }

        public static void SetTintColor (Color color)
        {
            if (Script.GetCloudsMaterial() != null)
                if (Script.GetCloudsMaterial().HasProperty("_Color"))
                    Script.GetCloudsMaterial().SetColor("_Color", new Color(color.r, color.g, color.b, Script.cloudOpacity));
        }

        public static void SetEmissionColor (Color color)
        {
            if (Script.GetCloudsMaterial() != null)
                if (Script.GetCloudsMaterial().HasProperty("_EmissionColor"))
                    Script.GetCloudsMaterial().SetColor("_EmissionColor", color);
        }

        private void SetOpacity()
        {
            if (GetCloudsMaterial() != null)
                if (GetCloudsMaterial().HasProperty("_Color"))
                    GetCloudsMaterial().SetColor("_Color", new Color(tintColor.r, tintColor.g, tintColor.b, cloudOpacity));
        }

        public void FlatShadingRenderSwitch ()
        {
            Material m = GetCloudsMaterial();

            if (m != null)
            {
                if (!isCustomShape)
                {
#if UNITY_EDITOR
                    if (isFlatShading)
                        SetStandardMaterialParams.SwitchMaterialBlendingType(m, SetStandardMaterialParams.BlendMode.Cutout);
                    else
                        SetStandardMaterialParams.SwitchMaterialBlendingType(m, SetStandardMaterialParams.BlendMode.Fade);
#endif

                    if (GetCloudsParent() != null)
                    {
                        foreach (ParticleSystemRenderer p in GetCloudsParent().GetComponentsInChildren<ParticleSystemRenderer>())
                        {
                            if (isFlatShading)
                                p.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
                            else
                                p.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
                        }
                    }
                }
                else
                {
#if UNITY_EDITOR
                    if (isFlatShadingCustomShape)
                    {
                        if (hasFog) SetStandardMaterialParams.SwitchMaterialBlendingType(m, SetStandardMaterialParams.BlendMode.Opaque);
                        else SetStandardMaterialParams.SwitchMaterialBlendingType(m, SetStandardMaterialParams.BlendMode.Transparent);
                    }
                    else
                        SetStandardMaterialParams.SwitchMaterialBlendingType(m, SetStandardMaterialParams.BlendMode.Fade);
#endif
                    if (GetCloudsParent() != null)
                    {
                        foreach (ParticleSystemRenderer p in GetCloudsParent().GetComponentsInChildren<ParticleSystemRenderer>())
                        {
                            if (isFlatShadingCustomShape)
                                p.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
                            else
                                p.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
                        }
                    }
                }
            }
        }

        public static void SetFlatShading( bool isflatshading)
        {
            Script.isFlatShading = isflatshading;
            Script.FlatShadingRenderSwitch();
        }

        public void MoveCloudsWithWind()
        {
            if (!windMovement) return;
            if (GetCloudsParent() == null) return;

            if (WindManager.GetParams().hasWind)
            {
                GetCloudsParent().transform.eulerAngles = new Vector3(0, WindManager.WindDirection, 0);
                Vector3 cloudPosition = GetCloudsParent().transform.forward * Time.deltaTime * WindManager.WindZone.windMain * windSpeedMultiplier * TimeOfDay.GetParams().dayNightSpeed * 10f;
                
                //Vector3 cloudPosition = GetCloudsParent().transform.forward * Time.deltaTime * WindManager.WindZone.windMain * Mathf.Exp(windSpeedMultiplier) * 16f;

                //if (TimeOfDay.GetParams().dayNightControl.Equals(TimeOfDayControls.Dynamic))
                //cloudPosition *= Mathf.Exp(TimeOfDay.GetParams().dayNightSpeed);

                GetCloudsParent().transform.position += cloudPosition;
            }
        }

        private void UpdateCloudsAltitude()
        {
            if (GetCloudsParent() == null) return;

            GetCloudsParent().transform.position = new Vector3
            (
                GetCloudsParent().transform.position.x,
                altitude,
                GetCloudsParent().transform.position.z
            );
        }

        //private void DeactivateDistantParticles()
        //{
        //    Vector3 playerPosition = player.transform.position;
        //
        //    foreach (Transform p in particles)
        //    {
        //        if (emitProbability > 0 && !p.Equals(clouds.transform) && p.name.EndsWith(subEmitterStr) && playerPosition.y < p.position.y)
        //        {
        //            Vector2 distanceXZ = new Vector2(Mathf.Abs(p.position.x - playerPosition.x), Mathf.Abs(p.position.z - playerPosition.z));
        //            float particleDistance = distanceXZ.x + distanceXZ.y;
        //
        //            ParticleSystem cloudSystem = p.GetComponent<ParticleSystem>();
        //            ParticleSystem.EmissionModule emissionModule = cloudSystem.emission;
        //
        //            if (simulateWind)
        //            {
        //                float height = playerPosition.y;
        //                float normalizedHeight = Mathf.InverseLerp(minWorldHeight, maxWorldHeight, height);
        //
        //                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = cloudSystem.velocityOverLifetime;
        //                velocityOverLifetime.enabled = true;
        //                velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        //                velocityOverLifetime.speedModifierMultiplier = normalizedHeight * windMultiplier;
        //            }
        //
        //            if (particleDistance <= subEmitterCheckDistance)
        //                emissionModule.enabled = true;
        //            else
        //                emissionModule.enabled = false;
        //
        //            if (particleDistance <= checkDistance2X)
        //                p.gameObject.SetActive(true);
        //            else
        //                p.gameObject.SetActive(false);
        //        }
        //    }
        //}

        //private void SetCloudCullDistance()
        //{
        //    renderingDistance = checkDistance2X;
        //    Camera camera = Camera.main;
        //    float[] distances = new float[32];
        //    distances[renderingLayer] = renderingDistance;
        //    camera.layerCullDistances = distances;
        //    print("Set rendering distance for layer " + LayerMask.LayerToName(renderingLayer));
        //}
    }
}

