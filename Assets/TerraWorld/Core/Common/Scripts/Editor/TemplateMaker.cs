#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using TerraUnity.UI;


namespace TerraUnity.Edittime.UI
{
    public class TemplateMaker : EditorWindow
    {
        //[MenuItem("Tools/TerraUnity/Graph Package Maker", false, 3)]
        static void Init()
        {
            TemplateMaker window = (TemplateMaker)GetWindow(typeof(TemplateMaker));
            window.position = new Rect(5, 135, 480, 800);
            window.titleContent = new GUIContent("Graph Package Maker", "Graph Package Maker");
        }

        static string aboutText;
        static Object templateGraph;
        static string packageName = "My Graph";
        static List<string> assetPaths;
        static bool NoBug = false;
        static bool AutoReplace = false;
        //static string graphicsPath;
        //static string materialsPath;
        //static string texturesPath;
        //static string resourcesPathManager;

        private void OnEnable()
        {
            aboutText = "GRAPH EXPORTER" + "\n" +
                "Ver. " + "1.0";

            //#if TERRAWORLD_PRO
            //            graphicsPath = "Assets/TerraWorld/Core/Graphics/Pro";
            //            materialsPath = "Assets/TerraWorld/Core/Resources/Materials";
            //            texturesPath = "Assets/TerraWorld/Core/Resources/Textures";
            //#else
            //            graphicsPath = "Assets/TerraWorld/Core/Graphics/Lite";
            //            materialsPath = "Assets/TerraWorld/Core/Resources/Materials";
            //            texturesPath = "Assets/TerraWorld/Core/Resources/Textures";
            //#endif
            //            resourcesPathManager = "Assets/TerraWorld/Core/Common/Scripts/Sources/TResourcesManager.cs";
            assetPaths = new List<string>();
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedHeight = 40;
            EditorGUI.BeginChangeCheck();
            templateGraph = THelpersUI.GUI_ObjectField(new GUIContent("GRAPH", "Insert TerraWorld's graph file in xml or twg format"), templateGraph, null, null, 40);
            if (EditorGUI.EndChangeCheck()) CheckGraphValidity();
            packageName = THelpersUI.GUI_TextField(new GUIContent("PACKAGE NAME", "Type Package name for the export"), packageName);

            if (templateGraph != null)
            {
                GUILayout.Space(30);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("CHECK PACKAGE", ""), style))
                {
                    if (EditorUtility.DisplayDialog("TERRAWORLD", "Do you want to auto replace the missing files?", "Yes", "No"))
                    {
                        AutoReplace = true;
                    }
                    else
                        AutoReplace = false;

                    CheckTempelatePackage(templateGraph);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();


                if (NoBug)
                {
                    GUILayout.Space(30);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("GENERATE PACKAGE", "Generates unitypackage file from inserted TerraWorld graph"), style))
                    {
                        CreateTempelatePackage(templateGraph, packageName);
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }


            }

            THelpersUI.GUI_HelpBoxInfo(aboutText, 20);
        }

        private static void CheckGraphValidity()
        {
            NoBug = false;
            if (templateGraph == null) return;
            string extension = Path.GetExtension(AssetDatabase.GetAssetPath(templateGraph));

            if (extension != ".xml" && extension != ".twg")
            {
                EditorUtility.DisplayDialog("TERRAWORLD","INVALID FORMAT : Insert a valid TerraWorld graph file in .xml or .twg format!", "Ok");
                templateGraph = null;
            }
        }

        public static void CheckTempelatePackage(Object graphToExport)
        {
            if (graphToExport == null)
            {
                EditorUtility.DisplayDialog("TERRAWORLD", "GRAPH NOT INSERTED : Insert a TerraWorld graph file in .xml or .twg format into the GRAPH slot first!", "Ok");
                return;
            }
            else
            {
                if (CheckGraphResources(graphToExport))
                {
                    NoBug = true;
                }
                else
                    NoBug = false;

            }
        }


        public static void CreateTempelatePackage(Object graphToExport, string exportName)
        {
            if (graphToExport == null)
            {
                EditorUtility.DisplayDialog("TERRAWORLD","GRAPH NOT INSERTED : Insert a TerraWorld graph file in .xml or .twg format into the GRAPH slot first!", "Ok");
                return;
            }
            else
            {
                int progressID = TProgressBar.StartProgressBar("TERRAWORLD", "CREATING GRAPH PACKAGE", TProgressBar.ProgressOptionsList.Indefinite, false);
                TProgressBar.DisplayProgressBar("TERRAWORLD", "CREATING GRAPH PACKAGE", 0.5f, progressID);
                if (CheckGraphResources(graphToExport))
                {
                    InitResources(exportName);
                    AddCoreResources(graphToExport);
                    AddGraphResources(graphToExport);
                    ExportPackage(exportName);
                }
                TProgressBar.RemoveProgressBar(progressID);
            }
        }

        private static void InitResources(string exportName)
        {
            if (string.IsNullOrEmpty(exportName)) exportName = "My Graph";
            assetPaths = new List<string>();
        }

        private static void AddCoreResources(Object graphToExport)
        {
            //string FolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(graphToExport));
            //string[] dbFiles = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
            //
            //foreach (string db in dbFiles)
            //{
            //    assetPaths.Add(db);
            //}

            assetPaths.Add(AssetDatabase.GetAssetPath(graphToExport));
            //assetPaths.Add(graphicsPath);
            //assetPaths.Add(materialsPath);
            //assetPaths.Add(texturesPath);
            //assetPaths.Add(resourcesPathManager);
        }

        private static bool CheckGraphResources(Object graphToExport)
        {
            bool result = true;
            TTerraWorldGraph terraworldGraph = new TTerraWorldGraph();
            try
            {
                terraworldGraph.LoadGraph(AssetDatabase.GetAssetPath(graphToExport), false);
            }
            catch 
            {
                result = false;
                Debug.LogError("Error on loading graph file.");
            }
            
            if (result)
            {
                for (int i = 0; i < terraworldGraph.graphList.Count; i++)
                    for (int j = 0; j < terraworldGraph.graphList[i].nodes.Count; j++)
                    {
                        List<string> nodeResources = terraworldGraph.graphList[i].nodes[j].GetResourcePaths();
                        if (nodeResources != null)
                            for (int k = 0; k < nodeResources.Count; k++)
                            {
                                if (nodeResources[k] != null)
                                {
                                    if (File.Exists(nodeResources[k]))
                                    {
                                        Debug.Log("OK (" + terraworldGraph.graphList[i].nodes[j].Data.name + ") - " + nodeResources[k]);
                                    }
                                    else
                                    {
                                        if (!ReplaceEmptyResource(nodeResources[k], AssetDatabase.GetAssetPath(graphToExport)))
                                        {
                                            result = false;
                                            Debug.LogError("File not found : " + nodeResources[k] + "  Node : \"" + terraworldGraph.graphList[i].nodes[j].Data.name + "\"" );
                                        }
                                    }
                                }
                            }
                    }

                List<string> vfxResources = terraworldGraph.VFXDATA.GetResourcePaths();
                if (vfxResources != null)
                    for (int k = 0; k < vfxResources.Count; k++)
                    {
                        if (vfxResources[k] != null)
                        {
                            if (File.Exists(vfxResources[k]))
                            {
                                Debug.Log("OK (VFX) - " + vfxResources[k]);
                            }
                            else
                            if (!ReplaceEmptyResource(vfxResources[k], AssetDatabase.GetAssetPath(graphToExport)))
                            {
                                result = false;
                                Debug.LogError("File not found : " + vfxResources[k] + "  Node : VFX");
                            }

                        }
                    }

            }


            return result;

        }


        private static void AddGraphResources(Object graphToExport)
        {
            TTerraWorldGraph terraworldGraph = new TTerraWorldGraph();
            terraworldGraph.LoadGraph(AssetDatabase.GetAssetPath(graphToExport), false);


            for (int i = 0; i < terraworldGraph.graphList.Count; i++)
                for (int j = 0; j < terraworldGraph.graphList[i].nodes.Count; j++)
                {
                    List<string> nodeResources = terraworldGraph.graphList[i].nodes[j].GetResourcePaths();
                    if (nodeResources != null)
                        for (int k = 0; k < nodeResources.Count; k++)
                        {
                            if (nodeResources[k] != null)
                            {
                                if (File.Exists(nodeResources[k]))
                                {
                                    assetPaths.Add(nodeResources[k]);
                                    Debug.Log("OK (" + terraworldGraph.graphList[i].nodes[j].Data.name + ") - " + nodeResources[k]);
                                }
                                else
                                    throw new System.Exception("File not found : " + nodeResources[k] + "  Node : \"" + terraworldGraph.graphList[i].nodes[j].Data.name + "\"");

                            }
                        }
                }

            List<string> vfxResources = terraworldGraph.VFXDATA.GetResourcePaths();
            if (vfxResources != null)
                for (int k = 0; k < vfxResources.Count; k++)
                {
                    if (vfxResources[k] != null)
                    {
                        if (File.Exists(vfxResources[k]))
                        {
                            Debug.Log("OK (VFX) - " + vfxResources[k]);
                            assetPaths.Add(vfxResources[k]);
                        }
                        else
                            throw new System.Exception("File not found : " + vfxResources[k]);

                    }
                }

        }

        private static void ExportPackage(string exportName)
        {
            if (assetPaths == null || assetPaths.Count == 0) return;
            AssetDatabase.ExportPackage(assetPaths.ToArray(), exportName + ".unitypackage", ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Interactive);
        }

        private static bool ReplaceEmptyResource(string resourcePath, string TerraworldFileFullPath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return false;
            bool result = false;
            string newpath = string.Empty;
            string partialName = Path.GetFileName(resourcePath);
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(Application.dataPath + "/");

            try
            {
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles(partialName, SearchOption.AllDirectories);

                // if (filesInDir.Length == 1 ) resourcePath = filesInDir[0].FullName;
                if (filesInDir.Length > 0)
                {
                    int index = filesInDir[0].FullName.IndexOf("Asset");
                    newpath = filesInDir[0].FullName.Remove(0, index);
                    newpath = newpath.Replace(Path.PathSeparator, '/');
                }

                if (string.IsNullOrEmpty(newpath))
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "Refrence to the following file is missing.\n" + resourcePath , "OK");
                    result = false;
                }
                else
                {
                    if (AutoReplace || EditorUtility.DisplayDialog("TERRAWORLD", "Refrence to the following file is missing.\n" + resourcePath +"\n Do you want to replace it with :\n" + newpath, "OK","No"))
                    {
                        string text = File.ReadAllText(TerraworldFileFullPath);
                        text = text.Replace(resourcePath, newpath);
                        File.WriteAllText(TerraworldFileFullPath, text);
                        result = true;
                        Debug.LogWarning("Replaced : " + resourcePath + " - " + newpath);
                    }
                }
            }
            catch { }

            return result;
        }

    }
}
#endif

