using UnityEditor.Experimental.GraphView;

namespace Hamstory.Editor
{
    public static class EdgeExtensions
    {
        public static bool Match(this Edge edge, ConnectionData data)
            => ((GraphNode)edge.output.node).GUID == data.FromGUID
                && edge.output.portName == data.FromPortName
                && ((GraphNode)edge.input.node).GUID == data.ToGUID
                && edge.input.portName == data.ToPortName;

        public static ConnectionData ToConnData(this Edge edge)
            => new(((GraphNode)edge.output.node).GUID,
                    edge.output.portName,
                    ((GraphNode)edge.input.node).GUID,
                    edge.input.portName);
    }
}