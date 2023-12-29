using UnityEngine;
using UnityEditor;

namespace Hamstory.Editor
{
    [CustomEditor(typeof(StoryGraph))]
    internal class StoryGraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开节点图窗口", GUILayout.Height(30)))
                StoryGraphWindow.ShowWindow(target as StoryGraph);
        }
    }
}