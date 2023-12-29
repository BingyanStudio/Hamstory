namespace Hamstory
{
    public class CloseParser : NoContentCommandParser
    {
        public override string Header => "/";

        protected override void CreateSentence(StoryParser parser)
        {
            int index = parser.AddSentence(new StnClose());
            if (parser.SearchBack<OpenSentence>(index - 1, s => s.IsOpen, out var os))
                os.Close(index);
            else parser.Warn("这个闭合符号无法找到需要闭合的语句块");
        }
    }

    public class CharacterDefParser : CommandParser
    {
        public override string Header => "Char";

        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length == 0) return;
            foreach (var item in content.Split(','))
                parser.RegisterCharacter(item.Trim());
        }
    }

    public class MenuParser : NoContentCommandParser
    {
        public override string Header => "Menu";

        protected override void CreateSentence(StoryParser parser)
        {
            parser.AddSentence(new StnMenu());
        }
    }

    public class MenuItemParser : PrefixParser
    {
        public override string Header => "-";

        public override void Parse(string content, StoryParser parser)
        {
            content = content.Substring(Header.Length, content.Length - Header.Length).Trim();

            if (content.Length == 0)
            {
                parser.Error("菜单选项似乎没有给定内容?\n正确示例: \"- 这是一个菜单\"");
                return;
            }

            int index = parser.LastSentenceIdx;
            if (parser.SearchBack<StnMenu>(index, s => s.IsOpen, out var menu))
            {
                menu.AddOption(content, index + 1);
                parser.AddSentence(new StnMenuItem(content, menu));
            }
            else parser.Error($"菜单选项\"- {content}\"需要位于一个 [Menu] 下！");
        }
    }

    public class IfParser : CommandParser
    {
        public override string Header => "If";

        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length == 0) parser.Error("[if] 的条件不能为空!");
            else parser.AddSentence(new StnIf(content));
        }
    }

    public class ElseIfParser : CommandParser
    {
        public override string Header => "Elif";

        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length == 0)
            {
                parser.Error("[elif] 的条件不能为空!");
                return;
            }

            int index = parser.LastSentenceIdx;
            if (parser.SearchBack<StnIf>(index, s => s.IsOpen, out var stnIf))
            {
                var stn = new StnElseIf(content);
                stnIf.AddElseIf(stn, index);
                parser.AddSentence(stn);
            }
            else parser.Error("[Elif] 需要放在某个 [If] 或另一个 [Elif] 下方！");
        }
    }

    public class ElseParser : NoContentCommandParser
    {
        public override string Header => "Else";

        protected override void CreateSentence(StoryParser parser)
        {
            int index = parser.AddSentence(new StnElse());
            if (parser.SearchBack<StnIf>(index - 1, s => s.IsOpen, out var stnElseIf))
                stnElseIf.SetElse(index);
            else parser.Error("[Else] 需要放在某个 [If] 或另一个 [Elif] 下方！");
        }
    }

    public class JumpParser : CommandParser
    {
        public override string Header => "Jump";

        public override void Parse(string content, StoryParser parser)
        {
            if (content.Length > 0) parser.RegisterJump(content);
            parser.AddSentence(new StnJump(content));
        }
    }
}