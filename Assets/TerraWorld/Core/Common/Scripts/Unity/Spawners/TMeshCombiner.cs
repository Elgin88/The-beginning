using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace TerraUnity.Runtime
{
    public class TMeshCombiner : MonoBehaviour
    {
        public string meshSaveName = "Combined Mesh";

        public void CombineMeshes(GameObject models, bool clearAfterCombining, bool saveMesh = false)
        {
            if (models == null) return;
            ArrayList materials = new ArrayList();
            ArrayList combineInstanceArrays = new ArrayList();
            MeshFilter[] meshFilters = models.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter mf in meshFilters)
            {
                MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();

                if (!meshRenderer || !mf.sharedMesh || meshRenderer.sharedMaterials.Length != mf.sharedMesh.subMeshCount)
                    continue;

                for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);

                    if (materialArrayIndex == -1)
                    {
                        materials.Add(meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = materials.Count - 1;
                    }

                    combineInstanceArrays.Add(new ArrayList());

                    CombineInstance combineInstance = new CombineInstance();

                    // Offset vertex positions based on original patch position
                    Matrix4x4 matrix = meshRenderer.transform.localToWorldMatrix;
                    combineInstance.transform = Matrix4x4.TRS(TMatrix.ExtractTranslationFromMatrix(ref matrix) - models.transform.position, TMatrix.ExtractRotationFromMatrix(ref matrix), TMatrix.ExtractScaleFromMatrix(ref matrix));

                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = mf.sharedMesh;
                    (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
                }
            }

            // Create mesh filter & renderer
            MeshFilter meshFilterCombine = models.GetComponent<MeshFilter>();

            if (meshFilterCombine == null)
                meshFilterCombine = models.AddComponent<MeshFilter>();

            MeshRenderer meshRendererCombine = models.GetComponent<MeshRenderer>();

            if (meshRendererCombine == null)
                meshRendererCombine = models.AddComponent<MeshRenderer>();

            // Combine by material index into per-material meshes
            // also, Create CombineInstance array for next step
            Mesh[] meshes = new Mesh[materials.Count];
            CombineInstance[] combineInstances = new CombineInstance[materials.Count];

            for (int m = 0; m < materials.Count; m++)
            {
                CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
                meshes[m] = new Mesh();
                meshes[m].CombineMeshes(combineInstanceArray, true, true);
                combineInstances[m] = new CombineInstance();
                combineInstances[m].mesh = meshes[m];
                combineInstances[m].subMeshIndex = 0;
            }

            // Combine into one
            meshFilterCombine.sharedMesh = new Mesh();
            meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

            // Assign materials
            Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
            meshRendererCombine.materials = materialsArray;

            if (clearAfterCombining)
                Clear(models);

#if UNITY_EDITOR
            if (saveMesh)
                SaveMesh();
#endif
        }

        private void Clear(GameObject models)
        {
            if (models == null) return;
            Transform[] objects = models.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in objects)
            {
                if (t == null) continue;

                if (t.GetComponent<TMeshCombiner>() == null)
                    DestroyImmediate(t.gameObject);
            }
        }

        private int Contains(ArrayList searchList, string searchName)
        {
            for (int i = 0; i < searchList.Count; i++)
                if (((Material)searchList[i]).name == searchName)
                    return i;

            return -1;
        }

#if UNITY_EDITOR
        private void SaveMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            string folderPath = Application.dataPath;
            bool meshIsSaved = AssetDatabase.Contains(mesh);

            if (!meshIsSaved && !AssetDatabase.IsValidFolder("Assets/" + folderPath))
            {
                string[] folderNames = folderPath.Split('/');
                folderNames = folderNames.Where((folderName) => !folderName.Equals("")).ToArray();
                folderNames = folderNames.Where((folderName) => !folderName.Equals(" ")).ToArray();
                folderPath = "/"; // Reset folder path.

                for (int i = 0; i < folderNames.Length; i++)
                {
                    folderNames[i] = folderNames[i].Trim();

                    if (!AssetDatabase.IsValidFolder("Assets" + folderPath + folderNames[i]))
                    {
                        string folderPathWithoutSlash = folderPath.Substring(0, folderPath.Length - 1); // Delete last "/" character.
                        AssetDatabase.CreateFolder("Assets" + folderPathWithoutSlash, folderNames[i]);
                    }

                    folderPath += folderNames[i] + "/";
                }

                folderPath = folderPath.Substring(1, folderPath.Length - 2); // Delete first and last "/" character.
            }

            if (!meshIsSaved)
            {
                string meshPath = "Assets/" + folderPath + "/" + meshSaveName + ".asset";
                int assetNumber = 1;
                Debug.Log(meshPath);

                while (AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) != null) // If Mesh with same name exists, change name.
                {
                    meshPath = "Assets/" + folderPath + "/" + meshSaveName + "_" + assetNumber + ".asset";
                    assetNumber++;
                }

                AssetDatabase.CreateAsset(mesh, meshPath);
                AssetDatabase.SaveAssets();
                ClearLog();
                Debug.Log("<color=#ff9900><b>Mesh \"" + mesh.name + "\" was saved in the \"" + folderPath + "\" folder.</b></color>"); // Show info about saved mesh.
            }
        }

        public void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
#endif
    }
}

