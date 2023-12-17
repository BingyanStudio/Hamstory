using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hamstory.Editor
{
    internal class GraphEdgeConnector : IEdgeConnectorListener
    {
        private StoryGraphView view;

        internal GraphEdgeConnector(StoryGraphView view)
        {
            this.view = view;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            (graphView as StoryGraphView).viewModel.AddConn(edge.ToConnData(), false);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            view.ShowSearchWindow(position, edge);
        }
    }
}