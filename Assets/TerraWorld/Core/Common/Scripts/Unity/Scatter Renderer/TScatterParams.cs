/// <summary>
/// In Game: Full CullingGroup API implementation to cull and LOD patches hense much better performance.
/// In Editor: Uses traditional distance and AABB Plane checks for LOD & Culling.
/// </summary>

using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TerraUnity.UI;
using TerraUnity.Utils;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class TScatterParams : TScatterLayer
    {
        // Patch system
        [HideInInspector] public Patch[] _patches; // Old patch system

        // Mask system
        [SerializeField] public MaskData[] maskData; // Old mask system

        [Serializable]
        public struct LODMaterials
        {
            public Material[] subMaterials;
        }
        [HideInInspector] public LODMaterials[] LODsMaterials;

        public List<float> LODDistances;
        [Range(1f, 5f)] public float LODMultiplier = 1;

        [HideInInspector] public bool LODGroupNotDetected;
        public float maxDistance;
        [HideInInspector] public List<GameObject> LODsGO;
        [HideInInspector] public Mesh[] LODsMeshes;
        //[HideInInspector] public GameObject LODWithCollider;

        public UnityEngine.Rendering.ShadowCastingMode shadowCastMode;
        public bool receiveShadows;

        // patchScale resolution supports
        //16:   supports 0.75 meters models distance
        //32:   supports 1    meters models distance
        //64:   supports 2    meters models distance
        //128:  supports 3    meters models distance
        //256:  supports 4    meters models distance
        //512:  supports 6    meters models distance
        //1024: supports 7    meters models distance
        //2048: supports 10   meters models distance
        //4096: supports 18   meters models distance
        [HideInInspector] public float patchScale = 256;
        [HideInInspector] public int patchesRowCount;
        [HideInInspector] public int patchesColCount;

        [Tooltip("Increase this value to prevent offscreen elements disappearing while their shadow is still in screen. Value of 1.1 means 10% bigger offscreen switching to keep elements outside camera frustum for 10% of patch size.")]
        [Range(1f, 3f)] public float frustumMultiplier = 1.1f;

        public bool randomColors = false;
        [Range(2, 128)] public int randomColorsCount = 32;
        [HideInInspector] public Color[] randomColorsList = null;
        [HideInInspector] public Gradient colorGradient;
        [HideInInspector] public MaterialPropertyBlock block;
        [HideInInspector] public int colorID;
        [HideInInspector] public bool isCompoundModel = false;
        [HideInInspector] public int instanceCount;
        [HideInInspector] public float maxRenderingDistanceSqrt;

        private CullingGroup localCullingGroup;
        private BoundingSphere[] cullingPoints;
        private int[] activePatchIndices;
        private Vector3 lastPositionTransform;
        private Vector3 lastPositionCamera;
        private Quaternion lastRotationCamera;
        private int[] activeIndices = null;
        private int[] LODList = null;
        private List<Matrix4x4>[,] matricesList = null;
        private Matrix4x4 lastColliderMatrix;
        private int progressID = -1;
        private float cameraHeight;
        private float occlusionCheckHeight = 150f;
        private bool loadMaskOnThreads = false;

        //private float deadZoneMeters = 1;
        //private float occlusionDistance;
        //private GameObject patchBox;
        //private Material patchBoxMaterial;
        //private Bounds patchBoxBounds;
        //private Mesh patchBoxMesh;


        // Prefab Analysis
        //-------------------------------------------------------------------------------------------------------

        TScatterParams()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
#endif
        }

        public GameObject Prefab
        {
            get => prefab;
            set
            {
                prefab = value;
                RegenerateLayer();
            }
        }

        public void SetPrefabWithoutUpdatePatches(GameObject Prefab)
        {
            prefab = Prefab;
        }

        private void RegenerateLayer()
        {
            try
            {
                if (prefab == null) return;

                // UPDATE: patch sizes are now dynamically calculated based on Area Size so the following statement is no longer valid!
                // Since now all layers share the same patch size in world, there is no need to regenerate
                // patches based on model's rendering distance unless for the first time when world crated!
                //if (patchData == null || patchData.Length == 0)
                {
                    //GetPrefabCollider();
                    SetLODDistances();
                    GetBiggestFaceLength();
                    GeneratePatches();
                }

                SyncLayer(null, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SyncLayer(List<int> indices = null, bool showProgress = false, bool updateDataBuffer = true, bool updateMaskData = true)
        {
            if (updateMaskData)
                SaveMaskData(indices, showProgress);
            else
                CreateInstances(indices, showProgress);

            if (updateDataBuffer)
                SaveDataBuffer();
            else
                SetupRendering();
        }

        public void SaveDataBuffer()
        {
            patchDataIsSaved = false;

#if UNITY_EDITOR && TERRAWORLD_PRO
            patchDataPath = Path.GetFullPath(TTerraWorldManager.WorkDirectoryLocalPath + this.gameObject.name + ".patchdata");
#endif

            if (patchData != null && patchData.Length > 0)
            {
                Thread thread = new Thread(new ThreadStart(SerializePatch));
                thread.Start();

#if UNITY_EDITOR
                thread.Join();
                AssetDatabase.Refresh();

                if (File.Exists(patchDataPath))
                {
                    patchDataFile = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(patchDataPath), typeof(UnityEngine.Object));
                    //AssetDatabase.ImportAsset(TAddresses.GetProjectPath(patchDataPath), ImportAssetOptions.ForceUpdate);
                    //AssetDatabase.Refresh();
                    LoadDataBuffer(true);
                }
#endif
            }
            else
                Debug.Log("Patch Data for layer: " + this.gameObject.name + " is empty!");
        }

        private void LoadDataBuffer(bool forced = false)
        {
            if (!forced && patchData != null) return;
            patchDataIsLoaded = false;

            // Port old version's patch data into our main PatchData struct
            if (_patches != null && _patches.Length > 0)
            {
                patchData = new PatchData[_patches.Length];

                for (int i = 0; i < _patches.Length; i++)
                {
                    patchData[i] = new PatchData();

                    // Patch Position
                    patchData[i].positionX = _patches[i].position.x;
                    patchData[i].positionY = _patches[i].position.y;
                    patchData[i].positionZ = _patches[i].position.z;

                    // Scale
                    patchData[i].scale = _patches[i].scale;

                    // Matrix Data
                    patchData[i].m00 = new float[_patches[i].matrices.Count];
                    patchData[i].m33 = new float[_patches[i].matrices.Count];
                    patchData[i].m23 = new float[_patches[i].matrices.Count];
                    patchData[i].m13 = new float[_patches[i].matrices.Count];
                    patchData[i].m03 = new float[_patches[i].matrices.Count];
                    patchData[i].m32 = new float[_patches[i].matrices.Count];
                    patchData[i].m22 = new float[_patches[i].matrices.Count];
                    patchData[i].m02 = new float[_patches[i].matrices.Count];
                    patchData[i].m12 = new float[_patches[i].matrices.Count];
                    patchData[i].m21 = new float[_patches[i].matrices.Count];
                    patchData[i].m11 = new float[_patches[i].matrices.Count];
                    patchData[i].m01 = new float[_patches[i].matrices.Count];
                    patchData[i].m30 = new float[_patches[i].matrices.Count];
                    patchData[i].m20 = new float[_patches[i].matrices.Count];
                    patchData[i].m10 = new float[_patches[i].matrices.Count];
                    patchData[i].m31 = new float[_patches[i].matrices.Count];

                    for (int j = 0; j < _patches[i].matrices.Count; j++)
                    {
                        patchData[i].m00[j] = _patches[i].matrices[j].m00;
                        patchData[i].m33[j] = _patches[i].matrices[j].m33;
                        patchData[i].m23[j] = _patches[i].matrices[j].m23;
                        patchData[i].m13[j] = _patches[i].matrices[j].m13;
                        patchData[i].m03[j] = _patches[i].matrices[j].m03;
                        patchData[i].m32[j] = _patches[i].matrices[j].m32;
                        patchData[i].m22[j] = _patches[i].matrices[j].m22;
                        patchData[i].m02[j] = _patches[i].matrices[j].m02;
                        patchData[i].m12[j] = _patches[i].matrices[j].m12;
                        patchData[i].m21[j] = _patches[i].matrices[j].m21;
                        patchData[i].m11[j] = _patches[i].matrices[j].m11;
                        patchData[i].m01[j] = _patches[i].matrices[j].m01;
                        patchData[i].m30[j] = _patches[i].matrices[j].m30;
                        patchData[i].m20[j] = _patches[i].matrices[j].m20;
                        patchData[i].m10[j] = _patches[i].matrices[j].m10;
                        patchData[i].m31[j] = _patches[i].matrices[j].m31;
                    }
                }

                //_patches = null;
                //SaveDataBuffer();
                patchDataIsLoaded = true;
            }

            // Fill in PatchData from data file which is already generated in World Directory
            else
            {
#if !UNITY_EDITOR
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
                    patchDataPath = Path.Combine(Application.streamingAssetsPath, this.gameObject.name + ".patchdata");
#endif

#if UNITY_EDITOR
                AssetDatabase.Refresh();
                if (patchDataFile != null) patchDataPath = Path.GetFullPath(AssetDatabase.GetAssetPath(patchDataFile));
#endif

                //if (File.Exists(patchDataPath))
                {
                    patchData = null;
                    DeserializePatch();
                    InitCullingPoints2Patch();

                    //Thread thread = new Thread(new ThreadStart(DeserializePatch));
                    //thread.Start();
                    //thread.Join();

                    // Needed when layer is updated via Editor
                    SetupRendering();
                }
                //else
                //Debug.Log("Data file for layer: " + this.gameObject.name + " not found!");
            }
        }

        public void SerializePatch()
        {
            using (FileStream fs = new FileStream(patchDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                try
                {
                    for (int i = 0; i < patchData.Length; i++)
                        patchData[i].Serialize();

                    BinaryFormatter writer = new BinaryFormatter();
                    writer.Serialize(fs, patchData);
                }
                catch (Exception e) { throw e; }
                finally
                {
                    if (fs != null) fs.Close();
                    patchDataIsSaved = true;
                }
            }
        }

        public void DeserializePatch()
        {
            //// Retry data loading if data saving is not finished yet!
            //if (!dataIsSaved)
            //{
            //    Thread.Sleep(1000);
            //    if (loadingThread.IsAlive) loadingThread.Abort();
            //    loadingThread = new Thread(new ThreadStart(DeserializeData));
            //    loadingThread.Start();
            //    return;
            //}

            Uri uri = new Uri(patchDataPath);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SendWebRequest();
                while (!webRequest.isDone);

#if UNITY_2020_1_OR_NEWER
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                        MemoryStream ms = new MemoryStream(webRequest.downloadHandler.data);

                        try
                        {
                            BinaryFormatter reader = new BinaryFormatter();
                            patchData = (PatchData[])reader.Deserialize(ms);
                        }
                        catch (Exception e) { throw e; }
                        finally
                        {
                            patchDataIsLoaded = true;
                        }
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("HTTP Error: " + webRequest.error);
                        break;
                }
#else
                if (webRequest.isNetworkError)
                    Debug.LogError("Error: " + webRequest.error);
                else if (webRequest.isHttpError)
                    Debug.LogError("HTTP Error: " + webRequest.error);
                else
                {
                    MemoryStream ms = new MemoryStream(webRequest.downloadHandler.data);

                    try
                    {
                        BinaryFormatter reader = new BinaryFormatter();
                        patchData = (PatchData[])reader.Deserialize(ms);

                        for (int i = 0; i < patchData.Length; i++)
                            patchData[i].Deserialize();
                    }
                    catch (Exception e) { throw e; }
                    finally
                    {
                        patchDataIsLoaded = true;
                    }
                }
#endif
            }
        }

        public void SaveMaskData(List<int> indices = null, bool showProgress = false)
        {
            if (Application.isPlaying) return;
            maskDataIsSaved = false;

#if UNITY_EDITOR && TERRAWORLD_PRO
            maskDataPath = Path.GetFullPath(TTerraWorldManager.WorkDirectoryLocalPath + this.gameObject.name + ".maskdata");
#endif

            if (maskDataFast != null && maskDataFast.Length > 0)
            {
                if (loadMaskOnThreads)
                {
                    Thread thread = new Thread(new ThreadStart(SerializeMask));
                    thread.Start();
                    thread.Join();
                }
                else
                    SerializeMask();

#if UNITY_EDITOR
                AssetDatabase.Refresh();

                if (File.Exists(maskDataPath))
                {
                    maskDataFile = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(maskDataPath), typeof(UnityEngine.Object));
                    LoadMaskData(true, indices, showProgress);
                }
#endif
            }
            else
                Debug.Log("Mask Data for layer: " + this.gameObject.name + " is empty!");
        }

        public void LoadMaskData(bool forced = false, List<int> indices = null, bool showProgress = false)
        {
            if (Application.isPlaying) return;
            if (!forced && maskDataFast != null) return;
            maskDataIsLoaded = false;

            // Port old version's mask data into our main MaskData struct
            if (maskData != null && maskData.Length > 0)
            {
                int maskResolution = maskData.Length;
                maskDataFast = new MaskDataFast[maskResolution];

                for (int i = 0; i < maskResolution; i++)
                {
                    maskDataFast[i].row = new float[maskResolution];

                    for (int j = 0; j < maskResolution; j++)
                        maskDataFast[i].row[j] = maskData[i].row[j];
                }

                //maskData = null;
                //SaveMaskData();
                maskDataIsLoaded = true;
            }
            else
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
                if (maskDataFile != null) maskDataPath = Path.GetFullPath(AssetDatabase.GetAssetPath(maskDataFile));
#endif

                if (File.Exists(maskDataPath))
                {
                    maskDataFast = null;

                    if (loadMaskOnThreads)
                    {
                        Thread thread = new Thread(new ThreadStart(DeserializeMask));
                        thread.Start();
                        thread.Join();
                    }
                    else
                        DeserializeMask();

                    CreateInstances(indices, showProgress);
                }
                else
                    Debug.Log("Mask Data file for layer: " + this.gameObject.name + " not found!");
            }
        }

        private void SetupRendering()
        {
            InitCullingPoints2Patch();
            InitRandomColors();
            SetCullingLODEditor(true);
        }

        public void InitRandomColors()
        {
            if (!randomColors) return;
            colorID = Shader.PropertyToID("_Color");
            if (colorID == -1) return;
            block = new MaterialPropertyBlock();
        }

        public void UpdateRandomColors()
        {
            if (!Application.isPlaying)
            {
                randomColorsList = new Color[randomColorsCount];

                for (int i = 0; i < randomColorsList.Length; i++)
                {
                    float colorRange = UnityEngine.Random.Range(0f, 1f);
                    randomColorsList[i] = colorGradient.Evaluate(colorRange);

                    // Other testings
                    //randomColorsList[i] = new Color(colorRange, colorRange, colorRange, 1f);
                    //randomColorsList[i] = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);
                }
            }
        }

        public bool GetPrefabCollider()
        {
            bool result = false;

            foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
            {
                if (!result && t.GetComponent<Collider>() != null && t.GetComponent<Collider>().enabled) // && hasCollision
                {
                    //LODWithCollider = t.gameObject;
                    result = true;
                }
            }

            if (!result)
            {
                foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
                {
                    if (!result && t.GetComponent<MeshCollider>() != null && t.GetComponent<MeshCollider>().enabled) // && hasCollision
                    {
                        //LODWithCollider = t.gameObject;
                        result = true;
                    }
                }
            }

            return result;
        }

        private void SetLODDistances()
        {
            LODsGO = new List<GameObject>();
            LODDistances = new List<float>();
            LODGroup lODGroup = prefab.GetComponent<LODGroup>();

            if (lODGroup != null)
            {
                LODGroupNotDetected = false;
                LOD[] lods = lODGroup.GetLODs();

                if (lods == null)
                    throw new Exception("LODgroup error on " + prefab.name);

                int index = 0;

                for (int i = 0; i < lods.Length; i++)
                {
                    Renderer[] renderers = lods[i].renderers;
                    if (renderers == null || renderers.Length == 0) throw new Exception("Rendrer error on LODGroup of " + prefab.name); ;
                    Renderer renderer = renderers[0];
                    if (renderer == null) throw new Exception("Rendrer[0] error on LODGroup of " + prefab.name); ;
                    if (renderer.sharedMaterial == null) throw new Exception("Material error on LODGroup of " + prefab.name); ;
                    if (renderer.gameObject.GetComponent<MeshFilter>() == null) throw new Exception("MeshFilter error on LODGroup of " + prefab.name);
                    if (renderer.gameObject.GetComponent<MeshFilter>().sharedMesh == null) throw new Exception("sharedMesh error on LODGroup of " + prefab.name);
                    LODsGO.Add(renderer.gameObject);
                    float LODDistance = GetDistanceToCamera(LODsGO[index], lods[i].screenRelativeTransitionHeight) * QualitySettings.lodBias * LODMultiplier;
                    LODDistances.Add(LODDistance);
                    index++;
                }

                LODsMeshes = new Mesh[lods.Length];
                LODsMaterials = new LODMaterials[lods.Length];

                for (int i = 0; i < LODsGO.Count; i++)
                {
                    LODsMeshes[i] = LODsGO[i].GetComponent<MeshFilter>().sharedMesh;
                    Material[] _LODsMaterials = LODsGO[i].GetComponent<Renderer>().sharedMaterials;
                    LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                    for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                        LODsMaterials[i].subMaterials[submeshIndex] = LODsGO[i].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                }

                if (LODDistances.Count > 0)
                    LODDistances[LODDistances.Count - 1] = GetDistanceToCamera(LODsGO[LODsGO.Count - 1], lods[lods.Length - 1].screenRelativeTransitionHeight) * QualitySettings.lodBias * LODMultiplier;
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
                        LODsMeshes = new Mesh[1];
                        LODsMaterials = new LODMaterials[1];

                        if (LODsGO[0] != null && LODsGO[0].GetComponent<MeshFilter>() != null && LODsGO[0].GetComponent<MeshFilter>().sharedMesh != null)
                            LODsMeshes[0] = LODsGO[0].GetComponent<MeshFilter>().sharedMesh;
                        else
                            throw new Exception("Mesh error on " + prefab.name);

                        Material[] _LODsMaterials = LODsGO[0].GetComponent<Renderer>().sharedMaterials;
                        LODsMaterials[0].subMaterials = new Material[_LODsMaterials.Length];

                        for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                        {
                            if (LODsGO[0] != null && LODsGO[0].GetComponent<Renderer>() != null && LODsGO[0].GetComponent<Renderer>().sharedMaterials[submeshIndex] != null)
                                LODsMaterials[0].subMaterials[submeshIndex] = LODsGO[0].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                            else
                                throw new Exception("Material error on " + prefab.name);
                        }

                        LODDistances.Add(maxDistance);
                    }
                    else
                    {
                        LODsMeshes = new Mesh[LODsGO.Count];
                        LODsMaterials = new LODMaterials[LODsGO.Count];
                        Material[] _LODsMaterials;

                        for (int i = 0; i < LODsGO.Count; i++)
                        {
                            if (LODsGO[i] != null && LODsGO[i].GetComponent<MeshFilter>() != null && LODsGO[i].GetComponent<MeshFilter>().sharedMesh != null)
                                LODsMeshes[i] = LODsGO[i].GetComponent<MeshFilter>().sharedMesh;
                            else
                                throw new Exception("Mesh error on " + prefab.name);

                            if (LODsGO[i] != null && LODsGO[i].GetComponent<Renderer>() != null && LODsGO[i].GetComponent<Renderer>().sharedMaterials != null)
                                _LODsMaterials = LODsGO[i].GetComponent<Renderer>().sharedMaterials;
                            else
                                throw new Exception("Material error on " + prefab.name);

                            LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                            for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                                if (LODsGO[i] != null && LODsGO[i].GetComponent<Renderer>() != null && LODsGO[i].GetComponent<Renderer>().sharedMaterials[submeshIndex] != null)
                                    LODsMaterials[i].subMaterials[submeshIndex] = LODsGO[i].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                                else
                                    throw new Exception("Material error on " + prefab.name);
                        }

                        LODDistances.Add(maxDistance);
                    }
                }
                else
                    throw new Exception("Prefab error : " + prefab.name);
            }
        }


        // Patches Generation
        //-------------------------------------------------------------------------------------------------------

        public override void UpdateLayer()
        {
            LoadMaskData();
            if (!CheckMask()) return;
            RegenerateLayer();
        }

        private void GeneratePatches()
        {
            bool debugDummies = false;
            float worldSizeMetersX = terrain.terrainData.size.x;
            float worldSizeMetersZ = terrain.terrainData.size.z;
            float maxRenderingDistance = maxDistance;
            if (LODDistances != null && LODDistances.Count > 0) maxRenderingDistance = LODDistances[LODDistances.Count - 1];
            float rowCount = (worldSizeMetersX / averageDistance);
            float totalNumber = Mathf.Pow(rowCount, 2);
            float instanceCountNormalizer = Mathf.Sqrt(totalNumber / 1000f); // 1000 is based on 1024 instances on each patch with 24 less items for backup
            int rowCountAverageDistance = Mathf.CeilToInt(instanceCountNormalizer);
            int rowCountMaxDistance = Mathf.CeilToInt(worldSizeMetersX / (maxRenderingDistance * 0.5f));
            if (rowCountAverageDistance > rowCountMaxDistance) patchesRowCount = rowCountAverageDistance;
            else patchesRowCount = rowCountMaxDistance;
            patchesColCount = patchesRowCount;
            patchScale = (worldSizeMetersX / patchesRowCount);
            patchData = new PatchData[patchesColCount * patchesRowCount];

            for (int x = 0; x < patchesRowCount; x++)
            {
                for (int y = 0; y < patchesColCount; y++)
                {
                    double xNormal = ((y + 0.5d) * 1.0d / patchesColCount);
                    double zNormal = ((x + 0.5d) * 1.0d / patchesRowCount);
                    float height = terrain.terrainData.GetInterpolatedHeight((float)xNormal, (float)zNormal);
                    Vector3 patchCenterWorldPosition = new Vector3((float)(xNormal * terrain.terrainData.size.x), height, (float)(zNormal * terrain.terrainData.size.z));
                    patchCenterWorldPosition.x += terrain.transform.position.x;
                    patchCenterWorldPosition.z += terrain.transform.position.z;
                    patchData[x * patchesColCount + y].positionX = patchCenterWorldPosition.x;
                    patchData[x * patchesColCount + y].positionY = patchCenterWorldPosition.y;
                    patchData[x * patchesColCount + y].positionZ = patchCenterWorldPosition.z;
                    patchData[x * patchesColCount + y].scale = patchScale;

                    if (debugDummies)
                    {
                        GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        dummy.name = "Sper - " + x + "-" + y + "-" + (x * patchesColCount + y).ToString();
                        dummy.transform.position = new Vector3(patchData[x * patchesColCount + y].positionX, patchData[x * patchesColCount + y].positionY, patchData[x * patchesColCount + y].positionZ);
                        dummy.transform.localScale = new Vector3(patchScale, patchScale, patchScale);
                    }
                }
            }
        }

        private void CreateInstances(List<int> indices = null, bool showProgress = false)
        {
            if (patchData == null || patchData.Length == 0) return;
            lastPositionTransform = Vector3.zero;

            try
            {
#if UNITY_EDITOR
                if (showProgress)
                {
                    progressID = TProgressBar.StartProgressBar("TERRAWORLD", "Updating Objects & Instances Placement for: " + gameObject.name, TProgressBar.ProgressOptionsList.Indefinite, false);
                    TProgressBar.DisplayProgressBar("TERRAWORLD", "Updating Objects & Instances Placement for: " + gameObject.name, 0.5f, progressID);
                }
#endif

                int terrainLayersCount = terrain.terrainData.terrainLayers.Length;
                if (exclusionOpacities == null || exclusionOpacities.Length == 0 || exclusionOpacities.Length != terrainLayersCount)
                {
                    exclusionOpacities = new float[terrainLayersCount];
                    for (int i = 0; i < exclusionOpacities.Length; i++) exclusionOpacities[i] = 0f;
                }

                for (int i = 0; i < patchData.Length; i++)
                {
                    if (indices != null && !indices.Contains(i)) continue;

                    patchData[i].Matrices = DataToMatrix.GenerateMatrices
                    (
                        maskDataFast,
                        terrain,
                        patchData[i],
                        averageDistance,
                        positionVariation,
                        density,
                        bypassLake,
                        underLake,
                        unityLayerMask,
                        minAllowedAngle,
                        maxAllowedAngle,
                        minAllowedHeight,
                        maxAllowedHeight,
                        positionOffset,
                        getSurfaceAngle,
                        lock90DegreeRotation,
                        minRotationRange,
                        maxRotationRange,
                        rotationOffset,
                        lockYRotation,
                        scale,
                        minScale,
                        maxScale,
                        checkBoundingBox,
                        biggestFaceLength,
                        exclusionOpacities,
                        seedNo + i
                    );
                }

                lastPositionTransform = transform.position;
            }
            finally
            {
#if UNITY_EDITOR
                if (showProgress)
                    TProgressBar.RemoveProgressBar(progressID);
#endif
            }
        }


        // Events
        //-------------------------------------------------------------------------------------------------------

        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;

            //base.Validate();
            if (prefab == null) return;

            for (int i = 0; i < LODDistances.Count; i++)
            {
                LODDistances[i] = Mathf.Clamp(LODDistances[i], 0, 100000);

                if (i > 0)
                    if (LODDistances[i] <= LODDistances[i - 1])
                        LODDistances[i] = LODDistances[i - 1] + 1;
            }

            if (LODGroupNotDetected && LODDistances != null)
            {
                if (LODDistances.Count > 0)
                    LODDistances[0] = maxDistance;
                else
                    LODDistances.Add(maxDistance);
            }

            SetCullingLODEditor();
        }

        private void OnEnable()
        {
            if (!isActiveAndEnabled) return;
            ConvertMaskFromTexture2D();
            if (TCameraManager.MainCamera != null && lastPositionCamera == TCameraManager.MainCamera.transform.position) lastPositionCamera += Vector3.one;
        }

        void OnDisable()
        {
            if (!isActiveAndEnabled) return;
            DisposeCullingGroup();
        }

        void Start()
        {
            if (!isActiveAndEnabled) return;
            Initialize();
        }

        private void Update()
        {
            if (!isActiveAndEnabled) return;
            TranslatePatchesIfNeeded();
            RenderPatches();
        }

        private void TranslatePatchesIfNeeded()
        {
            if (lastPositionTransform != transform.position)
            {
                LoadDataBuffer();
                Vector3 delta = transform.position - lastPositionTransform;
                PatchData.OffsetPositions(patchData, delta);
                Initialize();
                lastPositionTransform = transform.position;
            }
        }

        void OnPrefabInstanceUpdate(GameObject instance)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == prefab)
                RegenerateLayer();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DisposeCullingGroup();

            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdate;
#endif
        }


        // Initialization
        //-------------------------------------------------------------------------------------------------------

        public void Initialize()
        {
            if (Application.isPlaying) LoadDataBuffer(); //InitCullingGroup();
        }


        // CullingGroup API
        //-------------------------------------------------------------------------------------------------------

        public void InitCullingPoints2Patch()
        {
            if (patchData == null || patchData.Length == 0) return;

            //int patchCount = PatchData.PatchesCount(patchData);
            //int activePatches = PatchData.ActivePatchesCount(patchData);
            instanceCount = PatchData.InstanceCount(patchData);
            activePatchIndices = PatchData.ActivePatchIndices(patchData);

            InitCullingGroup();

            //Debug.Log(gameObject.name + "   Total Patches: " + patchCount + "   Instances: " + instanceCount + "   Active Patches: " + activePatches);
        }

        public void InitCullingGroup()
        {
            if (!isActiveAndEnabled) return;
            DisposeCullingGroup();

            cullingPoints = new BoundingSphere[activePatchIndices.Length];

            for (int i = 0; i < activePatchIndices.Length; i++)
            {
                cullingPoints[i].position = new Vector3(patchData[activePatchIndices[i]].positionX, patchData[activePatchIndices[i]].positionY, patchData[activePatchIndices[i]].positionZ);
                float hypotenuseFromCenter = Mathf.Sqrt(patchScale * patchScale + patchScale * patchScale) / 2f * frustumMultiplier;
                cullingPoints[i].radius = hypotenuseFromCenter;
            }

            LODList = new int[LODDistances.Count];
            matricesList = new List<Matrix4x4>[LODDistances.Count, activePatchIndices.Length];
            activeIndices = new int[activePatchIndices.Length];

            localCullingGroup = new CullingGroup();
            localCullingGroup.onStateChanged += CullingEvent;
            localCullingGroup.SetBoundingSpheres(cullingPoints);
            localCullingGroup.SetBoundingDistances(LODDistances.ToArray());
            localCullingGroup.SetDistanceReferencePoint(TCameraManager.MainCamera.transform.position);
            localCullingGroup.targetCamera = TCameraManager.MainCamera;

            //patchBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //patchBox.transform.localScale = Vector3.one * patchScale * frustumMultiplier;
            //
            //
            //patchBox.GetComponent<MeshRenderer>().enabled = false;
            //patchBox.GetComponent<BoxCollider>().isTrigger = true;
            //
            ////patchBoxMaterial = new Material(Shader.Find("TerraUnity/Occlusion Detector"));
            ////patchBox.GetComponent<MeshRenderer>().material = patchBoxMaterial;
            ////patchBox.GetComponent<BoxCollider>().enabled = false;
            //
            //
            //patchBox.hideFlags = HideFlags.HideInHierarchy;
            ////patchBoxMesh = patchBox.GetComponent<MeshFilter>().sharedMesh;
            //patchBoxBounds = patchBox.GetComponent<BoxCollider>().bounds;

            //float startDistance = 0;
            //float endDistance = LODDistances[LODDistances.Count - 1];
            //Vector3 patchSize = new Vector3(patchScale * frustumMultiplier, patchScale * frustumMultiplier, patchScale * frustumMultiplier);

            maxRenderingDistanceSqrt = LODDistances[LODDistances.Count - 1] * LODDistances[LODDistances.Count - 1];
        }

        private void DisposeCullingGroup()
        {
            if (localCullingGroup == null) return;
            localCullingGroup.onStateChanged -= CullingEvent;
            localCullingGroup.Dispose();
            localCullingGroup = null;
        }

        private void SetReferencePoint()
        {
            if (localCullingGroup == null) LoadDataBuffer(); //InitCullingGroup();
            if (localCullingGroup != null) localCullingGroup.SetDistanceReferencePoint(TCameraManager.MainCamera.transform.position);
        }

        void CullingEvent(CullingGroupEvent sphere)
        {
            if (!Application.isPlaying) return;
            //ManageCulling(sphere.index, sphere.isVisible);
            ManageLODs();
        }


        // Rendering
        //-------------------------------------------------------------------------------------------------------

        private void RenderPatches()
        {
            if (Application.isPlaying) SetReferencePoint();
            else SetCullingLODEditor();

            //if (activePatchIndices == null || activePatchIndices.Length == 0) InitcullingPoints2Patch();
            //if (activePatchIndices == null || activePatchIndices.Length == 0) return;
            if (prefab == null) throw new Exception("Missing prefab of  " + gameObject.name); ;
            if (LODList == null) return;
            if (LODDistances == null) return;
            if (LODsMaterials == null) return;
            if (LODsMeshes == null) return;
            if (matricesList == null) return;
            if (randomColors && block == null) InitRandomColors();

            if (!isCompoundModel)
            {
                for (int LODIndex = 0; LODIndex < LODDistances.Count; LODIndex++)
                {
                    for (int i = 0; i < LODList[LODIndex]; i++)
                    {
                        for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].subMaterials.Length; submeshIndex++)
                        {
                            if (matricesList[LODIndex, i] == null) continue;

                            if (randomColors && randomColorsList != null && randomColorsList.Length == randomColorsCount)
                            {
                                for (int j = 0; j < matricesList[LODIndex, i].Count; j++)
                                {
                                    Matrix4x4 matrix = matricesList[LODIndex, i][j];
                                    int determinant = TMatrix.Determinant(matrix);
                                    System.Random randomSeed = new System.Random(determinant);
                                    Color randomColor = randomColorsList[(int)TUtils.RandomRangeSeed(randomSeed, 0f, randomColorsList.Length)];
                                    block.SetColor(colorID, randomColor);

                                    if (LODsMeshes[LODIndex] == null)
                                        throw new Exception("Mesh Error  on " + prefab);
                                    if (LODsMaterials[LODIndex].subMaterials[submeshIndex] == null)
                                        throw new Exception("Material Error  on " + prefab);

                                    Graphics.DrawMesh
                                    (
                                        LODsMeshes[LODIndex],
                                        matricesList[LODIndex, i][j],
                                        LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                        unityLayerIndex,
                                        null,
                                        submeshIndex,
                                        block,
                                        true,
                                        receiveShadows
                                    );

                                    //List<Matrix4x4> xxx = new List<Matrix4x4>(1);
                                    //xxx.Add(matricesList[LODIndex, i][j]);
                                    //
                                    //Graphics.DrawMeshInstanced
                                    //(
                                    //    LODsMeshes[LODIndex],
                                    //    submeshIndex,
                                    //    LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                    //    xxx,
                                    //    block,
                                    //    shadowCastMode,
                                    //    receiveShadows,
                                    //    unityLayerIndex
                                    //);
                                }
                            }
                            else
                            {
                                if (LODsMeshes[LODIndex] == null)
                                    throw new Exception("Mesh Error  on " + prefab);
                                if (LODsMaterials[LODIndex].subMaterials[submeshIndex] == null)
                                    throw new Exception("Material Error  on " + prefab);
                                Graphics.DrawMeshInstanced
                                (
                                    LODsMeshes[LODIndex],
                                    submeshIndex,
                                    LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                    matricesList[LODIndex, i],
                                    null,
                                    shadowCastMode,
                                    receiveShadows,
                                    unityLayerIndex
                                );
                            }
                        }
                    }
                }
            }
            else
            {
                for (int LODIndex = 0; LODIndex < LODsMeshes.Length; LODIndex++)
                {
                    for (int i = 0; i < LODList[0]; i++)
                    {
                        for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].subMaterials.Length; submeshIndex++)
                        {
                            if (matricesList[0, i] == null) continue;

                            if (randomColors && randomColorsList != null && randomColorsList.Length == randomColorsCount)
                            {
                                for (int j = 0; j < matricesList[0, i].Count; j++)
                                {
                                    Matrix4x4 matrix = matricesList[0, i][j];
                                    int determinant = TMatrix.Determinant(matrix);
                                    System.Random randomSeed = new System.Random(determinant);
                                    Color randomColor = randomColorsList[(int)TUtils.RandomRangeSeed(randomSeed, 0f, randomColorsList.Length)];
                                    block.SetColor(colorID, randomColor);

                                    if (LODsMeshes[LODIndex] == null)
                                        throw new Exception("Mesh Error  on " + prefab);
                                    if (LODsMaterials[LODIndex].subMaterials[submeshIndex] == null)
                                        throw new Exception("Material Error  on " + prefab);

                                    Graphics.DrawMesh
                                    (
                                        LODsMeshes[LODIndex],
                                        matricesList[0, i][j],
                                        LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                        unityLayerIndex,
                                        null,
                                        submeshIndex,
                                        block,
                                        true,
                                        receiveShadows
                                    );
                                }
                            }
                            else
                            {
                                if (LODsMeshes[LODIndex] == null)
                                    throw new Exception("Mesh Error  on " + prefab);
                                if (LODsMaterials[LODIndex].subMaterials[submeshIndex] == null)
                                    throw new Exception("Material Error  on " + prefab);

                                Graphics.DrawMeshInstanced
                                (
                                    LODsMeshes[LODIndex],
                                    submeshIndex,
                                    LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                    matricesList[0, i],
                                    null,
                                    shadowCastMode,
                                    receiveShadows,
                                    unityLayerIndex
                                );
                            }
                        }
                    }
                }
            }
        }


        // LOD & Culling Runtime
        //-------------------------------------------------------------------------------------------------------

        //private bool IsInView(Vector3[] targetPoints)
        //{
        //    int hits = 0;
        //
        //    for (int i = 0; i < targetPoints.Length; i++)
        //    {
        //        RaycastHit hit;
        //
        //        // Only check against colliders with Static flag of "Occluder Static" such as Terrains in scene
        //        if (Physics.Linecast(renderingCamera.transform.position, targetPoints[i], out hit))
        //        {
        //            if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //                hits++;
        //            else
        //                return true;
        //        }
        //    }
        //
        //    //for (int i = 0; i < targetPoints.Length; i++)
        //    //{
        //    //    Vector3 heading = targetPoints[i] - renderingCamera.transform.position;
        //    //    Vector3 direction = heading.normalized;// / heading.magnitude;
        //    //    Ray ray = new Ray(renderingCamera.transform.position, direction);
        //    //    RaycastHit hit;
        //    //
        //    //    // Only check against colliders with Static flag of "Occluder Static" such as Terrains in scene
        //    //    if (Raycasts.RaycastNonAlloc(ray, out hit, ~0, LODDistances[LODDistances.Count - 1]))
        //    //    {
        //    //        if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //    //            hits++;
        //    //        else
        //    //            return true;
        //    //    }
        //    //}
        //
        //    if (hits == targetPoints.Length) return false;
        //    else return true;
        //}

        //private bool IsInView(Vector3 targetPoint)
        //{
        //    bool result = true;
        //    Vector3 pointOnScreen = renderingCamera.WorldToScreenPoint(targetPoint);
        //    RaycastHit hit;
        //
        //    // Only check against colliders in front of camera with Static flag of "Occluder Static" such as Terrains in scene
        //    if (pointOnScreen.z > patchScale && Physics.Linecast(renderingCamera.transform.position, targetPoint, out hit))
        //    {
        //        Renderer[] renderers = hit.transform.GetComponentsInChildren<Renderer>();
        //        
        //        for (int i = 0; i < renderers.Length; i++)
        //        {
        //            if (renderers[i].isPartOfStaticBatch)
        //            {
        //                result = false;
        //                break;
        //            }
        //        }
        //    
        //        //if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //        //    result = false;
        //        //else
        //        //    result = true;
        //    }
        //}

        private bool IsInView(Vector3 targetPoint)
        {
            if (Physics.Linecast(TCameraManager.MainCamera.transform.position, targetPoint))
                return false;

            return true;
        }

        private void ManageLODs()
        {
            if (lastRotationCamera == TCameraManager.MainCamera.transform.rotation && lastPositionCamera == TCameraManager.MainCamera.transform.position) return;

            //if (lastRotationCamera == renderingCamera.transform.rotation && lastPositionCamera != renderingCamera.transform.position)
            //{
            //    float lastPosDistance = (lastPositionCamera - renderingCamera.transform.position).sqrMagnitude;
            //    if (lastPosDistance <= deadZoneMeters * deadZoneMeters) return;
            //}

            if (occlusionCulling)
            {
                cameraHeight = TCameraManager.MainCamera.transform.position.y - terrain.SampleHeight(TCameraManager.MainCamera.transform.position);
                //occlusionDistance = Mathf.Pow(LODDistances[0] * 2f, 2f);
            }

            // Find all visible spheres in different LODs
            for (int i = 0; i < LODDistances.Count; i++)
            {
                LODList[i] = localCullingGroup.QueryIndices(true, i, activeIndices, 0);
                //int patchesNotInView = 0;
                //int index = 0;

                for (int j = 0; j < LODList[i]; j++)
                {
                    if (activeIndices[j] > activePatchIndices.Length - 1) return;
                    int patchIndex = activePatchIndices[activeIndices[j]];
                    PatchData patch = patchData[patchIndex];
                    Vector3 patchPosition = new Vector3(patch.positionX, patch.positionY, patch.positionZ);
                    float patchDistance = (patchPosition - TCameraManager.MainCamera.transform.position).sqrMagnitude;

                    if (patchDistance <= maxRenderingDistanceSqrt)
                    {
                        if (occlusionCulling)
                        {
                            if (cameraHeight < occlusionCheckHeight)
                            //if (patchDistance <= occlusionDistance && cameraHeight < occlusionCheckHeight)
                            {
                                //Vector3[] targetPoints = new Vector3[1];
                                //targetPoints[0] = patch.position + (Vector3.up * patchScale);
                                //targetPoints[1] = patch.position + (Vector3.up * patchScale) + (Vector3.left * patchScale);
                                //targetPoints[2] = patch.position + (Vector3.up * patchScale) + (Vector3.right * patchScale);
                                //targetPoints[3] = patch.position + (Vector3.left * patchScale);
                                //targetPoints[4] = patch.position + (Vector3.right * patchScale);

                                // Ray against top-center point on patch's bounding box to check if any collisions are through
                                if (IsInView(patchPosition + (Vector3.up * patchScale * 0.666f)))
                                    matricesList[i, j] = patch.Matrices;
                                else
                                    matricesList[i, j] = null;
                            }
                            else
                                matricesList[i, j] = patch.Matrices;
                        }
                        else
                            matricesList[i, j] = patch.Matrices;
                    }
                    else
                        matricesList[i, j] = null;


                    //patchBox.transform.position = patch.position;
                    //Vector3 heading = patchBox.transform.position - renderingCamera.transform.position;
                    //Vector3 direction = heading.normalized;// / heading.magnitude;
                    //
                    //Ray ray = new Ray(renderingCamera.transform.position, renderingCamera.transform.forward * Vector3.Angle(renderingCamera.transform.position, patchBox.transform.position));
                    //
                    //if (!patchBoxBounds.IntersectRay(ray))
                    //{
                    //    patchesNotInView++;
                    //    Debug.Log("Skipped " + patchIndex);
                    //    continue;
                    //}

                    //Graphics.Blit(null, renderTexture, patchBoxMaterial);
                    //RenderTexture.active = renderTexture;
                    //outputTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0, false);
                    //Color[] colors = outputTex.GetPixels();
                    //bool occluded = false;
                    //
                    //for (int k = 0; k < colors.Length; k++)
                    //{
                    //    if (colors[k] == Color.white)
                    //    {
                    //        occluded = false;
                    //        break;
                    //    }
                    //
                    //    if (k == colors.Length - 1)
                    //    {
                    //        occluded = true;
                    //        Debug.Log("Skipped " + patchIndex);
                    //    }
                    //}
                    //
                    //if (occluded)
                    //{
                    //    patchesNotInView++;
                    //    continue;
                    //}
                    //
                    //matricesList[i, index] = patch.matrices;
                    //index++;

                    //bool notInView = false;
                    //
                    //for (int k = 0; k < 8; k++)
                    //{
                    //    Ray ray = new Ray(renderingCamera.transform.position, renderingCamera.transform.forward * Vector3.Angle(renderingCamera.transform.position, patchBoxMesh.vertices[k]));
                    //    
                    //    if (!Physics.Raycast(ray))
                    //    {
                    //        notInView = true;
                    //        break;
                    //    }
                    //}
                    //
                    //if (notInView)
                    //{
                    //    patchesNotInView++;
                    //    continue;
                    //}

                    //matricesList[i, index] = patch.matrices;
                    //index++;
                }

                //LODList[i] -= patchesNotInView;
            }

            lastRotationCamera = TCameraManager.MainCamera.transform.rotation;
            lastPositionCamera = TCameraManager.MainCamera.transform.position;
        }


        // LOD Editor
        //-------------------------------------------------------------------------------------------------------

        public void SetCullingLODEditor(bool forced = false)
        {
#if UNITY_EDITOR
            if (Application.isPlaying || TCameraManager.SceneCamera == null) return;
            if (LODDistances == null || LODDistances.Count == 0) return;

            if
            (
                lastPositionCamera != TCameraManager.SceneCamera.transform.position ||
                lastRotationCamera != TCameraManager.SceneCamera.transform.rotation ||
                forced
            )
            {
                Vector3 cameraPosition = TCameraManager.SceneCamera.transform.position;
                if (LODList == null || LODList.Length != LODDistances.Count) LODList = new int[LODDistances.Count];

                LoadDataBuffer();
                if (!patchDataIsLoaded) return;

                if (activePatchIndices == null || activePatchIndices.Length == 0)
                    InitCullingPoints2Patch();

                if (matricesList == null || matricesList.GetLength(0) != LODDistances.Count || matricesList.GetLength(1) != patchData.Length)
                    matricesList = new List<Matrix4x4>[LODDistances.Count, patchData.Length];

                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(TCameraManager.SceneCamera);
                float startDistance = 0;
                float endDistance = LODDistances[LODDistances.Count - 1];
                Vector3 patchSize = new Vector3(patchScale * frustumMultiplier, patchScale * frustumMultiplier, patchScale * frustumMultiplier);
                float maxDistance = LODDistances[LODDistances.Count - 1] * LODDistances[LODDistances.Count - 1];

                for (int i = 0; i < LODDistances.Count; i++)
                {
                    LODList[i] = 0;

                    for (int j = 0; j < activePatchIndices.Length; j++)
                    {
                        Vector3 patchPosition = new Vector3(patchData[activePatchIndices[j]].positionX, patchData[activePatchIndices[j]].positionY, patchData[activePatchIndices[j]].positionZ);
                        float cameraDistance = (cameraPosition - patchPosition).sqrMagnitude;
                        if (cameraDistance > maxDistance) continue;
                        Bounds patchBounds = new Bounds(patchPosition, patchSize);
                        if (!GeometryUtility.TestPlanesAABB(planes, patchBounds)) continue;
                        if (i == 0) startDistance = 0;
                        else startDistance = LODDistances[i - 1];
                        endDistance = LODDistances[i];

                        if (cameraDistance > startDistance * startDistance && cameraDistance <= endDistance * endDistance)
                        {
                            matricesList[i, LODList[i]] = patchData[activePatchIndices[j]].Matrices;
                            LODList[i]++;
                        }
                    }
                }

                SceneView.RepaintAll();

                lastPositionCamera = TCameraManager.SceneCamera.transform.position;
                lastRotationCamera = TCameraManager.SceneCamera.transform.rotation;
            }
#endif
        }


        // Player Interactions
        //-------------------------------------------------------------------------------------------------------

        public Matrix4x4 GetColliderMatrix(Vector3 playerPos)
        {
            if (localCullingGroup == null) return lastColliderMatrix;
            Matrix4x4 result = new Matrix4x4();
            Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
            int index = 0;
            int row = 0;
            int col = 0;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                float lastDistance = float.MaxValue;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            List<Matrix4x4> matrixList = patchData[index].Matrices;

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector2 instancePos2D = TMatrix.ExtractTranslationFromMatrix2D(ref matrix);
                                float distance = (instancePos2D - playerPos2D).sqrMagnitude;

                                if (distance < lastDistance)
                                {
                                    result = matrix;
                                    lastDistance = distance;
                                }
                            }
                        }
                    }
                }
            }

            lastColliderMatrix = result;
            return result;
        }

        public List<Matrix4x4> GetInstanceMatrices3D(Vector3 playerPos, float checkDistance, int neighborPatchesCount)
        {
            if (patchData == null) return null;
            Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
            List<Matrix4x4> result = new List<Matrix4x4>();
            int index, row, col;
            int indexStart = -neighborPatchesCount;
            int indexEnd = neighborPatchesCount + 1;
            //int patchesCount = (indexStart * -1) + indexEnd;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                for (int i = indexStart; i < indexEnd; i++)
                {
                    for (int j = indexStart; j < indexEnd; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            if (index > patchData.Length - 1) continue;
                            List<Matrix4x4> matrixList = patchData[index].Matrices;
                            if (matrixList == null || matrixList.Count == 0) continue;
                            List<int> validIndices = new List<int>();

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector3 instancePos = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                                float distance = (instancePos - playerPos).sqrMagnitude;

                                if (distance < checkDistance * checkDistance)
                                {
                                    validIndices.Add(k);
                                    result.Add(matrix);
                                }
                            }

                            GPURemove(index, validIndices);
                        }
                    }
                }
            }

            return result;
        }

        public List<Matrix4x4> GetInstanceMatrices2D(Vector2 playerPos2D, float checkDistance, int neighborPatchesCount)
        {
            if (patchData == null) return null;
            List<Matrix4x4> result = new List<Matrix4x4>();
            int index, row, col;
            int indexStart = -neighborPatchesCount;
            int indexEnd = neighborPatchesCount + 1;
            //int patchesCount = (indexStart * -1) + indexEnd;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                for (int i = indexStart; i < indexEnd; i++)
                {
                    for (int j = indexStart; j < indexEnd; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            if (index > patchData.Length - 1) continue;
                            List<Matrix4x4> matrixList = patchData[index].Matrices;
                            if (matrixList == null || matrixList.Count == 0) continue;
                            List<int> validIndices = new List<int>();

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector2 instancePos2D = TMatrix.ExtractTranslationFromMatrix2D(ref matrix);
                                float distance = (instancePos2D - playerPos2D).sqrMagnitude;

                                if (distance < checkDistance * checkDistance)
                                {
                                    validIndices.Add(k);
                                    result.Add(matrix);
                                }
                            }

                            GPURemove(index, validIndices);
                        }
                    }
                }
            }

            return result;
        }

        private void GPURemove(int index, List<int> indices)
        {
            for (int i = 0; i < indices.Count; i++)
                patchData[index].Matrices.RemoveAt(indices[i] - i);
        }


        // Helpers
        //-------------------------------------------------------------------------------------------------------

        public bool GetPatchesRowCol(Vector2 GlobalPosition, out int Index, out int Row, out int Col)
        {
            Index = Row = Col = 0;
            if (GlobalPosition.x < terrain.transform.position.x) return false;
            if (GlobalPosition.x > terrain.transform.position.x + terrain.terrainData.size.x) return false;
            if (GlobalPosition.y < terrain.transform.position.z) return false;
            if (GlobalPosition.y > terrain.transform.position.z + terrain.terrainData.size.z) return false;
            double xNormal = (GlobalPosition.x - terrain.transform.position.x) * 1.0d / terrain.terrainData.size.x;
            double zNormal = (GlobalPosition.y - terrain.transform.position.z) * 1.0d / terrain.terrainData.size.z;
            Col = (int)(xNormal * patchesColCount);
            Row = (int)(zNormal * patchesRowCount);
            if (GetPatchesIndex(Row, Col, out Index)) return true;

            return false;
        }

        public bool GetPatchesIndex(int Row, int Col, out int Index)
        {
            Index = 0;
            if (Col < 0) return false;
            if (Col >= patchesColCount) return false;
            if (Row < 0) return false;
            if (Row >= patchesRowCount) return false;
            Index = Row * patchesColCount + Col;

            return true;
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

        private float GetDistanceToCamera(GameObject go, float NormalizedPersentage)
        {
            GetBiggestFaceLength(go);
            return biggestFaceLength * maxScale / (((TCameraManager.MainCamera.pixelRect.height * NormalizedPersentage) / TCameraManager.MainCamera.pixelHeight) * (2 * Mathf.Tan(TCameraManager.MainCamera.fieldOfView / 2 * Mathf.Deg2Rad)));
        }

#if UNITY_EDITOR
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL) return;
            string streamingAssetsPath = Path.GetFullPath(Application.dataPath + "/StreamingAssets");
            if (!Directory.Exists(streamingAssetsPath)) return;
            string[] patchDataPaths = Directory.GetFiles(streamingAssetsPath, "*.patchdata", SearchOption.AllDirectories);
            if (patchDataPaths == null || patchDataPaths.Length == 0) return;
            foreach (string s in patchDataPaths) File.Delete(s);
            AssetDatabase.Refresh();

            Debug.Log("All GPU layers's data have been embedded into build's internal data.");
        }
#endif

        //private float RandomRange(System.Random random, float min, float max)
        //{
        //    return (float)(random.NextDouble() * (max - min) + min);
        //}

        //private void ManageCulling (int index, bool visible)
        //{
        //    patchRenderers[index].enabled = visible;
        //}
    }

#if UNITY_EDITOR
    public class BuildProcessorGPU : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL) return;
            string streamingAssetsPath = Path.GetFullPath(Application.dataPath + "/StreamingAssets");

            if (!Directory.Exists(streamingAssetsPath))
                Directory.CreateDirectory(streamingAssetsPath);
            else
            {
                string[] patchDataPaths = Directory.GetFiles(streamingAssetsPath, "*.patchdata", SearchOption.AllDirectories);
                foreach (string s in patchDataPaths) File.Delete(s);
            }

            AssetDatabase.Refresh();
            TScatterParams[] GPULayers = MonoBehaviour.FindObjectsOfType<TScatterParams>();

            if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                foreach (TScatterParams g in GPULayers)
                {
                    string streamingAssetsPatchDataPath = Path.Combine(streamingAssetsPath, g.gameObject.name + ".patchdata");
                    File.Copy(g.patchDataPath, streamingAssetsPatchDataPath, true);
                }
        }
    }
#endif
}

