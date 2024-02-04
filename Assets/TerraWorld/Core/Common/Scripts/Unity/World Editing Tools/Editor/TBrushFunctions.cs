#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewlist.MassiveGrass;
using TerraUnity.UI;

namespace TerraUnity.Runtime.UI
{
    public class TBrushFunctions : Editor
    {
        public static WorldTools WT;
        public static GPUInstanceLayer GIL;
        public static GrassLayer GL;
        private const string worldPaintingLayers = "TerraWorld - Paint Layers";
        private const string worldPaintingLayerGPU = "TerraWorld - Paint Layer GPU";
        private const string worldPaintingLayerGrass = "TerraWorld - Paint Layer Grass";
        private static Vector2 lastMousePosition;
        private static List<TScatterLayer.MaskDataFast[]> lastFilters;
        private static TScatterLayer.MaskDataFast[] lastFilter;
        private static Terrain terrain = null;
        private static int progressID = -1;
        private static int maxParallelJobs;

        public static void OnDestroy()
        {
            WorldToolsParams.isEditMode = false;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif

            ResetParams();
        }

        private static void ResetParams()
        {
            if (WT != null)
            {
                if (WT.maskDataListGPU != null)
                {
                    WT.maskDataListGPU.Clear();
                    WT.maskDataListGPU = null;
                }

                if (WT.maskDataListGrass != null)
                {
                    WT.maskDataListGrass.Clear();
                    WT.maskDataListGrass = null;
                }
            }

            WorldToolsParams.isGPULayer = false;
            WorldToolsParams.isGrassLayer = false;

            //TODO: May need to reset all common parameters!
        }

        public static void EditPlacement()
        {
            WorldToolsParams.isEditMode = !WorldToolsParams.isEditMode;

#if UNITY_2019_1_OR_NEWER
            if (WorldToolsParams.isEditMode)
                SceneView.duringSceneGui += OnSceneGUI;
            else
                SceneView.duringSceneGui -= OnSceneGUI;
#else
            if (WorldToolsParams.isEditMode)
                SceneView.onSceneGUIDelegate += OnSceneGUI;
            else
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif

            if (WorldToolsParams.globalMode)
            {
                if (WorldToolsParams.isEditMode)
                {
                    WT.isolateLayer = false;
                    WorldToolsParams.isolatedIndexGPU = -1;
                    WorldToolsParams.isolatedIndexGrass = -1;
                    LoadAllLayerMasks();
                    RefreshLayers();
                }
            }
            else
            {
                if (WorldToolsParams.isEditMode)
                {
                    if (WorldToolsParams.isGPULayer)
                    {
                        GIL.isolateLayer = true;
                        if (GIL.parameters == null) GIL.parameters = GIL.transform.GetChild(0).GetComponent<TScatterParams>();
                        ConvertMaskDataToImage(GIL.parameters.maskDataFast);
                    }
                    else if (WorldToolsParams.isGrassLayer)
                    {
                        GL.isolateLayer = true;
                        ConvertMaskDataToImage(GL.MGP.maskDataFast);
                    }
                }
            }

            //SetMaxParallelJobs();

            //if (WorldToolsParams.isEditMode)
            //Undo.undoRedoPerformed += DetectPaintingChanges;
        }

        private static void LoadAllLayerMasks()
        {
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();
            GrassLayer[] GrassLayers = FindObjectsOfType<GrassLayer>();

            if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                foreach (TScatterParams g in GPULayers)
                    g.LoadMaskData(true);

            if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                foreach (GrassLayer g in GrassLayers)
                    g.LoadMaskData(true);
        }

        public static void SetMaxParallelJobs(bool reset = false)
        {
            if (reset)
            {
                //TODO: Implement grass max parallel jobs for WorldTools
                if (WorldToolsParams.globalMode)
                {

                }
                else if (WorldToolsParams.isGrassLayer)
                    GL.massiveGrass.maxParallelJobCount = maxParallelJobs;
            }
            else
            {
                if (WorldToolsParams.isEditMode)
                {
                    //TODO: Implement grass max parallel jobs for WorldTools
                    if (WorldToolsParams.globalMode)
                    {

                    }
                    else if (WorldToolsParams.isGrassLayer)
                    {
                        maxParallelJobs = GL.massiveGrass.maxParallelJobCount;
                        GL.massiveGrass.maxParallelJobCount = 200;
                    }
                }
                else
                {
                    //TODO: Implement grass max parallel jobs for WorldTools
                    if (WorldToolsParams.globalMode)
                    {

                    }
                    else if (WorldToolsParams.isGrassLayer)
                        GL.massiveGrass.maxParallelJobCount = maxParallelJobs;
                }
            }
        }

        public static void MaskEditorSceneGUI()
        {
            try
            {
                if (!WorldToolsParams.isEditMode) return;
                //if (WorldToolsParams.globalMode && !WT.affectGPULayers && !WT.affectGrassLayers) return;

                if (terrain == null)
                {
                    TTerraWorldTerrainManager[] TTM = FindObjectsOfType<TTerraWorldTerrainManager>();
                    if (TTM != null && TTM.Length > 0 && TTM[0] != null) terrain = TTM[0].gameObject.GetComponent<Terrain>();
                }

                // Global Params
                WorldToolsParams.e = Event.current;
                RaycastHit hit;

                Vector2 mousePosition = new Vector2(WorldToolsParams.e.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - WorldToolsParams.e.mousePosition.y);

                // The following function works properly even if Windows Screen Scale & Zoom settings
                // is set to a value other than 100% (e.g. 125%)
                Ray ray = HandleUtility.GUIPointToWorldRay(WorldToolsParams.e.mousePosition);
                //Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);

                if (!Physics.Raycast(ray, out hit)) return;

                float terrainWidth = 2000;
                float terrainlength = 2000;
                Vector2 terrainPosition = Vector2.zero;
                int brushRadius = 300;
                float brushDensity = 100f;
                //float brushDamping = 7f;
                bool isolateLayer = true;

                if (WorldToolsParams.globalMode)
                {
                    terrainWidth = WT.Terrain.terrainData.size.x;
                    terrainlength = WT.Terrain.terrainData.size.z;
                    terrainPosition = new Vector2(WT.Terrain.transform.position.x, WT.Terrain.transform.position.z);
                    brushRadius = WT.brushRadius;
                    brushDensity = WT.brushDensity;
                    //brushDamping = WT.brushDamping;
                    isolateLayer = WT.isolateLayer;
                }
                else
                {
                    if (WorldToolsParams.isGPULayer)
                    {
                        terrainWidth = GIL.parameters.terrain.terrainData.size.x;
                        terrainlength = GIL.parameters.terrain.terrainData.size.z;
                        terrainPosition = new Vector2(GIL.parameters.terrain.transform.position.x, GIL.parameters.terrain.transform.position.z);
                        brushRadius = GIL.brushRadius;
                        brushDensity = GIL.brushDensity;
                        //brushDamping = GIL.brushDamping;
                        isolateLayer = GIL.isolateLayer;
                    }
                    else if (WorldToolsParams.isGrassLayer)
                    {
                        terrainWidth = terrain.terrainData.size.x;
                        terrainlength = terrain.terrainData.size.z;
                        terrainPosition = new Vector2(terrain.transform.position.x, terrain.transform.position.z);
                        brushRadius = GL.brushRadius;
                        brushDensity = GL.brushDensity;
                        //brushDamping = GL.brushDamping;
                        isolateLayer = GL.isolateLayer;
                    }
                }

                Vector3[] points = new Vector3[4];
                for (int i = 0; i < points.Length; i++) points[i] = Vector3.zero;
                int dataWidthCurrent = 1;
                int dataHeightCurrent = 1;

                if (WorldToolsParams.globalMode)
                {
                    // Calculate Brush data
                    dataWidthCurrent = WorldToolsParams.dataWidthBrush;
                    dataHeightCurrent = WorldToolsParams.dataHeightBrush;
                    float pixelSize = terrainWidth / dataWidthCurrent / 2;
                    WorldToolsParams.brushPixels = (int)Mathf.Clamp((brushRadius / pixelSize * 2) + 1, 1, 5);
                    WorldToolsParams.pixelSizeBrush = (float)brushRadius / (WorldToolsParams.brushPixels * 2.0f);
                    int pixelX = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainWidth, hit.point.x - terrainPosition.x - pixelSize) * dataWidthCurrent);
                    int pixelY = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainlength, hit.point.z - terrainPosition.y - pixelSize) * dataHeightCurrent);

                    WorldToolsParams.centerPos = new Vector3
                    (
                        (1.0f * pixelX * terrainWidth / dataWidthCurrent) + terrainPosition.x + pixelSize,
                        hit.point.y,
                        (1.0f * pixelY * terrainlength / dataHeightCurrent) + terrainPosition.y + pixelSize
                    );

                    // Calculate GPU Layers data
                    if (WT.maskDataListGPU != null)
                    {
                        if (WorldToolsParams.paintedPixelsGPU == null || WorldToolsParams.paintedPixelsGPU.Length == 0 || WorldToolsParams.paintedPixelsGPU.Length != WT.maskDataListGPU.Count) WorldToolsParams.paintedPixelsGPU = new List<Vector2>[WT.maskDataListGPU.Count];

                        for (int i = 0; i < WT.maskDataListGPU.Count; i++)
                        {
                            if (WorldToolsParams.paintedPixelsGPU[i] == null) WorldToolsParams.paintedPixelsGPU[i] = new List<Vector2>();
                            dataWidthCurrent = WT.maskDataListGPU[i].Length;
                            dataHeightCurrent = WT.maskDataListGPU[i][0].row.Length;
                            pixelSize = terrainWidth / dataWidthCurrent / 2;
                            pixelX = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainWidth, hit.point.x - terrainPosition.x - pixelSize) * dataWidthCurrent);
                            pixelY = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainlength, hit.point.z - terrainPosition.y - pixelSize) * dataHeightCurrent);
                            if (WorldToolsParams.painting || WorldToolsParams.erasing) WorldToolsParams.paintedPixelsGPU[i].Add(new Vector2(pixelX, pixelY));
                        }
                    }

                    // Calculate Grass Layers data
                    if (WT.maskDataListGrass != null)
                    {
                        if (WorldToolsParams.paintedPixelsGrass == null || WorldToolsParams.paintedPixelsGrass.Length == 0 || WorldToolsParams.paintedPixelsGrass.Length != WT.maskDataListGrass.Count) WorldToolsParams.paintedPixelsGrass = new List<Vector2>[WT.maskDataListGrass.Count];

                        for (int i = 0; i < WT.maskDataListGrass.Count; i++)
                        {
                            if (WorldToolsParams.paintedPixelsGrass[i] == null) WorldToolsParams.paintedPixelsGrass[i] = new List<Vector2>();
                            dataWidthCurrent = WT.maskDataListGrass[i].Length;
                            dataHeightCurrent = WT.maskDataListGrass[i][0].row.Length;
                            pixelSize = terrainWidth / dataWidthCurrent / 2;
                            pixelX = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainWidth, hit.point.x - terrainPosition.x - pixelSize) * dataWidthCurrent);
                            pixelY = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainlength, hit.point.z - terrainPosition.y - pixelSize) * dataHeightCurrent);
                            if (WorldToolsParams.painting || WorldToolsParams.erasing) WorldToolsParams.paintedPixelsGrass[i].Add(new Vector2(pixelX, pixelY));
                        }
                    }
                }
                else
                {
                    if (WorldToolsParams.paintedPixels == null) WorldToolsParams.paintedPixels = new List<Vector2>();

                    if (WorldToolsParams.isGPULayer)
                    {
                        dataWidthCurrent = GIL.parameters.maskDataFast.Length;
                        dataHeightCurrent = GIL.parameters.maskDataFast[0].row.Length;
                    }
                    else if (WorldToolsParams.isGrassLayer)
                    {
                        dataWidthCurrent = GL.MGP.maskDataFast.Length;
                        dataHeightCurrent = GL.MGP.maskDataFast[0].row.Length;
                    }

                    float pixelSize = terrainWidth / dataWidthCurrent / 2;
                    WorldToolsParams.brushPixels = (int)Mathf.Clamp((brushRadius / pixelSize * 2) + 1, 1, 5);
                    WorldToolsParams.pixelSizeBrush = (float)brushRadius / (WorldToolsParams.brushPixels * 2.0f);
                    int pixelX = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainWidth, hit.point.x - terrainPosition.x - pixelSize) * dataWidthCurrent);
                    int pixelY = Mathf.RoundToInt(Mathf.InverseLerp(0f, terrainlength, hit.point.z - terrainPosition.y - pixelSize) * dataHeightCurrent);
                    if (WorldToolsParams.painting || WorldToolsParams.erasing) WorldToolsParams.paintedPixels.Add(new Vector2(pixelX, pixelY));

                    WorldToolsParams.centerPos = new Vector3
                    (
                        (1.0f * pixelX * terrainWidth / dataWidthCurrent) + terrainPosition.x + pixelSize,
                        hit.point.y,
                        (1.0f * pixelY * terrainlength / dataHeightCurrent) + terrainPosition.y + pixelSize
                    );
                }

                Color color = Color.cyan;
                // If Shift is pressed and scroll wheel is not used, then we are erasing and gizmo color becomes red
                if (Tools.current != Tool.View && WorldToolsParams.e.shift) color = Color.red;

                for (int i = -WorldToolsParams.brushPixels; i < WorldToolsParams.brushPixels + 1; i++)
                {
                    for (int j = -WorldToolsParams.brushPixels; j < WorldToolsParams.brushPixels + 1; j++)
                    {
                        Vector3 offset = WorldToolsParams.centerPos + new Vector3(i * WorldToolsParams.pixelSizeBrush, 100000f, j * WorldToolsParams.pixelSizeBrush);
                        ray = new Ray(offset, Vector3.down);
                        color.a = 0.25f;
                        Handles.color = color;

                        if (Physics.Raycast(ray, out hit))
                        {
                            float distance = Mathf.Abs((new Vector2(offset.x, offset.z) - new Vector2(WorldToolsParams.centerPos.x, WorldToolsParams.centerPos.z)).magnitude);
                            if (distance > (brushRadius / 2.0f)) continue;

                            //Handles.DrawSolidArc(hit.point, hit.normal, Vector3.Cross(hit.normal, ray.direction), 360, pixelSizeBrush / 2f);
                            Handles.DrawWireCube(hit.point, Vector3.one * WorldToolsParams.pixelSizeBrush);

                            float posY = hit.point.y;
                            points[0] = new Vector3(hit.point.x - WorldToolsParams.pixelSizeBrush / 2f, posY, hit.point.z - WorldToolsParams.pixelSizeBrush / 2f);
                            points[1] = new Vector3(hit.point.x + WorldToolsParams.pixelSizeBrush / 2f, posY, hit.point.z - WorldToolsParams.pixelSizeBrush / 2f);
                            points[2] = new Vector3(hit.point.x + WorldToolsParams.pixelSizeBrush / 2f, posY, hit.point.z + WorldToolsParams.pixelSizeBrush / 2f);
                            points[3] = new Vector3(hit.point.x - WorldToolsParams.pixelSizeBrush / 2f, posY, hit.point.z + WorldToolsParams.pixelSizeBrush / 2f);

                            Handles.DrawAAConvexPolygon(points);
                            Handles.color = Color.white;
                        }
                    }
                }

                // Brush radius resize
                if (WorldToolsParams.e.shift && WorldToolsParams.e.rawType == EventType.ScrollWheel)
                {
                    if (WorldToolsParams.globalMode)
                        WT.brushRadius += (int)WorldToolsParams.e.delta.y;
                    else
                    {
                        if (WorldToolsParams.isGPULayer)
                            GIL.brushRadius += (int)WorldToolsParams.e.delta.y;
                        else if (WorldToolsParams.isGrassLayer)
                            GL.brushRadius += (int)WorldToolsParams.e.delta.y;
                    }

                    WorldToolsParams.e.Use();
                    //GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    WorldToolsParams.maskIsDirty = false;
                }

                if (Tools.current != Tool.View)
                {
                    // Detect Erasing events
                    if (WorldToolsParams.e.shift && WorldToolsParams.e.rawType == EventType.MouseDown)
                    {
                        if (WorldToolsParams.e.button == 0)
                        {
                            WorldToolsParams.e.Use();
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                            if (isolateLayer)
                            {
                                if (WorldToolsParams.globalMode)
                                {
                                    Transform isolatedObject = null;

                                    if (WorldToolsParams.isolatedIndexGPU > 0)
                                        isolatedObject = WT.GPULayers[WorldToolsParams.isolatedIndexGPU].transform.parent;
                                    else if (WorldToolsParams.isolatedIndexGrass > 0)
                                        isolatedObject = WT.grassLayers[WorldToolsParams.isolatedIndexGrass].transform;

                                    if (isolatedObject != null)
                                        IsolatePreview(isolatedObject, true, true);
                                }
                                else
                                {
                                    if (WorldToolsParams.isGPULayer)
                                        IsolatePreview(GIL.transform, true, false);
                                    else if (WorldToolsParams.isGrassLayer)
                                        IsolatePreview(GL.transform, false, true);
                                }
                            }

                            WorldToolsParams.painting = false;
                            WorldToolsParams.erasing = true;
                            WorldToolsParams.maskIsDirty = true;
                        }
                    }

                    // Detect Painting events
                    else if (WorldToolsParams.e.rawType == EventType.MouseDown)
                    {
                        if (WorldToolsParams.e.button == 0)
                        {
                            WorldToolsParams.e.Use();
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                            if (isolateLayer)
                            {
                                if (WorldToolsParams.globalMode)
                                {
                                    Transform isolatedObject = null;

                                    if (WorldToolsParams.isolatedIndexGPU > 0)
                                        isolatedObject = WT.GPULayers[WorldToolsParams.isolatedIndexGPU].transform.parent;
                                    else if (WorldToolsParams.isolatedIndexGrass > 0)
                                        isolatedObject = WT.grassLayers[WorldToolsParams.isolatedIndexGrass].transform;

                                    if (isolatedObject != null)
                                        IsolatePreview(isolatedObject, true, true);
                                }
                                else
                                {
                                    if (WorldToolsParams.isGPULayer)
                                        IsolatePreview(GIL.transform, true, false);
                                    else if (WorldToolsParams.isGrassLayer)
                                        IsolatePreview(GL.transform, false, true);
                                }
                            }

                            WorldToolsParams.painting = true;
                            WorldToolsParams.erasing = false;
                            WorldToolsParams.maskIsDirty = true;
                        }
                    }

                    // Since mask data is not serialized for faster binary read/write now, we cannot use Undo operations
                    // Perform updating layers with Undo (It is slow and waits for Mouse-Up event to update)
                    if ((WorldToolsParams.globalMode && WT.undoMode == 0) || (WorldToolsParams.isGPULayer && GIL.parameters.undoMode == 0) || (WorldToolsParams.isGrassLayer && GL.MGP.undoMode == 0))
                    {
                        if (WorldToolsParams.e.rawType == EventType.MouseUp && WorldToolsParams.maskIsDirty)
                        {
                            if (WorldToolsParams.globalMode)
                            {
                                lastFilters = new List<TScatterLayer.MaskDataFast[]>();

                                if (WT.maskDataListGPU != null)
                                {
                                    List<TScatterParams> GPUProfiles = new List<TScatterParams>();

                                    for (int i = 0; i < WT.GPULayers.Count; i++)
                                    {
                                        if (WT.editableGPU[i])
                                            if (WorldToolsParams.isolatedIndexGPU == -1 || WorldToolsParams.isolatedIndexGPU == i)
                                                GPUProfiles.Add(WT.GPULayers[i]);
                                    }

                                    if (WT.undoMode == 0 && GPUProfiles != null && GPUProfiles.Count > 0 && GPUProfiles[0] != null)
                                    {
                                        ShowProgress();
                                        Undo.RecordObjects(GPUProfiles.ToArray(), worldPaintingLayers);
                                    }

                                    for (int i = 0; i < WT.maskDataListGPU.Count; i++)
                                    {
                                        if (WorldToolsParams.isolatedIndexGPU != -1 && WorldToolsParams.isolatedIndexGPU != i) continue;
                                        if (!WT.editableGPU[i]) continue;

                                        WorldToolsParams.paintedPixelsGPU[i] = WorldToolsParams.paintedPixelsGPU[i].Distinct().ToList();

                                        if (WorldToolsParams.paintedPixelsGPU[i].Count > 0)
                                        {
                                            float grayscale = 1;
                                            if (WorldToolsParams.painting) grayscale = 1f;
                                            else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;

                                            for (int j = 0; j < WorldToolsParams.paintedPixelsGPU[i].Count; j++)
                                            {
                                                int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGPU[i][j].x);
                                                int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGPU[i][j].y);
                                                int dataWidth = WT.maskDataListGPU[i].Length;
                                                int dataHeight = WT.maskDataListGPU[i][0].row.Length;
                                                int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                                for (int x = (int)(-1.0f * radius); x < (radius * 1.0f + 1); x++)
                                                    for (int y = (int)(-1.0f * radius); y < (radius * 1.0f + 1); y++)
                                                        if (Mathf.Sqrt((x * x) + (y * y)) <= radius)
                                                        {
                                                            float random = Random.Range(0f, 100f);
                                                            if (brushDensity <= random) continue;
                                                            WT.maskDataListGPU[i][Mathf.Clamp(centerPixelX + x, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + y, 0, dataHeight - 1)] = grayscale;
                                                        }
                                            }
                                        }

                                        lastFilters.Add(WT.maskDataListGPU[i]);
                                        WorldToolsParams.paintedPixelsGPU[i].Clear();
                                    }
                                }

                                if (WT.maskDataListGrass != null)
                                {
                                    List<MassiveGrassProfile> grassProfiles = new List<MassiveGrassProfile>();

                                    for (int i = 0; i < WT.grassLayers.Count; i++)
                                    {
                                        if (WT.editableGrass[i])
                                            if (WorldToolsParams.isolatedIndexGrass == -1 || WorldToolsParams.isolatedIndexGrass == i)
                                                grassProfiles.Add(WT.grassLayers[i].MGP);
                                    }

                                    if (WT.undoMode == 0 && grassProfiles != null && grassProfiles.Count > 0 && grassProfiles[0] != null)
                                    {
                                        ShowProgress();
                                        Undo.RecordObjects(grassProfiles.ToArray(), worldPaintingLayers);
                                    }

                                    for (int i = 0; i < WT.maskDataListGrass.Count; i++)
                                    {
                                        if (WorldToolsParams.isolatedIndexGrass != -1 && WorldToolsParams.isolatedIndexGrass != i) continue;
                                        if (!WT.editableGrass[i]) continue;

                                        WorldToolsParams.paintedPixelsGrass[i] = WorldToolsParams.paintedPixelsGrass[i].Distinct().ToList();

                                        if (WorldToolsParams.paintedPixelsGrass[i].Count > 0)
                                        {
                                            float grayscale = 1;
                                            if (WorldToolsParams.painting) grayscale = 1f;
                                            else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;

                                            for (int j = 0; j < WorldToolsParams.paintedPixelsGrass[i].Count; j++)
                                            {
                                                int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGrass[i][j].x);
                                                int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGrass[i][j].y);
                                                int dataWidth = WT.maskDataListGrass[i].Length;
                                                int dataHeight = WT.maskDataListGrass[i][0].row.Length;
                                                int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                                for (int x = (int)(-1.0f * radius); x < (radius * 1.0f + 1); x++)
                                                    for (int y = (int)(-1.0f * radius); y < (radius * 1.0f + 1); y++)
                                                        if (Mathf.Sqrt((x * x) + (y * y)) <= radius)
                                                        {
                                                            float random = Random.Range(0f, 100f);
                                                            if (brushDensity <= random) continue;
                                                            WT.maskDataListGrass[i][Mathf.Clamp(centerPixelX + x, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + y, 0, dataHeight - 1)] = grayscale;
                                                        }
                                            }
                                        }

                                        lastFilters.Add(WT.maskDataListGrass[i]);
                                        WorldToolsParams.paintedPixelsGrass[i].Clear();
                                    }

                                    if (grassProfiles != null && grassProfiles.Count > 0 && grassProfiles[0] != null)
                                        for (int x = 0; x < grassProfiles.Count; x++)
                                            EditorUtility.SetDirty(grassProfiles[x]);
                                }

                                DetectTerrainChanges.SyncLayersWithTerrain(WorldToolsParams.e.mousePosition, true, true, lastFilters, progressID);
                            }
                            else
                            {
                                WorldToolsParams.paintedPixels = WorldToolsParams.paintedPixels.Distinct().ToList();

                                if (WorldToolsParams.paintedPixels.Count > 0)
                                {
                                    float grayscale = 1f;
                                    if (WorldToolsParams.painting) grayscale = 1f;
                                    else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;
                                    int dataWidth = 1;
                                    int dataHeight = 1;

                                    if (WorldToolsParams.isGPULayer)
                                    {
                                        if (GIL.parameters.undoMode == 0)
                                            Undo.RecordObject(GIL.parameters, worldPaintingLayerGPU);

                                        dataWidth = GIL.parameters.maskDataFast.Length;
                                        dataHeight = GIL.parameters.maskDataFast[0].row.Length;
                                    }
                                    else if (WorldToolsParams.isGrassLayer)
                                    {
                                        if (GL.MGP.undoMode == 0)
                                            Undo.RecordObject(GL.MGP, worldPaintingLayerGrass);

                                        dataWidth = GL.MGP.maskDataFast.Length;
                                        dataHeight = GL.MGP.maskDataFast[0].row.Length;
                                    }

                                    int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                    for (int i = 0; i < WorldToolsParams.paintedPixels.Count; i++)
                                    {
                                        int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixels[i].x);
                                        int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixels[i].y);

                                        for (int j = (int)(-1f * radius); j < (radius * 1f + 1); j++)
                                            for (int k = (int)(-1f * radius); k < (radius * 1f + 1); k++)
                                                if (Mathf.Sqrt((j * j) + (k * k)) <= radius)
                                                {
                                                    //TODO: Later we can give opacity to grayscale based on distance from center to give it a smooth falloff
                                                    float random = Random.Range(0f, 100f);
                                                    if (brushDensity <= random) continue;

                                                    if (WorldToolsParams.isGPULayer)
                                                        GIL.parameters.maskDataFast[Mathf.Clamp(centerPixelX + j, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + k, 0, dataHeight - 1)] = grayscale;
                                                    else if (WorldToolsParams.isGrassLayer)
                                                        GL.MGP.maskDataFast[Mathf.Clamp(centerPixelX + j, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + k, 0, dataHeight - 1)] = grayscale;
                                                }
                                    }

                                    if (WorldToolsParams.isGrassLayer)
                                        EditorUtility.SetDirty(GL.MGP);

                                    if (WorldToolsParams.isGPULayer)
                                    {
                                        DetectTerrainChanges.SyncLayerWithTerrain(WorldToolsParams.e.mousePosition, GIL.parameters.maskDataFast, progressID);
                                        lastMousePosition = WorldToolsParams.e.mousePosition;
                                        lastFilter = GIL.parameters.maskDataFast;
                                    }
                                    else if (WorldToolsParams.isGrassLayer)
                                    {
                                        DetectTerrainChanges.SyncLayerWithTerrain(WorldToolsParams.e.mousePosition, GL.MGP.maskDataFast, progressID);
                                        lastMousePosition = WorldToolsParams.e.mousePosition;
                                        lastFilter = GL.MGP.maskDataFast;
                                    }

                                    //TODO: Add Real-Time Mask Preview checkbox later so that user can decide whethere it should be activated or not!
                                    // Temporarily dissabled due to heavy operation
                                    //ConvertMaskDataToImage(lastFilter);

                                    WorldToolsParams.paintedPixels.Clear();
                                }
                            }

                            if (isolateLayer)
                            {
                                if (WorldToolsParams.globalMode)
                                {
                                    Transform isolatedObject = null;

                                    if (WorldToolsParams.isolatedIndexGPU > 0)
                                        isolatedObject = WT.GPULayers[WorldToolsParams.isolatedIndexGPU].transform.parent;
                                    else if (WorldToolsParams.isolatedIndexGrass > 0)
                                        isolatedObject = WT.grassLayers[WorldToolsParams.isolatedIndexGrass].transform;

                                    if (isolatedObject != null)
                                        RevertPreview(isolatedObject, true, true);
                                }
                                else
                                {
                                    if (WorldToolsParams.isGPULayer)
                                        RevertPreview(GIL.transform, true, false);
                                    else if (WorldToolsParams.isGrassLayer)
                                        RevertPreview(GL.transform, false, true);
                                }
                            }

                            Undo.FlushUndoRecordObjects();

                            WorldToolsParams.painting = false;
                            WorldToolsParams.erasing = false;
                            WorldToolsParams.maskIsDirty = false;

                            // Needed to update and refresh mask references
                            RefreshLayers();
                        }
                    }
                    // Perform updating layers without Undo for real-time and faster experience
                    else
                    {
                        if (WorldToolsParams.e.rawType == EventType.MouseDrag)
                        {
                            if (WorldToolsParams.e.button != 0) return;

                            if (WorldToolsParams.globalMode)
                            {
                                lastFilters = new List<TScatterLayer.MaskDataFast[]>();

                                if (WT.maskDataListGPU != null)
                                {
                                    for (int i = 0; i < WT.maskDataListGPU.Count; i++)
                                    {
                                        if (WorldToolsParams.isolatedIndexGPU != -1 && WorldToolsParams.isolatedIndexGPU != i) continue;
                                        if (!WT.editableGPU[i]) continue;

                                        WorldToolsParams.paintedPixelsGPU[i] = WorldToolsParams.paintedPixelsGPU[i].Distinct().ToList();

                                        if (WorldToolsParams.paintedPixelsGPU[i].Count > 0)
                                        {
                                            float grayscale = 1;
                                            if (WorldToolsParams.painting) grayscale = 1f;
                                            else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;

                                            for (int j = 0; j < WorldToolsParams.paintedPixelsGPU[i].Count; j++)
                                            {
                                                int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGPU[i][j].x);
                                                int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGPU[i][j].y);
                                                int dataWidth = WT.maskDataListGPU[i].Length;
                                                int dataHeight = WT.maskDataListGPU[i][0].row.Length;
                                                int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                                for (int x = (int)(-1.0f * radius); x < (radius * 1.0f + 1); x++)
                                                    for (int y = (int)(-1.0f * radius); y < (radius * 1.0f + 1); y++)
                                                        if (Mathf.Sqrt((x * x) + (y * y)) <= radius)
                                                        {
                                                            float random = Random.Range(0f, 100f);
                                                            if (brushDensity <= random) continue;
                                                            WT.maskDataListGPU[i][Mathf.Clamp(centerPixelX + x, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + y, 0, dataHeight - 1)] = grayscale;
                                                        }
                                            }
                                        }

                                        lastFilters.Add(WT.maskDataListGPU[i]);
                                    }
                                }

                                if (WT.maskDataListGrass != null)
                                {
                                    for (int i = 0; i < WT.maskDataListGrass.Count; i++)
                                    {
                                        if (WorldToolsParams.isolatedIndexGrass != -1 && WorldToolsParams.isolatedIndexGrass != i) continue;
                                        if (!WT.editableGrass[i]) continue;

                                        WorldToolsParams.paintedPixelsGrass[i] = WorldToolsParams.paintedPixelsGrass[i].Distinct().ToList();

                                        if (WorldToolsParams.paintedPixelsGrass[i].Count > 0)
                                        {
                                            float grayscale = 1;
                                            if (WorldToolsParams.painting) grayscale = 1f;
                                            else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;

                                            for (int j = 0; j < WorldToolsParams.paintedPixelsGrass[i].Count; j++)
                                            {
                                                int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGrass[i][j].x);
                                                int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixelsGrass[i][j].y);
                                                int dataWidth = WT.maskDataListGrass[i].Length;
                                                int dataHeight = WT.maskDataListGrass[i][0].row.Length;
                                                int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                                for (int x = (int)(-1.0f * radius); x < (radius * 1.0f + 1); x++)
                                                    for (int y = (int)(-1.0f * radius); y < (radius * 1.0f + 1); y++)
                                                        if (Mathf.Sqrt((x * x) + (y * y)) <= radius)
                                                        {
                                                            float random = Random.Range(0f, 100f);
                                                            if (brushDensity <= random) continue;
                                                            WT.maskDataListGrass[i][Mathf.Clamp(centerPixelX + x, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + y, 0, dataHeight - 1)] = grayscale;
                                                        }
                                            }
                                        }

                                        lastFilters.Add(WT.maskDataListGrass[i]);
                                    }
                                }

                                DetectTerrainChanges.SyncLayersWithTerrain(WorldToolsParams.e.mousePosition, true, true, lastFilters, progressID, false, false);
                            }
                            else
                            {
                                WorldToolsParams.paintedPixels = WorldToolsParams.paintedPixels.Distinct().ToList();

                                if (WorldToolsParams.paintedPixels.Count > 0)
                                {
                                    float grayscale = 1f;
                                    if (WorldToolsParams.painting) grayscale = 1f;
                                    else if (WorldToolsParams.erasing) grayscale = -1 * float.Epsilon;
                                    int dataWidth = 1;
                                    int dataHeight = 1;

                                    if (WorldToolsParams.isGPULayer)
                                    {
                                        dataWidth = GIL.parameters.maskDataFast.Length;
                                        dataHeight = GIL.parameters.maskDataFast[0].row.Length;
                                    }
                                    else if (WorldToolsParams.isGrassLayer)
                                    {
                                        dataWidth = GL.MGP.maskDataFast.Length;
                                        dataHeight = GL.MGP.maskDataFast[0].row.Length;
                                    }

                                    int radius = (int)(brushRadius * dataWidth * 0.5f / terrainWidth);

                                    for (int i = 0; i < WorldToolsParams.paintedPixels.Count; i++)
                                    {
                                        int centerPixelX = Mathf.CeilToInt(WorldToolsParams.paintedPixels[i].x);
                                        int centerPixelY = Mathf.CeilToInt(WorldToolsParams.paintedPixels[i].y);

                                        for (int j = (int)(-1f * radius); j < (radius * 1f + 1); j++)
                                            for (int k = (int)(-1f * radius); k < (radius * 1f + 1); k++)
                                                if (Mathf.Sqrt((j * j) + (k * k)) <= radius)
                                                {
                                                    //TODO: Later we can give opacity to grayscale based on distance from center to give it a smooth falloff
                                                    float random = Random.Range(0f, 100f);
                                                    if (brushDensity <= random) continue;

                                                    if (WorldToolsParams.isGPULayer)
                                                        GIL.parameters.maskDataFast[Mathf.Clamp(centerPixelX + j, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + k, 0, dataHeight - 1)] = grayscale;
                                                    else if (WorldToolsParams.isGrassLayer)
                                                        GL.MGP.maskDataFast[Mathf.Clamp(centerPixelX + j, 0, dataWidth - 1)].row[Mathf.Clamp(centerPixelY + k, 0, dataHeight - 1)] = grayscale;
                                                }
                                    }

                                    if (WorldToolsParams.isGPULayer)
                                        DetectTerrainChanges.SyncLayerWithTerrain(WorldToolsParams.e.mousePosition, GIL.parameters.maskDataFast, progressID, false, false);
                                    else if (WorldToolsParams.isGrassLayer)
                                        DetectTerrainChanges.SyncLayerWithTerrain(WorldToolsParams.e.mousePosition, GL.MGP.maskDataFast, progressID, false, false);
                                }
                            }
                        }

                        // Detect Mouse-Up (finalization) events
                        if (WorldToolsParams.e.rawType == EventType.MouseUp && WorldToolsParams.maskIsDirty)
                        {
                            if (WorldToolsParams.globalMode)
                            {
                                if (WT.maskDataListGPU != null)
                                    for (int i = 0; i < WT.maskDataListGPU.Count; i++)
                                        WorldToolsParams.paintedPixelsGPU[i].Clear();

                                // Update GPU Layers' data files
                                TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

                                if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                                {
                                    foreach (TScatterParams g in GPULayers)
                                    {
                                        if (lastFilters != null && !lastFilters.Contains(g.maskDataFast)) continue;
                                        g.SaveMaskData();
                                        g.SaveDataBuffer();
                                    }
                                }

                                if (WT.maskDataListGrass != null)
                                {
                                    for (int i = 0; i < WT.maskDataListGrass.Count; i++)
                                        WorldToolsParams.paintedPixelsGrass[i].Clear();

                                    // Update Grass Layers' data files
                                    List<MassiveGrassProfile> grassProfiles = new List<MassiveGrassProfile>();

                                    for (int i = 0; i < WT.grassLayers.Count; i++)
                                    {
                                        if (WT.editableGrass[i])
                                            if (WorldToolsParams.isolatedIndexGrass == -1 || WorldToolsParams.isolatedIndexGrass == i)
                                            {
                                                if (lastFilters != null && !lastFilters.Contains(WT.grassLayers[i].MGP.maskDataFast)) continue;
                                                grassProfiles.Add(WT.grassLayers[i].MGP);
                                                WT.grassLayers[i].SaveMaskData();
                                            }
                                    }

                                    if (grassProfiles != null && grassProfiles.Count > 0 && grassProfiles[0] != null)
                                        for (int x = 0; x < grassProfiles.Count; x++)
                                            EditorUtility.SetDirty(grassProfiles[x]);
                                }
                            }
                            else
                            {
                                if (WorldToolsParams.isGPULayer)
                                {
                                    lastMousePosition = WorldToolsParams.e.mousePosition;

                                    // Update GPU Layer's data files
                                    lastFilter = GIL.parameters.maskDataFast;
                                    GIL.parameters.SaveMaskData();
                                    GIL.parameters.SaveDataBuffer();
                                }
                                else if (WorldToolsParams.isGrassLayer)
                                {
                                    lastMousePosition = WorldToolsParams.e.mousePosition;

                                    // Update Grass Layer's data files
                                    lastFilter = GL.MGP.maskDataFast;
                                    GL.SaveMaskData();

                                    EditorUtility.SetDirty(GL.MGP);
                                }

                                //TODO: Add Real-Time Mask Preview checkbox later so that user can decide whethere it should be activated or not!
                                // Temporarily disabled due to heavy operation
                                //ConvertMaskDataToImage(lastFilter);

                                WorldToolsParams.paintedPixels.Clear();
                            }

                            if (isolateLayer)
                            {
                                if (WorldToolsParams.globalMode)
                                {
                                    Transform isolatedObject = null;

                                    if (WorldToolsParams.isolatedIndexGPU > 0)
                                        isolatedObject = WT.GPULayers[WorldToolsParams.isolatedIndexGPU].transform.parent;
                                    else if (WorldToolsParams.isolatedIndexGrass > 0)
                                        isolatedObject = WT.grassLayers[WorldToolsParams.isolatedIndexGrass].transform;

                                    if (isolatedObject != null)
                                        RevertPreview(isolatedObject, true, true);
                                }
                                else
                                {
                                    if (WorldToolsParams.isGPULayer)
                                        RevertPreview(GIL.transform, true, false);
                                    else if (WorldToolsParams.isGrassLayer)
                                        RevertPreview(GL.transform, false, true);
                                }
                            }

                            WorldToolsParams.painting = false;
                            WorldToolsParams.erasing = false;
                            WorldToolsParams.maskIsDirty = false;

                            // Needed to update and refresh mask references
                            RefreshLayers();
                        }
                    }
                }
            }
#if TERRAWORLD_DEBUG
            catch (System.Exception e)
            {
                throw e;
            }
#else
            catch { }
#endif
        }

        public static void DetectPaintingChanges()
        {
            if (Undo.GetCurrentGroupName() == worldPaintingLayers)
                DetectTerrainChanges.SyncLayersWithTerrain(true, true, lastFilters, progressID);

            if (Undo.GetCurrentGroupName() == worldPaintingLayerGPU || Undo.GetCurrentGroupName() == worldPaintingLayerGrass)
                DetectTerrainChanges.SyncLayerWithTerrain(lastMousePosition, lastFilter, progressID);
        }

        public static void IsolatePreview(Transform exception, bool isGPU, bool isGrass, bool affectGrassLayers = false)
        {
            if (isGPU)
            {
                WorldToolsParams.cachedStatesGPU = new List<bool>();

                foreach (GPUInstanceLayer t in Resources.FindObjectsOfTypeAll(typeof(GPUInstanceLayer)) as GPUInstanceLayer[])
                {
                    if (t != null && t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                    {
                        if (exception != null && t.transform == exception) continue;
                        WorldToolsParams.cachedStatesGPU.Add(t.gameObject.activeSelf);
                        t.gameObject.SetActive(false);
                    }
                }
            }

            // Disabled grass layers' isolation by default for faster and better visuals while editing
            if (affectGrassLayers && isGrass)
            {
                GrassLayer[] grassLayers = Resources.FindObjectsOfTypeAll(typeof(GrassLayer)) as GrassLayer[];
                if (grassLayers == null || grassLayers.Length == 0 || grassLayers[0] == null) return;
                WorldToolsParams.cachedStatesGrass = new List<bool>();

                foreach (GrassLayer t in grassLayers)
                {
                    if (t != null && t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                    {
                        if (exception != null && t.transform == exception) continue;
                        WorldToolsParams.cachedStatesGrass.Add(t.gameObject.activeSelf);
                        t.gameObject.SetActive(false);
                    }
                }
            }
        }

        public static void RevertPreview(Transform exception, bool isGPU, bool isGrass, bool affectGrassLayers = false)
        {
            if (isGPU)
            {
                int index = 0;

                foreach (GPUInstanceLayer t in Resources.FindObjectsOfTypeAll(typeof(GPUInstanceLayer)) as GPUInstanceLayer[])
                    if (t != null && t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                    {
                        if (exception != null && t.transform == exception) continue;
                        t.gameObject.SetActive(WorldToolsParams.cachedStatesGPU[index++]);
                    }
            }

            // Disabled grass layers' revert by default for faster and better visuals while editing
            if (affectGrassLayers && isGrass)
            {
                int index = 0;
                GrassLayer[] grassLayers = Resources.FindObjectsOfTypeAll(typeof(GrassLayer)) as GrassLayer[];
                if (grassLayers == null || grassLayers.Length == 0 || grassLayers[0] == null) return;

                foreach (GrassLayer t in grassLayers)
                    if (t != null && t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                    {
                        if (exception != null && t.transform == exception) continue;
                        t.gameObject.SetActive(WorldToolsParams.cachedStatesGrass[index]);
                        index++;
                    }
            }
        }

        private static void SetLayerMaskForAllLayers(int layerMask, bool affectGrassLayers)
        {
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

            if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                foreach (TScatterParams g in GPULayers)
                    g.unityLayerMask = layerMask;

            List<GrassLayer> GrassLayers = new List<GrassLayer>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go.GetComponent<GrassLayer>() != null)
                        GrassLayers.Add(go.GetComponent<GrassLayer>());

            if (GrassLayers != null && GrassLayers.Count > 0 && GrassLayers[0] != null)
                for (int i = 0; i < GrassLayers.Count; i++)
                    if (GrassLayers[i].MGP != null)
                    {
                        if (affectGrassLayers)
                        {
                            GrassLayers[i].MGP.layerBasedPlacement = true;
                            GrassLayers[i].MGP.unityLayerMask = layerMask;
                        }
                        else
                            GrassLayers[i].MGP.layerBasedPlacement = false;
                    }
        }

        public static void SyncAllLayers()
        {
            DetectTerrainChanges.SyncLayersWithTerrain();
        }

        public static void SyncAllLayersWithLayerMask(int layerMask, bool affectGrassLayers)
        {
            SetLayerMaskForAllLayers(layerMask, affectGrassLayers);
            DetectTerrainChanges.SyncLayersWithTerrain();
        }

        public static void SyncDelayedGPU()
        {
            WorldToolsParams.syncAllGPU = true;
            WorldToolsParams.lastEditorUpdateTime = Time.realtimeSinceStartup;
            WorldToolsParams.applySyncing = true;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void SyncDelayedGrass()
        {
            WorldToolsParams.syncAllGrass = true;
            WorldToolsParams.lastEditorUpdateTime = Time.realtimeSinceStartup;
            WorldToolsParams.applySyncing = true;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void RefreshLayers()
        {
            WorldToolsEditor.refresh = true;
        }

        public static void RemoveLayer(GameObject go)
        {
            if (EditorUtility.DisplayDialog("REMOVE LAYER", "Are you sure you want to remove this layer?\n\nNote: You can retrieve this layer if \"Ctrl + Z\" is pressed.", "Yes", "No"))
            {
                Undo.DestroyObjectImmediate(go);
                RefreshLayers();
                SceneView.RepaintAll();
            }
        }

        public static void DuplicateLayer(GameObject go)
        {
            if (EditorUtility.DisplayDialog("DUPLICATE LAYER", "Are you sure you want to duplicate this layer?\n\nThis will create an exact copy of the layer in the scene hierarchy!", "Yes", "No"))
            {
                GameObject duplicatedGO = Instantiate(go);
                Undo.RegisterCreatedObjectUndo(duplicatedGO, "Duplicated TerraWorld Layer");
                duplicatedGO.name = go.name + " Clone";
                duplicatedGO.transform.parent = go.transform.parent;
                duplicatedGO.transform.localPosition = go.transform.localPosition;
                if (duplicatedGO.transform.GetChild(0) != null) duplicatedGO.transform.GetChild(0).hideFlags = HideFlags.HideInHierarchy;
                RefreshLayers();
                SceneView.RepaintAll();
            }
        }

        private static void OnSceneGUI(SceneView scnView)
        {
            MaskEditorSceneGUI();
        }

        public static void OnEditorUpdate()
        {
            if (WorldToolsParams.applySyncing && Time.realtimeSinceStartup - WorldToolsParams.lastEditorUpdateTime > WorldToolsParams.syncDelay)
            {
                RefreshLayers();
                List<TScatterLayer.MaskDataFast[]> filters = new List<TScatterLayer.MaskDataFast[]>();

                for (int i = 0; i < WT.maskDataListGPU.Count; i++)
                    if (WT.editableGPU[i])
                        filters.Add(WT.maskDataListGPU[i]);

                for (int i = 0; i < WT.maskDataListGrass.Count; i++)
                    if (WT.editableGrass[i])
                        filters.Add(WT.maskDataListGrass[i]);

                DetectTerrainChanges.SyncLayersWithTerrain(WorldToolsParams.syncAllGPU, WorldToolsParams.syncAllGrass, filters, progressID);
                WorldToolsParams.syncAllGPU = false;
                WorldToolsParams.syncAllGrass = false;
                WorldToolsParams.applySyncing = false;
            }
        }

        private static void ShowProgress()
        {
            progressID = TProgressBar.StartProgressBar("WORLD TOOLS", "Registering Undo operation", TProgressBar.ProgressOptionsList.Indefinite, false);
            TProgressBar.DisplayProgressBar("WORLD TOOLS", "Registering Undo operation", 0.5f, progressID);
        }

        // This is a heavy operation so avoid using if not needed!
        public static void ConvertMaskDataToImage(TScatterLayer.MaskDataFast[] maskData)
        {
            int progressMinResolution = 4096;
            int maskResolution = maskData.GetLength(0);
            WorldToolsParams.maskImage = new Texture2D(maskResolution, maskResolution, TextureFormat.RGBA32, false);
            if (maskResolution > progressMinResolution) progressID = TProgressBar.StartProgressBar("WORLD TOOLS", "Converting Mask Data To Raster Image", TProgressBar.ProgressOptionsList.None, false);

            for (int i = 0; i < maskResolution; i++)
                for (int j = 0; j < maskResolution; j++)
                {
                    float grayscale = maskData[i].row[j];
                    WorldToolsParams.maskImage.SetPixel(i, j, new Color(grayscale, grayscale, grayscale, 1));

                    if (maskResolution > progressMinResolution && i % 1024 == 0)
                        TProgressBar.DisplayProgressBar("WORLD TOOLS", "Converting Mask Data To Raster Image", (float)(i + 1) / (float)maskResolution, progressID);
                }

            WorldToolsParams.maskImage.Apply();
            if (GIL != null) EditorUtility.SetDirty(GIL);
            if (GL != null) EditorUtility.SetDirty(GL);
            if (maskResolution > progressMinResolution) TProgressBar.RemoveProgressBar(progressID);
        }

        public static void ConvertImageToMaskData(ref TScatterLayer.MaskDataFast[] maskData)
        {
            int maskResolution = WorldToolsParams.maskImage.width;
            maskData = new TScatterLayer.MaskDataFast[maskResolution];

            for (int i = 0; i < maskResolution; i++)
            {
                maskData[i].row = new float[maskResolution];

                for (int j = 0; j < maskResolution; j++)
                    maskData[i].row[j] = WorldToolsParams.maskImage.GetPixel(i, j).r;
            }
        }

        public static void WarmUpMask(Texture2D mask)
        {
            if (mask == null) return;
            string imagePath = AssetDatabase.GetAssetPath(mask);
            TextureImporter imageImport = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            imageImport.isReadable = true;
            imageImport.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        public static void ExportMask()
        {
            string filepath = EditorUtility.SaveFilePanel("Select a folder to save mask image", Application.dataPath, "Layer Mask", "jpg");

            if (!string.IsNullOrEmpty(filepath))
            {
                byte[] bytes = WorldToolsParams.maskImage.EncodeToJPG();
                File.WriteAllBytes(filepath, bytes);
                AssetDatabase.ImportAsset(filepath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
                WarmUpMask(AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(filepath), typeof(Texture2D)) as Texture2D);
                //                WarmUpMask(AssetDatabase.LoadAssetAtPath(filepath.Substring(filepath.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D);
            }
        }

        public static void ExportMask(Texture2D mask)
        {
            string filepath = EditorUtility.SaveFilePanel("Select a folder to save mask image", Application.dataPath, "Layer Mask", "jpg");

            if (!string.IsNullOrEmpty(filepath))
            {
                byte[] bytes = mask.EncodeToJPG();
                File.WriteAllBytes(filepath, bytes);
                AssetDatabase.ImportAsset(filepath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
                WarmUpMask(AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(filepath), typeof(Texture2D)) as Texture2D);
            }
        }

        public static void GoToEditorWorld()
        {
            if (TTerraWorldManager.IsMainTerraworldGameObject == null) return;

            EditorGUIUtility.PingObject(TTerraWorldManager.IsMainTerraworldGameObject);
            Selection.activeObject = TTerraWorldManager.IsMainTerraworldGameObject;
            System.Type windowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorWindow window = EditorWindow.GetWindow(windowType);
            window.Focus();
        }

        public static void GoToEditorLayer(GameObject layerGO)
        {
            if (layerGO == null) return;

            EditorGUIUtility.PingObject(layerGO);
            Selection.activeObject = layerGO;
            System.Type windowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorWindow window = EditorWindow.GetWindow(windowType);
            window.Focus();
        }



        //private string prefabName;
        //private int seedNo;
        //private float averageDistance = 10f;
        //private int gridResolution = 100;
        //private bool rotation90Degrees = false;
        //private bool lockYRotation = false;
        //private bool getSurfaceAngle = false;
        //private float minRotationRange = 0f;
        //private float maxRotationRange = 359f;
        //private float positionVariation = 100f;
        //private Vector3 scaleMultiplier = Vector3.one;
        //private float minScale = 0.8f;
        //private float maxScale = 1.5f;
        //private string unityLayerName = "Default";
        //private int maskLayer = ~0;
        //private string layerName = "GPU Instance Scatter";
        //private float minSlope = 0;
        //private float maxSlope = 90;
        //private Vector3 positionOffset = Vector3.zero;
        //private Vector3 rotationOffset = Vector3.zero;
        //private int priority = 0;
        //private bool receiveShadows = true;
        //private bool bypassLakes = true;
        //private bool underLakes = false;
        //private bool underLakesMask = false;
        //private bool onLakes = false;
        //private TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        //private float LODMultiplier = 1.0f;
        //private float maxDistance = 2000f;
        //private float frustumMultiplier = 1.1f;
        //private bool checkBoundingBox = false;
        //private float maxElevation = 100000;
        //private float minElevation = -100000;
        //private bool occlusionCulling = true;
        //private int maskResolution = 2048;
        //
        //public static void AddLayer()
        //{
        //    //TODO: Add a prefab slot in inspector and when it is filled, then continue with the following lines
        //
        //    System.Random rand = new System.Random();
        //    seedNo = rand.Next();
        //    GameObject instanceLayer = new GameObject("New Layer");
        //    instanceLayer.transform.parent = WT.terrain.transform;
        //
        //    GameObject dataHanlder = new GameObject(layerName);
        //    dataHanlder.transform.parent = instanceLayer.transform;
        //    TScatterParams terraLayerManager = dataHanlder.AddComponent<TScatterParams>();
        //    dataHanlder.hideFlags = HideFlags.HideInHierarchy;
        //    instanceLayer.AddComponent<GPUInstanceLayer>();
        //
        //    terraLayerManager.averageDistance = averageDistance;
        //    terraLayerManager.scale = scaleMultiplier;
        //    terraLayerManager.minScale = minScale;
        //    terraLayerManager.maxScale = maxScale;
        //    terraLayerManager.positionVariation = positionVariation;
        //    terraLayerManager.lock90DegreeRotation = rotation90Degrees;
        //    terraLayerManager.lockYRotation = lockYRotation;
        //    terraLayerManager.getSurfaceAngle = getSurfaceAngle;
        //    terraLayerManager.seedNo = seedNo;
        //    terraLayerManager.priority = priority;
        //    terraLayerManager.unityLayerName = unityLayerName;
        //    terraLayerManager.unityLayerMask = maskLayer;
        //    terraLayerManager.positionVariation = positionVariation;
        //    terraLayerManager.positionVariation = positionVariation;
        //    terraLayerManager.positionOffset = positionOffset;
        //    terraLayerManager.rotationOffset = rotationOffset;
        //    terraLayerManager.minRotationRange = minRotationRange;
        //    terraLayerManager.maxRotationRange = maxRotationRange;
        //    terraLayerManager.minAllowedAngle = minSlope;
        //    terraLayerManager.maxAllowedAngle = maxSlope;
        //    terraLayerManager.minAllowedHeight = minElevation;
        //    terraLayerManager.maxAllowedHeight = maxElevation;
        //    terraLayerManager.shadowCastMode = (UnityEngine.Rendering.ShadowCastingMode)shadowCastingMode;
        //    terraLayerManager.receiveShadows = receiveShadows;
        //    terraLayerManager.bypassLake = bypassLakes;
        //    terraLayerManager.underLake = underLakes;
        //    terraLayerManager.underLakeMask = underLakesMask;
        //    terraLayerManager.onLake = onLakes;
        //    terraLayerManager.maxDistance = maxDistance;
        //    terraLayerManager.LODMultiplier = LODMultiplier;
        //    terraLayerManager.maxDistance = maxDistance;
        //    terraLayerManager.gridResolution = gridResolution;
        //    terraLayerManager.frustumMultiplier = frustumMultiplier;
        //    terraLayerManager.checkBoundingBox = checkBoundingBox;
        //    terraLayerManager.occlusionCulling = occlusionCulling;
        //
        //    terraLayerManager.maskData = new TScatterLayer.maskDataFast[maskResolution];
        //
        //    for (int i = 0; i < maskResolution; i++)
        //    {
        //        terraLayerManager.maskDataFast[i].row = new float[maskResolution];
        //
        //        for (int j = 0; j < maskResolution; j++)
        //            terraLayerManager.maskDataFast[i].row[j] = -1 * float.Epsilon;
        //    }
        //
        //    terraLayerManager.SetPrefabWithoutUpdatePatches(AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject)) as GameObject);
        //}
    }
}
#endif

