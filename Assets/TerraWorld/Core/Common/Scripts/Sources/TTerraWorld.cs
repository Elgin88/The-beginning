#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TerraUnity.Runtime;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
using XNode;
#endif

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum EventCategory
    {
        UX,
        Params,
        Nodes,
        Templates,
        SystemInfo,
        SoftwareInfo
    }

    public enum EventAction
    {
        Click,
        Version,
        Uses,
        OperatingSystem,
        DeviceType,
        SystemMemorySize,
        graphicsMemorySize,
        UnityVersion,
        Platform
    }

    public class TTerraWorld
    {
        public List<TTerrain> _terrains;
        private Action<TTerraWorld> _lastActions;
        private int _terrainsDone = 0;
        public static string _ID;
        private static int Counter;
        public static string TemplateName;
        private static bool? _feedbackSystem;

        public static bool FeedbackSystem
        {
            set
            {
                _feedbackSystem = value;
                TProjectSettings.FeedbackSystem = value;
            }
            get
            {
                if (!_feedbackSystem.HasValue)
                    _feedbackSystem = TProjectSettings.FeedbackSystem;
                return _feedbackSystem.Value;
            }
        }

        private static TArea _worldArea = null;
        public static TArea WorldArea 
        {
            get 
            {
                  if (_worldArea == null)
                  {
                      TProjectSettings.BBoxPoints bBoxPoints = TProjectSettings.ProjectArea;
                      _worldArea = new TArea(bBoxPoints.Top, bBoxPoints.Left, bBoxPoints.Bottom, bBoxPoints.Right);
                  }
                  
                  if (_worldArea.Dirty)
                  {
                      TProjectSettings.BBoxPoints bBoxPoints2;
                      bBoxPoints2.Top = _worldArea.Top;
                      bBoxPoints2.Left = _worldArea.Left;
                      bBoxPoints2.Bottom = _worldArea.Bottom;
                      bBoxPoints2.Right = _worldArea.Right;
                      TProjectSettings.ProjectArea = bBoxPoints2;
                      _worldArea.Clear();
                  }
                
                  return _worldArea; 
            }
        }

        public static void SetWorldArea(double latitude, double longitude, float AreaSizeLat, float AreaSizeLon)
        {
            if (_worldArea == null)
                _worldArea = new TArea(latitude, longitude, AreaSizeLat, AreaSizeLon);
            else
                _worldArea.UpdateCenterArea(latitude, longitude, AreaSizeLat, AreaSizeLon, _worldArea.ShapeMode);

            TProjectSettings.BBoxPoints bBoxPoints2;
            bBoxPoints2.Top = _worldArea.Top;
            bBoxPoints2.Left = _worldArea.Left;
            bBoxPoints2.Bottom = _worldArea.Bottom;
            bBoxPoints2.Right = _worldArea.Right;
            TProjectSettings.ProjectArea = bBoxPoints2;
            TTerraWorldManager.SaveOldGraph();
        }


        public static void SetWorldArea(double Top, double Left, double Bottom, double Right)
        {
            if (_worldArea == null)
                _worldArea = new TArea(Top, Left, Bottom, Right);
            else
                _worldArea.UpdateTLBR(Top, Left, Bottom, Right);

            TProjectSettings.BBoxPoints bBoxPoints2;
            bBoxPoints2.Top = _worldArea.Top;
            bBoxPoints2.Left = _worldArea.Left;
            bBoxPoints2.Bottom = _worldArea.Bottom;
            bBoxPoints2.Right = _worldArea.Right;
            TProjectSettings.ProjectArea = bBoxPoints2;
            TTerraWorldManager.SaveOldGraph();
        }


        public static TTerraWorldGraph WorldGraph { get => TTerraWorldManager.WorldGraph; }

        private static int _messageCounter = 0;
        public static int MessageCounter { get => _messageCounter; }

        private static string _newVersionWebAddress = "";
        public static string NewVersionWebAddress { get => _newVersionWebAddress; set => _newVersionWebAddress = value; }

        private TMapManager.ImageryMapServer _imagerySource = TMapManager.ImageryMapServer.ESRI;
        public TMapManager.ImageryMapServer ImagerySource { get => _imagerySource; set => _imagerySource = value; }

        private TMapManager.ElevationMapServer _elevationSource = TMapManager.ElevationMapServer.ESRI;
        public TMapManager.ElevationMapServer ElevationSource { get => _elevationSource; set => _elevationSource = value; }

        private TMapManager.LandcoverMapServer _landcoverSource = TMapManager.LandcoverMapServer.OSM;
        public TMapManager.LandcoverMapServer LandcoverSource { get => _landcoverSource; set => _landcoverSource = value; }

        public static void FeedbackSystemInfo()
        {
            FeedbackEvent(EventCategory.SystemInfo, EventAction.OperatingSystem, UnityEngine.SystemInfo.operatingSystem.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.DeviceType, UnityEngine.SystemInfo.deviceType.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.SystemMemorySize, UnityEngine.SystemInfo.systemMemorySize.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.graphicsMemorySize, UnityEngine.SystemInfo.graphicsMemorySize.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.UnityVersion, UnityEngine.Application.unityVersion.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.Platform, UnityEngine.Application.platform.ToString());
        }

        public static void FeedbackEvent(EventCategory EventCategory, EventAction EventAction, string label, int? value = null)
        {
            if (FeedbackSystem)
            {
                if (value != null)
                    TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction.ToString(), label + "_" + value.ToString());
                else
                    TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction.ToString(), label.ToString());
            }
        }

        public static void FeedbackEvent(EventCategory EventCategory, string EventAction, int value)
        {
            if (FeedbackSystem)
                TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction, value.ToString());
        }

        public static void FeedbackEvent(EventCategory EventCategory, string EventAction, string value)
        {
            if (FeedbackSystem)
                TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction, value);
        }

        public static string WorkDirectoryFullPath
        {
            get { return TAddresses.projectPath + TTerraWorldManager.WorkDirectoryLocalPath; }
        }

        public static string WorkDirectoryLocalPath
        {
            get { return TTerraWorldManager.WorkDirectoryLocalPath; }
        }

        public static void SaveGraphAsTemplate(string path)
        {
            TDebug.TraceMessage();
            if (WorldGraph == null) return;
            WorldGraph.templateIgnoreList.Ignore_AreaGraph = true;
            WorldGraph.UpdateRenderingAndVxfFromScene();
            WorldGraph.SaveGraph(path);
        }
   
        public static void LoadWorldGraph(string path, bool Template, out Exception exception, out bool reGenerate)
        {
            exception = null;
            reGenerate = false;

            try
            {
                if (TTerraWorldGraph.CheckGraph(path))
                    reGenerate = WorldGraph.LoadGraph(path, Template);
                else
                    throw new Exception("Corrupt File!");
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        public static bool LoadTemplate(string path, out Exception exception)
        {
            LoadWorldGraph(path, true, out exception, out bool reGenerate);
            return reGenerate;
        }

        public string HeightmapPath
        {
            get { return WorkDirectoryLocalPath + "Terrain Data.asset"; }
        }

        public string HeightmapPathBackground
        {
            get { return WorkDirectoryLocalPath + "Terrain Data BG.asset"; }
        }

        private int _ElevationzoomLevel = 10;
        public int ElevationZoomLevel
        {
            get { return _ElevationzoomLevel; }
            set
            {
                _ElevationzoomLevel = value;
            }
        }

        private int _ImagerzoomLevel = 10;
        public int ImageZoomLevel
        {
            get { return _ImagerzoomLevel; }
            set
            {
                _ImagerzoomLevel = value;
            }
        }

        //private int _landcoverimageryzoomLevel = 13;
        private int _landcoverimageryzoomLevel = -1;
        public int LandcoverImageryZoomLevel
        {
            get { return _landcoverimageryzoomLevel; }
            set
            {
                _landcoverimageryzoomLevel = value;
            }
        }

        public static bool CacheData
        {
            get { return TProjectSettings.CacheData; }
        }

        //private float _progressPersentage;
        public float ProgressPersentage
        {
            get 
            {
                float _progressPersentage = 0;

                for (int i = 0; i < _terrains.Count; i++)
                    _progressPersentage += _terrains[i].Progress;

                _progressPersentage = (_progressPersentage * 1.0f / _terrains.Count);

                return _progressPersentage;
            }
        }

        public TTerraWorld()
        {
            TDebug.TraceMessage();
            Random rand = new Random((int)DateTime.Now.Ticks);
            _ID = (rand.Next() + Counter++).ToString();
        }

        public void GenerateTerrains(TArea area)
        {
            TDebug.TraceMessage();
            _terrains.Clear();
            _terrainsDone = 0;
            TTerrain tempTerrain = new TTerrain(area.Top, area.Left, area.Bottom, area.Right, this);
            _terrains.Add(tempTerrain);
        }

        public void UpdateTerraWorld(Action<TTerraWorld> act = null)
        {
            TDebug.TraceMessage();
            if (act != null) _lastActions = act;
            _terrains = new List<TTerrain>();
            GenerateTerrains(WorldArea);

            foreach (TTerrain t in _terrains)
                t.UpdateTerrain();
        }

        public void EachTerrainDone(TTerrain CurrentTerrain)
        {
            TDebug.TraceMessage();
            _terrainsDone++;

            if (_terrainsDone == _terrains.Count)
                WhenAllDone();
        }

        private void WhenAllDone()
        {
            TDebug.TraceMessage();
            if (_lastActions != null)
                _lastActions.Invoke(this);
        }

        public System.Numerics.Vector3 GetWorldPosition(TGlobalPoint gpoint) => _terrains[0].GetWorldPositionWithHeight(gpoint);
        public System.Numerics.Vector3 GetAngle(TGlobalPoint gpoint) => _terrains[0].GetAngle(gpoint);
        public float GetSteepness(TGlobalPoint gpoint) => _terrains[0].GetSteepness(gpoint);
        public System.Numerics.Vector2 GetNormalPositionN(TGlobalPoint gpoint) => _terrains[0].GetNormalPositionN(gpoint);
    }
#endif
}
#endif
#endif

