using System.Collections.Generic;

namespace Hamstory
{
    public class StnClose : Sentence
    {
        public override void Execute(StoryExecutorBase executor)
        {
            executor.PopState();
            executor.Continue();
        }
    }

    public class StnSay : Sentence
    {
        private string characterKey;
        private string content;
        private string extra;

        public StnSay(string characterKey, string content, string extra = "")
        {
            this.characterKey = characterKey;
            this.content = content;
            this.extra = extra;
        }

        public override void Execute(StoryExecutorBase executor)
        {
            if (characterKey.Length > 0) executor.SetCharacter(characterKey, extra);
            else executor.ClearCharacter();
            executor.SetText(content);
        }
    }

    public class StnMenu : OpenSentence
    {
        private List<MenuOption> options = new();

        public override void Execute(StoryExecutorBase executor)
        {
            executor.CreateMenu(options);
            executor.PushState(this);
        }

        public void AddOption(string option, int targetIndex)
        {
            options.Add(new(option, targetIndex + 1));
        }

        public override void OnExecuteInside(StoryExecutorBase executor)
        {
            if (executor.GetCurrentSentence() is StnMenuItem) executor.Goto(ExitPoint);
        }
    }

    public class StnMenuItem : Sentence
    {
        private StnMenu menu;
        private string content;

        public StnMenuItem(string content, StnMenu menu)
        {
            this.content = content;
            this.menu = menu;
        }
    }

    public struct MenuOption
    {
        public string Content { get; set; }
        public readonly int SentenceIndex;

        public MenuOption(string content, int sentenceIndex)
        {
            Content = content;
            SentenceIndex = sentenceIndex;
        }
    }

    public class StnIf : OpenSentence
    {
        private string expression;

        private List<(StnElseIf, int)> stnElseIfs = new();
        private int elsePoint;
        private bool hasElse = false;

        public StnIf(string expression)
        {
            this.expression = expression;
        }

        public override void Execute(StoryExecutorBase executor)
        {
            executor.PushState(this);
            if (executor.Predicate(expression)) { executor.Continue(); return; }
            else foreach (var item in stnElseIfs)
                    if (executor.Predicate(item.Item1.Expression))
                    {
                        executor.Goto(item.Item2);
                        return;
                    }


            if (hasElse) executor.Goto(elsePoint);
            else executor.Goto(ExitPoint);
        }

        public override void OnExecuteInside(StoryExecutorBase executor)
        {
            var stn = executor.GetCurrentSentence();
            if (stn is StnElseIf || stn is StnElse) executor.Goto(ExitPoint);
        }

        public bool AddElseIf(StnElseIf elseIf, int index)
        {
            if (hasElse) return false;
            stnElseIfs.Add((elseIf, index));
            return true;
        }

        public bool SetElse(int index)
        {
            if (hasElse) return false;
            elsePoint = index;
            hasElse = true;
            return true;
        }
    }

    public class StnElseIf : Sentence
    {
        public string Expression { get; private set; }

        public StnElseIf(string expression)
        {
            this.Expression = expression;
        }
    }

    public class StnElse : Sentence { }

    public class StnJump : Sentence
    {
        private string tag;

        public StnJump(string tag)
        {
            this.tag = tag;
        }

        public override void Execute(StoryExecutorBase executor)
        {
            if (tag.Length == 0) executor.JumpToNext();
            else executor.JumpTo(tag);
        }
    }
}