using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Mewlist.MassiveGrass;
using static TerraUnity.Runtime.TScatterLayer;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GrassLayer : WorldToolsParams
    {
        public MassiveGrass massiveGrass { get => transform.parent.GetComponent<MassiveGrass>(); }
        public MassiveGrassProfile MGP;
        public string maskDataPath;
        public string maskDataBuildPath;

        private bool loadMaskOnThreads = false;
        //private bool maskDataIsSaved = false;
        //private bool maskDataIsLoaded = false;
        private Terrain _terrain = null;

        public void UpdateLayer(MassiveGrassProfile filter = null, Vector2 mousePos = new Vector2(), bool updateMaskData = true)
        {
#if UNITY_EDITOR
            if (filter != null && mousePos == Vector2.zero)
            {
                filter = null;
                Debug.Log("No Mouse input detected for grass layer update! Updating all grass layers!");
            }

            if (updateMaskData)
                SaveMaskData();
            else
            {
                if (filter != null)
                    massiveGrass.Refresh(filter, mousePos);
                else
                    LoadMaskData();
            }
#endif
        }

        private void OnEnable()
        {
            if (MGP == null) return;
            MGP.isActive = true;
            massiveGrass.RegenerateLayer();
        }

        private void OnDisable()
        {
            if (MGP == null) return;
            MGP.isActive = false;
            massiveGrass.RegenerateLayer();
        }

        private void Start()
        {
            if (Application.isPlaying)
                LoadMaskData(true);
        }

        private Terrain GetTerrain()
        {
            Transform parent = transform;

            while (parent != null)
            {
                if (parent.GetComponent<Terrain>() != null) return parent.GetComponent<Terrain>();
                parent = parent.transform.parent;
            }

            return null;
        }

        public Terrain Terrain
        {
            get
            {
                if (_terrain == null) _terrain = GetTerrain();
                return _terrain;
            }
        }

        public void SaveMaskData(List<int> indices = null, bool showProgress = false, MassiveGrassProfile filter = null, Vector2 mousePos = new Vector2())
        {
            if (Application.isPlaying) return;
            //maskDataIsSaved = false;

#if UNITY_EDITOR && TERRAWORLD_PRO
            maskDataPath = Path.GetFullPath(TTerraWorldManager.WorkDirectoryLocalPath + this.gameObject.name + ".maskdata");
#endif

            if (MGP.maskDataFast != null && MGP.maskDataFast.Length > 0)
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
                    AssetDatabase.Refresh();
                    MGP.maskDataFile = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(maskDataPath), typeof(UnityEngine.Object));
                    LoadMaskData(true, indices, showProgress, filter, mousePos);
                }
#endif
            }
            else
                Debug.Log("Mask Data for layer: " + this.gameObject.name + " is empty!");
        }

        public void LoadMaskData(bool forced = false, List<int> indices = null, bool showProgress = false, MassiveGrassProfile filter = null, Vector2 mousePos = new Vector2(), bool updateLayer = true)
        {
            if (!forced && MGP.maskDataFast != null) return;
            //maskDataIsLoaded = false;

            // Port old version's mask data into our main MaskData struct
            if (MGP.maskData != null && MGP.maskData.Length > 0)
            {
                int maskResolution = MGP.maskData.Length;
                MGP.maskDataFast = new MaskDataFast[maskResolution];

                for (int i = 0; i < maskResolution; i++)
                {
                    MGP.maskDataFast[i].row = new float[maskResolution];

                    for (int j = 0; j < maskResolution; j++)
                        MGP.maskDataFast[i].row[j] = MGP.maskData[i].row[j];
                }

                //maskDataIsLoaded = true;
            }
            else
            {
#if UNITY_EDITOR
                if (MGP.maskDataFile != null) maskDataPath = Path.GetFullPath(AssetDatabase.GetAssetPath(MGP.maskDataFile));
#else
                if (massiveGrass.externalMaskDataFiles)
                {
                    if
                    (
                        Application.platform == RuntimePlatform.WindowsPlayer ||
                        Application.platform == RuntimePlatform.OSXPlayer ||
                        Application.platform == RuntimePlatform.LinuxPlayer
                    )
                        maskDataPath = Path.Combine(Application.dataPath, maskDataBuildPath);
                    else
                        maskDataPath = Path.Combine(Application.persistentDataPath, maskDataBuildPath);
                }
                else
                    maskDataPath = Path.Combine(Application.streamingAssetsPath, this.gameObject.name + ".maskdata");
#endif

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

                //if (File.Exists(maskDataPath))
                {
                    MGP.maskDataFast = null;

                    if (loadMaskOnThreads)
                    {
                        Thread thread = new Thread(new ThreadStart(DeserializeMask));
                        thread.Start();
                        thread.Join();
                    }
                    else
                        DeserializeMask();

                    if (updateLayer) massiveGrass.RegenerateLayer();
                }
                //else
                //Debug.Log("Mask Data file for layer: " + this.gameObject.name + " not found!");
            }
        }

        public void SerializeMask()
        {
            using (FileStream fs = new FileStream(maskDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                try
                {
                    BinaryFormatter writer = new BinaryFormatter();
                    writer.Serialize(fs, MGP.maskDataFast);
                }
                catch (Exception e) { throw e; }
                finally
                {
                    if (fs != null) fs.Close();
                    //maskDataIsSaved = true;
                }
            }
        }

        public void DeserializeMask()
        {
            Uri uri = new Uri(maskDataPath);

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
                            MGP.maskDataFast = (MaskDataFast[])reader.Deserialize(ms);
                        }
                        catch (Exception e) { throw e; }
                        finally
                        {
                            //maskDataIsLoaded = true;
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
                        MGP.maskDataFast = (MaskDataFast[])reader.Deserialize(ms);
                    }
                    catch (Exception e) { throw e; }
                    finally
                    {
                        //maskDataIsLoaded = true;
                    }
                }
#endif
            }
        }

#if UNITY_EDITOR
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            AssetDatabase.Refresh();

            if (BuildProcessorGRASS.externalMaskDataFiles)
            {
                for (int i = 0; i < BuildProcessorGRASS.maskPathsEditor.Count; i++)
                {
                    if (!Directory.Exists(BuildProcessorGRASS.TerraWorldBuildPath)) Directory.CreateDirectory(BuildProcessorGRASS.TerraWorldBuildPath);
                    if (File.Exists(BuildProcessorGRASS.maskPathsBuild[i])) File.Delete(BuildProcessorGRASS.maskPathsBuild[i]);
                    File.Copy(BuildProcessorGRASS.maskPathsEditor[i], BuildProcessorGRASS.maskPathsBuild[i], true);
                }

                Debug.Log("Copied all grass layers's data to build path @: " + BuildProcessorGRASS.TerraWorldBuildPath);
            }
            else
            {
                string streamingAssetsPath = Path.GetFullPath(Application.dataPath + "/StreamingAssets");
                if (!Directory.Exists(streamingAssetsPath)) return;
                string[] maskDataPaths = Directory.GetFiles(streamingAssetsPath, "*.maskdata", SearchOption.AllDirectories);
                if (maskDataPaths == null || maskDataPaths.Length == 0) return;
                foreach (string s in maskDataPaths) File.Delete(s);
                AssetDatabase.Refresh();

                Debug.Log("All grass layers's data have been embedded into build's internal data.");
            }
        }
#endif
    }

#if UNITY_EDITOR
    public class BuildProcessorGRASS : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public static string TerraWorldBuildPath;
        public static List<string> maskPathsEditor;
        public static List<string> maskPathsBuild;
        public static bool externalMaskDataFiles;

        public void OnPreprocessBuild(BuildReport report)
        {
            MassiveGrass[] massiveGrassInstances = MonoBehaviour.FindObjectsOfType<MassiveGrass>();
            MassiveGrass massiveGrass = null;
            if (massiveGrassInstances != null && massiveGrassInstances.Length > 0 && massiveGrassInstances[0] != null) massiveGrass = massiveGrassInstances[0];
            if (massiveGrass == null) return;
            externalMaskDataFiles = massiveGrass.externalMaskDataFiles;

            if (massiveGrass.externalMaskDataFiles)
            {
                maskPathsEditor = new List<string>();
                maskPathsBuild = new List<string>();
                string prefixPath;
                string[] paths = new string[3];

                if
                (
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX ||
#pragma warning disable CS0618 // Type or member is obsolete
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ||
#pragma warning restore CS0618 // Type or member is obsolete
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64 ||
#pragma warning disable CS0618 // Type or member is obsolete
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinuxUniversal
#pragma warning restore CS0618 // Type or member is obsolete
                )
                    prefixPath = Path.Combine(Path.GetDirectoryName(report.summary.outputPath), Path.GetFileNameWithoutExtension(report.summary.outputPath) + "_Data");
                else
                    prefixPath = Application.persistentDataPath;

                paths[0] = "TerraWorld";
                paths[1] = "World Data";
                if (SceneManager.GetActiveScene() != null) paths[2] = SceneManager.GetActiveScene().name;
                else paths[2] = "Scene";

                TerraWorldBuildPath = Path.Combine(prefixPath, paths[0], paths[1], paths[2]);
                string TerraWorldResourcePath = Path.Combine(paths);

                GrassLayer[] GrassLayers = MonoBehaviour.FindObjectsOfType<GrassLayer>();

                if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                    foreach (GrassLayer g in GrassLayers)
                    {
                        g.maskDataBuildPath = Path.Combine(TerraWorldResourcePath, g.gameObject.name + ".maskdata");
                        if (g.MGP.maskDataFile == null) continue;
                        maskPathsEditor.Add(g.maskDataPath);

                        // If we need to reference path from the mask data file itself, we get it as follows:
                        //maskPathsEditor.Add(Path.GetFullPath(AssetDatabase.GetAssetPath(g.MGP.maskDataFile)));

                        maskPathsBuild.Add(Path.Combine(TerraWorldBuildPath, g.gameObject.name + ".maskdata"));
                    }
            }
            else
            {
                string streamingAssetsPath = Path.GetFullPath(Application.dataPath + "/StreamingAssets");

                if (!Directory.Exists(streamingAssetsPath))
                    Directory.CreateDirectory(streamingAssetsPath);
                else
                {
                    string[] maskDataPaths = Directory.GetFiles(streamingAssetsPath, "*.maskdata", SearchOption.AllDirectories);
                    foreach (string s in maskDataPaths) File.Delete(s);
                }

                AssetDatabase.Refresh();
                GrassLayer[] GrassLayers = MonoBehaviour.FindObjectsOfType<GrassLayer>();

                if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                    foreach (GrassLayer g in GrassLayers)
                    {
                        string streamingAssetsMaskDataPath = Path.Combine(streamingAssetsPath, g.gameObject.name + ".maskdata");
                        File.Copy(g.maskDataPath, streamingAssetsMaskDataPath, true);
                    }
            }
        }
    }
#endif
}

