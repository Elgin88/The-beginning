// Landscape Builder. Copyright (c) 2016-2021 SCSM Pty Ltd. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

namespace LandscapeBuilder
{
    /// <summary>
    /// LBManager helps to finalise a project so that it can be released.
    /// Has a dependency on LBSetup.cs, LBLayer, LBWater, LBTerrainData, LBLandscape,
    /// LBMapPath, LBGroup, LBGroupMember, LBTerrainTree, LBTerrainTexture, LBTerrainGrass,
    /// LBObjectPath etc
    /// NOTE: Try to keep dependencies to core LB classes. Don't include other editor classes
    /// apart from LBEditorCommon.
    /// </summary>
    public class LBManager : EditorWindow
    {
        #region Public variables

        #endregion

        #region Private variables
        private LBLandscape landscape = null;

        // Phase 1
        private string landscapeTemplateName = "Landscape Template";
        private bool createTemplatePackage = true;
        private bool addMapTexturesToTemplatePackage = false;
        private bool addLayerHeightmapTexturesToTemplatePackage = false;
        private bool addLBLightingToTemplate = true;
        private bool addPathsToTemplate = true;
        private bool addStencilsToTemplate = true;
        private bool addPathMeshMaterialsToTemplate = false;

        // Phase 2
        private bool isRemovePrefabItems = false;  // Off by default because cannot restore from a Template
        private bool isRemoveMapPaths = true;
        private bool isRemoveStencils = true;
        private bool isRemoveLBLandscape = false;

        // Phase 3
        private bool isRemoveEditorScripts = true;
        private bool isRemoveDemoScene = true;
        private bool isRemoveDemoAssets = false;
        private bool isRemoveUnityWater = false;
        private bool isRemoveModifiers = false;
        private bool isRemoveRuntimeSamples = false;
        private bool isRemoveSRP = true;

        // General
        private List<Component> tempComponentList = null;

        private Vector2 scrollPosition = Vector2.zero;
        private Color currentBgndColor = Color.white;
        private string txtColourName = "black";
        private GUIStyle labelFieldRichText = null;
        private static readonly float labelWidthWideOffset = 25f;
        private static readonly float labelWidthNarrowOffset = 16f;

        #endregion

        // TODO - LBManager
        // 1. Remove mesh controller from parent GO
        // 2. Remove LBLandscape script
        // 3. Warn about standard assets in LB from being deleted/moved before deleting LB folder
        // 4. Search for other LB scripts being used.

        // Things that may need to be retained
        // Terrain shaders
        // LBLighting
        // LB Screen Shot

        #region GUIContent
        private static readonly GUIContent landscapeGUIContent = new GUIContent(" Landscape", "Drag in the Landscape parent GameObject from the scene Hierarchy.");
        private static readonly GUIContent saveTemplateNameContent = new GUIContent("Template Name", "The name of the saved Landscape Template. IMPORTANT: Make sure these are unique in your project to avoid some being overwritten.");
        private static readonly GUIContent inventoryButtonContent = new GUIContent("Get Inventory", "This will output an text file containining the items in your landscape. NOTE: Prefabs will be listed but will not show their individual assets.");
        #endregion

        #region GUIContent - Phase 2
        private static readonly GUIContent removePrefabItemContent = new GUIContent("Remove Group IDs", "Remove all references to the Groups from the gameobjects in the landscape");
        private static readonly GUIContent removeMapPathContent = new GUIContent("Remove Map Paths", "Remove all LBMapPath scripts from the gameobjects in the landscape along with the path points. Path meshes will be retained in the scene.");
        private static readonly GUIContent removeStencilContent = new GUIContent("Remove Stencils", "Remove all LBStencil scripts and stencil data from the gameobjects in the landscape");
        private static readonly GUIContent removeLBLandscapeContent = new GUIContent("Remove Meta-data", "Remove landscape meta-data. This is where the majority of data in the LB Editor comes from. You will need a valid Template for each Landscape to restore this data.");
        private static readonly GUIContent finaliseBtnContent = new GUIContent("Finalise Landscape", "WARNING: This will remove references to Landscape Builder from the current landscape. There is currently no UNDO button. Use with caution.");
        #endregion

        #region GUIContent - Phase 3
        private static readonly GUIContent removeEditorScriptsContent = new GUIContent("Remove Editor Scripts", "Remove all LB Editor scripts from the project");
        private static readonly GUIContent removeDemoSceneContent = new GUIContent("Remove Demo Scene", "Remove LB Demo Scenes from the project");
        private static readonly GUIContent removeDemoAssetsContent = new GUIContent("Remove Demo Assets", "Remove all LB Demo assets (including textures, trees, and meshes) from the project");
        private static readonly GUIContent removeUnityWaterContent = new GUIContent("Remove Legacy Unity Water", "Remove old Unity Water Standard Assets included with LB from the project. If you don't use Unity water from Standard Assets you can safely remove these.");
        private static readonly GUIContent removeModifiersrContent = new GUIContent("Remove Modifiers & Images", "Remove ALL Topography Image Modifiers and sample heightmap Images and masks. Unless you are using these to generate heightmaps at runtime, you don't need them.");
        private static readonly GUIContent removeRuntimeSamplesContent = new GUIContent("Remove Runtime Samples", "Remove runtime prefabs, scripts, MapPath demo textures, and objects from the project");
        private static readonly GUIContent removeSRPContent = new GUIContent("Remove SRP Packages", "Remove the LB SRP folder and packages from the project. Even if using LWRP, URP or HDRP, you will have most likely already opened and installed the assets in these packages. Now they can safely be removed.");
        private static readonly GUIContent uninstallBtnContent = new GUIContent("Uninstall", "WARNING: This will DELETE scripts and folders in your project. There is no UNDO button. Use with caution.");

        private static readonly string phase3Info = "You need to: Selectively remove unnecessary Landscape Builder scripts and assets in the project. Re-import LB package to re-install them.";
        private static readonly string phase3DemoAssetWarning = "If you use any sample or demo assets like trees, ground textures, sheep, meshes etc., please move them outside the LandscapeBuilder folder first.";
        private static readonly string phase3ModifiersWarning = "If you have your own modifier files in the Modifiers folder please move them outside the LandscapeBuilder folder first.";
        private static readonly string phase3Prompt = "This action will uninstall all selected components. Make sure you have Finalised all landscape in all scenes before proceeding. If you are unsure Cancel.\n\nWARNING: There is NO UNDO.";

        #endregion

        #region Event Methods

        private void OnGUI()
        {
            //if (labelFieldRichText == null)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;
            }

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.HelpBox("The LB Manager will help you optimise your landscapes prior to shipping your game. It will remove all unneeded LB components. PLEASE READ MANUAL FIRST.", MessageType.Info, true);

            LBEditorHelper.InTechPreview();

            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth -= labelWidthNarrowOffset;
            landscape = (LBLandscape)EditorGUILayout.ObjectField(landscapeGUIContent, landscape, typeof(LBLandscape), true);
            EditorGUIUtility.labelWidth += labelWidthNarrowOffset;
            if (EditorGUI.EndChangeCheck() && landscape != null)
            {
                landscapeTemplateName = "LB_Backup_" + landscape.name + "_Template";
            }
            GUILayout.EndVertical();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUIUtility.labelWidth += labelWidthWideOffset;

            #region Start of Phase 1
            currentBgndColor = GUI.backgroundColor;
            GUI.backgroundColor = GUI.backgroundColor * (EditorGUIUtility.isProSkin ? 0.7f : 1.3f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = currentBgndColor;
            EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Phase 1 - Backup</b></color>", labelFieldRichText);

            EditorGUILayout.HelpBox("You need to: Backup each landscape in each scene to a template.", MessageType.Info, true);
            EditorGUIUtility.labelWidth -= labelWidthWideOffset + labelWidthNarrowOffset;
            landscapeTemplateName = EditorGUILayout.TextField(saveTemplateNameContent, landscapeTemplateName);
            EditorGUIUtility.labelWidth += labelWidthWideOffset + labelWidthNarrowOffset;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Backup Template", GUILayout.MaxWidth(130f)))
            {
                bool isSceneSaveRequired = false;

                if (landscape != null)
                {

                    LBEditorCommon.SaveTemplate(landscape, LBEditorCommon.LBVersion,
                                                landscapeTemplateName,
                                                ref isSceneSaveRequired,
                                                createTemplatePackage,
                                                addMapTexturesToTemplatePackage,
                                                addLayerHeightmapTexturesToTemplatePackage,
                                                addLBLightingToTemplate,
                                                addPathsToTemplate,
                                                addStencilsToTemplate,
                                                addPathMeshMaterialsToTemplate);
                }
                else
                {
                    EditorUtility.DisplayDialog("Backup Template", "Select a landscape above so that you can back it up to a template", "Got it!");
                }
            }

            if (GUILayout.Button(inventoryButtonContent, GUILayout.MaxWidth(110f)))
            {
                GetInventory();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical(); // End of Phase 1
            #endregion End Phase 1

            EditorGUILayout.Space();

            #region Start of Phase 2
            currentBgndColor = GUI.backgroundColor;
            GUI.backgroundColor = GUI.backgroundColor * (EditorGUIUtility.isProSkin ? 0.7f : 1.3f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = currentBgndColor;
            EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Phase 2 - Optimise</b></color>", labelFieldRichText);

            EditorGUILayout.HelpBox("You need to: Optimise each landscape in EACH scene by removing unnecessary scripts. You will need to open each scene one at a time.", MessageType.Info, true);

            isRemovePrefabItems = EditorGUILayout.Toggle(removePrefabItemContent, isRemovePrefabItems);
            isRemoveMapPaths = EditorGUILayout.Toggle(removeMapPathContent, isRemoveMapPaths);
            isRemoveStencils = EditorGUILayout.Toggle(removeStencilContent, isRemoveStencils);
            isRemoveLBLandscape = EditorGUILayout.Toggle(removeLBLandscapeContent, isRemoveLBLandscape);

            if (GUILayout.Button(finaliseBtnContent, GUILayout.MaxWidth(130f)))
            {
                if (landscape != null)
                {

                    if (EditorUtility.DisplayDialog("Finalise Landscape", "This action will clear all LB Editor data based on your preferences above.\n\nWARNING: There is NO UNDO.", "FINALISE", "Cancel"))
                    {
                        if (isRemovePrefabItems) { RemovePrefabItems(); }
                        if (isRemoveMapPaths) { RemoveMapPaths(); }
                        if (isRemoveStencils) { RemoveStencils(); }
                        if (isRemoveLBLandscape) { RemoveLBLandscape(); }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Finalise Landscape", "Select a landscape above so that you can finalise it", "Got it!");
                }
            }

            GUILayout.EndVertical(); // End of Phase 2
            #endregion End Phase 2

            EditorGUILayout.Space();

            #region Start of Phase 3
            currentBgndColor = GUI.backgroundColor;
            GUI.backgroundColor = GUI.backgroundColor * (EditorGUIUtility.isProSkin ? 0.7f : 1.3f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = currentBgndColor;
            EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Phase 3 - Uninstall</b></color>", labelFieldRichText);

            EditorGUILayout.HelpBox(phase3Info, MessageType.Info, true);

            isRemoveDemoScene = EditorGUILayout.Toggle(removeDemoSceneContent, isRemoveDemoScene);

            if (isRemoveDemoAssets) { EditorGUILayout.HelpBox(phase3DemoAssetWarning, MessageType.Warning, true); }
            
            isRemoveDemoAssets = EditorGUILayout.Toggle(removeDemoAssetsContent, isRemoveDemoAssets);
            isRemoveUnityWater = EditorGUILayout.Toggle(removeUnityWaterContent, isRemoveUnityWater);

            if (isRemoveModifiers) { EditorGUILayout.HelpBox(phase3ModifiersWarning, MessageType.Warning, true); }
            isRemoveModifiers = EditorGUILayout.Toggle(removeModifiersrContent, isRemoveModifiers);

            isRemoveRuntimeSamples = EditorGUILayout.Toggle(removeRuntimeSamplesContent, isRemoveRuntimeSamples);
            isRemoveEditorScripts = EditorGUILayout.Toggle(removeEditorScriptsContent, isRemoveEditorScripts);
            isRemoveSRP = EditorGUILayout.Toggle(removeSRPContent, isRemoveSRP);

            if (GUILayout.Button(uninstallBtnContent, GUILayout.MaxWidth(130f)))
            {
                if (EditorUtility.DisplayDialog("Uninstall components", phase3Prompt, "DO IT!", "Cancel"))
                {
                    // Close the LB Editor if it is open
                    var lbW = LBEditorHelper.GetLBW();
                    if (lbW != null) { lbW.Close(); }

                    if (isRemoveDemoAssets) { UninstallDemoAssets(); }
                    else if (isRemoveDemoScene) { UninstallDemoScene(); }
                    if (isRemoveUnityWater) { UninstallUnityWater(); }
                    if (isRemoveModifiers) { UninstallModifiers(); }
                    if (isRemoveRuntimeSamples) { UinstallRuntimeSamples(); }
                    if (isRemoveEditorScripts) { UninstallEditorScripts(); }
                    if (isRemoveSRP) { UninstallSRP(); }
                }
            }

            GUILayout.EndVertical(); // End of Phase 3
            #endregion End Phase 3

            EditorGUIUtility.labelWidth -= 10f;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
        }

        private void OnEnable()
        {
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; }
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that the editor window can be opened via the window menu tab
        [MenuItem("Window/Landscape Builder/Landscape Builder Manager")]
        public static void ShowWindow()
        {
            // Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(LBManager), false, "LB Manager");
        }

        #endregion

        #region Private Methods - General

        /// <summary>
        /// Create or clear the shared recyclable component list to reduce GC
        /// </summary>
        private void ClearTempComponentList()
        {
            if (tempComponentList == null) { tempComponentList = new List<Component>(10); }
            else { tempComponentList.Clear(); }
        }

        /// <summary>
        /// Open the Finder (Mac) or Explorer (Windows) in a given folder.
        /// </summary>
        /// <param name="folderPath"></param>
        private void OpenFileBrowser(string folderPath)
        {
            try
            {
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    System.Diagnostics.Process.Start("open", folderPath);
                }
                else
                {
                    // Windows (not sure able Linux)
                    System.Diagnostics.Process.Start(folderPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Could not open file browser. " + ex.Message);
            }
        }

        /// <summary>
        /// Get the text of the path for the texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private string GetAssetPathText(Texture2D texture)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            return string.IsNullOrEmpty(assetPath) ? "None" : assetPath;
        }

        /// <summary>
        /// Get the text of the path for the material.
        /// If the material is in the scene, rather than an asset, it will return
        /// None (materialname).
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        private string GetAssetPathText(Material mat)
        {
            string assetPath = AssetDatabase.GetAssetPath(mat);
            return string.IsNullOrEmpty(assetPath) ? (mat != null ?  "None (" + mat.name + ")" : "None") : assetPath;
        }

        /// <summary>
        ///  Get the text of the path for the GameObject
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private string GetAssetPathText(GameObject go)
        {
            if (go == null) { return "None"; }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(go);
                return string.IsNullOrEmpty(assetPath) ? "None" : assetPath;
            }
        }

        /// <summary>
        /// Return an space or the subname in brackets with a leading space
        /// </summary>
        /// <param name="subname"></param>
        /// <returns></returns>
        private string GetFormattedSecondaryName(string subname)
        {
            return string.IsNullOrEmpty(subname) ? " " : " (" + subname + ")";
        }

        private string GetTextNoSpace(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s;
        }

        #endregion

        #region Private Methods - Phase 1

        private void GetInventory()
        {
            if (landscape != null)
            {
                // Create the file outside the assets folder in the LandscapeBuilder folder.
                string invFilePath = "LandscapeBuilder/" + SceneManager.GetActiveScene().name + "_" + landscape.name + "_inventory.txt";

                bool isContinue = !File.Exists(invFilePath);

                if (!isContinue)
                {
                    isContinue = EditorUtility.DisplayDialog("Overwrite Inventory File", "Do you want to overwrite the existing inventory text file?\n\nWARNING: There is NO UNDO.", "DO IT!", "Cancel");
                }

                if (isContinue)
                {
                    string newLine = string.Empty;
                    //string assetPath = string.Empty;
                    // Create a new file. Overwrite existing.
                    StreamWriter fs = File.CreateText(invFilePath);

                    #region Landscape Basics

                    newLine = "Landscape Builder version " + LBEditorCommon.LBVersion + " " + LBEditorCommon.LBBetaVersion;
                    fs.WriteLine(newLine);
                    fs.WriteLine();

                    newLine = "Landscape name: " + landscape.name;
                    fs.WriteLine(newLine);

                    newLine = "Start Position: x:" + landscape.start.x + " y:" + landscape.start.y + " z:" + +landscape.start.z;
                    fs.WriteLine(newLine);

                    bool isNonStandardSize = landscape.size.x != landscape.size.y;
                    newLine = "Landscape Size: " + landscape.size.x + "m by " + landscape.size.y + "m" + (isNonStandardSize ? " * *Non - Standard * *" : "");
                    fs.WriteLine(newLine);

                    newLine = "Terrain Size: " + landscape.GetLandscapeTerrainWidth() + "m by " + landscape.GetLandscapeTerrainLength() + "m";
                    fs.WriteLine(newLine);

                    newLine = "Terrain Height: " + landscape.GetLandscapeTerrainHeight() + "m";
                    fs.WriteLine(newLine);

                    newLine = "Terrain Heightmap Resolution: " + landscape.GetLandscapeTerrainHeightmapResolution();
                    fs.WriteLine(newLine);
                    
                    fs.WriteLine("GPU Acceleration - Topography: " + landscape.useGPUTopography.ToString());
                    fs.WriteLine("GPU Acceleration - Texturing: " + landscape.useGPUTexturing.ToString());
                    fs.WriteLine("GPU Acceleration - Grass: " + landscape.useGPUGrass.ToString());
                    fs.WriteLine("GPU Acceleration - Path: " + landscape.useGPUPath.ToString());

                    #endregion

                    #region Inventory - water
                    foreach (LBWater lbWater in landscape.landscapeWaterList)
                    {
                        newLine = "[Water] " + lbWater.name + (lbWater.isPrimaryWater ? " (Primary)" : "") + " prefab: " + lbWater.waterPrefabName;
                        fs.WriteLine(newLine);
                    }
                    #endregion

                    #region Inventory - MapPaths

                    List<LBMapPath> lbMapPaths = new List<LBMapPath>(1);

                    landscape.GetComponentsInChildren(lbMapPaths);

                    int numMapPaths = lbMapPaths == null ? 0 : lbMapPaths.Count;
                    if (numMapPaths > 0) { fs.WriteLine(); fs.WriteLine("MAP PATHS"); }

                    for (int i = 0; i < numMapPaths; i++)
                    {
                        LBMapPath lbMapPath = lbMapPaths[i];
                        if (lbMapPath != null && lbMapPath.lbPath != null)
                        {
                            Transform trfm = lbMapPath.transform.Find(lbMapPath.lbPath.pathName + " Mesh");
                            newLine = "[MapPath] " + lbMapPath.lbPath.pathName + " (mesh: " + (trfm == null ? "None" : trfm.name) + ") Mesh Material: " + GetAssetPathText(lbMapPath.meshMaterial);
                            fs.WriteLine(newLine);
                        }
                    }

                    #endregion

                    #region Inventory - Topography

                    int numLayers = landscape.topographyLayersList == null ? 0 : landscape.topographyLayersList.Count;

                    if (numLayers > 0) { fs.WriteLine(); fs.WriteLine("TOPOGRAPHY"); }

                    for(int i = 0; i < numLayers; i++)
                    {
                        LBLayer lbLayer = landscape.topographyLayersList[i];

                        string layerName = (lbLayer.isDisabled ? "{Disabled} " : " ")  + (string.IsNullOrEmpty(lbLayer.layerName) ? "Layer " + (i+1).ToString("000") : lbLayer.layerName);

                        // Image add/sub/detail layers
                        if (lbLayer.LayerTypeInt >= 4 && lbLayer.LayerTypeInt <= 7)
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") imageSource: " + lbLayer.imageSource.ToString();
                            fs.WriteLine(newLine);
                        }

                        if (lbLayer.heightmapImage != null)
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") heightmapImage: " + GetAssetPathText(lbLayer.heightmapImage);
                            fs.WriteLine(newLine);
                        }

                        if (lbLayer.modifierLBWater != null && lbLayer.modifierLBWater.waterMaterial)
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") water material: " + GetAssetPathText(lbLayer.modifierLBWater.waterMaterial);
                            fs.WriteLine(newLine);
                        }

                        if (lbLayer.modifierLBWater != null && string.IsNullOrEmpty(lbLayer.modifierLBWater.waterPrefabName))
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") water prefab: " + lbLayer.modifierLBWater.waterPrefabName;
                            fs.WriteLine(newLine);
                        }

                        if (lbLayer.type == LBLayer.LayerType.UnityTerrains && LBTerrainData.HasRAWHeightData(lbLayer.lbTerrainDataList))
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") Original Data Source: " + LBTerrainData.GetDataSourceDisplayName(lbLayer.lbTerrainDataList);
                            fs.WriteLine(newLine);
                        }

                        if (lbLayer.type == LBLayer.LayerType.ImageModifier)
                        {
                            newLine = "[LBLayer] " + layerName + " (" + lbLayer.type.ToString() + ") " + lbLayer.modifierSourceFileType.ToString() + " Data Source: ";
                            if (lbLayer.modifierRAWFile != null)
                            {
                                newLine += string.IsNullOrEmpty(lbLayer.modifierRAWFile.dataSourceName) ? "Unknown" : lbLayer.modifierRAWFile.dataSourceName;
                            }
                            else { newLine += "Unknown"; }
                            fs.WriteLine(newLine);
                        }
                    }

                    #endregion

                    #region Inventory - Texturing
                    int numTextures = landscape.terrainTexturesList == null ? 0 : landscape.terrainTexturesList.Count;
                    if (numTextures > 0) { fs.WriteLine(); fs.WriteLine("TEXTURING"); }
                    for (int i = 0; i < numTextures; i++)
                    {
                        LBTerrainTexture lbTerrainTexture = landscape.terrainTexturesList[i];
                        if (lbTerrainTexture != null)
                        {
                            fs.WriteLine("[Texture] " + (i + 1).ToString("000") + (lbTerrainTexture.isDisabled ? " {Disabled}" : ""));
                            fs.WriteLine(" texture: " + GetAssetPathText(lbTerrainTexture.texture) + GetFormattedSecondaryName(lbTerrainTexture.textureName));
                            fs.WriteLine(" normalmap: " + GetAssetPathText(lbTerrainTexture.normalMap) + GetFormattedSecondaryName(lbTerrainTexture.normalMapName));
                            fs.WriteLine(" heightmap: " + GetAssetPathText(lbTerrainTexture.heightMap));
                            if (lbTerrainTexture.texturingMode == LBTerrainTexture.TexturingMode.Map || lbTerrainTexture.texturingMode == LBTerrainTexture.TexturingMode.HeightInclinationMap)
                            {
                                fs.WriteLine(" map: " + GetAssetPathText(lbTerrainTexture.map));
                            }
                        }
                    }
                    #endregion

                    #region Inventory - Trees
                    int numTrees = landscape.terrainTreesList == null ? 0 : landscape.terrainTreesList.Count;
                    if (numTrees > 0) { fs.WriteLine(); fs.WriteLine("TREES"); }
                    for (int i = 0; i < numTrees; i++)
                    {
                        LBTerrainTree lbTerrainTree = landscape.terrainTreesList[i];
                        if (lbTerrainTree != null)
                        {
                            fs.WriteLine("[Tree] " + (i + 1).ToString("000") + (lbTerrainTree.isDisabled ? " {Disabled}" : "") + " prefab: " + GetAssetPathText(lbTerrainTree.prefab) + GetFormattedSecondaryName(lbTerrainTree.prefabName));                           
                            if (lbTerrainTree.treePlacingMode == LBTerrainTree.TreePlacingMode.Map || lbTerrainTree.treePlacingMode == LBTerrainTree.TreePlacingMode.HeightInclinationMap)
                            {
                                fs.WriteLine(" map: " + GetAssetPathText(lbTerrainTree.map));
                            }
                        }
                    }
                    #endregion

                    #region Inventory - Grass
                    int numGrass = landscape.terrainGrassList == null ? 0 : landscape.terrainGrassList.Count;
                    if (numGrass > 0) { fs.WriteLine(); fs.WriteLine("GRASS"); }
                    for (int i = 0; i < numGrass; i++)
                    {
                        LBTerrainGrass lbGrass = landscape.terrainGrassList[i];
                        if (lbGrass != null)
                        {
                            newLine = "[Grass] " + (i + 1).ToString("000") + (lbGrass.isDisabled ? " { Disabled}" : "") + " ";
                            if (lbGrass.useMeshPrefab)
                            {
                                newLine += GetAssetPathText(lbGrass.meshPrefab) + GetFormattedSecondaryName(lbGrass.meshPrefabName);
                            }
                            else
                            {
                                newLine += GetAssetPathText(lbGrass.texture) + GetFormattedSecondaryName(lbGrass.textureName);
                            }
                            fs.WriteLine(newLine);
                            if (lbGrass.grassPlacingMode == LBTerrainGrass.GrassPlacingMode.Map || lbGrass.grassPlacingMode == LBTerrainGrass.GrassPlacingMode.HeightInclinationMap)
                            {
                                fs.WriteLine(" map: " + GetAssetPathText(lbGrass.map));
                            }
                        }
                    }

                    #endregion

                    #region Inventory - Groups
                    int numGroups = landscape.lbGroupList == null ? 0 : landscape.lbGroupList.Count;
                    if (numGroups > 0) { fs.WriteLine(); fs.WriteLine("GROUPS"); }
                    for (int gIdx = 0; gIdx < numGroups; gIdx++)
                    {
                        LBGroup lbGroup = landscape.lbGroupList[gIdx];
                        if (lbGroup != null)
                        {
                            fs.WriteLine("[GROUP] " + (gIdx + 1).ToString("000") + (lbGroup.isDisabled ? " { Disabled} " : " ") + (string.IsNullOrEmpty(lbGroup.groupName) ? "No Name" : lbGroup.groupName) + " GroupType: " + lbGroup.lbGroupType.ToString());

                            int numMembers = lbGroup.groupMemberList == null ? 0 : lbGroup.groupMemberList.Count;
                            
                            if (numMembers < 1) { fs.WriteLine(" [No Members]"); }
                            else
                            {
                                for (int mIdx = 0; mIdx < numMembers; mIdx++)
                                {
                                    LBGroupMember lbGroupMember = lbGroup.groupMemberList[mIdx];
                                    if (lbGroupMember != null)
                                    {
                                        fs.WriteLine(" [GROUPMEMBER] " + (mIdx + 1).ToString("000") + (lbGroupMember.isDisabled ? " {Disabled} " : " ") + lbGroupMember.lbMemberType.ToString());
                                        if (lbGroupMember.lbMemberType == LBGroupMember.LBMemberType.Prefab)
                                        {
                                            fs.WriteLine("  prefab: " + GetAssetPathText(lbGroupMember.prefab) + GetFormattedSecondaryName(lbGroupMember.prefabName));
                                        }
                                        else if (lbGroupMember.lbMemberType == LBGroupMember.LBMemberType.ObjPath)
                                        {
                                            LBObjPath lbObjPath = lbGroupMember.lbObjPath;

                                            if (lbObjPath != null)
                                            {
                                                fs.WriteLine("  path name: " + GetTextNoSpace(lbObjPath.pathName));
                                                fs.WriteLine("  surface mesh material: " + GetAssetPathText(lbObjPath.surfaceMeshMaterial));
                                                fs.WriteLine("  base mesh material: " + GetAssetPathText(lbObjPath.baseMeshMaterial));

                                                // Default Series and Width-based Series only contain linkage (GUID) member data. The actual prefabs
                                                // are stored in regular GroupMember's of type prefab.
                                            }    
                                        }
                                    }
                                }
                            }                            
                        }
                    }

                    #endregion

                    fs.Close();

                    string fullFolderPath = Path.GetFullPath(Path.GetDirectoryName(invFilePath));
                    OpenFileBrowser(fullFolderPath);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Get Inventory", "Select a landscape above so that you can get its inventory.", "Got it!");
            }
        }

        #endregion

        #region Private Methods - Phase 2

        /// <summary>
        /// This removes the LBLandscape script and all meta-data about the landscape
        /// from the parent gameobject. It also removes the mesh controller if there is one present
        /// </summary>
        private void RemoveLBLandscape()
        {
            if (landscape != null)
            {
                string landscapeName = landscape.name;

                // Remove the LandscapeMeshController if it exists
                LBLandscapeMeshController lmc = landscape.gameObject.GetComponent<LBLandscapeMeshController>();
                if (lmc != null) { Object.DestroyImmediate(lmc); }

                // Remove any old terrain mesh controllers (these will only exist if something went wrong during a combine mesh operation)
                LBTerrainMeshController[] terrainMeshControllers = landscape.gameObject.GetComponentsInChildren<LBTerrainMeshController>();

                int numTerrainMeshControllers = terrainMeshControllers == null ? 0 : terrainMeshControllers.Length;

                for (int tmc = 0; tmc < numTerrainMeshControllers; tmc++)
                {
                    DestroyImmediate(terrainMeshControllers[tmc].gameObject);
                }

                Object.DestroyImmediate(landscape);

                Debug.Log("LB Manager. Finalise Landscape. Removed LBLandscape component and meta-data from " + landscapeName);
            }
        }

        /// <summary>
        /// Remove LBPrefabItem scripts with their ID data
        /// </summary>
        private void RemovePrefabItems()
        {
            if (landscape != null)
            {
                LBPrefabItem[] prefabItems = landscape.GetComponentsInChildren<LBPrefabItem>(true);

                int numPrefabItems = prefabItems == null ? 0 : prefabItems.Length;

                for (int i = numPrefabItems - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(prefabItems[i]);
                }

                Debug.Log("LB Manager. Finalise Landscape. Removed " + numPrefabItems + " LBPrefabItem components from " + landscape.name);
            }
        }

        /// <summary>
        /// Remove all the MapPath scripts from the landscape. Retain any path meshes.
        /// </summary>
        private void RemoveMapPaths()
        {
            if (landscape != null)
            {
                LBMapPath[] mapPaths = landscape.GetComponentsInChildren<LBMapPath>(true);
                int numMapPaths = mapPaths == null ? 0 : mapPaths.Length;

                ClearTempComponentList();

                for (int i = numMapPaths - 1; i >= 0; i--)
                {
                    mapPaths[i].gameObject.GetComponentsInChildren(true, tempComponentList);

                    // If there is only the transform and the LBMapPath scripts, remove the whole gameobject,
                    // else only remove the LBMapPath script component
                    if (tempComponentList.Count == 2) { Object.DestroyImmediate(mapPaths[i].gameObject); }
                    else { Object.DestroyImmediate(mapPaths[i]); }

                    ClearTempComponentList();
                }

                Debug.Log("LB Manager. Finalise Landscape. Removed " + numMapPaths + " LBMapPath components and data from " + landscape.name);
            }
        }

        /// <summary>
        /// Remove all Stencil scripts, configuration and data from the landscape
        /// </summary>
        private void RemoveStencils()
        {
            if (landscape != null)
            {
                LBStencil[] stencils = landscape.GetComponentsInChildren<LBStencil>(true);
                int numStencils = stencils == null ? 0 : stencils.Length;

                ClearTempComponentList();

                for (int i = numStencils - 1; i >= 0; i--)
                {
                    stencils[i].gameObject.GetComponentsInChildren(true, tempComponentList);

                    // If there is only the transform and the LBStencil scripts, remove the whole gameobject,
                    // else only remove the LBStencil script component
                    if (tempComponentList.Count == 2) { Object.DestroyImmediate(stencils[i].gameObject); }
                    else { Object.DestroyImmediate(stencils[i]); }

                    ClearTempComponentList();
                }

                Debug.Log("LB Manager. Finalise Landscape. Removed " + numStencils + " LBStencil components and data from " + landscape.name);
            }
        }


        #endregion

        #region Private Methods - Phase 3

        private void UninstallDemoScene()
        {
            if (Directory.Exists(LBSetup.demosceneFolder))
            {
                if (AssetDatabase.DeleteAsset(LBSetup.demosceneFolder + "/LBDemoScene2.unity"))
                {
                    Debug.Log("LB Manager - Uninstalled the Demo Scene");
                }
            }
        }

        /// <summary>
        /// Delete the Demo Scene folder and the templates that ship with LB.
        /// Delete Bootcamp Assets, Textures Heightmap, and Textures NormalMaps folders.
        /// Delete sample heightmap images from Images folder.
        /// </summary>
        private void UninstallDemoAssets()
        {
            // Remove templates first
            if (Directory.Exists(LBSetup.templatesFolder))
            {
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/DemoLandscape1 Template.prefab");
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/DemoLandscape3 Template.prefab");
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/FPS Forest Demo Template.prefab");
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/Mountains Plane Demo Template.prefab");
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/ObjPath Demo Template.prefab");
                AssetDatabase.DeleteAsset(LBSetup.templatesFolder + "/Swiss Mountains Demo Template.prefab");
            }

            if (Directory.Exists(LBSetup.demosceneFolder))
            {
                AssetDatabase.DeleteAsset(LBSetup.demosceneFolder);
            }

            if (Directory.Exists(LBSetup.lbFolder + "/Bootcamp Assets"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Bootcamp Assets");
            }

            // Cleanup Textures folder
            if (Directory.Exists(LBSetup.lbFolder + "/Textures/Heightmap"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Textures/Heightmap");
            }

            if (Directory.Exists(LBSetup.lbFolder + "/Textures/NormalMaps"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Textures/NormalMaps");
            }

            // Old ST prefab trees from pre-2.3.0
            if (Directory.Exists(LBSetup.lbFolder + "/OptimisedTrees"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/OptimisedTrees");
            }

            // Remove standard asset textures that come with LB
            if (Directory.Exists(LBSetup.lbFolder + "/Standard Assets/Environment/TerrainAssets/BillboardTextures"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Standard Assets/Environment/TerrainAssets/BillboardTextures");
            }

            // Remove standard asset textures that come with LB
            if (Directory.Exists(LBSetup.lbFolder + "/Standard Assets/Environment/TerrainAssets/SurfaceTextures"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Standard Assets/Environment/TerrainAssets/SurfaceTextures");
            }

            Debug.Log("LB Manager - Uninstalled the Demo assets.");
        }

        /// <summary>
        /// Remove legacy Unity Water assets that are in the Standard Assets
        /// folder in Landscape Builder
        /// </summary>
        private void UninstallUnityWater()
        {
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Standard Assets/Environment/Water");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Standard Assets/Environment/Water (Basic)");
            Debug.Log("LB Manager - Uninstalled Legacy Unity Water");
        }

        /// <summary>
        /// Remove Topography Image Modifiers and sample heightmap Images
        /// </summary>
        private void UninstallModifiers()
        {
            // cleanup old heightmap images and masks
            if (Directory.Exists(LBSetup.lbFolder + "/Images"))
            {
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/CottageHeightmap1.png");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/HillsHeightmap.psd");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/IslandMask1.psd");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/IslandMask2.psd");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/IslandMask3.psd");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/LagoonMask.psd");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/LayerMapMask1.png");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/LayerMapMask2.png");
                AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Images/MountainsHeightmap.psd");
            }

            // remove whole topgraphy Image Modifiers folder
            if (Directory.Exists(LBSetup.modifiersFolder))
            {
                AssetDatabase.DeleteAsset(LBSetup.modifiersFolder);
            }

            Debug.Log("LB Manager - Uninstalled All Topography Image Modifiers and sample heightmap Images and masks.");
        }

        /// <summary>
        /// Delete the Samples folder which includes runtime prefabs, scripts etc.
        /// </summary>
        private void UinstallRuntimeSamples()
        {
            if (Directory.Exists(LBSetup.samplesFolder))
            {
                AssetDatabase.DeleteAsset(LBSetup.samplesFolder);
                Debug.Log("LB Manager - Uninstalled Runtime Samples");
            }

            // Remove the runtime MapPath map textures used in the samples
            if (Directory.Exists(LBSetup.demosceneFolder + "/Maps"))
            {
                AssetDatabase.DeleteAsset(LBSetup.demosceneFolder + "/Maps/RTGrassLayer6.png");
                AssetDatabase.DeleteAsset(LBSetup.demosceneFolder + "/Maps/RTPathway6.png");
                AssetDatabase.DeleteAsset(LBSetup.demosceneFolder + "/Maps/RTPathway7.png");
                AssetDatabase.DeleteAsset(LBSetup.demosceneFolder + "/Maps/RTValleyFloor6.png");
            }
        }

        /// <summary>
        /// Delete Editor scripts including Designers.
        /// Do not delete the LBEditorCommon.cs script as this is used
        /// by LBManager.
        /// </summary>
        private void UninstallEditorScripts()
        {
            // Remove the in-scene Designers
            // This assumes there are no active designers in any landscape in any scene
            if (Directory.Exists(LBSetup.scriptsDesignersFolder))
            {
                // Cannot delete the whole folder because it contains LBStencilBrushPainter.cs which is a
                // dependency of LBStencil.cs (not just the stencil custom editor)

                // Remove the global #define for the LB Editor scripts so that LBGroupDesignerItem
                // is not referenced in LBLandscapeTerrain.PopulateTerrainWithGroups(..)
                const string LBEditor_Define = "LB_EDITOR";
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (defines.Contains(LBEditor_Define))
                {
                    defines = defines.Replace(LBEditor_Define + ";", "");
                    defines = defines.Replace(LBEditor_Define, "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
                }

                AssetDatabase.DeleteAsset(LBSetup.scriptsDesignersFolder + "/LBGroupDesigner.cs");
                AssetDatabase.DeleteAsset(LBSetup.scriptsDesignersFolder + "/LBGroupLocationItem.cs");
                AssetDatabase.DeleteAsset(LBSetup.scriptsDesignersFolder + "/LBObjPathDesigner.cs");
                AssetDatabase.DeleteAsset(LBSetup.scriptsDesignersFolder + "/LBGroupDesignerItem.cs");
            }

            if (Directory.Exists(LBSetup.editorFolder))
            {
                // Keep LBEditorCommon.cs and LBManager.cs

                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/3rdPartyLicenses");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/Textures");

                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LandscapeBuilderGrassEditor.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LandscapeBuilderGrassSelector.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBGroupDesignerItemEditor.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBGroupLocationItemEditor.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBTemplateEditor.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBTextureGeneratorWindow.cs");

                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LandscapeBuilderWindow.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBImportTIFF.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LBEditorIntegration.cs");
                AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LibTIFF");

                // Sometimes not all content from LibTIFF is removed
                AssetDatabase.Refresh(ImportAssetOptions.Default);

                // If some content remains delete it after a refresh
                if (Directory.Exists(LBSetup.editorFolder + "/LibTIFF"))
                {
                    AssetDatabase.DeleteAsset(LBSetup.editorFolder + "/LibTIFF");
                }
            }

            // Remove the setup folder which contains the grass setup data
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Setup");

            // Editor presets
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Prefabs/LB Default Resources.prefab");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Prefabs/LBLogo.prefab");

            // Remove highlighters used in the editors
            AssetDatabase.DeleteAsset(LBSetup.materialsFolder + "/AreaHighlighter.mat");
            AssetDatabase.DeleteAsset(LBSetup.materialsFolder + "/HeightPickerHighlighter.mat");
            AssetDatabase.DeleteAsset(LBSetup.materialsFolder + "/LBLocation.mat");
            AssetDatabase.DeleteAsset(LBSetup.materialsFolder + "/LBClear.mat");
            AssetDatabase.DeleteAsset(LBSetup.materialsFolder + "/TerrainHighlighter.mat");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/AreaHighlighter.png");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/highlighter2.png");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/highlighter3.png");
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/LB1Icon.psd");

            // ProjectorLight shader used with highlighters
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Standard Assets/Effects/Projectors");

            // Remove the manual
            AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/Landscape Builder.pdf");

            Debug.Log("LB Manager - Uninstalled Editor scripts");
        }

        /// <summary>
        /// Remove the LB SRP folder from the project. This contains the LWRP and HDRP
        /// asset packages.
        /// </summary>
        private void UninstallSRP()
        {
            if (Directory.Exists(LBSetup.editorFolder))
            {
                if (AssetDatabase.DeleteAsset(LBSetup.lbFolder + "/SRP"))
                {
                    Debug.Log("LB Manager - Uninstalled SRP folder");
                }
            }
        }

        #endregion
    }
}
