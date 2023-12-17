using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hamstory.Editor
{
    internal class StoryGraphViewModel
    {
        private static CopiedGraphData copiedData;

        private StoryGraph graph;
        private StoryGraphView view;
        private StoryGraphWindow window;

        private Stack<StoryGraphOperation> operations = new();
        private StoryGraphOperation opBatch;
        private bool undoing = false;

        private SearchWindowProvider searchProvider;
        private Port pendingPort;

        internal StoryGraphViewModel(StoryGraph graph, StoryGraphWindow window)
        {
            this.graph = graph;
            this.window = window;

            opBatch = new(this);

            searchProvider = SearchWindowProvider.CreateInstance<SearchWindowProvider>();
            searchProvider.Selected += OnSearchSelected;

            // 若是新的graph，则初始化节点
            if (graph.StartNode.GUID.Length == 0) graph.StartNode = new NodeData(GUID.Generate().ToString(), new(100, 200));
            if (graph.EndNode.GUID.Length == 0) graph.EndNode = new NodeData(GUID.Generate().ToString(), new(600, 200));
        }

        internal void Bind(StoryGraphView view)
        {
            this.view = view;
            view.InitGraph(graph);
        }

        internal void ShowSearchWindow(Vector2 pos, Edge targetEdge)
        {
            pendingPort = targetEdge.input ?? targetEdge.output;
            SearchWindow.Open(new(pos + window.position.position), searchProvider);
        }

        private void OnSearchSelected(string path, Vector2 pos)
        {
            pos -= window.position.position;
            pos = view.GetMousePosition(pos);

            if (pendingPort.direction == Direction.Input) pos.x -= 350;

            string newGUID;
            if (path.EndsWith(".asset"))
            {
                var graph = AssetDatabase.LoadAssetAtPath<StoryGraph>(path);
                newGUID = _CreateSubGraphNode(pos, graph);
            }
            else
            {
                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                newGUID = _CreateStoryNode(pos, text);
            }

            if (pendingPort.direction == Direction.Input)
                _AddConn(new(newGUID, StoryGraph.DEFAULT_PORT_NAME, ((GraphNode)pendingPort.node).GUID, pendingPort.portName));
            else
            {
                var conn = pendingPort.connections.FirstOrDefault();
                if (conn != null) _RemoveConn(conn.ToConnData());
                _AddConn(new(((GraphNode)pendingPort.node).GUID, pendingPort.portName, newGUID, StoryGraph.DEFAULT_PORT_NAME));
            }
            PushOperation();
        }

        internal string CreateStoryNode(Vector2 pos) => CreateStoryNode(pos, null);

        internal string CreateStoryNode(Vector2 pos, TextAsset text)
        {
            var guid = _CreateStoryNode(pos, text);
            PushOperation();
            return guid;
        }

        private string _CreateStoryNode(Vector2 pos, TextAsset text)
        {
            var guid = GUID.Generate().ToString();
            _AddStoryNode(new(guid, pos) { StoryText = text });
            return guid;
        }

        private void _AddStoryNode(StoryNodeData data)
        {
            graph.StoryNodes.Add(data);
            view.OnNodeAdded(data);

            opBatch.AddedNodes.Add(data);
        }

        internal string CreateSubGraphNode(Vector2 pos) => CreateSubGraphNode(pos, null);

        internal string CreateSubGraphNode(Vector2 pos, StoryGraph graph)
        {
            var guid = _CreateSubGraphNode(pos, graph);
            PushOperation();
            return guid;
        }

        private string _CreateSubGraphNode(Vector2 pos, StoryGraph graph)
        {
            var guid = GUID.Generate().ToString();
            _AddSubGraphNode(new(guid, pos)
            {
                Subgraph = graph
            });
            return guid;
        }

        private void _AddSubGraphNode(SubGraphNodeData data)
        {
            graph.SubGraphNodes.Add(data);
            view.OnNodeAdded(data);

            opBatch.AddedNodes.Add(data);
        }

        internal void AddNodes(List<NodeData> datas)
        {
            _AddNodes(datas);
            PushOperation();
        }

        private void _AddNodes(List<NodeData> datas)
        {
            datas.ForEach(i =>
            {
                if (i is StoryNodeData story) _AddStoryNode(story);
                else if (i is SubGraphNodeData sub) _AddSubGraphNode(sub);
            });
        }

        internal void RemoveNodes(List<NodeData> nodes, bool updateView = true)
            => RemoveNodes(nodes.Select(i => i.GUID).ToList(), updateView);

        internal void RemoveNodes(List<string> guids, bool updateView = true)
        {
            if (guids.Count == 0) return;
            _RemoveNodes(guids);
            PushOperation();

            if (updateView) view.OnNodeRemoved(guids);
        }

        private void _RemoveNodes(IEnumerable<string> guids, bool updateView = true)
        {
            opBatch.RemovedNodes.AddRange(graph.GetNodes(i => guids.Contains(i.GUID)));
            graph.RemoveAllNodes(i => guids.Contains(i.GUID));

            if (updateView) view.OnNodeRemoved(guids.ToList());
        }

        internal void MoveNodes(List<(string, Vector2)> datas, bool updateView = true)
        {
            _MoveNodes(datas);
            PushOperation();
        }

        private void _MoveNode(NodeData node, Vector2 pos, bool updateView = true)
        {
            Vector2 origin = node.Pos;
            node.Pos = pos;

            if (opBatch.MovedNodes.ContainsKey(node.GUID))
                opBatch.MovedNodes[node.GUID] = origin;
            else opBatch.MovedNodes[node.GUID] = origin;

            if (updateView) view.OnNodeMoved(node.GUID, pos);
        }

        private void _MoveNodes(List<(string, Vector2)> datas, bool updateView = true)
        {
            foreach (var data in datas)
            {
                if (data.Item1 == graph.StartNode.GUID) _MoveNode(graph.StartNode, data.Item2, updateView);
                else if (data.Item1 == graph.EndNode.GUID) _MoveNode(graph.EndNode, data.Item2, updateView);
                else _MoveNode(graph.GetNode(data.Item1), data.Item2, updateView);
            }
        }

        internal void CreateConn(Edge edge)
            => AddConn(edge.ToConnData());

        internal void CreateConn(string fromID, string fromPort, string toID, string toPort)
            => AddConn(new(fromID, fromPort, toID, toPort));

        internal void AddConn(ConnectionData data, bool updateView = true)
        {
            _AddConn(data, updateView);
            PushOperation();
        }

        private void _AddConn(ConnectionData data, bool updateView = true)
        {
            graph.Conns.Add(data);
            opBatch.AddedConns.Add(data);

            if (updateView) view.OnEdgeAdded(data);
        }

        internal void AddConns(List<ConnectionData> datas)
        {
            _AddConns(datas);
            PushOperation();
        }

        private void _AddConns(List<ConnectionData> datas, bool updateView = true)
        {
            datas.ForEach(i => _AddConn(i, updateView));
        }

        internal void RemoveConn(ConnectionData data, bool updateView = true)
        {
            _RemoveConn(data, updateView);
            PushOperation();
        }

        private void _RemoveConn(ConnectionData data, bool updateView = true)
        {
            graph.Conns.Remove(data);
            opBatch.RemovedConns.Add(data);

            if (updateView) view.OnEdgeRemoved(data);
        }

        internal void RemoveConns(List<ConnectionData> datas, bool updateView = true)
        {
            _RemoveConns(datas, updateView);
            PushOperation();
        }

        private void _RemoveConns(List<ConnectionData> datas, bool updateView = true)
        {
            datas.ForEach(i => _RemoveConn(i));
            if (updateView) datas.ForEach(i => view.OnEdgeRemoved(i));
        }

        internal void ApplyRemoveChanges(List<string> nodes, List<ConnectionData> conns)
        {
            _RemoveNodes(nodes, false);
            conns.ForEach(i => _RemoveConn(i, false));
            PushOperation();
        }

        internal void Copy(List<ISelectable> selections)
        {
            var nodes = selections.Where(i => i is GraphNode node && node.GUID != graph.StartNode.GUID && node.GUID != graph.EndNode.GUID)
                .Cast<GraphNode>()
                .Select(i => i.GUID);
            var conns = selections.Where(i => i is Edge edge).Cast<Edge>();

            var nodeDatas = graph.GetNodes(i => nodes.Contains(i.GUID)).ToList();
            var connDatas = conns.Select(i => i.ToConnData()).ToList();
            if (nodeDatas.Count == 0) return;

            copiedData = new(nodeDatas, connDatas);
        }

        internal void Paste(Vector2 pos, bool relative = false)
        {
            if (copiedData == null) return;
            copiedData.GetClonedNodeDatas(pos, relative)
                .ForEach(i =>
                {
                    if (i is StoryNodeData story) _AddStoryNode(story);
                    else if (i is SubGraphNodeData sub) _AddSubGraphNode(sub);
                });
            copiedData.GetClonedConnDatas().ForEach(i => _AddConn(i));
            PushOperation();
        }

        internal void Undo()
        {
            if (operations.Count > 0)
            {
                undoing = true;
                var op = operations.Pop();

                _MoveNodes(op.MovedNodes.Select(i => (i.Key, i.Value)).ToList());

                _RemoveConns(op.AddedConns);
                _RemoveNodes(op.AddedNodes.Select(i => i.GUID));

                _AddNodes(op.RemovedNodes);
                _AddConns(op.RemovedConns);

                undoing = false;
                opBatch = new(this);
            }
        }

        private void PushOperation()
        {
            if (undoing) return;

            Debug.Log($"push: {opBatch.AddedNodes.Count}, {opBatch.AddedConns.Count}, {opBatch.RemovedNodes.Count}, {opBatch.RemovedConns.Count}, {opBatch.MovedNodes.Count}");

            operations.Push(opBatch);
            opBatch = new(this);
        }
    }
}