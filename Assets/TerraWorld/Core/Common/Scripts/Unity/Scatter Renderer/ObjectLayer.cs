#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class ObjectLayer : TScatterLayer
    {
        public override void UpdateLayer()
        {
            LoadMaskData();
            if (!CheckMask()) return;
            RemovePreviousInstances();
            GetBiggestFaceLength();
            GeneratePatches();
            UpdatePatches();
            UpdateInstances();
        }

        public void SaveMaskData()
        {
            if (Application.isPlaying) return;
            maskDataIsSaved = false;

#if UNITY_EDITOR && TERRAWORLD_PRO
            maskDataPath = Path.GetFullPath(TTerraWorldManager.WorkDirectoryLocalPath + this.gameObject.name + ".maskdata");
#endif

            if (maskDataFast != null && maskDataFast.Length > 0)
            {
                Thread thread = new Thread(new ThreadStart(SerializeMask));
                thread.Start();

#if UNITY_EDITOR
                thread.Join();

                if (File.Exists(maskDataPath))
                {
                    maskDataFile = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(maskDataPath), typeof(UnityEngine.Object));
                    LoadMaskData(true);
                }
#endif
            }
            else
                Debug.Log("Patch Data for layer: " + this.gameObject.name + " is empty!");
        }

        public void LoadMaskData(bool forced = false)
        {
            if (!forced && maskDataFast != null) return;
            maskDataIsLoaded = false;

            if (File.Exists(maskDataPath))
            {
                maskDataFast = null;
                DeserializeMask();

                //Thread thread = new Thread(new ThreadStart(DeserializeMask));
                //thread.Start();
                //thread.Join();
                //SetupRendering();
            }
            else
                Debug.Log("Mask Data file for layer: " + this.gameObject.name + " not found!");
        }

        private void RemovePreviousInstances()
        {
            //TODO: Destroy root objects under this transform if available
        }

        private void GeneratePatches()
        {
            patchData = new PatchData[1];
            patchData[0] = new PatchData();
            Vector3 terrainPosition = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;
            patchData[0].positionX = terrainPosition.x + (terrainSize.x / 2f);
            patchData[0].positionY = terrainPosition.y;
            patchData[0].positionZ = terrainPosition.z + (terrainSize.z / 2f);
            patchData[0].scale = terrainSize.x;
        }

        public void UpdatePatches()
        {
            if (patchData == null || patchData.Length == 0) return;
            if (!CheckMask()) return;

            int terrainLayersCount = terrain.terrainData.terrainLayers.Length;
            if (exclusionOpacities == null || exclusionOpacities.Length == 0 || exclusionOpacities.Length != terrainLayersCount)
            {
                exclusionOpacities = new float[terrainLayersCount];
                for (int i = 0; i < exclusionOpacities.Length; i++) exclusionOpacities[i] = 0f;
            }

            for (int i = 0; i < patchData.Length; i++)
            {
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
                    seedNo + i,
                    placeSingleItem
                );
            }
        }

        private void UpdateInstances()
        {
            System.Random rand = new System.Random(prefabNames.Count);

            for (int i = 0; i < patchData.Length; i++)
            {
                List<Matrix4x4> matrices = patchData[i].Matrices;

                for (int j = 0; j < matrices.Count; j++)
                {
                    Matrix4x4 matrix = matrices[j];
                    Vector3 position = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                    Quaternion rotation = TMatrix.ExtractRotationFromMatrix(ref matrix);
                    Vector3 localScale = TMatrix.ExtractScaleFromMatrix(ref matrix);

                    if (layerType == LayerType.ScatteredObject)
                    {
                        int randomObjectIndex = rand.Next(0, prefabNames.Count);
                        string prefabName = prefabNames[randomObjectIndex];
                        UnityEngine.Object Objectprefab = AssetDatabase.LoadAssetAtPath(prefabName, typeof(UnityEngine.Object));
                        GameObject go = Instantiate(Objectprefab) as GameObject;
                        go.name = Path.GetFileNameWithoutExtension(prefabName) + " " + (j + 1);
                        go.transform.parent = transform;

                        foreach (Transform t in go.GetComponentsInChildren(typeof(Transform), false))
                            t.gameObject.layer = LayerMask.NameToLayer(unityLayerName);

                        go.transform.position = position;
                        go.transform.rotation = rotation;
                        go.transform.localScale = localScale;
                    }
                    else if (layerType == LayerType.ScatteredTrees)
                    {
                        TreeInstance tree = new TreeInstance();
                        tree.prototypeIndex = terrain.terrainData.treePrototypes.Length - 1;
                        Vector3 offsetPos = position;
                        offsetPos = offsetPos - terrain.transform.position;
                        tree.position = new Vector3(offsetPos.x * 1.0f / terrain.terrainData.size.x, 0, offsetPos.z * 1.0f / terrain.terrainData.size.z);
                        tree.rotation = rotation.y * Mathf.Deg2Rad;
                        tree.widthScale = localScale.x;
                        tree.heightScale = localScale.y;
                        terrain.AddTreeInstance(tree);
                    }
                }
            }

            if (layerType == LayerType.ScatteredTrees)
            {
                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();
            }
        }

        private void GetBiggestFaceLength()
        {
            try
            {
                biggestFaceLength = float.MinValue;

                for (int i = 0; i < prefabNames.Count; i++)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath(prefabNames[i], typeof(GameObject)) as GameObject;

                    foreach (Transform t in go.GetComponentsInChildren(typeof(Transform), true))
                    {
                        if (t.GetComponent<Renderer>() == null) continue;
                        Bounds bounds = t.GetComponent<Renderer>().bounds;

                        if (biggestFaceLength < bounds.extents.x)
                            biggestFaceLength = bounds.extents.x;
                        if (biggestFaceLength < bounds.extents.y)
                            biggestFaceLength = bounds.extents.y;
                        if (biggestFaceLength < bounds.extents.z)
                            biggestFaceLength = bounds.extents.z;
                    }
                }

                if (scale.x >= scale.z)
                    biggestFaceLength *= scale.x * maxScale;
                else
                    biggestFaceLength *= scale.z * maxScale;
            }
            finally
            {
                biggestFaceLength = 1;
            }
        }

        // Events
        //-------------------------------------------------------------------------------------------------------

        private void OnEnable()
        {
            ConvertMaskFromTexture2D();
        }

        private void OnValidate()
        {
            base.Validate();

            //TODO: Update layers when prefab changes or any of the main settings are changed!
        }

        void OnPrefabInstanceUpdate(GameObject instance)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == prefab)
                UpdateLayer();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdate;
#endif
        }
    }
}
#endif

