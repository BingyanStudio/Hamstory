using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Hamstory
{
    public class StoryParser
    {
        private static Dictionary<string, SentenceParser> commandParsers;
        private static Dictionary<string, SentenceParser> prefixParsers;
        static StoryParser()
        {
            var types = typeof(StoryParser).Assembly.GetTypes();
            var parsers = types.Where(i => !i.IsAbstract && i.IsSubclassOf(typeof(SentenceParser)))
                        .Select(i => Activator.CreateInstance(i) as SentenceParser).ToList();
            commandParsers = parsers.Where(i => i.IsCommand).ToDictionary(i => i.Header.ToLower(), i => i);
            prefixParsers = parsers.Where(i => !i.IsCommand).ToDictionary(i => i.Header.ToLower(), i => i);
        }

        public static SentenceParser GetSentenceParser(string command)
        {
            if (!commandParsers.TryGetValue(command, out var value)) throw new Exception($"找不到与 [{command}] 对应的解析器！");
            return value;
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

        private bool Parse(out Story story) => Parse(new SayParser(), out story);

        private bool Parse(SayParser sayParser, out Story story)
        {
            try
            {
                if (sayParser == null) sayParser = new SayParser();

                for (lineIndex = 0; lineIndex < contents.Length; lineIndex++)
                {
                    // 空行，过！
                    if (this.line.Trim().Length == 0) continue;

                    // 删除缩进空格
                    var line = this.line.Trim();

                    // 是个指令
                    if (line.StartsWith('['))
                    {
                        int blockIndex = line.IndexOf(']');
                        if (blockIndex == -1) Error("方括号没有闭合");

                        var cmd = line.Substring(1, blockIndex - 1);
                        var cmdLower = cmd.ToLower();
                        if (!commandParsers.ContainsKey(cmdLower)) Error($"未知的标记: [{cmd}]");
                        commandParsers[cmdLower].Parse(line.Substring(blockIndex + 1, line.Length - blockIndex - 1).Trim(), this);
                    }
                    else
                    {
                        var prefixs = prefixParsers.Where(i => line.StartsWith(i.Key.ToLower())).ToArray();
                        if (prefixs.Length > 0)    // 交给前缀解析器
                        {
                            if (prefixs.Length > 1)
                                Warn($"有多个语句解析器与这一行匹配。选取 {prefixs[0].Value.GetType()} 进行解析");

                            var length = prefixs[0].Key.Length;
                            prefixs[0].Value.Parse(line.Substring(length, line.Length - length).Trim(), this);
                        }
                        else sayParser.Parse(line, this);    // 交给对话解析器
                    }

                }

                results.ForEach(i =>
                {
                    if (i is OpenSentence os && os.IsOpen)
                        Warn($"{i} 需要闭合，但并没有。这可能会带来意料意外的后果！\n请注意添加闭合符号 [/]");
                });

                story = new Story(results.Select(i => i).ToList(), characterDefs, jumps);
                return true;
            }
            catch (Exception)
            {
                story = null;
                return false;
            }
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