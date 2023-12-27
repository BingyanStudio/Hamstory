using System;

namespace Hamstory
{
    public abstract class SentenceParser
    {
        public abstract bool CanParse(string line);
        public abstract void Parse(string content, StoryParser parser);
    }

    public abstract class HeaderParser : SentenceParser
    {
        public abstract string Header { get; }
    }

    public abstract class CommandParser : HeaderParser
    {
        public override bool CanParse(string line)
            => line.ToLower() == Header.ToLower();
    }

    public abstract class NoContentCommandParser : CommandParser
    {
        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length > 0) parser.Warn($"[{Header}]后方不应当出现其他文字");
            CreateSentence(parser);
        }

        protected abstract void CreateSentence(StoryParser parser);
    }

    public abstract class PrefixParser : HeaderParser
    {
        public override bool CanParse(string line)
            => line.StartsWith(Header, true, System.Globalization.CultureInfo.DefaultThreadCurrentCulture);
    }
}