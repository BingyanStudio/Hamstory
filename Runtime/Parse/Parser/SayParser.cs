using UnityEngine;

namespace Hamstory
{
    public class SayParser : SentenceParser
    {
        public override bool CanParse(string line) => false;

        public override void Parse(string content, StoryParser parser)
        {
            if (!content.Contains(':'))
            {
                Debug.Log($"{content}");
                return;
            }

            int spliterIdx = content.IndexOf(':');
            var host = content.Substring(0, spliterIdx);
            if (host.Length != 0 && !parser.HasCharacter(host)) parser.Error($"角色 {host} 没有在 [Character] 处定义!");

            var msg = content.Substring(spliterIdx + 1, content.Length - spliterIdx - 1);
            var extra = "";

            if (host.EndsWith(')'))
            {
                int leftIdx = host.IndexOf('(');
                if (leftIdx == -1) parser.Warn("此处可能缺少左括号?");
                else
                {
                    extra = host.Substring(leftIdx + 1, host.Length - leftIdx - 2);
                    host = host.Substring(0, leftIdx);
                }
            }

            var stn = new StnSay(host, msg, extra);
            parser.AddSentence(stn);
        }
    }
}