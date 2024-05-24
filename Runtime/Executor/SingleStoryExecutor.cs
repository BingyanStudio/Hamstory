using System;
using Bingyan;
using UnityEngine;

namespace Hamstory
{
    [AddComponentMenu("Hamstory/SingleStoryExecutor")]
    public class SingleStoryExecutor : StoryExecutorBase
    {
        [SerializeField, Title("开始时执行")] private bool executeOnAwake = false;
        [SerializeField, Title("故事脚本")] private TextAsset storyText;

        [SerializeField, HideInInspector] private CharacterConfig[] characters;

        private void Awake()
        {
            if (executeOnAwake) Execute(storyText);
        }

        public override void Execute(Action<string> callback = null)
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
    }
}