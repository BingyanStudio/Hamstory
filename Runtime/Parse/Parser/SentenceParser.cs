using System;

namespace Hamstory
{
    public abstract class SentenceParser
    {
        public virtual bool IsCommand { get; } = true;
        public abstract string Header { get; }
        public abstract void Parse(string content, StoryParser parser);
    }

    public abstract class CommandParser : SentenceParser
    {
        public override sealed bool IsCommand => true;

        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length > 0) parser.Warn($"[{Header}]后方不应当出现其他文字");
            CreateSentence(parser);
        }

        protected abstract void CreateSentence(StoryParser parser);
    }
}