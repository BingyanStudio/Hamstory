using System.Linq;
using System.Collections.Generic;

namespace Hamstory
{
    public class StoryChain
    {
        public const string DEFAULT_BRANCH = "→";

        public bool Ended { get; private set; } = false;
        public Story CurStory => ((StoryChainNode)curNode).Story;

        private Dictionary<StoryGraph, List<ChainNode>> nodes = new();
        private Stack<(StoryGraph, int)> stack = new();

        private StoryGraph curGraph, rootGraph;
        private int index = 0;

        private ChainNode curNode => nodes[curGraph][index];

        private Queue<StoryGraph> buildQueue;
        public StoryChain(StoryGraph graph)
        {
            rootGraph = graph;

            buildQueue = new();
            buildQueue.Enqueue(graph);

            while (buildQueue.Count > 0)
            {
                var target = buildQueue.Dequeue();
                if (nodes.ContainsKey(target)) continue;

                var list = new List<ChainNode>();
                var conn = target.GetNodeConns(target.StartNode).FirstOrDefault();
                if (conn != null)
                {
                    var data = target.GetOutputNode(conn);
                    if (data is StoryNodeData d1) list.Add(new StoryChainNode(d1));
                    else if (data is SubGraphNodeData d2) list.Add(new SubGraphChainNode(d2));

                    BuildChain(target, list, list[0]);
                }

                nodes.Add(target, list);
            }
        }

        private void BuildChain(StoryGraph graph, List<ChainNode> list, ChainNode cur)
        {
            var conns = graph.GetNodeConns(cur.Data);
            conns.ForEach(i =>
            {
                var data = graph.GetOutputNode(i);
                cur.AddNext(i.FromPortName, graph, list.Count);
                if (data == graph.EndNode)
                    list.Add(new EndChainNode(graph.EndNode));

                else if (data is StoryNodeData d1)
                {
                    var newNode = new StoryChainNode(d1);
                    list.Add(newNode);
                    BuildChain(graph, list, newNode);
                }
                else if (data is SubGraphNodeData d2)
                {
                    var newNode = new SubGraphChainNode(d2);
                    list.Add(newNode);
                    buildQueue.Enqueue(d2.Subgraph);
                    BuildChain(graph, list, newNode);
                }
            });
        }

        public void Reset()
        {
            curGraph = rootGraph;
            index = 0;
        }

        public bool Next() => curNode.Next(this, DEFAULT_BRANCH);


        public bool Next(string key) => Ended || curNode.Next(this, key);


        public CharacterConfig GetCurrentCharacter(string key)
        {
            var storyNode = curNode as StoryChainNode;
            int idx = storyNode.Story.Characters.IndexOf(key);
            if (idx == -1) throw new System.Exception($"角色 {key} 未配置!");
            return storyNode.StoryData.Characters[idx];
        }

        internal bool JumpTo(StoryGraph graph, int index)
        {
            curGraph = graph;
            this.index = index;

            if (curNode is SubGraphChainNode sub) return sub.Next(this, DEFAULT_BRANCH);
            if (curNode is EndChainNode end) return end.Next(this, "");
            return false;
        }

        internal bool End()
        {
            if (stack.Count > 0)
            {
                var dest = stack.Pop();
                return JumpTo(dest.Item1, dest.Item2);
            }
            Ended = true;
            return true;
        }

        internal void PushTransfer((StoryGraph, int) destination)
        {
            stack.Push(destination);
        }
    }
}