using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TerraUnity.Edittime;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
using XNodeEditor;
using System.Collections.Generic;
#endif

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class TTerraWorldManager : MonoBehaviour
    {
#pragma warning disable CS0414 // Add readonly modifier

#if !TERRAWORLD_DEBUG
        [HideInInspector]
#endif
        //string _workDirectoryLocalPath1 = "";
        public static bool isQuitting = false;
#pragma warning restore CS0414 // Add readonly modifier

        public static TTerraWorldManager TerraWorldManagerScript { get => GetTerraWorldManager(); }
        private static TTerraWorldManager _terraWorldManagerScript;

        public static GameObject SceneSettingsGO1 { get => GetSceneSettingsGameObject(); }
        private static GameObject _sceneSettingsGO;

        public static TTerraWorldTerrainManager TerrainParamsScript { get => GetTerrainParams(); }
        private static TTerraWorldTerrainManager _terrainParamsScript;

        public static GameObject IsMainTerraworldGameObject { get => GetMainTerraworldGameObject(); }
        public static GameObject CreateAndGetTerraworldGameObject { get => CreateTerraworldGameObject(); }
        private static GameObject _mainTerraworldGameObject;

        public static GameObject MainTerrainGO { get => GetMainTerrainGameObject(); }
        private static GameObject _mainTerrainGO;

        public static Terrain MainTerrain { get => GetMainTerrain(); }
        private static Terrain _mainTerrain;

        public static GameObject BackgroundTerrainGO { get => GetBackgroundTerrainGameObject(); }
        private static GameObject _backgroundTerrainGO;

        public static Terrain BackgroundTerrain { get => GetBackgroundTerrain(); }
        private static Terrain _backgroundTerrain;


        private static string defaultMainTerraworldGameObjectName = "TerraWorld";

        public static GameObject Sun { get => GetSun(); }
        private static GameObject _sun;

        private static GameObject GetSun()
        {
            if (_sun == null)
            {
                List<GameObject> rootObjects = new List<GameObject>();
                Scene scene = SceneManager.GetActiveScene();
                if (scene == null || !scene.isLoaded) return _sun;
                scene.GetRootGameObjects(rootObjects);

                for (int i = 0; i < rootObjects.Count; ++i)
                {
                    GameObject root = rootObjects[i];

                    foreach (Transform t in root.GetComponentsInChildren(typeof(Transform), true))
                        if (t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                            if (!t.name.Equals("Moon") && t.GetComponent<Light>() != null && t.GetComponent<Light>().type == LightType.Directional)
                            {
                                _sun = t.gameObject;
                                break;
                            }
                }

                //Disable Mie Scattering on Atmospheric Fog to avoid extra sun halo
                //if (_sun != null)
                //    if (_sun.GetComponent<AtmosphericScatteringSun>() != null)
                //        _sun.GetComponent<AtmosphericScatteringSun>().enabled = false;
            }

            if (_sun == null)
                throw new Exception("Sun (Directional Light) not found!");

            return _sun;
        }

        public static GameObject Moon { get => GetMoon(); }
        private static GameObject _moon;

        private static GameObject GetMoon()
        {
            if (_moon == null)
            {
                foreach (Transform t in SceneSettingsGO1.GetComponentsInChildren(typeof(Transform), true))
                    if (t.name.Equals("Moon"))
                    {
                        _moon = t.gameObject;
                        break;
                    }

                if (_moon == null)
                {
                    _moon = Instantiate(Sun);
                    _moon.name = "Moon";
                    _moon.transform.parent = SceneSettingsGO1.transform;

                    //if (_moon.GetComponent<AtmosphericScatteringSun>() != null)
                    //    _moon.GetComponent<AtmosphericScatteringSun>().enabled = false;
                }
            }

            if (_moon == null)
                throw new Exception("Moon not found!");

            return _moon;
        }

        public static Light SunLight { get => GetSunLight(); }
        private static Light _sunLight;

        private static Light GetSunLight()
        {
            if (_sunLight == null)
                _sunLight = Sun.GetComponent<Light>();

            return _sunLight;
        }

        public static Light MoonLight { get => GetMoonLight(); }
        private static Light _moonLight;

        private static Light GetMoonLight()
        {
            if (_moonLight == null)
                _moonLight = Moon.GetComponent<Light>();

            //if (_moonLight != null) _moonLight.shadows = LightShadows.None;

            return _moonLight;
        }

        private static GameObject CreateTerraworldGameObject()
        {
#if UNITY_EDITOR
            if (IsMainTerraworldGameObject == null)
            {
                _mainTerraworldGameObject = new GameObject(defaultMainTerraworldGameObjectName);
                _terraWorldManagerScript = _mainTerraworldGameObject.AddComponent<TTerraWorldManager>();
                _mainTerraworldGameObject.AddComponent<WorldTools>();
            }
#endif
            return _mainTerraworldGameObject;
        }

        private static GameObject GetMainTerraworldGameObject()
        {
            if (_mainTerraworldGameObject == null)
            {
                _mainTerraworldGameObject = GameObject.Find(defaultMainTerraworldGameObjectName);
                if (_mainTerraworldGameObject != null && _mainTerraworldGameObject.GetComponent<TTerraWorldManager>() == null) _mainTerraworldGameObject = null;

#if UNITY_EDITOR
                if (_mainTerraworldGameObject == null)
                {
                    List<GameObject> rootObjects = new List<GameObject>();
                    Scene scene = SceneManager.GetActiveScene();
                    if (!scene.isLoaded) return null;
                    scene.GetRootGameObjects(rootObjects);

                    for (int i = 0; i < rootObjects.Count; ++i)
                    {
                        GameObject root = rootObjects[i];

                        foreach (Transform t in root.GetComponentsInChildren(typeof(Transform), true))
                            if (t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                                if (t.GetComponent<WorldTools>() != null || t.GetComponent<TTerraWorldManager>() != null)
                                {
                                    _mainTerraworldGameObject = t.gameObject;
                                    break;
                                }
                    }
                }
#endif
            }

            return _mainTerraworldGameObject;
        }

        private static TTerraWorldManager GetTerraWorldManager()
        {
            if (_terraWorldManagerScript == null)
            {
                _terraWorldManagerScript = CreateAndGetTerraworldGameObject.GetComponent<TTerraWorldManager>();

                if (_terraWorldManagerScript == null)
                    _terraWorldManagerScript = CreateAndGetTerraworldGameObject.AddComponent<TTerraWorldManager>();
            }

            return _terraWorldManagerScript;
        }

        private static GameObject GetMainTerrainGameObject()
        {
            if (IsMainTerraworldGameObject == null) return null;
            if (_mainTerrainGO == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.GetComponent<TTerraWorldTerrainManager>() != null)
                    {
                        _mainTerrainGO = t.gameObject;
                        break;
                    }
                }
            }

            return _mainTerrainGO;
        }

        private static Terrain GetMainTerrain()
        {
            if (IsMainTerraworldGameObject == null) return null;
            if (_mainTerrain == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.GetComponent<TTerraWorldTerrainManager>() != null)
                    {
                        _mainTerrain = terrain;
                        break;
                    }
                }
            }

            return _mainTerrain;
        }

        private static GameObject GetBackgroundTerrainGameObject()
        {
            if (MainTerrainGO == null) return null;
            if (_backgroundTerrainGO == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.gameObject.name == "Background Terrain")
                    {
                        _backgroundTerrainGO = t.gameObject;
                        break;
                    }
                }
            }

            return _backgroundTerrainGO;
        }

        private static Terrain GetBackgroundTerrain()
        {
            if (GetBackgroundTerrainGameObject() == null) return null;
            if (_backgroundTerrain == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.gameObject.name == "Background Terrain")
                    {
                        _backgroundTerrain = terrain;
                        break;
                    }
                }
            }

            return _backgroundTerrain;
        }

        private static TTerraWorldTerrainManager GetTerrainParams()
        {
            if (MainTerrainGO == null) return null;

            if (_terrainParamsScript == null)
            {
                _terrainParamsScript = MainTerrainGO.GetComponent<TTerraWorldTerrainManager>();

                if (_terrainParamsScript == null)
                    _terrainParamsScript = MainTerrainGO.AddComponent<TTerraWorldTerrainManager>();
            }

            return _terrainParamsScript;
        }

        public static bool IsTerrainAvailable()
        {
            if (MainTerrainGO != null && MainTerrain != null)
                return true;
            else
                return false;
        }

#if UNITY_EDITOR
#if TERRAWORLD_PRO
        public static string WorkDirectoryLocalPath { get => getWorkDirectoryLocalPath(); }

        private static string getWorkDirectoryLocalPath()
        {
            string path = TerraWorldManagerScript.GetWorkingDirectoryLocalName();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private string GetWorkingDirectoryLocalName()
        {
            if (string.IsNullOrEmpty(TProjectSettings.LastProjectDirPath))
                TProjectSettings.LastProjectDirPath = TAddresses.GetNewWorkDirectoryPath();

            if (!Directory.Exists(TProjectSettings.LastProjectDirPath))
                Directory.CreateDirectory(TProjectSettings.LastProjectDirPath);

            return TProjectSettings.LastProjectDirPath;
        }
#endif
#endif

#if UNITY_EDITOR
#if TERRAWORLD_PRO
        private void OnEnable()
        {
            //TODO: Will be removed after organizing classes!
            EditorApplication.quitting += Quit;
        }
#endif
#endif

#if UNITY_EDITOR
#if TERRAWORLD_XPRO
#if !TERRAWORLD_DEBUG
        [HideInInspector]
#endif
        public TXGraph xGraphFile;
#endif

#if TERRAWORLD_PRO
#if !TERRAWORLD_DEBUG
        [HideInInspector]
#endif
        public UnityEngine.Object graphFile;
        private static int oldGraphFileHash = 0;
        private static TTerraWorldGraph _oldGraph = null;

        static void Quit()
        {
            isQuitting = true;
        }
#endif
#endif

#if UNITY_EDITOR
#if TERRAWORLD_PRO
        public static TTerraWorldGraph WorldGraph { get => GetOldGraph(); }

        private static TTerraWorldGraph GetOldGraph()
        {
            if (_oldGraph != null)
            {
                if (TerraWorldManagerScript.graphFile == null) _oldGraph = null;
                else if (oldGraphFileHash != TerraWorldManagerScript.graphFile.GetHashCode()) _oldGraph = null;
            }

            if (_oldGraph == null)
            {
                ResetOldGraph();
                string savedPath = WorkDirectoryLocalPath + "graph.xml";

                try
                {
                    if (File.Exists(savedPath))
                    {
                        if (TTerraWorldGraph.CheckGraph(savedPath))
                        {
                            bool reGenerate = _oldGraph.LoadGraph(savedPath, false);
                        }
                        else
                            throw new Exception("Internal Error 277!");
                    }
                    else if (!string.IsNullOrEmpty(TerraWorldGraphPath))
                    {
                        string path = TerraWorldGraphPath;

                        if (TTerraWorldGraph.CheckGraph(TerraWorldGraphPath))
                        {
                            bool reGenerate = _oldGraph.LoadGraph(path, false);
                        }
                        else
                            throw new Exception("Graph file is corrupted!");
                    }
                    else if (File.Exists("Assets/TerraWorld/Core/Presets/Graph.xml"))
                    {
                        string path = TAddresses.projectPath + "Assets/TerraWorld/Core/Presets/Graph.xml";

                        if (TTerraWorldGraph.CheckGraph(path))
                        {
                            bool reGenerate = _oldGraph.LoadGraph(path, false);
                        }
                        else
                            throw new Exception("Internal Error 278!");
                    }

                    if (_oldGraph != null)
                        SaveOldGraph();

                }
                catch (Exception e)
                {
                    TDebug.LogErrorToUnityUI(e);
                }
            }

            return _oldGraph;
        }

        public static void ResetOldGraph()
        {
            //TAreaGraph OldareaGraphArea = null;

            //if (_oldGraph != null)
            //OldareaGraphArea = _oldGraph.areaGraph;

            _oldGraph = TTerraWorldGraph.GetNewWorldGraph(TVersionController.MajorVersion, TVersionController.MinorVersion);

            //if (OldareaGraphArea != null)
            //_oldGraph.areaGraph = OldareaGraphArea;
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            if (Directory.Exists(sourcePath))
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void DouplicateWorkDirectory()
        {
            string newDirPath = TAddresses.GetNewWorkDirectoryPath();
            if (!Directory.Exists(newDirPath)) Directory.CreateDirectory(newDirPath);
            if (File.Exists(TProjectSettings.LastProjectDirPath)) CopyFilesRecursively(TProjectSettings.LastProjectDirPath, newDirPath);
            AssetDatabase.Refresh();
            TProjectSettings.LastProjectDirPath = newDirPath;
            string newGraphPath = newDirPath + "graph.xml";
            if (File.Exists(newGraphPath))
            {
                _oldGraph.LoadGraph(newGraphPath, false);
                SceneSettingsManager.VFXData fXData = _oldGraph.VFXDATA;
                SceneSettingsManager.ReplaceRefrences(ref fXData);
                SaveOldGraph();
            }
        }

        public static void SaveOldGraph()
        {
            string savedPath = WorkDirectoryLocalPath + "graph.xml";
            if (_oldGraph == null)
                _oldGraph = GetOldGraph();

            _oldGraph.SaveGraph(savedPath);

            if (!Application.isPlaying && !EditorApplication.isPlaying)
                AssetDatabase.Refresh();

            _terraWorldManagerScript = null;
            _mainTerraworldGameObject = null;
            TerraWorldManagerScript.graphFile = AssetDatabase.LoadAssetAtPath(savedPath, typeof(UnityEngine.Object));

            if (TerraWorldManagerScript.graphFile != null)
                oldGraphFileHash = TerraWorldManagerScript.graphFile.GetHashCode();
        }

        public static string TerraWorldGraphPath { get => TerraWorldManagerScript.GraphFilePath; }

        public string GraphFilePath
        {
            get
            {
                if (graphFile != null)
                {
                    string path = TAddresses.projectPath + AssetDatabase.GetAssetPath(graphFile);
                    return path;
                }

                return null;
            }
        }
#endif

#if TERRAWORLD_XPRO
        public static TXGraph XGraph { get => GetXGraph(); } 

        private static TXGraph GetXGraph()
        {
            try
            {
                if (TerraWorldManagerScript.xGraphFile == null)
                {
                    string xPath = WorkDirectoryLocalPath + "xgraph.asset";
                    TerraWorldManagerScript.xGraphFile = AssetDatabase.LoadAssetAtPath(xPath, typeof(TXGraph)) as TXGraph;
                }
  
                if (TerraWorldManagerScript.xGraphFile == null) 
                    ResetXGraph();

                if (TerraWorldManagerScript.xGraphFile == null)
                    throw new Exception("Error in initializing  xgraph file");

                return TerraWorldManagerScript.xGraphFile;
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
                return null;
            }
        }

        public static void ResetXGraph()
        {
            TerraWorldManagerScript.xGraphFile = ScriptableObject.CreateInstance<TXGraph>();
            string savedPath = WorkDirectoryLocalPath + "xgraph.asset";
            if (File.Exists(savedPath)) AssetDatabase.DeleteAsset(savedPath);
            AssetDatabase.CreateAsset(TerraWorldManagerScript.xGraphFile, savedPath);
            AssetDatabase.Refresh();
        }

        public static void RefreshXGraph()
        {
            TerraWorldManagerScript.xGraphFile = null;
            AssetDatabase.Refresh();
        }

        public static string GetXGraphFilePath()
        {
             return TAddresses.projectPath + WorkDirectoryLocalPath + "xgraph.asset";
        }

        public static void ExportXPackage(string exportfileName)
        {
            List<string> assetPaths = new List<string>();

            if (TerraWorldManagerScript.xGraphFile != null)
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TerraWorldManagerScript.xGraphFile), TAddresses.GetTempDirectoryPath() + "xgraph.twgt");
                assetPaths.Add(TAddresses.GetTempDirectoryPath() + "xgraph.twgt");
            }

            if (TerraWorldManagerScript.graphFile != null)
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TerraWorldManagerScript.graphFile), TAddresses.GetTempDirectoryPath() + "graph.twgt");
                assetPaths.Add(TAddresses.GetTempDirectoryPath() + "graph.twgt");
            }

            if (assetPaths == null || assetPaths.Count < 2) throw new Exception("Internal Error 284");
            AssetDatabase.ExportPackage(assetPaths.ToArray(), exportfileName, ExportPackageOptions.Default);
        }

        public static void ImportXPackage(string importFileName)
        {
            if (Directory.Exists(TAddresses.GetTempDirectoryPath()))
                Directory.Delete(TAddresses.GetTempDirectoryPath(), true);

            AssetDatabase.Refresh();
            AssetDatabase.importPackageCompleted += AfterImportXPackage;
            AssetDatabase.ImportPackage(importFileName, false);
        }

        private static void AfterImportXPackage(string packageName)
        {
            AssetDatabase.importPackageCompleted -= AfterImportXPackage;

            if (TerraWorldManagerScript.graphFile != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(TerraWorldManagerScript.graphFile));
                TerraWorldManagerScript.graphFile = null;
            }

            string source = TAddresses.GetTempDirectoryPath() + "graph.twgt";
            string des = WorkDirectoryLocalPath + "graph.xml";
            AssetDatabase.CopyAsset(source, des);

            if (TerraWorldManagerScript.xGraphFile != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(TerraWorldManagerScript.xGraphFile));
                TerraWorldManagerScript.xGraphFile = null;
            }

            source = TAddresses.GetTempDirectoryPath() + "xgraph.twgt";
            des = WorkDirectoryLocalPath + "xgraph.asset";
            AssetDatabase.CopyAsset(source, des);

            TTerraworldGenerator.RunLoadedGraph(true);
        }
#endif
#endif

        private static GameObject GetSceneSettingsGameObject()
        {
            //if (_mainTerraworldGameObject == null) return null;
            if (IsMainTerraworldGameObject == null) return null;

            if (_sceneSettingsGO == null)
            {
                foreach (Transform t in _mainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                    if (t.GetComponent<SceneSettingsGameObjectManager>() != null)
                    {
                        _sceneSettingsGO = t.gameObject;
                        break;
                    }
            }

            if (_sceneSettingsGO == null)
            {
                Scene scene = SceneManager.GetActiveScene();

                if (scene.isLoaded)
                {
                    GameObject[] sceneObjects = scene.GetRootGameObjects();
                    //List<GameObject> sceneObjects = new List<GameObject>();
                    //scene.GetRootGameObjects(sceneObjects);

                    for (int i = 0; i < sceneObjects.Length; ++i)
                    {
                        if (!sceneObjects[i].activeSelf) continue;

                        foreach (Transform t in sceneObjects[i].GetComponentsInChildren(typeof(Transform), true))
                        {
                            if (t == null) continue;

                            if (t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                                if (t.GetComponent<SceneSettingsGameObjectManager>() != null)
                                {
                                    _sceneSettingsGO = t.gameObject;
                                    break;
                                }
                        }
                    }
                }
            }

            return _sceneSettingsGO;
        }
    }
}

