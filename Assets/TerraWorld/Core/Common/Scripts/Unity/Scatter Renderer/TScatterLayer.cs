using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerraUnity.Runtime
{
    [Serializable]
    public class TScatterLayer : TLayerScript
    {
        public GameObject prefab;
        [HideInInspector, Range(1f, 100f)] public float density = 100f;
        [HideInInspector] public Vector3 scale;
        [HideInInspector, Range(0.1f, 20f)] public float minScale;
        [HideInInspector, Range(0.1f, 20f)] public float maxScale;
        [HideInInspector, Range(0f, 100f)] public float positionVariation;

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
        // Default is 256 so 4 meters average distance between models is supported
        [HideInInspector, Range(1f, 1000f)] public float averageDistance;

        [HideInInspector] public bool lock90DegreeRotation;
        [HideInInspector] public bool lockYRotation;
        [HideInInspector] public bool getSurfaceAngle;
        [HideInInspector] public int seedNo;
        [HideInInspector] public int priority;
        [HideInInspector] public Vector3 positionOffset;
        [HideInInspector] public Vector3 rotationOffset;
        [HideInInspector, Range(0f, 359f)] public float minRotationRange;
        [HideInInspector, Range(0f, 359f)] public float maxRotationRange;
        [HideInInspector, Range(0f, 90f)] public float minAllowedAngle;
        [HideInInspector, Range(0f, 90f)] public float maxAllowedAngle;
        [HideInInspector, Range(-100000f, 100000f)] public float minAllowedHeight;
        [HideInInspector, Range(-100000f, 100000f)] public float maxAllowedHeight;
        [HideInInspector] public LayerMask layerMask;
        [HideInInspector] public bool bypassLake;
        [HideInInspector] public bool underLake;
        [HideInInspector] public bool underLakeMask;
        [HideInInspector] public bool onLake;

        //public class LayerAttribute : PropertyAttribute { }
        //[Layer]
        [HideInInspector] public int unityLayerMask;

        [HideInInspector] public bool isInitialized = false;
        [HideInInspector] public bool occlusionCulling = false;
        [HideInInspector] public bool checkBoundingBox;
        [HideInInspector] public float biggestFaceLength = float.MinValue;
        [HideInInspector, Range(0f, 100f)] public float[] exclusionOpacities;

#if UNITY_EDITOR
        [HideInInspector] public LayerType layerType;
#endif

        [HideInInspector] public List<string> prefabNames;
        [HideInInspector] public int undoMode = 1;
        [HideInInspector] public bool placeSingleItem;

        public Texture2D filter;

        // New Patch system which holds its data in a file with binary format for fast saving/loading of layer's data
        [Serializable]
        public struct PatchData
        {
            // Patch Position
            public float positionX;
            public float positionY;
            public float positionZ;

            // Scale
            public float scale;

            // Matrix Data
            public float[] m00;
            public float[] m33;
            public float[] m23;
            public float[] m13;
            public float[] m03;
            public float[] m32;
            public float[] m22;
            public float[] m02;
            public float[] m12;
            public float[] m21;
            public float[] m11;
            public float[] m01;
            public float[] m30;
            public float[] m20;
            public float[] m10;
            public float[] m31;

            public List<Matrix4x4> Matrices
            {
                get
                {
                    return _matrices;
                }
                set
                {
                    _matrices = value;
                }
            }

            [NonSerialized]
            private List<Matrix4x4> _matrices;

            public void Serialize()
            {
                FillMatrixDataFromList(_matrices);
            }

            public void Deserialize()
            {
                _matrices = GetMatrices();
            }

            public static int PatchesCount(PatchData[] patches)
            {
                return patches.Length;
            }

            public static int ActivePatchesCount(PatchData[] patches)
            {
                int result = 0;

                for (int i = 0; i < patches.Length; i++)
                    if (!patches[i].IsEmpty())
                        result++;

                return result;
            }

            public static int[] ActivePatchIndices(PatchData[] patches)
            {
                int[] result = new int[ActivePatchesCount(patches)];
                int counter = 0;

                for (int i = 0; i < patches.Length; i++)
                    if (!patches[i].IsEmpty())
                        result[counter++] = i;

                return result;
            }

            public static int InstanceCount(PatchData[] patches)
            {
                int result = 0;

                for (int i = 0; i < patches.Length; i++)
                    if (!patches[i].IsEmpty())
                        result += patches[i]._matrices.Count;

                return result;
            }

            public static void OffsetPositions(PatchData[] patches, Vector3 offset)
            {
                for (int i = 0; i < patches.Length; i++)
                {
                    if (!patches[i].IsEmpty())
                    {
                        for (int j = 0; j < patches[i]._matrices.Count; j++)
                        {
                            float offsetPositionX = patches[i].m03[j] + offset.x;
                            float offsetPositionY = patches[i].m13[j] + offset.y;
                            float offsetPositionZ = patches[i].m23[j] + offset.z;
                            patches[i].m03[j] = offsetPositionX;
                            patches[i].m13[j] = offsetPositionY;
                            patches[i].m23[j] = offsetPositionZ;
                        }
                    }

                    patches[i].Deserialize();
                }
            }

            private bool IsEmpty()
            {
                if (_matrices == null || _matrices.Count == 0)
                    Deserialize();

                if (_matrices == null || _matrices.Count == 0)
                    return true;
                else
                    return false;
            }

            private Matrix4x4 GetMatrix(int index)
            {
                Matrix4x4 matrix = new Matrix4x4();
                matrix.m00 = m00[index];
                matrix.m33 = m33[index];
                matrix.m23 = m23[index];
                matrix.m13 = m13[index];
                matrix.m03 = m03[index];
                matrix.m32 = m32[index];
                matrix.m22 = m22[index];
                matrix.m02 = m02[index];
                matrix.m12 = m12[index];
                matrix.m21 = m21[index];
                matrix.m11 = m11[index];
                matrix.m01 = m01[index];
                matrix.m30 = m30[index];
                matrix.m20 = m20[index];
                matrix.m10 = m10[index];
                matrix.m31 = m31[index];
                return matrix;
            }

            private List<Matrix4x4> GetMatrices()
            {
                if (_matrices == null) _matrices = new List<Matrix4x4>(); else _matrices.Clear();
                for (int i = 0; i < m00.Length; i++) _matrices.Add(GetMatrix(i));
                return _matrices;
            }

            private void FillMatrixDataFromList(List<Matrix4x4> matrices)
            {
                m00 = new float[matrices.Count];
                m33 = new float[matrices.Count];
                m23 = new float[matrices.Count];
                m13 = new float[matrices.Count];
                m03 = new float[matrices.Count];
                m32 = new float[matrices.Count];
                m22 = new float[matrices.Count];
                m02 = new float[matrices.Count];
                m12 = new float[matrices.Count];
                m21 = new float[matrices.Count];
                m11 = new float[matrices.Count];
                m01 = new float[matrices.Count];
                m30 = new float[matrices.Count];
                m20 = new float[matrices.Count];
                m10 = new float[matrices.Count];
                m31 = new float[matrices.Count];

                for (int i = 0; i < matrices.Count; i++)
                {
                    m00[i] = matrices[i].m00;
                    m33[i] = matrices[i].m33;
                    m23[i] = matrices[i].m23;
                    m13[i] = matrices[i].m13;
                    m03[i] = matrices[i].m03;
                    m32[i] = matrices[i].m32;
                    m22[i] = matrices[i].m22;
                    m02[i] = matrices[i].m02;
                    m12[i] = matrices[i].m12;
                    m21[i] = matrices[i].m21;
                    m11[i] = matrices[i].m11;
                    m01[i] = matrices[i].m01;
                    m30[i] = matrices[i].m30;
                    m20[i] = matrices[i].m20;
                    m10[i] = matrices[i].m10;
                    m31[i] = matrices[i].m31;
                }
            }

            private void RemoveMatrices(List<int> indices)
            {
                for (int removeIndex = 0; removeIndex < indices.Count; removeIndex++)
                {
                    m00 = m00.Where((source, index) => index != removeIndex).ToArray();
                    m33 = m33.Where((source, index) => index != removeIndex).ToArray();
                    m23 = m23.Where((source, index) => index != removeIndex).ToArray();
                    m13 = m13.Where((source, index) => index != removeIndex).ToArray();
                    m03 = m03.Where((source, index) => index != removeIndex).ToArray();
                    m32 = m32.Where((source, index) => index != removeIndex).ToArray();
                    m22 = m22.Where((source, index) => index != removeIndex).ToArray();
                    m02 = m02.Where((source, index) => index != removeIndex).ToArray();
                    m12 = m12.Where((source, index) => index != removeIndex).ToArray();
                    m21 = m21.Where((source, index) => index != removeIndex).ToArray();
                    m11 = m11.Where((source, index) => index != removeIndex).ToArray();
                    m01 = m01.Where((source, index) => index != removeIndex).ToArray();
                    m30 = m30.Where((source, index) => index != removeIndex).ToArray();
                    m20 = m20.Where((source, index) => index != removeIndex).ToArray();
                    m10 = m10.Where((source, index) => index != removeIndex).ToArray();
                    m31 = m31.Where((source, index) => index != removeIndex).ToArray();
                }
            }
        }

        // Old Patch struct which is replaced by PatchData struct for faster custom serialization/deserialization
        [Serializable]
        public struct Patch
        {
            public Vector3 position;
            public float scale;
            public List<Matrix4x4> matrices;
        }

        [HideInInspector, NonSerialized] public PatchData[] patchData = null; // New patch system with fast binary read/write of layer's data
        public UnityEngine.Object patchDataFile = null; // Patch Data file reference which is needed to be included in builds
        [HideInInspector] public string patchDataPath;
        public bool patchDataIsSaved = false;
        public bool patchDataIsLoaded = false;
        //private Thread loadingThread;

        [Serializable]
        public struct MaskDataFast
        {
            public float[] row;
        }

        // Old Mask struct which is replaced by MaskDataFast struct for faster custom serialization/deserialization
        [Serializable]
        public struct MaskData
        {
            [SerializeField] public float[] row;
        }

        [NonSerialized, HideInInspector] public MaskDataFast[] maskDataFast = null; // New Mask Data system with fast binary read/write of layer's data
        public UnityEngine.Object maskDataFile = null; // Mask Data file reference which is needed to be included in builds
        public string maskDataPath;
        public bool maskDataIsSaved = false;
        public bool maskDataIsLoaded = false;

        public void SerializeMask()
        {
            using (FileStream fs = new FileStream(maskDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                try
                {
                    BinaryFormatter writer = new BinaryFormatter();
                    writer.Serialize(fs, maskDataFast);
                }
                catch (Exception e) { throw e; }
                finally
                {
                    if (fs != null) fs.Close();
                    maskDataIsSaved = true;
                }
            }
        }

        public void DeserializeMask()
        {
            using (FileStream fs = new FileStream(maskDataPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                try
                {
                    BinaryFormatter reader = new BinaryFormatter();
                    maskDataFast = (MaskDataFast[])reader.Deserialize(fs);
                }
                catch (Exception e) { throw e; }
                finally
                {
                    if (fs != null) fs.Close();
                    maskDataIsLoaded = true;
                }
            }
        }

        protected void ConvertMaskFromTexture2D()
        {
            if (maskDataFast == null && filter != null)
            {
                int maskResolution = filter.width;
                maskDataFast = new MaskDataFast[maskResolution];

                for (int i = 0; i < maskResolution; i++)
                {
                    maskDataFast[i].row = new float[maskResolution];

                    for (int j = 0; j < maskResolution; j++)
                        maskDataFast[i].row[j] = filter.GetPixel(i, j).a;
                }
            }
        }

        protected bool CheckMask()
        {
            try
            {
                if (maskDataFast == null)
                    throw new Exception("Mask is missing for " + gameObject.name + " layer! Aborting placement.");

                return true;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.Log(e);
#endif
                return false;
            }
        }

        // Save from temporary patch data
        //PatchData[] PD = new PatchData[_patches.Length];
        //
        //for (int i = 0; i < _patches.Length; i++)
        //{
        //    PD[i] = new PatchData();
        //
        //    // Patch Position
        //    PD[i].x = _patches[i].position.x;
        //    PD[i].y = _patches[i].position.y;
        //    PD[i].z = _patches[i].position.z;
        //    
        //    // Scale
        //    PD[i].scale = _patches[i].scale;
        //
        //    // Matrix Data
        //    PD[i].m00 = new float[_patches[i].matrices.Count];
        //    PD[i].m33 = new float[_patches[i].matrices.Count];
        //    PD[i].m23 = new float[_patches[i].matrices.Count];
        //    PD[i].m13 = new float[_patches[i].matrices.Count];
        //    PD[i].m03 = new float[_patches[i].matrices.Count];
        //    PD[i].m32 = new float[_patches[i].matrices.Count];
        //    PD[i].m22 = new float[_patches[i].matrices.Count];
        //    PD[i].m02 = new float[_patches[i].matrices.Count];
        //    PD[i].m12 = new float[_patches[i].matrices.Count];
        //    PD[i].m21 = new float[_patches[i].matrices.Count];
        //    PD[i].m11 = new float[_patches[i].matrices.Count];
        //    PD[i].m01 = new float[_patches[i].matrices.Count];
        //    PD[i].m30 = new float[_patches[i].matrices.Count];
        //    PD[i].m20 = new float[_patches[i].matrices.Count];
        //    PD[i].m10 = new float[_patches[i].matrices.Count];
        //    PD[i].m31 = new float[_patches[i].matrices.Count];
        //
        //    for (int j = 0; j < _patches[i].matrices.Count; j++)
        //    {
        //        PD[i].m00[j] = _patches[i].matrices[j].m00;
        //        PD[i].m33[j] = _patches[i].matrices[j].m33;
        //        PD[i].m23[j] = _patches[i].matrices[j].m23;
        //        PD[i].m13[j] = _patches[i].matrices[j].m13;
        //        PD[i].m03[j] = _patches[i].matrices[j].m03;
        //        PD[i].m32[j] = _patches[i].matrices[j].m32;
        //        PD[i].m22[j] = _patches[i].matrices[j].m22;
        //        PD[i].m02[j] = _patches[i].matrices[j].m02;
        //        PD[i].m12[j] = _patches[i].matrices[j].m12;
        //        PD[i].m21[j] = _patches[i].matrices[j].m21;
        //        PD[i].m11[j] = _patches[i].matrices[j].m11;
        //        PD[i].m01[j] = _patches[i].matrices[j].m01;
        //        PD[i].m30[j] = _patches[i].matrices[j].m30;
        //        PD[i].m20[j] = _patches[i].matrices[j].m20;
        //        PD[i].m10[j] = _patches[i].matrices[j].m10;
        //        PD[i].m31[j] = _patches[i].matrices[j].m31;
        //    }
        //}

        // Load from temporary patch data
        //_patches = new Patch[PD.Length];
        //
        //for (int i = 0; i < PD.Length; i++)
        //{
        //    _patches[i] = new Patch();
        //    _patches[i].position = new Vector3(PD[i].x, PD[i].y, PD[i].z);
        //    _patches[i].scale = PD[i].scale;
        //    _patches[i].matrices = new List<Matrix4x4>();
        //
        //    for (int j = 0; j < PD[i].m00.Length; j++)
        //    {
        //        Matrix4x4 matrix = new Matrix4x4();
        //        matrix.m00 = PD[i].m00[j];
        //        matrix.m33 = PD[i].m33[j];
        //        matrix.m23 = PD[i].m23[j];
        //        matrix.m13 = PD[i].m13[j];
        //        matrix.m03 = PD[i].m03[j];
        //        matrix.m32 = PD[i].m32[j];
        //        matrix.m22 = PD[i].m22[j];
        //        matrix.m02 = PD[i].m02[j];
        //        matrix.m12 = PD[i].m12[j];
        //        matrix.m21 = PD[i].m21[j];
        //        matrix.m11 = PD[i].m11[j];
        //        matrix.m01 = PD[i].m01[j];
        //        matrix.m30 = PD[i].m30[j];
        //        matrix.m20 = PD[i].m20[j];
        //        matrix.m10 = PD[i].m10[j];
        //        matrix.m31 = PD[i].m31[j];
        //        _patches[i].matrices.Add(matrix);
        //    }
        //}
    }
}

