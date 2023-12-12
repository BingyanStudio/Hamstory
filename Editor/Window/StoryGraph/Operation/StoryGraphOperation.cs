using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hamstory.Editor
{
    public abstract class StoryGraphOperation
    {
        protected StoryGraphView view;

        protected StoryGraphOperation(StoryGraphView view)
        {
            this.view = view;
        }

        public abstract void Undo();
    }

    public class CreateOperation : StoryGraphOperation
    {
        private GraphElement[] els;

        public CreateOperation(StoryGraphView view, params GraphElement[] els) : base(view)
        {
            this.els = els;
        }

        public CreateOperation(StoryGraphView view, IEnumerable<GraphElement> els) : base(view)
        {
            this.els = els.ToArray();
        }

        public override void Undo()
        {
            view.MarkDeletionAsUndo();
            view.DeleteElements(els);
        }
    }

    public class MoveOperation : StoryGraphOperation
    {
        private GraphNode node;
        private NodeData data;
        private Vector2 pos;

        public MoveOperation(StoryGraphView view, GraphNode node, NodeData data, Vector2 pos) : base(view)
        {
            this.node = node;
            this.data = data;
            this.pos = pos;
        }

        public override void Undo()
        {
            var rectPos = node.GetPosition();
            rectPos.position = pos;
            node.SetPosition(rectPos);
            data.Pos = pos;
        }
    }

    public class RemoveOperation : StoryGraphOperation
    {
        private StoryGraph graph;
        private List<NodeData> removedNodes;
        private List<ConnectionData> removedConns;

        public RemoveOperation(StoryGraphView view, StoryGraph graph, List<NodeData> removedNodes, List<ConnectionData> removedConns) : base(view)
        {
            this.graph = graph;
            this.removedNodes = removedNodes;
            this.removedConns = removedConns;
        }

        public override void Undo()
        {
            graph.AddNodes(removedNodes);
            view.BuildNodes(removedNodes);

            graph.Connections.AddRange(removedConns);
            view.BuildConns(removedConns);
        }
    }
}