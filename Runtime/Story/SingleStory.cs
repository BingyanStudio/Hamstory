using Bingyan;
using UnityEngine;

namespace Hamstory
{
    [CreateAssetMenu(fileName = "SingleStory", menuName = "Hamstory/SingleStory", order = 0)]
    public class SingleStory : ScriptableObject
    {
        [SerializeField, Title("故事脚本")] private TextAsset story;
        [SerializeField, HideInInspector] private CharacterConfig[] characters;

        public TextAsset Story => story;
        public CharacterConfig[] Characters => characters;
    }
}