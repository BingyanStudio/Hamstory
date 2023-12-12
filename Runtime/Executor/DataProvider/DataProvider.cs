using UnityEngine;

namespace Hamstory
{
    public abstract class DataProvider : ScriptableObject
    {
        public abstract bool Predicate(StoryExecutorBase executor, string expression);
        public abstract string Serialize(StoryExecutorBase executor, string content);
    }
}