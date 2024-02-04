using UnityEngine;
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
    public class RuntimeSpawnerGPU : MonoBehaviour
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
        [Tooltip("Frustum Culling Multiplier defines how models bounds are in camera view. 1 means original model size where higher values give wider range of camera FOV to guarantee offscreen models are also rendered for cases such as a tree which is not in view but needs its shadow to be rendered")]
        [Range(1, 3)] public float frustumCullingMultiplier = 1.1f;
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

        [Header("Rendering Options"), Space(10)]
        public UnityEngine.Rendering.ShadowCastingMode shadowCastMode = UnityEngine.Rendering.ShadowCastingMode.On;
        public bool receiveShadows = true;

        [Header("Instance Layer"), Space(10)]
        public string instanceLayer = "Default";

        [HideInInspector] public List<Matrix4x4> matrices = null;

        private List<float> LODDistances = new List<float>();
        private float checkingHeight = 100000f;
        private Quaternion rotation;
        private bool isBypassWater;
        private bool isUnderwater;
        private float startDistance;
        private float endDistance;
        private List<GameObject> LODsGO;
        private Mesh[] LODsMeshes;
        private Vector3 modelSize;
        private float nextInterval = 0f;

        public struct LODMaterials
        {
            public Material[] subMaterials;
        }
        public LODMaterials[] LODsMaterials;

        private GameObject lastPrefab = null;
        private List<Matrix4x4>[] LODMatrices;
        private Vector3 lastPositionCamera;
        private Vector3 lastPositionCameraEditor;
        private Collider[] hitColliders;
        private int volumesCountToCheck = 10;
        private Vector3[] spawnPositions;
        private Vector3 initialPlayerPosition;
        private Vector3 patchShift = Vector3.zero;
        private Terrain terrain;
        private bool isTerrainAvailable = false;
        private int alphamapWidth;
        private int alphamapHeight;
        private float terrainWidth;
        private float terrainLength;
        private Camera renderingCamera;
        private bool LODGroupNotDetected;
        private float biggestFaceLength = float.MinValue;
        [Range(0.1f, 20f)] private float maxScale;
        private bool isCompoundModel = false;
        float maxDistance;
        //private float[] angles;

        private enum PlayerDirection
        {
            North,
            West,
            South,
            East,
            None
        }
        private PlayerDirection playerDirection = PlayerDirection.None;

        private void OnValidate()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        RuntimeSpawnerGPU()
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
                UpdateObjects(false);
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            if (renderingCamera == null) GetCamera();
            if (renderingCamera == null || prefab == null) return;

            if (!Application.isPlaying)
            {
                if (realTimeEditor)
                    UpdateObjects();
                else
                {
                    Vector3 cameraPosition = renderingCamera.transform.position;

                    if (Vector3.Distance(lastPositionCameraEditor, cameraPosition) <= 0.1f)
                        UpdateObjects();
                    else
                    {
                        RenderInstances();
                        lastPositionCameraEditor = cameraPosition;
                    }
                }
            }
            else
                PerformPlacement();
        }

        private void PerformPlacement ()
        {
            // Only performs if prefab is changed
            InitLODs();

            if (LODsMaterials == null || LODsMaterials.Length == 0) return;
            if (matrices == null || matrices.Count == 0) return;

            Vector3 cameraPosition = renderingCamera.transform.position;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(renderingCamera);

            for (int i = 0; i < LODDistances.Count; i++)
                LODMatrices[i] = new List<Matrix4x4>();

            for (int i = 0; i < matrices.Count; i++)
            {
                Matrix4x4 matrix = matrices[i];
                if (matrix == Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one)) continue;

                // Frustum Culling
                Vector3 matrixPosition = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                Vector3 matrixScaleLocal = TMatrix.ExtractScaleFromMatrix(ref matrix);
                Vector3 matrixScale = new Vector3(modelSize.x * matrixScaleLocal.x, modelSize.y * matrixScaleLocal.y, modelSize.z * matrixScaleLocal.z) * frustumCullingMultiplier;
                Bounds matrixBounds = new Bounds(matrixPosition, matrixScale);
                if (!GeometryUtility.TestPlanesAABB(planes, matrixBounds)) continue;

                float instanceDistance = (cameraPosition - TMatrix.ExtractTranslationFromMatrix(ref matrix)).sqrMagnitude;
                if (!CheckValidDistance(instanceDistance, spawnRadius)) continue;

                for (int j = 0; j < LODDistances.Count; j++)
                {
                    if (j == 0) startDistance = 0;
                    else startDistance = LODDistances[j - 1];
                    endDistance = LODDistances[j];

                    if (instanceDistance > startDistance * startDistance && instanceDistance <= endDistance * endDistance)
                    {
                        LODMatrices[j].Add(matrix);
                        break;
                    }
                }
            }

            RenderInstances();
            lastPositionCamera = cameraPosition;
        }

        private void RenderInstances ()
        {
            for (int LODIndex = 0; LODIndex < LODDistances.Count; LODIndex++)
                for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].subMaterials.Length; submeshIndex++)
                {
                    if (LODMatrices[LODIndex] == null || LODMatrices[LODIndex].Count == 0) continue;
                    Graphics.DrawMeshInstanced(LODsMeshes[LODIndex], submeshIndex, LODsMaterials[LODIndex].subMaterials[submeshIndex], LODMatrices[LODIndex], null, shadowCastMode, receiveShadows, LayerMask.NameToLayer(instanceLayer));
                }
        }

        public void InitLODs()
        {
            if (prefab == lastPrefab && LODsMaterials != null) return;
            if (prefab == null) return;
            if (LODsGO == null) SetLODDistances();
            lastPrefab = prefab;
        }

        private void SetLODDistances()
        {
            LODsGO = new List<GameObject>();
            LODDistances = new List<float>();
            LODGroup lODGroup = prefab.GetComponent<LODGroup>();
            maxDistance = spawnRadius;
            maxScale = scaleRange.y;

            if (lODGroup != null)
            {
                LODGroupNotDetected = false;
                LOD[] lods = lODGroup.GetLODs();
                int index = 0;

                for (int i = 0; i < lods.Length; i++)
                {
                    Renderer[] renderers = lods[i].renderers;
                    if (renderers == null || renderers.Length == 0) continue;
                    Renderer renderer = renderers[0];
                    if (renderer == null) continue;
                    if (renderer.sharedMaterial == null) continue;
                    if (renderer.gameObject.GetComponent<MeshFilter>().sharedMesh == null) continue;
                    LODsGO.Add(renderer.gameObject);
                    float LODDistance = GetDistanceToCamera(LODsGO[index], lods[i].screenRelativeTransitionHeight);
                    LODDistances.Add(LODDistance);
                    index++;
                }

                LODsMeshes = new Mesh[LODsGO.Count];
                LODsMaterials = new LODMaterials[LODsGO.Count];

                for (int i = 0; i < LODsGO.Count; i++)
                {
                    if (LODsGO[i].GetComponent<MeshFilter>() != null)
                    {
                        MeshFilter meshFilter = LODsGO[i].GetComponent<MeshFilter>();
                        LODsMeshes[i] = meshFilter.sharedMesh;
                        Renderer renderer = LODsGO[i].GetComponent<Renderer>();
                        Material[] _LODsMaterials = renderer.sharedMaterials;
                        LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                        for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                            LODsMaterials[i].subMaterials[submeshIndex] = _LODsMaterials[submeshIndex];
                    }
                    else if (LODsGO[i].GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        SkinnedMeshRenderer renderer = LODsGO[i].GetComponent<SkinnedMeshRenderer>();
                        LODsMeshes[i] = renderer.sharedMesh;
                        Material[] _LODsMaterials = renderer.sharedMaterials;
                        LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                        for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                            LODsMaterials[i].subMaterials[submeshIndex] = _LODsMaterials[submeshIndex];
                    }
                }

                if (LODDistances.Count > 0)
                    LODDistances[LODDistances.Count - 1] = GetDistanceToCamera(LODsGO[LODsGO.Count - 1], lods[lods.Length - 1].screenRelativeTransitionHeight);
            }
            else
            {
                LODGroupNotDetected = true;

                foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
                    if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                        LODsGO.Add(t.gameObject);

                if (LODsGO != null && LODsGO.Count > 0)
                {
                    if (LODsGO.Count == 1)
                        isCompoundModel = false;
                    else
                        isCompoundModel = true;

                    if (!isCompoundModel)
                    {
                        LODDistances.Add(maxDistance);
                        LODsMeshes = new Mesh[1];
                        LODsMaterials = new LODMaterials[1];
                        LODsMeshes[0] = LODsGO[0].GetComponent<MeshFilter>().sharedMesh;
                        Material[] _LODsMaterials = LODsGO[0].GetComponent<Renderer>().sharedMaterials;
                        LODsMaterials[0].subMaterials = new Material[_LODsMaterials.Length];

                        for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                            LODsMaterials[0].subMaterials[submeshIndex] = LODsGO[0].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                    }
                    else
                    {
                        LODDistances.Add(maxDistance);
                        LODsMeshes = new Mesh[LODsGO.Count];
                        LODsMaterials = new LODMaterials[LODsGO.Count];

                        for (int i = 0; i < LODsGO.Count; i++)
                        {
                            if (LODsGO[i].GetComponent<MeshFilter>() != null)
                            {
                                MeshFilter meshFilter = LODsGO[i].GetComponent<MeshFilter>();
                                LODsMeshes[i] = meshFilter.sharedMesh;
                                Renderer renderer = LODsGO[i].GetComponent<Renderer>();
                                Material[] _LODsMaterials = renderer.sharedMaterials;
                                LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                                for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                                    LODsMaterials[i].subMaterials[submeshIndex] = _LODsMaterials[submeshIndex];
                            }
                            else if (LODsGO[i].GetComponent<SkinnedMeshRenderer>() != null)
                            {
                                SkinnedMeshRenderer renderer = LODsGO[i].GetComponent<SkinnedMeshRenderer>();
                                LODsMeshes[i] = renderer.sharedMesh;
                                Material[] _LODsMaterials = renderer.sharedMaterials;
                                LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                                for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                                    LODsMaterials[i].subMaterials[submeshIndex] = _LODsMaterials[submeshIndex];
                            }
                        }
                    }
                }
            }
        }

        private float GetDistanceToCamera(GameObject go, float NormalizedPersentage)
        {
            GetBiggestFaceLength(go);
            return biggestFaceLength * maxScale / (((TCameraManager.MainCamera.pixelRect.height * NormalizedPersentage) / TCameraManager.MainCamera.pixelHeight) * (2 * Mathf.Tan(TCameraManager.MainCamera.fieldOfView / 2 * Mathf.Deg2Rad)));
        }

        private void GetBiggestFaceLength(GameObject go = null)
        {
            biggestFaceLength = float.MinValue;

            if (go == null)
            {
                List<float> lengths = new List<float>();

                for (int i = 0; i < LODsGO.Count; i++)
                {
                    Bounds bounds = LODsGO[i].GetComponent<Renderer>().bounds;

                    if (biggestFaceLength < bounds.extents.x)
                        biggestFaceLength = bounds.extents.x;
                    if (biggestFaceLength < bounds.extents.y)
                        biggestFaceLength = bounds.extents.y;
                    if (biggestFaceLength < bounds.extents.z)
                        biggestFaceLength = bounds.extents.z;

                    lengths.Add(biggestFaceLength);
                }

                biggestFaceLength = lengths.Max();
            }
            else
            {
                Bounds bounds = go.GetComponent<Renderer>().bounds;

                if (biggestFaceLength < bounds.extents.x)
                    biggestFaceLength = bounds.extents.x;
                if (biggestFaceLength < bounds.extents.y)
                    biggestFaceLength = bounds.extents.y;
                if (biggestFaceLength < bounds.extents.z)
                    biggestFaceLength = bounds.extents.z;
            }

            if (scale.x >= scale.z)
                biggestFaceLength *= scale.x * maxScale;
            else
                biggestFaceLength *= scale.z * maxScale;
        }

        void Initialize ()
        {
            if (!isActiveAndEnabled) return;
            if (prefab == null) return;

            GetCamera();
            UnityEngine.Random.InitState(seedNo);

            if (waterDetection == WaterDetection.bypassWater) isBypassWater = true;
            else isBypassWater = false;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;

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

            matrices = new List<Matrix4x4>(instanceCount);
            //angles = new float[instanceCount];
            hitColliders = new Collider[volumesCountToCheck];
            spawnPositions = new Vector3[instanceCount];
            patchShift = Vector3.zero;
            playerDirection = PlayerDirection.None;
            lastPositionCameraEditor = Vector3.zero;
            Vector3 cameraPosition = Vector3.zero;

            if (renderingCamera != null)
            {
                lastPositionCameraEditor = renderingCamera.transform.position;
                cameraPosition = renderingCamera.transform.position;
            }

            initialPlayerPosition = Vector3.zero;
            patchShift = CalculatePositionShift(initialPlayerPosition, cameraPosition);

            //CheckPhysicsComponents();

            for (int i = 0; i < instanceCount; i++)
            {
                Vector3 spawnPosition = new Vector3
                (
                    initialPlayerPosition.x + (UnityEngine.Random.insideUnitCircle * spawnRadius).x,
                    0,
                    initialPlayerPosition.z + (UnityEngine.Random.insideUnitCircle * spawnRadius).y
                );

                // Square placement zone instead of Cicular zone
                //Vector3 spwanPosition = new Vector3
                //(
                //    initialPlayerPosition.x + (Random.Range(0f, 1f) * spawnRadius) - (spawnRadius / 2f),
                //    0,
                //    initialPlayerPosition.z + (Random.Range(0f, 1f) * spawnRadius) - (spawnRadius / 2f)
                //);

                //angles[i] = Random.value * 360;

                spawnPositions[i] = spawnPosition;
                Vector3 spawnPositionShifted = spawnPosition + patchShift;

                //matrices.Add(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
                matrices.Add(Matrix4x4.TRS(spawnPositionShifted, Quaternion.identity, Vector3.one));
                SpawnObject(i, spawnPositionShifted);
            }

            SetLODDistances();

            for (int i = 0; i < LODDistances.Count; i++)
            {
                LODDistances[i] = Mathf.Clamp(LODDistances[i], 0, 100000);
            
                if (i > 0)
                    if (LODDistances[i] <= LODDistances[i - 1])
                        LODDistances[i] = LODDistances[i - 1] + 1;
            }
            
            if (LODGroupNotDetected && LODDistances != null)
                LODDistances[0] = maxDistance;
            
            LODMatrices = new List<Matrix4x4>[LODDistances.Count];
            modelSize = CalculateBounds();

            ClearMemory();

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
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

        public Vector3 CalculateBounds ()
        {
            Bounds bounds = new Bounds();
            bounds.size = Vector3.zero;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.size;
        }

        private Vector3 CalculatePositionShift (Vector3 sourcePos, Vector3 destinationPos)
        {
            Vector3 playerPosDelta = destinationPos - sourcePos;
            int shiftX = Mathf.RoundToInt(playerPosDelta.x / spawnRadius / 2f);
            int shiftZ = Mathf.RoundToInt(playerPosDelta.z / spawnRadius / 2f);
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
                float validDistance = (spawnRadius * 1f) + positionOffset.y;
                if (!Application.isPlaying) Physics.autoSimulation = true;
                if (elevation < -1f) return false;
                if (elevation <= validDistance) return true;
                else return false;
            }
            
            if (!Application.isPlaying) Physics.autoSimulation = true;
            return false;
        }

        private void DisableAllInstances()
        {
            for (int i = 0; i < matrices.Count; i++)
                matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        }

        private void UpdateObjects (bool updatePlacement = true)
        {
            if (renderingCamera == null) GetCamera();
            if (renderingCamera == null || prefab == null) return;
            //if (checkSceneVolumes) transform.position = player.transform.position;

            if (Time.time > nextInterval)
            {
                nextInterval += updateInterval;

                Vector3 cameraPosition = renderingCamera.transform.position;
                if (!CheckPlacementElevation(cameraPosition))
                {
                    //DisableAllInstances();
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

                //if (playerDirection == PlayerDirection.North)
                //    cameraPosition += Vector3.forward * checkingRadius;
                //else if (playerDirection == PlayerDirection.South)
                //    cameraPosition += Vector3.back * checkingRadius;
                //
                //if (playerDirection == PlayerDirection.East)
                //    cameraPosition += Vector3.right * checkingRadius;
                //else if (playerDirection == PlayerDirection.West)
                //    cameraPosition += Vector3.left * checkingRadius;

                if (playerDirection != PlayerDirection.None)
                    cameraPosition += renderingCamera.transform.forward * checkingRadius;

                // Only update if player changed position from last state
                if (LODMatrices != null && LODMatrices.Length > 0 && LODMatrices[0] != null)
                {
                    if (distanceCheck3D)
                    {
                        if
                        (
                            (int)lastPositionCamera.x == (int)cameraPosition.x &&
                            (int)lastPositionCamera.y == (int)cameraPosition.y &&
                            (int)lastPositionCamera.z == (int)cameraPosition.z
                        )
                        {
                            if (!Application.isPlaying) RenderInstances();
                            return;
                        }
                    }
                    else
                    {
                        if
                        (
                            (int)lastPositionCamera.x == (int)cameraPosition.x &&
                            (int)lastPositionCamera.z == (int)cameraPosition.z
                        )
                        {
                            if (!Application.isPlaying) RenderInstances();
                            return;
                        }
                    }
                }

                patchShift = CalculatePositionShift(initialPlayerPosition, cameraPosition);

                for (int i = 0; i < matrices.Count; i++)
                {
                    Matrix4x4 matrix = matrices[i];
                    Vector3 matrixPosition = TMatrix.ExtractTranslationFromMatrix(ref matrix);

                    // If instance position is out of bounds, then find another position inside distance radius and re-spawn
                    // We need to check in 2D space instead of 3D axis to preserve previous valid matrices when going back and forth in allowed 3D volume
                    if (CheckValidDistance(matrixPosition, cameraPosition, spawnRadius, distanceCheck3D))
                        continue;

                    Vector3 spawnPosition = spawnPositions[i] + patchShift; // Predictable placement
                                                                            //Vector3 spawnPosition = RandomCircle(i, cameraPosition, spawnRadius); // Non-predictable placement

                    if (Application.isPlaying)
                        StartCoroutine(SpawnObjectDelayed(i, spawnPosition));
                    else
                        SpawnObject(i, spawnPosition);
                }

                if (updatePlacement)
                    PerformPlacement();
            }
        }

        private IEnumerator SpawnObjectDelayed (int index, Vector3 origin)
        {
            yield return new WaitForSeconds(index * spawnDelay);
            SpawnObject(index, origin);
        }

        private void SpawnObject(int index, Vector3 origin)
        {
            if (renderingCamera == null || prefab == null) return;
            //if (checkSceneVolumes && !isInsideVolume) return;

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
                    matrices[index] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                    return;
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
                //matrices[index] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                return;
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

                    //// Sample code to get mesh vertices and compare position within its bounds
                    //Mesh mesh = hitColliders[x].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    //Vector3[] polyPoints = new Vector3[mesh.vertexCount];
                    //int counter = 0;
                    //
                    //foreach (Vector3 v in polyPoints)
                    //    polyPoints[counter++] = hitColliders[x].transform.TransformPoint(v);
                    //
                    //isInsideVolume = PointInPolygon(polyPoints, origin);
                }

                if (!isInsideVolume)
                {
                    if (!Application.isPlaying) Physics.autoSimulation = true;
                    return;
                }
            }

            if (origin.y < heightRange.x || origin.y > heightRange.y)
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                return;
            }

            Vector3 normal = hit.normal;

            if (Vector3.Angle(normal, Vector3.up) < slopeRange.x || Vector3.Angle(normal, Vector3.up) > slopeRange.y)
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                return;
            }

            // --- position offset
            origin += positionOffset;

            Matrix4x4 matrix = matrices[index];
            
            if (origin == TMatrix.ExtractTranslationFromMatrix(ref matrix))
            {
                if (!Application.isPlaying) Physics.autoSimulation = true;
                return;
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
                    float rotationY = Mathf.Round(UnityEngine.Random.Range(0f, 4)) * 90;
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
                    rotationX += Mathf.Round(UnityEngine.Random.Range(0f, 4)) * 90;
                    rotationY += Mathf.Round(UnityEngine.Random.Range(0f, 4)) * 90;
                    rotationZ += Mathf.Round(UnityEngine.Random.Range(0f, 4)) * 90;
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

            matrices[index] = Matrix4x4.TRS(origin, rotation, lossyScale);

            //Debug.Log("Updated Instance " + prefab.name);

            if (!Application.isPlaying) Physics.autoSimulation = true;
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

        private bool CheckValidDistance(float distanceX2, float distanceRadius)
        {
            if (distanceX2 <= distanceRadius * distanceRadius)
                return true;
            else
                return false;
        }

        //private Vector3 RandomCircle(int index, Vector3 center, float radius)
        //{
        //    //float angle = angles[index];
        //    float angle = UnityEngine.Random.value * 360;
        //
        //    return new Vector3
        //    (
        //        center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad),
        //        center.y,
        //        center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad)
        //    );
        //}

        //private bool isInsideVolume = false;
        //
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (player == null || !checkSceneVolumes) return;
        //
        //    if (volumeLayers == (volumeLayers | (1 << other.gameObject.layer)))
        //        isInsideVolume = true;
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (player == null || !checkSceneVolumes) return;
        //
        //    if (volumeLayers == (volumeLayers | (1 << other.gameObject.layer)))
        //    {
        //        isInsideVolume = false;
        //
        //        // Devalidate matrices to bypass rendering of all instances
        //        for (int i = 0; i < matrices.Count; i++)
        //            matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        //    }
        //}

        //private void CheckPhysicsComponents ()
        //{
        //    SphereCollider SP = gameObject.GetComponent<SphereCollider>();
        //    Rigidbody RB = gameObject.GetComponent<Rigidbody>();
        //    
        //    if (checkSceneVolumes)
        //    {
        //        if (SP == null) SP = gameObject.AddComponent<SphereCollider>();
        //        SP.isTrigger = true;
        //        SP.radius = 1f;
        //    
        //        if (RB == null) RB = gameObject.AddComponent<Rigidbody>();
        //        RB.useGravity = false;
        //        RB.isKinematic = true;
        //    }
        //    else
        //    {
        //        if (SP != null)
        //        {
        //            if (Application.isPlaying) Destroy(SP);
        //            else DestroyImmediate(SP);
        //        }
        //    
        //        if (RB != null)
        //        {
        //            if (Application.isPlaying) Destroy(RB);
        //            else DestroyImmediate(RB);
        //        }
        //    }
        //}
    }
}

