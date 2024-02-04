#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum RenderingModules
    {
        RenderingModule
    }

    public abstract class TRenderingModules : TNode
    {
#if !TERRAWORLD_XPRO
        public TerrainRenderingParams renderingParams = new TerrainRenderingParams(true);
#endif

        public TRenderingModules() : base()
        {
        }
    }


    [XmlType("RenderingNode")]
    public class RenderingNode : TRenderingModules
    {
        public RenderingNode() : base()
        {
            Data.moduleType = ModuleType.Processor;
            //type = typeof(RenderingNode).FullName;
            Data.name = "Rendering";
            isRemovable = false;
            isRunnable = false;
            //ResetToDefaultSettings();
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Area;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    public class TRenderingGraph : TGraph
    {
        public TRenderingGraph() : base(ConnectionDataType.Global, "RENDERING Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {
            worldGraph = terraWorldGraph;
            _title = "RENDERING";
            if (nodes.Count > 0) return;
            RenderingNode node = new RenderingNode();
            node.Init(this);
            nodes.Add(node);
        }

        public RenderingNode GetEntryNode()
        {
            return (nodes[0] as RenderingNode);
        }
    }
#endif
}
#endif
#endif

