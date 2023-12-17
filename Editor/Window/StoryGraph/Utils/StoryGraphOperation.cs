using System.Collections.Generic;
using UnityEngine;

namespace Hamstory.Editor
{
    internal class StoryGraphOperation
    {
        protected StoryGraphViewModel viewModel;
        internal List<NodeData> AddedNodes { get; } = new();
        internal List<NodeData> RemovedNodes { get; } = new();
        internal Dictionary<string, Vector2> MovedNodes { get; } = new();
        internal List<ConnectionData> AddedConns { get; } = new();
        internal List<ConnectionData> RemovedConns { get; } = new();

        internal StoryGraphOperation(StoryGraphViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        internal bool IsEmpty()
            => AddedNodes.Count == 0
                && AddedConns.Count == 0
                && RemovedNodes.Count == 0
                && RemovedConns.Count == 0
                && MovedNodes.Count == 0;
    }
}