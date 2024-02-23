using System;
using Bingyan;
using UnityEngine;

namespace Hamstory
{
    public class SingleStoryExecutor : StoryExecutorBase
    {
        [SerializeField, Title("开始时执行")] private bool executeOnAwake = false;
        [SerializeField, Title("界面源")] private VisualProvider visual;
        [SerializeField, Title("数据源")] private DataProvider data;
        [SerializeField, Title("故事脚本")] private TextAsset storyText;

        [SerializeField, HideInInspector] private CharacterConfig[] characters;

        public override VisualProvider Visual => visual;

        public override DataProvider Data => data;

        private void Awake()
        {
            if (executeOnAwake) Execute(storyText);
        }

        public void Execute(Action<string> callback = null)
        {
            Execute(storyText, callback);
        }

        public override void JumpTo(string target)
        {
            Warn("单剧情脚本执行器不支持跨脚本跳转！");
        }

        public override void JumpToNext()
        {
            Warn("单剧情脚本执行器不支持跨脚本跳转！");
        }

        public override CharacterConfig GetCharacter(string key)
            => characters[story.Characters.IndexOf(key)];

        public override void SetCharacter(string key, string extra = "")
        {
            base.SetCharacter(key, extra);
            visual.SetCharacter(GetCharacter(key), extra);
        }

        /// <summary>
        /// 设置当前执行器的 <see cref="VisualProvider"/>
        /// </summary>
        public void SetVisual(VisualProvider visual)
        {
            this.visual = visual;
        }

        /// <summary>
        /// 设置当前执行器的 <see cref="DataProvider"/>
        /// </summary>
        public void SetData(DataProvider data)
        {
            this.data = data;
        }
    }
}