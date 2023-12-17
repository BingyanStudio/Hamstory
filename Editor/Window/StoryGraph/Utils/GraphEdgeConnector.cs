using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hamstory.Editor
{
    internal class GraphEdgeConnector : IEdgeConnectorListener
    {
        private StoryGraphViewModel viewModel;

        internal GraphEdgeConnector(StoryGraphViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            (graphView as StoryGraphView).viewModel.AddConn(edge.ToConnData(), false);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            viewModel.ShowSearchWindow(position, edge);
        }
    }
}