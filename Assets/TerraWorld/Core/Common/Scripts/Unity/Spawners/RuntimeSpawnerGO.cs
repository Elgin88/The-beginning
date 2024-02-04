using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class RuntimeSpawnerGO : MonoBehaviour
    {
        [Header("Origin Controls"), Space(5)]
        public GameObject prefab;

        [Header("Placement Volumes"), Space(10)]
        [Tooltip("If enabled, placement will be based on volumes in scene. Currently only Box colliders with arbitrary orientation and size are supported")]
        public bool checkSceneVolumes = false;
        [Tooltip("Define which volume layers should be considered for this spawner")]
        public LayerMask volumeLayers;
        [Tooltip("If enabled, placement will happen everywhere outside the volume and if disabled, placement will only happen in volumes space")]
        public bool invertVolumes = false;

        [Header("Terrain Layers Filtering"), Space(10)]
        [Tooltip("Filter placement based on terrain layers and control exclusion damping percentage")]
        [Range(0f, 100f)] public float[] terrainLayerExclusions;

        [Header("Instance Settings"), Space(10)]
        [Range(1, 1000)] public int instanceCount = 100;
        [Tooltip("If enabled, all distance checks for placement will be checked in all 3 axis including height (Y) position too")]
        public bool distanceCheck3D = true;
        public float spawnRadius = 100f;

        [Header("Performance Settings"), Space(10)]
        [Tooltip("Update placement interval in seconds")]
        [Range(0f, 3f)] public float updateInterval = 1f;
        [Tooltip("Delay in scenonds between each physics raycasting operation for placement in runtime. It is recommended to increase this value if camera movement is slow-paced but generally 0 would perform better")]
        [Range(0f, 0.01f)] public float spawnDelay = 0f;
        [Tooltip("Combine meshes and bake throughout the whole area for the best peformance")]
        public bool bakeCombinedMeshes;
        public GameObject billboardLOD;
        public float billboardRenderingDistance = 1000f;
        //[Tooltip("Frustum Culling checks if models bounds are in camera view to cull offscreen models from rendering to boost performance")]
        //public bool frustumCulling = true;
        [Tooltip("If enabled, placement will be performed in realtime involving physics raycastings in the editor which is not recommended if you have many runtime layers and instances")]
        public bool realTimeEditor = false;

        [Header("Placement Conditions"), Space(10)]
        public int seedNo = 12345;
        [MinMax(-10000, 10000, ShowEditRange = true)] public Vector2 heightRange = new Vector2(-10000, 10000);
        [MinMax(0, 90, ShowEditRange = true)] public Vector2 slopeRange = new Vector2(0, 90);

        [Header("Layermask Filtering"), Space(10)]
        public LayerMask layerMask = ~0;

        [Header("Offset"), Space(10)]
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        [Header("Transform Settings"), Space(10)]
        [MinMax(0, 359, ShowEditRange = true)] public Vector2 rotationRange = new Vector2(0, 359);
        public bool lock90DegreeRotation = false;
        public bool lockYRotation = false;
        public bool getGroundAngle = true;
        public Vector3 scale = Vector3.one;
        [MinMax(0, 10, ShowEditRange = true)] public Vector2 scaleRange = new Vector2(0.8f, 1.5f);

        public enum WaterDetection { bypassWater, underWater, onWater }
        [Header("Water Detection"), Space(10)]
        public WaterDetection waterDetection = WaterDetection.bypassWater;

        [Header("Update Intervals"), Space(10)]
        public bool createOnce = false;

        [HideInInspector] public bool isChanged = false;
        [HideInInspector] public bool isDirty = false;
        [HideInInspector] public GameObject[] instances;

        private float combiningProgress = 0f;
        private float checkingHeight = 100000f;
        private Quaternion rotation;
        private bool isBypassWater;
        private bool isUnderwater;
        private GameObject instancesGO;
        private bool hasColliders = false;
        private bool hasRigidbody = false;
        private bool isKinematic = false;
        private Vector3 lastPositionCamera;
        private Vector3 lastPositionCameraEditor;
        private Collider[] hitColliders;
        private int volumesCountToCheck = 10;
        private Vector3[] spawnPositions;
        private Vector3 initialPlayerPosition;
        private Vector3 patchShift = Vector3.zero;
        private bool isInitialized = false;
        private Terrain terrain;
        private bool isTerrainAvailable = false;
        private int alphamapWidth;
        private int alphamapHeight;
        private float terrainWidth;
        private float terrainLength;
        private GameObject patchesGo;
        private Camera renderingCamera;
        private string instancesParentName;
        private string patchesParentName;
        private float nextInterval = 0f;

        private enum PlayerDirection
        {
            North,
            West,
            South,
            East,
            None
        }
        private PlayerDirection playerDirection = PlayerDirection.None;

        private struct InstanceData
        {
            public GameObject patchGO;
            public Vector3 patchPosition;
            public List<Vector3> positions;
            public List<Quaternion> rotations;
            public List<Vector3> scales;
        }
        private InstanceData[] instanceData;

        private void OnValidate()
        {
            if (bakeCombinedMeshes)
                isDirty = true;
            else
                isChanged = true;
        }

        private void OnEnable()
        {
            isChanged = true;
        }

        private void OnDisable()
        {
            RemoveInstances();
        }

        RuntimeSpawnerGO()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdate;
#endif
        }

        private void OnPrefabInstanceUpdate(GameObject instance)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == prefab)
                Initialize();
#endif
        }

        void FixedUpdate()
        {
            if (!isActiveAndEnabled) return;

            if (Application.isPlaying)
            {
                if (isChanged)
                    UpdateObjects(true);
                else
                    UpdateObjects();
            }
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            if (renderingCamera == null) GetCamera();
            if (renderingCamera == null || prefab == null) return;

            if (isChanged)
            {
                Initialize();
                if (!Application.isPlaying) UpdateObjects(true);
                isChanged = false;
            }
            else if (!Application.isPlaying)
            {
                Vector3 cameraPosition = renderingCamera.transform.position;

                if (realTimeEditor)
                    UpdateObjects();
                else
                {
                    if (Vector3.Distance(lastPositionCameraEditor, cameraPosition) <= 0.1f)
                        UpdateObjects();
                    else
                        lastPositionCameraEditor = cameraPosition;
                }
            }
        }

        void Initialize()
        {
            if (!isActiveAndEnabled) return;
            if (prefab == null) return;

            GetCamera();

            isInitialized = false;
            hitColliders = new Collider[volumesCountToCheck];
            spawnPositions = new Vector3[instanceCount];
            patchShift = Vector3.zero;
            playerDirection = PlayerDirection.None;
            lastPositionCameraEditor = renderingCamera.transform.position;

            RemoveInstances();
            CreateInstancesParent();
            CheckRigidbody();
            CreateInstances();
            ClearMemory();

            isInitialized = true;

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        private void ClearMemory()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            Resources.UnloadUnusedAssets();
        }

        private void GetCamera()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                renderingCamera = Camera.main;
            else if (TCameraManager.SceneCamera != null)
                renderingCamera = TCameraManager.SceneCamera;
#else
            renderingCamera = Camera.main;
#endif
        }

        private void RemoveInstances()
        {
            instancesParentName = "Instances " + prefab.name;
            patchesParentName = "Patches " + prefab.name;

            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;

                if (t.gameObject.name.Equals(instancesParentName) || t.gameObject.name.Equals(patchesParentName))
                {
                    if (Application.isPlaying)
                        Destroy(t.gameObject);
                    else
                        DestroyImmediate(t.gameObject);
                }
            }
        }

        private void CreateInstancesParent()
        {
            instancesParentName = "Instances " + prefab.name;
            instancesGO = new GameObject(instancesParentName);
            if (TTerraWorldManager.MainTerrainGO != null)
                instancesGO.transform.parent = this.transform;

            if (bakeCombinedMeshes && billboardLOD != null)
            {
                patchesParentName = "Patches " + prefab.name;
                patchesGo = new GameObject(patchesParentName);
                if (TTerraWorldManager.MainTerrainGO != null)
                    patchesGo.transform.parent = this.transform;
            }
        }

        private void CreateInstances()
        {
            UnityEngine.Random.InitState(seedNo);

            if (waterDetection == WaterDetection.bypassWater) isBypassWater = true;
            else isBypassWater = false;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;
            instances = new GameObject[instanceCount];

            Terrain[] terrains = FindObjectsOfType<Terrain>();

            if (terrains != null && terrains.Length > 0)
            {
                foreach (Terrain t in terrains)
                {
                    if (t.GetComponent<TTerraWorldTerrainManager>() != null)
                    {
                        terrain = t;
                        alphamapWidth = terrain.terrainData.alphamapWidth;
                        alphamapHeight = terrain.terrainData.alphamapHeight;
                        terrainWidth = terrain.terrainData.size.x;
                        terrainLength = terrain.terrainData.size.z;
                        int terrainLayersCount = terrain.terrainData.terrainLayers.Length;

                        if (terrainLayerExclusions == null || terrainLayerExclusions.Length == 0 || terrainLayerExclusions.Length != terrainLayersCount)
                        {
                            terrainLayerExclusions = new float[terrainLayersCount];
                            for (int i = 0; i < terrainLayerExclusions.Length; i++) terrainLayerExclusions[i] = 0f;
                        }

                        isTerrainAvailable = true;
                        break;
                    }
                }
            }
            else
                isTerrainAvailable = false;

            //if (bakeCombinedMeshes && billboardLOD != null && isTerrainAvailable)
            //{
            //    float positionX = terrain.transform.position.x + (spawnRadius / 2f);
            //    float positionY = terrain.transform.position.y;
            //    float positionZ = terrain.transform.position.z + (spawnRadius / 2f);
            //    Vector3 patchPosition = new Vector3(positionX, positionY, positionZ);
            //    initialPlayerPosition = patchPosition;
            //}
            //else
            initialPlayerPosition = Vector3.zero;

            if (bakeCombinedMeshes && billboardLOD != null && isTerrainAvailable)
            {
                if (patchesGo == null) return;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                    List<string> modelPaths = new List<string>();

                    foreach (MeshFilter meshFilter in meshFilters)
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        if (mesh != null) modelPaths.Add(AssetDatabase.GetAssetPath(mesh));
                    }

                    modelPaths = modelPaths.Distinct().ToList();

                    if (modelPaths != null && modelPaths.Count > 0)
                    {
                        foreach (string s in modelPaths)
                        {
                            ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(s);
                            modelImporter.isReadable = true;
                            modelImporter.indexFormat = ModelImporterIndexFormat.UInt32;
                            modelImporter.SaveAndReimport();
                        }

                        AssetDatabase.Refresh();
                    }
                }
#endif

                float terrainSizeX = terrain.terrainData.size.x;
                float terrainSizeZ = terrain.terrainData.size.z;
                int widthSteps = Mathf.CeilToInt(terrainSizeX / (spawnRadius * 2f));
                int lengthSteps = Mathf.CeilToInt(terrainSizeZ / (spawnRadius * 2f));

                instanceData = new InstanceData[widthSteps * lengthSteps];

                for (int i = 0; i < widthSteps * lengthSteps; i++)
                {
                    instanceData[i].positions = new List<Vector3>();
                    instanceData[i].rotations = new List<Quaternion>();
                    instanceData[i].scales = new List<Vector3>();
                }

                int counter = 0;

#if UNITY_EDITOR
                int progressID = TProgressBar.StartProgressBar("TERRAWORLD", "Baking models in scene...", TProgressBar.ProgressOptionsList.Managed, false);
                combiningProgress = 0;
#endif

                for (int x = 0; x < widthSteps; x++)
                {
                    for (int y = 0; y < lengthSteps; y++)
                    {
                        GameObject patch = new GameObject("Patch " + (counter + 1));
                        patch.transform.parent = patchesGo.transform;

                        float spawnRadius2X = spawnRadius * 2f;
                        float positionX = terrain.transform.position.x + (x * spawnRadius2X) + (spawnRadius / 2f);
                        float positionY = terrain.transform.position.y;
                        float positionZ = terrain.transform.position.z + (y * spawnRadius2X) + (spawnRadius / 2f);
                        Vector3 patchPosition = new Vector3(positionX, positionY, positionZ);
                        patch.transform.position = patchPosition;
                        
                        //if (frustumCulling)
                        //{
                        //    TCameraFrustumActivation frustum = patch.GetComponent<TCameraFrustumActivation>();
                        //    if (frustum == null) frustum = patch.AddComponent<TCameraFrustumActivation>();
                        //}

                        instanceData[counter].patchGO = patch;
                        instanceData[counter].patchPosition = patchPosition;

                        int passedCount = 0;

                        for (int i = 0; i < instanceCount; i++)
                        {
                            // Populate main instances
                            if (counter == 0)
                            {
                                instances[i] = Instantiate(prefab, Vector3.zero, Quaternion.identity, instancesGO.transform);
                                instances[i].name = prefab.name + "_" + (i + 1).ToString();
                                instances[i].SetActive(false);

                                //if (frustumCulling)
                                //{
                                //    TCameraFrustumActivation frustum = instances[i].GetComponent<TCameraFrustumActivation>();
                                //    if (frustum == null) frustum = instances[i].AddComponent<TCameraFrustumActivation>();
                                //}
                            }

                            Vector3 spawnPosition = new Vector3
                            (
                                patchPosition.x + (UnityEngine.Random.insideUnitCircle * spawnRadius).x,
                                patchPosition.y,
                                patchPosition.z + (UnityEngine.Random.insideUnitCircle * spawnRadius).y
                            );

                            // Populate billboard instances
                            GameObject patchInstance = Instantiate(billboardLOD, Vector3.zero, Quaternion.identity, patch.transform);
                            patchInstance.name = "Patch_" + (x + 1).ToString() + "-" + (y + 1).ToString() + " - " + prefab.name + "_" + (i + 1).ToString();

                            if (SpawnObject(patchInstance, spawnPosition))
                            {
                                instanceData[counter].positions.Add(patchInstance.transform.position);
                                instanceData[counter].rotations.Add(patchInstance.transform.rotation);
                                instanceData[counter].scales.Add(patchInstance.transform.lossyScale);
                                passedCount++;
                            }
                            else
                            {
                                // Remove instances who did not have placement conditions
                                if (Application.isPlaying)
                                    Destroy(patchInstance);
                                else
                                    DestroyImmediate(patchInstance);
                            }
                        }

                        // Remove patch if no placement is performed
                        if (passedCount == 0)
                        {
                            if (Application.isPlaying)
                                Destroy(patch);
                            else
                                DestroyImmediate(patch);
                        }
                        else
                        {
                            TMeshCombiner meshCombiner = patch.GetComponent<TMeshCombiner>();
                            if (meshCombiner == null) meshCombiner = patch.AddComponent<TMeshCombiner>();
                            meshCombiner.CombineMeshes(patch, true);
                        }

                        counter++;

#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            combiningProgress = (float)counter / (float)(widthSteps * lengthSteps);
                            TProgressBar.DisplayProgressBar("TerraWorld", "Baking models in scene for layer: " + prefab.name + "...", combiningProgress, progressID);
                        }
#endif
                    }
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    TProgressBar.RemoveProgressBar(progressID);
#endif
            }
            else
            {
                Vector3 cameraPosition = Vector3.zero;
                if (renderingCamera != null) cameraPosition = renderingCamera.transform.position;
                patchShift = CalculatePositionShift(initialPlayerPosition, cameraPosition);

                for (int i = 0; i < instanceCount; i++)
                {
                    Vector3 spawnPosition = new Vector3
                    (
                        initialPlayerPosition.x + (UnityEngine.Random.insideUnitCircle * spawnRadius).x,
                        0,
                        initialPlayerPosition.z + (UnityEngine.Random.insideUnitCircle * spawnRadius).y
                    );

                    spawnPositions[i] = spawnPosition;

                    Vector3 spawnPositionShifted = spawnPosition + patchShift;
                    instances[i] = Instantiate(prefab, spawnPositionShifted, Quaternion.identity, instancesGO.transform);
                    instances[i].name = prefab.name + "_" + (i + 1).ToString();

                    //if (frustumCulling)
                    //{
                    //    TCameraFrustumActivation frustum = instances[i].GetComponent<TCameraFrustumActivation>();
                    //    if (frustum == null) frustum = instances[i].AddComponent<TCameraFrustumActivation>();
                    //}

                    // It is safe to disable colliders after instantiation since physics calls will be performed in the next frame
                    DisableColliders(instances[i]);

                    SpawnObject(instances[i], spawnPositionShifted);

                    // Reset detected colliders on instance if there were any before instantiation to avoid physics collision steps while being placed in world
                    ResetColliders(instances[i]);
                }
            }
        }

        private Vector3 CalculatePositionShift(Vector3 sourcePos, Vector3 destinationPos)
        {
            Vector3 positionDelta = destinationPos - sourcePos;
            int shiftX = Mathf.RoundToInt(positionDelta.x / spawnRadius / 2f);
            int shiftZ = Mathf.RoundToInt(positionDelta.z / spawnRadius / 2f);
            return new Vector3(shiftX * spawnRadius * 2f, 0f, shiftZ * spawnRadius * 2f);
        }

        // Restrict placement if camera elevation is beyond placement range
        private bool CheckPlacementElevation(Vector3 cameraPos)
        {
            if (!Application.isPlaying)
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
            }

            Ray ray = new Ray(cameraPos, Vector3.down);
            RaycastHit hit;

            if (Raycasts.RaycastNonAllocSorted(ray, false, false, out hit, layerMask, Mathf.Infinity, QueryTriggerInteraction.Ignore))
            {
                float elevation = cameraPos.y - hit.point.y;
                float validDistance = (spawnRadius * 1.5f) + positionOffset.y;
                if (!Application.isPlaying) Physics.autoSimulation = true;
                if (elevation < -1f) return false;
                if (elevation <= validDistance) return true;
                else return false;
            }

            if (!Application.isPlaying) Physics.autoSimulation = true;
            return false;
        }

        private void DisableAllInstances ()
        {
            for (int i = 0; i < instanceCount; i++)
                if (instances[i] != null)
                    instances[i].SetActive(false);
        }

        private void UpdateObjects(bool forced = false)
        {
            if (renderingCamera == null) GetCamera();
            if (renderingCamera == null || prefab == null) return;

            if (!forced && !isInitialized) return;
            if (!forced && createOnce) return;

            if (Time.time > nextInterval)
            {
                nextInterval += updateInterval;

                Vector3 cameraPosition = renderingCamera.transform.position;

                if (!bakeCombinedMeshes && !CheckPlacementElevation(cameraPosition))
                {
                    DisableAllInstances();
                    return;
                }

                float deltaX = Mathf.Abs(cameraPosition.x - lastPositionCamera.x);
                float deltaZ = Mathf.Abs(cameraPosition.z - lastPositionCamera.z);

                if (cameraPosition.x > lastPositionCamera.x)
                    playerDirection = PlayerDirection.East;

                else if (cameraPosition.x < lastPositionCamera.x)
                    playerDirection = PlayerDirection.West;

                if (deltaX < deltaZ)
                {
                    if (cameraPosition.z > lastPositionCamera.z)
                        playerDirection = PlayerDirection.North;
                    else if (cameraPosition.z < lastPositionCamera.z)
                        playerDirection = PlayerDirection.South;
                }

                if (cameraPosition.x == lastPositionCamera.x && cameraPosition.z == lastPositionCamera.z)
                    //if (deltaX < 0.1f && deltaZ < 0.1f)
                    playerDirection = PlayerDirection.None;

                float checkingRadius = spawnRadius / 3f; // "/= 1.5f"

                if (playerDirection != PlayerDirection.None)
                    cameraPosition += renderingCamera.transform.forward * checkingRadius;

                // Only update if player changed position from last state
                if (!forced)
                {
                    if (distanceCheck3D)
                    {
                        if
                        (
                            (int)lastPositionCamera.x == (int)cameraPosition.x &&
                            (int)lastPositionCamera.y == (int)cameraPosition.y &&
                            (int)lastPositionCamera.z == (int)cameraPosition.z
                        )
                            return;
                    }
                    else
                    {
                        if
                        (
                            (int)lastPositionCamera.x == (int)cameraPosition.x &&
                            (int)lastPositionCamera.z == (int)cameraPosition.z
                        )
                            return;
                    }
                }

                if (bakeCombinedMeshes)
                    SpawnObjectFromData(cameraPosition, spawnRadius);
                else
                {
                    if (spawnPositions == null || spawnPositions.Length == 0) return;
                    patchShift = CalculatePositionShift(initialPlayerPosition, cameraPosition);

                    for (int i = 0; i < instances.Length; i++)
                    {
                        if (instances[i] == null) continue;

                        Vector3 instancePosition = instances[i].transform.position;

                        // If instance position is out of bounds, then find another position inside distance radius and re-spawn
                        if (!CheckValidDistance(instancePosition, cameraPosition, spawnRadius * 1.5f, distanceCheck3D))
                        {
                            instances[i].SetActive(false);

                            Vector3 spawnPosition = spawnPositions[i] + patchShift; // Predictable placement
                                                                                    //Vector3 spawnPosition = RandomCircle(cameraPosition, spawnRadius); // Non-predictable placement

                            if (Application.isPlaying)
                                StartCoroutine(SpawnObjectDelayed(i, spawnPosition));
                            else
                                SpawnObject(instances[i], spawnPosition);
                        }
                        else
                            instances[i].SetActive(true);
                    }
                }

                lastPositionCamera = cameraPosition;
            }
        }

        private void SpawnObjectFromData(Vector3 cameraPosition, float checkingRadius)
        {
            if (renderingCamera == null || prefab == null) return;
            if (instanceData == null) return;

            //bool isInElevationRange = CheckPlacementElevation(cameraPosition);
            //if (!isInElevationRange) DisableAllInstances();

            for (int i = 0; i < instanceData.Length; i++)
            {
                if (instanceData[i].patchGO == null) continue;

                if
                (
                    cameraPosition.x <= instanceData[i].patchPosition.x + checkingRadius &&
                    cameraPosition.x >= instanceData[i].patchPosition.x - checkingRadius &&
                    cameraPosition.z <= instanceData[i].patchPosition.z + checkingRadius &&
                    cameraPosition.z >= instanceData[i].patchPosition.z - checkingRadius
                )
                {
                    //if (isInElevationRange)
                    {
                        instanceData[i].patchGO.SetActive(false);

                        for (int j = 0; j < instanceCount; j++)
                        {
                            if (j < instanceData[i].positions.Count)
                            {
                                instances[j].transform.position = instanceData[i].positions[j];
                                instances[j].transform.rotation = instanceData[i].rotations[j];
                                instances[j].transform.localScale = instanceData[i].scales[j];
                                instances[j].SetActive(true);
                            }
                            else
                                instances[j].SetActive(false);
                        }
                    }
                    //else
                        //instanceData[i].patchGO.SetActive(true);
                }
                else
                {
                    Vector3 patchPosition = instanceData[i].patchPosition;

                    //TODO: If distanceCheck3D (3D distance check for placement) is needed, then we have to properly detect Y position of patch by raycasting
                    float patchDistance = (new Vector2(patchPosition.x, patchPosition.z) - new Vector2(cameraPosition.x, cameraPosition.z)).sqrMagnitude;

                    if (patchDistance <= billboardRenderingDistance * billboardRenderingDistance)
                        instanceData[i].patchGO.SetActive(true);
                    else
                        instanceData[i].patchGO.SetActive(false);
                }

                //int neighborIndex = i + 1;
                //instanceData[neighborIndex].patchGO.SetActive(false);
                //instances[instanceIndex].transform.position = instanceData[neighborIndex].positions[instanceIndex];
                //instances[instanceIndex].transform.rotation = instanceData[neighborIndex].rotations[instanceIndex];
                //instances[instanceIndex].transform.localScale = instanceData[neighborIndex].scales[instanceIndex];
            }
        }

        private IEnumerator SpawnObjectDelayed(int index, Vector3 origin)
        {
            yield return new WaitForSeconds(index * spawnDelay);
            SpawnObject(instances[index], origin);
        }

        private bool SpawnObject(GameObject instance, Vector3 origin)
        {
            if (renderingCamera == null || prefab == null) return false;

            if (isTerrainAvailable)
            {
                bool excludedPixel = false;
                float localOffsetNormalizedX = Mathf.Clamp01((origin.x - terrain.transform.position.x) / terrainWidth);
                float localOffsetNormalizedZ = Mathf.Clamp01((origin.z - terrain.transform.position.z) / terrainLength);

                for (int k = 0; k < terrainLayerExclusions.Length; k++)
                {
                    if (terrainLayerExclusions[k] == 0) continue;

                    int pixelX = Mathf.Clamp((int)(localOffsetNormalizedX * alphamapWidth), 0, alphamapWidth - 1);
                    int pixelY = Mathf.Clamp((int)(localOffsetNormalizedZ * alphamapHeight), 0, alphamapHeight - 1);
                    float[,,] alphamap = terrain.terrainData.GetAlphamaps(pixelX, pixelY, 1, 1);

                    if (alphamap[0, 0, k] > 1f - (terrainLayerExclusions[k] / 100f))
                    {
                        excludedPixel = true;
                        break;
                    }
                }

                if (excludedPixel)
                {
                    instance.SetActive(false);
                    instance.transform.position = origin + (Vector3.forward * spawnRadius * 10f);
                    return false;
                }
            }

            if (!Application.isPlaying)
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
            }

            origin.y = checkingHeight;
            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit hit;

            if (!Raycasts.RaycastNonAllocSorted(ray, isBypassWater, isUnderwater, out hit, layerMask, Mathf.Infinity, QueryTriggerInteraction.Ignore))
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                instance.SetActive(false);
                return false;
            }

            origin = hit.point;

            if (checkSceneVolumes)
            {
                int volumesCount = Physics.OverlapSphereNonAlloc(origin, spawnRadius, hitColliders, volumeLayers, QueryTriggerInteraction.Collide);
                bool isInsideVolume = false;

                for (int x = 0; x < volumesCount; x++)
                {
                    // Calculate if instance position is inside current volume in local space to cover arbitrary volume rotations and scales
                    // Assuming if volume's scale and rotation is being defined on collider's transform
                    Vector3 pointInVolumeSpace = hitColliders[x].transform.InverseTransformPoint(origin);
                    Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);
                    isInsideVolume = localBounds.Contains(pointInVolumeSpace);
                    if (invertVolumes) isInsideVolume = !isInsideVolume;
                    if (isInsideVolume) break;
                }

                if (!isInsideVolume)
                {
                    if (!Application.isPlaying) Physics.autoSimulation = true;
                    instance.SetActive(false);
                    return false;
                }
            }

            if (origin.y < heightRange.x || origin.y > heightRange.y)
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                instance.SetActive(false);
                return false;
            }

            Vector3 normal = hit.normal;

            if (Vector3.Angle(normal, Vector3.up) < slopeRange.x || Vector3.Angle(normal, Vector3.up) > slopeRange.y)
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                instance.SetActive(false);
                return false;
            }

            // --- position offset
            origin += positionOffset;

            if (origin == instance.transform.position)
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                return false;
            }

            // --- rotation
            if (getGroundAngle)
            {
                Vector3 finalRotation = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
                Quaternion surfaceRotation = Quaternion.Euler(finalRotation);

                if (!lock90DegreeRotation)
                {
                    float rotationY = UnityEngine.Random.Range(rotationRange.x, rotationRange.y);
                    surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                }
                else
                {
                    float rotationY = Mathf.Round(UnityEngine.Random.Range(0, 4)) * 90;
                    surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                    surfaceRotation.eulerAngles = new Vector3(surfaceRotation.eulerAngles.x, rotationY, surfaceRotation.eulerAngles.z);
                }

                rotation = surfaceRotation * Quaternion.Euler(rotationOffset);
            }
            else
            {
                float rotationX = rotationOffset.x;
                float rotationY = rotationOffset.y;
                float rotationZ = rotationOffset.z;

                if (!lock90DegreeRotation)
                {
                    rotationX += UnityEngine.Random.Range(rotationRange.x, rotationRange.y);
                    rotationY += UnityEngine.Random.Range(rotationRange.x, rotationRange.y);
                    rotationZ += UnityEngine.Random.Range(rotationRange.x, rotationRange.y);
                }
                else
                {
                    rotationX += Mathf.Round(UnityEngine.Random.Range(0, 4)) * 90;
                    rotationY += Mathf.Round(UnityEngine.Random.Range(0, 4)) * 90;
                    rotationZ += Mathf.Round(UnityEngine.Random.Range(0, 4)) * 90;
                }

                if (lockYRotation)
                {
                    rotationX = rotationOffset.x;
                    rotationZ = rotationOffset.z;
                }

                rotation = Quaternion.Euler(new Vector3(rotationX, rotationY, rotationZ));
            }

            // --- scaling
            Vector3 lossyScale = scale;
            float randomScale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
            lossyScale.x *= randomScale;
            lossyScale.y *= randomScale;
            lossyScale.z *= randomScale;

            ResetRigidbody(instance);

            instance.transform.position = origin;
            instance.transform.rotation = rotation;
            instance.transform.localScale = lossyScale;
            instance.SetActive(true);

            if (!Application.isPlaying) Physics.autoSimulation = true;

            return true;
        }

        private bool CheckValidDistance(Vector3 sourcePosition, Vector3 destinationPosition, float distanceRadius, bool check3DAxis)
        {
            float distance = 0;

            if (check3DAxis)
                distance = (sourcePosition - destinationPosition).sqrMagnitude;
            else
                distance = (new Vector2(sourcePosition.x, sourcePosition.z) - new Vector2(destinationPosition.x, destinationPosition.z)).sqrMagnitude;

            if (distance <= distanceRadius * distanceRadius)
                return true;
            else
                return false;
        }

        private void DisableColliders(GameObject instance)
        {
            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            {
                Collider col = t.GetComponent<Collider>();

                if (col != null && col.enabled)
                {
                    col.enabled = false;
                    hasColliders = true;
                }
            }
        }

        private void ResetColliders(GameObject instance)
        {
            if (!hasColliders) return;

            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            {
                Collider col = t.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
        }

        private void CheckRigidbody()
        {
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                Rigidbody rb = t.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    if (rb.isKinematic) isKinematic = true;
                    hasRigidbody = true;
                    break;
                }
            }
        }

        private void ResetRigidbody(GameObject instance)
        {
            if (!hasRigidbody) return;

            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            {
                Rigidbody rb = t.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = isKinematic;
                }
            }
        }

        //private Vector3 RandomCircle(Vector3 center, float radius)
        //{
        //    float ang = Random.value * 360;
        //    Vector3 pos;
        //    pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        //    pos.y = center.y;
        //    pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        //    return pos;
        //}

        //private void RemoveInstance(int i)
        //{
        //    if (Application.isPlaying)
        //        Destroy(instances[i]);
        //    else
        //        DestroyImmediate(instances[i]);
        //
        //    instances.ToList().RemoveAt(i);
        //
        //    //instance[i].SetActive(false);
        //}
    }
}

