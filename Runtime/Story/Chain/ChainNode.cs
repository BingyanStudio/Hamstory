using System.Collections.Generic;
using UnityEngine;

namespace Hamstory
{
    internal abstract class ChainNode
    {
        internal NodeData Data => data;
        private NodeData data;

        protected Dictionary<string, (StoryGraph, int)> nexts = new();

        protected ChainNode(NodeData data)
        {
            this.data = data;
        }

        internal void AddNext(string key, StoryGraph graph, int index)
        {
            nexts.TryAdd(key, (graph, index));
        }

        internal abstract bool Next(StoryChain chain, string key);
    }

    internal class StoryChainNode : ChainNode
    {
        public Story Story => story;
        private Story story;
        public StoryNodeData StoryData => Data as StoryNodeData;

        internal StoryChainNode(StoryNodeData data) : base(data)
        {
            StoryParser.Parse(data.StoryText, out story);
        }

        internal override bool Next(StoryChain chain, string key)
        {
            if (nexts.TryGetValue(key, out var dest))
                return chain.JumpTo(dest.Item1, dest.Item2);
            return true;
        }
    }

    internal class SubGraphChainNode : ChainNode
    {
        private StoryGraph graph;

        internal SubGraphChainNode(SubGraphNodeData data) : base(data)
        {
            this.graph = data.Subgraph;
        }

        internal override bool Next(StoryChain chain, string key)
        {
            if (nexts.TryGetValue(key, out var dest))
                chain.PushTransfer(dest);
            return chain.JumpTo(graph, 0);
        }
    }

    internal class EndChainNode : ChainNode
    {
        internal EndChainNode(NodeData data) : base(data) { }

        internal override bool Next(StoryChain chain, string key)
        {
            return chain.End();
        }
    }
}