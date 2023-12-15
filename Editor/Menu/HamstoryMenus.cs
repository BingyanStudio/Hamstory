using System.Collections;
using System.Collections.Generic;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace Hamstory
{
    public static class HamstoryMenus
    {
        [MenuItem("Assets/Create/Hamstory/StoryText")]
        public static void CreateStoryText()
        {
            // EditorPrefs.
            var ts = new TextAsset();
            ProjectWindowUtil.CreateAssetWithContent("Story.hamstory", "[Char] A, B\r\nA: 你好!\r\nB: 欢迎使用 Hamstory !");
        }
    }
}