using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;

namespace Hamstory
{
    public class StoryParser
    {
        private static List<SentenceParser> parsers;
        private static SayParser sayParser = new();

        static StoryParser()
        {
            var types = typeof(StoryParser).Assembly.GetTypes();
            parsers = types.Where(i => !i.IsAbstract && i.IsSubclassOf(typeof(SentenceParser)) && i != typeof(SayParser))
                        .Select(i => Activator.CreateInstance(i) as SentenceParser).ToList();
        }

        public static void RegisterParser(SentenceParser parser)
        {
            if (parser != null) parsers.Add(parser);
        }

        public static void UnregisterParser(SentenceParser parser)
        {
            if (parsers.Contains(parser))
                parsers.Remove(parser);
        }

        public static void SetSayParser(SayParser parser)
        {
            if (parser != null) sayParser = parser;
        }

        public static bool Parse(string path, string content, out Story result)
            => new StoryParser(path, content).Parse(out result);

        public static bool Parse(TextAsset text, out Story result)
            => Parse(text.name, text.text, out result);

        private string filePath;

        private string[] contents;
        private int lineIndex;
        private string line => contents[lineIndex];

        private List<string> characterDefs = new();
        private List<string> jumps = new();
        private List<Sentence> results = new();

        /// <summary>
        /// 最后一个语句的序号
        /// </summary>
        public int LastSentenceIdx => results.Count - 1;

        private StoryParser(string filePath, string content)
        {
            this.filePath = filePath;
            contents = content.Split('\n');
            lineIndex = 0;
        }

        private bool Parse(out Story story)
        {
            var commandParsers = parsers.Where(i => i is CommandParser).Cast<CommandParser>()
                .ToDictionary(i => i.Header.ToLower(), i => i);

            for (lineIndex = 0; lineIndex < contents.Length; lineIndex++)
            {
                if (this.line.Trim().Length == 0) continue;
                var line = this.line.Trim();

                SentenceParser parser = null;
                string content = line;

                // 是个指令
                if (line.StartsWith('['))
                {
                    int rb = line.IndexOf(']');
                    if (rb == -1) Error("指令方括号没有闭合");

                    var cmd = line.Substring(1, rb - 1).ToLower();
                    if (commandParsers.TryGetValue(cmd, out var p))
                    {
                        parser = p;
                        content = line.Substring(rb + 1, line.Length - rb - 1).Trim();
                    }
                    else Error($"未知的标记: [{cmd}]");
                }
                else
                {
                    var availableParsers = parsers.Where(i => i.CanParse(line)).ToArray();
                    if (availableParsers.Length > 0)    // 交给前缀解析器
                    {
                        if (availableParsers.Length > 1)
                        {
                            var sb = new StringBuilder($"有多个语句解析器与这一行匹配。选取 {availableParsers[0].GetType()} 进行解析。");
                            sb.AppendLine("其他解析器是: ");
                            for (int i = 1; i < availableParsers.Length; i++)
                                sb.AppendLine(availableParsers[i].GetType().ToString());
                            Warn(sb.ToString());
                        }
                        parser = availableParsers[0];
                    }
                    else parser = sayParser;  // 交给对话解析器
                }

                parser.Parse(content, this);
            }

            results.ForEach(i =>
            {
                if (i is OpenSentence os && os.IsOpen)
                    Warn($"{i} 需要闭合，但并没有。这可能会带来意料意外的后果！\n请注意添加闭合符号 [/]");
            });

            story = new Story(results.Select(i => i).ToList(), characterDefs, jumps);
            return true;
        }

        private int GetIndent()
        {
            int indent = 0;
            while (line[indent] == ' ') indent++;
            if (indent % 4 != 0) Error("缩进数量不正确，空格应当为4的整数倍");
            indent /= 4;
            return indent;
        }

        public void Warn(string msg)
            => Debug.LogWarning($"剧情解析警告: {filePath} 第{lineIndex + 1}行:\n{msg}");

        public void Error(string msg)
        {
            Debug.LogError($"剧情解析出错: {filePath} 第{lineIndex + 1}行:\n{msg}");
            throw new Exception("解析剧情时出错");
        }

        public void RegisterCharacter(string name)
        {
            characterDefs.Add(name);
        }

        public void RegisterJump(string name)
        {
            jumps.Add(name);
        }

        public bool HasCharacter(string key) => characterDefs.Contains(key);

        public int AddSentence(Sentence sentence)
        {
            results.Add(sentence);
            return results.Count - 1;
        }

        public bool SearchBack<T>(int from, out T sentence) where T : Sentence
            => SearchBack<T>(from, s => true, out sentence);

        public bool SearchBack<T>(int from, Func<T, bool> predicate, out T sentence) where T : Sentence
        {
            for (int i = from; i >= 0; i--)
                if (results[i] is T result && predicate(result))
                {
                    sentence = result;
                    return true;
                }

            sentence = null;
            return false;
        }
    }
}