using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Hamstory.Editor
{
    internal class StoryDragManipulator : PointerManipulator
    {
        private StoryGraphView view;

        public StoryDragManipulator(StoryGraphView view)
        {
            this.view = view;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void OnDragUpdate(DragUpdatedEvent e)
        {
            if (DragAndDrop.paths.Length == 0 || DragAndDrop.paths.Length > 1)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            var obj = DragAndDrop.objectReferences[0];
            DragAndDrop.visualMode = obj is TextAsset || obj is StoryGraph ?
                DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;
        }

        private void OnDragPerform(DragPerformEvent e)
        {
            var obj = DragAndDrop.objectReferences[0];
            var pos = view.GetMousePosition(e.localMousePosition);
            if (obj is TextAsset text)
                view.viewModel.CreateStoryNode(pos, text);
            else if (obj is StoryGraph graph)
                view.viewModel.CreateSubGraphNode(pos, graph);
        }
    }
}