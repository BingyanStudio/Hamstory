using System;
using System.Text;

namespace Hamstory
{
    public abstract class BasicDataProvider : DataProvider
    {
        public override bool Predicate(StoryExecutorBase executor, string expression)
        {
            var result = false;
            if (TryPredicate(expression, "==", (l, r) => l.Equals(r), out result)) return result;
            if (TryPredicate(expression, "!=", (l, r) => !l.Equals(r), out result)) return result;

            if (TryPredicate(expression, ">=", (l, r) =>
                {
                    if (!Compare(l, r, out var cmp)) return false;
                    return cmp >= 0;
                }, out result)) return result;

            if (TryPredicate(expression, "<=", (l, r) =>
                {
                    if (!Compare(l, r, out var cmp)) return false;
                    return cmp <= 0;
                }, out result)) return result;

            if (TryPredicate(expression, ">", (l, r) =>
                {
                    if (!Compare(l, r, out var cmp)) return false;
                    return cmp > 0;
                }, out result)) return result;

            if (TryPredicate(expression, "<", (l, r) =>
                {
                    if (!Compare(l, r, out var cmp)) return false;
                    return cmp < 0;
                }, out result)) return result;

            executor.Error("未识别出表达式的判断符号\n支持的符号：\n==\n!=\n>=\n>\n<=\n<");
            return false;
        }

        public override string Serialize(StoryExecutorBase executor, string content)
        {
            if (!content.Contains('{')) return content;

            StringBuilder sb = new();
            int idx = 0;
            int l, r;
            while ((l = content.IndexOf('{', idx)) != -1)
            {
                if ((r = content.IndexOf('}', l)) == -1) executor.Error("大括号没有闭合");
                else
                {
                    sb.Append(content[idx..l]);
                    sb.Append(Get(content.Substring(l + 1, r - l - 1)));
                    idx = r + 1;
                }
            }
            sb.Append(content[idx..]);
            return sb.ToString();
        }

        private bool TryPredicate(string expression, string op, Func<object, object, bool> action, out bool result)
        {
            result = false;
            if (expression.Contains(op))
            {
                var parts = expression.Split(op);
                if (parts.Length < 2) return false;
                result = action.Invoke(ParseVar(parts[0]), ParseVar(parts[1]));
                return true;
            }
            return false;
        }

        protected virtual bool Compare(object l, object r, out int result)
        {
            result = 0;
            if (l is not IComparable com) return false;
            if (l.GetType() != r.GetType()) return false;
            result = com.CompareTo(r);
            return true;
        }

        protected virtual object ParseVar(string key)
        {
            key = key.Trim();
            if (key.Length == 0) return null;

            // 变量
            if (key.StartsWith("{") && key.EndsWith("}")) return Get(key.TrimStart('{').TrimEnd('}'));

            // 整数
            if (int.TryParse(key, out var intVal)) return intVal;

            // 小数
            if (float.TryParse(key, out var fltVal)) return fltVal;

            // 字符串
            return key;
        }

        protected abstract object Get(string key);
    }
}