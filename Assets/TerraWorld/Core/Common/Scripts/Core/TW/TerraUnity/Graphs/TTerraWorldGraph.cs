#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
#endif
using TerraUnity.Runtime;
using TerraUnity.Utils;


namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum Presets
    {
        mobile,
        tablet,
        pc,
        console,
        farm,
        custom
    }

    public enum VersionMode
    {
        Pro,
        Lite,
        xNode
    }

    public class TSequence
    {
        private List<TNode> _sequence;

        public TSequence()
        {
            _sequence = new List<TNode>();
        }

        public List<TNode> Nodes { get => _sequence; set => _sequence = value; }
    }

    public struct TemplateIgnoreList
    {
        public bool Ignore_AreaGraph;
        public bool Ignore_HeightmapGraph;
        public bool Ignore_ColormapGraph;
        public bool Ignore_BiomesGraph;

        //Rendering Graph Section
        public bool Ignore_RenderingGraph_surfaceTint;
        public bool Ignore_RenderingGraph_modernRendering;
        public bool Ignore_RenderingGraph_instancedDrawing;
        public bool Ignore_RenderingGraph_tessellation;
        public bool Ignore_RenderingGraph_heightmapBlending;
        public bool Ignore_RenderingGraph_TillingRemover;
        public bool Ignore_RenderingGraph_colormapBlending;
        public bool Ignore_RenderingGraph_proceduralSnow;
        public bool Ignore_RenderingGraph_proceduralPuddles;
        public bool Ignore_RenderingGraph_LayerProperties;
        public bool Ignore_RenderingGraph_isFlatShading;
        public bool Ignore_RenderingGraph_SplatmapSettings;
        public bool Ignore_RenderingGraph_MainTerrainSettings;
        public bool Ignore_RenderingGraph_BGTerrainSettings;

        public bool Ignore_FXGraph_selectionIndexVFX;
        public bool Ignore_FXGraph_TimeOfDay;
        public bool Ignore_FXGraph_CrepuscularRay;
        public bool Ignore_FXGraph_CloudsSettings;
        public bool Ignore_FXGraph_AtmosphericScatteringSettings;
        public bool Ignore_FXGraph_VolumetricFogSettings;
        public bool Ignore_FXGraph_WindSettings;
        public bool Ignore_FXGraph_WeatherSettings;
        public bool Ignore_FXGraph_ReflectionSettings;
        public bool Ignore_FXGraph_WaterSettings;
        public bool Ignore_FXGraph_PostProcessSettings;
        public bool Ignore_FXGraph_HorizonFogSettings;
        public bool Ignore_FXGraph_FlatShadingSettings;
        public bool Ignore_FXGraph_SateliteImageBlendingSettings;
        public bool Ignore_PlayerGraph;
    }

    [Serializable]
    public struct FileData
    {
        public byte[] data;
    }

    [Serializable]
    public class TTerraWorldGraph
    {
#if TERRAWORLD_PRO
        public VersionMode versionMode = VersionMode.Pro;
#elif TERRAWORLD_XPRO
        public VersionMode versionMode = VersionMode.xNode;
#else
        public VersionMode versionMode = VersionMode.Lite;
#endif

        public TemplateIgnoreList templateIgnoreList;

        //public TerrainRenderingParams TerrainRenderingParams {get> renderingGraph.GetEntryNode().renderingParams()};
        //public FXParams _VFXParams;

        public bool _timeOfDayParamsSaved = false;
        private OldTimeOfDayParams _timeOfDayParams;

        [XmlElement(Namespace = "TimeOfDayParams")]
        public OldTimeOfDayParams TimeOfDayParams { get => _timeOfDayParams; set => _timeOfDayParams = value; }

        public bool _globalTimeParamsSaved = false;

        private TerrainRenderingParams _RenderingDATA = new TerrainRenderingParams();
        [XmlElement(Namespace = "TerrainRenderingParams")]
        public TerrainRenderingParams RenderingDATA { get => _RenderingDATA; set => _RenderingDATA = value; }

        private SceneSettingsManager.VFXData _VFXDATA = new SceneSettingsManager.VFXData();
        [XmlElement(Namespace = "VFXData")]
        public SceneSettingsManager.VFXData VFXDATA { get => _VFXDATA; set => _VFXDATA = value; }

        private static Random rand = new Random();
        public delegate void AreaChangedHandler();

        // UI Settings
        public static float scaleFactor = 1;
        // public static TMapManager.ImageryMapServer _interactiveMapImagerySource = TMapManager.ImageryMapServer.arcgisonline;
        // public static TMapManager.ImageryMapServer _OSMImagerySource = TMapManager.ImageryMapServer.Standard;
        // -----------------------------------------------------------------------------------------------------------------------------------

        public TAreaGraph areaGraph = new TAreaGraph();
        public THeightmapGraph heightmapGraph = new THeightmapGraph();
        public TColormapGraph colormapGraph = new TColormapGraph();
        public TBiomesGraph biomesGraph = new TBiomesGraph();
        public TRenderingGraph renderingGraph = new TRenderingGraph();
        public TFXGraph FXGraph = new TFXGraph();
        public TPlayerGraph playerGraph = new TPlayerGraph();

        [XmlIgnore] public List<TGraph> graphList = new List<TGraph>();
        public int GraphMajorVersion = TVersionController.MajorVersion;
        public int GraphMinorVersion = TVersionController.MinorVersion;

#if TERRAWORLD_XPRO
    //[XmlElement("NaughtyXmlCharacters")]
    public string XGraphDataAsString
    {
        get
        {
            if (XGraphCharacters == null) return string.Empty;
            return BitConverter.ToString(XGraphCharacters);
        }
        set
        {
            // without this, the property is not serialized.
            if (!String.IsNullOrEmpty(value))
            {
                String[] arr = value.Split('-');
                byte[] array = new byte[arr.Length];
                for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);
                XGraphCharacters = array;
            }
            else
                XGraphCharacters = null;
        }
    }
     
    [XmlIgnore]
    public byte[] XGraphCharacters
    {
        get;
        set;
    }
#endif

        public static int GetNewID()
        {
            return rand.Next();
        }

        public static TTerraWorldGraph GetNewWorldGraph(int MajorVersion, int MinorVersion)
        {
            TTerraWorldGraph _result = new TTerraWorldGraph();
            _result.GraphMajorVersion = MajorVersion;
            _result.GraphMinorVersion = MinorVersion;
            return _result;
        }

        public void UpdateRenderingAndVxfFromScene()
        {
            RenderingDATA = TerrainRenderingManager.GetParams();
            VFXDATA = SceneSettingsManager.GetVFXData();
        }

        public TTerraWorldGraph()
        {
            InitWorldGraph();
        }

        public void InitWorldGraph()
        {
            graphList = new List<TGraph>();

            graphList.Add(areaGraph);
            areaGraph.InitGraph(this);

            graphList.Add(heightmapGraph);
            heightmapGraph.InitGraph(this);

            graphList.Add(colormapGraph);
            colormapGraph.InitGraph(this);

            graphList.Add(biomesGraph);
            biomesGraph.InitGraph(this);

            graphList.Add(renderingGraph);
            renderingGraph.InitGraph(this);

            graphList.Add(FXGraph);
            FXGraph.InitGraph(this);

            graphList.Add(playerGraph);
            playerGraph.InitGraph(this);

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    graphList[i].nodes[j].parentGraph = graphList[i];

                graphList[i].UpdateConnections();
            }
        }

        public void SaveGraph(string path)
        {
            TDebug.TraceMessage();

            areaGraph.WorldArea.latitude = TTerraWorld.WorldArea.Latitude.ToString();
            areaGraph.WorldArea.longitude = TTerraWorld.WorldArea.Longitude.ToString();
            areaGraph.WorldArea.WorldSizeKMLat = TTerraWorld.WorldArea.AreaSizeLat;
            areaGraph.WorldArea.WorldSizeKMLon = TTerraWorld.WorldArea.AreaSizeLon;
            areaGraph.WorldArea.zoomLevel = TProjectSettings.InteractiveMapZoomLevel;

            GraphMajorVersion = TVersionController.MajorVersion;
            GraphMinorVersion = TVersionController.MinorVersion;

#if TERRAWORLD_XPRO
            //string filename = TTerraWorldManager.GetXGraphFilePath();
            ////
            ////SceneSettingsManager.SaveSceneSettingsPreFab(filename);
            ////
            //if (!String.IsNullOrEmpty(filename) && File.Exists(filename))
            //    using (FileStream fs1 = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //    {
            //        try
            //        {
            //            // Create a byte array of file stream length
            //            byte[] bytes = System.IO.File.ReadAllBytes(filename);
            //            //Read block of bytes from stream into the byte array
            //            fs1.Read(bytes, 0, System.Convert.ToInt32(fs1.Length));
            //            XGraphCharacters = bytes; //return the byte data
            //
            //        }
            //        catch (Exception e)
            //        {
            //            XGraphCharacters = null;
            //            throw e;
            //        }
            //        finally
            //        {
            //            //Close the File Stream
            //            fs1.Close();
            //        }
            //    }
            //
#endif

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    serializer.Serialize(fs, this);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public string GetAsString()
        {
           // MemoryStream stream = new MemoryStream();
           // Serializer.Serialize<SuperExample>(stream, test);
           // stream.Position = 0;
           // string strout = StreamToString(stream);
           // MemoryStream result = (MemoryStream)StringToStream(strout);
           // var other = Serializer.Deserialize<SuperExample>(result);

            MemoryStream ms = GetAsStream();
            string strout = StreamToString(ms);
            return strout;
        }

        public MemoryStream GetAsStream()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, this);
            ms.Position = 0;
            return ms;
        }

        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

        public List<TGraph> LoadGraphListCurrent()
        {
            List<TGraph> result = new List<TGraph>();
            result.Add(areaGraph);
            result.Add(heightmapGraph);
            result.Add(colormapGraph);
            result.Add(biomesGraph);
            result.Add(renderingGraph);
            result.Add(FXGraph);
            result.Add(playerGraph);

            return result;
        }

        public List<TGraph> LoadGraphList(string path)
        {
            TDebug.TraceMessage();
            List<TGraph> result = new List<TGraph>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    TTerraWorldGraph graph = (TTerraWorldGraph)serializer.Deserialize(fs);
                    result.Add(graph.areaGraph);
                    result.Add(graph.heightmapGraph);
                    result.Add(graph.colormapGraph);
                    result.Add(graph.biomesGraph);
                    result.Add(graph.renderingGraph);
                    result.Add(graph.FXGraph);
                    result.Add(graph.playerGraph);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    fs.Close();
                }
            }

            return result;
        }

        public static bool CheckGraph(string graphPath)
        {
            FileStream fs = new FileStream(graphPath, FileMode.Open);
            bool result = true; 

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                TTerraWorldGraph newLoadedGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
                result = true;
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
                result = false;
                //return false;
            }
            finally
            {
                fs.Close();
            }

            return result;
        }

        public bool LoadGraph(string graphPath, bool isTemplate)
        {
            TDebug.TraceMessage();
            bool needRegenerate = false;
            TTerraWorldGraph newLoadedGraph;

            using (FileStream fs = new FileStream(graphPath, FileMode.Open))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    newLoadedGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    fs.Close();
                }
            }

            if ((newLoadedGraph.GraphMajorVersion > TVersionController.MajorVersion)) throw new Exception("Unable to load graph. It has been created by " + newLoadedGraph.GraphMajorVersion + "." + newLoadedGraph.GraphMajorVersion + " version!");
            else if ((newLoadedGraph.GraphMajorVersion == TVersionController.MajorVersion) && (newLoadedGraph.GraphMinorVersion > TVersionController.MinorVersion)) throw new Exception("Unable to load graph. It has been created by " + newLoadedGraph.GraphMajorVersion + "." + newLoadedGraph.GraphMajorVersion + " version!");

            //TODO: Remove the following lines later when all templates are synced with this new feature and terrain layers have proper colors
            int graphVersion = newLoadedGraph.GraphMajorVersion * 1000 + newLoadedGraph.GraphMinorVersion;

            templateIgnoreList = newLoadedGraph.templateIgnoreList;

            if (isTemplate) templateIgnoreList.Ignore_AreaGraph = true;

            if (!templateIgnoreList.Ignore_AreaGraph || !isTemplate)
            {
                areaGraph = newLoadedGraph.areaGraph;

                TTerraWorld.SetWorldArea(double.Parse(areaGraph.WorldArea.latitude), double.Parse(areaGraph.WorldArea.longitude), areaGraph.WorldArea.WorldSizeKMLat, areaGraph.WorldArea.WorldSizeKMLon);
                TProjectSettings.InteractiveMapZoomLevel = areaGraph.WorldArea.zoomLevel;
                needRegenerate = true;
            }

#if !TERRAWORLD_XPRO
            if (graphVersion < 2301)
                newLoadedGraph.heightmapGraph.HeightmapMaster()._pixelError = newLoadedGraph.renderingGraph.GetEntryNode().renderingParams.terrainPixelError;
#endif

            if (!templateIgnoreList.Ignore_HeightmapGraph || !isTemplate)
            {
                heightmapGraph = newLoadedGraph.heightmapGraph;
                needRegenerate = true;
            }

            if (!templateIgnoreList.Ignore_ColormapGraph || !isTemplate)
            {
                colormapGraph = newLoadedGraph.colormapGraph;
                needRegenerate = true;
            }

            if (!templateIgnoreList.Ignore_BiomesGraph || !isTemplate)
            {
                biomesGraph = newLoadedGraph.biomesGraph;
                needRegenerate = true;
            }

#if TERRAWORLD_XPRO
            //if (!String.IsNullOrEmpty(newLoadedGraph.XGraphDataAsString))
            //{
            //    string filename = TTerraWorldManager.GetXGraphFilePath();
            //    if (File.Exists(filename)) File.Delete(filename);
            //      using (FileStream fs1 = new FileStream(filename, FileMode.Create, FileAccess.Write))
            //      {
            //          try
            //          {
            //              fs1.Write(newLoadedGraph.XGraphCharacters, 0, System.Convert.ToInt32(newLoadedGraph.XGraphCharacters.Length));
            //          }
            //          catch (Exception e)
            //          {
            //              throw e;
            //          }
            //          finally
            //          {
            //              //Close the File Stream
            //              fs1.Close();
            //          }
            //      }
            //
            //      
            //    {
            //    }
            //    TTerraWorldManager.RefreshXGraph();
            //    
            //}
#endif

#if TERRAWORLD_PRO
            if (newLoadedGraph.versionMode != VersionMode.Lite)
            {
#if !TERRAWORLD_XPRO
                if (graphVersion <= 2130)
                {
                    newLoadedGraph.renderingGraph.GetEntryNode().renderingParams.surfaceTintColorMAIN = TUtils.UnityColorToVector4(new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1.0f));
                    newLoadedGraph.renderingGraph.GetEntryNode().renderingParams.surfaceTintColorBG = TUtils.UnityColorToVector4(new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1.0f));
                }

                if (graphVersion < 2500)
                {
                    newLoadedGraph.RenderingDATA = newLoadedGraph.renderingGraph.GetEntryNode().renderingParams;
                    SceneSettingsManager.InstantiateNewSceneSettings();
                    SceneSettingsManager.VFXData vFXData = SceneSettingsManager.GetVFXData();

                    //newLoadedGraph.VFXDATA = SceneSettingsManager.GetVFXData();

                    vFXData.ApplyOldfxParams(newLoadedGraph.FXGraph.GetEntryNode().fxParams);

                    if (newLoadedGraph._timeOfDayParamsSaved)
                        vFXData.ApplyOldTimeOfDayParams(newLoadedGraph.TimeOfDayParams);

                    newLoadedGraph.VFXDATA = vFXData;
                }
#endif

                //TODO : apply template ignore list
                RenderingDATA = newLoadedGraph.RenderingDATA;

                //RenderingDATA.FilterCopy
                //(
                //     newLoadedGraph.RenderingDATA,
                //     templateIgnoreList.Ignore_RenderingGraph_surfaceTint,
                //     templateIgnoreList.Ignore_RenderingGraph_modernRendering,
                //     templateIgnoreList.Ignore_RenderingGraph_tessellation,
                //     templateIgnoreList.Ignore_RenderingGraph_heightmapBlending,
                //     templateIgnoreList.Ignore_RenderingGraph_TillingRemover,
                //     templateIgnoreList.Ignore_RenderingGraph_colormapBlending,
                //     templateIgnoreList.Ignore_RenderingGraph_proceduralSnow,
                //     templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles,
                //     templateIgnoreList.Ignore_RenderingGraph_LayerProperties,
                //     templateIgnoreList.Ignore_RenderingGraph_isFlatShading,
                //     templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings,
                //     templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings
                //);

                //TODO: apply template ignore list
                VFXDATA = newLoadedGraph.VFXDATA;

                //TerrainRenderingManager.SetParams(RenderingDATA);
                //SceneSettingsManager.SetVFXData(VFXDATA);
            }
#else
            if (newLoadedGraph.versionMode  != VersionMode.Lite ) throw new Exception("Unable to load graph. Graph has been created by Pro version of TerraWorld!");
#endif

            if (!templateIgnoreList.Ignore_PlayerGraph || !isTemplate)
            {
                playerGraph = newLoadedGraph.playerGraph;
                needRegenerate = true;
            }

            InitWorldGraph();
            //HandleAreaWorldChange();

            return needRegenerate;
        }

        public bool CheckConnections()
        {
            TDebug.TraceMessage();

            for (int i = 0; i < graphList.Count; i++)
            {
                graphList[i].UpdateConnections();
                if (!graphList[i].CheckConnections()) return false;
            }

            return true;
        }

        public void RemoveNodeByID(int ID)
        {
            TDebug.TraceMessage();
            if (ID == -1) return;
            List<TNode> outputNodes = GetOutputNodes(ID);

            foreach (TNode node in outputNodes)
                for (int i = 0; i < node.inputConnections.Count; i++)
                    if (node.inputConnections[i].previousNodeID == ID)
                        node.inputConnections[i].previousNodeID = -1;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    if (graphList[i].nodes[j].Data.ID.Equals(ID))
                        graphList[i].nodes.Remove(graphList[i].nodes[j--]);
        }

        public TNode GetNodeByID(int ID)
        {
            TNode result = null;
            if (ID == -1) return null;

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    if (graphList[i].nodes[j].Data.ID.Equals(ID))
                    {
                        result = graphList[i].nodes[j];
                        break;
                    }
                }

                if (result != null)
                    return result;
            }

            return result;
        }

        public int GetNodeCounts()
        {
            int result = 0;

            for (int i = 0; i < graphList.Count; i++)
                result += graphList[i].nodes.Count;

            return (result - 10);
        }

        public List<TNode> GetOutputNodes(int ID)
        {
            List<TNode> result = new List<TNode>();
            if (ID == -1) return result;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    for (int k = 0; k < graphList[i].nodes[j].inputConnections.Count; k++)
                        if (graphList[i].nodes[j].inputConnections[k].previousNodeID.Equals(ID))
                            result.Add(graphList[i].nodes[j]);

            return result;
        }

        public List<TNode> GetLastNodes(ConnectionDataType connectionDataType)
        {
            TDebug.TraceMessage();
            List<TNode> result = new List<TNode>();

            for (int i = 0; i < graphList.Count; i++)
            {
                result = graphList[i].GetLastNodes(connectionDataType);
                if (result.Count != 0) return result;
            }

            return result;
        }

        public void MakeChain(ref List<TNode> nodesList, TNode node)
        {

            for (int i = 0; i < nodesList.Count; i++)
            {
                TNode n = nodesList[i];

                if (n == node)
                {
                    nodesList.Remove(n);
                    i--;
                }
            }

            nodesList.Add(node);

            for (int i = 0; i < node.inputConnections.Count; i++)
            {
                if (node.inputConnections[i].previousNodeID != -1)
                {
                    TNode currentNode = GetNodeByID(node.inputConnections[i].previousNodeID);
                    MakeChain(ref nodesList, currentNode);
                }
            }
        }

        // Proper sequence of nodes
        public bool GetSequences(ref List<TSequence> sequences, ConnectionDataType DataType)
        {
            TDebug.TraceMessage();
            sequences.Clear();
            List<TNode> lastnodes = GetLastNodes(DataType);
            if (lastnodes.Count == 0) return false;

            for (int u = 0; u < lastnodes.Count; u++)
            {
                TSequence sequence = new TSequence();
                List<TNode> nodesList = new List<TNode>();
                MakeChain(ref nodesList, lastnodes[u]);

                for (int i = nodesList.Count - 1; i >= 0; i--)
                    sequence.Nodes.Add(nodesList[i]);

                sequences.Add(sequence);
            }

            return true;
        }

        public int UpdateInputList(ref Dictionary<TNode, string> InputList, TNode module, int inputIndex)
        {
            ConnectionDataType dataType = module.inputConnections[inputIndex].connectionDataType;
            InputList.Clear();
            InputList.Add(new TNullNode(), "No Filter");
            int result = 0;
            TNode node = null;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    node = graphList[i].nodes[j];

                    if (node != module && node.outputConnectionType == dataType)
                        InputList.Add(node, node.Data.name);
                }

            node = GetNodeByID(module.inputConnections[inputIndex].previousNodeID);

            for (int j = 0; j < InputList.Count; j++)
                if (node == InputList.Keys.ToList()[j])
                    result = j;

            return result;
        }

        public void ResetGraphsStatus()
        {
            TDebug.TraceMessage();

            for (int i = 0; i < graphList.Count; i++)
                graphList[i].ResetNodesStatus();
        }

        //public void ResetBoundingBoxesArea()
        //{
        //    TDebug.TraceMessage();
        //
        //    for (int i = 0; i < graphList.Count; i++)
        //        for (int j = 0; j < graphList[i].nodes.Count; j++)
        //            graphList[i].nodes[j].ResetAreaBound();
        //}

        //public void HandleAreaWorldChange()
        //{
        //    TDebug.TraceMessage();
        //    ResetBoundingBoxesArea();
        //}

        public int GetLastTypeIndex(TNode node)
        {
            int result = 0;

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    if (graphList[i].nodes[j].GetType() == node.GetType())
                    {
                        if (result < graphList[i].nodes[j].NodeTypeIndex)
                            result = graphList[i].nodes[j].NodeTypeIndex;
                    }
                }
            }

            return result;
        }

        // Return a deep clone of an object of type T.
        public TTerraWorldGraph DeepClone()
        {
            using (MemoryStream memory_stream = new MemoryStream())
            {
                // Serialize the object into the memory stream.
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory_stream, this);

                // Rewind the stream and use it to create a new object.
                memory_stream.Position = 0;
                return (TTerraWorldGraph)formatter.Deserialize(memory_stream);
            }
        }

        public bool Isdirty()
        {
            bool result = true;
            return result;
        }
    }
#endif
}
#endif
#endif

