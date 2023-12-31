using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;

namespace Hamstory
{
    internal class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        internal event Action<string, Vector2> Selected;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.Add(new SearchTreeGroupEntry(new("创建节点")));
            list.Add(new SearchTreeGroupEntry(new("故事"), 1));

            AssetDatabase.GetAllAssetPaths()
                .Where(i => i.StartsWith("Assets") && (i.EndsWith(".txt") || i.EndsWith(".hamstory")))
                .ToList().ForEach(i =>
                    list.Add(new SearchTreeEntry(new(i.Substring(7, i.Length - 7))) { level = 2, userData = i })
                );

            list.Add(new SearchTreeGroupEntry(new("故事节点图"), 1));

            AssetDatabase.GetAllAssetPaths()
                .Where(i => i.StartsWith("Assets") && i.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<StoryGraph>(i))
                .ToList().ForEach(i =>
                {
                    int j = 2, k = i.Length;
                    while (j > 0 && k >= 0)
                    {
                        k--;
                        if (i[k] == '/') j--;
                    }
                    list.Add(new SearchTreeEntry(new(i.Substring(k, i.Length - k).Trim('/'))) { level = 2, userData = i });
                });

            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            Selected?.Invoke(entry.userData.ToString(), context.screenMousePosition);
            return true;
        }
    }
}