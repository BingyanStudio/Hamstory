using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Hamstory.Editor
{
    [ScriptedImporter(0, "hamstory")]
    internal class StoryTextImporter : ScriptedImporter
    {
        private void Awake()
        {
            string[] exts = new[] { "hamstory" };
            EditorSettings.projectGenerationUserExtensions =
                EditorSettings.projectGenerationUserExtensions.Concat(exts).Distinct().ToArray();
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset text = new(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("hamstory", text);
            ctx.SetMainObject(text);
        }
    }
}