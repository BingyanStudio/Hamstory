using System.Collections.Generic;
using UnityEngine;

namespace Hamstory
{
    public abstract class VisualProvider : MonoBehaviour
    {
        public abstract GameObject GetDialogPanel();
        public abstract void SetCharacter(CharacterConfig config, string extraArgs);
        public abstract void ClearCharacter();
        public abstract void SetText(StoryExecutorBase executor, string content);
        public abstract void CreateMenu(StoryExecutorBase executor, List<MenuOption> options);
        public abstract void ClearMenu();
    }
}