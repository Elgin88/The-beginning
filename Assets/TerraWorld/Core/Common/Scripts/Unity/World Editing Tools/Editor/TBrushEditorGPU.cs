#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime.UI
{
    public class TBrushEditorGPU : Editor
    {
        public GPUInstanceLayer WT;
        private Event e;

        public void MaskEditorGUI()
        {
            Repaint();
            SceneView.RepaintAll();
            e = Event.current;
            SwitchEditMode();

            if (WT.parameters == null) WT.parameters = WT.transform.GetChild(0).GetComponent<TScatterParams>();

            if (WT.parameters.maskDataFast != null)
            {
                TBrushFunctions.GIL = WT;
                WorldToolsParams.globalMode = false;
                WorldToolsParams.isGPULayer = true;
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 150; style.fixedHeight = 32;

                THelpersUIRuntime.GUI_Button(new GUIContent("GO TO WORLD EDITOR", "Press this button to go to the WorldTools Editor component to batch edit all layers in world!"), style, TBrushFunctions.GoToEditorWorld, 0, 0, new Color(0.2f, 0.65f, 0.2f, 0.75f));

                if (WorldToolsParams.isEditMode)
                {
                    THelpersUIRuntime.GUI_Button(new GUIContent("EXIT EDIT MODE", "Finish editing placement"), style, TBrushFunctions.EditPlacement, 0, 0, Color.red);
                    THelpersUIRuntime.GUI_HelpBox(WorldToolsParams.editingText, MessageType.None);

                    // Since mask data is not serialized for faster binary read/write now, we cannot use Undo operations
                    WT.parameters.undoMode = 1;
                    //THelpersUIRuntime.GUI_HelpBox(new GUIContent("UNDO OPERATIONS", ""), true, 20);
                    //int undoMode = WT.parameters.undoMode;
                    //EditorGUI.BeginChangeCheck();
                    //undoMode = THelpersUIRuntime.GUI_SelectionGrid(undoMode, WorldToolsParams.onOffSelection, new GUIStyle(EditorStyles.toolbarButton), -10);
                    //if (EditorGUI.EndChangeCheck()) WT.parameters.undoMode = undoMode;
                    //
                    //if (WT.parameters.undoMode == 0)
                    //    THelpersUIRuntime.GUI_HelpBox("TURN OFF UNDO FOR REAL-TIME & FASTER WORLD EDITING!", MessageType.Error, -5);
                    //else if (WT.parameters.undoMode == 1)
                    //    THelpersUIRuntime.GUI_HelpBox("UNDO IS BYPASSED FOR FASTER WORLD EDITING!", MessageType.Info, -5);

                    if (WorldToolsParams.painting) THelpersUIRuntime.GUI_HelpBox("PLACING MODELS", MessageType.None, 10, Color.cyan);
                    if (WorldToolsParams.erasing) THelpersUIRuntime.GUI_HelpBox("REMOVING MODELS", MessageType.None, 10, Color.red);

                    GUILayout.Space(40);

                    int maximumBrushSize = (int)WT.parameters.terrain.terrainData.size.x / 4;
                    int minimumBrushSize = Mathf.Clamp((int)(WT.parameters.terrain.terrainData.size.x / WT.parameters.maskDataFast.Length), 1, maximumBrushSize);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("brushRadius"), new GUIContent("BRUSH RADIUS", "Brush Radius for painting and editing placement"));
                    WT.brushRadius = Mathf.Clamp(WT.brushRadius, minimumBrushSize, maximumBrushSize);
                    
                    GUILayout.Space(5);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("brushDensity"), new GUIContent("BRUSH DENSITY", "Brush Density for painting and editing placement"));
                    
                    GUILayout.Space(15);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isolateLayer"), new GUIContent("ISOLATE PREVIEW", "Isolate preview and only show this layer in scene view while painting"));
                    
                    GUILayout.Space(20);
                    WT.sectionToggleTexturesFiltering = THelpersUIRuntime.Foldout("TERRAIN LAYERS (TEXTURES) FILTERING", ref WT.sectionToggleTexturesFiltering);

                    if (WT.sectionToggleTexturesFiltering)
                    {
                        int terrainLayersCount = WT.parameters.terrain.terrainData.terrainLayers.Length;

                        if (terrainLayersCount > 0)
                        {
                            style = new GUIStyle(); style.fixedWidth = 64; style.fixedHeight = 64;
                            if (WorldToolsParams.terrainLayers == null || WorldToolsParams.terrainLayers.Length == 0 || WorldToolsParams.terrainLayers.Length != terrainLayersCount) WorldToolsParams.terrainLayers = WT.parameters.terrain.terrainData.terrainLayers;
                        
                            //if (WT.exclusion == null || WT.exclusion.Length == 0 || WT.exclusion.Length != terrainLayersCount)
                            //{
                            //    serializedObject.Update();
                            //    WT.exclusion = new float[terrainLayersCount];
                            //    for (int i = 0; i < WT.exclusion.Length; i++) WT.exclusion[i] = 0f;
                            //    serializedObject.ApplyModifiedProperties();
                            //}

                            EditorGUI.BeginChangeCheck();
                            for (int i = 0; i < WT.parameters.exclusionOpacities.Length; i++)
                            {
                                GUILayout.Space(60);
                                WorldToolsParams.lastRect = GUILayoutUtility.GetLastRect();
                                WorldToolsParams.lastRect.x = (Screen.width / 2) - (64 / 2);
                                WorldToolsParams.lastRect.y += 30;
                                THelpersUIRuntime.GUI_Label(WorldToolsParams.terrainLayers[i].diffuseTexture, WorldToolsParams.lastRect, style);
                                GUILayout.Space(20);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("exclusion").GetArrayElementAtIndex(i), new GUIContent("EXCLUSION PERCENTAGE  %", "Filtering percentage to remove models based on this terrain layer's alphamap weight"));
                                WT.parameters.exclusionOpacities[i] = WT.exclusion[i];
                            }
                            if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed(true);
                        }
                        else
                            THelpersUIRuntime.GUI_HelpBox("NO TERRAIN LAYERS (TEXTURES) DETECTED ON TERRAIN!", MessageType.Info, 10);
                    }

                    GUILayout.Space(40);

                    EditorGUI.BeginChangeCheck();
                    WorldToolsParams.sectionToggleMaskEdit = THelpersUIRuntime.Foldout("CUSTOM MASK", ref WorldToolsParams.sectionToggleMaskEdit);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (WorldToolsParams.sectionToggleMaskEdit)
                            TBrushFunctions.ConvertMaskDataToImage(WT.parameters.maskDataFast);
                    }

                    THelpersUIRuntime.GUI_HelpBox("YOU CAN DRAG & DROP YOUR CUSTOM MASK IMAGE TO UPDATE PLACEMENT! (BETA)", MessageType.Info, 20);

                    GUILayout.Space(20);

                    if (WorldToolsParams.sectionToggleMaskEdit)
                    {
                        if (WorldToolsParams.maskImage != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            WorldToolsParams.maskImage = (Texture2D)THelpersUIRuntime.GUI_ObjectField(new GUIContent("Mask", "Drag & drop a custom mask to update!"), WorldToolsParams.maskImage, typeof(Texture2D));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (WorldToolsParams.maskImage != null)
                                {
                                    if (WorldToolsParams.maskImageTemp != null && EditorUtility.DisplayDialog("EXPORT MASK", "Do you want to export current mask into an image file for later usage?", "Yes", "No"))
                                        TBrushFunctions.ExportMask(WorldToolsParams.maskImageTemp);

                                    if (!WorldToolsParams.maskImage.isReadable) TBrushFunctions.WarmUpMask(WorldToolsParams.maskImage);
                                    TBrushFunctions.ConvertImageToMaskData(ref WT.parameters.maskDataFast);
                                    UpdatePlacement();
                                }
                                else
                                    TBrushFunctions.ConvertMaskDataToImage(WT.parameters.maskDataFast);

                                WorldToolsParams.maskImageTemp = WorldToolsParams.maskImage;
                            }

                            if (WorldToolsParams.maskImage == null) return;

                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            lastRect.x = (Screen.width / 2) - (WorldToolsParams.maskPreviewResolution / 2);
                            lastRect.y += 30;
                            lastRect.width = WorldToolsParams.maskPreviewResolution;
                            lastRect.height = WorldToolsParams.maskPreviewResolution;
                            EditorGUI.DrawPreviewTexture(lastRect, WorldToolsParams.maskImage);
                            GUILayout.Space(WorldToolsParams.maskPreviewResolution);

                            style = new GUIStyle(EditorStyles.toolbarButton);
                            style.fixedWidth = 150;
                            style.fixedHeight = 32;

                            GUILayout.Space(30);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("UPDATE MASK", "Update mask preview with latest painting changes"), style))
                            {
                                TBrushFunctions.ConvertMaskDataToImage(WT.parameters.maskDataFast);
                            }
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            THelpersUIRuntime.GUI_Button(new GUIContent("EXPORT MASK", "Export painted mask into a file!"), style, TBrushFunctions.ExportMask);

                            THelpersUIRuntime.DrawUILine(30);
                        }
                    }
                }
                else
                {
                    style = new GUIStyle(EditorStyles.toolbarButton);
                    style.fixedWidth = 150;
                    style.fixedHeight = 32;
                    THelpersUIRuntime.GUI_Button(new GUIContent("EDIT PLACEMENT", "Paint and edit placement in scene"), style, TBrushFunctions.EditPlacement);
                }
                GUILayout.Space(10);
            }
        }

        private void SwitchEditMode()
        {
            if (e != null && e.keyCode == KeyCode.E)
            {
                if (!WorldToolsParams.isEditMode) WorldToolsParams.isEditMode = true;
                Repaint();
            }
            else if (e != null && e.keyCode == KeyCode.Escape)
            {
                if (WorldToolsParams.isEditMode) WorldToolsParams.isEditMode = false;
                Repaint();
            }
        }

        public void OnDestroy()
        {
            TBrushFunctions.OnDestroy();
        }

        public virtual void UpdatePlacement() { }
        public virtual void UpdatePlacementDelayed(bool forced = false) { }
        public virtual void SyncPlacement() { }
        public virtual void SyncPlacementDelayed(bool forced = false) { }
    }
}
#endif

