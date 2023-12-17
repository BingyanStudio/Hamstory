using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    public class StoryGraphWindow : EditorWindow
    {
        private static Dictionary<StoryGraph, StoryGraphWindow> windows = new();

        internal static void ShowWindow(StoryGraph source)
        {
            if (windows.ContainsKey(source)) windows[source].Focus();
            else
            {
                StoryGraphWindow wnd = CreateWindow<StoryGraphWindow>(source.name);
                wnd.Init(source);
                windows.Add(source, wnd);
            }
        }

        private StoryGraph graph;
        private StoryGraphView graphView;
        private StoryGraphViewModel graphViewModel;


        [SerializeField]
        private StyleSheet m_StyleSheet = default;

        private void OnInspectorUpdate()
        {
            if (!graph)
                Close();
        }

        private void OnDestroy()
        {
            if (graph) windows.Remove(graph);
            SaveChanges();
        }

        public void CreateGUI()
        {
            BuildLayout();

            if (graph && !windows.ContainsKey(graph))
            {
                windows.Add(graph, this);
                Init(graph);
            }

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void BuildLayout()
        {
            var root = rootVisualElement;

            graphView = new(this);
            graphView.styleSheets.Add(m_StyleSheet);
            graphView.StretchToParentSize();
            root.Add(graphView);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
                SaveChanges();
        }

        private void Init(StoryGraph graph)
        {
            this.graph = graph;

            graphViewModel = new(graph, this);
            graphView.Init(graphViewModel);
        }

        public override void SaveChanges()
        {
            if (!graph) return;

            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssetIfDirty(graph);
            EditorUtility.ClearDirty(graph);
        }
    }
}