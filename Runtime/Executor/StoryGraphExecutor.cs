using Bingyan;
using UnityEngine;

namespace Hamstory
{
    public class StoryGraphExecutor : StoryExecutorBase
    {
        [SerializeField, Title("开始时执行")] private bool executeOnAwake = false;

        [SerializeField, Title("界面源")] private VisualProvider visual;
        [SerializeField, Title("数据源")] private DataProvider data;
        [SerializeField, Title("故事线")] private StoryGraph graph;

        private StoryChain chain;

        public override VisualProvider Visual => visual;
        public override DataProvider Data => data;

        private void Awake()
        {
            chain = new(graph);

            if (executeOnAwake)
                Execute();
        }

        public void Execute()
        {
            if (chain == null) chain = new(graph);
            else chain.Reset();
            Execute(chain.CurStory);
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

        public override void SetCharacter(string key, string extra = "")
        {
            visual.SetCharacter(chain.GetCurrentCharacter(key), extra);
        }

        public override void OnFinish()
        {
            if (chain.Next()) base.OnFinish();
            else Execute(chain.CurStory);
        }
    }
}