using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    public class StoryGraphView : GraphView
    {
        private static CopiedGraphData copiedData;

        private StoryGraphWindow window;

        private StartNode startNode;
        private EndNode endNode;
        private StoryGraph graph;

        private Stack<StoryGraphOperation> ops = new();
        private bool deletionForUndo = false;

        internal GraphEdgeConnector Connector { get; private set; }
        private Port pendingPort;

        private SearchWindowProvider searchProvider;

        public StoryGraphView(StoryGraphWindow window)
        {
            this.window = window;

            var grid = new GridBackground();
            Insert(0, grid);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new RectangleSelector());

            Connector = new GraphEdgeConnector(this);
            searchProvider = SearchWindowProvider.CreateInstance<SearchWindowProvider>();
            searchProvider.Selected += OnSearchSelected;

            graphViewChanged += OnGraphChanged;
        }

        public void Init(StoryGraph graph)
        {
            this.graph = graph;
            BuildGraph();
        }

        private void BuildGraph()
        {
            if (graph.StartNode.GUID.Length == 0) graph.StartNode = new NodeData(GUID.Generate().ToString(), new(100, 200));
            if (graph.EndNode.GUID.Length == 0) graph.EndNode = new NodeData(GUID.Generate().ToString(), new(600, 200));

            startNode = new StartNode(this, graph.StartNode);
            endNode = new EndNode(this, graph.EndNode);

            AddElement(startNode);
            AddElement(endNode);

            graph.GetNodes().ForEach(i =>
            {
                switch (i)
                {
                    case StoryNodeData sn:
                        BuildStoryNode(sn);
                        break;

                    case SubGraphNodeData gn:
                        BuildSubGraphNode(gn);
                        break;
                }
            });

            BuildConns(graph.Connections);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(i => i.node != startPort.node && i.direction != startPort.direction && i.portType == startPort.portType).ToList();
        }

        private GraphViewChange OnGraphChanged(GraphViewChange c)
        {
            if (c.elementsToRemove != null)
            {
                if (c.elementsToRemove.Contains(startNode)) c.elementsToRemove.Remove(startNode);
                if (c.elementsToRemove.Contains(endNode)) c.elementsToRemove.Remove(endNode);

                var nodesToRemove = new List<NodeData>();
                var edgesToRemove = new List<ConnectionData>();
                c.elementsToRemove.ForEach(i =>
                {
                    if (i is GraphNode node)
                    {
                        var nodeToRemove = graph.GetNode(node.GUID);
                        nodesToRemove.Add(nodeToRemove);
                        graph.RemoveNode(nodeToRemove);
                    }

                    else if (i is Edge edge)
                    {
                        var data = GetConnDataByEdge(edge);
                        edgesToRemove.Add(data);

                        graph.Connections.Remove(data);
                    }
                });

                if (!deletionForUndo)
                {
                    PushOperation(new RemoveOperation(this, graph, nodesToRemove, edgesToRemove));
                    deletionForUndo = false;
                }
            }

            if (c.movedElements != null)
            {
                c.movedElements.ForEach(i =>
                {
                    switch (i)
                    {
                        case StartNode start:
                            PushOperation(new MoveOperation(this, start, graph.StartNode, graph.StartNode.Pos));
                            graph.StartNode.Pos = start.GetPosition().position;
                            break;

                        case EndNode end:
                            PushOperation(new MoveOperation(this, end, graph.EndNode, graph.EndNode.Pos));
                            graph.EndNode.Pos = end.GetPosition().position;
                            break;

                        case StoryNode node:
                            MoveGraphNode(node);
                            break;

                        case SubGraphNode sgn:
                            MoveGraphNode(sgn);
                            break;
                    }
                });
            }

            if (c.edgesToCreate != null)
            {
                c.edgesToCreate.ForEach(i =>
                {
                    graph.Connections.Add(CreateConnData(i.output, i.input));
                    PushOperation(new CreateOperation(this, i));
                });
            }

            Save();
            return c;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.ClearItems();

            evt.menu.AppendAction("复制", a => CopySelection(),
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("剪切", a => { CopySelection(); DeleteSelection(); },
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("粘贴", a => Paste(GetMousePosition(a.eventInfo.localMousePosition)),
                copiedData == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("克隆", a => { CopySelection(); Paste(Vector2.one * 30, true); },
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("添加故事脚本", a => CreateStoryNode(GetMousePosition(a.eventInfo.localMousePosition)));

            evt.menu.AppendAction("添加故事链", a => CreateSubGraphNode(GetMousePosition(a.eventInfo.localMousePosition)));
        }

        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);

            if (evt is KeyDownEvent key)
            {
                if (key.ctrlKey)
                {
                    switch (key.keyCode)
                    {
                        case KeyCode.Z:
                            key.StopPropagation();
                            if (ops.Count > 0) ops.Pop().Undo();
                            break;

                        case KeyCode.C:
                            key.StopPropagation();
                            CopySelection();
                            break;

                        case KeyCode.V:
                            key.StopPropagation();
                            Paste(GetMousePosition(evt.originalMousePosition));
                            break;

                        case KeyCode.X:
                            key.StopPropagation();
                            CopySelection();
                            DeleteSelection();
                            break;

                        case KeyCode.D:
                            key.StopPropagation();
                            CopySelection();
                            Paste(Vector2.one * 30, true);
                            break;
                    }
                }
            }
        }

        public void PushOperation(StoryGraphOperation op)
        {
            ops.Push(op);
        }

        private void CopySelection()
        {
            var nodes = selection.Where(i => i is GraphNode node && node != startNode && node != endNode).Cast<GraphNode>().Select(i => i.GUID);
            var conns = selection.Where(i => i is Edge edge).Cast<Edge>();

            var nodeDatas = graph.GetNodes(i => nodes.Contains(i.GUID)).ToList();
            var connDatas = conns.Select(i => GetConnDataByEdge(i)).ToList();
            if (nodeDatas.Count == 0) return;

            copiedData = new(nodeDatas, connDatas);
        }

        private void Paste(Vector2 pos, bool relative = false)
        {
            if (copiedData != null)
            {
                ClearSelection();
                copiedData.Paste(this, graph, pos, relative).ForEach(i => AddToSelection(i));
            }
        }

        internal void ShowSearchWindow(Vector2 pos, Edge targetEdge)
        {
            pendingPort = targetEdge.input ?? targetEdge.output;
            SearchWindow.Open(new(pos + window.position.position), searchProvider);
        }

        private void OnSearchSelected(string path, Vector2 pos)
        {
            pos -= window.position.position;
            pos = contentViewContainer.WorldToLocal(pos);

            if (pendingPort.direction == Direction.Input) pos.x -= 350;

            GraphNode node;
            if (path.EndsWith(".asset"))
            {
                var graph = AssetDatabase.LoadAssetAtPath<StoryGraph>(path);
                var gn = CreateSubGraphNode(pos);
                gn.SetSubgraph(graph);
                node = gn;
            }
            else
            {
                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                var gn = CreateStoryNode(pos);
                gn.SetStory(text);
                node = gn;
            }

            Edge conn;
            if (pendingPort.direction == Direction.Input)
            {
                conn = ConnectPort(node.FlowOut, pendingPort);
                graph.Connections.Add(CreateConnData(node.FlowOut, pendingPort));
            }
            else
            {
                DeleteElements(pendingPort.connections);
                conn = ConnectPort(pendingPort, node.FlowIn);
                graph.Connections.Add(CreateConnData(pendingPort, node.FlowIn));
            }

            PushOperation(new CreateOperation(this, node, conn));
        }

        internal StoryNode CreateStoryNode(Vector2 pos)
        {
            var data = new StoryNodeData(GUID.Generate().ToString(), pos);
            var node = BuildStoryNode(data);
            graph.StoryNodes.Add(data);
            Save();

            PushOperation(new CreateOperation(this, node));
            return node;
        }

        internal StoryNode BuildStoryNode(StoryNodeData data)
        {
            var node = new StoryNode(this, data);
            AddElement(node);

            return node;
        }

        internal SubGraphNode CreateSubGraphNode(Vector2 pos)
        {
            var data = new SubGraphNodeData(GUID.Generate().ToString(), pos);
            var node = BuildSubGraphNode(data);
            graph.SubNodes.Add(data);

            PushOperation(new CreateOperation(this, node));
            return node;
        }

        internal SubGraphNode BuildSubGraphNode(SubGraphNodeData data)
        {
            var node = new SubGraphNode(this, data);
            AddElement(node);

            return node;
        }

        internal List<GraphNode> BuildNodes(List<NodeData> nodes)
        {
            var results = new List<GraphNode>();
            nodes.ForEach(i =>
                {
                    switch (i)
                    {
                        case StoryNodeData story:
                            results.Add(BuildStoryNode(story));
                            break;

                        case SubGraphNodeData sub:
                            results.Add(BuildSubGraphNode(sub));
                            break;
                    }
                });
            return results;
        }

        internal List<Edge> BuildConns(List<ConnectionData> data)
        {
            var results = new List<Edge>();
            data.ForEach(i =>
            {
                var output = ports.FirstOrDefault(j => j.direction == Direction.Output && ((GraphNode)j.node).GUID == i.FromGUID && j.portName == i.FromPortName);
                var input = ports.FirstOrDefault(j => j.direction == Direction.Input && ((GraphNode)j.node).GUID == i.ToGUID && j.portName == i.ToPortName);

                if (input == null || output == null) return;

                results.Add(ConnectPort(output, input));
            });
            return results;
        }

        private Edge ConnectPort(Port from, Port to)
        {
            var edge = from.ConnectTo(to);
            AddElement(edge);
            return edge;
        }

        private ConnectionData CreateConnData(Port from, Port to)
            => new(((GraphNode)from.node).GUID, from.portName, ((GraphNode)to.node).GUID, to.portName);

        private ConnectionData GetConnDataByEdge(Edge edge)
        {
            return graph.Connections.Where(j =>
                j.FromGUID == ((GraphNode)edge.output.node).GUID
                && j.FromPortName == edge.output.portName
                && j.ToGUID == ((GraphNode)edge.input.node).GUID
                && j.ToPortName == edge.input.portName)
                .FirstOrDefault();
        }

        private void MoveGraphNode(GraphNode node)
        {
            var target = graph.GetNode(node.GUID);
            if (target == null) return;
            PushOperation(new MoveOperation(this, node, target, target.Pos));
            target.Pos = node.GetPosition().position;
        }

        internal void MarkDeletionAsUndo()
        {
            deletionForUndo = true;
        }

        private Vector2 GetMousePosition(Vector2 world)
            => contentViewContainer.WorldToLocal(world);

        public void Save() => window.SaveChanges();
    }
}