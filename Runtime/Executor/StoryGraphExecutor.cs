using System;
using Bingyan;
using UnityEngine;

namespace Hamstory
{
    [AddComponentMenu("Hamstory/StoryGraphExecutor")]
    public class StoryGraphExecutor : StoryExecutorBase
    {
        [SerializeField, Title("开始时执行")] private bool executeOnAwake = false;

        [SerializeField, Title("故事节点图")] private StoryGraph graph;

        private StoryChain chain;

        private void Awake()
        {
            chain = new(graph);

            if (executeOnAwake)
                Execute();
        }

        public override void Execute(Action<string> callback = null)
        {
            if (chain == null) chain = new(graph);
            else chain.Reset();
            Execute(chain.CurStory, callback);
        }

        public override void JumpTo(string target)
        {
            if (!chain.Next(target)) Execute(chain.CurStory);
            else base.OnFinish();
        }

        public override void JumpToNext()
        {
            if (!chain.Next()) Execute(chain.CurStory);
            else base.OnFinish();
        }

        public override CharacterConfig GetCharacter(string key)
            => chain.GetCurrentCharacter(key);

        public override void OnFinish()
        {
            if (chain.Next()) base.OnFinish();
            else Execute(chain.CurStory);
        }
    }
}