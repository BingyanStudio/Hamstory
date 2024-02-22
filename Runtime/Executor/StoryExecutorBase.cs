using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Hamstory
{
    public abstract class StoryExecutorBase : MonoBehaviour
    {
        /// <summary>
        /// 剧情结束时触发的回调
        /// </summary>
        public event Action Finished;

        /// <summary>
        /// 角色发生变化时触发的回调<br/>
        /// 如果切换为旁白，或角色被清空，则传递 <see cref="null"/>
        /// </summary>
        public event Action<CharacterConfig> CharacterChanged;

        // 执行器
        protected Story story;
        protected int index = 0;
        protected Stack<OpenSentence> state;

        protected Coroutine coroutine;
        protected bool running = false;

        // 缓存
        private string currentCharKey = "";

        // 回调
        private Action cbkExecuteEnded;

        /// <summary>
        /// 执行一个故事，并在执行完毕后调用回调
        /// </summary>
        /// <param name="storyText">故事文本</param>
        /// <param name="callback">回调</param>
        public virtual void Execute(TextAsset storyText, Action callback = null)
        {
            StoryParser.Parse(storyText.name, storyText.text, out var story);
            Execute(story, callback);
        }

        /// <summary>
        /// 执行一个故事，并在执行完毕后调用回调
        /// </summary>
        /// <param name="story">故事</param>
        /// <param name="callback">回调</param>
        public virtual void Execute(Story story, Action callback = null)
        {
            this.story = story;
            cbkExecuteEnded = callback;

            if (coroutine != null) StopAllCoroutines();

            index = 0;
            running = true;
            state?.Clear();
            state ??= new();
            currentCharKey = "";
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
        public abstract CharacterConfig GetCharacter(string key);
        public virtual void SetCharacter(string key, string extra = "")
        {
            if (!currentCharKey.Equals(key))
            {
                CharacterChanged?.Invoke(key.Length == 0 ? null : GetCharacter(key));
                currentCharKey = key;
            }
        }
        public virtual void ClearCharacter()
        {
            CharacterChanged?.Invoke(null);
            currentCharKey = "";
            Visual.ClearCharacter();
        }
        public virtual void SetText(string content) => Visual.SetText(this, Data ? Data.Serialize(this, content) : content);
        public virtual void CreateMenu(List<MenuOption> options) => Visual.CreateMenu(this, Data ? options.Select(i =>
        {
            i.Content = Data.Serialize(this, i.Content);
            return i;
        }).ToList() : options);
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
            Finished?.Invoke();
            cbkExecuteEnded?.Invoke();
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