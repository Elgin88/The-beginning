#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TerraUnity.Edittime;
using TerraUnity.Runtime;
using TerraUnity.Utils;
#if TERRAWORLD_XPRO
using XNodeEditor;
#endif

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace TerraUnity.Edittime.UI
{
    public class TerraWorld : EditorWindow
    {
        private static DateTime startTerraworldTime = DateTime.Now;

        public static List<double[]> _coords;
        public static List<string> _locations;

#if TERRAWORLD_PRO
        [MenuItem("Tools/TerraUnity/TerraWorld Pro", false, 1)]
#endif
        static void Init()
        {

#if TERRAWORLD_PRO
            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "TerraWorldPro");
#else
            TTerraWorld.FeedbackEvent(EventCategory.UX , EventAction.Click, EventAction.UX_Click_Menu, "TerraWorldLite", 0);
#endif
            TTerraWorld.FeedbackEvent(EventCategory.SoftwareInfo, EventAction.Version, TVersionController.MajorVersion.ToString() + "." + TVersionController.MinorVersion.ToString());

            TTerraWorld.FeedbackSystemInfo();
            TerraWorld window = (TerraWorld)GetWindow(typeof(TerraWorld));
            window.position = new Rect(5, 135, 480, 800);

#if TERRAWORLD_PRO
            window.titleContent = new GUIContent("TerraWorld Pro", "Real-World Level Designer");
#else
            window.titleContent = new GUIContent("TerraWorld Lite", "Real-World Level Designer");
#endif
        }

        public float windowWidth, windowHeight;
        int labelFontSize = 10;

        public enum ShowTabs
        {
            showArea,
            showHeightmap,
            showColormap,
            showBiomes,
            showRendering,
            showFX,
            showGlobal,
            showPlayer
        }
        public static ShowTabs activeTab = ShowTabs.showArea;

        //bool[] sectionActivated;
        Texture2D moduleIcon, moduleState;
        public Color enabledColor = Color.white;
        public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        public Color buttonColor = new Color(0.275f, 0.825f, 0.75f);
        public Color warningColor = new Color(0.8f, 0.35f, 0.15f);
        public Color wireColor = new Color(0.5f, 0.5f, 0.5f, 0.85f);
        public Color wireColorInProgress = new Color(0f, 0.35f, 0.65f, 0.8f); //new Color(0.25f, 0.8f, 0.8f, 1);

        Vector2 nodeSize = new Vector2(75, 60); //new Vector2(100, 75);
        Vector2 nodeSizeMaster = new Vector2(85, 85);
        Vector2 nodeSizeExternal = new Vector2(30, 30);
        float externalNodeSpace = 70;
        float progressBarHeight = 5;
        float headerSpace = 5; //150
        float moduleSpace = 10;
        float wireWidth = 3;

        Vector3 startPositionModule, endPositionModule, startTangentModule, endTangentModule;
        private float nodeSpace = 0;

        private Vector2 scrollPositionNode = Vector2.zero;
        private float targetScrollPos;
        private int capturedSecondValue;
        private float scrollSpeed = 8f;
        private bool isScrolling = false;
        private Vector2 scrollPositionSettings = Vector2.zero;
        private Rect lastRect;

        public static int mapWindowIsOpen = 0;

        int showSceneViewGUI = 0;

        //bool liveUpdate = false;
        Rect activeNodeRect = Rect.zero;
        //bool syncPaused = false;
        //private float lastEditorUpdateTime;
        //private float syncDelay = 4f;
        float maxRectPosY;
        Dictionary<TNode, string> moduleList1 = new Dictionary<TNode, string>();
        Dictionary<TNode, string> moduleList2 = new Dictionary<TNode, string>();
        int input1, input2;

        private TGraph activeGraph;
        private static ShowTabs lastTab;
        private static TNode lastNode;
        private Vector3 objectScale = Vector3.one;
        private Material material;
        private Mesh mesh;
        //private Texture2D nodeTexture;
        private List<GameObject> tempObjects;
        private List<GameObject> LODObjects;
        //private GameObject gameObjectWithCollider;
        private Texture2D diffuseTexture = null;
        private Texture2D normalmapTexture = null;
        private Texture2D maskmapTexture = null;
        private TerrainLayer terrainLayer = null;

        // Processors
        public HeightmapProcessors heightmapProcessors = HeightmapProcessors.Heightmap_Source;
        public ColormapProcessors colormapProcessors = ColormapProcessors.Satellite_Image_Source;

        // Masks
        public HeightmapMasks heightmapMasks = HeightmapMasks.Slope_Filter;
        public ColormapMasks colormapMasks = ColormapMasks.Color_Filter;
        public BiomeMasks biomeMasks = BiomeMasks.Biome_Type_Filter;

        // Scatters
        public BiomeScatters biomeScatters = BiomeScatters.Terrain_Tree_Scatter;

        // TerraMesh
        public BiomeMeshGenerators biomeMeshGenerators = BiomeMeshGenerators.Terrain_Mesh_Generator;

        //private bool showBounds = true;
        private float minRangeUI = 0;
        private float maxRangeUI = 1;

        //private bool showbounds { get { return TAreaPreview.BoundsVisible; } }

        //private WorldArea worldArea;
        //{
        //    get
        //    {
        //        return worldGraph.areaGraph.GetEntryNode();
        //    }
        //}

        private Dictionary<int, int> externalIDs;
        //public static TNode dirtyNode = null;
        private int branchCount = -1;
        private Dictionary<TNode, int> branches;

        private string[] _terrainLayerTitles = new string[] { "EXISTING", "NEW" };
        private string[] _colormapTitles = new string[] { " BAKE IMAGE ", " BAKE SIMPLIFIED IMAGE ", " FILL WITH COLOR " };
        private string[] _image2MaskTitles = new string[] { "TEXTURE NODE", "EXTERNAL IMAGE" };

        private Vector3 tempVector3D;
        private Vector2 tempVector2D;
        private Color tempColor;

        private string[] onOffSelection = new string[] { "ON", "OFF" };
        private string[] waterPlacementMode = new string[] { "BYPASS", "UNDER WATER", "ON WATER" };
        private string[] renderingMode = new string[] { "TERRAFORMER", "UNITY STANDARD" };
        private string[] manualAutoMode = new string[] { "MANUAL", "AUTO" };
        private string[] dayNightMode = new string[] { "BYPASS", "STATIC TIME", "DAY/NIGHT CYCLE" };
        //private string[] weatherMode = new string[] { "COLD", "WARM" };
        private string[] convertSelection = new string[] { "CONVERT", "BYPASS" };

        private static ListRequest listRequest;
        private static string searchPackageIdOrName;
        private static bool packageIsInstalled = false;

       // private static float creationProgress = 0;

        //private string[] presetNames;
        float terracePointMin = 0.0f;
        float terracePointMax = 1.0f;

        private string aboutText;
        private static bool projectIsSRP;

        private string incompatibleMaterialWarningText = "Features like \'SNOW\' & \'WIND\' are not available for material: ";
        private List<string> incompatibleMaterialNames;

        private GameObject addedTarget = null;
        private bool targetRemoved;
        private bool colliderDetected = false;
        private bool LODGroupDetected = false;

        private bool[] globalStates;
        private bool[] targetStates;
        public static bool[] toggles;

        AmbientOcclusion AO = null;
        private float verticalSlidersSpace;

        public int targetFrameRate = 60;

        private void InitGraphStates()
        {
            if (activeTab.Equals(ShowTabs.showArea))
                activeGraph = TTerraWorld.WorldGraph.areaGraph;
            else if (activeTab.Equals(ShowTabs.showHeightmap))
                activeGraph = TTerraWorld.WorldGraph.heightmapGraph;
            else if (activeTab.Equals(ShowTabs.showColormap))
                activeGraph = TTerraWorld.WorldGraph.colormapGraph;
            else if (activeTab.Equals(ShowTabs.showBiomes))
                activeGraph = TTerraWorld.WorldGraph.biomesGraph;
            else if (activeTab.Equals(ShowTabs.showRendering))
                activeGraph = TTerraWorld.WorldGraph.renderingGraph;
            else if (activeTab.Equals(ShowTabs.showFX))
                activeGraph = TTerraWorld.WorldGraph.FXGraph;
            else if (activeTab.Equals(ShowTabs.showPlayer))
                activeGraph = TTerraWorld.WorldGraph.playerGraph;

            TProjectSettings.ActiveGraphIndex = (int)activeTab;

            if (activeGraph != null)
                activeGraph.UpdateConnections();

#if TERRAWORLD_PRO
            //SceneSettingsManager.worldGraph = TTerraWorld.WorldGraph;
            //RenderingSettingsManager.worldGraph = TTerraWorld.WorldGraph;
#endif

            RemoveNotification();
            InteractiveTargets.GetPlayerTargets();
            //InitPlayerParams();
        }

        //private void InitPlayerParams()
        //{
        //    //PlayerInteractions.playerTargets = new List<GameObject>();
        //    PlayerInteractions.GetPlayerTargets();
        //}

        private void InitializeScene()
        {
            InitGraphStates();
            //InitSceneVisuals();
        }

        private void Initialize()
        {
            windowWidth = this.position.width;
            windowHeight = this.position.height;
            THelpersUI.windowWidth = position.width;
            THelpersUI.windowHeight = position.height;

            CheckProjectRenderingPipeline();
            TResourcesManager.LoadAllResources();
            LoadGraphOnLoad();
            InitializeScene();

            TTerraWorld.WorldGraph.ResetGraphsStatus();
            TTerrainGenerator.ResetNodesProgress();

#if TERRAWORLD_PRO
            aboutText = "TERRAWORLD Pro" + "\n" +
            "Ver. " + TVersionController.VersionStr + "\n\n" +
            "TerraUnity Corporation (c)" + "\n" +
            "All Rights Reserved " + DateTime.Now.Year + "\n" +
            "www.terraunity.com";
#endif

            lastNode = null;

//#if TW_TEMPLATES
            TimeSpan timeSpan = (DateTime.Now - TProjectSettings.LastTimeTemplateSyncCalled);
            if (timeSpan.TotalDays > 1) TTemplatesManager.ASyncTemplatesFoldersFromServer();
//#endif
        }

        private void OnEnable()
        {
            if (!Application.isEditor) return;

            if (DownloadDemoScene.status.Equals(SceneDownloaderStatus.Downloading))
            {
                ShowNotification(new GUIContent("TerraWorld is downloading the demo scene!\n\nPlease wait...", ""));
                return;
            }

            // Temporary solution - remove when all UI parameters read data from the scene
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path.Contains("TerraWorld/Scenes"))
            {
                ShowNotification(new GUIContent("TerraWorld is locked when demo scene is loaded!\n\nIt will be unlocked when new or other existing\n\nscene is loaded", ""));
                return;
            }

            //if (activeScene == SceneManager.GetSceneByPath(DownloadDemoScene.demoScenePath) || activeScene == SceneManager.GetSceneByPath(DownloadDemoScene.demoSceneDownloadPath))
            //{
            //    ShowNotification(new GUIContent("TerraWorld is locked when demo scene is loaded!\n\nIt will be unlocked when new or other existing\n\nscene is loaded", ""));
            //    return;
            //}

            activeTab = (ShowTabs)TProjectSettings.ActiveGraphIndex;
            Initialize();
            ClearHelperObjects();
            THelpersUI.SwitchSceneGUI(TProjectSettings.SceneViewGUI);

#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
#endif

        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying) return;

#if UNITY_EDITOR
            TimeSpan timeDiff = DateTime.Now - startTerraworldTime;
            int temp = ((int)(timeDiff.TotalMinutes * 1.0f / 15) + 1) * 15;
            TTerraWorld.FeedbackEvent(EventCategory.SoftwareInfo, "Using Time (Min)", temp);

            EditorApplication.update -= OnEditorUpdate;
            if (TTerraWorldManager.IsMainTerraworldGameObject == null) return;

            ClearHelperObjects();
            //if (!TTerraWorldManager.isQuitting) TTerraWorld.SaveWorldGraphFromScene();
            //if (!TTerraWorldManager.isQuitting) TTerraWorldManager.UpdateWorldGraphFromScene();
            THelpersUI.DestroySceneGUI();
#endif
        }

        private void ClearHelperObjects()
        {
            if (TAreaPreview.terrain != null)
                DestroyImmediate(TAreaPreview.terrain.gameObject);

            //if (TBoundingBox.boundingBox != null)
            //DestroyImmediate(TBoundingBox.boundingBox.gameObject);

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                {
                    if (go.name == "TerraWorld Preview Terrain")
                        DestroyImmediate(go.gameObject);
                    else if (go.name == "Bounding Box")
                        DestroyImmediate(go.gameObject);
                }
            }
        }

        protected virtual void OnEditorUpdate()
        {
            if (Application.isPlaying) return;

            // Check editor time for syncs and delays
            //if (liveUpdate && syncPaused)
            //{
            //    if (Time.realtimeSinceStartup >= lastEditorUpdateTime)
            //    {
            //        SyncAllProcesses();
            //        syncPaused = false;
            //    }
            //}

           // if (TTerrainGenerator.WorldInProgress)
           //     creationProgress = TTerrainGenerator.Progress;
           //     //creationProgress = Mathf.Lerp(creationProgress, TTerrainGenerator.Progress, Time.deltaTime * 0.1f);
           // else
           //     creationProgress = 100;

            if (scrollPositionNode.y < targetScrollPos)
            {
                if (isScrolling)
                {
                    scrollPositionNode.y += (DateTime.Now.Second - capturedSecondValue) * scrollSpeed;
                    Repaint();
                }
            }
            else
                isScrolling = false;
        }

        private void CheckProjectRenderingPipeline()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
                projectIsSRP = true;
            else
                projectIsSRP = false;
        }

        public void OnGUI()
        {
            if (DownloadDemoScene.status.Equals(SceneDownloaderStatus.Downloading))
            {
                ShowNotification(new GUIContent("TerraWorld is downloading the demo scene!\n\nPlease wait...", ""));
                return;
            }

            // Temporary solution - remove when all UI parameters read data from the scene
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path.Contains("TerraWorld/Scenes"))
            {
                ShowNotification(new GUIContent("TerraWorld is locked when demo scene is loaded!\n\nIt will be unlocked when new or other existing\n\nscene is loaded", ""));
                return;
            }

            //if (activeScene == SceneManager.GetSceneByPath(DownloadDemoScene.demoScenePath) || activeScene == SceneManager.GetSceneByPath(DownloadDemoScene.demoSceneDownloadPath))
            //{
            //    ShowNotification(new GUIContent("TerraWorld is locked when demo scene is loaded!\n\nIt will be unlocked when new or other existing\n\nscene is loaded", ""));
            //    return;
            //}

            if (TTerraWorldManager.CreateAndGetTerraworldGameObject == null)
            {
                //GUIStyle _style = new GUIStyle(EditorStyles.toolbarButton); _style.fixedHeight = 45;
                //THelpersUI.GUI_Button(new GUIContent("ACTIVE TERRAWORLD", "Create and active terraworld game object and it's features!"), _style, ActivateTerraWorld, 100, 0, new Color(0.88f, 0.79f, 0.45f, 1f));
                ShowNotification(new GUIContent("No TerraWorld Game Object Found! (Error 0025)", ""));
                return;
            }

#if !TERRAWORLD_DEBUG
            try
#endif
            {
                GlobalGUISettings();

                if (activeTab.Equals(ShowTabs.showArea))
                    activeGraph = TTerraWorld.WorldGraph.areaGraph;
                else if (activeTab.Equals(ShowTabs.showHeightmap))
                    activeGraph = TTerraWorld.WorldGraph.heightmapGraph;
                else if (activeTab.Equals(ShowTabs.showColormap))
                    activeGraph = TTerraWorld.WorldGraph.colormapGraph;
                else if (activeTab.Equals(ShowTabs.showBiomes))
                    activeGraph = TTerraWorld.WorldGraph.biomesGraph;
                else if (activeTab.Equals(ShowTabs.showRendering))
                    activeGraph = TTerraWorld.WorldGraph.renderingGraph;
                else if (activeTab.Equals(ShowTabs.showFX))
                    activeGraph = TTerraWorld.WorldGraph.FXGraph;
                else if (activeTab.Equals(ShowTabs.showPlayer))
                    activeGraph = TTerraWorld.WorldGraph.playerGraph;

                if (activeGraph == null) activeGraph = TTerraWorld.WorldGraph.areaGraph;
                if (activeGraph == null) return;

                //if (THelpersUI.ActiveNode == null && activeGraph.nodes.Count > 0) THelpersUI.ActiveNode = activeGraph.nodes[0];
                if (THelpersUI.ActiveNode == null && activeGraph.nodes.Count > 0) THelpersUI.ActiveNode = TTerraWorld.WorldGraph.areaGraph.nodes[0];

                HeaderGUI();

                TabsGUI();
                
                GraphGUI();
                if (lastTab != activeTab )
                {
                    lastTab = activeTab;
                    TTerraWorldManager.SaveOldGraph();
                }

                if (lastNode != THelpersUI.ActiveNode)
                {
                    lastNode = THelpersUI.ActiveNode;
                    TTerraWorldManager.SaveOldGraph();
                    //TAreaPreview.Visible = false;
                }

                GenerateWorldGUI();

                if (activeTab.Equals(ShowTabs.showArea))
                    TTerraWorld.WorldGraph.areaGraph = (TAreaGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showHeightmap))
                    TTerraWorld.WorldGraph.heightmapGraph = (THeightmapGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showColormap))
                    TTerraWorld.WorldGraph.colormapGraph = (TColormapGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showBiomes))
                    TTerraWorld.WorldGraph.biomesGraph = (TBiomesGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showRendering))
                    TTerraWorld.WorldGraph.renderingGraph = (TRenderingGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showFX))
                    TTerraWorld.WorldGraph.FXGraph = (TFXGraph)activeGraph;
                else if (activeTab.Equals(ShowTabs.showPlayer))
                    TTerraWorld.WorldGraph.playerGraph = (TPlayerGraph)activeGraph;
            }
#if !TERRAWORLD_DEBUG
            catch
            {
                GUIUtility.ExitGUI();
            }
#endif

            //if (GUI.changed) SetObjectDirty(null);
        }

        //private static void SetObjectDirty(UnityEngine.Object o)
        //{
        //    EditorUtility.SetDirty(o);
        //}

        private void GlobalGUISettings()
        {
            GUI.skin.label.fontSize = labelFontSize;
            windowWidth = this.position.width;
            windowHeight = this.position.height;
            THelpersUI.windowWidth = position.width;
            THelpersUI.windowHeight = position.height;

            if (EditorGUIUtility.isProSkin)
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            else
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        }

        private void TabsGUI()
        {
            //BackgroundUIColor(new Color(0.96f, 0.96f, 0.96f, 1f), 0, 78);

            int tabSize = 50;
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = tabSize;
            style.fixedHeight = tabSize;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            // Area Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            //GUI.color = new Color(1f, 1f, 1f, 0.65f);

            if (activeTab.Equals(ShowTabs.showArea))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.areaIcon, "AREA SELECTION"), style))
            {
                activeTab = ShowTabs.showArea;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            // Heightmap Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showHeightmap))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.heightmapIcon, "HEIGHTMAP"), style))
            {
                activeTab = ShowTabs.showHeightmap;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            // Colormap Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showColormap))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.colormapIcon, "COLORMAP"), style))
            {
                activeTab = ShowTabs.showColormap;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            // Biomes Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showBiomes))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.biomesIcon, "BIOMES"), style))
            {
                activeTab = ShowTabs.showBiomes;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            // Player Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showPlayer))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.runtimeIcon, "PLAYER"), style))
            {
                activeTab = ShowTabs.showPlayer;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            GUILayout.FlexibleSpace();

#if TERRAWORLD_PRO
            // Rendering Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showRendering))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.renderingIcon, "TERRAIN"), style))
            {
                activeTab = ShowTabs.showRendering;
                InitGraphStates();
            }

            GUI.color = enabledColor;

            // FX Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showFX))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.FXIcon, "VFX"), style))
            {
                activeTab = ShowTabs.showFX;
                InitGraphStates();
            }

            GUI.color = enabledColor;
#endif
            // Gloabl Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showGlobal))
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            if (GUILayout.Button(new GUIContent(TResourcesManager.globalSettingsIcon, "SETTINGS"), style))
            {
                activeTab = ShowTabs.showGlobal;
                InitGraphStates();
            }

            GUI.backgroundColor = enabledColor;
            GUI.color = enabledColor;

            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.fontSize = 7;
            style.fixedWidth = tabSize - 3;
            style.fixedHeight = 12;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            if (GUILayout.Button("AREA", style)) { }
            if (GUILayout.Button("HEIGHTS", style)) { }
            if (GUILayout.Button("COLORS", style)) { }
            if (GUILayout.Button("BIOMES", style)) { }
            if (GUILayout.Button("PLAYER", style)) { }

            GUILayout.FlexibleSpace();

#if TERRAWORLD_PRO
            if (GUILayout.Button("TERRAIN", style)) { }
            if (GUILayout.Button("VFX", style)) { }
#endif
            if (GUILayout.Button("SETTINGS", style)) { }

            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        // Graph
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        private void GraphGUI()
        {
            if (activeTab.Equals(ShowTabs.showArea))
            {
                THelpersUI.ActiveNode = activeGraph.nodes[0];
                WorldArea node = (WorldArea)THelpersUI.ActiveNode;
#if !TERRAWORLD_DEBUG
                try
                {
                    WorldAreaSettings(ref node);
                }
                catch 
                {
                }
#else
                WorldAreaSettings(ref node);
#endif

            }
#if TERRAWORLD_PRO

            else if (activeTab.Equals(ShowTabs.showRendering))
            {
                THelpersUI.ActiveNode = activeGraph.nodes[0];
                RenderingNode node = (RenderingNode)THelpersUI.ActiveNode;
                TerrainRenderingSettings(ref node);
            }
            else if (activeTab.Equals(ShowTabs.showFX))
            {
                THelpersUI.ActiveNode = activeGraph.nodes[0];
                FXNode node = (FXNode)THelpersUI.ActiveNode;
                VFXSettings(ref node);
            }
#endif
            else if (activeTab.Equals(ShowTabs.showGlobal))
            {
                GlobalSettings();
            }
            else if (activeTab.Equals(ShowTabs.showPlayer))
            {
                THelpersUI.ActiveNode = activeGraph.nodes[0];
                PlayerNode node = (PlayerNode)THelpersUI.ActiveNode;
                PlayerSettings(ref node);
            }
            else
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                AddModulesGUI();
                ModulesGUI();

                EditorGUILayout.EndScrollView();

                //style = new GUIStyle(EditorStyles.radioButton);
                //style.fontSize = 8;
                //
                //EditorGUILayout.BeginHorizontal();
                //
                //if (liveUpdate)
                //    GUI.color = Color.green;
                //else
                //    GUI.color = Color.grey;
                //
                //EditorGUILayout.HelpBox("LIVE UPDATE", MessageType.None);
                //GUI.color = Color.white;
                //
                //lastRect = GUILayoutUtility.GetLastRect();
                //lastRect.x = 95;
                //liveUpdate = GUI.Toggle(lastRect, liveUpdate, "", style);
                //GUILayout.FlexibleSpace();
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                if (THelpersUI.ActiveNode != null && THelpersUI.ActiveNode.parentGraph == activeGraph)
                    CommonGUISettings();
            }
        }

        // Generate World
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateNewVersion()
        {
            TDebug.OpenWebPage("New version available. Do you wish to update to the latest version?", "TerraWorld", TTerraWorld.NewVersionWebAddress);
        }

        private void GenerateWorldGUI()
        {
            //GetDirtyNode();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
            int ribbonSize = 50;
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedHeight = ribbonSize;
            style.fixedWidth = ribbonSize;

            EditorGUILayout.BeginHorizontal();

            if (!TTerrainGenerator.Error)
            {
                if (TTerrainGenerator.WorldInProgress)
                {
                    //if (TTerrainGenerator.Progress != null && TTerrainGenerator.Progress > 0 && TTerrainGenerator.Progress < 30)
                    //    TTerrainGenerator.ShowProgressWindow("Fetching Raw Data From Servers ...", TTerrainGenerator.Progress.Value * 0.5f / 100, TTerrainGenerator.progressID);
                    //else if (TTerrainGenerator.Progress != null && TTerrainGenerator.Progress > 30 && TTerrainGenerator.Progress < 95)
                    //    TTerrainGenerator.ShowProgressWindow("Analyzing Graph ...", TTerrainGenerator.Progress.Value * 0.5f / 100, TTerrainGenerator.progressID);
                    //TTerrainGenerator.ShowProgressWindow("Analyzing Graph : " + TDebug.currentProcessLog, TTerrainGenerator.Progress * 0.5f / 100, TTerrainGenerator.progressID);

                    if (TTerrainGenerator.Progress != null)
                        TTerrainGenerator.ShowProgressWindow(TTerrainGenerator.ProgressDetails, TTerrainGenerator.Progress.Value * 0.9f / 100, TTerrainGenerator.progressID);

                    GUI.backgroundColor = new Color(1, 1, 1, 0.05f);

                    //if (TTerrainGenerator.WorldInProgress)
                    GUI.color = new Color(1, 1, 1, 0.75f);
                    //else
                    //GUI.color = new Color(1, 1, 1, 0.25f);

                    //int iconIndex = (int)Mathf.Clamp((creationProgress / 100f * progressIcons.Length) - 1, 0, progressIcons.Length - 1);
                    //progressMain = progressIcons[iconIndex];
                    //THelpersUI.GUI_Label(progressMain, style, -20, 0, Color.clear, Color.clear);

                    style.font = TResourcesManager.digitalFont;
                    style.fontSize = 32;
                    //style.fontStyle = FontStyle.Bold;
                    style.alignment = TextAnchor.MiddleLeft;
                    style.fixedWidth = ribbonSize + 10;
                    style.fixedHeight = ribbonSize;

                    lastRect = new Rect();
                    lastRect.x = 3;
                    lastRect.y = position.height - 56;
                    lastRect.width = ribbonSize + 10;
                    lastRect.height = ribbonSize;

                    // creationProgress = Mathf.Lerp(creationProgress, TTerrainGenerator.Progress, Time.deltaTime * 0.1f);

                    //THelpersUI.GUI_Label(new GUIContent(((int)creationProgress).ToString(), "Generation progress"), lastRect, style);
                    if (TTerrainGenerator.Progress != null)
                        THelpersUI.GUI_Label(new GUIContent(((int)TTerrainGenerator.Progress).ToString(), "Generation progress"), lastRect, style);

                    style.fontSize = 18;
                    style.alignment = TextAnchor.MiddleRight;
                    THelpersUI.GUI_Label(new GUIContent("%", "Generation progress"), lastRect, style);

                    style.alignment = TextAnchor.MiddleCenter;
                }
                else
                {
                    TTerrainGenerator.CloseProgressWindow(TTerrainGenerator.progressID);
                    style = new GUIStyle(EditorStyles.toolbarButton);
                    style.fixedHeight = ribbonSize;
                    style.fixedWidth = ribbonSize;

                    if (string.IsNullOrEmpty(TTerraWorld.NewVersionWebAddress))
                    {
                        GUI.backgroundColor = new Color(1, 1, 1, 0.05f);
                        GUI.color = new Color(1, 1, 1, 0.25f);
                        THelpersUI.GUI_Button(new GUIContent(TResourcesManager.versionUpdateIcon, "TerraWorld is up to date!"), style, null, -13);
                    }
                    else
                    {
                        bool blinking = true;
                        if (DateTime.Now.Second % 2 == 0) blinking = false;

                        if (blinking)
                        {
                            GUI.backgroundColor = new Color(1, 1, 1, 0.05f);
                            GUI.color = new Color(1, 1, 1, 0.25f);
                        }
                        else
                        {
                            GUI.color = enabledColor;
                            GUI.backgroundColor = enabledColor;
                        }

                        THelpersUI.GUI_Button(new GUIContent(TResourcesManager.versionUpdateIcon, "New version update available!"), style, UpdateNewVersion, -13);

                        TimeSpan timeSpan = (DateTime.Now - TProjectSettings.LastTimeUpdateWindowShowed);
                        if (timeSpan.TotalDays > 1)
                        {
                            UpdateNewVersion();
                        }
                    }
                }

                GUI.color = enabledColor;
                GUI.backgroundColor = enabledColor;

                //GUI.backgroundColor = new Color(1, 1, 1, 0.4f);

                //EditorGUILayout.Knob
                //    (
                //    new Vector2(ribbonSize, ribbonSize),                                                               
                //    (int)Mathf.Clamp(creationProgress, 0f, 100f),
                //    0,
                //    100,
                //    "%",
                //    Color.gray,
                //    new Color(0.0f, 0.5f, 0.0f, 1),
                //    true,
                //    GUILayout.Width(ribbonSize)
                //    );
            }
            else
            {
                TTerrainGenerator.CloseProgressWindow(TTerrainGenerator.progressID);
                GUI.backgroundColor = new Color(1, 1, 1, 0.05f);

                if (GUILayout.Button(new GUIContent(TResourcesManager.sadFaceIcon, "Report Bug!"), GUILayout.Width(ribbonSize), GUILayout.Height(ribbonSize)) && BugReporter.GenerateFeedbackLog())
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Bugreport");
                    BugReporterEditor BGE = (BugReporterEditor)GetWindow(typeof(BugReporterEditor), false, "Bug Reporter", true);
                }

                GUI.backgroundColor = enabledColor;
            }

            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);

            //if (!TTerrainGenerator.Error)
            //style.fixedWidth = position.width - ribbonSize - 20;
            //else
            //            style.fixedWidth = position.width - ribbonSize - 75;
            style.fixedWidth = position.width - ribbonSize - 75;

            if (!TTerrainGenerator.WorldInProgress)
            {
                if (GUILayout.Button(new GUIContent(TResourcesManager.launchIcon, "Generate World"), style))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Play");
                    CreateWorld();
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent(TResourcesManager.stopIcon, "Stop Generation"), style))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Stop");
                    StopGeneration();
                }
            }
            //}
            //else
            //{
            //if (TTerrainGenerator.Error)
            //{
            //    GUI.backgroundColor = new Color(1, 1, 1, 0.05f);
            //
            //    if (GUILayout.Button(THelpersUI.cautionIcon, GUILayout.Width(ribbonSize), GUILayout.Height(ribbonSize)))
            //    {
            //        FocusOnConsole();
            //    }
            //
            //    GUI.backgroundColor = enabledColor;
            //}

            //if (GUILayout.Button(launchIcon, style))
            //{
            //    GenerateWorld();
            //}
            //}

            GUI.backgroundColor = new Color(1, 1, 1, 0.05f);

            //if (TAreaPreview.Visible)
            //    GUI.color = enabledColor;
            //else
            //    GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            //
            //if (GUILayout.Button(new GUIContent(TResourcesManager.icon3D, "Preview 3D World"), GUILayout.Width(ribbonSize), GUILayout.Height(ribbonSize - 10)))
            //{
            //    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Preview");
            //    TAreaPreview.ForceVisible = !TAreaPreview.ForceVisible;
            //}
            //
            //GUI.backgroundColor = enabledColor;
            //GUI.color = enabledColor;
            //
            //GUI.contentColor = Color.cyan;
            //
            //if (TAreaPreview.Visible)
            //{
            //    float previewProgress = TAreaPreview.Progress / 100f;
            //
            //    if (previewProgress != 0 && previewProgress != 1)
            //    {
            //        lastRect = GUILayoutUtility.GetLastRect();
            //        lastRect.y += ribbonSize - 8;
            //        lastRect.width = ribbonSize;
            //        lastRect.height = 4;
            //        EditorGUI.ProgressBar(lastRect, previewProgress, "");
            //    }
            //}

            if (GUILayout.Button(new GUIContent(TResourcesManager.SupportIcon, "Support"), GUILayout.Width(ribbonSize), GUILayout.Height(ribbonSize - 10)))
            {
                TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Support");
                SocialLinkes.Init();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void InitModule()
        {
            int index = activeGraph.nodes.Count - 1;
            TNode currentNode = activeGraph.nodes[index];

            if (activeGraph.GetType() == typeof(THeightmapGraph))
                index -= 1;
            else if (activeGraph.GetType() == typeof(TColormapGraph))
                index -= 5;

            //currentNode.Data.rect = new System.Numerics.Vector4
            //(
            //    (position.width / 2) - (nodeSize.x / 2),
            //    headerSpace + (index * (nodeSize.y + moduleSpace)),
            //    nodeSize.x,
            //    nodeSize.y
            //);

            float widthOffset = 0;
            if (index % 2 == 0) widthOffset = 55;
            else widthOffset = -55;

            currentNode.Data.position += new System.Numerics.Vector4
            (
                (position.width / 2) - (nodeSize.x / 2) + widthOffset,
                headerSpace + (index * (nodeSize.y + 10) / 2),
                nodeSize.x,
                nodeSize.y
            );
        }

        private void SwitchTabAndScroll(string tabName, float scrollPositionY = 0)
        {
            if (tabName == "AREA")
                activeTab = ShowTabs.showArea;
            else if (tabName == "COLORMAP")
                activeTab = ShowTabs.showColormap;
            else if (tabName == "HEIGHTMAP")
                activeTab = ShowTabs.showHeightmap;
            else if (tabName == "BIOMES")
                activeTab = ShowTabs.showBiomes;
            else if (tabName == "RENDERING")
                activeTab = ShowTabs.showRendering;
            else if (tabName == "FX")
                activeTab = ShowTabs.showFX;
            else if (tabName == "GLOBAL")
                activeTab = ShowTabs.showGlobal;

            InitGraphStates();
            SmoothScroll(scrollPositionY);
        }

        private void SmoothScroll(float target)
        {
            scrollPositionNode = Vector2.zero;
            targetScrollPos = target;
            capturedSecondValue = DateTime.Now.Second;
            isScrolling = true;
        }

        private void SwitchTabAndFocusNode(TNode node)
        {
            TGraph nodeGraph = node.parentGraph;

            if (nodeGraph._title == "AREA")
                activeTab = ShowTabs.showArea;
            else if (nodeGraph._title == "COLORMAP")
                activeTab = ShowTabs.showColormap;
            else if (nodeGraph._title == "HEIGHTMAP")
                activeTab = ShowTabs.showHeightmap;
            else if (nodeGraph._title == "BIOMES")
                activeTab = ShowTabs.showBiomes;
            else if (nodeGraph._title == "RENDERING")
                activeTab = ShowTabs.showRendering;
            else if (nodeGraph._title == "FX")
                activeTab = ShowTabs.showFX;
            else if (nodeGraph._title == "GLOBAL")
                activeTab = ShowTabs.showGlobal;

            InitGraphStates();
            THelpersUI.ActiveNode = node;
        }

        private void ModuleFunction(int windowID)
        {
            TNode currentNode = activeGraph.GetNodeByID(windowID);

            if (currentNode == null)
                return;

            if (currentNode.isActive)
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            GUI.backgroundColor = new Color(1, 1, 1, 0.25f);

            if (currentNode.Data.moduleType.Equals(ModuleType.Processor))
                moduleIcon = TResourcesManager.processorIcon;
            else if (currentNode.Data.moduleType.Equals(ModuleType.Operator))
                moduleIcon = TResourcesManager.operatorIcon;
            else if (currentNode.Data.moduleType.Equals(ModuleType.Mask))
                moduleIcon = TResourcesManager.maskOperatorIcon;
            else if (currentNode.Data.moduleType.Equals(ModuleType.Extractor))
                moduleIcon = TResourcesManager.extractorIcon;
            else if (currentNode.Data.moduleType.Equals(ModuleType.Scatter))
                moduleIcon = TResourcesManager.scatterIcon;
            else if (currentNode.Data.moduleType.Equals(ModuleType.TerraMesh))
                moduleIcon = TResourcesManager.terraMeshIcon;
            else
                moduleIcon = TResourcesManager.processorIcon;

            if (currentNode.isActive)
                THelpersUI.GUI_Label(moduleIcon, 40, 40, -17, 17, enabledColor);
            else
                THelpersUI.GUI_Label(moduleIcon, 40, 40, -17, 17, disabledColor);

            if (currentNode.isActive)
                moduleState = TResourcesManager.onIcon;
            else
                moduleState = TResourcesManager.offIcon;

            GUIStyle style = new GUIStyle(EditorStyles.textField);
            style.fontSize = 8;
            style.alignment = TextAnchor.UpperCenter;
            THelpersUI.GUI_Label(new GUIContent(currentNode.Data.name.ToUpper(), ""), new Rect(1, 1, nodeSize.x - 2, 13), style);

            style = new GUIStyle();
            THelpersUI.GUI_Label(new GUIContent("", currentNode.Data.name), new Rect(20, 0, nodeSize.x - 20, nodeSize.y), style);
            THelpersUI.GUI_Label(new GUIContent("", "Enable / Disable"), new Rect(0, 16, 20, 20), style);

            // Enable/Disable module
            if (currentNode.isSwitchable && GUI.Button(new Rect(0, 16, 20, 20), moduleState))
            {
                //currentNode.isActive = GUI.Toggle(lastRect, currentNode.isActive, currentNode.Data.name);
                currentNode.isActive = !currentNode.isActive;
                activeGraph.UpdateConnections();
                //UpdateModulesList(currentNode);
            }

            style = new GUIStyle();

            // Remove module
            if (currentNode.isRemovable)
            {
                THelpersUI.GUI_Label(new GUIContent("", "Remove"), new Rect(0, 37, 20, 20), style);

                if (GUI.Button(new Rect(0, 37, 20, 20), TResourcesManager.removeIcon))
                {
                    if (currentNode.GetType() == typeof(HeightmapSource) && activeGraph.nodes.Count > 2)
                    {
                        EditorUtility.DisplayDialog("TERRAWORLD", "INVALID OPERATION : There are dependant modules available in the graph!\n\nTry to remove them first.", "Ok");
                        return;
                    }

                    if (EditorUtility.DisplayDialog("TERRAWORLD", "REMOVE MODULE \n Are you sure you want to delete module?", "No", "Yes"))
                        return;

                    activeGraph.RemoveNode(currentNode.Data.ID);

                    if (activeGraph.nodes.Count > 0 && activeGraph.nodes[0] != null)
                    {
                        THelpersUI.ActiveNode = activeGraph.nodes[0];
                        GUI.FocusWindow(0);
                        activeGraph.UpdateConnections();
                    }

                    //if (TBoundingBox.boundingBox != null)
                    //DestroyImmediate(TBoundingBox.boundingBox);

                    return;
                }
            }

            //style = new GUIStyle(EditorStyles.toolbarButton);

            // Run module
            //if (currentNode.isRunnable)
            //{
            //    style.fixedWidth = 28;
            //    style.fixedHeight = 28;
            //
            //    if (currentNode.isActive)
            //    {
            //        GUILayout.Space(5);
            //
            //        EditorGUILayout.BeginHorizontal();
            //        GUILayout.FlexibleSpace();
            //        if (GUILayout.Button(nextIcon, style))
            //        {
            //            //Debug.Log(GUI.GetNameOfFocusedControl());
            //            RunModule(currentNode);
            //        }
            //        GUILayout.FlexibleSpace();
            //        EditorGUILayout.EndHorizontal();
            //    }
            //}
            //else
            //{
            //    style.fixedWidth = 50;
            //    style.fixedHeight = 50;
            //    //style.onHover.background = mainIcon;
            //
            //    GUI.backgroundColor = Color.clear;
            //    EditorGUILayout.BeginHorizontal();
            //    GUILayout.FlexibleSpace();
            //    GUILayout.Label(mainIcon, style);
            //    GUILayout.FlexibleSpace();
            //    EditorGUILayout.EndHorizontal();
            //}

            GUI.backgroundColor = enabledColor;
            GUI.color = enabledColor;

            // Progress bar display
            if (currentNode.isRunnable)
            {
                //if (currentNode.Progress != 1)
                //{
                //    GUI.backgroundColor = enabledColor;
                //    //GUI.color = Color.green;
                //}
                //else
                //{
                //    GUI.backgroundColor = disabledColor;
                //    //GUI.color = Color.green;
                //}

                Rect rect = new Rect(0, nodeSize.y - progressBarHeight, nodeSize.x, progressBarHeight);
                EditorGUI.ProgressBar(rect, currentNode.Progress, "");
                GUI.color = enabledColor;
                GUI.backgroundColor = enabledColor;
            }

            // Limit modules workspace
            float minSpaceAllowedX = 10;
            float maxSpaceAllowedX = position.width - 35;
            float minSpaceAllowedY = headerSpace;
            //float maxSpaceAllowedY = (position.height * 0.75f) - headerSpace;
            float maxSpaceAllowedY = 64000;

            if (currentNode.yMin() >= maxSpaceAllowedY)
                currentNode.Data.position.Y = maxSpaceAllowedY - nodeSize.y - 1;
            else if (currentNode.yMax() <= minSpaceAllowedY)
                currentNode.Data.position.Y = minSpaceAllowedY + 1;
            else if (currentNode.xMin() <= minSpaceAllowedX)
                currentNode.Data.position.X = minSpaceAllowedX + 1;
            else if (currentNode.xMax() >= maxSpaceAllowedX)
                currentNode.Data.position.X = maxSpaceAllowedX - nodeSize.x - 1;
            else
                GUI.DragWindow();

            if (Event.current.GetTypeForControl(windowID) == EventType.Used)
                THelpersUI.ActiveNode = currentNode;
        }

        private void ModuleFunctionMaster(int windowID)
        {
            TNode currentNode = activeGraph.GetNodeByID(windowID);

            if (currentNode == null)
                return;

            if (currentNode.inputConnections.Count > 0 && currentNode.inputConnections[0].previousNodeID != -1)
                GUI.contentColor = enabledColor;
            else
                GUI.contentColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);

            string moduleText = "";

            if (currentNode.GetType() == typeof(HeightmapMaster))
            {
                moduleIcon = TResourcesManager.heightmapIconMaster;
                moduleText = "HEIGHTMAP";
            }
            else if (currentNode.GetType() == typeof(ColormapMaster))
            {
                moduleIcon = TResourcesManager.colormapIconMaster;
                moduleText = "COLORMAP";
            }
            else if (currentNode.GetType() == typeof(TerrainLayerMaster1))
            {
                moduleIcon = TResourcesManager.terrainLayerIcon;
                moduleText = "LAYER 1";
            }
            else if (currentNode.GetType() == typeof(TerrainLayerMaster2))
            {
                moduleIcon = TResourcesManager.terrainLayerIcon;
                moduleText = "LAYER 2";
            }
            else if (currentNode.GetType() == typeof(TerrainLayerMaster3))
            {
                moduleIcon = TResourcesManager.terrainLayerIcon;
                moduleText = "LAYER 3";
            }
            else if (currentNode.GetType() == typeof(TerrainLayerMaster4))
            {
                moduleIcon = TResourcesManager.terrainLayerIcon;
                moduleText = "LAYER 4";
            }

            if (currentNode.isActive)
                THelpersUI.GUI_Label(moduleIcon, 75, 75, -30, 0, enabledColor);
            else
                THelpersUI.GUI_Label(moduleIcon, 75, 75, -30, 0, disabledColor);

            GUIStyle style = new GUIStyle(EditorStyles.toolbarTextField);
            //style.fontSize = 13;
            style.alignment = TextAnchor.MiddleCenter;
            THelpersUI.GUI_Label(new GUIContent(moduleText, ""), new Rect(-5, 70, 100, 20), style);

            style = new GUIStyle();
            THelpersUI.GUI_Label(new GUIContent("", currentNode.Data.name), new Rect(0, 0, nodeSizeMaster.x, nodeSizeMaster.y), style);

            if (currentNode.isActive)
                moduleState = TResourcesManager.onIcon;
            else
                moduleState = TResourcesManager.offIcon;

            PlaceMasterNodes(currentNode);

            // Limit modules workspace
            float minSpaceAllowedX = 10;
            float maxSpaceAllowedX = position.width - 35;
            float minSpaceAllowedY = headerSpace;
            //float maxSpaceAllowedY = (position.height * 0.75f) - headerSpace;
            float maxSpaceAllowedY = 64000;

            if (currentNode.yMin() >= maxSpaceAllowedY)
                currentNode.Data.position.Y = maxSpaceAllowedY - nodeSizeMaster.y - 1;
            else if (currentNode.yMax() <= minSpaceAllowedY)
                currentNode.Data.position.Y = minSpaceAllowedY + 1;
            else if (currentNode.xMin() <= minSpaceAllowedX)
                currentNode.Data.position.X = minSpaceAllowedX + 1;
            else if (currentNode.xMax() >= maxSpaceAllowedX)
                currentNode.Data.position.X = maxSpaceAllowedX - nodeSizeMaster.x - 1;
            else
                GUI.DragWindow();

            if (Event.current.GetTypeForControl(windowID) == EventType.Used)
                THelpersUI.ActiveNode = currentNode;
        }

        private void ModuleFunctionExternal(int windowID)
        {
            int nodeID = 0;

            if (!externalIDs.TryGetValue(windowID, out nodeID))
                return;

            TNode currentNode = activeGraph.GetNodeByID(nodeID);

            if (currentNode == null)
                return;

            if (currentNode.isActive)
                GUI.color = enabledColor;
            else
                GUI.color = disabledColor;

            GUI.backgroundColor = new Color(1, 1, 1, 0.25f);

            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 28;
            style.fixedHeight = 28;

            GUILayout.Space(-20);

            // Go to module in its corresponding graph
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(TResourcesManager.directIcon, style))
            {
                SwitchTabAndFocusNode(currentNode);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            style = new GUIStyle();
            THelpersUI.GUI_Label(new GUIContent("", currentNode.parentGraph._title + " / " + currentNode.Data.name), new Rect(0, 0, nodeSizeExternal.x, nodeSizeExternal.y), style);
        }

        // Area Tab
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void HeaderGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            GUIStyle style = new GUIStyle();
            style.fixedWidth = 150;
            style.fixedHeight = 50;

            GUI.DrawTexture(new Rect(0, 0, position.width, 78), TResourcesManager.black);

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
#if TERRAWORLD_PRO
            GUILayout.Label(TResourcesManager.logoPro, style);
#else
            GUILayout.Label(TResourcesManager.logoLite, style);
#endif
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void WorldAreaSettings(ref WorldArea module)
        {
            if (lastTab != activeTab)
            {
            }

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (Event.current.shift && Event.current.control)
            {
                THelpersUI.GUI_HelpBoxTitleLeft(new GUIContent("TEMPLATE IGNORE LIST", ""), -10, enabledColor, THelpersUI.UIColor);

                // Manual Preset Save/Load
                GUI.backgroundColor = new Color(1, 1, 1, 0.2f);

                if (GUILayout.Button(new GUIContent("INVERSE"), GUILayout.Width(200), GUILayout.Height(20)))
                {
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_AreaGraph = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_AreaGraph;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_HeightmapGraph = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_HeightmapGraph;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_ColormapGraph = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_ColormapGraph;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_BiomesGraph = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_BiomesGraph;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_modernRendering = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_modernRendering;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_instancedDrawing = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_instancedDrawing;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_tessellation = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_tessellation;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_heightmapBlending = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_heightmapBlending;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_TillingRemover = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_TillingRemover;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_colormapBlending = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_colormapBlending;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralSnow = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralSnow;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_LayerProperties = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_LayerProperties;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_isFlatShading = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_isFlatShading;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_MainTerrainSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_MainTerrainSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_selectionIndexVFX = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_selectionIndexVFX;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_TimeOfDay = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_TimeOfDay;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CrepuscularRay = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CrepuscularRay;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CloudsSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CloudsSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_AtmosphericScatteringSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_AtmosphericScatteringSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_VolumetricFogSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_VolumetricFogSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WindSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WindSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WeatherSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WeatherSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_ReflectionSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_ReflectionSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WaterSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WaterSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_PostProcessSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_PostProcessSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_HorizonFogSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_HorizonFogSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_FlatShadingSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_FlatShadingSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_SateliteImageBlendingSettings = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_SateliteImageBlendingSettings;
                    TTerraWorld.WorldGraph.templateIgnoreList.Ignore_PlayerGraph = !TTerraWorld.WorldGraph.templateIgnoreList.Ignore_PlayerGraph;
                }

                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_AreaGraph = THelpersUI.GUI_Toggle(new GUIContent("Ignore_AreaGraph     ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_AreaGraph, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_HeightmapGraph = THelpersUI.GUI_Toggle(new GUIContent("Ignore_HeightmapGraph", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_HeightmapGraph, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_ColormapGraph = THelpersUI.GUI_Toggle(new GUIContent("Ignore_ColormapGraph ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_ColormapGraph, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_BiomesGraph = THelpersUI.GUI_Toggle(new GUIContent("Ignore_BiomesGraph   ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_BiomesGraph, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_modernRendering = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_modernRendering    ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_modernRendering, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_instancedDrawing = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_instancedDrawing   ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_instancedDrawing, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_tessellation = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_tessellation       ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_tessellation, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_heightmapBlending = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_heightmapBlending  ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_heightmapBlending, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_TillingRemover = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_TillingRemover     ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_TillingRemover, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_colormapBlending = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_colormapBlending   ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_colormapBlending, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralSnow = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_proceduralSnow     ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralSnow, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_proceduralPuddles  ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_LayerProperties = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_LayerProperties    ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_LayerProperties, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_isFlatShading = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_isFlatShading      ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_isFlatShading, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_SplatmapSettings   ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_MainTerrainSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_MainTerrainSettings", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_MainTerrainSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_RenderingGraph_BGTerrainSettings  ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_selectionIndexVFX = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_selectionIndexVFX           ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_selectionIndexVFX, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_TimeOfDay = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_TimeOfDay                   ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_TimeOfDay, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CrepuscularRay = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_CrepuscularRay              ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CrepuscularRay, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CloudsSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_CloudsSettings              ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_CloudsSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_AtmosphericScatteringSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_AtmosphericScatteringSettings", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_AtmosphericScatteringSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_VolumetricFogSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_VolumetricFogSettings       ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_VolumetricFogSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WindSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_WindSettings                ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WindSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WeatherSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_WeatherSettings             ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WeatherSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_ReflectionSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_ReflectionSettings          ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_ReflectionSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WaterSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_WaterSettings               ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_WaterSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_PostProcessSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_PostProcessSettings         ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_PostProcessSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_HorizonFogSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_HorizonFogSettings          ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_HorizonFogSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_FlatShadingSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_Ignore_FXGraph_FlatShadingSettings          ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_FlatShadingSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_SateliteImageBlendingSettings = THelpersUI.GUI_Toggle(new GUIContent("Ignore_FXGraph_SateliteImageBlendingSettings          ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_FXGraph_SateliteImageBlendingSettings, -10);
                TTerraWorld.WorldGraph.templateIgnoreList.Ignore_PlayerGraph = THelpersUI.GUI_Toggle(new GUIContent("Ignore_PlayerGraph         ", ""), TTerraWorld.WorldGraph.templateIgnoreList.Ignore_PlayerGraph, -10);

                THelpersUI.GUI_HelpBoxTitleLeft(new GUIContent("SAVE AS TEMPLATE", ""), -10, enabledColor, THelpersUI.UIColor);

                // Manual Preset Save/Load
                GUI.backgroundColor = new Color(1, 1, 1, 0.2f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Save"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    string graphPath = EditorUtility.SaveFilePanel("Save Template", TAddresses.presetsPath, "Graph.twg", "twg");

                    if (!string.IsNullOrEmpty(graphPath) && Directory.Exists(Path.GetDirectoryName(graphPath)) && graphPath.EndsWith(".twg"))
                    {
                        TTerraWorld.SaveGraphAsTemplate(graphPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        //EditorUtility.DisplayDialog("UNKNOWN PATH", "Path is not valid!", "Ok");
                        return;
                    }
                }

                if (GUILayout.Button(new GUIContent("Load"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    string graphPath = EditorUtility.OpenFilePanel("Load Graph", TAddresses.presetsPath, "twg");
                    Exception _exception = null;
                    if (File.Exists(graphPath))
                        TTerraWorld.LoadTemplate(graphPath, out _exception);
                    if (_exception != null) throw _exception;
                }
            }
            else
            {
                GUIStyle style = new GUIStyle(EditorStyles.toolbarTextField);
                style.fixedHeight = 30;
                style.fontSize = 13;
                style.alignment = TextAnchor.MiddleCenter;
                THelpersUI.GUI_HelpBoxTitleLeft(new GUIContent("LOCATION", "Display location on Interactive Map"), 0, enabledColor, THelpersUI.UIColor);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                TProjectSettings.LastSearchedPlace = EditorGUILayout.TextField(TProjectSettings.LastSearchedPlace, style);


                GUILayout.Space(5);

                float yOffset = GUILayoutUtility.GetLastRect().y + 43;
                GUILayout.BeginArea(new Rect((position.width / 2) + 100, yOffset, 35, 35));

                GUI.backgroundColor = new Color(1, 1, 1, 0.2f);

                if (GUILayout.Button(new GUIContent(TResourcesManager.searchIcon, "Search for typed location or address"), GUILayout.Width(35), GUILayout.Height(35)))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Search");
                    _coords = GeoCoder.AddressToLatLong(Regex.Replace(TProjectSettings.LastSearchedPlace, @"\s+", string.Empty));
                    _locations = GeoCoder.foundLocations;

                    if (_coords != null && _locations != null && GeoCoder.recognized)
                    {
                        SearchResultsDisplay searchResultsDisplay = (SearchResultsDisplay)GetWindow(typeof(SearchResultsDisplay), false, "Search Results Display", true);
                        searchResultsDisplay.position = new Rect(5, 135, 500, 384);
                        searchResultsDisplay.coordinates = _coords;
                        searchResultsDisplay.locations = _locations;
                    }
                }

                GUILayout.EndArea();

                GUI.backgroundColor = enabledColor;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);

                if (!GeoCoder.recognized)
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "UNKNOWN LOCATION : Address/Location Is Not Recognized!", "Ok");
                    GeoCoder.recognized = true;
                    return;
                }
                else
                    GUILayout.Space(5);

                GUI.backgroundColor = new Color(1, 1, 1, 0.1f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(TResourcesManager.locationIcon, "Display location on Interactive Map"), GUILayout.Width(75), GUILayout.Height(75)))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "DisplayLocation");
                    ShowMapAndRefresh();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = enabledColor;

                //GUILayout.Space(20);
                //
                //GUI.backgroundColor = Color.clear;
                //float iconRGB = 1f;
                //float iconAlpha = (float)TUtils.Clamp(0d, 1d, (creationProgress / 100d) + 0.2d);
                //GUI.color = new Color(iconRGB, iconRGB, iconRGB, iconAlpha);
                //style.fixedWidth = 450; style.fixedHeight = 273.375f;

                //THelpersUI.GUI_Label(homeIcon, style);
                //GUI.backgroundColor = new Color(1, 1, 1, 0.55f);
                //GUI.color = Color.white;

                THelpersUI.DrawUILine(0);

//#if TW_TEMPLATES
                GUI.enabled = TemplatesUI.SectionUI(this);
//#endif

                THelpersUI.GUI_HelpBoxTitleLeft(new GUIContent("PROJECT", ""), -10, enabledColor, THelpersUI.UIColor);

                GUILayout.Space(10);

                // Manual Preset Save/Load
                GUI.backgroundColor = new Color(1, 1, 1, 0.2f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(TResourcesManager.saveIcon, "Save current project into preset file"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    string graphPath = EditorUtility.SaveFilePanel("Save project", TAddresses.presetsPath, "Terra World Project.xml", "xml");

                    if (!string.IsNullOrEmpty(graphPath) && Directory.Exists(Path.GetDirectoryName(graphPath)) && graphPath.EndsWith(".xml"))
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Save");
                        TTerraWorld.TemplateName = "";
                        TTerraWorld.WorldGraph.UpdateRenderingAndVxfFromScene();
                        TTerraWorld.WorldGraph.SaveGraph(graphPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        //EditorUtility.DisplayDialog("UNKNOWN PATH", "Path is not valid!", "Ok");
                        return;
                    }
                }

                if (GUILayout.Button(new GUIContent(TResourcesManager.loadIcon, "Load project"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    string graphPath = EditorUtility.OpenFilePanel("Load Terraworld project", TAddresses.presetsPath, "xml");

                    if (File.Exists(graphPath))
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Load");
                        TTerraWorld.TemplateName = "";
#if TERRAWORLD_XPRO
                        CloseXGraph();
#endif

                        //#if TW_TEMPLATES
                        TemplatesUI.CustomGraphLoaded();
                        //#endif

                        LoadButtonClick(graphPath);
                    }
                    else
                        return;
                }

                GUILayout.Space(30);

                if (GUILayout.Button(new GUIContent(TResourcesManager.newIcon, "Create blank project to start from scratch"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "New");
                    TTerraWorld.TemplateName = "";
                    NewProject();
#if TERRAWORLD_XPRO
                    CloseXGraph();
#endif
                }
#if TERRAWORLD_XPRO

                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent(TResourcesManager.newIcon, "Edit Graph"), GUILayout.Width(55), GUILayout.Height(55)))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "EditGraph");
                    TTerraWorld.TemplateName = "";
                    OpenXGraph();
                }
#endif

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = enabledColor;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.clear;
                EditorGUILayout.HelpBox("SAVE", MessageType.None, true);
                GUILayout.Space(12);
                EditorGUILayout.HelpBox("LOAD", MessageType.None, true);
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("NEW", MessageType.None, true);

#if TERRAWORLD_XPRO
                GUILayout.Space(40);
                EditorGUILayout.HelpBox("Edit", MessageType.None, true);
#endif

                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);

                //   EditorGUILayout.BeginHorizontal();
                //   GUILayout.FlexibleSpace();
                //   GUI.backgroundColor = new Color(1, 1, 1, 0.2f);
                //   if (GUILayout.Button(new GUIContent(TResourcesManager.exportPackageIcon, "Export graph and its resources into a unitypackage file"), GUILayout.Width(55), GUILayout.Height(55)))
                //   {
                //       if (EditorUtility.DisplayDialog("TERRAWORLD", "EXPORT PACKAGE FROM GRAPH \n This operation will export a unitypackage file out of the current graph and its resource assets!", "OK", "Cancel"))
                //       {
                //           TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Export Package");
                //           TTerraWorld.SaveWorldGraph();
                //           AssetDatabase.Refresh();
                //           TemplateMaker.CreateTempelatePackage(AssetDatabase.LoadAssetAtPath(TTerraWorld.GraphPath, typeof(UnityEngine.Object)), TTerraWorld.WorldGraph.areaGraph.WorldArea._address);
                //       }
                //   }
                //   GUI.backgroundColor = enabledColor;
                //   GUILayout.FlexibleSpace();
                //   EditorGUILayout.EndHorizontal();
                //
                //   EditorGUILayout.BeginHorizontal();
                //   GUILayout.FlexibleSpace();
                //   GUI.backgroundColor = Color.clear;
                //   EditorGUILayout.HelpBox("EXPORT PACKAGE", MessageType.None, true);
                //   GUI.backgroundColor = Color.white;
                //   GUILayout.FlexibleSpace();
                //   EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(40);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        public static void CreateWorld()
        {
            //InitializeScene();
            //RenderingSettingsManager.UpdateTerrainLayers();
            // TTerraWorld.WorldGraph._renderingParams1 = TerrainRenderingManager.TerrainRenderingParams;
            // TTerraWorld.WorldGraph._VFXParams = SceneSettingsManager.FXParameters;
//#if TW_TEMPLATES
            if (TTemplatesManager.Status == TemplateManagerStatus.FetchingFolders)
                EditorUtility.DisplayDialog("TERRAWORLD", "SYNCING IN PROGRESS \n Templates are being synced from server!\n\nPlease wait until syncing is finished and try again!", "OK");
            else if (TTemplatesManager.Status == TemplateManagerStatus.Downloading)
                EditorUtility.DisplayDialog("TERRAWORLD", "DOWNLOAD IN PROGRESS \n Selected template content is currently downloading!\n\nPlease wait until download is finished and try again!", "OK");
            else if (TTemplatesManager.Status == TemplateManagerStatus.Importing)
                EditorUtility.DisplayDialog("TERRAWORLD", "IMPORTING IN PROGRESS \n Selected template content is currently importing!\n\nPlease wait !", "OK");
            else
            //#endif
            {
                //creationProgress = 0;
                TDebug.Initialize();
                TTerraworldGenerator.RunGraphFromUI();
            }
        }

        public void LoadButtonClick(string graphPath)
        {
            //InitializeScene();
            //RenderingSettingsManager.UpdateTerrainLayers();

//#if TW_TEMPLATES
            if (TTemplatesManager.Status == TemplateManagerStatus.FetchingFolders)
                EditorUtility.DisplayDialog("TERRAWORLD", "SYNCING IN PROGRESS \n Templates are being synced from server!\n\nPlease wait until syncing is finished and try again!", "OK");
            else if (TTemplatesManager.Status == TemplateManagerStatus.Downloading)
                EditorUtility.DisplayDialog("TERRAWORLD", "DOWNLOAD IN PROGRESS \n Selected template content is currently downloading!\n\nPlease wait until download is finished and try again!", "OK");
      //      else
//#else
      //          creationProgress = 0;

            TDebug.Initialize();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TTerraworldGenerator.LoadAndRunWorldGraph(graphPath, false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                              //#endif
        }

        public void NewProject()
        {
            if (EditorUtility.DisplayDialog("TERRAWORLD", "NEW GRAPH : Are you sure you want to use new graph?\n\nThis will break all the settings on the current scene, so make sure you are in a new scene!", "Yes", "No"))
            {
                CreateNewProject();
                ClearHelperObjects();
#if TW_TEMPLATES
                TemplatesUI.CustomGraphLoaded();
#endif
                InitGraphStates();
            }
        }

        private void CreateNewProject()
        {
            TProjectSettings.LastProjectDirPath = string.Empty;
            TTerraWorldManager.ResetOldGraph();
#if TERRAWORLD_XPRO
            TTerraWorldManager.ResetXGraph();
#endif
            TerrainRenderingManager.Reset();
            SceneSettingsManager.InstantiateNewSceneSettings();
            AssetDatabase.Refresh();
        }

        //public static void UpdateTerrainLayers()
        //{
        //    Terrain terrain = TTerrainGenerator.worldReference.GetComponent<Terrain>();
        //
        //    if (terrain == null || terrain.terrainData == null) return;
        //
        //    TColormapGraph colormapGraph = TTerraWorld.WorldGraph.colormapGraph;
        //    List<TerrainLayer> layersList = new List<TerrainLayer>();
        //    TNode masterNode = null;
        //    int maximumLayers = 4;
        //    int detectedCount = 0;
        //
        //    for (int i = 0; i < colormapGraph.nodes.Count; i++)
        //    {
        //        if (detectedCount < maximumLayers)
        //        {
        //            TNode node = colormapGraph.nodes[i];
        //
        //            if (node.type == typeof(TerrainLayerMaster1).FullName)
        //                masterNode = (TerrainLayerMaster1)node;
        //            else if (node.type == typeof(TerrainLayerMaster2).FullName)
        //                masterNode = (TerrainLayerMaster2)node;
        //            else if (node.type == typeof(TerrainLayerMaster3).FullName)
        //                masterNode = (TerrainLayerMaster3)node;
        //            else if (node.type == typeof(TerrainLayerMaster4).FullName)
        //                masterNode = (TerrainLayerMaster4)node;
        //
        //            if (masterNode != null && masterNode.inputConnections[0] != null)
        //            {
        //                Mask2DetailTexture layerNode = (Mask2DetailTexture)TTerraWorld.WorldGraph.GetNodeByID(masterNode.inputConnections[0].previousNodeID);
        //
        //                if (layerNode != null && !string.IsNullOrEmpty(layerNode.terrainLayerPath) && File.Exists(layerNode.terrainLayerPath))
        //                {
        //                    TerrainLayer layer = AssetDatabase.LoadAssetAtPath(layerNode.terrainLayerPath, typeof(TerrainLayer)) as TerrainLayer;
        //                    layersList.Add(layer);
        //                    detectedCount++;
        //                }
        //            }
        //        }
        //    }
        //
        //     if (layersList.Count > 0)
        //     {
        //         terrain.terrainData.terrainLayers = layersList.ToArray();
        //         terrain.Flush();
        //     }
        // }

        //private void GetTemplates ()
        //{
        //    //if (templateIndex == 0)
        //    //    presetNames = Directory.GetFiles(TAddresses.templatesPathProcedural, "*.twg", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
        //    //else if (templateIndex == 1)
        //    //    presetNames = Directory.GetFiles(TAddresses.templatesPathLandcover, "*.twg", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
        //
        //    presetNames = Directory.GetFiles(TAddresses.templatesPathPro, "*.twg", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
        //
        //    //if (templateIndex != -1 && presetNames != null && presetNames.Length > 0)
        //    //{
        //    //    //if (biomeIndex == -1)
        //    //    //THelpersUI.GUI_HelpBox("SELECT ONE OF THE FOLLOWING BIOME TYPES", MessageType.Warning, 10);
        //    //
        //    //    if (biomeIndex == -1)
        //    //        GUI.color = disabledColor;
        //    //    else
        //    //        GUI.color = enabledColor;
        //    //
        //    //    GUILayout.Space(5);
        //    //
        //    //    EditorGUILayout.BeginHorizontal();
        //    //    GUILayout.FlexibleSpace();
        //    //
        //    //    EditorGUI.BeginChangeCheck();
        //    //    if (templateIndex == 0)
        //    //    {
        //    //        style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 50; style.fixedHeight = 50;
        //    //
        //    //        //biomeIndexProcedural = GUILayout.SelectionGrid(biomeIndexProcedural, biomeIcons, 4, style);
        //    //        //biomeIndexProcedural = THelpersUI.GUI_Popup("PRESET: ", biomeIndexProcedural, presetNames);
        //    //    }
        //    //    else if (templateIndex == 1)
        //    //    {
        //    //        style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 50; style.fixedHeight = 50;
        //    //
        //    //        //biomeIndexLandcover = GUILayout.SelectionGrid(biomeIndexLandcover, biomeIcons, 4, style);
        //    //        //biomeIndexLandcover = THelpersUI.GUI_Popup("PRESET: ", biomeIndexLandcover, presetNames);
        //    //    }
        //    //
        //    //    if (EditorGUI.EndChangeCheck())
        //    //    {
        //    //        if (templateIndex == 0)
        //    //            biomeIndex = biomeIndexProcedural;
        //    //        else if (templateIndex == 1)
        //    //            biomeIndex = biomeIndexLandcover;
        //    //
        //    //        if (biomeIndex > presetNames.Length - 1)
        //    //        {
        //    //            EditorUtility.DisplayDialog("TEMPLATE MISSING", "Selected Template is not available in project!", "Ok");
        //    //            return;
        //    //        }
        //    //    }
        //    //
        //    //    GUILayout.FlexibleSpace();
        //    //    EditorGUILayout.EndHorizontal();
        //    //
        //    //    if (biomeIndex != -1)
        //    //    {
        //    //        style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 40; style.fixedHeight = 40;
        //    //        THelpersUI.GUI_Button(resetIcon, style, LoadTemplate);
        //    //    }
        //    //}
        //}

        //        private void LoadTemplate(string graphPath)
        //        {
        //            if (EditorUtility.DisplayDialog("LOAD TEMPLATE", "Are you sure you want to reset graph and load template " + Path.GetFileNameWithoutExtension(graphPath).ToUpper() + "?", "Yes", "No"))
        //            {
        //                templatePath = TAreaGraph.templatePath =  Path.GetDirectoryName(graphPath);;
        //                TAreaGraph.templatePathFull = graphPath;
        //                TTerraWorld.WorldGraph.SaveGraph(TAddresses.currentPresetPath.Replace(".twg", ".xml"));
        //                bool reGenerate = TTerraWorld.WorldGraph.LoadGraph(graphPath, true);
        //                InitializeScene();
        //
        //#if TERRAWORLD_PRO
        //                RenderingSettingsManager.UpdateTerrainLayers();
        //#else
        //                UpdateTerrainLayers();
        //#endif
        //
        //                if (reGenerate) GenerateWorld();
        //                AssetDatabase.Refresh();
        //            }
        //        }

        //        private void LoadGraphCustom(string path)
        //        {
        //            platformIndex = -1;
        //            templatePath = path;
        //            TAreaGraph.templatePath = templatePath;
        //            templateIsDisabled = TAreaGraph.templateIsDisabled = true;
        //            TAreaGraph.templatePathFull = "";
        //
        //            bool reGenerate = TTerraWorld.WorldGraph.LoadGraph(path,false);
        //            InitializeScene();
        //
        //#if TERRAWORLD_PRO
        //            RenderingSettingsManager.UpdateTerrainLayers();
        //#else
        //            UpdateTerrainLayers();
        //#endif
        //
        //            if (reGenerate) GenerateWorld();
        //        }

        private void LoadGraphOnLoad()
        {
            //if (!string.IsNullOrEmpty(TTerraWorldManager.TerraWorldGraphPath)) return;
            //Exception exception = null;
            //string oldPath = "Assets/TerraWorld/Core/Presets/Graph.xml";
            ////       string DemoPath = "Assets/TerraWorld/Scenes/Pro/Alps/Swiss Alps.xml";
            //
            //if (File.Exists(TTerraWorld.GraphPath))
            //    TTerraWorld.LoadWorldGraph(TTerraWorld.GraphPath, false, out exception, out bool reGenerate);
            //else if (File.Exists(oldPath))
            //    TTerraWorld.LoadWorldGraph(oldPath, false, out exception, out bool reGenerate);
            ////       else if (File.Exists(DemoPath))
            ////           TTerraWorld.LoadWorldGraph(DemoPath, false, out exception, out bool reGenerate);
            //
            //if (exception != null) throw exception;
        }

        // Terrain Rendering Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        //private void DefaultRenderingSettings()
        //{
        //    if (EditorUtility.DisplayDialog("TERRAWORLD", "RESET SETTINGS \n Are you sure you want to reset settings?", "No", "Yes"))
        //        return;
        //
        //    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "RESETSETTINGS");
        //    RenderingNode renderingModule = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode();
        //    renderingModule.ResetToDefaultSettings();
        //    InitRenderingSettings(renderingModule);
        //    SceneView.RepaintAll();
        //}

        private void TerrainRenderingSettings(ref RenderingNode module)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUIStyle style = new GUIStyle();
            style.fixedWidth = 64; style.fixedHeight = 64;
            lastRect = new Rect();
            lastRect.x += (position.width / 2) - 170;
            lastRect.y += 20;

            //THelpersUI.GUI_Label(TResourcesManager.terrainIcon, lastRect, style, 20, 0, Color.clear, Color.clear, Color.clear);
            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("TERRAIN RENDERING", ""), 0 , enabledColor, THelpersUI.UIColor);

            TerrainRenderingParams renderingParams = TerrainRenderingManager.GetParams();
            EditorGUI.BeginChangeCheck();  // Wraps around all TERRAIN tab

            if (TTerraWorldManager.MainTerrainGO == null)
                THelpersUI.GUI_HelpBoxInfo("No Terrains Detected!");
            else
            {
                TerrainRenderingModeUI(ref renderingParams);

                if (renderingParams.modernRendering)
                    TerrainRenderingUI(ref module, ref renderingParams);
            }

#if !TERRAWORLD_XPRO
            SplatmapsUI(ref renderingParams);
            BackgroundTerrainUI(ref module, ref renderingParams);
#endif
            if (EditorGUI.EndChangeCheck())  // Wraps around all TERRAIN tab
            {
                TerrainRenderingManager.SetParams(renderingParams);
            }
            GUI.color = enabledColor;
            GUILayout.Space(60);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void TerrainRenderingModeUI(ref TerrainRenderingParams renderingParams)
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            THelpersUI.GUI_HelpBox(new GUIContent("TERRAIN RENDERING MODE", "Select between the high-end & built-in terrain rendering"), true, 20);
            int state = Convert.ToInt32(!renderingParams.modernRendering);
            EditorGUI.BeginChangeCheck();
            state = THelpersUI.GUI_SelectionGrid(state, renderingMode, style, -10);

            if (EditorGUI.EndChangeCheck())
            {
                renderingParams.modernRendering = !Convert.ToBoolean(state);

                if (renderingParams.modernRendering)
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "modernRendering", 1);
                else
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "modernRendering", 0);
            }

            if (TerrainRenderingManager.TerrainMaterial != null)
                THelpersUI.GUI_ObjectField(new GUIContent("USED MATERIAL", "Material used for scene's terrain rendering"), TerrainRenderingManager.TerrainMaterial, typeof(GameObject), null, 10);
            else
                THelpersUI.GUI_HelpBox("NO MATERIALS DETECTED ON TERRAIN!", MessageType.Warning, -10);

            GUILayout.Space(15);
        }

        private void TerrainRenderingUI(ref RenderingNode module , ref TerrainRenderingParams terrainRenderingParams)
        {
            int previewSize = 128;
            int padLeft = 150;
            int extraSpace = -10;
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);

            if (THelpersUI.SectionSettingsWithTitle(ref module.sectionToggles.Settings1, "RENDERING"))
            {
                // Surface Tint
                //-----------------------------------------------------------------------

                THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SURFACE TINT", "Surface Tint Color affecting all features on surface"), 10, enabledColor, THelpersUI.UIColor);

                Color colorValue = Color.white;

                // Global Color Tint
                //-----------------------------------------------------------------------

                terrainRenderingParams.surfaceTintColorMAIN = TUtils.UnityColorToVector4(THelpersUI.GUI_ColorField(new GUIContent("TERRAINS COLOR TINT", "Color tint on main terrain surface"), TUtils.Vector4ToUnityColor(terrainRenderingParams.surfaceTintColorMAIN), 20));
                terrainRenderingParams.surfaceTintColorBG = TUtils.UnityColorToVector4(THelpersUI.GUI_ColorField(new GUIContent("BACKGROUND TERRAINS COLOR TINT", "Color tint on background terrain surface"), TUtils.Vector4ToUnityColor(terrainRenderingParams.surfaceTintColorBG), 20));

                // Tessellation
                //-----------------------------------------------------------------------

                //EditorGUI.BeginDisabledGroup(module.terrainRenderingParams.instancedDrawing );
                THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("TESSELLATION", "Activate DX11 Tessellation on surface"), 20, enabledColor, THelpersUI.UIColor);
                int state = Convert.ToInt32(!terrainRenderingParams.tessellation);
                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    terrainRenderingParams.tessellation = !Convert.ToBoolean(state);

                    if (terrainRenderingParams.tessellation)
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "tessellation", 1);
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "tessellation", 0);
                }

                if (terrainRenderingParams.tessellation)
                {
                    THelpersUI.GUI_HelpBox("DISPLACED SURFACE IS HAPPENING ON GPU ONLY AND DOES NOT CONTRIBUTE TO PHYSICS! SO OBJECTS WITH COLLIDERS MAY FALL THROUGH THE SURFACE!", MessageType.Error);
                    THelpersUI.GUI_HelpBox("FOR THE BEST PERFORMANCE, TURN THIS FEATURE OFF. TESSELLATION WILL DROP FRAMERATE ESPECIALLY ON MOBILE/VR PLATFORMS!", MessageType.Info);
                    
                    if (TerrainRenderingManager.TerrainLayersCount > 4)
                        THelpersUI.GUI_HelpBox("MORE THAN 4 LAYERS IN TESSELLATION SHADER IS EXPERIMENTAL AND MAY BRING TEXTURE BLENDING ISSUES. IT IS RECOMMENDED TO TURN OFF TESSELLATION!", MessageType.Error);

                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings1))
                    {
                        terrainRenderingParams.tessellationQuality = THelpersUI.GUI_Slider(new GUIContent("QUALITY", "Tessellation quality on surface"), terrainRenderingParams.tessellationQuality, 0f, 100f);
                        
                        if (terrainRenderingParams.tessellationQuality > 50)
                            THelpersUI.GUI_HelpBox("SUBDIVIDING EXCESSIVE POLYGONS WILL RESULT IN HEAVY GPU LOAD!", MessageType.Warning, -5);

                        terrainRenderingParams.edgeSmoothness = THelpersUI.GUI_Slider(new GUIContent("EDGE SMOOTHNESS", "Phong smoothness on surface edges"), terrainRenderingParams.edgeSmoothness, 0f, 10f, 5);
                    }
                }


                // Heightmap Blending
                //-----------------------------------------------------------------------

                THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("HEIGHTMAP BLENDING", "Activate Heightmap Blending on surface"), 20, enabledColor, THelpersUI.UIColor);
                state = Convert.ToInt32(!terrainRenderingParams.heightmapBlending);
                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    terrainRenderingParams.heightmapBlending = !Convert.ToBoolean(state);

                    if (terrainRenderingParams.heightmapBlending)
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "heightmapBlending", 1);
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "heightmapBlending", 0);
                }

                EditorGUI.BeginDisabledGroup(!terrainRenderingParams.heightmapBlending);
                if (THelpersUI.SectionSettings(ref module.uIToggles.Settings2))
                {
                    terrainRenderingParams.heightBlending = THelpersUI.GUI_Slider(new GUIContent("HEIGHT BLENDING", "Heightmap Blending range between terrain layers"), terrainRenderingParams.heightBlending, 0.001f, 1f);
                }
                EditorGUI.EndDisabledGroup();


                // Colormap Blending
                //-----------------------------------------------------------------------

                THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("COLORMAP BLENDING", "Activate Colormap Blending on surface"), 20, enabledColor, THelpersUI.UIColor);
                state = Convert.ToInt32(!terrainRenderingParams.colormapBlending);
                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    terrainRenderingParams.colormapBlending = !Convert.ToBoolean(state);

                    if (terrainRenderingParams.colormapBlending)
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "colormapBlending", 1);
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "colormapBlending", 0);
                }

                if (TerrainRenderingManager.ColormapTexture == null)
                    THelpersUI.GUI_HelpBox("NO COLORMAP TEXTURES FOUND ON TERRAIN MATERIAL", MessageType.Warning);

                EditorGUI.BeginDisabledGroup(!terrainRenderingParams.colormapBlending || !TerrainRenderingManager.ColormapTexture);
                if (THelpersUI.SectionSettings(ref module.uIToggles.Settings3))
                {
                    style = new GUIStyle(); style.fixedWidth = previewSize; style.fixedHeight = previewSize;

                    //TODO: Ability to add/remove/replace colormap texture through its slot
                    lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.x += 5;
                    lastRect.y += previewSize / 2;
                    THelpersUI.GUI_Label(TerrainRenderingManager.ColormapTexture, lastRect, style);
                    terrainRenderingParams.colormapBlendingDistance = THelpersUI.GUI_Slider(new GUIContent("DISTANCE", "Blending distance between Colormap & detail textures"), terrainRenderingParams.colormapBlendingDistance, 0f, 50000f, 0, padLeft);  
                }
                EditorGUI.EndDisabledGroup();


                // Procedural Snow
                //-----------------------------------------------------------------------

                style = new GUIStyle(EditorStyles.toolbarButton);

                if (module.uIToggles.Settings3)
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("PROCEDURAL SNOW", "Activate Procedural Snow on surface"), 140, enabledColor, THelpersUI.UIColor);
                else
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("PROCEDURAL SNOW", "Activate Procedural Snow on surface"), 20, enabledColor, THelpersUI.UIColor);

                SceneSettingsManager.VFXData _VFXData = SceneSettingsManager.GetVFXData();

                if (_VFXData.Enabled && _VFXData.snowManagerParams.hasSnow)
                    THelpersUI.GUI_HelpBox("GLOBAL SNOW IS ENABLED VIA VFX TAB", MessageType.Info);
                else
                {
                    if (!_VFXData.Enabled)
                        THelpersUI.GUI_HelpBox("EFFECTS & POST PROCESSING FEATURES ARE TURNED OFF IN VFX TAB", MessageType.Warning);
                    else if (!_VFXData.snowManagerParams.hasSnow)
                        THelpersUI.GUI_HelpBox("GLOBAL SNOW IS DISABLED IN VFX TAB", MessageType.Warning);
                }

                EditorGUI.BeginDisabledGroup(!_VFXData.Enabled || !_VFXData.snowManagerParams.hasSnow);
                if (THelpersUI.SectionSettings(ref module.uIToggles.Settings4))
                {
                    style = new GUIStyle(); style.fixedWidth = previewSize; style.fixedHeight = previewSize;
                    lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.x += 5;
                    lastRect.y += previewSize / 2;
                    THelpersUI.GUI_Label(TerrainRenderingManager.SnowTexture, lastRect, style);

                    style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 170; style.fixedHeight = 25;
                    THelpersUI.GUI_Button(new GUIContent("GLOBAL SNOW SETTINGS", "Go to Global Snow Settings"), style, GoToGlobalSnowSettings, 0, padLeft);

                    // Snow color
                    EditorGUI.BeginChangeCheck();
                    colorValue = new Color(terrainRenderingParams.snowColorR, terrainRenderingParams.snowColorG, terrainRenderingParams.snowColorB);
                    colorValue = THelpersUI.GUI_ColorField(new GUIContent("SNOW COLOR", "Snow color on terrain surface"), colorValue, 20, padLeft);
                    if (EditorGUI.EndChangeCheck())
                    {
                        terrainRenderingParams.snowColorR = colorValue.r;
                        terrainRenderingParams.snowColorG = colorValue.g;
                        terrainRenderingParams.snowColorB = colorValue.b;
                    }

                    terrainRenderingParams.snowTiling = THelpersUI.GUI_Slider(new GUIContent("TILING", "Texture tiling for snow rendering"), terrainRenderingParams.snowTiling, 0.001f, 10000f, extraSpace, padLeft);
                    terrainRenderingParams.snowAmount = THelpersUI.GUI_Slider(new GUIContent("AMOUNT", "Amount value for snow rendering"), terrainRenderingParams.snowAmount, 0.001f, 1f, extraSpace, padLeft);
                    terrainRenderingParams.snowAngles = THelpersUI.GUI_Slider(new GUIContent("ANGLES", "Angles distribution for snow rendering"), terrainRenderingParams.snowAngles, -1f, 1f, extraSpace, padLeft);
                    terrainRenderingParams.snowNormalInfluence = THelpersUI.GUI_Slider(new GUIContent("NORMAL INFLUENCE", "Detail textures normalmap influence for snow rendering"), terrainRenderingParams.snowNormalInfluence, 0f, 1f, extraSpace, padLeft);
                    terrainRenderingParams.snowPower = THelpersUI.GUI_Slider(new GUIContent("POWER", "Power value for snow rendering"), terrainRenderingParams.snowPower, 0.1f, 2f, extraSpace, padLeft);
                    terrainRenderingParams.snowSmoothness = THelpersUI.GUI_Slider(new GUIContent("SMOOTHNESS", "Smoothness value for snow rendering"), terrainRenderingParams.snowSmoothness, 0f, 1f, extraSpace, padLeft);

                    //module.terrainRenderingParams.snowFalloff = THelpersUI.GUI_Slider(new GUIContent("FALLOFF", "Falloff intensity in vertical axis for snow rendering"), module.terrainRenderingParams.snowFalloff, 0f, 10000f, extraSpace, padLeft);
                    //module.snowMetallic = THelpersUI.GUI_Slider(new GUIContent("METALLIC", "Metallic value for snow rendering"), module.snowMetallic, 0f, 1f, extraSpace, padLeft);
                }
                EditorGUI.EndDisabledGroup();


                // Procedural Puddles
                //-----------------------------------------------------------------------

                style = new GUIStyle(EditorStyles.toolbarButton);
                THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("PROCEDURAL PUDDLES", "Activate Procedural Puddles on surface"), 20, enabledColor, THelpersUI.UIColor);
                state = Convert.ToInt32(!terrainRenderingParams.proceduralPuddles);
                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    terrainRenderingParams.proceduralPuddles = !Convert.ToBoolean(state);

                    if (terrainRenderingParams.proceduralPuddles)
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "proceduralPuddles", 1);
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "proceduralPuddles", 0);
                }

                //EditorGUI.BeginDisabledGroup(!module.terrainRenderingParams.proceduralPuddles || TTerraWorld.worldReference == null);
                EditorGUI.BeginDisabledGroup(!terrainRenderingParams.proceduralPuddles);
                if (THelpersUI.SectionSettings(ref module.uIToggles.Settings13))
                {
                    //style = new GUIStyle(); style.fixedWidth = previewSize; style.fixedHeight = previewSize;
                    //lastRect = GUILayoutUtility.GetLastRect();
                    //lastRect.x += 5;
                    //lastRect.y += previewSize / 2;
                    //THelpersUI.GUI_Label(TerrainRenderingManager.NoiseTexture, lastRect, style);

                    EditorGUI.BeginChangeCheck();
                    colorValue = new Color(terrainRenderingParams.puddleColorR, terrainRenderingParams.puddleColorG, terrainRenderingParams.puddleColorB);
                    colorValue = THelpersUI.GUI_ColorField(new GUIContent("PUDDLE COLOR", "Puddle color on terrain surface"), colorValue, 0);
                    if (EditorGUI.EndChangeCheck())
                    {
                        terrainRenderingParams.puddleColorR = colorValue.r;
                        terrainRenderingParams.puddleColorG = colorValue.g;
                        terrainRenderingParams.puddleColorB = colorValue.b;
                    }

                    terrainRenderingParams.puddleRefraction = THelpersUI.GUI_Slider(new GUIContent("REFRACTION", "Refraction value for puddle rendering"), terrainRenderingParams.puddleRefraction, 0f, 1f, extraSpace);
                    terrainRenderingParams.puddleMetallic = THelpersUI.GUI_Slider(new GUIContent("METALLIC", "Metallic value for puddle rendering"), terrainRenderingParams.puddleMetallic, 0f, 1f, extraSpace);
                    terrainRenderingParams.puddleSmoothness = THelpersUI.GUI_Slider(new GUIContent("SMOOTHNESS", "Smoothness value for puddle rendering"), terrainRenderingParams.puddleSmoothness, 0f, 1f, extraSpace);
                    terrainRenderingParams.puddleSlope = THelpersUI.GUI_Slider(new GUIContent("MAX. SLOPE", "Maximum slope value for puddle rendering"), terrainRenderingParams.puddleSlope, 0.0000001f, 0.01f, extraSpace);
                    terrainRenderingParams.puddleMinSlope = THelpersUI.GUI_Slider(new GUIContent("MIN. SLOPE", "Minimum slope value for puddle rendering"), terrainRenderingParams.puddleMinSlope, 0f, 0.01f, extraSpace);
                    terrainRenderingParams.puddleNoiseTiling = THelpersUI.GUI_Slider(new GUIContent("NOISE TILING", "Noise tiling value for puddle rendering"), terrainRenderingParams.puddleNoiseTiling, 0f, 1000f, extraSpace);

                    // TerrainRenderingManager.PuddlewaterHeight = THelpersUI.GUI_Slider(new GUIContent("WATER HEIGHT", "Water height value for puddle rendering"), TerrainRenderingManager.PuddlewaterHeight, 0f, 1f, extraSpace, padLeft);
                    // TerrainRenderingManager.PuddleNoiseInfluence = THelpersUI.GUI_Slider(new GUIContent("NOISE INFLUENCE", "noise influence value for puddle rendering"), TerrainRenderingManager.PuddleNoiseInfluence, 0.001f, 1f, extraSpace, padLeft);
                    //
                    // EditorGUI.BeginChangeCheck();
                    // TerrainRenderingManager.PuddleReflections = THelpersUI.GUI_Toggle(new GUIContent("REAL-TIME REFLECTIONS", "Real-time reflections around camera on puddles and metallic surfaces"), TerrainRenderingManager.PuddleReflections, extraSpace, padLeft);
                    // if (EditorGUI.EndChangeCheck()) SceneSettingsManager.SwitchRealTimeReflections();
                }
                EditorGUI.EndDisabledGroup();


                // Terrain Material Per Layer Settings
                //-----------------------------------------------------------------------

                if (TerrainRenderingManager.TerrainLayersCount == 0)
                    GUI.color = enabledColor;
                else if (terrainRenderingParams.modernRendering)
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("TERRAIN LAYERS", "Terrain Layers available on terrain"), 60, enabledColor, THelpersUI.UIColor);

                GUILayout.Space(10);

                if (terrainRenderingParams.modernRendering)
                {
                    int sectionSpace = 50;
                    style = new GUIStyle(); style.fixedWidth = previewSize; style.fixedHeight = previewSize;
                    if (toggles == null || toggles.Length != TerrainRenderingManager.TerrainLayersCount) toggles = new bool[TerrainRenderingManager.TerrainLayersCount];
                    float valueF = 0;
                    Vector4 vector = Vector4.zero;
                    bool isEnabled = false;

                    for (int i = 0; i < TerrainRenderingManager.TerrainLayersCount; i++)
                    {
                        if (i == 4 && TerrainRenderingManager.TerrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer"))
                            THelpersUI.GUI_HelpBox("MORE THAN 4 LAYERS IN TESSELLATION SHADER IS EXPERIMENTAL AND MAY BRING TEXTURE BLENDING ISSUES. IT IS RECOMMENDED TO TURN OFF TESSELLATION!", MessageType.Error);

                        lastRect = GUILayoutUtility.GetLastRect();

                        if (i == 0)
                            lastRect.y += sectionSpace;
                        else
                            lastRect.y += (previewSize + sectionSpace) / 2;

                        TerrainLayer terrainLayer = TerrainRenderingManager.GetTerrainLayer(i);

                        if (terrainLayer != null)
                        {
                            THelpersUI.GUI_TerrainLayerSection(i, 10, padLeft, lastRect, style, terrainLayer);

                            if (THelpersUI.SectionSettings(ref toggles[i], padLeft))
                            {
                                EditorGUI.BeginChangeCheck();
                                colorValue = TerrainRenderingManager.GetLayerColorUI(ref terrainRenderingParams, i);
                                colorValue = THelpersUI.GUI_ColorField(new GUIContent("COLOR TINT", "Layer's color tint on terrain surface"), colorValue, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerColorUI(ref terrainRenderingParams, i, colorValue);

                                if (terrainRenderingParams.tessellation)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    valueF = TerrainRenderingManager.GetDisplacementUI(ref terrainRenderingParams, i);
                                    valueF = THelpersUI.GUI_Slider(new GUIContent("TESSELLATION DISPLACEMENT", "Tessellation displacement taken from layer heightmap on surface"), valueF, 0.1f, 32f, extraSpace, padLeft);
                                    if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetDisplacementUI(ref terrainRenderingParams,i, valueF);
                                    
                                    EditorGUI.BeginChangeCheck();
                                    valueF = TerrainRenderingManager.GetHeightOffsetUI(ref terrainRenderingParams, i);
                                    valueF = THelpersUI.GUI_Slider(new GUIContent("HEIGHT OFFSET", "Layer height offset on surface"), valueF, 0.1f, 8f, extraSpace, padLeft);
                                    if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetHeightOffsetUI(ref terrainRenderingParams, i, valueF);
                                }

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetTileRemoverUI(ref terrainRenderingParams, i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("TILING REMOVER INTENSITY", "Tiling Remover intensity"), valueF, 0f, 64f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetTileRemoverUI(ref terrainRenderingParams, i, valueF);

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetNoiseTilingUI(ref terrainRenderingParams, i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("TILING REMOVER NOISE SCALE", "Tiling Remover noise scale"), valueF, 0f, 200f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetNoseTilingUI(ref terrainRenderingParams, i, valueF);

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetLayerNormalScaleUI(ref terrainRenderingParams, i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("NORMAL STRENGTH", "Normalmap strength for terrain layer"), valueF, 0f, 4f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerNormalScaleUI(ref terrainRenderingParams, i, valueF);

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetLayerMetallicUI(ref terrainRenderingParams, i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("METALLIC", "Metallic strength for terrain layer"), valueF, 0f, 1f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerMetallicUI(ref terrainRenderingParams, i, valueF);

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetLayerSmoothnessUI(ref terrainRenderingParams, i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("SMOOTHNESS", "Smoothness strength for terrain layer"), valueF, 0f, 1f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerSmoothnessUI(ref terrainRenderingParams, i, valueF);

                                EditorGUI.BeginChangeCheck();
                                valueF = TerrainRenderingManager.GetLayerAOUI(ref terrainRenderingParams,i);
                                valueF = THelpersUI.GUI_Slider(new GUIContent("AMBIENT OCCLUSION (AO)", "Ambient Occlusion strength for terrain layer"), valueF, 0f, 1f, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerAOUI(ref terrainRenderingParams,i, valueF);

                                EditorGUI.BeginChangeCheck();
                                vector = TerrainRenderingManager.GetLayerTilingUI(ref terrainRenderingParams, i);
                                vector = THelpersUI.GUI_Vector4Field(new GUIContent("TILING", "X = SizeX, Y = SizeY, Z = OffsetX, W = OffsetY"), vector, extraSpace, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerTilingUI(ref terrainRenderingParams, i, vector);

                                EditorGUI.BeginChangeCheck();
                                isEnabled = TerrainRenderingManager.GetLayerProceduralTexturingUI(ref terrainRenderingParams, i);
                                isEnabled = THelpersUI.GUI_Toggle(new GUIContent("PROCEDURAL TEXTURING", "Procedural Texturing based on automatic calculations on GPU suited for generic global terrain layers by bringing micro details and color variations using normalmaps in terrain layer"), isEnabled, extraSpace + 20, padLeft);
                                if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerProceduralTexturingUI(ref terrainRenderingParams, i, isEnabled);
                                
                                if (isEnabled)
                                {
                                    if (terrainLayer.normalMapTexture == null)
                                        THelpersUI.GUI_HelpBox("PROCEDURAL TEXTURING USES NORMALMAP TO CREATE DETAILS ON SURFACE! PLEASE INSERT A NORMALMAP ON THIS TERRAIN LAYER!", MessageType.Error, -7);
                                    else if (terrainLayer.normalScale <= 0)
                                        THelpersUI.GUI_HelpBox("PROCEDURAL TEXTURING USES NORMALMAP TO CREATE DETAILS ON SURFACE! PLEASE INCREASE NORMAL SCALE VALUE ON THIS TERRAIN LAYER!", MessageType.Error, -7);

                                    GUILayout.Space(10);
                                }

                                if (TerrainRenderingManager.GetLayerProceduralTexturingUI(ref terrainRenderingParams, i))
                                {
                                    EditorGUI.BeginChangeCheck();
                                    isEnabled = TerrainRenderingManager.GetLayerGradientTexturingUI(ref terrainRenderingParams, i);
                                    isEnabled = THelpersUI.GUI_Toggle(new GUIContent("GRADIENT TEXTURING", "Is Diffuse Texture a Gradient image?"), isEnabled, extraSpace, padLeft);
                                    if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerGradientTexturingUI(ref terrainRenderingParams, i, isEnabled);

                                    if (TerrainRenderingManager.GetLayerGradientTexturingUI(ref terrainRenderingParams, i))
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        valueF = TerrainRenderingManager.GetLayerGradientTiling(ref terrainRenderingParams, i);
                                        valueF = THelpersUI.GUI_Slider(new GUIContent("GRADIENT TILING", "Vertical tiling for gradient texturing effect"), valueF, 0f, 1f, extraSpace, padLeft);
                                        if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerGradientTiling(ref terrainRenderingParams, i, valueF);
                                    }

                                    EditorGUI.BeginChangeCheck();
                                    valueF = TerrainRenderingManager.GetLayerColorDamping(ref terrainRenderingParams, i);
                                    valueF = THelpersUI.GUI_Slider(new GUIContent("SLOPE COLOR DAMPING", "Tint color variations based on terrain slope"), valueF, -0.1f, 0.1f, extraSpace, padLeft);
                                    if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerColorDamping(ref terrainRenderingParams, i, valueF);

                                    EditorGUI.BeginChangeCheck();
                                    valueF = TerrainRenderingManager.GetLayerSlopeInfluence(ref terrainRenderingParams, i);
                                    valueF = THelpersUI.GUI_Slider(new GUIContent("SLOPE INFLUENCE", "Slope influence for tint color variations"), valueF, 0f, 1f, extraSpace, padLeft);
                                    if (EditorGUI.EndChangeCheck()) TerrainRenderingManager.SetLayerSlopeInfluence(ref terrainRenderingParams, i, valueF);
                                }
                            }
                        }

                        GUILayout.Space(sectionSpace);
                    }
                }

                GUILayout.Space(70);
            }
        }

        private void GoToGlobalSnowSettings()
        {
            //SwitchTabAndScroll("FX", 1745);
            SwitchTabAndScroll("FX", 0);
        }

        private void SplatmapsUI(ref TerrainRenderingParams renderingParams)
        {
#if !TERRAWORLD_XPRO
            //GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SPLATMAPS", ""), 20, enabledColor, THelpersUI.UIColor);
            //style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;

            //if (THelpersUI.SectionSettingsWithTitle(ref module.sectionToggles.Settings2, "SPLATMAPS"))
            {
                //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SPLATMAPS", "Splatmap settings used on terrains for texturing"), 10, enabledColor, THelpersUI.UIColor);
                renderingParams.splatmapResolutionBestFit = THelpersUI.GUI_Toggle(new GUIContent("BEST FIT", "Automatically calculate splatmap resolution"), renderingParams.splatmapResolutionBestFit);
                
                if (!renderingParams.splatmapResolutionBestFit)
                {
                    renderingParams.splatmapResolution = THelpersUI.GUI_IntSlider(new GUIContent("RESOLUTION", "Splatmap Resolution for texture painting on terrain"), renderingParams.splatmapResolution, 64, 2048, -10);
                    renderingParams.splatmapResolution = TUtils.nearestPowerOfTwo(renderingParams.splatmapResolution);
                }
                
                renderingParams.splatmapSmoothness = THelpersUI.GUI_IntSlider(new GUIContent("SMOOTHNESS", "Smoothness iterations for the splatmap channels blending"), renderingParams.splatmapSmoothness, 0, 4, -10);
            }
#endif
        }

        //private void MainTerrainUI(ref RenderingNode module)
        //{
        //    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("TERRAINS", ""), 20, enabledColor, THelpersUI.UIColor);
        //
        //    {
        //        TerrainRenderingManager.TerrainPixelError = THelpersUI.GUI_IntSlider(new GUIContent("PIXEL ERROR", "Pixel Error value for the surface quality of terrains"), TerrainRenderingManager.TerrainPixelError, 1, 200);
        //    }
        //}

        private void BackgroundTerrainUI(ref RenderingNode module , ref TerrainRenderingParams renderingParams)
        {
#if !TERRAWORLD_XPRO
            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("BACKGROUND TERRAIN", ""), 20, enabledColor, THelpersUI.UIColor);
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);

            //if (THelpersUI.SectionSettingsWithTitle(ref module.sectionToggles.Settings4, "BACKGROUND TERRAIN"))
            {
                //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("BACKGROUND TERRAIN", "Background Terrain settings"), 10, enabledColor, THelpersUI.UIColor);
                int selectionIndexBGMountains = renderingParams.BGMountains ? 0 : 1;
                selectionIndexBGMountains = THelpersUI.GUI_SelectionGrid(selectionIndexBGMountains, onOffSelection, style);
                renderingParams.BGMountains = selectionIndexBGMountains == 0 ? true : false;

                if (renderingParams.BGMountains)
                {
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings1))
                    {
                        renderingParams.BGTerrainScaleMultiplier = THelpersUI.GUI_IntSlider(new GUIContent("AREA MULTIPLIER", "Background Terrain's area size is value multiplied with the original world size"), renderingParams.BGTerrainScaleMultiplier, 2, 10, -10);
                        
                        renderingParams.BGTerrainHeightmapResolution = THelpersUI.GUI_IntSlider(new GUIContent("HEIGHTMAP RESOLUTION", "Select heightmap resolution for the background world"), renderingParams.BGTerrainHeightmapResolution, 128, 4096, -10);
                        renderingParams.BGTerrainHeightmapResolution = TUtils.nearestPowerOfTwo(renderingParams.BGTerrainHeightmapResolution);

                        renderingParams.BGTerrainSatelliteImageResolution = THelpersUI.GUI_IntSlider(new GUIContent("IMAGE RESOLUTION", "Select satellite image resolution for the background world"), renderingParams.BGTerrainSatelliteImageResolution, 32, 4096, -10);
                        renderingParams.BGTerrainSatelliteImageResolution = TUtils.nearestPowerOfTwo(renderingParams.BGTerrainSatelliteImageResolution);

                        renderingParams.BGTerrainPixelError = THelpersUI.GUI_IntSlider(new GUIContent("PIXEL ERROR", "Pixel Error factor for the background terrain surface quality"), renderingParams.BGTerrainPixelError, 1, 200, -10);
                        renderingParams.BGTerrainOffset = THelpersUI.GUI_FloatField(new GUIContent("Y OFFSET", "Offset in meters for the background terrain's Y position"), renderingParams.BGTerrainOffset, -2000, 2000, -10);
                    }
                }
            }
#endif
        }

        //private void TerrainSettingsDefaultsUI(ref RenderingNode module)
        //{
        //    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("DEFAULT SETTINGS", ""), 20, enabledColor, THelpersUI.UIColor);
        //    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
        //
        //    //if (THelpersUI.SectionSettingsWithTitle(ref module.sectionToggles.Settings5, "DEFAULT SETTINGS"))
        //    //{
        //    //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("DEFAULT SETTINGS", "Reset to default settings"), 30, enabledColor, THelpersUI.UIColor);
        //    style.fixedWidth = 45; style.fixedHeight = 45;
        //    THelpersUI.GUI_Button(new GUIContent(TResourcesManager.resetIcon, "Reset to Default settings"), style, DefaultRenderingSettings, 10);
        //    //}
        //}


        // FX Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void EditFXSettings()
        {
#if TERRAWORLD_PRO
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;

            EditorGUIUtility.PingObject(TTerraWorldManager.SceneSettingsGO1);
            Selection.activeObject = TTerraWorldManager.SceneSettingsGO1;
            Type windowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorWindow window = GetWindow(windowType);
            window.Focus();
#endif
        }

        private void VFXSettings(ref FXNode module)
        {
#if TERRAWORLD_PRO
            //FXParams fXParamsUI = SceneSettingsManager.GetFXParams();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUIStyle style = new GUIStyle();

            if (!SceneSettingsManager.Active)
            {
                GUIStyle _style = new GUIStyle(EditorStyles.toolbarButton); _style.fixedHeight = 32;
                THelpersUI.GUI_Button(new GUIContent("ACTIVATE VFX FEATURES", "Activate TerraWorld's Custom VFX features!"), _style, ActivateVFX, 20, 0, new Color(0.8f, 0.4f, 0.4f, 1f));
                ShowNotification(new GUIContent("TerraWorld's Custom VFX features\nare not active!", ""));
            }
            else if (projectIsSRP)
            {
                ShowNotification(new GUIContent("TerraWorld's Custom VFX features are not supported in Scriptable Rndering Pipelines!", ""));
            }
            else
            {
                SceneSettingsManager.VFXData _VFXData = SceneSettingsManager.GetVFXData();

                if (lastTab != activeTab)
                {
                    PostProcessingList("com.unity.postprocessing");

                    if (_VFXData.windManagerParams.showWindGizmo)
                        ExpandWindManagerInspector();
                }

                if (EditorGUIUtility.isProSkin) style.normal.textColor = Color.white;
                else style.normal.textColor = Color.black;
                lastRect = new Rect();

                if (_VFXData.Enabled) lastRect.x = (position.width / 2) + 10;
                else lastRect.x = (position.width / 2) + 20;
                lastRect.y += 42;

                THelpersUI.GUI_Label(new GUIContent("VFX", "Switch VFX Settings"), lastRect, style);
                style = new GUIStyle(); style.fixedWidth = 64; style.fixedHeight = 64;
                THelpersUI.GUI_Label(TResourcesManager.VFXIcon, style, -10, -30);

#region VFX
                EditorGUI.BeginChangeCheck(); // Wraps around all VFX tab

#region VFX state
                EditorGUI.BeginChangeCheck();
                style = new GUIStyle(EditorStyles.toolbarButton);

                int state = Convert.ToInt32(!_VFXData.Enabled);
                state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                if (EditorGUI.EndChangeCheck())
                {
                    _VFXData.Enabled = !Convert.ToBoolean(state);

                    if (_VFXData.Enabled)
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "selectionIndexVFX", 1);
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "selectionIndexVFX", 0);

                    RemoveNotification();
                }
#endregion
                if (!_VFXData.Enabled)
                {
                    ShowNotification(new GUIContent("Turn on VFX\nto achieve high-end graphics!", "Turn on VFX to achieve high-end graphics!"));
                }
                else
                {
#region Time Of Day
                    // Time Of Day settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("DAY NIGHT CYCLE", "Is dynamic time of day enabled?"), 20, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.timeOfDayIcon, style);
                    style = new GUIStyle(EditorStyles.toolbarButton);

                    EditorGUI.BeginChangeCheck(); // Wraps around Time of Day settings

                    EditorGUI.BeginChangeCheck();
                    _VFXData.timeOfDayParams.dayNightControl = (TimeOfDayControls)THelpersUI.GUI_SelectionGrid((int)_VFXData.timeOfDayParams.dayNightControl, dayNightMode, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "dayNightControl", (int)_VFXData.timeOfDayParams.dayNightControl);
                    }

                    //TODO: Example of how to convert variables to serializable properties for Undo and serialization operations
                    //SerializedObject serializedAsset = new SerializedObject(TTerraWorldManager.SceneSettingsGO1.GetComponent<TimeOfDay>());
                    //serializedAsset.Update();
                    //SerializedProperty xxx = serializedAsset.FindProperty("elevation");
                    //EditorGUILayout.PropertyField(xxx, new GUIContent("Models Distance", "Distance between placed instances"));
                    //_VFXData.timeOfDayParams.elevation = xxx.floatValue;
                    //serializedAsset.ApplyModifiedProperties();

                    THelpersUI.GUI_HelpBox("\nBYPASS:  Bypasses Sun, Sky, Lighting & Day/Night controls for user-customized controls\n\nSTATIC TIME:  Static time of day controls with synced lighting\n\nDAY/NIGHT CYCLE:  Day/Night cycle with dynamic lighting and global time of day speed\n", MessageType.Info);

                    if (_VFXData.timeOfDayParams.dayNightControl != 0)
                    {
                        if (THelpersUI.SectionSettings(ref module.uIToggles.Settings1))
                        {
                            if (_VFXData.timeOfDayParams.dayNightControl != TimeOfDayControls.Dynamic)
                            {
                                _VFXData.timeOfDayParams.elevation = THelpersUI.GUI_Slider(new GUIContent("SUN ELEVATION", "elavation of the Sun in the world"), _VFXData.timeOfDayParams.elevation, 0f, 359f);
                                _VFXData.timeOfDayParams.azimuth = THelpersUI.GUI_Slider(new GUIContent("AZIMUTH", "azimuth of the Sun in the world"), _VFXData.timeOfDayParams.azimuth, 0f, 359f, -10);
                            }
                            else
                            {
                                _VFXData.timeOfDayParams.dayNightSpeed = THelpersUI.GUI_Slider(new GUIContent("GLOBAL SPEED", "Global speed for the day/night cycle"), _VFXData.timeOfDayParams.dayNightSpeed, 1f, 12f);
                                int updateIntervalInMiliSeconds = Mathf.FloorToInt ((234858f / Mathf.Exp(_VFXData.timeOfDayParams.dayNightSpeed)));
                                TimeSpan t = TimeSpan.FromSeconds(updateIntervalInMiliSeconds);
                                THelpersUI.GUI_HelpBox("Day/Night Cycle Time : " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s",t.Hours, t.Minutes, t.Seconds), MessageType.None, -10);

                                _VFXData.timeOfDayParams.nightSpeedRatio = THelpersUI.GUI_Slider(new GUIContent("NIGHT SPEED RATIO", "Night time speed ratio compared to day time frame"), _VFXData.timeOfDayParams.nightSpeedRatio, 0.1f, 10f);
                            }

                            if (_VFXData.timeOfDayParams.dayNightControl == TimeOfDayControls.Dynamic)
                            {
                                if (_VFXData.timeOfDayParams.dayNightSpeed > 3)
                                    THelpersUI.GUI_HelpBox("LIGHTING WILL UPDATE EVERY FRAME AND HEAVILY AFFECTS PERFORMANCE!", MessageType.Error);
                            }

                            _VFXData.timeOfDayParams.enableFog = THelpersUI.GUI_Toggle(new GUIContent("ENABLE FOG", "Enable Aerial Fog for atmospheric scattering based on time of day"), _VFXData.timeOfDayParams.enableFog, 10);
                            
                            if (_VFXData.timeOfDayParams.enableFog)
                            {
                                _VFXData.timeOfDayParams.globalFogMode = (GlobalFogMode)THelpersUI.GUI_EnumPopup(new GUIContent("MODE", "Scattering mode for global fog in scene"), _VFXData.timeOfDayParams.globalFogMode, -5);

                                if (_VFXData.timeOfDayParams.globalFogMode == GlobalFogMode.Exponential || _VFXData.timeOfDayParams.globalFogMode == GlobalFogMode.ExponentialSquared)
                                    _VFXData.timeOfDayParams.fogDensity = THelpersUI.GUI_Slider(new GUIContent("DENSITY", "Density of fog within scattering range"), _VFXData.timeOfDayParams.fogDensity, 0.00005f, 0.0005f, -10);
                                else if (_VFXData.timeOfDayParams.globalFogMode == GlobalFogMode.Linear)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    _VFXData.timeOfDayParams.linearFogStartDistance = THelpersUI.GUI_Slider(new GUIContent("START DISTANCE", "Start distance of linear fog scattering range"), _VFXData.timeOfDayParams.linearFogStartDistance, 0, 4000, -10);
                                    _VFXData.timeOfDayParams.linearFogEndDistance = THelpersUI.GUI_Slider(new GUIContent("END DISTANCE", "End distance of linear fog scattering range"), _VFXData.timeOfDayParams.linearFogEndDistance, 1, 20000, -10);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (_VFXData.timeOfDayParams.linearFogEndDistance <= _VFXData.timeOfDayParams.linearFogStartDistance)
                                            _VFXData.timeOfDayParams.linearFogEndDistance = _VFXData.timeOfDayParams.linearFogStartDistance + 1f;
                                    }
                                }

                                _VFXData.timeOfDayParams.fogLuminance = THelpersUI.GUI_Slider(new GUIContent("LUMINANCE", "Fog Luminance for light scattering simulation based on time of day"), _VFXData.timeOfDayParams.fogLuminance, 1f, 2.5f, -5);
                            }

                            _VFXData.timeOfDayParams.skyMaterial = (Material)THelpersUI.GUI_ObjectField(new GUIContent("SKY MATERIAL", "Material used for Sky rendering. Drag & drop a prefab to update!"), _VFXData.timeOfDayParams.skyMaterial, typeof(Material), null, 20);

                            if (_VFXData.timeOfDayParams.skyMaterial == null)
                                THelpersUI.GUI_Alert();

                            EditorGUI.BeginChangeCheck();
                            _VFXData.timeOfDayParams.starsPrefab = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("STARS PREFAB", "Prefab used for Stars scattering. Drag & drop a prefab to update!"), _VFXData.timeOfDayParams.starsPrefab, typeof(GameObject), null, -10);
                            if (EditorGUI.EndChangeCheck())
                            {
                                ParticleSystemRenderer PSR = _VFXData.timeOfDayParams.starsPrefab.GetComponent<ParticleSystemRenderer>();

                                if (PSR == null)
                                {
                                    _VFXData.timeOfDayParams.starsPrefab = null;
                                    ShowNotification(new GUIContent("Stars prefab should contain a Particle System! ", ""));
                                }
                            }

                            if (_VFXData.timeOfDayParams.starsPrefab == null)
                                THelpersUI.GUI_Alert();

                            THelpersUI.GUI_HelpBox("You can drop your own resources into the above field(s)!", MessageType.Info, -5);
                        }
                    }
                    if (EditorGUI.EndChangeCheck()) // Wraps around Time of Day settings
                    {
                    }
#endregion

#region God Rays
                    // Crepuscular Rays settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("GOD RAYS", "God Rays effect from sun"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.godRaysIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.crepuscularParams.hasGodRays);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.crepuscularParams.hasGodRays = !Convert.ToBoolean(state);

                        if (_VFXData.crepuscularParams.hasGodRays)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasGodRays", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasGodRays", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.crepuscularParams.hasGodRays);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings13))
                    {
                        _VFXData.crepuscularParams.godRaySamples = THelpersUI.GUI_Slider(new GUIContent("SAMPLES", "Samples count for post rendering"), _VFXData.crepuscularParams.godRaySamples, 8, 1024);
                        _VFXData.crepuscularParams.godRayDensity = THelpersUI.GUI_Slider(new GUIContent("DENSITY", "Samples count for post rendering"), _VFXData.crepuscularParams.godRayDensity, 0.01f, 1f, -10);
                        _VFXData.crepuscularParams.godRayWeight = THelpersUI.GUI_Slider(new GUIContent("WEIGHT", "Samples count for post rendering"), _VFXData.crepuscularParams.godRayWeight, 0.01f, 1f, -10);
                        _VFXData.crepuscularParams.godRayDecay = THelpersUI.GUI_Slider(new GUIContent("DECAY", "Samples count for post rendering"), _VFXData.crepuscularParams.godRayDecay, 0.01f, 1f, -10);
                        _VFXData.crepuscularParams.godRayExposure = THelpersUI.GUI_Slider(new GUIContent("EXPOSURE", "Samples count for post rendering"), _VFXData.crepuscularParams.godRayExposure, 0.5f, 1f, -10);

                        THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL (LOCKED)", "Material used for God Rays rendering"), _VFXData.crepuscularParams.material, typeof(Material), null, 10);
                        //_VFXData.crepuscularParams.material = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for God Rays rendering"), _VFXData.crepuscularParams.material, typeof(Material), null, 10);
                        //if (_VFXData.crepuscularParams.material == null) THelpersUI.GUI_Alert();
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region Clouds
                    // Clouds settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("CLOUDS", "Enable/disable clouds in scene"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.cloudsIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.cloudsManagerParams.hasClouds);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.cloudsManagerParams.hasClouds = !Convert.ToBoolean(state);

                        if (_VFXData.cloudsManagerParams.hasClouds)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasClouds", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasClouds", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.cloudsManagerParams.hasClouds);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings2))
                    {
                        _VFXData.cloudsManagerParams.altitude = THelpersUI.GUI_Slider(new GUIContent("ALTITUDE", "Clouds altitude in the 3D space"), _VFXData.cloudsManagerParams.altitude, -1000, 10000);
                        _VFXData.cloudsManagerParams.density = THelpersUI.GUI_DelayedFloatField(new GUIContent("DENSITY", "Density of the clouds in the area"), _VFXData.cloudsManagerParams.density, 0.01f, 10f, -10);
                        _VFXData.cloudsManagerParams.cloudSize = THelpersUI.GUI_DelayedFloatField(new GUIContent("SIZE", "Size of cloud instances"), _VFXData.cloudsManagerParams.cloudSize, 1f, 2000f, -10);
                        _VFXData.cloudsManagerParams.seed = THelpersUI.GUI_DelayedIntField(new GUIContent("SEED NO.", "Seed No. for the clouds randomization"), _VFXData.cloudsManagerParams.seed, 0, 99999, -10);

                        //EditorGUI.BeginChangeCheck();
                        //fXParamsUI.emitProbability = THelpersUI.GUI_DelayedIntField(new GUIContent("EMIT PROBABILITY (%)", "The percentage of snow/rain emit probability from the clouds"), fXParamsUI.emitProbability, 0, 100, -10);
                        //if (EditorGUI.EndChangeCheck()) SceneSettingsManager.UpdateClouds();

                        //_VFXData.cloudsManagerParams.emitProbability = 0;

                        state = Convert.ToInt32(!_VFXData.cloudsManagerParams.castShadows);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("SHADOWS", "Turn on/off clouds shadow casting!"), state, onOffSelection, style, -10);
                        if (EditorGUI.EndChangeCheck()) _VFXData.cloudsManagerParams.castShadows = !Convert.ToBoolean(state);

                        _VFXData.cloudsManagerParams.tintColor = THelpersUI.GUI_ColorField(new GUIContent("COLOR", "Clouds color"), _VFXData.cloudsManagerParams.tintColor, -10);

                        state = Convert.ToInt32(!_VFXData.cloudsManagerParams.isCustomShape);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("CUSTOM CLOUD SHAPE", "Change the shape of rendering clouds via custom meshes"), state, onOffSelection, style, 20);
                        if (EditorGUI.EndChangeCheck()) _VFXData.cloudsManagerParams.isCustomShape = !Convert.ToBoolean(state);

                        if (_VFXData.cloudsManagerParams.isCustomShape) // Custom Mesh slot
                        {
                            EditorGUI.BeginChangeCheck();
                            int cloudShapeIndex = -1;

                            if (_VFXData.cloudsManagerParams.cloudMeshUser != null)
                            {
                                for (int i = 0; i < TResourcesManager.cloudsMeshNames.Count; i++)
                                {
                                    if (TResourcesManager.cloudsMeshNames[i].Equals(_VFXData.cloudsManagerParams.cloudMeshUser.name))
                                    {
                                        cloudShapeIndex = i;
                                        break;
                                    }
                                }
                            }

                            if (cloudShapeIndex == -1)
                                cloudShapeIndex = TResourcesManager.cloudsMeshNames.Count - 1;

                            cloudShapeIndex = THelpersUI.GUI_Popup("SHAPES", cloudShapeIndex, TResourcesManager.cloudsMeshNames.ToArray(), 20);
                            if (EditorGUI.EndChangeCheck())
                                _VFXData.cloudsManagerParams.cloudMeshUser = TResourcesManager.GetMeshObject(TResourcesManager.cloudMeshCustomModelPath, TResourcesManager.cloudsMeshNames[cloudShapeIndex]);

                            if (cloudShapeIndex == TResourcesManager.cloudsMeshNames.Count - 1)
                                _VFXData.cloudsManagerParams.cloudMeshUser = (Mesh)THelpersUI.GUI_ObjectField(new GUIContent("CLOUD MESH", "Mesh used for cloud shapes"), _VFXData.cloudsManagerParams.cloudMeshUser, null, null, -10);

                            THelpersUI.GUI_HelpBox("SELECT \"CUSTOM\" IN SHAPES DROPDOWN TO DRAG & DROP YOUR OWN MESH INTO \"CLOUD MESH\" SLOT!", MessageType.Info);

                            _VFXData.cloudsManagerParams.particleSystemRenderSpace = (ParticleSystemRenderSpace)THelpersUI.GUI_EnumPopup(new GUIContent("RENDER ALIGNMENT", "Specifies if the particles will face the camera, align to world axes, or stay local to the system's transform."), _VFXData.cloudsManagerParams.particleSystemRenderSpace, 15);

                            state = Convert.ToInt32(!_VFXData.cloudsManagerParams.isFlatShadingCustomShape);
                            EditorGUI.BeginChangeCheck();
                            state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("SOLID RENDERING", "Switch between Solid and Transparent clouds rendering"), state, onOffSelection, style, -5);
                            if (EditorGUI.EndChangeCheck()) _VFXData.cloudsManagerParams.isFlatShadingCustomShape = !Convert.ToBoolean(state);

                            _VFXData.cloudsManagerParams.hasRotation = THelpersUI.GUI_Toggle(new GUIContent("RANDOM ROTATION", "Random Rotation of cloud particles"), _VFXData.cloudsManagerParams.hasRotation, -5);
                            _VFXData.cloudsManagerParams.hasFog = THelpersUI.GUI_Toggle(new GUIContent("ENABLE FOG", "Is fog enabled on cloud particles?"), _VFXData.cloudsManagerParams.hasFog, -5);

                            //EditorGUI.BeginChangeCheck();
                            //fXParamsUI.cloudNormalOffset = THelpersUI.GUI_DelayedFloatField(new GUIContent("MESH SCALE", "Normals Offset for the cloud rendering mesh"), fXParamsUI.cloudNormalOffset, 0.1f, 20f, -10);
                            //if (EditorGUI.EndChangeCheck()) SceneSettingsManager.UpdateClouds();

                            //EditorGUI.BeginChangeCheck();
                            //nodeTexture = (Texture2D)THelpersUI.GUI_ObjectField(new GUIContent("MESH TEXTURE", "Texture used for cloud mesh"), nodeTexture, typeof(Texture2D), null, -10);
                            //if (EditorGUI.EndChangeCheck())
                            //{
                            //    try
                            //    {
                            //        if (nodeTexture == null && !nodeTexture.isReadable)
                            //        {
                            //            TextureImporter imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(nodeTexture)) as TextureImporter;
                            //            imageImport.isReadable = true;
                            //            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(nodeTexture), ImportAssetOptions.ForceUpdate);
                            //            AssetDatabase.Refresh();
                            //            fXParamsUI.cloudMeshTexturePath = AssetDatabase.GetAssetPath(nodeTexture);
                            //        }
                            //
                            //        SceneSettingsManager.UpdateClouds();
                            //    }
                            //    catch {}
                            //}
                        }

                        _VFXData.cloudsManagerParams.cloudPrefab = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("PREFAB", "Prefab used for clouds scattering"), _VFXData.cloudsManagerParams.cloudPrefab, typeof(GameObject), null, 10);
                        if (_VFXData.cloudsManagerParams.cloudPrefab == null) THelpersUI.GUI_Alert();

                        THelpersUI.GUI_HelpBox("You can drop your own resources into the above field(s)!", MessageType.Info, -5);
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region Atmospheric Scattering
#if UNITY_STANDALONE_WIN // Atmospheric Scattering is only enabled on Windows Standalone

                    // Disabled Atmospheric Fog from TerraWorld scenes and replaced it with Global Fog effect in
                    // TimeOfDay script for more optimized and artist-friendly solution which works on all
                    // platforms in both Forward & Deferred rendering paths
                    // _VFXData.atmosphericScatteringParams.hasAtmosphericScattering = false;

                    /*
                    // Atmospheric Scattering settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("ATMOSPHERIC SCATTERING", "Is Atmospheric Scattering enabled?"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.atmosphericScatteringIcon, style);

                    if (_VFXData.atmosphericScatteringParams.hasAtmosphericScattering && QualitySettings.shadowCascades != 4)
                        QualitySettings.shadowCascades = 4;

                    if (TCameraManager.MainCamera.renderingPath != RenderingPath.DeferredShading)
                        THelpersUI.GUI_HelpBox("ATMOSPHERIC SCATTERING IS ONLY ENABLED IN DEFERRED RENDERING MODE!", MessageType.Warning, 20);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.atmosphericScatteringParams.hasAtmosphericScattering);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.atmosphericScatteringParams.hasAtmosphericScattering = !Convert.ToBoolean(state);
                        if (_VFXData.atmosphericScatteringParams.hasAtmosphericScattering)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasAtmosphericScattering", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasAtmosphericScattering", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.atmosphericScatteringParams.hasAtmosphericScattering);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings3))
                    {
                        _VFXData.atmosphericScatteringParams.worldRayleighColorIntensity = THelpersUI.GUI_Slider(new GUIContent("LIGHT INTENSITY", "Volumetric light intensity"), _VFXData.atmosphericScatteringParams.worldRayleighColorIntensity, 0f, 20f, -10);
                        _VFXData.atmosphericScatteringParams.heightRayleighIntensity = THelpersUI.GUI_Slider(new GUIContent("AERIAL FOG INTENSITY", "Atmospheric aerial fog intensity"), _VFXData.atmosphericScatteringParams.heightRayleighIntensity, 0f, 3f, -10);
                        _VFXData.atmosphericScatteringParams.heightRayleighDensity = THelpersUI.GUI_Slider(new GUIContent("AERIAL FOG DENSITY", "Atmospheric aerial fog density"), _VFXData.atmosphericScatteringParams.heightRayleighDensity, 0f, 0.0002f, -10);
                        _VFXData.atmosphericScatteringParams.heightDistance = THelpersUI.GUI_Slider(new GUIContent("AERIAL FOG DISTANCE", "Atmospheric aerial fog distance"), _VFXData.atmosphericScatteringParams.heightDistance, 1000f, 1500f, -10);
                    }
                    EditorGUI.EndDisabledGroup();
                    */
#endif
#endregion

#region Voluemtric Fog
#if UNITY_STANDALONE_WIN // Volumetric Fog is only enabled on Windows Standalone

                    // Volumetric Fog settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("VOLUMETRIC FOG", "Is Volumetric Fog enabled?"), 40, enabledColor, THelpersUI.UIColor);

                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.fogIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.volumetricFogParams.hasVolumetricFog);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.volumetricFogParams.hasVolumetricFog = !Convert.ToBoolean(state);

                        if (_VFXData.volumetricFogParams.hasVolumetricFog)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasVolumetricFog", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasVolumetricFog", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.volumetricFogParams.hasVolumetricFog);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings4))
                    {
                        _VFXData.volumetricFogParams.m_GlobalDensityMult = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Fog Strength in scene"), _VFXData.volumetricFogParams.m_GlobalDensityMult, 0f, 10f);

                        THelpersUI.GUI_MinMaxSlider(new GUIContent("RANGE (M)", "Minimum & Maximum distance of the volumetric fog effect"), ref _VFXData.volumetricFogParams.m_NearClip, ref _VFXData.volumetricFogParams.m_FarClipMax, 0.01f, 5000f, -10);
                        _VFXData.volumetricFogParams.m_ConstantFog = THelpersUI.GUI_Slider(new GUIContent("DENSITY", "Fog Density in scene"), _VFXData.volumetricFogParams.m_ConstantFog, 0.01f, 10f, -10);
                        _VFXData.volumetricFogParams.m_NoiseFogAmount = THelpersUI.GUI_Slider(new GUIContent("NOISE AMOUNT", "Fog Noise Influence"), _VFXData.volumetricFogParams.m_NoiseFogAmount, 0f, 1f, -10);
                        _VFXData.volumetricFogParams.m_NoiseFogScale = THelpersUI.GUI_Slider(new GUIContent("NOISE SCALE", "Fog Noise Scale"), _VFXData.volumetricFogParams.m_NoiseFogScale, 0.01f, 2f, -10);

                        state = Convert.ToInt32(_VFXData.volumetricFogParams.autoColor);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("FOG COLOR", "Switch between physically calculated fog color or user specified color"), state, manualAutoMode, style, 10);
                        if (EditorGUI.EndChangeCheck()) _VFXData.volumetricFogParams.autoColor = Convert.ToBoolean(state);

                        if (state == 0)
                            _VFXData.volumetricFogParams.userColor = THelpersUI.GUI_ColorField(new GUIContent("TINT", "Volumetric Fog Color to manually change sky color and scene mood"), _VFXData.volumetricFogParams.userColor, -5);
                    }
                    EditorGUI.EndDisabledGroup();
#endif
#endregion

#region Wind
                    // Wind settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("WIND SIMULATION", "Is Wind Simulation enabled?"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.windIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.windManagerParams.hasWind);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.windManagerParams.hasWind = !Convert.ToBoolean(state);
                        if (_VFXData.windManagerParams.hasWind) TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasWind", 1);
                        else TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasWind", 0);
                    }

                    //TODO: Implement Wind UI here
                    EditorGUI.BeginDisabledGroup(!_VFXData.windManagerParams.hasWind);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings5))
                    {
                        _VFXData.windManagerParams.windMain = THelpersUI.GUI_Slider(new GUIContent("SPEED", "Global Wind Speed"), _VFXData.windManagerParams.windMain, 0.01f, 5f);
                        _VFXData.windManagerParams.windDirection = THelpersUI.GUI_Slider(new GUIContent("DIRECTION", "Global Wind Direction which affects all layers in world"), _VFXData.windManagerParams.windDirection, 0f, 360f, -10);
                        _VFXData.windManagerParams.randomWind = THelpersUI.GUI_Toggle(new GUIContent("RANDOM SPEED", "Random Speed for wind for more natural wind simulation and vertex animations"), _VFXData.windManagerParams.randomWind, -10);
                        _VFXData.windManagerParams.randomWindTime = THelpersUI.GUI_Slider(new GUIContent("RANDOM TIME", "Time-frame in seconds between randomization steps in speed"), _VFXData.windManagerParams.randomWindTime, 0.1f, 3f, -10);

                        EditorGUI.BeginChangeCheck();
                        _VFXData.windManagerParams.showWindGizmo = THelpersUI.GUI_Toggle(new GUIContent("SHOW GIZMO", "Show/Hide Wind Gizmo"), _VFXData.windManagerParams.showWindGizmo, -10);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_VFXData.windManagerParams.showWindGizmo)
                                ExpandWindManagerInspector();
                        }

                        // Speed multiplier values for each feature
                        THelpersUI.GUI_HelpBox("Here you can override each fearure's wind speed with a multiplier value", MessageType.Info, 10);

                        // Clouds
                        _VFXData.cloudsManagerParams.windSpeedMultiplier = THelpersUI.GUI_Slider(new GUIContent("CLOUDS", "Clouds particles speed"), _VFXData.cloudsManagerParams.windSpeedMultiplier, 0.1f, 10f);

                        // Volumetric Fog
                        EditorGUI.BeginDisabledGroup(!_VFXData.volumetricFogParams.hasVolumetricFog);
                        _VFXData.volumetricFogParams.windSpeedMultiplier = THelpersUI.GUI_Slider(new GUIContent("VOLUMETRIC FOG", "Movement speed for volumetric fog particles"), _VFXData.volumetricFogParams.windSpeedMultiplier, 0.1f, 10f);
                        EditorGUI.EndDisabledGroup();

                        if (!_VFXData.volumetricFogParams.hasVolumetricFog)
                            THelpersUI.GUI_HelpBox("Volumetric Fog is disabled!", MessageType.Info, -7);

                        //TODO: Models (Vegetation)
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region Horizon Fog
                    // Horizon Fog settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("HORIZON FOG", "Switch Horizon Fog in scene"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.horizonIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.horizonFogParams.hasHorizonFog);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.horizonFogParams.hasHorizonFog = !Convert.ToBoolean(state);

                        if (_VFXData.horizonFogParams.hasHorizonFog)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasHorizonFog", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasHorizonFog", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.horizonFogParams.hasHorizonFog);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings6))
                    {
                        state = Convert.ToInt32(_VFXData.horizonFogParams.autoColor);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("COLOR", "Switch between physically calculated fog color or user specified color"), state, manualAutoMode, style, 10);
                        if (EditorGUI.EndChangeCheck()) _VFXData.horizonFogParams.autoColor = Convert.ToBoolean(state);

                        if (state == 0)
                        {
                            _VFXData.horizonFogParams.horizonFogVolumeColor = THelpersUI.GUI_ColorField(new GUIContent("FOG COLOR", "Atmospheric Fog Color to manually change sky color and scene mood"), _VFXData.horizonFogParams.horizonFogVolumeColor, -10);
                            _VFXData.horizonFogParams.visibilityUser = THelpersUI.GUI_Slider(new GUIContent("VISIBILITY", "Horizon Fog Visibility"), _VFXData.horizonFogParams.visibilityUser, 0f, 100000f, 10);
                            THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE (M)", "Minimum & Maximum starting height of horizon fog"), ref _VFXData.horizonFogParams.startOffsetUser, ref _VFXData.horizonFogParams.endOffsetUser, 0f, 100000f, -10);
                            _VFXData.horizonFogParams.strengthUser = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Horizon Fog Strength"), _VFXData.horizonFogParams.strengthUser, 0.25f, 5f);
                            _VFXData.horizonFogParams.horizonBlendMode = (HorizonFog.HorizonBlendMode)THelpersUI.GUI_EnumPopup(new GUIContent("SCENE BLEND MODE", "Blend Mode for the Horizon Fog in scene.\n\nAffects clouds and transparent or semi-transparent objects in the world"), _VFXData.horizonFogParams.horizonBlendMode);
                        }
                        else if (state == 1)
                        {
                            //fXParamsUI.horizonFogDensityAuto = THelpersUI.GUI_Slider(new GUIContent("DENSITY", "Horizon Fog Density"), fXParamsUI.horizonFogDensityAuto, 0f, TCameraManager.MainCamera.farClipPlane / 2);
                            _VFXData.horizonFogParams.visibility = THelpersUI.GUI_Slider(new GUIContent("VISIBILITY", "Horizon Fog Visibility"), _VFXData.horizonFogParams.visibility, 0f, 100000f, 10);
                            //THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE (M)", "Minimum & Maximum starting height of horizon fog"), ref fXParamsUI.horizonFogStartHeightAuto, ref fXParamsUI.horizonFogEndHeightAuto, 0f, TCameraManager.MainCamera.farClipPlane / 2, -10);
                            THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE (M)", "Minimum & Maximum starting height of horizon fog"), ref _VFXData.horizonFogParams.startOffset, ref _VFXData.horizonFogParams.endOffset, 0f, 100000f, -10);
                            _VFXData.horizonFogParams.strength = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Horizon Fog Strength"), _VFXData.horizonFogParams.strength, 0.25f, 5f);
                        }

                        THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL (LOCKED)", "Material used for Horizon Fog rendering"), _VFXData.horizonFogParams.material, typeof(Material), null, 10);
                        //_VFXData.horizonFogParams.material = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for Horizon Fog rendering"), _VFXData.horizonFogParams.material, typeof(Material), null, 10);
                        //if (_VFXData.horizonFogParams.material == null) THelpersUI.GUI_Alert();
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region Snow
                    // Snow settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SNOW", "Toggle snow rendering in scene"), 40, enabledColor, THelpersUI.UIColor);

                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.snowIcon, style);

                    style = new GUIStyle(EditorStyles.toolbarButton);
                    state = Convert.ToInt32(!_VFXData.snowManagerParams.hasSnow);
                    EditorGUI.BeginChangeCheck();
                    state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _VFXData.snowManagerParams.hasSnow = !Convert.ToBoolean(state);

                        if (_VFXData.snowManagerParams.hasSnow)
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasSnow", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "hasSnow", 0);
                    }

                    EditorGUI.BeginDisabledGroup(!_VFXData.snowManagerParams.hasSnow);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings7))
                    {
                        _VFXData.snowManagerParams.snowHeight = THelpersUI.GUI_Slider(new GUIContent("START HEIGHT", "Starting height of the snow on surface"), _VFXData.snowManagerParams.snowHeight, -1000, 10000, -10);
                        _VFXData.snowManagerParams.snowFalloff = THelpersUI.GUI_Slider(new GUIContent("HEIGHT FALLOFF", "Height Falloff for the starting point of the snow on surface"), _VFXData.snowManagerParams.snowFalloff, 0f, 100f, -10);
                        _VFXData.snowManagerParams.snowColor = THelpersUI.GUI_ColorField(new GUIContent("COLOR", "Snow color in scene"), _VFXData.snowManagerParams.snowColor, -5);
                        THelpersUI.GUI_HelpBox("YOU CAN ALSO CHANGE SNOW COLOR ON TERRAIN EXCLUSIVELY FROM TERRAIN TAB!", MessageType.Info, -5);

                        //fXParamsUI.snowThickness = THelpersUI.GUI_Slider(new GUIContent("THICKNESS", "Thickness of the snow on the surface"), fXParamsUI.snowThickness, 0f, 10f, 20);
                        //fXParamsUI.snowDamping = THelpersUI.GUI_Slider(new GUIContent("DAMPING", "Damping of the snow thickness throughout the surface"), fXParamsUI.snowDamping, 0f, 0.999f, -10);

                        //tempColor = new Color(fXParamsUI.warmColorR, fXParamsUI.warmColorG, fXParamsUI.warmColorB);
                        //tempColor = THelpersUI.GUI_ColorField(new GUIContent("WARM COLOR", "Layer color on terrain surface"), tempColor);
                        //module.warmColorR = tempColor.r;
                        //module.warmColorG = tempColor.g;
                        //module.warmColorB = tempColor.b;
                        //
                        //tempColor = new Color(fXParamsUI.coldColorR, fXParamsUI.coldColorG, fXParamsUI.coldColorB);
                        //tempColor = THelpersUI.GUI_ColorField(new GUIContent("COLD COLOR", "Layer color on terrain surface"), tempColor);
                        //module.coldColorR = tempColor.r;
                        //module.coldColorG = tempColor.g;
                        //module.coldColorB = tempColor.b;
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region Water Rendering
                    // Water settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("WATER", "Water rendering settings"), 40, enabledColor, THelpersUI.UIColor);

                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.waterIcon, style);

                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings8))
                    {
                        _VFXData.waterManagerParams.waterQuality = (WaterQuality)THelpersUI.GUI_EnumPopup(new GUIContent("QUALITY", "Quality of water rendering and features"), _VFXData.waterManagerParams.waterQuality);
                        _VFXData.waterManagerParams.waterBaseColor = THelpersUI.GUI_ColorField(new GUIContent("BASE COLOR", "Base water color"), _VFXData.waterManagerParams.waterBaseColor, -10);

                        if (_VFXData.waterManagerParams.waterQuality == WaterQuality.High || _VFXData.waterManagerParams.waterQuality == WaterQuality.Medium)
                        {
                            _VFXData.waterManagerParams.hasReflection = THelpersUI.GUI_Toggle(new GUIContent("PLANAR REFLECTION", "Switch Planar Reflection to simulate real-time reflections on surface"), _VFXData.waterManagerParams.hasReflection);

                            if (_VFXData.waterManagerParams.hasReflection)
                            {
                                THelpersUI.GUI_HelpBox("HAVING THIS FEATURE ON WILL GREATLY IMPACT PERFORMANCE!", MessageType.Warning, -10);
                                _VFXData.waterManagerParams.reflectionQuality = THelpersUI.GUI_DelayedFloatField(new GUIContent("REFLECTION QUALITY (0~1)", "Quality of the reflection on water surface"), _VFXData.waterManagerParams.reflectionQuality, 0.1f, 1f);
                                _VFXData.waterManagerParams.waterReflectionColor = THelpersUI.GUI_ColorField(new GUIContent("REFLECTION COLOR", "Reflection color and depth on water surface"), _VFXData.waterManagerParams.waterReflectionColor, -10);
                            }
                        }

                        if (_VFXData.waterManagerParams.waterQuality == WaterQuality.High)
                            _VFXData.waterManagerParams.edgeBlend = THelpersUI.GUI_Toggle(new GUIContent("EDGE BLEND", "Blending of water surface at the edges to simulate foam and transparency at the edges"), _VFXData.waterManagerParams.edgeBlend);

                        _VFXData.waterManagerParams.specularLighting = THelpersUI.GUI_Toggle(new GUIContent("SUN SPECULAR", "Simulate sun specular lighting on surface"), _VFXData.waterManagerParams.specularLighting, -10);

                        if (_VFXData.waterManagerParams.waterQuality == WaterQuality.High || _VFXData.waterManagerParams.waterQuality == WaterQuality.Medium)
                            _VFXData.waterManagerParams.gerstnerWaves = THelpersUI.GUI_Toggle(new GUIContent("GERSTNER WAVES", "Switch Gerstner Waves on water surface which displaces water mesh vertices to simulate waves"), _VFXData.waterManagerParams.gerstnerWaves, -10);

                        //EditorGUI.BeginChangeCheck();
                        //TTerraWorld.waterManager.waterMaterial = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for Water areas such as lakes, rivers & oceans rendering"), TTerraWorld.waterManager.waterMaterial, typeof(Material), null, 10);
                        //if (EditorGUI.EndChangeCheck()) SceneSettingsManager.UpdateWaterRendering(fXParamsUI);
                        //if (TTerraWorld.waterManager.waterMaterial == null) THelpersUI.GUI_Alert();
                    }
#endregion

#region Post Processing
#if UNITY_POST_PROCESSING_STACK_V2
                    // Post Processing settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("POST PROCESSING", "Post Processing settings"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.postProcessingIcon, style);

                    if (packageIsInstalled)
                    {
                        style = new GUIStyle(EditorStyles.toolbarButton);
                        EditorGUI.BeginChangeCheck();

                        int postProcessingState = Convert.ToInt32(!_VFXData.isPostProcessing);
                        postProcessingState = THelpersUI.GUI_SelectionGrid(postProcessingState, onOffSelection, style);
                        _VFXData.isPostProcessing = !Convert.ToBoolean(postProcessingState);

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_VFXData.isPostProcessing)
                                TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "isPostProcessing", 1);
                            else
                                TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "isPostProcessing", 0);
                        }

                        style.fixedWidth = 185; style.fixedHeight = 30;

                        if (_VFXData.isPostProcessing)
                        {
                            if (TCameraManager.MainCamera.renderingPath == RenderingPath.Forward)
                            {
                                if (AO == null)
                                    _VFXData.postProcessingParams.postProcessProfile.TryGetSettings(out AO);

                                if (AO != null && AO.mode == AmbientOcclusionMode.ScalableAmbientObscurance)
                                {
                                    THelpersUI.GUI_HelpBox("\"Scalable Ambient Obscurance\" mode in AO is not supported in Forward rendering path!\n\nEither change main camera's Rendering Path in scene or press below button to change AO mode for a supported type!", MessageType.Warning, 15);

                                    GUILayout.Space(10);
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button(new GUIContent("SOLVE AO IN FORWARD", "Change Ambient Occlusion mode to a supported type in Forward Rendering Path"), style))
                                    {
                                        AO.mode.value = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
                                    }
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            else if (TCameraManager.MainCamera.renderingPath == RenderingPath.DeferredShading)
                            {
                                if (AO == null)
                                    _VFXData.postProcessingParams.postProcessProfile.TryGetSettings(out AO);

                                if (AO != null && AO.mode == AmbientOcclusionMode.MultiScaleVolumetricObscurance)
                                {
                                    THelpersUI.GUI_HelpBox("\"Scalable Ambient Obscurance\" brings better visuals while in Deferred Rendering Path. Press below button to change AO mode if desired!", MessageType.Info, 15);

                                    GUILayout.Space(10);
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button(new GUIContent("CHANGE AO MODE (OPTIONAL)", "Change Ambient Occlusion mode for better overall visuals!"), style))
                                    {
                                        AO.mode.value = AmbientOcclusionMode.ScalableAmbientObscurance;
                                    }
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }

                            GUILayout.Space(10);
                        }

                        EditorGUI.BeginDisabledGroup(!_VFXData.isPostProcessing);
                        if (THelpersUI.SectionSettings(ref module.uIToggles.Settings9))
                        {
                            if (_VFXData.Enabled)
                            {
                                if (_VFXData.isPostProcessing) GUI.color = enabledColor;
                                else GUI.color = disabledColor;
                            }
                            THelpersUI.GUI_Button(new GUIContent("EDIT POST PROCESSING", "Edit Post Processing settings"), style, EditPostProcessingAsset);

                            //THelpersUI.GUI_HelpBox("POST PROCESSING IS INSTALLED", MessageType.None, 20, Color.green);
                            //
                            //style.fixedWidth = 180; style.fixedHeight = 25;
                            //THelpersUI.GUI_Button(new GUIContent("UNINSTALL POST PROCESSING", "Uninstall Post Processing package in Unity"), style, RemovePostProcessingPackage, 0, 0, new Color(0.5f, 0.5f, 0.5f, 0.65f));

                            //THelpersUI.GUI_ObjectField(new GUIContent("PROFILE", "Post Process Profile used for visual effects"), PostProcessingManager.SharedProfile, typeof(PostProcessProfile), null, 20);
                            //PostProcessingManager.SharedProfile = (PostProcessProfile)THelpersUI.GUI_ObjectField(new GUIContent("PROFILE", "Post Process Profile used for visual effects"), PostProcessingManager.SharedProfile, typeof(PostProcessProfile), null, 20);
                            //if (PostProcessingManager.SharedProfile == null) THelpersUI.GUI_Alert();

                            EditorGUI.BeginChangeCheck();
                            //_VFXData.postProcessingParams.antiAliasingType = (PostProcessLayer.Antialiasing)THelpersUI.GUI_EnumPopup(new GUIContent("ANTI ALIASING MODE", ""), _VFXData.postProcessingParams.antiAliasingType, 5);
                            PostProcessLayer.Antialiasing antialiasing = _VFXData.postProcessingParams.antiAliasingType;
                            antialiasing = (PostProcessLayer.Antialiasing)THelpersUI.GUI_EnumPopup(new GUIContent("ANTI ALIASING MODE", ""), antialiasing, 20);
                            if (EditorGUI.EndChangeCheck()) _VFXData.postProcessingParams.antiAliasingType = antialiasing;

                            if (PostProcessingManager.PostProcessLayer.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing)
                                THelpersUI.GUI_HelpBox("USE FAST MODE IF PERFORMANCE IS CONCERNED SUITED FOR MOBILE/VR/AR", MessageType.Info, -5);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        THelpersUI.GUI_HelpBox("POST PROCESSING IS NOT INSTALLED", MessageType.None, 60, Color.red);
                        THelpersUI.GUI_HelpBox("Install Post Processing Stack from Unity's Package Manager window to unlock this feature", MessageType.Warning, -10);

                        //style.fixedWidth = 180; style.fixedHeight = 30;
                        //THelpersUI.GUI_Button(new GUIContent("INSTALL POST PROCESSING", "Install Post Processing package in Unity"), style, AddPostProcessingPackage);
                    }
#endif
#endregion

#region Flat Shading
                    // Flat Shading (Low-Poly Style) settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("FLAT/SMOOTH SHADING", "Toggle Flat or Smooth Shading and rendering style in scene.\n\nFlat Shading will simulate low-poly style while Smooth Shading is the common smooth rendering of surfaces"), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.flatShadingIcon, style);

                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings10))
                    {
                        style = new GUIStyle(EditorStyles.toolbarButton);
                        TerrainRenderingParams terrainRenderingParams = TerrainRenderingManager.GetParams();

                        EditorGUI.BeginDisabledGroup(!terrainRenderingParams.modernRendering);
                        {
                            state = Convert.ToInt32(!_VFXData.flatShadingParams.isFlatShadingTerrain);
                            THelpersUI.GUI_HelpBox(new GUIContent("TERRAIN FLATSHADING RENDERING", "Switch between Stylish and Smooth terrain rendering"), true);
                            EditorGUI.BeginChangeCheck();
                            state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                            if (EditorGUI.EndChangeCheck()) _VFXData.flatShadingParams.isFlatShadingTerrain = !Convert.ToBoolean(state);

                            // TODO: Implement later
                            //THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope that Flat Shading takes effect on surface"), ref fXParamsUI.minSlopeFlatShading, ref fXParamsUI.maxSlopeFlatShading, 0f, 90f);
                            //SceneSettingsManager.MinSlopeFlatShading = 0f;
                            //SceneSettingsManager.MaxSlopeFlatShading = 90f;
                        }
                        EditorGUI.EndDisabledGroup();

                        state = Convert.ToInt32(!_VFXData.flatShadingParams.isFlatShadingObjects);
                        THelpersUI.GUI_HelpBox(new GUIContent("MODELS FLATSHADING RENDERING", "Switch between Stylish and Smooth gameobject rendering"), true, 30);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                        if (EditorGUI.EndChangeCheck()) _VFXData.flatShadingParams.isFlatShadingObjects = !Convert.ToBoolean(state);

                        //if (_VFXData.flatShadingParams.isFlatShadingObjects)
                        //{
                        //    fXParamsUI.flatShadingStrengthObject = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Strength of the Flat Shading effect on object surface"), fXParamsUI.flatShadingStrengthObject, 0f, 1f, -10);
                        //    SceneSettingsManager.FlatShadingStrengthObject = 0;
                        //}

                        state = Convert.ToInt32(!_VFXData.flatShadingParams.isFlatShadingClouds);
                        THelpersUI.GUI_HelpBox(new GUIContent("CLOUDS FLATSHADING RENDERING", "Switch between Cutout and Smooth clouds rendering"), true, 30);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style);
                        if (EditorGUI.EndChangeCheck()) _VFXData.flatShadingParams.isFlatShadingClouds = !Convert.ToBoolean(state);
                    }
#endregion

#region Colormap Blending
                    // Colormap Blending settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SATELLITE IMAGE BLENDING", "Blends placed models' material color with satellite image pixels underneath for more natural and organic distribution of environment colors and high performance variations for free.\n\nThis is mostly suited for organic models like vegetation and transparent objects."), 40, enabledColor, THelpersUI.UIColor);
                    style = new GUIStyle(); style.fixedWidth = 55; style.fixedHeight = 55;
                    THelpersUI.GUI_Label(TResourcesManager.colormapBlendingIcon, style);

                    if (ColormapBlendingManager.Colormap == null)
                        THelpersUI.GUI_HelpBox("NO SATELLITE IMAGES (COLORMAPS) DETECTED ON TERRAIN!", MessageType.Error);

                    EditorGUI.BeginDisabledGroup(!ColormapBlendingManager.Colormap);
                    if (THelpersUI.SectionSettings(ref module.uIToggles.Settings11))
                    {
                        THelpersUI.GUI_HelpBox("\nBlends placed models' color with satellite image pixels underneath for more natural and organic distribution of environment colors and high performance variations for free.\n\nThis is mostly suited for organic models like vegetation and transparent objects.\n", MessageType.Info);

                        style = new GUIStyle(EditorStyles.toolbarButton);

                        state = Convert.ToInt32(!_VFXData.colormapBlendingParams.hasColormapBlendingGPU);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("TREES, ROCKS, BUILDINGS...", "Activate Satellite Image Blending for GPU Instanced models"), state, onOffSelection, style, 30);
                        if (EditorGUI.EndChangeCheck()) _VFXData.colormapBlendingParams.hasColormapBlendingGPU = !Convert.ToBoolean(state);

                        EditorGUI.BeginDisabledGroup(!_VFXData.colormapBlendingParams.hasColormapBlendingGPU);
                        _VFXData.colormapBlendingParams.blendingStrengthGPU = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Blending Strength starting from the root of model"), _VFXData.colormapBlendingParams.blendingStrengthGPU, 1f, 100f, 5);
                        _VFXData.colormapBlendingParams.excludeOpaqueMaterialsGPU = THelpersUI.GUI_Toggle(new GUIContent("EXCLUDE OPAQUE MATERIALS", "Exclude opaque materials (tree bark, rocks, boulders...) from Satellite Image Blending\n\nCurrent implementation is based on model UVs and not suited for custom UV-ed materials"), _VFXData.colormapBlendingParams.excludeOpaqueMaterialsGPU, -10);
                        EditorGUI.EndDisabledGroup();

                        state = Convert.ToInt32(!_VFXData.colormapBlendingParams.hasColormapBlendingGrass);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("GRASS, BUSH, FLOWER...", "Activate Satellite Image Blending for Grass models"), state, onOffSelection, style, 50);
                        if (EditorGUI.EndChangeCheck()) _VFXData.colormapBlendingParams.hasColormapBlendingGrass = !Convert.ToBoolean(state);

                        EditorGUI.BeginDisabledGroup(!_VFXData.colormapBlendingParams.hasColormapBlendingGrass);
                        _VFXData.colormapBlendingParams.blendingStrengthGrass = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Blending Strength starting from the root of model"), _VFXData.colormapBlendingParams.blendingStrengthGrass, 1f, 100f, 5);
                        _VFXData.colormapBlendingParams.excludeOpaqueMaterialsGrass = THelpersUI.GUI_Toggle(new GUIContent("EXCLUDE OPAQUE MATERIALS", "Exclude opaque materials from Satellite Image Blending\n\nCurrent implementation is based on model UVs and not suited for custom UV-ed materials"), _VFXData.colormapBlendingParams.excludeOpaqueMaterialsGrass, -10);
                        EditorGUI.EndDisabledGroup();

                        state = Convert.ToInt32(!_VFXData.colormapBlendingParams.hasColormapBlendingRuntimeSpawners);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("RUNTIME SPAWNERS...", "Activate Satellite Image Blending for Runtime Spawners"), state, onOffSelection, style, 30);
                        if (EditorGUI.EndChangeCheck()) _VFXData.colormapBlendingParams.hasColormapBlendingRuntimeSpawners = !Convert.ToBoolean(state);

                        EditorGUI.BeginDisabledGroup(!_VFXData.colormapBlendingParams.hasColormapBlendingRuntimeSpawners);
                        _VFXData.colormapBlendingParams.blendingStrengthRuntimeSpawner = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Blending Strength starting from the root of model"), _VFXData.colormapBlendingParams.blendingStrengthRuntimeSpawner, 1f, 100f, 5);
                        _VFXData.colormapBlendingParams.excludeOpaqueMaterialsRuntimeSpawner = THelpersUI.GUI_Toggle(new GUIContent("EXCLUDE OPAQUE MATERIALS", "Exclude opaque materials from Satellite Image Blending\n\nCurrent implementation is based on model UVs and not suited for custom UV-ed materials"), _VFXData.colormapBlendingParams.excludeOpaqueMaterialsRuntimeSpawner, -10);
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.EndDisabledGroup();
#endregion

#region VFX Advanced Settings
                    // VFX Edit settings
                    THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("ADVANCED SETTINGS", "Reset to default settings or edit all runtime VFX parameters"), 40, enabledColor, THelpersUI.UIColor);

                    if (_VFXData.Enabled) GUI.color = enabledColor;
                    else GUI.color = disabledColor;

                    GUILayout.Space(30);

                    style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 45; style.fixedHeight = 45;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(TResourcesManager.resetIcon, "Reset to Default settings"), style))
                    {
#if UNITY_EDITOR
                        if (EditorUtility.DisplayDialog("TERRAWORLD", "RESET SETTINGS : Are you sure you want to reset settings?", "No", "Yes"))
                            return;
#endif

                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Reset", 0);
                        SceneSettingsManager.InstantiateNewSceneSettings();
                    }

                    GUILayout.Space(7);

                    if (GUILayout.Button(new GUIContent(TResourcesManager.editIcon, "Edit Advanced Settings"), style))
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Advanced", 0);
                        EditFXSettings();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    //THelpersUI.GUI_ObjectField(new GUIContent("SCENE SETTINGS PREFAB", "Prefab used for all Scene Settings components"), TTerraWorldManager.SceneSettingsGO1, typeof(GameObject), null, 20);
                    //TTerraWorldManager.TerraWorldManagerScript.sceneSettings = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("SCENE SETTINGS PREFAB", "Prefab used for all Scene Settings components"), TTerraWorldManager.TerraWorldManagerScript.sceneSettings, typeof(GameObject), null, 20);
                    //if (TTerraWorldManager.TerraWorldManagerScript.sceneSettings == null) THelpersUI.GUI_Alert();
#endregion
                }

                if (_VFXData.Enabled) GUI.color = enabledColor;

                if (EditorGUI.EndChangeCheck())  // Wraps around all VFX tab
                {
                    //SceneSettingsManager.SetFXParams(fXParamsUI);
                    SceneSettingsManager.SetVFXData(_VFXData);
                }
#endregion

                GUILayout.Space(60);
                GUI.color = enabledColor;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
#endif
        }

        private void EditPostProcessingAsset()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (PostProcessingManager.PostProcessVolumeScript.profile != null)
            {
                EditorGUIUtility.PingObject(PostProcessingManager.PostProcessVolumeScript.profile);
                Selection.activeObject = PostProcessingManager.PostProcessVolumeScript.profile;
                Type windowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
                EditorWindow window = GetWindow(windowType);
                window.Focus();
            }
            else
            {
                EditorUtility.DisplayDialog("NOT FOUND", "Post Processing asset file not found in the project!", "Ok");
                return;
            }
#endif
        }

        //private static void SearchInstalledPackage(string packageIdOrName)
        //{
        //    searchRequest = Client.Search(packageIdOrName);
        //    EditorApplication.update += SearchClientProgress;
        //}
        //
        //private static void SearchClientProgress()
        //{
        //    if (searchRequest.IsCompleted)
        //    {
        //        if (searchRequest.Status == StatusCode.Success)
        //        {
        //            List<UnityEditor.PackageManager.PackageInfo> PIs = searchRequest.Result.ToList();
        //
        //            foreach(UnityEditor.PackageManager.PackageInfo p in PIs)
        //                Debug.Log(p.version);
        //        }   
        //        else if (searchRequest.Status >= StatusCode.Failure)
        //            Debug.Log(searchRequest.Error.message);
        //
        //        EditorApplication.update -= SearchClientProgress;
        //    }
        //}

        private static void ExpandWindManagerInspector ()
        {
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(FindObjectOfType<WindManager>(), true);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void PostProcessingList(string packageIdOrName)
        {
            searchPackageIdOrName = packageIdOrName;
            listRequest = Client.List();
            EditorApplication.update += PostProcessingListProgress;
        }

        private static void PostProcessingListProgress()
        {
            packageIsInstalled = false;

            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                        if (package.name.Equals(searchPackageIdOrName))
                            packageIsInstalled = true;
                }
                else if (listRequest.Status >= StatusCode.Failure)
                    TDebug.LogErrorToUnityUI(new Exception (listRequest.Error.message));

                EditorApplication.update -= PostProcessingListProgress;
            }
        }

        //private static void AddPostProcessingPackage ()
        //{
        //    addRequest = Client.Add("com.unity.postprocessing");
        //    EditorApplication.update += AddClientProgress;
        //}
        //
        //private static void AddClientProgress()
        //{
        //    if (addRequest.IsCompleted)
        //    {
        //        if (addRequest.Status == StatusCode.Success)
        //            Debug.Log("Installed: " + addRequest.Result.packageId);
        //        else if (addRequest.Status >= StatusCode.Failure)
        //            Debug.Log(addRequest.Error.message);
        //
        //        EditorApplication.update -= AddClientProgress;
        //    }
        //}
        //
        //private static void RemovePostProcessingPackage()
        //{
        //    removeRequest = Client.Remove("com.unity.postprocessing");
        //    EditorApplication.update += RemoveClientProgress;
        //}
        //
        //private static void RemoveClientProgress()
        //{
        //    if (removeRequest.IsCompleted)
        //    {
        //        if (removeRequest.Status == StatusCode.Success)
        //            Debug.Log("Uninstalled: " + removeRequest.PackageIdOrName);
        //        else if (removeRequest.Status >= StatusCode.Failure)
        //            Debug.Log(removeRequest.Error.message);
        //
        //        EditorApplication.update -= RemoveClientProgress;
        //    }
        //}


        // Global Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------


        private void GlobalSettings()
        {
            if (lastTab != activeTab)
            {
            }

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);

            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("CACHE", "Cache settings"), 10, enabledColor, THelpersUI.UIColor);
            THelpersUI.GUI_HelpBox(new GUIContent("CACHE DATA FROM INTERNET", "Caching retrieved geo-data tiles"), true);

            EditorGUI.BeginChangeCheck();
            int state = TProjectSettings.CacheData ? 0 : 1;
            state = THelpersUI.GUI_SelectionGrid(state, onOffSelection, style, -10);
            TProjectSettings.CacheData = state == 0 ? true : false;
            if (EditorGUI.EndChangeCheck())
            {
                if (state == 0)
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "selectionIndexCacheData", 1);
                else
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "selectionIndexCacheData", 0);

                if (state == 0) TProjectSettings.CacheData = true;
                else TProjectSettings.CacheData = false;
            }

            THelpersUI.GUI_Button(new GUIContent("CLEAR CACHE", "Clear all cache data from the system"), style, ClearCache, 0, 0, new Color(0.8f, 0.4f, 0.4f, 1f));

            style.fixedHeight = 35;
            THelpersUI.GUI_Button(new GUIContent("TEMPLATES CACHE FOLDER", "Templates package cache location"), style, ShowTemplatesCacheLocation, 20);
            THelpersUI.GUI_Button(new GUIContent("DEMO SCENES CACHE FOLDER", "Demo scenes package cache location"), style, ShowDemoSceneCacheLocation);

            //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("PREVIEW QUALITY", "Resolution for preview requests in scene and UI"), 10, enabledColor, THelpersUI.UIColor);
            //TProjectSettings.PreviewResolution = THelpersUI.GUI_IntSlider(new GUIContent("RESOLUTION", "Resolution for preview requests in scene and UI"), Mathf.ClosestPowerOfTwo(TProjectSettings.PreviewResolution), 64, 256);

            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("CAMERA CONTROLS", "Turn on/off camera controls"), 10, enabledColor, THelpersUI.UIColor);

            EditorGUI.BeginChangeCheck();
            bool editorCamSync, cameraFlyMode, setTargetFramerate;
            TCameraManager.GetCameraControls(out editorCamSync, out cameraFlyMode, out setTargetFramerate, out targetFrameRate);
            editorCamSync = THelpersUI.GUI_Toggle(new GUIContent("EDITOR/GAME CAMERA SYNC", "Syncs Gameplay and Editor camera transform settings"), editorCamSync, 10);
            cameraFlyMode = THelpersUI.GUI_Toggle(new GUIContent("RUNTIME CAMERA FLY MODE", "Enable/Disable camera free fly while in Play mode"), cameraFlyMode, -10);
            setTargetFramerate = THelpersUI.GUI_Toggle(new GUIContent("SET TARGET FRAMERATE", "Having a constant value for FPS is recommended for the best performance"), setTargetFramerate, -10);
            if (setTargetFramerate) targetFrameRate = THelpersUI.GUI_IntSlider(new GUIContent("FRAMERATE", "Target FPS value when game starts"), targetFrameRate, 30, 120, -10);
            if (EditorGUI.EndChangeCheck()) TCameraManager.SetCameraControls(editorCamSync, cameraFlyMode, setTargetFramerate, targetFrameRate);
            

            //TODO: Mobile/VR/AR related settings
            //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("PROJECT SETTINGS", "Set Project Settings based on target platform to cover mobile/VR to consoles"), 10, enabledColor, THelpersUI.UIColor);
            //EditorGUI.BeginChangeCheck();
            //selectionIndexMobileMode = module.mobileMode ? 0 : 1;
            //selectionIndexMobileMode = THelpersUI.GUI_SelectionGridToolbar(new GUIContent("MOBILE / VR MODE", "Turn on this option to set project settings for Mobile/VR platforms"), selectionIndexMobileMode, onOffSelection, style, 20);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    if (EditorUtility.DisplayDialog("PROJECT SETTINGS CHANGE", "This option will overwrite your existing Project Settings and may take up to a few minutes.\n\nAre you sure you want to continue?", "No", "Yes"))
            //        return;
            //
            //    module.mobileMode = selectionIndexMobileMode == 0 ? true : false;
            //        
            //    if (module.mobileMode)
            //    {
            //        module.forwardVSDeferred = true;
            //        module.LDRVSHDR = true;
            //        module.gammaVSLinear = true;
            //        module.LQVSHQ = true;
            //        module.noneVSAtmosphericScattering = true;
            //    }
            //
            //    GlobalSettingsManager.SwitchMobileMode(module);
            //}
            //
            //EditorGUI.BeginChangeCheck();
            //if (THelpersUI.SectionSettings(ref module.uIToggles.Settings2))
            //{
            //    EditorGUI.BeginDisabledGroup(selectionIndexMobileMode == 0 ? true : false);
            //    module.forwardVSDeferred = THelpersUI.GUI_Toggle(new GUIContent("FORWARD RENDERING", "Set Forward Rendering for camera"), module.forwardVSDeferred);
            //    module.LDRVSHDR = THelpersUI.GUI_Toggle(new GUIContent("LDR", "Disable HDR on camera and set Low Dynamic Range rendering"), module.LDRVSHDR);
            //    module.gammaVSLinear = THelpersUI.GUI_Toggle(new GUIContent("GAMMA COLOR SPACE", "Use Gamma color space"), module.gammaVSLinear);
            //    module.LQVSHQ = THelpersUI.GUI_Toggle(new GUIContent("LOW QUALITY LEVEL", "Set Lowest option in project's Quality settings"), module.LQVSHQ);
            //    //if (Camera.main.actualRenderingPath == RenderingPath.DeferredShading) module.noneVSAtmosphericScattering = THelpersUI.GUI_Toggle(new GUIContent("DISABLE ATMOSPHERIC SCATTERING", "Disable Atmospheric Scattering post effect"), module.noneVSAtmosphericScattering);
            //    //module.noneVSPackageName = THelpersUI.GUI_Toggle(new GUIContent("AUTO BUNDLE NAME", "Automatically set Player settings's bundle name for builds"), module.noneVSPackageName);
            //    EditorGUI.EndDisabledGroup();
            //}
            //if (EditorGUI.EndChangeCheck())
            //{
            //    GlobalSettingsManager.SwitchMobileMode(module);
            //
            //    //if (!module.forwardVSDeferred || !module.LDRVSHDR || !module.gammaVSLinear || !module.LQVSHQ || !module.noneVSAtmosphericScattering)
            //    //{
            //    //    module.mobileMode = false;
            //    //    selectionIndexMobileMode = 1;
            //    //}
            //    //else
            //    //{
            //    //    module.mobileMode = true;
            //    //    selectionIndexMobileMode = 0;
            //    //}
            //}
            //
            //bool state = Camera.main.actualRenderingPath.Equals(RenderingPath.Forward);
            //if (state) module.forwardVSDeferred = true;
            //else module.forwardVSDeferred = false;
            //
            //state = !Camera.main.allowHDR;
            //if (state) module.LDRVSHDR = true;
            //else module.LDRVSHDR = false;
            //
            //state = PlayerSettings.colorSpace.Equals(ColorSpace.Gamma);
            //if (state) module.gammaVSLinear = true;
            //else module.gammaVSLinear = false;
            //
            //state = QualitySettings.GetQualityLevel().Equals(0);
            //if (state) module.LQVSHQ = true;
            //else module.LQVSHQ = false;

            style = new GUIStyle(EditorStyles.toolbarButton);
            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("SCENE VIEW UI", "Turn on/off Scene View UI display"), 10, enabledColor, THelpersUI.UIColor);
            TProjectSettings.SceneViewGUI = THelpersUI.GUI_SelectionGrid(TProjectSettings.SceneViewGUI, onOffSelection, style);
            if (TProjectSettings.SceneViewGUI != showSceneViewGUI)
            {
                showSceneViewGUI = TProjectSettings.SceneViewGUI;
                TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "showSceneViewGUI", showSceneViewGUI);
                THelpersUI.SwitchSceneGUI(showSceneViewGUI);
            }

            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("Developer Options", ""), 10, enabledColor, THelpersUI.UIColor);

            //int debugSwitch = TProjectSettings.DebugLogSystem ? 0 : 1;
            //debugSwitch = THelpersUI.GUI_SelectionGrid(debugSwitch, onOffSelection, style);
            //TProjectSettings.DebugLogSystem = debugSwitch == 0 ? true : false;

            //TProjectSettings.NewGraphSystem = THelpersUI.GUI_Toggle(new GUIContent("New Graph System (Experimental)", ""), TProjectSettings.NewGraphSystem);
            TProjectSettings.FastConnection = THelpersUI.GUI_Toggle(new GUIContent("Fast Downlod System (Experimental)", ""), TProjectSettings.FastConnection, 0); // -10 extraSpace after uncommenting the above line!
            
            TProjectSettings.MaxAsyncConnection = THelpersUI.GUI_IntSlider(new GUIContent("Async Connections", "More connections means more download speed but you may face some connection errors!"), TProjectSettings.MaxAsyncConnection, 1, 50, -10);
            TProjectSettings.DebugMode = THelpersUI.GUI_Toggle(new GUIContent("Debug Mode", "Debug details will be shown in the console."), TProjectSettings.DebugMode, -10);
            TTerraWorld.FeedbackSystem = THelpersUI.GUI_Toggle(new GUIContent("Enable Feedback System", "Use \"Google Analytics\" system to feedback user actions for better software improvements. No private data will be collected."), TTerraWorld.FeedbackSystem, -10);
            THelpersUI.GUI_Button(new GUIContent("SHOW LOG", "Open debug log file"), style, ShowDebugLog, 20);

            //TDebug.ErrorLog = TProjectSettings.ErrorLog;

            //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("Feedback System", "Use Google Analytics system to feedback user actions for UX improvements. Non-private data will be collected."), 10, enabledColor, THelpersUI.UIColor);
            //int feedbackSwitch = TProjectSettings.FeedbackSystem ? 0 : 1;
            //feedbackSwitch = THelpersUI.GUI_SelectionGrid(feedbackSwitch, onOffSelection, style);
            //TProjectSettings.FeedbackSystem = feedbackSwitch == 0 ? true : false;
            //TTerraWorld.FeedbackSystem = TProjectSettings.FeedbackSystem;

            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("Clean-Up Options", ""), 10, enabledColor, THelpersUI.UIColor);

            TProjectSettings.CleanUpUnderTerrainObjects = THelpersUI.GUI_Toggle(new GUIContent("Remove Objects Under Terrain", "Objects that placed under the generated terrain will be removed!"), TProjectSettings.CleanUpUnderTerrainObjects);
            TProjectSettings.CreateNewWorkDirectory = THelpersUI.GUI_Toggle(new GUIContent("New \"World Resources\" Directory On Creation", "New directory will be created inside the \"World Resources\" directory when you push \"Generate\" buttom. "), TProjectSettings.CreateNewWorkDirectory, -10);

            if (!TProjectSettings.CreateNewWorkDirectory)
                THelpersUI.GUI_HelpBox("HAVING THIS OPTION DISABLED, YOU WILL LOSE MULTI SCENE WORLDS AND TERRAWORLD WILL SUPPORT ONLY ONE SCENE FOR EACH PROJECT WHICH IS NOT RECOMMENDED!", MessageType.Error);

            THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("ABOUT", ""), 10, enabledColor, THelpersUI.UIColor);
            THelpersUI.GUI_HelpBoxInfo(aboutText);

            //THelpersUI.GUI_HelpBox("TERRAWORLD", MessageType.None, 0);
            //THelpersUI.GUI_HelpBox("Version "+ TTerraWorld.MajorVersion.ToString() +"." + TTerraWorld.MinorVersion.ToString(), MessageType.None, 0);
            //THelpersUI.GUI_HelpBox("www.terraunity.com", MessageType.None, 0);
            //THelpersUI.GUI_HelpBox("(c)2019 Terra Unity Corporation", MessageType.None, 0);
            //THelpersUI.GUI_HelpBox("All Rights Reserved.", MessageType.None, 0);

            //THelpersUI.GUI_HelpBoxTitleWide(new GUIContent("WORLD ITEMS", "Toggle on/off placed items in world"), 10, enabledColor, THelpersUI.UIColor);
            //
            //THelpersUI.GUI_HelpBox("TERRAIN TREES", MessageType.None, 20);
            //EditorGUI.BeginChangeCheck();
            //selectionIndexTerrainTrees = THelpersUI.GUI_SelectionGrid(selectionIndexTerrainTrees, onOffSelection, style, -10);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    if (TTerrainGenerator.worldReference != null && TTerrainGenerator.worldReference.GetComponent<Terrain>() != null)
            //        TTerrainGenerator.worldReference.GetComponent<Terrain>().drawTreesAndFoliage = selectionIndexTerrainTrees == 0 ? true : false;
            //}
            //
            //THelpersUI.GUI_HelpBox("PLACED ITEMS", MessageType.None, 20);
            //EditorGUI.BeginChangeCheck();
            //selectionIndexPlacedItems = THelpersUI.GUI_SelectionGrid(selectionIndexPlacedItems, onOffSelection, style, -10);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    if (TTerrainGenerator.worldReference != null)
            //    {
            //        bool isEnabled = selectionIndexPlacedItems == 0 ? true : false;
            //
            //        foreach (Transform t in TTerrainGenerator.worldReference.GetComponentInChildren<Transform>())
            //        {
            //            if (t.name != TTerrainGenerator.worldReference.name && t.name != "Background Terrain")
            //            {
            //                if(isEnabled)
            //                    t.gameObject.SetActive(true);
            //                else
            //                    t.gameObject.SetActive(false);
            //            }
            //        }
            //    }
            //}

            GUILayout.Space(60);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ShowTemplatesCacheLocation()
        {
            EditorUtility.RevealInFinder(TAddresses.templatesPath_Pro_Cache);
        }

        private void ShowDebugLog()
        {
            TDebug.OpenDebugFile();
        }

        private void ShowDemoSceneCacheLocation()
        {
            EditorUtility.RevealInFinder(TAddresses.scenesPath_Pro_Cache);
        }

        // Runtime PLAYER Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void PlayerSettings(ref PlayerNode module)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionNode = EditorGUILayout.BeginScrollView(scrollPositionNode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (lastTab != activeTab)
            {
                if (InteractiveTargets.playerTargets == null) InteractiveTargets.GetPlayerTargets();
            }

            DisplayPlayerTargets();

            GUILayout.Space(60);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        //private void GetPlayerTargets()
        //{
        //    playerTargets = new List<GameObject>();
        //
        //    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        //    {
        //        if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
        //        {
        //            if (go.activeSelf && go.GetComponent<PlayerInteractions>() != null)
        //            {
        //                if (go.GetComponent<PlayerInteractions>().GPULayers == null || go.GetComponent<PlayerInteractions>().statesGPU == null)
        //                    go.GetComponent<PlayerInteractions>().ScanScatters();
        //
        //                playerTargets.Add(go);
        //            }
        //        }
        //    }
        //}

        private void DisplayPlayerTargets()
        {
            GUILayout.Space(30);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.clear;
            GUI.backgroundColor = Color.clear;
            GUI.contentColor = Color.clear;

            GUILayoutOption[] layoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(175),
                GUILayout.Height(75)
            };

            EditorGUI.BeginChangeCheck();
            addedTarget = (GameObject)THelpersUI.GUI_ObjectFieldCentered(new GUIContent("", ""), addedTarget, typeof(GameObject), layoutOptions);
            if (EditorGUI.EndChangeCheck())
            {
                if (addedTarget != null)
                {
                    AddlayerTarget(addedTarget);
                    addedTarget = null;
                }
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            int dropBoxWidth = 175;
            int dropBoxHeight = 75;
            GUIStyle style = new GUIStyle(EditorStyles.miniTextField); style.fixedWidth = dropBoxWidth; style.fixedHeight = dropBoxHeight;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;

            GUILayout.BeginArea(new Rect((position.width / 2) - (dropBoxWidth / 2f), 30, dropBoxWidth, dropBoxHeight));
            //GUI.backgroundColor = new Color(1, 1, 1, 0.8f);

            if (GUILayout.Button(new GUIContent("DROP TARGET HERE\n\n▼", "Drop a GameObject (e.g. a camera, player character or NPC) from the scene to convert GPU models into interactable objects with physics, collisions... around this target.\n\nYou can drop as many objects you want to have multiple targets."), style))
            {
                InteractiveTargets.GetPlayerTargets();
            }

            GUILayout.EndArea();

            THelpersUI.GUI_HelpBox("Drop a GameObject (e.g. a camera, player character or NPC) from the scene to convert GPU models into interactable objects with physics, collisions... around this target.\n\nYou can drop as many objects you want to have multiple targets.", MessageType.Info);

            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 30; style.fixedHeight = 30;
            targetRemoved = false;

            if (InteractiveTargets.playerTargets == null || InteractiveTargets.playerTargets.Count == 0) return;
            if (globalStates == null || globalStates.Length == 0 || globalStates.Length != InteractiveTargets.playerTargets.Count) globalStates = new bool[InteractiveTargets.playerTargets.Count];
            if (targetStates == null || targetStates.Length == 0 || targetStates.Length != InteractiveTargets.playerTargets.Count) targetStates = new bool[InteractiveTargets.playerTargets.Count];

            for (int i = 0; i < InteractiveTargets.playerTargets.Count; i++)
            {
                GUILayout.Space(40);
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                GameObject cachedGo = InteractiveTargets.playerTargets[i];
                InteractiveTargets.playerTargets[i] = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("TARGET " + (i + 1), "Refernce to the GPU Layer in scene hierarchy"), InteractiveTargets.playerTargets[i], typeof(GameObject));
                if (EditorGUI.EndChangeCheck())
                {
                    if (InteractiveTargets.playerTargets[i] != cachedGo)
                        InteractiveTargets.playerTargets[i] = cachedGo;

                    //UpdatePlayerTargets(playerTargets[i]);
                }

                GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(-8);

                if (GUILayout.Button(new GUIContent(TResourcesManager.clearIcon, "Clear current target"), style))
                {
                    RemovePlayerTarget(InteractiveTargets.playerTargets[i]);
                    targetRemoved = true;
                }

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (targetRemoved) return;

                PlayerInteractions playerInteractions = InteractiveTargets.playerTargets[i].GetComponent<PlayerInteractions>();

                if (playerInteractions.GPULayers != null && playerInteractions.GPULayers.Count > 0)
                {
                    if (THelpersUI.Foldout("ADVANCED SETTINGS", ref globalStates[i]))
                    {
                        playerInteractions.maximumObjects = THelpersUI.GUI_IntSlider(new GUIContent("MAX. INTERACTIVE OBJECTS (POOL SIZE)", "Maximum count for interactive objects around target for each layer which will be updated dynamically in runtime"), playerInteractions.maximumObjects, 1, 128, 20);
                        playerInteractions.deadZoneMeters = THelpersUI.GUI_Slider(new GUIContent("DEAD ZONE UNITS", "Only update interactables if target moves in all directions every Dead Zone Units\n\nHigher value gives better performance but with less accurate GPU to CPU object conversion and suited for less dense environments\n\n0 means it will update interactive objects every frame"), playerInteractions.deadZoneMeters, 0f, 10f, -10);
                        playerInteractions.checkDistance3D = THelpersUI.GUI_Toggle(new GUIContent("3D DISTANCE CHECK", "If this option is enabled, all distances will be calculated as a 3D sphere around target\n\nIf disabled, all distances will be calculated based on XZ plane ignoring the Y axis (Height) of the target suited for Top-Down view games\n\nEnable this option for the best performance"), playerInteractions.checkDistance3D, -10);
                        //playerInteractions.neighborPatchesCount = THelpersUI.GUI_IntSlider(new GUIContent("NEIGHBOR PATCHES COUNT", "Defines how many neighbor rendering patches should be taken into account from target position to convert GPU Instances to Interactable Objects\n\nLower value gives better performance as the instance list will get smaller"), playerInteractions.neighborPatchesCount, 1, 10, -10);

                        GUILayout.Space(30);
                    }

                    if (!THelpersUI.Foldout("LAYERS", ref targetStates[i])) continue;

                    THelpersUI.GUI_HelpBox("Assign which layers around selected target should convert to interactable objects in game.\n\nFor the best performance, only select layers with colliders or if need specific interactions.", MessageType.Info, 20);
                    GUILayout.Space(20);

                    //THelpersUI.GUI_HelpBox("GPU LAYERS", MessageType.None);
                    //GUILayout.Space(20);

                    for (int j = 0; j < playerInteractions.GPULayers.Count; j++)
                    {
                        if (playerInteractions.GPULayers[j] == null) InteractiveTargets.GetPlayerTargets();
                        continue;
                    }

                    // Runtime GPU Instance layers in scene
                    for (int j = 0; j < playerInteractions.GPULayers.Count; j++)
                    {
                        THelpersUI.DrawUILine(THelpersUI.SubUIColor, 2, 20);
                        GameObject layerObject = playerInteractions.GPULayers[j].transform.parent.gameObject;

                        if (!layerObject.activeSelf)
                            THelpersUI.GUI_HelpBox("Layer is disabled in scene!", MessageType.Error, 20);

                        if (!layerObject.activeSelf) playerInteractions.statesGPU[j] = false;

                        int state = Convert.ToInt32(!playerInteractions.statesGPU[j]);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUI.GUI_SelectionGrid(state, convertSelection, new GUIStyle(EditorStyles.toolbarButton));
                        if (EditorGUI.EndChangeCheck()) playerInteractions.statesGPU[j] = !Convert.ToBoolean(state);

                        GUILayout.Space(5);

                        if (playerInteractions.GPULayersList[j] == null || playerInteractions.GPULayersList[j].Prefab == null)
                            continue;

                        EditorGUI.BeginDisabledGroup(!layerObject.activeSelf || !playerInteractions.statesGPU[j]);

                        if (playerInteractions.GPULayersList[j].Prefab != null)
                        {
                            if (!layerObject.activeSelf || !playerInteractions.statesGPU[j]) GUI.color = disabledColor;
                            else GUI.color = enabledColor;
                            ModelPreviewUI.ModelPreviewList(playerInteractions.GPULayersList[j].Prefab, new GUIStyle(EditorStyles.helpBox), 128);
                            GUI.color = enabledColor;
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        THelpersUI.GUI_ObjectField(new GUIContent("Layer " + (j + 1).ToString(), "GPU Instance layer for this target"), playerInteractions.GPULayers[j].transform.parent.gameObject, typeof(GameObject));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (!playerInteractions.GPULayersList[j].GetPrefabCollider())
                            THelpersUI.GUI_HelpBox("No Colliders detected on this layer's prefab!\nModels in this layer will not interact with Collision Detection & Physics!", MessageType.Warning);

                        playerInteractions.GPULayersDistances[j] = THelpersUI.GUI_Slider(new GUIContent("DISTANCE RADIUS", "Radius around player where GPU Instances will become normal GameObjects to be intractable"), playerInteractions.GPULayersDistances[j], 1, 500, 10);
                        if (playerInteractions.GPULayersDistances[j] > 200) THelpersUI.GUI_HelpBox("HIGHER VALUES MAY AFFECT PERFORMANCE ON DENSE LAYERS!", MessageType.Warning);

                        GUILayout.Space(40);
                        EditorGUI.EndDisabledGroup();
                    }
                }
                else
                    THelpersUI.GUI_HelpBox("No layers in scene! Options will become available after world generation!", MessageType.Warning);

                //if (playerInteractions.CollisionLayers != null && playerInteractions.CollisionLayers.Count > 0)
                //{
                //    THelpersUI.GUI_HelpBox("COLLISION LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // GPU Collision layers in scene
                //    for (int j = 0; j < playerInteractions.CollisionLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //
                //        if (playerInteractions.CollisionLayers[j].activeSelf)
                //        {
                //            THelpersUI.GUI_ObjectField(new GUIContent("COLLISION " + (j + 1), "Collision layer for this target"), playerInteractions.CollisionLayers[j], typeof(GameObject));
                //            playerInteractions.statesCollision[j] = THelpersUI.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesCollision[j]);
                //        }
                //        else
                //        {
                //            GUI.backgroundColor = disabledColor;
                //            THelpersUI.GUI_ObjectField(new GUIContent("COLLISION " + (j + 1), "Layer is disabled in hierarchy"), playerInteractions.CollisionLayers[j], typeof(GameObject));
                //            playerInteractions.statesCollision[j] = THelpersUI.GUI_Toggle(new GUIContent("", "Layer is disabled in hierarchy"), false);
                //            GUI.backgroundColor = Color.white;
                //        }
                //
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}
                //
                //if (playerInteractions.GameobjectLayers != null && playerInteractions.GameobjectLayers.Count > 0)
                //{
                //    THelpersUI.GUI_HelpBox("OBJECT LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // Runtime Gameobject layers in scene
                //    for (int j = 0; j < playerInteractions.GameobjectLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //        THelpersUI.GUI_ObjectField(new GUIContent("OBJECT " + (j + 1), "Gameobject layer for this target"), playerInteractions.GameobjectLayers[j], typeof(GameObject));
                //        playerInteractions.statesGameobject[j] = THelpersUI.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesGameobject[j]);
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}
                //
                //if (playerInteractions.FXLayers != null && playerInteractions.FXLayers.Count > 0)
                //{
                //    THelpersUI.GUI_HelpBox("FX LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // Runtime FX layers in scene
                //    for (int j = 0; j < playerInteractions.FXLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //        THelpersUI.GUI_ObjectField(new GUIContent("FX " + (j + 1), "FX layer for this target"), playerInteractions.FXLayers[j], typeof(GameObject));
                //        playerInteractions.statesFX[j] = THelpersUI.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesFX[j]);
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}

                THelpersUI.DrawUILine(30);
            }
        }

        private void AddlayerTarget(GameObject target)
        {
            if (!target.scene.IsValid())
            {
                EditorUtility.DisplayDialog("NON SCENE OBJECT", "Select a gameobject from the scene!", "OK");
                return;
            }

            if (InteractiveTargets.playerTargets.Contains(target))
            {
                EditorUtility.DisplayDialog("ALREADY IN LIST", "Target already exists!", "OK");
                return;
            }

            InteractiveTargets.playerTargets.Add(target);
            UpdatePlayerTargets();
        }

        private void RemovePlayerTarget(GameObject target)
        {
            InteractiveTargets.playerTargets.Remove(target);

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go == target && go.GetComponent<PlayerInteractions>() != null)
                        DestroyImmediate(go.GetComponent<PlayerInteractions>());
            }

            UpdatePlayerTargets();
        }

        private void UpdatePlayerTargets(GameObject tempGo = null)
        {
            for (int i = 0; i < InteractiveTargets.playerTargets.Count; i++)
            {
                if (InteractiveTargets.playerTargets[i] != null)
                {
                    if (InteractiveTargets.playerTargets[i].GetComponent<PlayerInteractions>() == null)
                    {
                        InteractiveTargets.playerTargets[i].AddComponent<PlayerInteractions>();
                        InteractiveTargets.playerTargets[i].GetComponent<PlayerInteractions>().ScanScatters();
                    }
                }
                else
                {
                    if (tempGo != null)
                    {
                        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                        {
                            if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                                if (go == tempGo && go.GetComponent<PlayerInteractions>() != null)
                                    DestroyImmediate(go.GetComponent<PlayerInteractions>());
                        }
                    }

                    InteractiveTargets.playerTargets.RemoveAt(i);
                    targetRemoved = true;
                }
            }
        }

        private void ClearPlayerTargets()
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go.GetComponent<PlayerInteractions>() != null)
                        DestroyImmediate(go.GetComponent<PlayerInteractions>());
            }
        }

     //   private void ScanScatterNodes()
     //   {
     //       for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
     //           for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
     //           {
     //               if (TTerraWorld.WorldGraph.graphList[i].nodes[j].GetType() == typeof(InstanceScatter))
     //                   TDebug.LogInfoToUnityUI(TTerraWorld.WorldGraph.graphList[i].nodes[j].Data.name);
     //               else if (TTerraWorld.WorldGraph.graphList[i].nodes[j].GetType() == typeof(ObjectScatter))
     //                   TDebug.LogInfoToUnityUI(TTerraWorld.WorldGraph.graphList[i].nodes[j].Data.name);
     //           }
     //   }


        // Graph Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void ActivateVFX()
        {
            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "ActivateVFX");
            SceneSettingsManager.Active = true;
        }

        private void ClearCache()
        {
            if (EditorUtility.DisplayDialog("CLEAR CACHE", "Are you sure you want to clear all cache data from the system?", "No", "Yes"))
                return;

            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "ClearCacheData");

            if (Directory.Exists(TAddresses.cachePath))
                Directory.Delete(TAddresses.cachePath, true);
        }

        private void AddModulesGUI()
        {
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.HelpBox(activeGraph._title, MessageType.None);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Processors
            EditorGUILayout.BeginHorizontal();
            AddProcessors();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (activeGraph.GetType() == typeof(THeightmapGraph) || activeGraph.GetType() == typeof(TColormapGraph))
                GUILayout.Space(5);

            //   // Operators
            //   if (activeGraph.GetType() == typeof(THeightmapGraph) || activeGraph.GetType() == typeof(TColormapGraph))
            //   {
            //       EditorGUI.BeginChangeCheck();
            //       EditorGUILayout.BeginHorizontal();
            //       AddOperators();
            //       GUILayout.FlexibleSpace();
            //       EditorGUILayout.EndHorizontal();
            //
            //       GUILayout.Space(5);
            //   }

            // Masks
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            AddMasks();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            // Scatters
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            AddScatters();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // TerraMeshes
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            AddTerraMesh();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            //   // Biome Operators
            //   if (activeGraph.GetType() == typeof(TBiomesGraph))
            //   {
            //       EditorGUI.BeginChangeCheck();
            //       EditorGUILayout.BeginHorizontal();
            //       AddOperators();
            //       GUILayout.FlexibleSpace();
            //       EditorGUILayout.EndHorizontal();
            //   }
        }

        private void AddProcessors()
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldoutPreDrop);
            style.fixedWidth = 200;

            if (activeGraph.GetType() == typeof(THeightmapGraph))
            {
                //heightmapProcessors = (HeightmapProcessors)EditorGUILayout.EnumPopup(heightmapProcessors, style);

                int listIndex = 0;
                HeightmapProcessors[] nodesArray = Enum.GetValues(typeof(HeightmapProcessors)).Cast<HeightmapProcessors>().ToArray();
                List<string> nodesArrayNames = new List<string>();

                EditorGUI.BeginChangeCheck();

                if (activeGraph.nodes.Count == 1)
                {
                    nodesArrayNames.Add("Add Source");
                    nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));
                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    heightmapProcessors = (HeightmapProcessors)listIndex;
                }
                else
                {
                    nodesArrayNames.Add("Add Processor");

                    // Index starts from 1 to have only 1 source node in this graph
                    for (int i = 1; i < nodesArray.Length; i++)
                        nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);

                    heightmapProcessors = (HeightmapProcessors)listIndex;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (activeGraph.nodes.Count == 1)
                    {
                        ((THeightmapGraph)activeGraph).AddNode(HeightmapProcessors.Heightmap_Source);
                        PlaceModuleInGraph();
                    }
                    else if (listIndex > 0)
                    {
                        ((THeightmapGraph)activeGraph).AddNode(heightmapProcessors);
                        PlaceModuleInGraph();
                    }
                }
            }
            else if (activeGraph.GetType() == typeof(TColormapGraph))
            {
                //colormapProcessors = (ColormapProcessors)EditorGUILayout.EnumPopup(colormapProcessors, style);

                int listIndex = 0;
                ColormapProcessors[] nodesArray = Enum.GetValues(typeof(ColormapProcessors)).Cast<ColormapProcessors>().ToArray();
                List<string> nodesArrayNames = new List<string>();

                EditorGUI.BeginChangeCheck();

                if (activeGraph.nodes.Count == 5)
                {
                    nodesArrayNames.Add("Add Source");
                    nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));
                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    colormapProcessors = (ColormapProcessors)listIndex;
                }
                else
                {
                    nodesArrayNames.Add("Add Processor");

                    for (int i = 1; i < nodesArray.Length; i++)
                        nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    colormapProcessors = (ColormapProcessors)listIndex;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (activeGraph.nodes.Count == 5)
                    {
                        ((TColormapGraph)activeGraph).AddNode(ColormapProcessors.Satellite_Image_Source);
                        PlaceModuleInGraph();
                    }
                    else if (listIndex > 0)
                    {
                        ((TColormapGraph)activeGraph).AddNode(colormapProcessors);
                        PlaceModuleInGraph();
                    }
                }
            }
        }

        //   private void AddOperators()
        //   {
        //       GUIStyle style = new GUIStyle();
        //       style = new GUIStyle(EditorStyles.foldoutPreDrop);
        //       style.fixedWidth = 200;
        //
        //       if (activeGraph.GetType() == typeof(THeightmapGraph))
        //       {
        //           // Only allow Operators if a Processor module exists
        //           if (activeGraph.nodes.Count < 2)
        //               return;
        //
        //           //heightmapOperators = (HeightmapOperators)EditorGUILayout.EnumPopup(heightmapOperators, style);
        //
        //           int listIndex = 0;
        //           HeightmapOperators[] nodesArray = Enum.GetValues(typeof(HeightmapOperators)).Cast<HeightmapOperators>().ToArray();
        //           List<string> nodesArrayNames = new List<string>();
        //
        //           if (activeGraph.nodes.Count == 0)
        //           {
        //               listIndex = 0;
        //               nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               heightmapOperators = (HeightmapOperators)listIndex;
        //           }
        //           else
        //           {
        //               nodesArrayNames.Add("Add Operator");
        //
        //               for (int i = 0; i < nodesArray.Length; i++)
        //                   nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               heightmapOperators = (HeightmapOperators)listIndex - 1;
        //           }
        //
        //           if (EditorGUI.EndChangeCheck())
        //           {
        //               ((THeightmapGraph)activeGraph).AddNode(heightmapOperators);
        //               PlaceModuleInGraph();
        //           }
        //       }
        //       else if (activeGraph.GetType() == typeof(TColormapGraph))
        //       {
        //           // Only allow Operators if a Processor module exists
        //           if (activeGraph.nodes.Count < 6)
        //               return;
        //
        //           //colormapOperators = (ColormapOperators)EditorGUILayout.EnumPopup(colormapOperators, style);
        //
        //           int listIndex = 0;
        //           ColormapOperators[] nodesArray = Enum.GetValues(typeof(ColormapOperators)).Cast<ColormapOperators>().ToArray();
        //           List<string> nodesArrayNames = new List<string>();
        //
        //           if (activeGraph.nodes.Count == 0)
        //           {
        //               listIndex = 0;
        //               nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               colormapOperators = (ColormapOperators)listIndex;
        //           }
        //           else
        //           {
        //               nodesArrayNames.Add("Add Operator");
        //
        //               for (int i = 0; i < nodesArray.Length; i++)
        //                   nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               colormapOperators = (ColormapOperators)listIndex - 1;
        //           }
        //
        //           if (EditorGUI.EndChangeCheck())
        //           {
        //               ((TColormapGraph)activeGraph).AddNode(colormapOperators);
        //               PlaceModuleInGraph();
        //           }
        //       }
        //       else if (activeGraph.GetType() == typeof(TBiomesGraph))
        //       {
        //           // Only allow Operators if a Processor module exists
        //           if (activeGraph.nodes.Count < 1)
        //               return;
        //
        //           int listIndex = 0;
        //           BiomeOperators[] nodesArray = Enum.GetValues(typeof(BiomeOperators)).Cast<BiomeOperators>().ToArray();
        //           List<string> nodesArrayNames = new List<string>();
        //
        //           if (activeGraph.nodes.Count == 0)
        //           {
        //               listIndex = 0;
        //               nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               biomeOperators = (BiomeOperators)listIndex;
        //           }
        //           else
        //           {
        //               nodesArrayNames.Add("Add Operator");
        //
        //               for (int i = 0; i < nodesArray.Length; i++)
        //                   nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));
        //
        //               listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
        //               biomeOperators = (BiomeOperators)listIndex - 1;
        //           }
        //
        //           if (EditorGUI.EndChangeCheck())
        //           {
        //               ((TBiomesGraph)activeGraph).AddNode(biomeOperators);
        //               PlaceModuleInGraph();
        //           }
        //       }
        //   }

        private void AddMasks()
        {
            //GUIStyle style = new GUIStyle();
            GUIStyle style = new GUIStyle(EditorStyles.foldoutPreDrop);
            style.fixedWidth = 200;

            if (activeGraph.GetType() == typeof(THeightmapGraph))
            {
                // Only allow Masks if a Processor module exists
                if (activeGraph.nodes.Count < 2)
                    return;

                //heightmapMasks = (HeightmapMasks)EditorGUILayout.EnumPopup(heightmapMasks, style);

                int listIndex = 0;
                HeightmapMasks[] nodesArray = Enum.GetValues(typeof(HeightmapMasks)).Cast<HeightmapMasks>().ToArray();
                List<string> nodesArrayNames = new List<string>();

                if (activeGraph.nodes.Count == 0)
                {
                    listIndex = 0;
                    nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    heightmapMasks = (HeightmapMasks)listIndex;
                }
                else
                {
                    nodesArrayNames.Add("Add Mask");

                    for (int i = 0; i < nodesArray.Length; i++)
                        nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    heightmapMasks = (HeightmapMasks)listIndex - 1;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ((THeightmapGraph)activeGraph).AddNode(heightmapMasks);
                    PlaceModuleInGraph();
                }
            }

            if (activeGraph.GetType() == typeof(TColormapGraph))
            {
                // Only allow Masks if a Processor module exists
                if (activeGraph.nodes.Count < 2)
                    return;

                //heightmapMasks = (HeightmapMasks)EditorGUILayout.EnumPopup(heightmapMasks, style);

                int listIndex = 0;
                ColormapMasks[] nodesArray = Enum.GetValues(typeof(ColormapMasks)).Cast<ColormapMasks>().ToArray();
                List<string> nodesArrayNames = new List<string>();

                if (activeGraph.nodes.Count == 0)
                {
                    listIndex = 0;
                    nodesArrayNames.Add(nodesArray[0].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    colormapMasks = (ColormapMasks)listIndex;
                }
                else
                {
                    nodesArrayNames.Add("Add Mask");

                    for (int i = 0; i < nodesArray.Length; i++)
                        nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    colormapMasks = (ColormapMasks)listIndex - 1;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ((TColormapGraph)activeGraph).AddNode(colormapMasks);
                    PlaceModuleInGraph();
                }
            }


            if (activeGraph.GetType() == typeof(TBiomesGraph))
            {

                //heightmapMasks = (HeightmapMasks)EditorGUILayout.EnumPopup(heightmapMasks, style);

                int listIndex = 0;
                BiomeMasks[] nodesArray = Enum.GetValues(typeof(BiomeMasks)).Cast<BiomeMasks>().ToArray();
                List<string> nodesArrayNames = new List<string>();

                {
                    nodesArrayNames.Add("Add Mask");

                    for (int i = 0; i < nodesArray.Length; i++)
                        nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                    listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                    biomeMasks = (BiomeMasks)listIndex - 1;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ((TBiomesGraph)activeGraph).AddNode(biomeMasks);
                    PlaceModuleInGraph();
                }
            }

        }

        private void AddScatters()
        {
            GUIStyle style = new GUIStyle();
            style = new GUIStyle(EditorStyles.foldoutPreDrop);
            style.fixedWidth = 200;

            if (activeGraph.GetType() == typeof(TBiomesGraph))
            {
                int listIndex = 0;
                BiomeScatters[] nodesArray = Enum.GetValues(typeof(BiomeScatters)).Cast<BiomeScatters>().ToArray();
                List<string> nodesArrayNames = new List<string>();
                nodesArrayNames.Add("Add Scatter");

                for (int i = 0; i < nodesArray.Length; i++)
                    nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                biomeScatters = (BiomeScatters)listIndex - 1;

                if (EditorGUI.EndChangeCheck())
                {
                    if (listIndex <= 0)
                        return;

                    ((TBiomesGraph)activeGraph).AddNode(biomeScatters);
                    PlaceModuleInGraph();
                }
            }
        }

        private void AddTerraMesh()
        {
            GUIStyle style = new GUIStyle();
            style = new GUIStyle(EditorStyles.foldoutPreDrop);
            style.fixedWidth = 200;

            if (activeGraph.GetType() == typeof(TBiomesGraph))
            {
                int listIndex = 0;
                BiomeMeshGenerators[] nodesArray = Enum.GetValues(typeof(BiomeMeshGenerators)).Cast<BiomeMeshGenerators>().ToArray();
                List<string> nodesArrayNames = new List<string>();
                nodesArrayNames.Add("Add Mesh");

                for (int i = 0; i < nodesArray.Length; i++)
                    nodesArrayNames.Add(nodesArray[i].ToString().Replace("_", " "));

                listIndex = EditorGUILayout.Popup(listIndex, nodesArrayNames.ToArray(), style);
                biomeMeshGenerators = (BiomeMeshGenerators)listIndex - 1;

                if (EditorGUI.EndChangeCheck())
                {
                    if (listIndex <= 0)
                        return;

                    ((TBiomesGraph)activeGraph).AddNode(biomeMeshGenerators);
                    PlaceModuleInGraph();
                }
            }
        }

        private void PlaceModuleInGraph()
        {
            InitModule();
            THelpersUI.ActiveNode = activeGraph.nodes[activeGraph.nodes.Count - 1];
            //RearrangeModules();
        }

        private void ModulesGUI()
        {
            GUIStyle style = new GUIStyle();

            if (activeGraph.nodes.Count >= 0)
            {
                maxRectPosY = float.MinValue;

                for (int i = 0; i < activeGraph.nodes.Count; i++)
                {
                    if (maxRectPosY < activeGraph.nodes[i].Data.position.Y)
                        maxRectPosY = activeGraph.nodes[i].Data.position.Y;
                }

                nodeSpace = Mathf.Clamp(maxRectPosY - nodeSize.y - headerSpace + 20, 10, 64000);

                //GUILayout.Space(30);

                //if(GUILayout.Button(arrangeIcon, GUILayout.Width(40), GUILayout.Height(40)))
                //{
                //    RearrangeModules();
                //}

                //GUILayout.Space(5);

                if (GUILayout.Button(TResourcesManager.clearIcon, GUILayout.Width(40), GUILayout.Height(40)))
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "ClearGraph");
                    ClearGraphCurrent();
                }
            }

            Handles.BeginGUI();

            for (int i = 0; i < activeGraph.nodes.Count; i++)
            {
                TNode currentNode = activeGraph.nodes[i];
                int offset = 0;

                for (int j = 0; j < currentNode.inputConnections.Count; j++)
                {
                    TNode inputNode = activeGraph.GetNodeByID(currentNode.inputConnections[j].previousNodeID);

                    if (inputNode != null)
                    {
                        bool isLocalModule = IsModuleInCurrentGraph(inputNode);

                        if (!isLocalModule)
                        {
                            startPositionModule.x = currentNode.xMin();
                            startPositionModule.y = currentNode.center().Y;
                            startTangentModule = new Vector2(currentNode.xMin(), currentNode.center().Y);
                            endPositionModule.x = currentNode.Data.position.X - externalNodeSpace - (offset * externalNodeSpace);
                            endPositionModule.y = currentNode.center().Y;
                            endTangentModule = new Vector2(currentNode.xMin() - 30, currentNode.center().Y);

                            if (currentNode.Progress > 0f && currentNode.Progress < 1f)
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColorInProgress, null, wireWidth);
                            else
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColor, null, wireWidth);

                            offset++;
                        }
                        else
                        {
                            startPositionModule.x = inputNode.xMax();
                            startPositionModule.y = inputNode.center().Y;
                            startTangentModule = new Vector2(inputNode.xMax() + nodeSize.x, inputNode.center().Y);
                            endPositionModule.x = currentNode.xMin();
                            endPositionModule.y = currentNode.center().Y;
                            endTangentModule = new Vector2(currentNode.xMin() - nodeSize.x, currentNode.center().Y);

                            if (currentNode.Progress > 0f && currentNode.Progress < 1f)
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColorInProgress, null, wireWidth);
                            else
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColor, null, wireWidth);
                        }
                    }
                }

                offset = 0;
                //TODO : this function runs under GUI
                List<TNode> outputNodes = activeGraph.GetOutputNodes(currentNode.Data.ID);

                for (int j = 0; j < outputNodes.Count; j++)
                {
                    TNode outputNode = outputNodes[j];

                    if (outputNode != null)
                    {
                        bool isLocalModule = IsModuleInCurrentGraph(outputNode);

                        if (!isLocalModule)
                        {
                            startPositionModule.x = currentNode.xMax();
                            startPositionModule.y = currentNode.center().Y;
                            startTangentModule = new Vector2(currentNode.xMax(), currentNode.center().Y);
                            endPositionModule.x = currentNode.Data.position.X + externalNodeSpace + (nodeSize.x / 2f) + (offset * externalNodeSpace);
                            endPositionModule.y = currentNode.center().Y;
                            endTangentModule = new Vector2(currentNode.xMax() + 30, currentNode.center().Y);

                            if (currentNode.Progress > 0f && currentNode.Progress < 1f)
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColorInProgress, null, wireWidth);
                            else
                                Handles.DrawBezier(startPositionModule, endPositionModule, startTangentModule, endTangentModule, wireColor, null, wireWidth);

                            offset++;
                        }
                    }
                }
            }

            Handles.EndGUI();

            BeginWindows();

            GUI.color = Color.white;

            int externalIndex = 0;
            externalIDs = new Dictionary<int, int>();

            //style = new GUIStyle(EditorStyles.textField);
            //style.fontSize = 8;
            //style.alignment = TextAnchor.UpperCenter;

            for (int i = 0; i < activeGraph.nodes.Count; i++)
            {
                TNode currentNode = activeGraph.nodes[i];
                int offset = 0;

                for (int j = 0; j < currentNode.inputConnections.Count; j++)
                {
                    TNode externalNode = activeGraph.GetNodeByID(currentNode.inputConnections[j].previousNodeID);

                    if (externalNode != null)
                    {
                        bool isLocalModule = IsModuleInCurrentGraph(externalNode);

                        if (!isLocalModule)
                        {
                            if (externalNode.isActive)
                                GUI.color = enabledColor;
                            else
                                GUI.color = disabledColor;

                            activeNodeRect.x = currentNode.Data.position.X - externalNodeSpace - (offset * externalNodeSpace);
                            activeNodeRect.y = currentNode.Data.position.Y + (nodeSize.y / 4f);
                            activeNodeRect.width = nodeSizeExternal.x;
                            activeNodeRect.height = nodeSizeExternal.y;

                            GUI.color = Color.white;

                            externalIDs.Add(externalIndex, externalNode.Data.ID);
                            //activeNodeRect = GUI.Window(externalIndex, activeNodeRect, ModuleFunctionExternal, new GUIContent("", ""), style);
                            activeNodeRect = GUI.Window(externalIndex, activeNodeRect, ModuleFunctionExternal, new GUIContent("", ""));

                            offset++;
                            externalIndex++;
                        }
                    }
                }

                offset = 0;
                //TODO : this function runs under GUI
                List<TNode> outputNodes = activeGraph.GetOutputNodes(currentNode.Data.ID);

                for (int j = 0; j < outputNodes.Count; j++)
                {
                    TNode externalNode = outputNodes[j];

                    if (externalNode != null)
                    {
                        bool isLocalModule = IsModuleInCurrentGraph(externalNode);

                        if (!isLocalModule)
                        {
                            if (externalNode.isActive)
                                GUI.color = enabledColor;
                            else
                                GUI.color = disabledColor;

                            activeNodeRect.x = currentNode.Data.position.X + externalNodeSpace + (nodeSize.x / 2f) + (offset * externalNodeSpace);
                            activeNodeRect.y = currentNode.Data.position.Y + (nodeSize.y / 4f);
                            activeNodeRect.width = nodeSizeExternal.x;
                            activeNodeRect.height = nodeSizeExternal.y;

                            GUI.color = Color.white;

                            externalIDs.Add(externalIndex, externalNode.Data.ID);
                            //activeNodeRect = GUI.Window(externalIndex, activeNodeRect, ModuleFunctionExternal, new GUIContent("", ""), style);
                            activeNodeRect = GUI.Window(externalIndex, activeNodeRect, ModuleFunctionExternal, new GUIContent("", ""));

                            offset++;
                            externalIndex++;
                        }
                    }
                }

                if (currentNode.Data.moduleType != ModuleType.Master)
                {
                    if (currentNode.isActive)
                        GUI.color = enabledColor;
                    else
                        GUI.color = disabledColor;

                    activeNodeRect.x = currentNode.Data.position.X;
                    activeNodeRect.y = currentNode.Data.position.Y;
                    activeNodeRect.width = currentNode.Data.position.Z;
                    activeNodeRect.height = currentNode.Data.position.W;

                    if (currentNode == THelpersUI.ActiveNode)
                        GUI.color = new Color(0.4f, 0.7f, 0.8f, 0.7f);
                    else
                        GUI.color = new Color(0.65f, 0.8f, 0.8f, 0.4f);

                    //activeNodeRect = GUI.Window(currentNode.Data.ID, activeNodeRect, ModuleFunction, new GUIContent(currentNode.Data.name.ToUpper(), "", style));
                    activeNodeRect = GUI.Window(currentNode.Data.ID, activeNodeRect, ModuleFunction, new GUIContent("", ""));
                    currentNode.Data.position = new System.Numerics.Vector4(activeNodeRect.x, activeNodeRect.y, activeNodeRect.width, activeNodeRect.height);
                }
                else
                {
                    if (currentNode.isActive)
                        GUI.color = enabledColor;
                    else
                        GUI.color = disabledColor;

                    activeNodeRect.x = currentNode.Data.position.X;
                    activeNodeRect.y = currentNode.Data.position.Y;
                    activeNodeRect.width = currentNode.Data.position.Z;
                    activeNodeRect.height = currentNode.Data.position.W;

                    if (currentNode == THelpersUI.ActiveNode)
                        GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    else
                        GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.4f);

                    //activeNodeRect = GUI.Window(currentNode.Data.ID, activeNodeRect, ModuleFunctionMaster, "", style);
                    activeNodeRect = GUI.Window(currentNode.Data.ID, activeNodeRect, ModuleFunctionMaster, "");

                    currentNode.Data.position = new System.Numerics.Vector4(activeNodeRect.x, activeNodeRect.y, activeNodeRect.width, activeNodeRect.height);
                }

                if (Event.current.type == EventType.MouseDown)
                    THelpersUI.ActiveNode = null;
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            EndWindows();

            GUILayout.Space(nodeSpace);
        }

        private void RearrangeModules()
        {
            if (activeGraph.nodes.Count <= 0)
                return;

            List<TNode> arrangedNodes = GetListOfArrangedNodes();

            // Entry node
            arrangedNodes[0].Data.position = new System.Numerics.Vector4
            (
                (position.width / 2) - (nodeSize.x / 2),
                headerSpace,
                nodeSize.x,
                nodeSize.y
            );

            // Arrange graph branches
            for (int j = 0; j <= branchCount; j++)
            {
                int counter = 1;

                for (int i = 1; i < arrangedNodes.Count; i++)
                {
                    TNode currentNode = arrangedNodes[i];
                    int branchNumber = 0;
                    branches.TryGetValue(currentNode, out branchNumber);

                    if (branchNumber == j)
                    {
                        float distance = nodeSize.x * 1.5f;
                        float centerOffset = (distance / 2f) * branchCount;

                        currentNode.Data.position = new System.Numerics.Vector4
                        (
                            (position.width / 2) - (nodeSize.x / 2) - (branchNumber * distance) + centerOffset,
                            headerSpace + (counter * (nodeSize.y + moduleSpace)),
                            nodeSize.x,
                            nodeSize.y
                        );

                        counter++;
                    }
                }
            }
        }

        private void ClearGraphCurrent()
        {
            if (activeGraph.nodes.Count <= 0)
                return;

            if (EditorUtility.DisplayDialog("CLEAR GRAPH", "Are you sure you want to clear current graph?", "No", "Yes"))
                return;

            for (int i = activeGraph.nodes.Count - 1; i >= 0; i--)
                if (activeGraph.nodes[i].Data.moduleType != ModuleType.Master)
                    activeGraph.RemoveNode(activeGraph.nodes[i].Data.ID);

            //if (TBoundingBox.boundingBox != null)
            //DestroyImmediate(TBoundingBox.boundingBox);
        }

        private void PlaceMasterNodes(TNode currentNode)
        {
            if (currentNode.Data.nodePosition == NodePosition._1)
            {
                currentNode.Data.position = new System.Numerics.Vector4
                (
                    position.width - nodeSizeMaster.x - 35,
                    headerSpace,
                    nodeSizeMaster.x,
                    nodeSizeMaster.y
                );
            }
            else if (currentNode.Data.nodePosition == NodePosition._2)
            {
                currentNode.Data.position = new System.Numerics.Vector4
                (
                    position.width - (nodeSizeMaster.x / 2) - 10,
                    headerSpace + nodeSizeMaster.y + 20,
                    nodeSizeMaster.x,
                    nodeSizeMaster.y
                );
            }
            else if (currentNode.Data.nodePosition == NodePosition._3)
            {
                currentNode.Data.position = new System.Numerics.Vector4
                (
                    position.width - (nodeSizeMaster.x / 2) - 10,
                    headerSpace + (nodeSizeMaster.y * 2) + 20 + 1,
                    nodeSizeMaster.x,
                    nodeSizeMaster.y
                );
            }
            else if (currentNode.Data.nodePosition == NodePosition._4)
            {
                currentNode.Data.position = new System.Numerics.Vector4
                (
                    position.width - (nodeSizeMaster.x / 2) - 10,
                    headerSpace + (nodeSizeMaster.y * 3) + 20 + 2,
                    nodeSizeMaster.x,
                    nodeSizeMaster.y
                );
            }
            else if (currentNode.Data.nodePosition == NodePosition._5)
            {
                currentNode.Data.position = new System.Numerics.Vector4
                (
                    position.width - (nodeSizeMaster.x / 2) - 10,
                    headerSpace + (nodeSizeMaster.y * 4) + 20 + 3,
                    nodeSizeMaster.x,
                    nodeSizeMaster.y
                );
            }

            currentNode.Data.nodePosition = NodePosition._Float;
        }

        private List<TNode> GetListOfArrangedNodes()
        {
            List<TNode> arrangedNodes = new List<TNode>();
            branchCount = -1;
            branches = new Dictionary<TNode, int>();

            for (int i = 0; i < activeGraph.nodes.Count; i++)
            {
                if (i == 0)
                    arrangedNodes.Add(activeGraph.nodes[0]);
                else
                {
                    foreach (TNode n in activeGraph.nodes)
                    {
                        if (!arrangedNodes.Contains(n))
                        {
                            List<TConnection> inputConnections = n.inputConnections;

                            for (int k = 0; k < inputConnections.Count; k++)
                            {
                                if (inputConnections[k].previousNodeID == activeGraph.nodes[i - 1].Data.ID)
                                {
                                    branchCount++;
                                    arrangedNodes.Add(n);
                                    branches.Add(n, branchCount);
                                }

                                foreach (TNode n2 in activeGraph.nodes)
                                {
                                    if (!arrangedNodes.Contains(n2))
                                    {
                                        List<TConnection> inputConnections2 = n2.inputConnections;

                                        for (int l = 0; l < inputConnections2.Count; l++)
                                        {
                                            if (inputConnections2[l].previousNodeID == arrangedNodes[arrangedNodes.Count - 1].Data.ID)
                                            {
                                                arrangedNodes.Add(n2);
                                                branches.Add(n2, branchCount);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return arrangedNodes;
        }

        //private void BackgroundUIColor(Color backgroundColor, int xOffset = 0, int yOffset = 0)
        //{
        //    int res = 32;
        //    int gridX = (int)(position.width / res) + 5;
        //    int gridY = (int)(position.height / res);
        //
        //    GUI.color = backgroundColor;
        //    for (int x = 0; x < gridX; x++)
        //    {
        //        for (int y = 0; y < gridY; y++)
        //        {
        //            GUILayout.BeginArea(new Rect((x * res) + xOffset, (y * res) + yOffset, res + 4, res + 4));
        //            GUILayout.Label(TResourcesManager.bgWhite);
        //            GUILayout.EndArea();
        //        }
        //    }
        //    GUI.color = Color.white;
        //}

        private void CommonGUISettings()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            scrollPositionSettings = EditorGUILayout.BeginScrollView(scrollPositionSettings, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            //BackgroundUIColor(UIColor);

            if (THelpersUI.ActiveNode != null)
            {
                Type nodeType = THelpersUI.ActiveNode.GetType();

                if (activeGraph.nodes.Count == 0)
                {
                    nodeType = null;

                    //if (TBoundingBox.boundingBox != null)
                    //DestroyImmediate(TBoundingBox.boundingBox);

                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                    return;
                }

                // Masters
                if (nodeType.Name.Equals("HeightmapMaster"))
                {
                    HeightmapMaster node = (HeightmapMaster)THelpersUI.ActiveNode;
                    HeightmapMasterSettings(ref node);
                }
                else if (nodeType.Name.Equals("ColormapMaster"))
                {
                    ColormapMaster node = (ColormapMaster)THelpersUI.ActiveNode;
                    ColormapMasterSettings(ref node);
                }
                else if (nodeType.Name.Equals("TerrainLayerMaster1"))
                {
                    TerrainLayerMaster1 node = (TerrainLayerMaster1)THelpersUI.ActiveNode;
                    TerrainLayerMaster1Settings(ref node);
                }
                else if (nodeType.Name.Equals("TerrainLayerMaster2"))
                {
                    TerrainLayerMaster2 node = (TerrainLayerMaster2)THelpersUI.ActiveNode;
                    TerrainLayerMaster2Settings(ref node);
                }
                else if (nodeType.Name.Equals("TerrainLayerMaster3"))
                {
                    TerrainLayerMaster3 node = (TerrainLayerMaster3)THelpersUI.ActiveNode;
                    TerrainLayerMaster3Settings(ref node);
                }
                else if (nodeType.Name.Equals("TerrainLayerMaster4"))
                {
                    TerrainLayerMaster4 node = (TerrainLayerMaster4)THelpersUI.ActiveNode;
                    TerrainLayerMaster4Settings(ref node);
                }

                // Processors
                else if (nodeType.Name.Equals("HeightmapSource"))
                {
                    HeightmapSource node = (HeightmapSource)THelpersUI.ActiveNode;
                    HeightmapSourceSettings(ref node);
                }
                else if (nodeType.Name.Equals("SmoothProcess"))
                {
                    SmoothProcess node = (SmoothProcess)THelpersUI.ActiveNode;
                    HeightmapSmoothSettings(ref node);
                }
                else if (nodeType.Name.Equals("HydraulicErosionMainProcess") || nodeType.Name.Equals("HydraulicErosionProcess"))
                {
                    HydraulicErosionMainProcess node = (HydraulicErosionMainProcess)THelpersUI.ActiveNode;
                    HydraulicErosionMainSettings(ref node);
                }
                //     else if (nodeType.Name.Equals("HydraulicErosionProcess"))
                //     {
                //         HydraulicErosionProcess node = (HydraulicErosionProcess)THelpersUI.ActiveNode;
                //         HydraulicErosionSettings(ref node);
                //     }
                else if (nodeType.Name.Equals("WaterErosionProcess"))
                {
                    WaterErosionProcess node = (WaterErosionProcess)THelpersUI.ActiveNode;
                    WaterErosionSettings(ref node);
                }
                else if (nodeType.Name.Equals("ThermalErosionProcess"))
                {
                    ThermalErosionProcess node = (ThermalErosionProcess)THelpersUI.ActiveNode;
                    ThermalErosionSettings(ref node);
                }
                else if (nodeType.Name.Equals("TerraceProcess"))
                {
                    TerraceProcess node = (TerraceProcess)THelpersUI.ActiveNode;
                    TerraceSettings(ref node);
                }
                else if (nodeType.Name.Equals("VoxelProcess"))
                {
                    VoxelProcess node = (VoxelProcess)THelpersUI.ActiveNode;
                    VoxelTerrainSettings(ref node);
                }
                else if (nodeType.Name.Equals("SatelliteImage"))
                {
                    SatelliteImage node = (SatelliteImage)THelpersUI.ActiveNode;
                    SatelliteImageSettings(ref node);
                }
                //else if (nodeType.Name.Equals("ShadowRemover"))
                //{
                //    ShadowRemover node = (ShadowRemover)THelpersUI.ActiveNode;
                //    ShadowRemoverSettings(ref node);
                //}
                else if (nodeType.Name.Equals("WaterGenerator"))
                {
                    WaterGenerator node = (WaterGenerator)THelpersUI.ActiveNode;
                    WaterGeneratorSettings(ref node);
                }
                else if (nodeType.Name.Equals("BiomeExtractor"))
                {
                    BiomeExtractor node = (BiomeExtractor)THelpersUI.ActiveNode;
                    BiomeExtractorSettings(ref node);
                }
                //else if (nodeType.Name.Equals("LakeShoreMask"))
                //{
                //    LakeShoreMask node = (LakeShoreMask)THelpersUI.ActiveNode;
                //    LakeShoreMaskSettings(ref node);
                //}
                else if (nodeType.Name.Equals("ObjectScatter"))
                {
                    ObjectScatter node = (ObjectScatter)THelpersUI.ActiveNode;
                    ObjectScatterSettings(ref node);
                }
                else if (nodeType.Name.Equals("TreeScatter"))
                {
                    TreeScatter node = (TreeScatter)THelpersUI.ActiveNode;
                    TreeScatterSettings(ref node);
                }
                else if (nodeType.Name.Equals("InstanceScatter"))
                {
                    InstanceScatter node = (InstanceScatter)THelpersUI.ActiveNode;
                    InstanceScatterSettings(ref node);
                }
                else if (nodeType.Name.Equals("GrassScatter"))
                {
                    GrassScatter node = (GrassScatter)THelpersUI.ActiveNode;
                    GrassScatterSettings(ref node);
                }
                else if (nodeType.Name.Equals("Mask2DetailTexture"))
                {
                    Mask2DetailTexture node = (Mask2DetailTexture)THelpersUI.ActiveNode;
                    Mask2DetailTextureSettings(ref node);
                }
                else if (nodeType.Name.Equals("Mask2ColorMap"))
                {
                    Mask2ColorMap node = (Mask2ColorMap)THelpersUI.ActiveNode;
                    Mask2ColorMapSettings(ref node);
                }
                else if (nodeType.Name.Equals("MeshGenerator"))
                {
                    MeshGenerator node = (MeshGenerator)THelpersUI.ActiveNode;
                    MeshGeneratorSettings(ref node);
                }

                // Operators
                else if (nodeType.Name.Equals("ApplyMask"))
                {
                    ApplyMask node = (ApplyMask)THelpersUI.ActiveNode;
                    ApplyMaskSettings(ref node);
                }
                //else if (nodeType.Name.Equals("ColormapFromSlope"))
                //{
                //    ColormapFromSlope node = (ColormapFromSlope)THelpersUI.ActiveNode;
                //    ColormapFromSlopeSettings(ref node);
                //}
                else if (nodeType.Name.Equals("MaskBlendOperator"))
                {
                    MaskBlendOperator node = (MaskBlendOperator)THelpersUI.ActiveNode;
                    MaskBlendSettings(ref node);
                }

                // Masks
                else if (nodeType.Name.Equals("Slopemask"))
                {
                    Slopemask node = (Slopemask)THelpersUI.ActiveNode;
                    SlopemaskSettings(ref node);
                }
                //else if (nodeType.Name.Equals("Slopemap"))
                //{
                //    Slopemap node = (Slopemap)THelpersUI.ActiveNode;
                //    SlopemapSettings(ref node);
                //}
                else if (nodeType.Name.Equals("Flowmap"))
                {
                    Flowmap node = (Flowmap)THelpersUI.ActiveNode;
                    FlowmapSettings(ref node);
                }
                else if (nodeType.Name.Equals("Image2Mask"))
                {
                    Image2Mask node = (Image2Mask)THelpersUI.ActiveNode;
                    Image2MaskSettings(ref node);
                }
                //else if (nodeType.Name.Equals("Curvaturemap"))
                //{
                //    Curvaturemap node = (Curvaturemap)THelpersUI.ActiveNode;
                //    CurvaturemapSettings(ref node);
                //}
                //else if (nodeType.Name.Equals("Normalmap"))
                //{
                //    Normalmap node = (Normalmap)THelpersUI.ActiveNode;
                //    NormalmapSettings(ref node);
                //}
                //else if (nodeType.Name.Equals("Aspectmap"))
                //{
                //    Aspectmap node = (Aspectmap)THelpersUI.ActiveNode;
                //    AspectmapSettings(ref node);
                //}
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool IsModuleInCurrentGraph(TNode node)
        {
            for (int i = 0; i < activeGraph.nodes.Count; i++)
                if (node.Data.ID == activeGraph.nodes[i].Data.ID)
                    return true;

            return false;
        }

        private void GUI_ConnectionsOutputOnly(TNode module)
        {
            //List<TNode> outputNodes = activeGraph.GetOutputNodes(module.Data.ID);
            //
            //for (int j = 0; j < outputNodes.Count; j++)
            //    THelpersUI.GUI_HelpBox("OUTPUT", outputNodes[j].Data.name, 0, Color.green);
            //
            //GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            if (module is IMaskPreModules)
            {
                if (THelpersUI.Foldout("OUTPUT PREVIEW", ref module.uIToggles.Outputs))
                {
                    OutputMaskPreviewButton();
                }
            }
        }

        private void GUI_ConnectionsSingleInput(TNode module)
        {
            if (THelpersUI.Foldout("INPUTS", ref module.uIToggles.Inputs))
            {
                EditorGUI.BeginChangeCheck();

                //TODO: Optional inputs
                //if(module.inputConnections[0].required)

                input1 = THelpersUI.GUI_Popup(module.inputConnections[0].Title, input1, moduleList1.Values.ToArray(), 20);
                if (EditorGUI.EndChangeCheck())
                {
                    TNode sourceNode = moduleList1.Keys.ToList()[input1];
                    THelpersUI.ActiveNode.AddInputConnection(sourceNode, 0);
                }

                if (input1 == 0) THelpersUI.GUI_Alert();

                else if (module is IMaskPreModules)
                    OutputMaskPreviewButton();

                //if (input1 != 0 && module.inputConnections[0]._connectionDataType == ConnectionDataType.Mask)
                //InputMaskPreviewButton(moduleList1.Keys.ToList()[input1]);
            }
        }

        private void GUI_ConnectionsDoubleInput(TNode module)
        {
            if (!THelpersUI.Foldout("INPUTS", ref module.uIToggles.Inputs)) return;

            EditorGUI.BeginChangeCheck();
            input1 = THelpersUI.GUI_Popup(module.inputConnections[0].Title, input1, moduleList1.Values.ToArray(), 20);
            if (EditorGUI.EndChangeCheck())
            {
                TNode sourceNode = moduleList1.Keys.ToList()[input1];
                THelpersUI.ActiveNode.AddInputConnection(sourceNode, 0);
                //RearrangeModules();

                //if (input1 != 0 && input2 != 0 && module.GetType() == typeof(IMaskPreModules))
                //{
                //    if (module.inputConnections[0]._connectionDataType == ConnectionDataType.Mask)
                //        THelpersUI.RequestModuleMask(moduleList1.Keys.ToList()[input1], worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
                //}
            }

            if (input1 == 0) THelpersUI.GUI_Alert();

            EditorGUI.BeginChangeCheck();
            input2 = THelpersUI.GUI_Popup(module.inputConnections[1].Title, input2, moduleList2.Values.ToArray(), -10);
            if (EditorGUI.EndChangeCheck())
            {
                TNode sourceNode = moduleList2.Keys.ToList()[input2];
                THelpersUI.ActiveNode.AddInputConnection(sourceNode, 1);
                //RearrangeModules();

                //if (input1 != 0 && input2 != 0)
                //{
                //    if (module.inputConnections[1]._connectionDataType == ConnectionDataType.Mask)
                //        THelpersUI.RequestModuleMask(moduleList2.Keys.ToList()[input2], worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
                //}
            }

            if (input2 == 0) THelpersUI.GUI_Alert();

            //List<TNode> outputNodes = activeGraph.GetOutputNodes(module.Data.ID);
            //
            //for (int j = 0; j < outputNodes.Count; j++)
            //   THelpersUI.GUI_HelpBox("OUTPUT", outputNodes[j].Data.name, 0, Color.green);

            // If active node is following types then bypass mask filtering
            if (module is MaskBlendOperator || module is ApplyMask) return;

            if (module is IMaskPreModules)
                OutputMaskPreviewButton();

            //if (input1 != 0)
            //{
            //    if (module.inputConnections[0]._connectionDataType == ConnectionDataType.Mask)
            //        InputMaskPreviewButton(moduleList1.Keys.ToList()[input1]);
            //}
            //
            //if (input2 != 0)
            //{
            //    if (module.inputConnections[1]._connectionDataType == ConnectionDataType.Mask)
            //        InputMaskPreviewButton(moduleList2.Keys.ToList()[input2]);
            //}
        }

        private void OutputMaskPreviewButton()
        {
            //     // Only allow displaying preview masks when input node is from Heightmap graph
            //     if (!(THelpersUI.ActiveNode is THeightmapModules)) return;
            //
            //     GUILayout.Space(40);
            //
            //     EditorGUILayout.BeginHorizontal();
            //     GUILayout.FlexibleSpace();
            //     if (GUILayout.Button(new GUIContent("SHOW FILTERED AREA", "Display filtered area defined by sliders in UI or Scene View"), GUILayout.Width(150), GUILayout.Height(30)))
            //     {
            //         THelpersUI.ShowOutputMask();
            //
            //         if (THelpersUI.showOutputMask)
            //             THelpersUI.RequestModuleMask(THelpersUI.ActiveNode, minRangeUI, maxRangeUI);
            //     }
            //     GUILayout.FlexibleSpace();
            //     EditorGUILayout.EndHorizontal();
            //
            //     THelpersUI.DisplayOutputMask(THelpersUI.ActiveNode);
        }


        // Module Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void HeightmapMasterSettings(ref HeightmapMaster module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("Connect the last heighmap node in graph tree to this module", MessageType.Info);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module._pixelError = THelpersUI.GUI_IntSlider(new GUIContent("PIXEL ERROR", "Pixel Error value for the surface quality of terrains"), module._pixelError, 1, 200);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void ColormapMasterSettings(ref ColormapMaster module)
        {
            //if (lastNode != THelpersUI.ActiveNode)
            //{
            //    input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            //}

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("This is where all global colormap nodes will be connected and merged automatically", MessageType.Info);

            //GUI_ConnectionsSingleInput(module);
        }

        private void TerrainLayerMaster1Settings(ref TerrainLayerMaster1 module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("Connect the first Terrain Layer node to this module", MessageType.Info);
            GUI_ConnectionsSingleInput(module);
        }

        private void TerrainLayerMaster2Settings(ref TerrainLayerMaster2 module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("Connect the second Terrain Layer node to this module", MessageType.Info);
            GUI_ConnectionsSingleInput(module);
        }

        private void TerrainLayerMaster3Settings(ref TerrainLayerMaster3 module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("Connect the third Terrain Layer node to this module", MessageType.Info);
            GUI_ConnectionsSingleInput(module);
        }

        private void TerrainLayerMaster4Settings(ref TerrainLayerMaster4 module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name.ToUpper());
            THelpersUI.GUI_HelpBox("Connect the fourth Terrain Layer node to this module", MessageType.Info);
            GUI_ConnectionsSingleInput(module);
        }

        private void HeightmapSourceSettings(ref HeightmapSource module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            //EditorGUI.BeginChangeCheck();
            //module._source = (TMapManager.mapElevationSourceEnum)THelpersUI.GUI_EnumPopup(new GUIContent("SOURCE", "Select elevation source from various mapping providers"), module._source);
            //if (EditorGUI.EndChangeCheck())
            //{
            //TTerrainGenerator._elevationSource = module._source;
            //}

            //EditorGUI.BeginChangeCheck();

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module.highestResolution = THelpersUI.GUI_Toggle(new GUIContent("BEST RESOLUTION", "The highest heightmap data resolution will be fetched from source automatically"), module.highestResolution, 10);

                if (!module.highestResolution)
                {
                    module._resolution = THelpersUI.GUI_IntSlider(new GUIContent("RESOLUTION", "Heightmap data will be resampled to given resolution"), module._resolution, 32, 4096, -10);
                    module._resolution = TUtils.nearestPowerOfTwo(module._resolution);
                }   

                module.elevationExaggeration = THelpersUI.GUI_Slider(new GUIContent("ELEVATION EXAGGERATION", "Increase or Decrease surface heights in vertical direction"), module.elevationExaggeration, 0.1f, 10f);
            }

            //if (EditorGUI.EndChangeCheck()) TTerrainGenerator._zoomLevel = module._zoomLevel;

            GUI_ConnectionsOutputOnly(module);
        }

        private void HeightmapSmoothSettings(ref SmoothProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module._steps = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Smoothing operation will be applied on heightmap"), module._steps, 1, 10, 10);
                module._blending = THelpersUI.GUI_Slider(new GUIContent("BLENDING", "Blending between the new smoothed & old heightmap shape"), module._blending, 0f, 1f, -10);
                module._smoothMode = (THeightmapProcessors.Neighbourhood)THelpersUI.GUI_EnumPopup(new GUIContent("TYPE", "Type of the Smooth operation"), module._smoothMode, -10);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void VoxelTerrainSettings(ref VoxelProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module.voxelSize = THelpersUI.GUI_IntSlider(new GUIContent("VOXEL SIZE", "Size of created voxels in terrain heightmap"), module.voxelSize, 2, 50);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void HydraulicErosionMainSettings(ref HydraulicErosionMainProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);

                //ComputeShader erosionCompute = null;
                //string[] computeShaderPath = Directory.GetFiles(TAddresses.TerraWorldPath, "*.compute", SearchOption.AllDirectories);
                //
                //for (int i = 0; i < computeShaderPath.Length; i++)
                //{
                //    if (Path.GetFileNameWithoutExtension(computeShaderPath[i]).Equals("Hydraulic Erosion"))
                //    {
                //        erosionCompute = AssetDatabase.LoadAssetAtPath(computeShaderPath[0].Substring(computeShaderPath[0].LastIndexOf("Assets")), typeof(ComputeShader)) as ComputeShader;
                //        break;
                //    }
                //}
                //
                //if (erosionCompute != null) module.erosionCompute = erosionCompute;
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module.hydraulicErosionMethod = (HydraulicErosionMethod)THelpersUI.GUI_EnumPopup(new GUIContent("PROCESS METHOD", "Select process method for hydraulic errosion"), module.hydraulicErosionMethod);

                switch (module.hydraulicErosionMethod)
                {
                    case HydraulicErosionMethod.Normal:
                        module._iterations = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Hydarulic Erosion operation will be applied on heightmap"), module._iterations, 0, 500);
                        module._rainAmount = THelpersUI.GUI_Slider(new GUIContent("RAIN AMOUNT", "Rain amount for the Hydraulic Erosion simulation"), module._rainAmount, 0f, 1f);
                        module._sediment = THelpersUI.GUI_Slider(new GUIContent("SEDIMENT", "Sediment strength for the Hydraulic Erosion simulation"), module._sediment, 0f, 1f);
                        module._evaporation = THelpersUI.GUI_Slider(new GUIContent("EVAPORATION", "Evaporation strength for the Hydraulic Erosion simulation"), module._evaporation, 0.5f, 1f);
                        break;
                    case HydraulicErosionMethod.Ultimate:
                        module._iterationsUltimate = THelpersUI.GUI_IntField(new GUIContent("ITERATIONS", "Number of iterations Hydraulic Erosion operation will be applied on heightmap"), module._iterationsUltimate);
                        break;
                    default:
                        break;
                }
            }

            GUI_ConnectionsSingleInput(module);
        }

        //private void HydraulicErosionSettings (ref HydraulicErosionProcess module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    module._iterations = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Hydarulic Erosion operation will be applied on heightmap"), module._iterations, 0, 500);
        //    module._rainAmount = THelpersUI.GUI_Slider(new GUIContent("RAIN AMOUNT", "Rain amount for the Hydraulic Erosion simulation"), module._rainAmount, 0f, 1f);
        //    module._sediment = THelpersUI.GUI_Slider(new GUIContent("SEDIMENT", "Sediment strength for the Hydraulic Erosion simulation"), module._sediment, 0f, 1f);
        //    module._evaporation = THelpersUI.GUI_Slider(new GUIContent("EVAPORATION", "Evaporation strength for the Hydraulic Erosion simulation"), module._evaporation, 0.5f, 1f);
        //    GUI_ConnectionsSingleInput(module);
        //}

        private void WaterErosionSettings(ref WaterErosionProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module._iterations = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Water Erosion operation will be applied on heightmap"), module._iterations, 1, 500);
                module._shape = THelpersUI.GUI_Slider(new GUIContent("SHAPE", "Shape for the Water Erosion simulation"), module._shape, 0.4f, 1f);
                module._rivers = THelpersUI.GUI_Slider(new GUIContent("RIVERS", "Rivers for the Water Erosion simulation"), module._rivers, 0f, 1f);
                module._vertical = THelpersUI.GUI_Slider(new GUIContent("DEPTH", "Depth for the Water Erosion simulation"), module._vertical, 0f, 10f);
                module._seaBedCarve = THelpersUI.GUI_Slider(new GUIContent("SEA BED", "Sea-bed carving amount for the Water Erosion simulation"), module._seaBedCarve, 0f, 1f);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void ThermalErosionSettings(ref ThermalErosionProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
                module._iterations = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Thermal Erosion operation will be applied on heightmap"), module._iterations, 1, 10);
            
            GUI_ConnectionsSingleInput(module);
        }

        private void TerraceSettings(ref TerraceProcess module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                GUILayout.Space(10);

                int terraceUISize = 256;

                lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x = (position.width / 2f) - (terraceUISize / 2f) - 20;
                lastRect.y += 60;
                lastRect.width = terraceUISize;
                lastRect.height = terraceUISize;

                GUI.DrawTexture(lastRect, TResourcesManager.mountainsIcon);

                EditorGUI.BeginChangeCheck();


                module._terraceCount = THelpersUI.GUI_IntSlider(new GUIContent("STEPS #", "Number of Terrace steps on the surface"), module._terraceCount, 2, 40);
                //module._terraceCount = THelpersUI.GUI_IntSlider(new GUIContent("STEPS #", "Number of Terrace steps on the surface"), module._terraceCount, 4, 40);


                if (EditorGUI.EndChangeCheck())
                {
                    verticalSlidersSpace = Mathf.Clamp((position.width / module._terraceCount) - module._terraceCount, 0f, 10f);
                    Array.Resize(ref module.controlPoints, module._terraceCount);
                }

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int i = 0; i < module._terraceCount; i++)
                {
                    GUILayout.BeginVertical();
                    module.controlPoints[i] = GUILayout.VerticalSlider(module.controlPoints[i], 1.0f, 0.0f, GUILayout.Height(terraceUISize));
                    GUILayout.EndVertical();

                    if (module.controlPoints[i] > terracePointMax)
                        module.controlPoints[i] = terracePointMax;

                    else if (module.controlPoints[i] < terracePointMin)
                        module.controlPoints[i] = terracePointMin;

                    GUILayout.Space(verticalSlidersSpace);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("VERTICAL RANGE", MessageType.None);
                EditorGUILayout.MinMaxSlider(ref terracePointMin, ref terracePointMax, 0.0f, 1.0f);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                float boxHeight = lastRect.height * (terracePointMax - terracePointMin);
                float boxOffsetY = lastRect.y + ((1 - terracePointMax) * lastRect.height); // Opposite Way: float boxOffsetY = rect.y + (rect.height + (terracePointMin * rect.height)) - rect.height;

                if (EditorGUIUtility.isProSkin)
                    GUI.backgroundColor = new Color(0, 0.9f, 0, 1f);
                else
                    GUI.backgroundColor = new Color(0, 0.9f, 0, 0.2f);

                GUI.Box(new Rect(lastRect.x, boxOffsetY, lastRect.width, boxHeight), "");
                GUI.backgroundColor = Color.white;

                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("AUTO SET", "Automatically set terrace points"), style))
                {
                    module.controlPoints = THeightmapProcessors.AutoSetTerraces(module._terraceCount, module._terraceVariation, terracePointMin, terracePointMax);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                module._strength = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Strength of the Terrace effect on surface"), module._strength, 0f, 1f, 20);
                module._terraceVariation = THelpersUI.GUI_Slider(new GUIContent("VARIATION", "Position variation of the Terrace steps in vertical axis"), module._terraceVariation, 0f, 1f);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void ApplyMaskSettings(ref ApplyMask module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                input2 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList2, module, 1);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module._depth = THelpersUI.GUI_Slider(new GUIContent("DEPTH (M)", "Depth in meters to dig through the surface"), module._depth, 0f, 100f);
                module._flat = THelpersUI.GUI_Toggle(new GUIContent("FLLATTEN FLOOR", "Flatten the floor of the deformed areas"), module._flat, 20);
            }
            GUI_ConnectionsDoubleInput(module);
        }

        // Colormap Modules
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void SatelliteImageSettings(ref SatelliteImage module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module._source = (TMapManager.ImageryMapServer)THelpersUI.GUI_EnumPopup(new GUIContent("SOURCE", "Select Imagery source from various mapping providers"), module._source);
                module.resolution = THelpersUI.GUI_IntSlider(new GUIContent("RESOLUTION", "Select satellite image resolution for the entire world"), module.resolution, 32, 4096);
                module.resolution = TUtils.nearestPowerOfTwo(module.resolution);
            }
            GUI_ConnectionsOutputOnly(module);
        }

        //private void ShadowRemoverSettings(ref ShadowRemover module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //
        //    tempColor = TUtils.CastToUnityColor(System.Drawing.Color.FromArgb(255, module._shadowColorR, module._shadowColorG, module._shadowColorB));
        //    tempColor = THelpersUI.GUI_ColorField(new GUIContent("SHADOW COLOR", "Shadow color in the image pixels"), tempColor);
        //    module._shadowColorR = (int)(tempColor.r * 255);
        //    module._shadowColorG = (int)(tempColor.g * 255);
        //    module._shadowColorB = (int)(tempColor.b * 255);
        //
        //    module._blockSize = THelpersUI.GUI_IntField(new GUIContent("BLOCK SIZE", "Grid number of adjacent pixels to perform shadow removal"), module._blockSize);
        //    GUI_ConnectionsSingleInput(module);
        //}

        //private void SlopemapSettings(ref Slopemap module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.5f, 2f);
        //    //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.5f, 2f);
        //
        //    //EditorGUI.BeginChangeCheck();
        //    //module._strength = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Strength of the slopemap operation"), module._strength, 0.01f, 2f);
        //    //if (EditorGUI.EndChangeCheck())
        //    //    THelpersUI.RequestModuleMask(THelpersUI.ActiveNode, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
        //
        //    //EditorGUI.BeginChangeCheck();
        //    GUI_ConnectionsSingleInput(module);
        //    //if (EditorGUI.EndChangeCheck())
        //    //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, worldGraph, minRangeUI, maxRangeUI);
        //}

        private void FlowmapSettings(ref Flowmap module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                minRangeUI = module.minRange;
                maxRangeUI = module.maxRange;
                //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.5f, 2f);
                //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.5f, 2f);
                module._iterations = THelpersUI.GUI_IntSlider(new GUIContent("ITERATIONS", "Number of iterations Flowmap operation will be applied on heightmap"), module._iterations, 1, 20);

                GUILayout.Space(10);

                EditorGUI.BeginChangeCheck();
                THelpersUI.GUI_MinMaxSlider(new GUIContent("FLOW RANGE", "Filter flow rate in normalized scope."), ref minRangeUI, ref maxRangeUI, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) if (module is THeightmapModules) THelpersUI.UpdateFilteredMask(minRangeUI, maxRangeUI);

                //Only allow displaying preview masks when input node is from Heightmap graph
                //if (!(module is THeightmapModules)) return;

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("SHOW FILTERED AREA", "Display filtered area defined by sliders in UI or Scene View"), GUILayout.Width(150), GUILayout.Height(50)))
                {
                    THelpersUI.ShowPreviewMask();

                    if (THelpersUI.showPreviewMask)
                        THelpersUI.RequestModuleMask(module, minRangeUI, maxRangeUI);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //THelpersUI.DisplayPreviewMask(module, ref minRangeUI, ref maxRangeUI);

                module.minRange = minRangeUI;
                module.maxRange = maxRangeUI;
            }

            //EditorGUI.BeginChangeCheck();
            GUI_ConnectionsSingleInput(module);
            //if (EditorGUI.EndChangeCheck())
            //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, worldGraph, minRangeUI, maxRangeUI);
        }

        //private void CurvaturemapSettings(ref Curvaturemap module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.5f, 2f);
        //    //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.5f, 2f);
        //    module._limit = THelpersUI.GUI_FloatField(new GUIContent("LIMIT", "Limit value for the process"), module._limit);
        //    GUI_ConnectionsSingleInput(module);
        //}

        //private void NormalmapSettings(ref Normalmap module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.5f, 2f);
        //    //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.5f, 2f);
        //    module._strength = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "Strength of the Normalmap operation"), module._strength, 0.01f, 2f);
        //    GUI_ConnectionsSingleInput(module);
        //}

        //private void AspectmapSettings(ref Aspectmap module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {tolerance
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.5f, 2f);
        //    //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.5f, 2f);
        //    module._aspectType = (THeightmapProcessors.ASPECT_TYPE)THelpersUI.GUI_EnumPopup(new GUIContent("TYPE", "Type of the Aspect orientation while creating map"), module._aspectType);
        //    GUI_ConnectionsSingleInput(module);
        //}

        private void Image2MaskSettings(ref Image2Mask module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
                //THelpersUI.SwitchBoundingBox();
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("COLOR SELECTION", ref module.uIToggles.Settings1))
            {
                tempColor = TUtils.CastToUnityColor(System.Drawing.Color.FromArgb(255, module._selectedColorR, module._selectedColorG, module._selectedColorB));
                tempColor = THelpersUI.GUI_ColorField(new GUIContent("SELECTED COLOR", "A mask will be generated for the system using the selected color"), tempColor);
                module._selectedColorR = (int)(tempColor.r * 255);
                module._selectedColorG = (int)(tempColor.g * 255);
                module._selectedColorB = (int)(tempColor.b * 255);
                module.tolerance = THelpersUI.GUI_IntSlider(new GUIContent("TOLERANCE %", "Tolerance range for the selected color in color space"), module.tolerance, 1, 100);
            }

            if (THelpersUI.Foldout("IMAGE SOURCE", ref module.uIToggles.Settings2))
            {
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedHeight = 20;
                module.SelectionImage2MaskMethodIndex = THelpersUI.GUI_SelectionGrid(module.SelectionImage2MaskMethodIndex, _image2MaskTitles, style);

                if (module.SelectionImage2MaskMethodIndex == 0)
                    GUI_ConnectionsSingleInput(module);
                else if (module.SelectionImage2MaskMethodIndex == 1)
                {
                    EditorGUI.BeginChangeCheck();
                    diffuseTexture = (Texture2D)THelpersUI.GUI_ObjectField(new GUIContent("EXTERNAL IMAGE", "Use external source image instead of the input node's image"), diffuseTexture, typeof(Texture2D), null, 20);
                    if (EditorGUI.EndChangeCheck()) module.diffusemapPath = AssetDatabase.GetAssetPath(diffuseTexture);

                    if (diffuseTexture == null) THelpersUI.GUI_Alert();
                }
            }
        }

        private void SlopemaskSettings(ref Slopemask module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                //if (input1 != 0) THelpersUI.RequestModuleMask(module, worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
                //THelpersUI.SwitchBoundingBox();
                //objectScale = TUtils.CastToUnity(module.scaleMultiplier);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                objectScale = TUtils.CastToUnity(module.scaleMultiplier);
                objectScale = THelpersUI.GUI_Vector3Field(new GUIContent("SCALE MULTIPLIER", "The multiplier value for the original object scale"), objectScale, 20);
                module.scaleMultiplier = TUtils.CastToNumerics(objectScale);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref module.MinSlope, ref module.MaxSlope, 0f, 90f);
                if (EditorGUI.EndChangeCheck()) if (input1 != 0) THelpersUI.LoadTextureFromMask();
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void LoadDataFromDetailTextureModule(Mask2DetailTexture module)
        {
            if (!string.IsNullOrEmpty(module.terrainLayerPath) && File.Exists(module.terrainLayerPath))
                terrainLayer = AssetDatabase.LoadAssetAtPath(module.terrainLayerPath, typeof(TerrainLayer)) as TerrainLayer;
            else
                terrainLayer = null;

            if (!string.IsNullOrEmpty(module.diffusemapPath) && File.Exists(module.diffusemapPath))
                diffuseTexture = AssetDatabase.LoadAssetAtPath(module.diffusemapPath, typeof(Texture2D)) as Texture2D;
            else
                diffuseTexture = null;

            if (!string.IsNullOrEmpty(module.normalmapPath) && File.Exists(module.normalmapPath))
                normalmapTexture = AssetDatabase.LoadAssetAtPath(module.normalmapPath, typeof(Texture2D)) as Texture2D;
            else
                normalmapTexture = null;

            if (!string.IsNullOrEmpty(module.maskmapPath) && File.Exists(module.maskmapPath))
                maskmapTexture = AssetDatabase.LoadAssetAtPath(module.maskmapPath, typeof(Texture2D)) as Texture2D;
            else
                maskmapTexture = null;

            minRangeUI = module.minRange;
            maxRangeUI = module.maxRange;
        }

        private void Mask2DetailTextureSettings(ref Mask2DetailTexture module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                string resourcePath = module.terrainLayerPath;
                CheckForEmptyResource(ref resourcePath);
                module.terrainLayerPath = resourcePath;

                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                LoadDataFromDetailTextureModule(module);
                //if (input1 != 0) THelpersUI.RequestModuleMask(moduleList1.Keys.ToList()[input1], worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("LAYER SOURCE", ref module.uIToggles.Settings1))
            {
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 70; style.fixedHeight = 20;
                module.SelectionMethodIndex = THelpersUI.GUI_SelectionGrid(module.SelectionMethodIndex, _terrainLayerTitles, style);
                if (module.SelectionMethodIndex == 0)
                {
                    module.isColorMap = false;
                    EditorGUI.BeginChangeCheck();
                    terrainLayer = (TerrainLayer)THelpersUI.GUI_ObjectField(new GUIContent("TERRAIN LAYER", "Select Terrain Layer for rendering"), terrainLayer, typeof(TerrainLayer), null, 20);
                    if (EditorGUI.EndChangeCheck())
                    {
                        module.terrainLayerPath = AssetDatabase.GetAssetPath(terrainLayer);
                        //if (input1 != 0 && THelpersUI.nodeMask != null) THelpersUI.UpdateFilteredMask(module.minRange, module.maxRange);
                    }

                    if (terrainLayer == null) THelpersUI.GUI_Alert();
                }
                else if (module.SelectionMethodIndex == 1)
                {
                    module.isColorMap = false;

                    EditorGUI.BeginChangeCheck();
                    diffuseTexture = (Texture2D)THelpersUI.GUI_ObjectField(new GUIContent("DIFFUSE MAP", "Diffuse texture for the layer"), diffuseTexture, typeof(Texture2D), null, 20);
                    if (EditorGUI.EndChangeCheck())
                    {
                        module.diffusemapPath = AssetDatabase.GetAssetPath(diffuseTexture);
                    }

                    if (diffuseTexture == null) THelpersUI.GUI_Alert();

                    EditorGUI.BeginChangeCheck();
                    normalmapTexture = (Texture2D)THelpersUI.GUI_ObjectField(new GUIContent("NORMAL MAP", "Normalmap texture for the layer"), normalmapTexture, typeof(Texture2D));
                    if (EditorGUI.EndChangeCheck()) module.normalmapPath = AssetDatabase.GetAssetPath(normalmapTexture);

                    if (normalmapTexture != null)
                        module.normalScale = THelpersUI.GUI_Slider(new GUIContent("NORMAL STRENGTH", "Normal map strength on the surface"), module.normalScale, 0, 4);

                    EditorGUI.BeginChangeCheck();
                    maskmapTexture = (Texture2D)THelpersUI.GUI_ObjectField(new GUIContent("MASK MAP", "Mask map texture for the layer"), maskmapTexture, typeof(Texture2D));
                    if (EditorGUI.EndChangeCheck()) module.maskmapPath = AssetDatabase.GetAssetPath(maskmapTexture);

                    tempVector2D = TUtils.CastToUnity(module.tiling);
                    tempVector2D = THelpersUI.GUI_Vector2Field(new GUIContent("TILING", "Tiling for the inserted texture on material"), tempVector2D, 20);
                    module.tiling = TUtils.CastToNumerics(tempVector2D);

                    tempVector2D = TUtils.CastToUnity(module.tilingOffset);
                    tempVector2D = THelpersUI.GUI_Vector2Field(new GUIContent("OFFSET", "Tiling Offset for the inserted texture on material"), tempVector2D);
                    module.tilingOffset = TUtils.CastToNumerics(tempVector2D);

                    tempColor = TUtils.CastToUnityColor(module.specular);
                    tempColor = THelpersUI.GUI_ColorField(new GUIContent("SPECULAR", "Specular color on the surface"), tempColor, 20);
                    module.specular = TUtils.CastToDrawingVector(tempColor);

                    module.metallic = THelpersUI.GUI_Slider(new GUIContent("METALLIC", "Metallic strength on the surface"), module.metallic, 0, 1);
                    module.smoothness = THelpersUI.GUI_Slider(new GUIContent("SMOOTHNESS", "Smoothness strength on the surface"), module.smoothness, 0, 1);
                }

            }
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings2))
            {
                module.opacity = THelpersUI.GUI_Slider(new GUIContent("OPACITY", "Opacity of the terrain layer"), module.opacity, 0f, 1f);
            }

            GUI_ConnectionsSingleInput(module);
        }

        private void Mask2ColorMapSettings(ref Mask2ColorMap module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                input2 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList2, module, 1);
                //LoadDataFromDetailTextureModule(module);
                //if (input1 != 0) THelpersUI.RequestModuleMask(moduleList1.Keys.ToList()[input1], worldArea, TTerraWorld.WorldGraph, minRangeUI, maxRangeUI);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);

            if (THelpersUI.Foldout("PROCESS METHOD", ref module.uIToggles.Settings1))
            {
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedHeight = 20;
                module.SelectionColormapMethodIndex = THelpersUI.GUI_SelectionGrid(module.SelectionColormapMethodIndex, _colormapTitles, style);
                if (module.SelectionColormapMethodIndex == 0)
                {
                    THelpersUI.GUI_HelpBox("SELECT AREA FROM INPUTS SECTION");
                    THelpersUI.GUI_HelpBox("SELECT SOURCE FROM INPUTS SECTION");
                }

                if (module.SelectionColormapMethodIndex == 1)
                {
                    module.mostUsedColor = THelpersUI.GUI_IntSlider(new GUIContent("COLORS COUNT", "Flatten image colors to the most used colors based on specified colors count"), module.mostUsedColor, 1, 256);
                    THelpersUI.GUI_HelpBox("SELECT AREA FROM INPUTS SECTION");
                    THelpersUI.GUI_HelpBox("SELECT SOURCE FROM INPUTS SECTION");
                }

                if (module.SelectionColormapMethodIndex == 2)
                {
                    tempColor = TUtils.CastToUnityColor(System.Drawing.Color.FromArgb(255, module._colorMapColorR, module._colorMapColorG, module._colorMapColorB));
                    tempColor = THelpersUI.GUI_ColorField(new GUIContent("FILL COLOR", "Fill masked areas with this color applied on colormap"), tempColor);
                    module._colorMapColorR = (int)(tempColor.r * 255);
                    module._colorMapColorG = (int)(tempColor.g * 255);
                    module._colorMapColorB = (int)(tempColor.b * 255);
                    THelpersUI.GUI_HelpBox("SELECT AREA FROM INPUTS SECTION");

                    //Debug.Log(module._colorMapColorR);
                }
            }
            //  GUI_ConnectionsSingleInput(module);
            GUI_ConnectionsDoubleInput(module);
        }


        // Biomes Modules
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void WaterGeneratorSettings(ref WaterGenerator module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                string resourcePath = module.Materialpath;
                CheckForEmptyResource(ref resourcePath);
                module.Materialpath = resourcePath;

                if (!string.IsNullOrEmpty(module.Materialpath) && File.Exists(module.Materialpath))
                    material = AssetDatabase.LoadAssetAtPath(module.Materialpath, typeof(Material)) as Material;
                else
                    material = null;

                //showBounds = false;
                //THelpersUI.SwitchBoundingBox();

                THelpersUI.GetMaskValue(module.unityLayerName);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("LAKES/RIVERS SETTING", ref module.uIToggles.Settings1))
            {
                module.GenerateOceans = THelpersUI.GUI_Toggle(new GUIContent("GENERATE OCEANS", "Generate oceans in the world"), module.GenerateOceans, 20);

                module.GenerateLakes = THelpersUI.GUI_Toggle(new GUIContent("GENERATE LAKES", "Generate lakes in the world"), module.GenerateLakes, 20);
                if (module.GenerateLakes) module.LakeMinSizeInM2 = THelpersUI.GUI_FloatField(new GUIContent("MIN. AREA (M2)", "Bypass generation of lakes with specified minimum area size in square meters (M2)"), module.LakeMinSizeInM2, -10);

                module.GenerateRiver = THelpersUI.GUI_Toggle(new GUIContent("GENERATE RIVERS", "Generate rivers in the world"), module.GenerateRiver, 20);
                if (module.GenerateRiver)
                {
                    module.RiverWidthInMeter = THelpersUI.GUI_FloatField(new GUIContent("RIVER WIDTH (M)", "Set default width for rivers in meters"), module.RiverWidthInMeter, -10);
                    module.Depth = THelpersUI.GUI_FloatField(new GUIContent("RIVER DEPTH (m)", "Set default dept for rivers/lakes in meters"), module.Depth, -10);
                }

                EditorGUI.BeginChangeCheck();
                material = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for this object"), material, typeof(Material), null, 10);
                if (EditorGUI.EndChangeCheck()) module.Materialpath = AssetDatabase.GetAssetPath(material);
                if (material == null) THelpersUI.GUI_Alert();

                module.lodCulling = THelpersUI.GUI_Slider(new GUIContent("LOD CULLING", "Minimum object's screen size percentage where it culls from rendering"), module.lodCulling, 0f, 100f);
                module.smoothOperation = THelpersUI.GUI_Toggle(new GUIContent("SMOOTHNESS OPERATION", "Performs smoothing on surface when terrain is deformed"), module.smoothOperation);
                module.deformAngle = THelpersUI.GUI_Slider(new GUIContent("DEFORMATION ANGLE", "Terrain deformation in degress from the edges into the center of the water area"), module.deformAngle, 1f, 89f);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings2))
            {
                tempVector3D = TUtils.CastToUnity(module.positionOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("OFFSET", "Placement offset of the object in 3 dimensions"), tempVector3D, 20);
                module.positionOffset = TUtils.CastToNumerics(tempVector3D);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings3))
            {
                //module.priority = THelpersUI.GUI_IntField(new GUIContent("PRIORITY", "Priority value of the current object's layer"), module.priority);
                module.layerName = "Water";
                module.layerName = THelpersUI.GUI_TextField(new GUIContent("LAYER NAME", "Layer name of the current object's layer"), module.layerName);
                //module.unityLayerName = THelpersUI.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));

                THelpersUI.GUI_HelpBox("Water layer objects will always have their layer set to \"Water\"", MessageType.Info, 20);
                GUILayout.Space(20);

                //maskColorUnity = THelpersUI.GUI_ColorField("MASK COLOR", maskColorUnity);
                //module.maskColor = System.Drawing.Color.FromArgb((int)maskColorUnity.r, (int)maskColorUnity.g, (int)maskColorUnity.b);

                //module.colorDamping = THelpersUI.GUI_IntSlider(new GUIContent("COLOR DAMPING", "The tolerance & damping of the selected color range"), module.colorDamping, 0, 255);

                //module.lakeAroundPointsDensity = THelpersUI.GUI_IntField(new GUIContent("LAKE AROUND DENSITY", "Grid number of adjacent pixels to perform shadow removal"), module.lakeAroundPointsDensity);
                //module.lakeAroundVariation = THelpersUI.GUI_FloatField(new GUIContent("AROUND VARIATION", "Placement variation for shore objects"), module.lakeAroundVariation);
            }

            GUI_ConnectionsOutputOnly(module);
        }

        private void BiomeExtractorSettings(ref BiomeExtractor module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                //THelpersUI.SwitchBoundingBox();
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                module.biomeType = (BiomeTypes)THelpersUI.GUI_EnumPopup(new GUIContent("BIOME TYPE", "Select biome type to filter area"), module.biomeType);
                module.bordersOnly = THelpersUI.GUI_Toggle(new GUIContent("BORDERS ONLY", "Extract only edges of the detected masked area"), module.bordersOnly, 10);
                if (module.biomeType == BiomeTypes.River || module.biomeType == BiomeTypes.Waters) module.riverWidth = THelpersUI.GUI_FloatField(new GUIContent("RIVERS WIDTH (M)", "Default rivers width in meters (M)"), module.riverWidth);
                if (module.biomeType != BiomeTypes.River) module.MinSize = THelpersUI.GUI_FloatField(new GUIContent("MIN. AREA (M2)", "Ignore areas with less than specified Min. size in square meters (M2)"), module.MinSize, 10);
                if (module.bordersOnly) module.edgeSize = THelpersUI.GUI_IntSlider(new GUIContent("EDGE SIZE (m)", "Area damping along the borders in meters"), module.edgeSize, 1, 1000);
                module.scaleFactor = THelpersUI.GUI_FloatField(new GUIContent("SCALE", "Scale of the whole shape of mask"), module.scaleFactor, 0.01f, 40f, 10);
                //module.FixWithImage = THelpersUI.GUI_Toggle(new GUIContent("EXTRACT FROM RENDERING, "Extract biome features by processing rendered landcover images"), module.FixWithImage, 20);
            }

            GUI_ConnectionsOutputOnly(module);
        }

        //private void LakeShoreMaskSettings(ref LakeShoreMask module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        //THelpersUI.SwitchBoundingBox();
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //
        //    module.bordersOnly = THelpersUI.GUI_Toggle(new GUIContent("BORDERS ONLY", "Is it the edges only or the whole zone"), module.bordersOnly);
        //    module.edgeSize = THelpersUI.GUI_IntField(new GUIContent("EDGE SIZE", "Size of the attacking in the edges"), module.edgeSize);
        //    module.scaleFactor = THelpersUI.GUI_FloatField(new GUIContent("SCALE", "Scale of the mask shape"), module.scaleFactor);
        //
        //    GUI_ConnectionsSingleInput(module);
        //}

        private void ObjectScatterSettings(ref ObjectScatter module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                for (int i = 0; i < module.prefabNames.Count; i++)
                {
                    string resourcePath = module.prefabNames[i];
                    CheckForEmptyResource(ref resourcePath);
                    module.prefabNames[i] = resourcePath;
                }

                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                //THelpersUI.SwitchBoundingBox();
                //THelpersUI.InitDynamicObjectsList(module.prefabNames);
                THelpersUI.GetMaskValue(module.unityLayerName);
                objectScale = TUtils.CastToUnity(module.scaleMultiplier);
                minRangeUI = module.minRange;
                maxRangeUI = module.maxRange;

                ////TODO: Temporary solution so that each node have its own prefab reference
                //tempObjects = new List<GameObject>();
                //int graphNodesCount = TTerraWorld.WorldGraph.biomesGraph.nodes.Count;
                //
                //for (int i = 0; i < graphNodesCount; i++)
                //{
                //    for (int j = 0; j < module.prefabNames.Count; j++)
                //    {
                //        GameObject tempObj = AssetDatabase.LoadAssetAtPath(module.prefabNames[j], typeof(GameObject)) as GameObject;
                //        tempObjects.Add(tempObj);
                //        if (tempObj == null) module.prefabNames[j] = "";
                //    }
                //}
                //
                //GetPrefabLODs(ref module);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;

            if (THelpersUI.Foldout("OBJECT SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                THelpersUI.GUI_ObjectFieldDynamicList(ref module.prefabNames, ref module.bounds, ref module.objectScales, typeof(GameObject));
                if (module.prefabNames != null && module.prefabNames.Count > 0 && !string.IsNullOrEmpty(module.prefabNames[0])) module.Data.name = Path.GetFileNameWithoutExtension(module.prefabNames[0]);

                //if (EditorGUI.EndChangeCheck()) GetPrefabLODs(ref module);

#if TERRAWORLD_PRO
                try
                {
                    for (int i = 0; i < incompatibleMaterialNames.Count; i++)
                        if (!string.IsNullOrEmpty(incompatibleMaterialNames[i]) && !string.IsNullOrEmpty(module.prefabNames[i])) THelpersUI.GUI_HelpBox(incompatibleMaterialWarningText + incompatibleMaterialNames[i], MessageType.Warning, 10);
                }
#if TERRAWORLD_DEBUG
                catch (Exception e)
                {
                    throw e;
                }
#else
                catch { }
#endif
#endif

            if (module.prefabNames == null || module.prefabNames.Count == 0 || string.IsNullOrEmpty(module.prefabNames[0])) THelpersUI.GUI_Alert();
            }

            if (THelpersUI.Foldout("SPECIAL SETTINGS", ref module.uIToggles.Settings3))
            {
                THelpersUI.GUI_HelpBox("PLACEMENT ON WATER", MessageType.None, 20);

                int state = 0;

                if (module.bypassLakes)
                    state = 0;
                else if (module.underLakes)
                    state = 1;
                else if (module.onLakes)
                    state = 2;

                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, waterPlacementMode, style, -10);

                if (EditorGUI.EndChangeCheck())
                {
                    if (state == 0)
                    {
                        module.bypassLakes = true;
                        module.underLakes = false;
                        module.underLakesMask = false;
                        module.onLakes = false;
                    }
                    else if (state == 1)
                    {
                        module.bypassLakes = false;
                        module.underLakes = true;
                        module.underLakesMask = true;
                        module.onLakes = false;
                    }
                    else if (state == 2)
                    {
                        module.bypassLakes = false;
                        module.underLakes = false;
                        module.underLakesMask = true;
                        module.onLakes = true;
                    }
                }

                //module.underLakes = THelpersUI.GUI_Toggle(new GUIContent("ALLOW UNDER WATER", "Is it allowed to place the model under water surface?"), module.underLakes);

                //module.hasCollider = THelpersUI.GUI_Toggle(new GUIContent("COLLIDER", "Does object contribute to collision detection!"), module.hasCollider, 20);
                //
                //if(module.hasCollider)
                //    module.hasPhysics = THelpersUI.GUI_Toggle(new GUIContent("PHYSICS", "Enable or disable Rigidbody component on the object to perform physics calculations"), module.hasPhysics);
                //else
                //    THelpersUI.GUI_HelpBox("DISABLING COLLIDER ON OBJECT WILL BYPASS PLACEMENT CHECK THROUGH RAYCASTS AND LAYERMASKS!", MessageType.Warning, -10);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings2))
            {
                module.seedNo = THelpersUI.GUI_IntField(new GUIContent("SEED#", "Seed number for unique procedural values"), module.seedNo);

                //module.density = THelpersUI.GUI_IntField(new GUIContent("DENSITY", "Placement density for the object"), module.density);
                //module.densityResolutionPerKilometer = THelpersUI.GUI_IntSlider(new GUIContent("KM2 RESOLUTION", "Density resolution per square kilometer for normalized placement"), Mathf.ClosestPowerOfTwo(module.densityResolutionPerKilometer), 8, 256);
                //module.densityResolutionPerKilometer = THelpersUI.GUI_IntSlider(new GUIContent("KM2 DENSITY", "Instance count per square kilometer for placement"), module.densityResolutionPerKilometer, 1, 2000);
                //TODO: This one is private and internal which will be picked randomly in runtime
                //module.rotations90 = (Rotate90Deg)THelpersUI.GUI_EnumPopup(new GUIContent("90° ROTATION", "Rotation is only performed in 90 degree multiplies - 0°, 90°, 180°, 270°"), module.rotations90);

                module.placeSingleItem = THelpersUI.GUI_Toggle(new GUIContent("PLACE SINGLE ITEM", "This node will only place one instance of inserted prefab suited for special cases like a Runtime Spawner instance"), module.placeSingleItem);

                EditorGUI.BeginDisabledGroup(module.placeSingleItem);
                module.averageDistance = THelpersUI.GUI_FloatField(new GUIContent("PLACEMENT DISTANCE (M)", "Placement distance in meters between instances"), module.averageDistance, 0.1f, float.MaxValue, -10);
                EditorGUI.EndDisabledGroup();

                module.getSurfaceAngle = THelpersUI.GUI_Toggle(new GUIContent("SURFACE ROTATION", "Rotation is calculated from the underlying surface angle"), module.getSurfaceAngle, 30);
                module.rotation90Degrees = THelpersUI.GUI_Toggle(new GUIContent("90° ROTATION", "Rotation is only performed in 90 degree multiplies 0°, 90°, 180° & 270°"), module.rotation90Degrees);
                if (!module.getSurfaceAngle) module.lockYRotation = THelpersUI.GUI_Toggle(new GUIContent("HORIZONTAL ROTATION", "Rotation is locked in horizontal rotation axis (Y) only suited for objects like trees"), module.lockYRotation);
                if (!module.rotation90Degrees) THelpersUI.GUI_MinMaxSlider(new GUIContent("ROTATION RANGE", "Minimum & Maximum rotation in degrees for object"), ref module.minRotationRange, ref module.maxRotationRange, 0f, 359f);

                module.positionVariation = THelpersUI.GUI_Slider(new GUIContent("RANDOM POSITION", "Percentage of the random position for instance"), module.positionVariation, 0, 100, 20);

                objectScale = THelpersUI.GUI_Vector3Field(new GUIContent("SCALE MULTIPLIER", "The multiplier value for the original object scale"), objectScale, 20);
                module.scaleMultiplier = TUtils.CastToNumerics(objectScale);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SCALE RANGE", "Minimum & Maximum random scale variation"), ref module.minScale, ref module.maxScale, 0.1f, 20f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref module.minSlope, ref module.maxSlope, 0f, 90f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref module.minElevation, ref module.maxElevation, -100000f, 100000f);

                //THelpersUI.GUI_HelpBox("POSITION OFFSET", MessageType.None, 10);
                //
                //EditorGUI.BeginChangeCheck();
                //style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 70; style.fixedHeight = 20;
                //selectionIndexOffset = THelpersUI.GUI_SelectionGrid(selectionIndexOffset, worldLocalSelection, style, -10);
                //if (EditorGUI.EndChangeCheck())
                //{
                //    if (selectionIndexOffset == 0)
                //        module.isWorldOffset = true;
                //    else
                //        module.isWorldOffset = false;
                //}

                tempVector3D = TUtils.CastToUnity(module.positionOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("POSITION OFFSET", "Position offset of the object in 3 dimensions"), tempVector3D);
                module.positionOffset = TUtils.CastToNumerics(tempVector3D);

                tempVector3D = TUtils.CastToUnity(module.rotationOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("ROTATION OFFSET", "Rotation offset of the object in 3 dimensions"), tempVector3D);
                module.rotationOffset = TUtils.CastToNumerics(tempVector3D);

                module.checkBoundingBox = THelpersUI.GUI_Toggle(new GUIContent("CHECK BOUNDING BOX", "Check model's bounding box for placement?\nIf checked, placement will only be applied when the whole model is inside masked areas\nIf unchecked, mask is only checked in pivot position of the model"), module.checkBoundingBox, 30);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings4))
            {
                //TODO: These values are per Layer not per Object
                // module.priority = THelpersUI.GUI_IntField(new GUIContent("PRIORITY", "Priority value of the current object's layer"), module.priority, int.MinValue, int.MaxValue, 20);
                module.layerName = THelpersUI.GUI_TextField(new GUIContent("LAYER NAME", "Parent Layer name of the current object's layer in hierarchy view"), module.layerName);
                module.unityLayerName = THelpersUI.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
                module.maskLayer = THelpersUI.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), module.maskLayer);
            }

            module.minRange = minRangeUI;
            module.maxRange = maxRangeUI;

            GUI_ConnectionsSingleInput(module);
        }

        private void TreeScatterSettings(ref TreeScatter module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                string resourcePath = module.prefabName;
                CheckForEmptyResource(ref resourcePath);
                module.prefabName = resourcePath;

                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                //THelpersUI.SwitchBoundingBox();
                //THelpersUI.GetMaskValue(module.unityLayerName);
                objectScale = TUtils.CastToUnity(module.scaleMultiplier);
                minRangeUI = module.minRange;
                maxRangeUI = module.maxRange;

                //TODO: Temporary solution so that each node have its own prefab reference
                tempObjects = new List<GameObject>();
                int graphNodesCount = TTerraWorld.WorldGraph.biomesGraph.nodes.Count;

                for (int i = 0; i < graphNodesCount; i++)
                {
                    GameObject tempObj = AssetDatabase.LoadAssetAtPath(module.prefabName, typeof(GameObject)) as GameObject;
                    tempObjects.Add(tempObj);
                    if (tempObj == null) module.prefabName = "";
                }

                if (!string.IsNullOrEmpty(module.prefabName)) GetPrefabLODs(ref module);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;

            if (THelpersUI.Foldout("TREE SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                //tempObject = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("PREFAB", "Prefab used for tree object"), tempObject, typeof(GameObject));
                //if (EditorGUI.EndChangeCheck()) module.prefabName = AssetDatabase.GetAssetPath(tempObject);

                tempObjects[tempObjects.Count - 1] = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("PREFAB", "Prefab used for tree object"), tempObjects[tempObjects.Count - 1], typeof(GameObject));
                if (EditorGUI.EndChangeCheck())
                {
                    module.prefabName = AssetDatabase.GetAssetPath(tempObjects[tempObjects.Count - 1]);
                    if (!string.IsNullOrEmpty(module.prefabName)) module.Data.name = Path.GetFileNameWithoutExtension(module.prefabName);
                    GetPrefabLODs(ref module);
                }

                ModelPreviewUI.ModelPreviewList(tempObjects[tempObjects.Count - 1], new GUIStyle(EditorStyles.helpBox), 128);

#if TERRAWORLD_PRO
                try
                {
                    for (int i = 0; i < incompatibleMaterialNames.Count; i++)
                        if (!string.IsNullOrEmpty(incompatibleMaterialNames[i]) && !string.IsNullOrEmpty(module.prefabName)) THelpersUI.GUI_HelpBox(incompatibleMaterialWarningText + incompatibleMaterialNames[i], MessageType.Warning, 10);
                }
#if TERRAWORLD_DEBUG
                catch (Exception e)
                {
                    throw e;
                }
#else
                catch { }
#endif
#endif

                if (string.IsNullOrEmpty(module.prefabName)) THelpersUI.GUI_Alert();
            }

            if (THelpersUI.Foldout("SPECIAL SETTINGS", ref module.uIToggles.Settings2))
            {
                THelpersUI.GUI_HelpBox("PLACEMENT ON WATER", MessageType.None, 20);

                int state = 0;

                if (module.bypassLakes)
                    state = 0;
                else if (module.underLakes)
                    state = 1;
                else if (module.onLakes)
                    state = 2;

                EditorGUI.BeginChangeCheck();
                state = THelpersUI.GUI_SelectionGrid(state, waterPlacementMode, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    if (state == 0)
                    {
                        module.bypassLakes = true;
                        module.underLakes = false;
                        module.underLakesMask = false;
                        module.onLakes = false;
                    }
                    else if (state == 1)
                    {
                        module.bypassLakes = false;
                        module.underLakes = true;
                        module.underLakesMask = true;
                        module.onLakes = false;
                    }
                    else if (state == 2)
                    {
                        module.bypassLakes = false;
                        module.underLakes = false;
                        module.underLakesMask = true;
                        module.onLakes = true;
                    }
                }

                //module.underLakes = THelpersUI.GUI_Toggle(new GUIContent("ALLOW UNDER WATER", "Is it allowed to place the model under water surface?"), module.underLakes);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings3))
            {
                module.seedNo = THelpersUI.GUI_IntField(new GUIContent("SEED#", "Seed number for unique procedural values"), module.seedNo);

                //module.density = THelpersUI.GUI_IntField(new GUIContent("DENSITY", "Placement density for the object"), module.density);
                //module.densityResolutionPerKilometer = THelpersUI.GUI_IntSlider(new GUIContent("KM2 RESOLUTION", "Density resolution per square kilometer for normalized placement"), Mathf.ClosestPowerOfTwo(module.densityResolutionPerKilometer), 8, 256);
                //module.densityResolutionPerKilometer = THelpersUI.GUI_IntSlider(new GUIContent("KM2 DENSITY", "Instance count per square kilometer for placement"), module.densityResolutionPerKilometer, 1, 50000);

                module.averageDistance = THelpersUI.GUI_FloatField(new GUIContent("PLACEMENT DISTANCE (M)", "Placement distance in meters between instances"), module.averageDistance, 0.1f);

                THelpersUI.GUI_MinMaxSlider(new GUIContent("ROTATION RANGE", "Minimum & Maximum rotation in degrees for object"), ref module.minRotationRange, ref module.maxRotationRange, 0f, 359f);
                module.positionVariation = THelpersUI.GUI_Slider(new GUIContent("RANDOM POSITION", "Percentage of the random position for instance"), module.positionVariation, 0, 100, 20);
                objectScale = THelpersUI.GUI_Vector3Field(new GUIContent("SCALE MULTIPLIER", "The multiplier value for the original object scale"), objectScale, 20);
                module.scaleMultiplier = TUtils.CastToNumerics(objectScale);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SCALE RANGE", "Minimum & Maximum random scale variation"), ref module.minScale, ref module.maxScale, 0.1f, 20f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref module.minSlope, ref module.maxSlope, 0f, 90f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref module.minElevation, ref module.maxElevation, -100000f, 100000f);
                module.checkBoundingBox = THelpersUI.GUI_Toggle(new GUIContent("CHECK BOUNDING BOX", "Check model's bounding box for placement?\nIf checked, placement will only be applied when the whole model is inside masked areas\nIf unchecked, mask is only checked in pivot position of the model"), module.checkBoundingBox, 30);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings4))
            {
                //module.layerName = THelpersUI.GUI_TextField(new GUIContent("LAYER NAME", "Parent Layer name of the current object's layer in hierarchy view"), module.layerName);
                module.maskLayer = THelpersUI.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), module.maskLayer);
            }

            module.minRange = minRangeUI;
            module.maxRange = maxRangeUI;

            GUI_ConnectionsSingleInput(module);
        }

        private void MeshGeneratorSettings(ref MeshGenerator module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                string resourcePath = module.Materialpath;
                CheckForEmptyResource(ref resourcePath);
                module.Materialpath = resourcePath;

                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                //THelpersUI.SwitchBoundingBox();

                if (!string.IsNullOrEmpty(module.Materialpath) && File.Exists(module.Materialpath))
                    material = AssetDatabase.LoadAssetAtPath(module.Materialpath, typeof(Material)) as Material;
                else
                    material = null;

                THelpersUI.GetMaskValue(module.unityLayerName);
                objectScale = TUtils.CastToUnity(module.scale);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                material = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for this object"), material, typeof(Material), null, 10);
                if (EditorGUI.EndChangeCheck()) module.Materialpath = AssetDatabase.GetAssetPath(material);
                if (material == null) THelpersUI.GUI_Alert();

                module.lodCulling = THelpersUI.GUI_Slider(new GUIContent("LOD CULLING (%)", "Object's screen size percentage where it culls from rendering"), module.lodCulling, 0f, 100f);

                module.hasShadowCasting = THelpersUI.GUI_Toggle(new GUIContent("CAST SHADOWS", "Specifies whether a geometry creates shadows or not when a shadow-casting light shines on it.\n\nIn most cases you need this option disabled to gain better performance since generated mesh is stuck on the ground and there is no need to cast shadows"), module.hasShadowCasting);

                if (module.hasShadowCasting)
                    THelpersUI.GUI_HelpBox("IT IS RECOMMENDED TO KEEP THIS OPTION DISABLED TO GAIN BETTER PERFORMANCE SINCE GENERATED MESH IS STUCK ON THE GROUND AND THERE IS NO NEED TO CAST SHADOWS FROM THIS SURFACE!", MessageType.Warning, -10);

                module.hasCollider = THelpersUI.GUI_Toggle(new GUIContent("COLLIDER", "Does object contribute to collision detection!"), module.hasCollider, -5);

                if (module.hasCollider)
                {
                    module.hasPhysics = THelpersUI.GUI_Toggle(new GUIContent("PHYSICS", "Enable or disable Rigidbody component on the object to perform physics calculations"), module.hasPhysics, -10);
                    THelpersUI.GUI_HelpBox("GENERATED SURFACE WILL CONTRIBUTE TO COLLISIONS BUT SINCE IT'S A DISPLACED SURFACE STUCK ON THE GROUND, THE COLLISIONS ARE NOT IN MATCH WITH TESSELLATION.\n\nIT IS RECOMMENDED TO KEEP THIS OPTION DISABLED UNSLESS YOU NEED TO DETECT THESE SURFACES VIA PHYSICS RAYCASTING TO DO PLACEMENT FILTERING OR OTHER OPERATIONS!", MessageType.Info);
                }
                else
                    THelpersUI.GUI_HelpBox("DISABLING COLLIDER ON OBJECT WILL BYPASS PLACEMENT CHECK THROUGH RAYCASTS AND LAYERMASKS!", MessageType.Info, -10);
            }

            if (THelpersUI.Foldout("SPECIAL SETTINGS", ref module.uIToggles.Settings2))
            {
                module.SeperatedObject = THelpersUI.GUI_Toggle(new GUIContent("SINGLE MESH", "If disabled, a grid of mesh tiles will be generated and if enabled, a single mesh object will be generated which is not recommended since having tiles of meshes will bring better performance with LOD culling on each tile."), module.SeperatedObject, 10);
                if (!module.SeperatedObject)
                {
                    module.gridCount = THelpersUI.GUI_IntSlider(new GUIContent("GRID COUNT IN EACH ROW", "Divide surface into a grid of mesh chunks"), module.gridCount, 1, 32);
                    THelpersUI.GUI_HelpBox(new GUIContent("Total of  " + Mathf.Pow(module.gridCount, 2) + "  object(s) will be generated", "Total number of mesh tiles copied from the terrain surface"), true, -10);
                }
                else
                    THelpersUI.GUI_HelpBox("A single mesh object will be generated which is not recommended since having tiles of meshes will bring better performance with LOD culling on each tile!", MessageType.Warning, -10);

                module.densityResolutionPerKilometer = THelpersUI.GUI_IntSlider(new GUIContent("RESOLUTION (M)", "Distance between mesh vertices in meters"), module.densityResolutionPerKilometer, 1, 50, 20);
                THelpersUI.GUI_HelpBox(new GUIContent("Generated mesh will have resolution of  " + module.densityResolutionPerKilometer + "  meters", "Distance between mesh vertices in meters"), true, -10);

                module.density = THelpersUI.GUI_IntSlider(new GUIContent("MESH DENSITY", "Density of generated mesh"), module.density, 1, 100, 20);
                module.edgeCurve = THelpersUI.GUI_FloatField(new GUIContent("EDGE CURVE (M)", "Curve amount in meters along the edges of the mesh vertices to cut through the terrain surface for better object blending and avoiding stuck-out edges"), module.edgeCurve, -100, 100);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings3))
            {
                objectScale = THelpersUI.GUI_Vector3Field(new GUIContent("SCALE", "Original scale of the object"), objectScale, 20);
                module.scale = TUtils.CastToNumerics(objectScale);

                tempVector3D = TUtils.CastToUnity(module.positionOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("OFFSET", "Placement offset of the object in 3 dimensions"), tempVector3D);
                module.positionOffset = TUtils.CastToNumerics(tempVector3D);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings4))
            {
                //                module.priority = THelpersUI.GUI_IntField(new GUIContent("PRIORITY", "Priority value of the current object's layer"), module.priority);
                module.layerName = THelpersUI.GUI_TextField(new GUIContent("LAYER NAME", "Parent Layer name of the current object's layer"), module.layerName);
                module.unityLayerName = THelpersUI.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
            }


            GUI_ConnectionsSingleInput(module);
        }

        private void GetPrefabCollider(ref InstanceScatter module)
        {
            if (!string.IsNullOrEmpty(module.prefabName) && tempObjects != null && tempObjects.Count > 0)
            {
                colliderDetected = false;

                if (LODObjects == null) LODObjects = new List<GameObject>(); else LODObjects.Clear();

                foreach (Transform t in tempObjects[tempObjects.Count - 1].GetComponentsInChildren(typeof(Transform), false))
                    if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                        LODObjects.Add(t.gameObject);

                foreach (Transform t in tempObjects[tempObjects.Count - 1].GetComponentsInChildren(typeof(Transform), false))
                {
                    if (!colliderDetected && t.GetComponent<Collider>() != null && t.GetComponent<Collider>().enabled)
                    {
                        //gameObjectWithCollider = t.gameObject;
                        colliderDetected = true;
                    }
                }

                if (!colliderDetected)
                {
                    foreach (Transform t in tempObjects[tempObjects.Count - 1].GetComponentsInChildren(typeof(Transform), false))
                    {
                        if (!colliderDetected && t.GetComponent<MeshCollider>() != null && t.GetComponent<MeshCollider>().enabled)
                        {
                            //gameObjectWithCollider = t.gameObject;
                            colliderDetected = true;
                        }
                    }
                }

                if (colliderDetected) module.prefabHasCollider = true;

                if (tempObjects[tempObjects.Count - 1].GetComponent<LODGroup>() != null)
                    LODGroupDetected = true;
                else
                    LODGroupDetected = false;
            }
            else
                LODObjects.Clear();

#if TERRAWORLD_PRO
            IsCompatibleMaterials(LODObjects);
#endif
        }

        private void GetPrefabLODs(ref TreeScatter module)
        {
            if (!string.IsNullOrEmpty(module.prefabName) && tempObjects != null && tempObjects.Count > 0)
            {
                if (LODObjects == null) LODObjects = new List<GameObject>(); else LODObjects.Clear();

                foreach (Transform t in tempObjects[tempObjects.Count - 1].GetComponentsInChildren(typeof(Transform), false))
                    if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                        LODObjects.Add(t.gameObject);
            }
            else
                LODObjects.Clear();

#if TERRAWORLD_PRO
            IsCompatibleMaterials(LODObjects);
#endif
        }

        //        private void GetPrefabLODs(ref ObjectScatter module)
        //        {
        //            for (int i = 0; i < module.prefabNames.Count; i++)
        //            {
        //                if (!string.IsNullOrEmpty(module.prefabNames[i]) && tempObjects != null && tempObjects.Count > 0)
        //                {
        //                    if (LODObjects == null) LODObjects = new List<GameObject>(); else LODObjects.Clear();
        //
        //                    foreach (Transform t in tempObjects[tempObjects.Count - 1].GetComponentsInChildren(typeof(Transform), false))
        //                        if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
        //                            LODObjects.Add(t.gameObject);
        //                }
        //                else
        //                    LODObjects.Clear();
        //
        //#if TERRAWORLD_PRO
        //                IsCompatibleMaterials(LODObjects, false, i);
        //#endif
        //            }
        //        }

#if TERRAWORLD_PRO
        public void IsCompatibleMaterials(List<GameObject> LODs, bool isSingleObject = true, int index = 0)
        {
            if (index == 0) incompatibleMaterialNames = new List<string>();

            for (int i = 0; i < LODs.Count; i++)
            {
                Material[] materials = LODs[i].GetComponent<Renderer>().sharedMaterials;

                for (int j = 0; j < materials.Length; j++)
                {
                    if (!materials[j].shader.name.Equals("TerraUnity/Standard"))
                    {
                        incompatibleMaterialNames.Add(materials[j].name);
                        //break;
                        return;
                    }

                    //List<string> keywords = materials[j].shaderKeywords.ToList();
                    //
                    //if
                    //(
                    //    keywords.Contains("_PROCEDURALSNOW") &&
                    //    keywords.Contains("_WIND") &&
                    //    keywords.Contains("_FLATSHADING")
                    //)
                    //    isCompatibleMaterials = true;
                }
            }
        }
#endif

      //  private void CheckForEmptyResourceOld(ref string resourcePath)
      //  {
      //      if (!string.IsNullOrEmpty(resourcePath))
      //      {
      //          if (!File.Exists(Path.GetFullPath(resourcePath)))
      //          {
      //              foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)) as UnityEngine.Object[])
      //              {
      //                  if (obj == null) continue;
      //                  if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave) continue;
      //
      //                  if (Path.GetFileNameWithoutExtension(obj.name) == Path.GetFileNameWithoutExtension(resourcePath))
      //                  {
      //                      string assetPath = AssetDatabase.GetAssetPath(obj);
      //
      //                      if (assetPath.Contains("Assets") && Path.GetExtension(assetPath) == Path.GetExtension(resourcePath))
      //                      {
      //                          resourcePath = assetPath;
      //                          //Debug.Log("Resource asset needed for node has been reset.");
      //                          break;
      //                      }
      //                  }
      //              }
      //          }
      //      }
      //  }

        private void CheckForEmptyResource(ref string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return;
            if (File.Exists(Path.GetFullPath(resourcePath))) return;
            string partialName = Path.GetFileName(resourcePath);
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(Application.dataPath + "/");

            try
            {
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles(partialName, SearchOption.AllDirectories);

                // if (filesInDir.Length == 1 ) resourcePath = filesInDir[0].FullName;
                if (filesInDir.Length > 0)
                {
                    int index = filesInDir[0].FullName.IndexOf("Asset");
                    string newpath = filesInDir[0].FullName.Remove(0, index);
                    newpath = newpath.Replace(Path.PathSeparator, '/');
                    if (EditorUtility.DisplayDialog("TERRAWORLD", "Refrence to the following file is missing.\n" + resourcePath + "\n Do you want to automatically recover it to \n" + newpath, "Yes", "No"))
                    {
                        Debug.Log("Address : " + resourcePath + " Have been replaced by : " + newpath);
                        resourcePath = newpath;
                    }
                }
            }
            catch { }
        }

        private void InstanceScatterSettings(ref InstanceScatter module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                //string resourcePath = module.prefabName;
                CheckForEmptyResource(ref module.prefabName);
                //module.prefabName = resourcePath;

                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                THelpersUI.GetMaskValue(module.unityLayerName);
                objectScale = TUtils.CastToUnity(module.scaleMultiplier);
                minRangeUI = module.minRange;
                maxRangeUI = module.maxRange;

                //TODO: Temporary solution so that each node have its own prefab reference
                tempObjects = new List<GameObject>();
                int graphNodesCount = TTerraWorld.WorldGraph.biomesGraph.nodes.Count;

                for (int i = 0; i < graphNodesCount; i++)
                {
                    GameObject tempObj = AssetDatabase.LoadAssetAtPath(module.prefabName, typeof(GameObject)) as GameObject;
                    tempObjects.Add(tempObj);
                    if (tempObj == null) module.prefabName = "";
                }

                if (!string.IsNullOrEmpty(module.prefabName)) GetPrefabCollider(ref module);
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("PREFAB SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                tempObjects[tempObjects.Count - 1] = (GameObject)THelpersUI.GUI_ObjectField(new GUIContent("PREFAB", "Prefab used for GPU instance object"), tempObjects[tempObjects.Count - 1], typeof(GameObject));
                if (EditorGUI.EndChangeCheck())
                {
                    module.prefabName = AssetDatabase.GetAssetPath(tempObjects[tempObjects.Count - 1]);
                    if (!string.IsNullOrEmpty(module.prefabName)) module.Data.name = Path.GetFileNameWithoutExtension(module.prefabName);
                    GetPrefabCollider(ref module);
                }

                ModelPreviewUI.ModelPreviewList(tempObjects[tempObjects.Count - 1], new GUIStyle(EditorStyles.helpBox), 128);

                if (!string.IsNullOrEmpty(module.prefabName) && LODObjects != null && LODObjects.Count > 0)
                {
                    if (!LODGroupDetected)
                        module.maxDistance = THelpersUI.GUI_FloatField(new GUIContent("RENDERING DISTANCE", "Maximum rendering distance for this object"), module.maxDistance, 1f, 10000f);
                    else
                        module.LODMultiplier = THelpersUI.GUI_Slider(new GUIContent("LOD Multiplier", "The LOD settings will multiply by this value."), module.LODMultiplier, 1f, 5f);

                    //EditorGUI.BeginDisabledGroup(!colliderDetected);
                    //if (!colliderDetected) module.prefabHasCollider = false;
                    //module.prefabHasCollider = THelpersUI.GUI_Toggle(new GUIContent("COLLISION", "Contribute to collision detection!"), module.prefabHasCollider);
                    //EditorGUI.EndDisabledGroup();

#if TERRAWORLD_PRO
                    for (int i = 0; i < incompatibleMaterialNames.Count; i++)
                        if (!string.IsNullOrEmpty(incompatibleMaterialNames[i]) && !string.IsNullOrEmpty(module.prefabName)) THelpersUI.GUI_HelpBox(incompatibleMaterialWarningText + incompatibleMaterialNames[i], MessageType.Warning, 10);
#endif

                    if (colliderDetected)
                        THelpersUI.GUI_HelpBox("THIS OBJECT HAS COLLIDERS TO INTERACT WITH PHYSICS!", MessageType.Info);
                    else
                        THelpersUI.GUI_HelpBox("THIS OBJECT HAS NO COLLIDERS TO INTERACT WITH PHYSICS!", MessageType.Warning);
                }
                else
                    THelpersUI.GUI_Alert();
            }

            if (THelpersUI.Foldout("SPECIAL SETTINGS", ref module.uIToggles.Settings2))
            {
                //TODO: This one is private and internal which will be picked randomly in runtime
                module.shadowCastingMode = (TShadowCastingMode)THelpersUI.GUI_EnumPopup(new GUIContent("SHADOW CASTING", "Shadow Casting mode for each instance"), module.shadowCastingMode);
                module.receiveShadows = THelpersUI.GUI_Toggle(new GUIContent("SHADOW RECEIVING", "Receive shadows on model or not?"), module.receiveShadows);

                THelpersUI.GUI_HelpBox("PLACEMENT ON WATER SURFACE", MessageType.None, 20);

                int state = 0;

                if (module.bypassLakes)
                    state = 0;
                else if (module.underLakes)
                    state = 1;
                else if (module.onLakes)
                    state = 2;

                EditorGUI.BeginChangeCheck();
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
                state = THelpersUI.GUI_SelectionGrid(state, waterPlacementMode, style, -10);
                if (EditorGUI.EndChangeCheck())
                {
                    if (state == 0)
                    {
                        module.bypassLakes = true;
                        module.underLakes = false;
                        module.underLakesMask = true;
                        module.onLakes = false;
                    }
                    else if (state == 1)
                    {
                        module.bypassLakes = false;
                        module.underLakes = true;
                        module.underLakesMask = true;
                        module.onLakes = false;
                    }
                    else if (state == 2)
                    {
                        module.bypassLakes = false;
                        module.underLakes = false;
                        module.underLakesMask = true;
                        module.onLakes = true;
                    }
                }

                //module.underLakes = THelpersUI.GUI_Toggle(new GUIContent("ALLOW UNDER WATER", "Is it allowed to place the model under water surface?"), module.underLakes);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings3))
            {
                module.seedNo = THelpersUI.GUI_IntField(new GUIContent("SEED#", "Seed number for unique procedural values"), module.seedNo, 0, Int32.MaxValue, 30);
                module.averageDistance = THelpersUI.GUI_FloatField(new GUIContent("PLACEMENT DISTANCE (M)", "Placement distance in meters between instances"), module.averageDistance, 0.1f);
                //module.gridSizeValue = (int)THelpersUI.GUI_Slider(new GUIContent("INSTANCE COUNT", "Maximum instance count for each generating patch (Lower values will reduce performance)"), module.gridSizeValue, 10, 1024);

                //TODO: Check the following lines later
                //int minimumResolution = (int)Mathf.Clamp((TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat * 1000) / (module.averageDistance * 30), 4, 150);
                //int maximumResolution = (int)Mathf.Clamp((TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat * 1000) / (module.averageDistance), minimumResolution, 150);
                //module.gridResolution = THelpersUI.GUI_IntSlider(new GUIContent("GRID RESOLUTION", "Grid resolution for GPU instance layer"), module.gridResolution, minimumResolution, maximumResolution);

                //module.maxDistance = THelpersUI.GUI_FloatField(new GUIContent("RENDERING DISTANCE (m)", "Auto Show/Hide distance from camera for each generating patch (Increasing the distance will reduce performance)"), module.maxDistance, module.averageDistance * 40);
                module.frustumMultiplier = THelpersUI.GUI_Slider(new GUIContent("Frustum Multiplier", "Camera Frustum range for wider frustum range"), module.frustumMultiplier, 1f, 3f);

                module.checkBoundingBox = THelpersUI.GUI_Toggle(new GUIContent("CHECK BOUNDING BOX", "Check model's bounding box for placement?\nIf checked, placement will only be applied when the whole model is inside masked areas\nIf unchecked, mask is only checked in pivot position of the model"), module.checkBoundingBox, 30);

                module.getSurfaceAngle = THelpersUI.GUI_Toggle(new GUIContent("SURFACE ROTATION", "Rotation is calculated from the underlying surface angle"), module.getSurfaceAngle, 20);
                module.rotation90Degrees = THelpersUI.GUI_Toggle(new GUIContent("90° ROTATION", "Rotation is only performed in 90 degree multiplies 0°, 90°, 180° & 270°"), module.rotation90Degrees);
                if (!module.getSurfaceAngle) module.lockYRotation = THelpersUI.GUI_Toggle(new GUIContent("HORIZONTAL ROTATION", "Rotation is locked in horizontal rotation axis (Y) only suited for objects like trees"), module.lockYRotation);
                if (!module.rotation90Degrees) THelpersUI.GUI_MinMaxSlider(new GUIContent("ROTATION RANGE", "Minimum & Maximum rotation in degrees for object"), ref module.minRotationRange, ref module.maxRotationRange, 0f, 359f);

                module.positionVariation = THelpersUI.GUI_Slider(new GUIContent("RANDOM POSITION", "Percentage of the random position for instance"), module.positionVariation, 0, 100, 20);
                //module.gridSizeValue = THelpersUI.GUI_IntSlider(new GUIContent("GRID COUNT", "Grid counts of area in each direction."), module.gridSizeValue, 0, 100);

                objectScale = THelpersUI.GUI_Vector3Field(new GUIContent("SCALE", "Original scale of the instance"), objectScale, 20);
                module.scaleMultiplier = TUtils.CastToNumerics(objectScale);

                //module.scaleMultiplier = THelpersUI.GUI_Slider(new GUIContent("SCALE MULTIPLIER", "The multiplier value for the original object scale"), module.scaleMultiplier, 0.1f, 10f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("SCALE RANGE", "Minimum & Maximum random scale variation which would multiply to SCALE"), ref module.minScale, ref module.maxScale, 0.1f, 20f);

                //TODO: If one is enabled, disable the other
                //module.hasPhysics = THelpersUI.GUI_Toggle(new GUIContent("PHYSICS", "Enable or disable Rigidbody component on the object"), module.hasPhysics);
                //TODO: Add isPhysicsTrigger

                THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref module.minSlope, ref module.maxSlope, 0f, 90f);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref module.minElevation, ref module.maxElevation, -100000f, 100000f);

                tempVector3D = TUtils.CastToUnity(module.positionOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("POSITION OFFSET", "Placement offset of the object in 3 dimensions"), tempVector3D);
                module.positionOffset = TUtils.CastToNumerics(tempVector3D);

                tempVector3D = TUtils.CastToUnity(module.rotationOffset);
                tempVector3D = THelpersUI.GUI_Vector3Field(new GUIContent("ROTATION OFFSET", "Rotation offset of the object in 3 dimensions"), tempVector3D);
                module.rotationOffset = TUtils.CastToNumerics(tempVector3D);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings4))
            {
                module.layerName = THelpersUI.GUI_TextField(new GUIContent("LAYER NAME", "Parent Layer name of the current object's layer"), module.layerName);
                module.unityLayerName = THelpersUI.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
                module.maskLayer = THelpersUI.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), module.maskLayer);
            }

            if (THelpersUI.Foldout("RENDERING SETTINGS", ref module.uIToggles.Settings5))
            {
                module.occlusionCulling = THelpersUI.GUI_Toggle(new GUIContent("OCCLUSION CULLING", "If selected, layer will be checked for Real-time Occlusion Culling against all objects with Static flag of \"Occluder Static\" in Game mode"), module.occlusionCulling, 10);
            }

            module.minRange = minRangeUI;
            module.maxRange = maxRangeUI;

            GUI_ConnectionsSingleInput(module);
        }

        private void GrassScatterSettings(ref GrassScatter module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);

                string resourcePath = module.Materialpath;
                CheckForEmptyResource(ref resourcePath);
                module.Materialpath = resourcePath;

                if (!string.IsNullOrEmpty(module.Materialpath) && File.Exists(module.Materialpath))
                    material = AssetDatabase.LoadAssetAtPath(module.Materialpath, typeof(Material)) as Material;
                else
                    material = null;

                mesh = null;

                if (!string.IsNullOrEmpty(module.Modelpath) && File.Exists(module.Modelpath) && !string.IsNullOrEmpty(module.MeshName))
                    mesh = TMesh.GetMeshObject(module.Modelpath, module.MeshName);
                else
                    mesh = null;

                THelpersUI.GetMaskValue(module.unityLayerName);

                minRangeUI = module.minRange;
                maxRangeUI = module.maxRange;
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            //THelpersUI.GUI_BoundingBox();

            if (THelpersUI.Foldout("MODEL SETTINGS", ref module.uIToggles.Settings1))
            {
                EditorGUI.BeginChangeCheck();
                material = (Material)THelpersUI.GUI_ObjectField(new GUIContent("MATERIAL", "Material used for grass/plant/bush rendering"), material, typeof(Material), null);
                if (EditorGUI.EndChangeCheck()) module.Materialpath = AssetDatabase.GetAssetPath(material);
                if (material == null) THelpersUI.GUI_Alert();
                ModelPreviewUI.ModelPreviewList(material, new GUIStyle(EditorStyles.helpBox), 128);

                module.builderType = (BuilderType)THelpersUI.GUI_EnumPopup(new GUIContent("MODEL TYPE", "Select between Quad (Billboards) and 3D Mesh for rendering"), module.builderType);

                if (module.builderType == BuilderType.FromMesh)
                {
                    EditorGUI.BeginChangeCheck();
                    mesh = (Mesh)THelpersUI.GUI_ObjectField(new GUIContent("MESH", "Mesh used for rendering"), mesh, null, null, -10);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (mesh != null)
                        {
                            string MeshName = "";
                            string Modelpath = "";
                            TMesh.GetMeshPath(mesh, ref Modelpath, ref MeshName);
                            module.MeshName = MeshName;
                            module.Modelpath = Modelpath;
                        }
                        else
                        {
                            module.MeshName = "";
                            module.Modelpath = "";
                        }
                    }

                    if (mesh == null) THelpersUI.GUI_Alert();
                }

                //module.maxParallelJobCount = THelpersUI.GUI_IntSlider(new GUIContent("PARALLEL JOBS", "Max Parallel Jobs Count for operations"), module.maxParallelJobCount, 1, 200, -10);
            }

            if (THelpersUI.Foldout("PLACEMENT SETTINGS", ref module.uIToggles.Settings2))
            {
                module.seedNo = THelpersUI.GUI_IntField(new GUIContent("SEED#", "Seed number for unique procedural values"), module.seedNo);

                //if (module.builderType == BuilderType.FromMesh)
                //    module.amountPerBlock = THelpersUI.GUI_IntSlider(new GUIContent("DENSITY", "Amount of instances per patch"), module.amountPerBlock, 10, 200, -10);
                //else
                module.amountPerBlock = THelpersUI.GUI_IntSlider(new GUIContent("DENSITY", "Amount of instances per patch"), module.amountPerBlock, 10, 5000, -10);

                tempVector2D = TUtils.CastToUnity(module.scale);
                tempVector2D = THelpersUI.GUI_Vector2Field(new GUIContent("SCALE", "Scale of grass/plant blades"), tempVector2D, -10);
                module.scale = TUtils.CastToNumerics(tempVector2D);

                module.slant = THelpersUI.GUI_Slider(new GUIContent("SLANT", "Slant amount for grass/plant blades to create random vertical rotations"), module.slant, 0f, 1f, -10);

                //module.alphaMapThreshold = THelpersUI.GUI_Slider(new GUIContent("MASK THRESOLD", "Masked area thresold"), module.alphaMapThreshold, 0.001f, 0.999f, -10);
                //module.densityFactor = THelpersUI.GUI_Slider(new GUIContent("MASK DENSITY", "Masked area density"), module.densityFactor, 0.001f, 0.999f, -10);
                module.alphaMapThreshold = 0.5f;
                module.densityFactor = 0.5f;

                module.groundOffset = THelpersUI.GUI_Slider(new GUIContent("GROUND OFFSET", "Ground offset in vertical direction"), module.groundOffset, -50f, 50f, -10);
                module.maskDamping = THelpersUI.GUI_Slider(new GUIContent("MASK DAMPING", "How much damping is needed for placement based on input area mask? 0 means no damping and exactly within given mask where higher values place items beyond mask borders for more organic placement."), module.maskDamping, 0f, 10f, -10);

                THelpersUI.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref module.minSlope, ref module.maxSlope, 0f, 90f, 10);
                THelpersUI.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref module.minElevation, ref module.maxElevation, -100000f, 100000f);

                module.layerBasedPlacement = THelpersUI.GUI_Toggle(new GUIContent("CHECK LAYERS", "Raycast against underlying surfaces to decide on placement based on detected layers"), module.layerBasedPlacement, 10);

                if (module.layerBasedPlacement)
                {
                    THelpersUI.GUI_HelpBox("THIS FEATURE WILL AFFECT PERFORMANCE ON DENSER LAYERS", MessageType.Warning);

                    THelpersUI.GUI_HelpBox("PLACEMENT ON WATER", MessageType.None, 10);

                    int state = 0;

                    if (module.bypassWater)
                        state = 0;
                    else if (module.underWater)
                        state = 1;
                    else if (module.onWater)
                        state = 2;

                    EditorGUI.BeginChangeCheck();
                    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
                    state = THelpersUI.GUI_SelectionGrid(state, waterPlacementMode, style, -10);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (state == 0)
                        {
                            module.bypassWater = true;
                            module.underWater = false;
                            module.onWater = false;
                        }
                        else if (state == 1)
                        {
                            module.bypassWater = false;
                            module.underWater = true;
                            module.onWater = false;
                        }
                        else if (state == 2)
                        {
                            module.bypassWater = false;
                            module.underWater = false;
                            module.onWater = true;
                        }
                    }

                    module.maskLayer = THelpersUI.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), module.maskLayer);
                }
            }

            if (THelpersUI.Foldout("RENDERING SETTINGS", ref module.uIToggles.Settings3))
            {
                //if (module.builderType == BuilderType.FromMesh)
                //{
                //    module.radius = THelpersUI.GUI_Slider(new GUIContent("RENDERING DISTANCE", "Rendering Distance to draw vegetation"), module.radius, module.gridSize * 2, 1000f);
                //    module.gridSize = THelpersUI.GUI_Slider(new GUIContent("GRID SIZE", "Grid Size for patches"), module.gridSize, 20f, 200f, -10);
                //}
                //else
                //{
                module.radius = THelpersUI.GUI_Slider(new GUIContent("RENDERING DISTANCE", "Rendering Distance to draw vegetation"), module.radius, module.gridSize * 2, 2000f);
                module.gridSize = THelpersUI.GUI_Slider(new GUIContent("GRID SIZE", "Grid Size for patches"), module.gridSize, 5f, 200f, -10);
                //}

                module.shadowCastingMode = (TShadowCastingMode)THelpersUI.GUI_EnumPopup(new GUIContent("SHADOW CASTING", "Shadow Casting mode for each instance"), module.shadowCastingMode, -10);
                module.normalType = (NormalType)THelpersUI.GUI_EnumPopup(new GUIContent("NORMAL TYPE", "Normal type for rendering"), module.normalType, -10);
            }

            if (THelpersUI.Foldout("LAYER SETTINGS", ref module.uIToggles.Settings4))
            {
                module.unityLayerName = THelpersUI.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
            }

            module.minRange = minRangeUI;
            module.maxRange = maxRangeUI;

            GUI_ConnectionsSingleInput(module);
        }

        //private void ColormapFromSlopeSettings(ref ColormapFromSlope module)
        //{
        //    if (lastNode != THelpersUI.ActiveNode)
        //    {
        //        input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
        //        input2 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList2, module, 1);
        //    }
        //
        //    THelpersUI.GUI_HelpBox(module.Data.name);
        //    module._strength = THelpersUI.GUI_Slider(new GUIContent("STRENGTH", "The strength of the operation"), module._strength, 0.001f, 1f);
        //    //module._widthMultiplier = THelpersUI.GUI_Slider("WIDTH MULTIPLIER", module._widthMultiplier, 0.1f, 2f);
        //    //module._heightMultiplier = THelpersUI.GUI_Slider("HEIGHT MULTIPLIER", module._heightMultiplier, 0.1f, 2f);
        //    module._colorsCount = THelpersUI.GUI_IntSlider(new GUIContent("COLORS COUNT", "Number of map colors"), module._colorsCount, 4, 256);
        //    module._damping = THelpersUI.GUI_Slider(new GUIContent("DAMPING", "The tolerance & damping of the selected color range"), module._damping, 0f, 1f);
        //    module._dampingTest = THelpersUI.GUI_Slider(new GUIContent("DAMPING TEST", "The tolerance & damping of the selected color range"), module._dampingTest, 0f, 1f);
        //    module._useSatelliteImage = THelpersUI.GUI_Toggle(new GUIContent("USE SATELLITE IMAGE", "Use satellite image blending"), module._useSatelliteImage);
        //    module._tolerance = THelpersUI.GUI_IntSlider(new GUIContent("TOLERANCE", "The tolerance & damping of the colors ranges"), module._tolerance, 2, 50);
        //
        //    EditorGUILayout.HelpBox("COLORS", MessageType.None);
        //    EditorGUILayout.HelpBox("TODO: COLOR ARRAY/GRADIENT", MessageType.None);
        //    EditorGUILayout.HelpBox("SLOPES", MessageType.None);
        //    EditorGUILayout.HelpBox("TODO: SLOPES SLIDERS LIKE TERRACE", MessageType.None);
        //
        //    GUI_ConnectionsDoubleInput(module);
        //}

        private void MaskBlendSettings(ref MaskBlendOperator module)
        {
            if (lastNode != THelpersUI.ActiveNode)
            {
                input1 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList1, module, 0);
                input2 = TTerraWorld.WorldGraph.UpdateInputList(ref moduleList2, module, 1);
                minRangeUI = 0;
                maxRangeUI = 1;
            }

            THelpersUI.GUI_HelpBox(module.Data.name);
            if (THelpersUI.Foldout("SETTINGS", ref module.uIToggles.Settings1))
                module.blendingMode = (TModuleActions.BlendingMode)THelpersUI.GUI_EnumPopup(new GUIContent("BLENDING MODE", "Select blending mode to merge masks"), module.blendingMode);

            GUI_ConnectionsDoubleInput(module);
        }

        public static void ShowMapAndRefresh()
        {
            InteractiveMapGUI.Init();
        }

        public static void StopGeneration()
        {
            if (EditorUtility.DisplayDialog("STOP GENERATION", "Are you sure you want to stop world generation?", "Yes", "No"))
            {
                TTerrainGenerator.CancelProgress();
                TTerrainGenerator.ResetNodesProgress();
                //creationProgress = 0;
                TTerrainGenerator.ClearMemory();
                //TTerrainGenerator.ResetCodes();
            }
        }

        private TNode NodeWithMissingResource()
        {
            List<TGraph> graphList = TTerraWorld.WorldGraph.LoadGraphListCurrent();

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    TNode node = graphList[i].nodes[j];

                    if (node.isActive)
                    {
                        List<string> nodeResources = node.GetResourcePaths();

                        if (nodeResources != null && nodeResources.Count > 0)
                            for (int k = 0; k < nodeResources.Count; k++)
                                if (string.IsNullOrEmpty(nodeResources[k]))
                                    return node;
                    }
                }

            return null;
        }

        private void GetDirtyNode()
        {
            if (GUI.changed && TTerrainGenerator.Idle)
                SetCacheStates(false);
        }

        public static void SetCacheStates(bool mapChanged)
        {
            bool nodeDetected = false;

            for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
                for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
                    TTerraWorld.WorldGraph.graphList[i].nodes[j].Progress = 0;

            if (activeTab == ShowTabs.showArea || mapChanged)
            {
                for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
                    for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
                        TTerraWorld.WorldGraph.graphList[i].nodes[j].isDone = false;
            }
            else if (THelpersUI.ActiveNode != null)
            {
                for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
                    for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
                    {
                        if (TTerraWorld.WorldGraph.graphList[i].nodes[j] == THelpersUI.ActiveNode) nodeDetected = true;
                        if (nodeDetected) TTerraWorld.WorldGraph.graphList[i].nodes[j].isDone = false;
                    }
            }
            else
                for (int i = 0; i < TTerraWorld.WorldGraph.graphList.Count; i++)
                    for (int j = 0; j < TTerraWorld.WorldGraph.graphList[i].nodes.Count; j++)
                        TTerraWorld.WorldGraph.graphList[i].nodes[j].isDone = false;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

#if TERRAWORLD_XPRO
        public static NodeEditorWindow OpenXGraph()
        {
            if (!TTerraworldGenerator.XGraph) return null;

            NodeEditorWindow w = GetWindow(typeof(NodeEditorWindow), false, "TERRAWORLD GRAPH", true) as NodeEditorWindow;
            w.wantsMouseMove = true;
            w.graph = TTerraworldGenerator.XGraph;
            return w;

        }

        public static void CloseXGraph()
        {
            NodeEditorWindow w = GetWindow(typeof(NodeEditorWindow), false, "TERRAWORLD GRAPH", true) as NodeEditorWindow;
            w?.Close();
        }
#endif
    }
}
#endif

