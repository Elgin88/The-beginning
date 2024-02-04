using UnityEditor;
using UnityEngine;

namespace TerraUnity.Runtime.UI
{
    [CustomEditor(typeof(GPUInstanceLayer))]
    public class GPUInstanceLayerEditor : TBrushEditorGPU
    {
        // Generic Parameters
        private float m_LastEditorUpdateTime;
        private bool updateChanges = false;
        private bool syncChanges = false;
        private float UIDelay = 2f;
        private bool isChanged = false;
        //private bool isUpdated = false;

        // Layer Classes
        private TScatterParams parameters;
        private SerializedObject serializedAsset;

        // Layer Parameters
        private int selectionIndexPlacement = 0;
        private string[] waterPlacementMode = new string[] { "BYPASS", "UNDER WATER", "ON WATER" };

        // Foldouts
        bool prefabFold = false;
        bool distanceFold = false;
        bool shadowsFold = false;
        bool waterFold = false;
        bool placementFold = false;
        bool rotationFold = false;
        bool positionFold = false;
        bool scaleFold = false;
        bool slopeFold = false;
        bool heightFold = false;
        bool offsetFold = false;
        bool layerFold = false;
        bool occlusionCullingFold = false;
        bool renderingVariationFold = false;
        bool dataFold = false;

        // Serialized Properties
        SerializedProperty prefab;
        SerializedProperty averageDistance;
        SerializedProperty LODMultiplier;
        SerializedProperty LODGroupNotDetected;
        SerializedProperty maxDistance;
        SerializedProperty shadowCastMode;
        SerializedProperty receiveShadows;
        SerializedProperty bypassLake;
        SerializedProperty underLake;
        SerializedProperty underLakeMask;
        SerializedProperty onLake;
        SerializedProperty seedNo;
        SerializedProperty frustumMultiplier;
        SerializedProperty checkBoundingBox;
        SerializedProperty getSurfaceAngle;
        SerializedProperty lock90DegreeRotation;
        SerializedProperty lockYRotation;
        SerializedProperty minRotationRange;
        SerializedProperty maxRotationRange;
        SerializedProperty positionVariation;
        SerializedProperty scale;
        SerializedProperty minScale;
        SerializedProperty maxScale;
        SerializedProperty minAllowedAngle;
        SerializedProperty maxAllowedAngle;
        SerializedProperty minAllowedHeight;
        SerializedProperty maxAllowedHeight;
        SerializedProperty positionOffset;
        SerializedProperty rotationOffset;
        SerializedProperty unityLayerName;
        SerializedProperty unityLayerMask;
        SerializedProperty occlusionCulling;
        SerializedProperty randomColors;
        SerializedProperty colorGradient;
        SerializedProperty randomColorsCount;
        //SerializedProperty patchDataFile;


        // Events
        //--------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
            if (WT == null) WT = (GPUInstanceLayer)target;
            if (parameters == null) parameters = WT.transform.GetChild(0).GetComponent<TScatterParams>();
            if (serializedAsset == null) serializedAsset = new SerializedObject(parameters);

            prefab = serializedAsset.FindProperty("prefab");
            averageDistance = serializedAsset.FindProperty("averageDistance");
            LODMultiplier = serializedAsset.FindProperty("LODMultiplier");
            LODGroupNotDetected = serializedAsset.FindProperty("LODGroupNotDetected");
            maxDistance = serializedAsset.FindProperty("maxDistance");
            shadowCastMode = serializedAsset.FindProperty("shadowCastMode");
            receiveShadows = serializedAsset.FindProperty("receiveShadows");
            bypassLake = serializedAsset.FindProperty("bypassLake");
            underLake = serializedAsset.FindProperty("underLake");
            underLakeMask = serializedAsset.FindProperty("underLakeMask");
            onLake = serializedAsset.FindProperty("onLake");
            seedNo = serializedAsset.FindProperty("seedNo");
            frustumMultiplier = serializedAsset.FindProperty("frustumMultiplier");
            checkBoundingBox = serializedAsset.FindProperty("checkBoundingBox");
            getSurfaceAngle = serializedAsset.FindProperty("getSurfaceAngle");
            lock90DegreeRotation = serializedAsset.FindProperty("lock90DegreeRotation");
            lockYRotation = serializedAsset.FindProperty("lockYRotation");
            minRotationRange = serializedAsset.FindProperty("minRotationRange");
            maxRotationRange = serializedAsset.FindProperty("maxRotationRange");
            positionVariation = serializedAsset.FindProperty("positionVariation");
            scale = serializedAsset.FindProperty("scale");
            minScale = serializedAsset.FindProperty("minScale");
            maxScale = serializedAsset.FindProperty("maxScale");
            minAllowedAngle = serializedAsset.FindProperty("minAllowedAngle");
            maxAllowedAngle = serializedAsset.FindProperty("maxAllowedAngle");
            minAllowedHeight = serializedAsset.FindProperty("minAllowedHeight");
            maxAllowedHeight = serializedAsset.FindProperty("maxAllowedHeight");
            positionOffset = serializedAsset.FindProperty("positionOffset");
            rotationOffset = serializedAsset.FindProperty("rotationOffset");
            unityLayerName = serializedAsset.FindProperty("unityLayerName");
            unityLayerMask = serializedAsset.FindProperty("unityLayerMask");
            occlusionCulling = serializedAsset.FindProperty("occlusionCulling");
            randomColors = serializedAsset.FindProperty("randomColors");
            colorGradient = serializedAsset.FindProperty("colorGradient");
            randomColorsCount = serializedAsset.FindProperty("randomColorsCount");
            //patchDataFile = serializedAsset.FindProperty("patchDataFile");

            ModelPreviewUIRuntime.InitPreview(prefab.objectReferenceValue);

            if (parameters.terrain != null)
            {
                int terrainLayersCount = parameters.terrain.terrainData.terrainLayers.Length;

                if (parameters.exclusionOpacities != null && parameters.exclusionOpacities.Length != 0 && parameters.exclusionOpacities.Length == terrainLayersCount)
                {
                    WT.exclusion = new float[terrainLayersCount];

                    for (int i = 0; i < parameters.exclusionOpacities.Length; i++)
                        WT.exclusion[i] = parameters.exclusionOpacities[i];
                }
            }

            Undo.undoRedoPerformed += UndoRedoPerformed;

            parameters.LoadMaskData(true);
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            ModelPreviewUIRuntime.DestroyPreviewEditor();
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        protected virtual void OnEditorUpdate()
        {
            if (updateChanges && Time.realtimeSinceStartup - m_LastEditorUpdateTime > UIDelay)
            {
                UpdatePlacement();
                updateChanges = false;
            }

            if (syncChanges && Time.realtimeSinceStartup - m_LastEditorUpdateTime > UIDelay)
            {
                SyncPlacement();
                syncChanges = false;
            }
        }

        private void UndoRedoPerformed()
        {
            UpdateSerializedObject();

            //if (WorldToolsParams.isEditMode && parameters.undoMode == 0)
            //{
            //    if (isUpdated)
            //        UpdatePlacement();
            //    else
            //        SyncPlacement();
            //    
            //    Debug.Log("GPULayerEditor");
            //}
            //else
            //{
            //    SceneView.RepaintAll();
            //    SceneManagement.MarkSceneDirty();
            //}
        }


        // Generic Methods
        //--------------------------------------------------------------------------------------------------------------------------------------

        // Needs regeneration of patches
        public override void UpdatePlacement()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            parameters.UpdateLayer();
            //LODGroupNotDetected = serializedAsset.FindProperty("LODGroupNotDetected");
            SceneManagement.MarkSceneDirty();
            isChanged = false;
            //isUpdated = true;
        }

        // Only updates instance properties without regeneration
        public override void SyncPlacement()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            parameters.SyncLayer(null, true);
            SceneManagement.MarkSceneDirty();
            isChanged = false;
            //isUpdated = false;
        }

        public override void UpdatePlacementDelayed(bool forced = false)
        {
            if (forced)
            {
                m_LastEditorUpdateTime = Time.realtimeSinceStartup;
                updateChanges = true;
            }
            else
                isChanged = true;
        }

        public override void SyncPlacementDelayed(bool forced = false)
        {
            if (forced)
            {
                m_LastEditorUpdateTime = Time.realtimeSinceStartup;
                syncChanges = true;
            }
            else
                isChanged = true;
        }

        private void UpdateEditorOnly()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            parameters.SetCullingLODEditor(true);
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
        }

        private void UpdateMaterialColors()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            parameters.UpdateRandomColors();
            parameters.SetCullingLODEditor(true);
            SceneView.RepaintAll();
        }

        private void UpdateSerializedObject()
        {
            //TODO: Check the following lines to avoid errors!
            if (this == null) return;
            if (serializedAsset != null) serializedAsset.Update();
            if (serializedObject != null) serializedObject.Update();
        }

        private void UnfoldOtherSections(ref bool activeFoldout)
        {
            prefabFold = false;
            distanceFold = false;
            shadowsFold = false;
            waterFold = false;
            placementFold = false;
            rotationFold = false;
            positionFold = false;
            scaleFold = false;
            slopeFold = false;
            heightFold = false;
            offsetFold = false;
            layerFold = false;
            occlusionCullingFold = false;
            renderingVariationFold = false;
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

            if (prefab.objectReferenceValue != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                style = new GUIStyle(EditorStyles.helpBox);
                ModelPreviewUIRuntime.ModelPreview(prefab.objectReferenceValue, style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (parameters.instanceCount > 0)
                THelpersUIRuntime.GUI_HelpBox("PLACED MODELS:  " + parameters.instanceCount.ToString(), MessageType.None, 0, new Color(0.1f, 1, 0.3f, 1f));
            else
                THelpersUIRuntime.GUI_HelpBox("THERE ARE NO PLACED MODELS BASED ON LAYER CONDITIONS!", MessageType.None, 0, new Color(1f, 0.1F, 0.3f, 1f));

            if (isChanged)
                THelpersUIRuntime.GUI_HelpBox("PRESS \"UPDATE CHANGES\" BUTTON AT BELOW TO UPDATE LAYER!", MessageType.Warning, 10);
            else
                THelpersUIRuntime.GUI_HelpBox("LAYER IS UP TO DATE! CHANGE PARAMETERS TO EDIT LAYER!", MessageType.Info, 10, Color.clear, Color.clear, Color.clear);

            GUILayout.Space(20);

            prefabFold = THelpersUIRuntime.Foldout("PREFAB", ref prefabFold);

            if (prefabFold)
            {
                UnfoldOtherSections(ref prefabFold);
                GUILayout.Space(10);
                THelpersUIRuntime.GUI_HelpBox("YOU CAN DRAG & DROP A PREFAB TO REPLACE MODEL INSTANCE", MessageType.Info, -10);
                GUILayout.Space(5);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prefab, new GUIContent("Prefab", "Prefab used for placement"));
                if (EditorGUI.EndChangeCheck())
                {
                    ModelPreviewUIRuntime.InitPreview(prefab.objectReferenceValue);
                    UpdatePlacement();
                    serializedObject.Update();
                    serializedAsset.Update();
                }
            }

            distanceFold = THelpersUIRuntime.Foldout("RENDERING DISTANCE & DENSITY", ref distanceFold);

            if (distanceFold)
            {
                UnfoldOtherSections(ref distanceFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(averageDistance, new GUIContent("Models Distance", "Distance between placed instances"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();

                if (LODGroupNotDetected.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(maxDistance, new GUIContent("Max Distance", "Max Distance for rendering"));
                    if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(LODMultiplier, new GUIContent("LOD Bias", "LOD Bias for rendering instances"));
                    if (EditorGUI.EndChangeCheck()) UpdateEditorOnly();
                }
            }

            shadowsFold = THelpersUIRuntime.Foldout("SHADOWS", ref shadowsFold);

            if (shadowsFold)
            {
                UnfoldOtherSections(ref shadowsFold);
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(shadowCastMode, new GUIContent("Cast Shadows", "Cast shadows on models?"));
                EditorGUILayout.PropertyField(receiveShadows, new GUIContent("Receive Shadows", "Receive Shadows on models?"));
            }

            waterFold = THelpersUIRuntime.Foldout("WATER AREAS", ref waterFold);

            if (waterFold)
            {
                UnfoldOtherSections(ref waterFold);
                GUILayout.Space(10);
                THelpersUIRuntime.GUI_HelpBox("PLACEMENT ON WATER SURFACE", MessageType.None, -10);

                if (bypassLake.boolValue)
                    selectionIndexPlacement = 0;
                else if (underLake.boolValue)
                    selectionIndexPlacement = 1;
                else if (onLake.boolValue)
                    selectionIndexPlacement = 2;

                EditorGUI.BeginChangeCheck();
                style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
                selectionIndexPlacement = THelpersUIRuntime.GUI_SelectionGrid(selectionIndexPlacement, waterPlacementMode, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    if (selectionIndexPlacement == 0)
                    {
                        bypassLake.boolValue = true;
                        underLake.boolValue = false;
                        underLakeMask.boolValue = true;
                        onLake.boolValue = false;
                    }
                    else if (selectionIndexPlacement == 1)
                    {
                        bypassLake.boolValue = false;
                        underLake.boolValue = true;
                        underLakeMask.boolValue = true;
                        onLake.boolValue = false;
                    }
                    else if (selectionIndexPlacement == 2)
                    {
                        bypassLake.boolValue = false;
                        underLake.boolValue = false;
                        underLakeMask.boolValue = true;
                        onLake.boolValue = true;
                    }

                    //UpdatePlacement();
                    SyncPlacement();
                }
            }

            placementFold = THelpersUIRuntime.Foldout("PLACEMENT", ref placementFold);

            if (placementFold)
            {
                UnfoldOtherSections(ref placementFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                EditorGUILayout.PropertyField(seedNo, new GUIContent("SEED #", "Seed number for unique procedural values"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(frustumMultiplier, new GUIContent("Frustum Multiplier", "Camera Frustum range for wider frustum range"));
                if (EditorGUI.EndChangeCheck()) UpdateEditorOnly();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(checkBoundingBox, new GUIContent("Check Bounding Box", "Check model's bounding box for placement?\nIf checked, placement will only be applied when the whole model is inside masked areas\nIf unchecked, mask is only checked in pivot position of the model"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacement();
                    SyncPlacement();
            }

            rotationFold = THelpersUIRuntime.Foldout("ROTATION", ref rotationFold);

            if (rotationFold)
            {
                UnfoldOtherSections(ref rotationFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(getSurfaceAngle, new GUIContent("SURFACE ROTATION", "Rotation is calculated from the underlying surface angle"));
                EditorGUILayout.PropertyField(lock90DegreeRotation, new GUIContent("90° ROTATION", "Rotation is only performed in 90 degree multiplies 0°, 90°, 180° & 270°"));
                if (!getSurfaceAngle.boolValue) EditorGUILayout.PropertyField(lockYRotation, new GUIContent("HORIZONTAL ROTATION", "Rotation is locked in horizontal rotation axis (Y) only"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacement();
                    SyncPlacement();

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(10);
                if (!lock90DegreeRotation.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minRotationRange, new GUIContent("MIN. ROTATION RANGE", "Minimum rotation in degrees for object"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (minRotationRange.floatValue > maxRotationRange.floatValue)
                        {
                            ShowNotification("Min. value should be lower than Max. value!");
                            minRotationRange.floatValue = maxRotationRange.floatValue - 1f;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(maxRotationRange, new GUIContent("MAX. ROTATION RANGE", "Maximum rotation in degrees for object"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (maxRotationRange.floatValue < minRotationRange.floatValue)
                        {
                            ShowNotification("Max. value should be higher than Min. value!");
                            maxRotationRange.floatValue = minRotationRange.floatValue + 1f;
                        }
                    }
                }
                if (!parameters.lock90DegreeRotation) THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("ROTATION RANGE", "Minimum & Maximum rotation in degrees for object"), ref parameters.minRotationRange, ref parameters.maxRotationRange, 0f, 359f);
                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
            }

            positionFold = THelpersUIRuntime.Foldout("POSITION", ref positionFold);

            if (positionFold)
            {
                UnfoldOtherSections(ref positionFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(positionVariation, new GUIContent("RANDOM POSITION", "Percentage of the random position for instance"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
            }

            scaleFold = THelpersUIRuntime.Foldout("SCALE", ref scaleFold);

            if (scaleFold)
            {
                UnfoldOtherSections(ref scaleFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(scale, new GUIContent("SCALE", "Original scale of the instance"));
                GUILayout.Space(10);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(minScale, new GUIContent("MIN. SCALE RANGE", "Minimum random scale variation which would multiply to SCALE"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (minScale.floatValue > maxScale.floatValue)
                    {
                        ShowNotification("Min. value should be lower than Max. value!");
                        minScale.floatValue = maxScale.floatValue - 1f;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxScale, new GUIContent("MAX. SCALE RANGE", "Maximum random scale variation which would multiply to SCALE"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxScale.floatValue < minScale.floatValue)
                    {
                        ShowNotification("Max. value should be higher than Min. value!");
                        maxScale.floatValue = minScale.floatValue + 1f;
                    }
                }

                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
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

                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
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
                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
            }

            offsetFold = THelpersUIRuntime.Foldout("OFFSET", ref offsetFold);

            if (offsetFold)
            {
                UnfoldOtherSections(ref offsetFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(positionOffset, new GUIContent("POSITION OFFSET", "Placement offset of the object in 3 dimensions"));
                EditorGUILayout.PropertyField(rotationOffset, new GUIContent("ROTATION OFFSET", "Rotation offset of the object in 3 dimensions"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacementDelayed();
                    SyncPlacementDelayed();
            }

            layerFold = THelpersUIRuntime.Foldout("LAYERS", ref layerFold);

            if (layerFold)
            {
                UnfoldOtherSections(ref layerFold);
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                parameters.unityLayerName = THelpersUIRuntime.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
                unityLayerName.stringValue = parameters.unityLayerName;
                parameters.unityLayerMask = THelpersUIRuntime.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), parameters.unityLayerMask);
                unityLayerMask.intValue = parameters.unityLayerMask;
                if (EditorGUI.EndChangeCheck()) //UpdatePlacement();
                    SyncPlacement();
            }

            occlusionCullingFold = THelpersUIRuntime.Foldout("OCCLUSION CULLING", ref occlusionCullingFold);

            if (occlusionCullingFold)
            {
                UnfoldOtherSections(ref occlusionCullingFold);
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(occlusionCulling, new GUIContent("OCCLUSION CULLING", "If selected, layer will be checked for Real-time Occlusion Culling against all objects with Static flag of \"Occluder Static\" in Game mode"));
            }

            renderingVariationFold = THelpersUIRuntime.Foldout("RENDERING VARIATIONS (COLOR / TRANSPARENCY)", ref renderingVariationFold);

            if (renderingVariationFold)
            {
                UnfoldOtherSections(ref renderingVariationFold);
                GUILayout.Space(20);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(randomColors, new GUIContent("ACTIVATE FEATURE", "By enabling this feature, you can add color & alpha variations for models. Note that it may affect performance if enabled!"));
                if (EditorGUI.EndChangeCheck()) //UpdatePlacement();
                    SyncPlacement();

                if (randomColors.boolValue == true)
                {
                    GUILayout.Space(20);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(randomColorsCount, new GUIContent("VARIATIONS COUNT", "Number of random generated variants for models!"));
                    THelpersUIRuntime.GUI_HelpBox("Higher values affect performance more due to less GPU batches in favor of improving realism and variation to placed models!", MessageType.Warning, -5);
                    GUILayout.Space(20);
                    EditorGUILayout.PropertyField(colorGradient, new GUIContent("COLOR/ALPHA GRADIENT", "You can add color and alpha ranges to each instance for the best rendering variations"));
                    THelpersUIRuntime.GUI_HelpBox("Use color keys to define color variations and alpha keys to randomize transparency if available (e.g. Dry trees with yellow-ish colors and less alpha for fewer leaves along with healthy trees with greenish colors and full alpha for more leaves).", MessageType.Info, -5);
                    if (EditorGUI.EndChangeCheck()) UpdateMaterialColors();
                }
            }

            dataFold = THelpersUIRuntime.Foldout("DATA", ref dataFold);

            if (dataFold)
            {
                UnfoldOtherSections(ref dataFold);
                THelpersUIRuntime.GUI_ObjectField(new GUIContent("Patch File", "Patch Data file which holds layer's patch data"), parameters.patchDataFile, typeof(UnityEngine.Object), null, 10);
                THelpersUIRuntime.GUI_ObjectField(new GUIContent("Mask File", "Mask file which holds layer's mask data"), parameters.maskDataFile, typeof(UnityEngine.Object), null, -5);

                //GUILayout.Space(10);
                //THelpersUIRuntime.GUI_HelpBox("YOU CAN DRAG & DROP A DATA FILE TO UPDATE MODELS PLACEMENT", MessageType.Info, -10);
                //GUILayout.Space(5);
                //EditorGUI.BeginChangeCheck();
                //EditorGUILayout.PropertyField(patchDataFile, new GUIContent("Data File", "Data file which holds layer's patch data"));
                //if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            }

            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 128; style.fixedHeight = 20;

            if (isChanged)
                THelpersUIRuntime.GUI_Button(new GUIContent("UPDATE CHANGES", "Update changes for placement"), style, UpdatePlacement, 40, 0, Color.red);
            else
                THelpersUIRuntime.GUI_Button(new GUIContent("FORCE UPDATE", "Force update placement"), style, UpdatePlacement, 40);

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

