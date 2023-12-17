using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace Hamstory.Editor
{
    [CustomEditor(typeof(SingleStoryExecutor))]
    internal class SingleStoryExecutorEditor : UnityEditor.Editor
    {
        private int hash = 0;
        private Story story;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var textField = serializedObject.FindProperty("storyText");
            var chars = serializedObject.FindProperty("characters");
            if (textField.objectReferenceValue && textField.objectReferenceValue is TextAsset t)
            {
                if (story == null || t.text.GetHashCode() != hash)
                    StoryParser.Parse(t.name, t.text, out story);

                hash = t.text.GetHashCode();
                chars.arraySize = story.Characters.Count;

                Space(20);
                LabelField("角色配置");
                for (int i = 0; i < story.Characters.Count; i++)
                {
                    chars.GetArrayElementAtIndex(i).objectReferenceValue
                        = ObjectField(story.Characters[i], chars.GetArrayElementAtIndex(i).objectReferenceValue, typeof(CharacterConfig), allowSceneObjects: false);
                }
            }
            else
            {
                hash = 0;
                chars.arraySize = 0;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}