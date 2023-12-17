using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hamstory
{
    [CreateAssetMenu(fileName = "StoryGraph", menuName = "Hamstory/StoryGraph")]
    public class StoryGraph : ScriptableObject
    {
        public const string DEFAULT_PORT_NAME = "→";
        public const string SCRIPT_EXTENSION = ".hamstory";

        [SerializeField] private NodeData startNode, endNode;
        [SerializeField] private List<StoryNodeData> storyNodes = new();
        [SerializeField] private List<SubGraphNodeData> subNodes = new();
        [SerializeField] private List<ConnectionData> connections = new();

        public NodeData StartNode { get => startNode; set => startNode = value; }
        public NodeData EndNode { get => endNode; set => endNode = value; }
        public List<ConnectionData> Conns { get => connections; }
        public List<StoryNodeData> StoryNodes { get => storyNodes; set => storyNodes = value; }
        public List<SubGraphNodeData> SubGraphNodes { get => subNodes; set => subNodes = value; }

        public NodeData GetOutputNode(ConnectionData conn)
        {
            if (conn.ToGUID == endNode.GUID) return endNode;
            return GetNode(conn.ToGUID);
        }

        public List<ConnectionData> GetNodeConns(NodeData from)
            => connections.Where(i => i.FromGUID == from.GUID).ToList();

        public void AddNodes(IEnumerable<NodeData> nodes)
        {
            storyNodes.AddRange(nodes.Where(i => i is StoryNodeData).Cast<StoryNodeData>());
            subNodes.AddRange(nodes.Where(i => i is SubGraphNodeData).Cast<SubGraphNodeData>());
        }

        public NodeData GetNode(string GUID) => GetNode(i => i.GUID == GUID);

        public NodeData GetNode(Func<NodeData, bool> expr)
        {
            var sn = storyNodes.FirstOrDefault(expr);
            if (sn != null) return sn;
            return subNodes.FirstOrDefault(expr);
        }

        public List<NodeData> GetNodes()
            => storyNodes.Cast<NodeData>()
                .Union(subNodes.Cast<NodeData>())
                .ToList();

        public List<NodeData> GetNodes(Func<NodeData, bool> expr)
            => storyNodes.Cast<NodeData>()
                .Union(subNodes.Cast<NodeData>())
                .Where(expr).ToList();

        public void RemoveNode(NodeData node)
        {
            if (node is StoryNodeData snd) storyNodes.Remove(snd);
            else if (node is SubGraphNodeData sbd) subNodes.Remove(sbd);
        }

        public void RemoveAllNodes(Func<NodeData, bool> expr)
        {
            storyNodes.RemoveAll(sn => expr.Invoke(sn));
            subNodes.RemoveAll(sn => expr.Invoke(sn));
        }
    }

    [Serializable]
    public class NodeData
    {
        [SerializeField] private Vector2 pos = Vector2.zero;
        [SerializeField] private string guid;

        public Vector2 Pos { get => pos; set => pos = value; }
        public string GUID { get => guid; set => guid = value; }

        public NodeData() { }

        public NodeData(string guid, Vector2 pos)
        {
            this.guid = guid;
            this.pos = pos;
        }

        public virtual NodeData Clone(string GUID)
        {
            var clonedData = new NodeData();
            clonedData.pos = pos;
            clonedData.guid = GUID;
            return clonedData;
        }
    }

    [Serializable]
    public class StoryNodeData : NodeData
    {
        [SerializeField] private TextAsset storyText;
        [SerializeField] private List<CharacterConfig> characters = new();

        public StoryNodeData(string guid, Vector2 pos) : base(guid, pos) { }

        public TextAsset StoryText { get => storyText; set => storyText = value; }
        public List<CharacterConfig> Characters { get => characters; set => characters = value; }

        public override NodeData Clone(string GUID)
        {
            var clonedData = new StoryNodeData(GUID, Pos);
            clonedData.storyText = storyText;
            clonedData.characters = new(characters);
            return clonedData;
        }
    }

    [Serializable]
    public class SubGraphNodeData : NodeData
    {
        [SerializeField] private StoryGraph subgraph;

        public SubGraphNodeData(string guid, Vector2 pos) : base(guid, pos) { }

        public StoryGraph Subgraph { get => subgraph; set => subgraph = value; }

        public override NodeData Clone(string GUID)
        {
            var clonedData = new SubGraphNodeData(GUID, Pos);
            clonedData.subgraph = subgraph;
            return clonedData;
        }
    }

    [Serializable]
    public class ConnectionData
    {
        [SerializeField] private string fromGUID, fromPortName, toGUID, toPortName;

        public string FromGUID { get => fromGUID; set => fromGUID = value; }
        public string FromPortName { get => fromPortName; set => fromPortName = value; }
        public string ToGUID { get => toGUID; set => toGUID = value; }
        public string ToPortName { get => toPortName; set => toPortName = value; }

        public ConnectionData(string fromGUID, string fromPortName, string toGUID, string toPortName)
        {
            FromGUID = fromGUID;
            FromPortName = fromPortName;
            ToGUID = toGUID;
            ToPortName = toPortName;
        }

        public bool ValueEquals(ConnectionData data)
            => fromGUID == data.fromGUID
                && fromPortName == data.fromPortName
                && toGUID == data.toGUID
                && toPortName == data.toPortName;

        public ConnectionData Clone(string fromGUID, string toGUID)
            => new(fromGUID, fromPortName, toGUID, toPortName);

        public override bool Equals(object obj)
        {
            return obj is ConnectionData data && ValueEquals(data);
        }

        public override int GetHashCode()
        {
            return fromGUID.GetHashCode() + fromPortName.GetHashCode() + toGUID.GetHashCode() + toPortName.GetHashCode();
        }
    }
}