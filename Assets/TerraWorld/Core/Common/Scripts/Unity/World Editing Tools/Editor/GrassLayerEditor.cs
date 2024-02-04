using UnityEditor;
using UnityEngine;
using Mewlist.MassiveGrass;

namespace TerraUnity.Runtime.UI
{
    [CustomEditor(typeof(GrassLayer))]
    public class GrassLayerEditor : TBrushEditorGrass
    {
        // Generic Parameters
        private float m_LastEditorUpdateTime;
        private bool applyChanges = false;
        private float UIDelay = 2f;

        // Layer Classes
        private SerializedObject serializedAsset;

        // Layer Parameters
        private int selectionIndexPlacement = 0;
        private string[] waterPlacementMode = new string[] { "BYPASS", "UNDER WATER", "ON WATER" };

        // Foldouts
        bool materialFold = false;
        bool modelTypeFold = false;
        bool slantFold = false;
        bool distanceFold = false;
        bool renderFold = false;
        bool placementFold = false;
        bool scaleFold = false;
        bool slopeFold = false;
        bool heightFold = false;
        bool offsetFold = false;
        bool layerFold = false;
        bool dataFold = false;

        // Serialized Properties
        SerializedProperty Material;
        SerializedProperty Seed;
        SerializedProperty builderType;
        SerializedProperty Mesh;
        SerializedProperty Scale;
        SerializedProperty minAllowedAngle;
        SerializedProperty maxAllowedAngle;
        SerializedProperty minAllowedHeight;
        SerializedProperty maxAllowedHeight;
        SerializedProperty Slant;
        SerializedProperty Radius;
        SerializedProperty AmountPerBlock;
        SerializedProperty GridSize;
        SerializedProperty MaskDamping;
        SerializedProperty NormalType;
        SerializedProperty GroundOffset;
        SerializedProperty CastShadows;
        SerializedProperty layerBasedPlacement;
        SerializedProperty bypassWater;
        SerializedProperty underWater;
        SerializedProperty onWater;
        SerializedProperty unityLayerMask;

        // Events
        //--------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
            if (GL == null) GL = (GrassLayer)target;
            if (serializedAsset == null) serializedAsset = new SerializedObject(GL.MGP);

            Material = serializedAsset.FindProperty("Material");
            Seed = serializedAsset.FindProperty("Seed");
            builderType = serializedAsset.FindProperty("BuilderType");
            Mesh = serializedAsset.FindProperty("Mesh");
            Scale = serializedAsset.FindProperty("Scale");
            minAllowedAngle = serializedAsset.FindProperty("minAllowedAngle");
            maxAllowedAngle = serializedAsset.FindProperty("maxAllowedAngle");
            minAllowedHeight = serializedAsset.FindProperty("minAllowedHeight");
            maxAllowedHeight = serializedAsset.FindProperty("maxAllowedHeight");
            Slant = serializedAsset.FindProperty("Slant");
            Radius = serializedAsset.FindProperty("Radius");
            AmountPerBlock = serializedAsset.FindProperty("AmountPerBlock");
            GridSize = serializedAsset.FindProperty("GridSize");
            MaskDamping = serializedAsset.FindProperty("brushDamping");
            NormalType = serializedAsset.FindProperty("NormalType");
            GroundOffset = serializedAsset.FindProperty("GroundOffset");
            CastShadows = serializedAsset.FindProperty("CastShadows");
            layerBasedPlacement = serializedAsset.FindProperty("layerBasedPlacement");
            bypassWater = serializedAsset.FindProperty("bypassWater");
            underWater = serializedAsset.FindProperty("underWater");
            onWater = serializedAsset.FindProperty("onWater");
            unityLayerMask = serializedAsset.FindProperty("unityLayerMask");

            ModelPreviewUIRuntime.InitPreview(Material.objectReferenceValue);

            Terrain terrain = null;
            TTerraWorldTerrainManager[] TTM = FindObjectsOfType<TTerraWorldTerrainManager>();
            if (TTM != null && TTM.Length > 0 && TTM[0] != null) terrain = TTM[0].gameObject.GetComponent<Terrain>();

            if (terrain != null)
            {
                int terrainLayersCount = terrain.terrainData.terrainLayers.Length;

                if (GL.MGP.exclusionOpacities != null && GL.MGP.exclusionOpacities.Length != 0 && GL.MGP.exclusionOpacities.Length == terrainLayersCount)
                {
                    GL.exclusion = new float[terrainLayersCount];

                    for (int i = 0; i < GL.MGP.exclusionOpacities.Length; i++)
                        GL.exclusion[i] = GL.MGP.exclusionOpacities[i];
                }
            }

            Undo.undoRedoPerformed += UndoRedoPerformed;

            if (GL.MGP.maskDataFast == null)
                GL.LoadMaskData(true);
            else
                GL.LoadMaskData(true, null, false, null, new Vector2(), false);
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            ModelPreviewUIRuntime.DestroyPreviewEditor();
            //TBrushFunctions.SetMaxParallelJobs(true);
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual void OnEditorUpdate()
        {
            if (applyChanges && Time.realtimeSinceStartup - m_LastEditorUpdateTime > UIDelay)
            {
                UpdatePlacement();
                applyChanges = false;
            }
        }

        private void UndoRedoPerformed()
        {
            UpdateSerializedObject();

            //if (WorldToolsParams.isEditMode && GL.MGP.undoMode == 0)
            //{
            //    UpdatePlacement();
            //    
            //    Debug.Log("GrassLayerEditor");
            //}
            //else
            //{
            //    SceneView.RepaintAll();
            //    SceneManagement.MarkSceneDirty();
            //}
        }


        // Generic Methods
        //--------------------------------------------------------------------------------------------------------------------------------------

        public override void UpdatePlacement()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();

            if (GL.MGP.BuilderType == BuilderType.FromMesh && GL.MGP.Mesh == null)
                ShowNotification("Mesh is missing in MODEL TYPE section!");

            GL.UpdateLayer();

            //GL.MGP.isActive = true;

            SceneManagement.MarkSceneDirty();
            SceneView.RepaintAll();
        }

        public override void UpdatePlacementDelayed()
        {
            m_LastEditorUpdateTime = Time.realtimeSinceStartup;
            applyChanges = true;
        }

        private void UpdateSerializedObject()
        {
            if (serializedAsset != null) serializedAsset.Update();
            if (serializedObject != null) serializedObject.Update();
        }

        private void UnfoldOtherSections(ref bool activeFoldout)
        {
            materialFold = false;
            modelTypeFold = false;
            slantFold = false;
            distanceFold = false;
            renderFold = false;
            placementFold = false;
            scaleFold = false;
            slopeFold = false;
            heightFold = false;
            offsetFold = false;
            layerFold = false;
            dataFold = false;
            activeFoldout = true;
        }


        // Layer Specific UI
        //--------------------------------------------------------------------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            //UpdateSerializedObject();
            if (serializedObject != null) serializedObject.Update();

            GUILayout.Space(20);
            MaskEditorGUI();

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            GUILayout.Space(25);

            if (Material.objectReferenceValue != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                style = new GUIStyle(EditorStyles.helpBox);
                ModelPreviewUIRuntime.ModelPreview(Material.objectReferenceValue, style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            EditorGUI.BeginDisabledGroup(!GL.MGP.isActive);

            materialFold = THelpersUIRuntime.Foldout("MATERIAL", ref materialFold);

            if (materialFold)
            {
                UnfoldOtherSections(ref materialFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(Material, new GUIContent("MATERIAL", "Material used for rendering"));
                if (EditorGUI.EndChangeCheck())
                {
                    ModelPreviewUIRuntime.InitPreview(Material.objectReferenceValue);
                    UpdatePlacement();
                }
            }

            placementFold = THelpersUIRuntime.Foldout("PLACEMENT", ref placementFold);

            if (placementFold)
            {
                UnfoldOtherSections(ref placementFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(Seed, new GUIContent("SEED#", "Seed number for unique procedural values"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            modelTypeFold = THelpersUIRuntime.Foldout("MODEL TYPE", ref modelTypeFold);

            if (modelTypeFold)
            {
                UnfoldOtherSections(ref modelTypeFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(builderType, new GUIContent("MODEL TYPE", "Select between Quad (Billboards) and 3D Mesh for rendering"));

                if (GL.MGP.BuilderType == BuilderType.FromMesh)
                {
                    EditorGUILayout.PropertyField(Mesh, new GUIContent("MESH", "Mesh used for grass rendering"));
                    if (Mesh == null) THelpersUIRuntime.GUI_Alert();
                }

                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            scaleFold = THelpersUIRuntime.Foldout("SCALE", ref scaleFold);

            if (scaleFold)
            {
                UnfoldOtherSections(ref scaleFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(Scale, new GUIContent("SCALE", "Scale of grass/plant blades"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            slopeFold = THelpersUIRuntime.Foldout("SLOPE RANGE", ref slopeFold);

            if (slopeFold)
            {
                UnfoldOtherSections(ref slopeFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(minAllowedAngle, new GUIContent("MIN. SLOPE RANGE", "Minimum slope in degrees compared to horizon level for object placement"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (minAllowedAngle.floatValue > maxAllowedAngle.floatValue)
                    {
                        ShowNotification("Min. value should be lower than Max. value!");
                        minAllowedAngle.floatValue = maxAllowedAngle.floatValue - 1f;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxAllowedAngle, new GUIContent("MAX. SLOPE RANGE", "Maximum slope in degrees compared to horizon level for object placement"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxAllowedAngle.floatValue < minAllowedAngle.floatValue)
                    {
                        ShowNotification("Max. value should be higher than Min. value!");
                        maxAllowedAngle.floatValue = minAllowedAngle.floatValue + 1f;
                    }
                }

                //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref parameters.minAllowedAngle, ref parameters.maxAllowedAngle, 0f, 90f);
                if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            }

            heightFold = THelpersUIRuntime.Foldout("HEIGHT RANGE", ref heightFold);

            if (heightFold)
            {
                UnfoldOtherSections(ref heightFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(minAllowedHeight, new GUIContent("MIN. HEIGHT RANGE", "Minimum height in meters (units) for object placement"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (minAllowedHeight.floatValue > maxAllowedHeight.floatValue)
                    {
                        ShowNotification("Min. value should be lower than Max. value!");
                        minAllowedHeight.floatValue = maxAllowedHeight.floatValue - 1f;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxAllowedHeight, new GUIContent("MAX. HEIGHT RANGE", "Maximum height in meters (units) for object placement"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxAllowedHeight.floatValue < minAllowedHeight.floatValue)
                    {
                        ShowNotification("Max. value should be higher than Min. value!");
                        maxAllowedHeight.floatValue = minAllowedHeight.floatValue + 1f;
                    }
                }

                //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref parameters.minAllowedHeight, ref parameters.maxAllowedHeight, -100000f, 100000f);
                if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            }

            slantFold = THelpersUIRuntime.Foldout("BLADES SLANT", ref slantFold);

            if (slantFold)
            {
                UnfoldOtherSections(ref slantFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(Slant, new GUIContent("SLANT", "Slant amount for grass/plant blades"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            distanceFold = THelpersUIRuntime.Foldout("RENDERING DISTANCE & DENSITY", ref distanceFold);

            if (distanceFold)
            {
                UnfoldOtherSections(ref distanceFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(Radius, new GUIContent("RENDERING DISTANCE", "Rendering Distance to draw vegetation"));
                EditorGUILayout.PropertyField(AmountPerBlock, new GUIContent("AMOUNT PER BLOCK", "Amount of instances per block"));
                EditorGUILayout.PropertyField(GridSize, new GUIContent("GRID SIZE", "Grid Size for patches"));
                EditorGUILayout.PropertyField(MaskDamping, new GUIContent("MASK DAMPING", "Damping of placement to also attack at mask borders"));
                THelpersUIRuntime.GUI_HelpBox("INCREASE DAMPING VALUE FOR MORE NATURAL DISTRIBUTION OF INSTANCES SUITED FOR LARGER WORLDS", MessageType.Info, -5);
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            //EditorGUILayout.PropertyField(AlphaMapThreshold, new GUIContent("MASK THRESOLD", "Masked area thresold"));
            //EditorGUILayout.PropertyField(DensityFactor, new GUIContent("MASK DENSITY", "Masked area density"));
            GL.MGP.AlphaMapThreshold = 0.5f;
            GL.MGP.DensityFactor = 0.5f;

            renderFold = THelpersUIRuntime.Foldout("RENDER SETTINGS", ref renderFold);

            if (renderFold)
            {
                UnfoldOtherSections(ref renderFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(CastShadows, new GUIContent("SHADOW CASTING", "Shadow Casting mode for each instance"));
                EditorGUILayout.PropertyField(NormalType, new GUIContent("NORMAL TYPE", "Normal type for rendering"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            offsetFold = THelpersUIRuntime.Foldout("OFFSET", ref offsetFold);

            if (offsetFold)
            {
                UnfoldOtherSections(ref offsetFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(GroundOffset, new GUIContent("GROUND OFFSET", "Ground offset in vertical direction"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            layerFold = THelpersUIRuntime.Foldout("LAYERS", ref layerFold);

            if (layerFold)
            {
                UnfoldOtherSections(ref layerFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(layerBasedPlacement, new GUIContent("CHECK LAYERS", "Raycast against underlying surfaces to decide on placement based on detected layers"));

                if (layerBasedPlacement.boolValue)
                {
                    THelpersUIRuntime.GUI_HelpBox("THIS FEATURE WILL AFFECT PERFORMANCE ON DENSER LAYERS", MessageType.Warning);
                    THelpersUIRuntime.GUI_HelpBox("PLACEMENT ON WATER", MessageType.None, 20);

                    if (bypassWater.boolValue)
                        selectionIndexPlacement = 0;
                    else if (underWater.boolValue)
                        selectionIndexPlacement = 1;
                    else if (onWater.boolValue)
                        selectionIndexPlacement = 2;

                    EditorGUI.BeginChangeCheck();
                    style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
                    selectionIndexPlacement = THelpersUIRuntime.GUI_SelectionGrid(selectionIndexPlacement, waterPlacementMode, style, -10);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectionIndexPlacement == 0)
                        {
                            bypassWater.boolValue = true;
                            underWater.boolValue = false;
                            onWater.boolValue = false;
                        }
                        else if (selectionIndexPlacement == 1)
                        {
                            bypassWater.boolValue = false;
                            underWater.boolValue = true;
                            onWater.boolValue = false;
                        }
                        else if (selectionIndexPlacement == 2)
                        {
                            bypassWater.boolValue = false;
                            underWater.boolValue = false;
                            onWater.boolValue = true;
                        }
                    }

                    GL.MGP.unityLayerMask = THelpersUIRuntime.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), GL.MGP.unityLayerMask, 10);
                    unityLayerMask.intValue = GL.MGP.unityLayerMask;
                }

                if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            dataFold = THelpersUIRuntime.Foldout("DATA", ref dataFold);

            if (dataFold)
            {
                UnfoldOtherSections(ref dataFold);
                THelpersUIRuntime.GUI_ObjectField(new GUIContent("Mask File", "Mask file which holds layer's mask data"), GL.MGP.maskDataFile, typeof(UnityEngine.Object), null, 10);
            }

            //TODO: Add Mask falloff options

            EditorGUI.EndDisabledGroup();

            style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 128;
            style.fixedHeight = 20;
            THelpersUIRuntime.GUI_Button(new GUIContent("FORCE UPDATE", "Force update placement"), style, UpdatePlacement, 40);

            //TODO: Display instances count updated only when synced with new changes
            //THelpersUIRuntime.GUI_HelpBox(parameters._patches.Length.ToString(), MessageType.None, 20);

            GUILayout.Space(30);

            //serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowNotification(string message)
        {
            foreach (SceneView scene in SceneView.sceneViews)
                scene.ShowNotification(new GUIContent(message, ""));
        }
    }
}

