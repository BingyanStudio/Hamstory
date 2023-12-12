using Bingyan;
using UnityEngine;

namespace Hamstory
{
    [CreateAssetMenu(menuName = "Hamstory/Character/Simple", fileName = "SimpleCharacter")]
    public class CharacterConfig : ScriptableObject
    {
        [SerializeField, Title("名称")] private string charName;

        public string CharName => charName;
    }
}