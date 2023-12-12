using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    public abstract class GraphNode : Node
    {
        public abstract string GUID { get; }
        public abstract Port FlowIn { get; }
        public abstract Port FlowOut { get; }

        protected StoryGraphView view;

        protected GraphNode(StoryGraphView view)
        {
            this.view = view;
        }
    }

    public abstract class GraphNode<T> : GraphNode where T : NodeData
    {
        public T Data { get; private set; }
        public override string GUID => Data.GUID;

        public GraphNode(StoryGraphView view, T data) : base(view)
        {
            Data = data;
            SetPosition(new(data.Pos, Vector2.zero));
        }

        protected Port CreateFlowInPort(int pos = -1)
        {
            var port = CreateInPort("→", Port.Capacity.Multi, typeof(Flow), pos);
            port.portColor = Flow.Color;
            return port;
        }

        protected Port CreateFlowOutPort(int pos = -1)
        {
            var port = CreateOutPort("→", Port.Capacity.Single, typeof(Flow), pos);
            port.portColor = Flow.Color;
            return port;
        }

        protected Port CreateInPort(string name, Port.Capacity capacity, Type type, int pos = -1)
        {
            var port = CreatePort(name, capacity, type, Direction.Input);

            if (pos == -1) inputContainer.Add(port);
            else inputContainer.Insert(pos, port);
            RefreshExpandedState();
            RefreshPorts();
            return port;
        }

        protected Port CreateOutPort(string name, Port.Capacity capacity, Type type, int pos = -1)
        {
            var port = CreatePort(name, capacity, type, Direction.Output);

            if (pos == -1) outputContainer.Add(port);
            else outputContainer.Insert(pos, port);
            RefreshExpandedState();
            RefreshPorts();
            return port;
        }

        private Port CreatePort(string name, Port.Capacity capacity, Type type, Direction dir)
        {
            var port = InstantiatePort(Orientation.Horizontal, dir, capacity, type);
            port.portName = name;
            port.AddManipulator(new EdgeConnector<Edge>(view.Connector));
            return port;
        }

        protected void Refresh()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class StartNode : GraphNode<NodeData>
    {
        public override Port FlowOut => port;
        public override Port FlowIn => null;
        private Port port;

        public StartNode(StoryGraphView view, NodeData data) : base(view, data)
        {
            title = "开始";
            port = CreateFlowOutPort();
        }
    }

    public class EndNode : GraphNode<NodeData>
    {
        public override Port FlowIn => port;
        public override Port FlowOut => null;
        private Port port;

        public EndNode(StoryGraphView view, NodeData data) : base(view, data)
        {
            title = "结束";
            port = CreateFlowInPort();
        }
    }
}