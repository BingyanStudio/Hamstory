using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hamstory.Editor
{
    public class CopiedGraphData
    {
        private List<NodeData> nodes;
        private List<ConnectionData> conns;

        public CopiedGraphData(List<NodeData> nodes, List<ConnectionData> conns)
        {
            this.nodes = nodes;
            this.conns = conns;
        }

        public List<NodeData> GetClonedNodeDatas(Vector2 pos, bool relative = false)
        {
            Vector2 deltaPos;
            if (!relative)
            {
                var posX = nodes.Min(i => i.Pos.x);
                var posY = nodes.Min(i => i.Pos.y);
                deltaPos = pos - new Vector2(posX, posY);
            }
            else deltaPos = pos;

            var idMap = new Dictionary<string, string>();
            var newNodes = new List<NodeData>();
            nodes.ForEach(i =>
            {
                var id = GUID.Generate().ToString();
                idMap.Add(i.GUID, id);
                var newNode = i.Clone(id);
                newNode.Pos += deltaPos;
                newNodes.Add(newNode);
            });

            return newNodes;
        }

        public List<ConnectionData> GetClonedConnDatas()
        {
            var idMap = new Dictionary<string, string>();
            var newConns = new List<ConnectionData>();
            conns.ForEach(i =>
            {
                if (idMap.ContainsKey(i.FromGUID) && idMap.ContainsKey(i.ToGUID))
                    newConns.Add(i.Clone(idMap[i.FromGUID], idMap[i.ToGUID]));
            });
            return newConns;
        }

        // public List<GraphElement> Paste(StoryGraphView view, StoryGraph graph, Vector2 pos, bool relative = false)
        // {
        //     Vector2 deltaPos;
        //     if (!relative)
        //     {
        //         var posX = nodes.Min(i => i.Pos.x);
        //         var posY = nodes.Min(i => i.Pos.y);
        //         deltaPos = pos - new Vector2(posX, posY);
        //     }
        //     else deltaPos = pos;
        //     var idMap = new Dictionary<string, string>();
        //     var newNodes = new List<NodeData>();
        //     nodes.ForEach(i =>
        //     {
        //         var id = GUID.Generate().ToString();
        //         idMap.Add(i.GUID, id);
        //         var newNode = i.Clone(id);
        //         newNode.Pos += deltaPos;
        //         newNodes.Add(newNode);
        //     });

        //     var newConns = new List<ConnectionData>();
        //     conns.ForEach(i =>
        //     {
        //         if (idMap.ContainsKey(i.FromGUID) && idMap.ContainsKey(i.ToGUID))
        //             newConns.Add(i.Clone(idMap[i.FromGUID], idMap[i.ToGUID]));
        //     });

        //     var els = new List<GraphElement>();

        //     graph.AddNodes(newNodes);
        //     els.AddRange(view.BuildNodes(newNodes));

        //     graph.Conns.AddRange(newConns);
        //     els.AddRange(view.BuildConns(newConns));

        //     view.PushOperation(new CreateOperation(view, els));

        //     return els;
        // }
    }
}