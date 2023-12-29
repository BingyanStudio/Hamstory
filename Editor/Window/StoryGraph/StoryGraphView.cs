using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    internal class StoryGraphView : GraphView
    {
        internal StoryGraphViewModel viewModel;
        private static CopiedGraphData copiedData;

        private StoryGraphWindow window;

        private StartNode startNode;
        private EndNode endNode;

        internal GraphEdgeConnector Connector { get; private set; }

        internal StoryGraphView(StoryGraphWindow window)
        {
            this.window = window;

            var grid = new GridBackground();
            Insert(0, grid);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new StoryDragManipulator(this));

            graphViewChanged += OnGraphChanged;
        }

        internal void Init(StoryGraphViewModel viewModel)
        {
            this.viewModel = viewModel;
            Connector = new GraphEdgeConnector(viewModel);
            viewModel.Bind(this);
        }

        internal void InitGraph(StoryGraph graph)
        {
            startNode = new StartNode(this, graph.StartNode);
            endNode = new EndNode(this, graph.EndNode);

            AddElement(startNode);
            AddElement(endNode);

            graph.GetNodes().ForEach(i =>
            {
                switch (i)
                {
                    case StoryNodeData sn:
                        AddStoryNode(sn);
                        break;

                    case SubGraphNodeData gn:
                        AddSubGraphNode(gn);
                        break;
                }
            });

            BuildConns(graph.Conns);
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

                var nodesToRemove = new List<string>();
                var edgesToRemove = new List<ConnectionData>();
                c.elementsToRemove.ForEach(i =>
                {
                    if (i is GraphNode node)
                        nodesToRemove.Add(node.GUID);

                    else if (i is Edge edge)
                        edgesToRemove.Add(edge.ToConnData());

                });

                viewModel.ApplyRemoveChanges(nodesToRemove, edgesToRemove);
            }

            List<(string, Vector2)> movedNodes = new();
            if (c.movedElements != null)
            {
                c.movedElements.ForEach(i =>
                {
                    switch (i)
                    {
                        case StartNode start:
                            movedNodes.Add((start.GUID, i.GetPosition().position));
                            break;

                        case EndNode end:
                            movedNodes.Add((end.GUID, i.GetPosition().position));
                            break;

                        case StoryNode story:
                            movedNodes.Add((story.GUID, i.GetPosition().position));
                            break;

                        case SubGraphNode sub:
                            movedNodes.Add((sub.GUID, i.GetPosition().position));
                            break;
                    }
                });
                viewModel.MoveNodes(movedNodes, false);
            }

            Save();
            return c;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.ClearItems();

            evt.menu.AppendAction("复制", a => viewModel.Copy(selection),
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("剪切", a => { viewModel.Copy(selection); DeleteSelection(); },
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("粘贴", a => viewModel.Paste(GetMousePosition(a.eventInfo.localMousePosition)),
                copiedData == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("克隆", a => { viewModel.Copy(selection); viewModel.Paste(Vector2.one * 30, true); },
                selection.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("添加故事脚本", a => viewModel.CreateStoryNode(GetMousePosition(a.eventInfo.localMousePosition)));

            evt.menu.AppendAction("添加故事节点图", a => viewModel.CreateSubGraphNode(GetMousePosition(a.eventInfo.localMousePosition)));
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
                            viewModel.Undo();
                            break;

                        case KeyCode.C:
                            key.StopPropagation();
                            viewModel.Copy(selection);
                            break;

                        case KeyCode.V:
                            key.StopPropagation();
                            viewModel.Paste(GetMousePosition(evt.originalMousePosition));
                            break;

                        case KeyCode.X:
                            key.StopPropagation();
                            viewModel.Copy(selection);
                            DeleteSelection();
                            break;

                        case KeyCode.D:
                            key.StopPropagation();
                            viewModel.Copy(selection);
                            viewModel.Paste(Vector2.one * 30, true);
                            break;
                    }
                }
            }
        }

        internal StoryNode AddStoryNode(StoryNodeData data)
        {
            var node = new StoryNode(this, data);
            AddElement(node);

            return node;
        }

        internal SubGraphNode AddSubGraphNode(SubGraphNodeData data)
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
                            results.Add(AddStoryNode(story));
                            break;

                        case SubGraphNodeData sub:
                            results.Add(AddSubGraphNode(sub));
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
                var result = AddConn(i);
                if (result != null)
                    results.Add(result);
            });
            return results;
        }

        private Edge AddConn(ConnectionData data)
        {
            var output = ports.FirstOrDefault(j => j.direction == Direction.Output && ((GraphNode)j.node).GUID == data.FromGUID && j.portName == data.FromPortName);
            var input = ports.FirstOrDefault(j => j.direction == Direction.Input && ((GraphNode)j.node).GUID == data.ToGUID && j.portName == data.ToPortName);

            if (input == null || output == null) return null;
            return ConnectPort(output, input);
        }

        private Edge ConnectPort(Port from, Port to)
        {
            var edge = from.ConnectTo(to);
            AddElement(edge);
            return edge;
        }

        internal Vector2 GetMousePosition(Vector2 world)
            => contentViewContainer.WorldToLocal(world);

        internal void Save() => window.SaveChanges();

        internal void OnNodeAdded(NodeData data)
        {
            if (nodes.Any(i => i is GraphNode node && node.GUID == data.GUID)) return;
            switch (data)
            {
                case StoryNodeData story:
                    AddStoryNode(story);
                    break;

                case SubGraphNodeData sub:
                    AddSubGraphNode(sub);
                    break;
            }
        }

        internal void OnNodeRemoved(List<string> guids)
        {
            DeleteElements(nodes.Where(i => i is GraphNode node && guids.Contains(node.GUID)));
        }

        internal void OnNodeMoved(string id, Vector2 pos)
        {
            var node = nodes.Where(i => i is GraphNode node && node.GUID == id)
                .FirstOrDefault();
            if (node != null) node.SetPosition(new(pos, node.GetPosition().size));
        }

        internal void OnEdgeAdded(ConnectionData data)
        {
            if (!edges.Any(i => i.Match(data)))
                AddConn(data);
        }

        internal void OnEdgeRemoved(ConnectionData data)
        {
            DeleteElements(edges.Where(i => i.Match(data)));
        }

        internal void Select(List<ISelectable> selections)
        {
            ClearSelection();
            selections.ForEach(i => AddToSelection(i));
        }

        internal void NotifyStoryNodeReparse()
        {
            nodes.Where(i => i is StoryNode).Cast<StoryNode>()
                .ToList().ForEach(i => i.ParseStory());
        }
    }
}