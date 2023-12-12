using System.Collections.Generic;

namespace Hamstory
{
    public abstract class Sentence
    {
        public virtual void OnParse(int index, StoryParser parser) { }
        public virtual void Execute(StoryExecutorBase executor) => executor.Continue();
    }

    public abstract class OpenSentence : Sentence
    {
        public bool IsOpen { get; private set; } = true;
        public int ExitPoint { get; private set; }

        public void Close(int exitPoint)
        {
            IsOpen = false;
            ExitPoint = exitPoint;
        }

        public virtual void OnExecuteInside(StoryExecutorBase executor) { }
    }
}