using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamstory
{
    public abstract class StoryExecutorBase : MonoBehaviour
    {
        // 执行器
        protected Story story;
        protected int index = 0;
        protected Stack<OpenSentence> state;

        protected Coroutine coroutine;
        protected bool running = false;

        public virtual void Execute(TextAsset storyText)
        {
            StoryParser.Parse(storyText.name, storyText.text, out var story);
            Execute(story);
        }

        public virtual void Execute(Story story)
        {
            this.story = story;

            if (coroutine != null) StopAllCoroutines();

            index = 0;
            running = true;
            state?.Clear();
            state = state ?? new();
            coroutine = StartCoroutine(_Execute());
        }

        private IEnumerator _Execute()
        {
            while (index < story.Length)
            {
                running = false;
                if (state.Count > 0) state.Peek().OnExecuteInside(this);
                story.GetSentence(index).Execute(this);
                yield return new WaitUntil(() => running);
            }

            OnFinish();
        }

        // 基础功能
        public abstract VisualProvider Visual { get; }
        public virtual GameObject GetDialogPanel() => Visual.GetDialogPanel();
        public abstract void SetCharacter(string key, string extra = "");
        public virtual void ClearCharacter() => Visual.ClearCharacter();
        public virtual void SetText(string content) => Visual.SetText(this, Data ? Data.Serialize(this, content) : content);
        public virtual void CreateMenu(List<MenuOption> options) => Visual.CreateMenu(this, options);
        public virtual void ClearMenu() => Visual.ClearMenu();

        // 处理变量
        public abstract DataProvider Data { get; }
        public virtual bool Predicate(string expression) => Data.Predicate(this, expression);

        // 状态控制
        public virtual void PushState(OpenSentence sentence) => state.Push(sentence);
        public virtual void PopState() => state.Pop();
        public virtual Sentence GetCurrentSentence() => story.GetSentence(index);
        public virtual void OnFinish()
        {
            End();
            GetDialogPanel().SetActive(false);
        }

        // 流程控制
        public virtual void Continue()
        {
            running = true;
            index++;
        }

        public virtual void Goto(int target)
        {
            index = target;
            running = true;
        }

        public abstract void JumpTo(string target);
        public abstract void JumpToNext();

        public virtual void End()
        {
            StopCoroutine(coroutine);
            running = false;
        }

        // 日志
        public virtual void Warn(string msg)
        {
            Debug.LogWarning(($"剧情执行警告: {msg}"));
        }

        public virtual void Error(string msg)
            => throw new System.Exception($"剧情执行出错: 在{story.GetSentence(index)}\n{msg}");
    }
}