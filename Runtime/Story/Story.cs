using System.Collections.Generic;

namespace Hamstory
{
    public class Story
    {
        private List<Sentence> sentences;
        private List<string> characters;
        private List<string> jumps;

        public int Length => sentences.Count;
        public List<string> Characters => characters;
        public List<string> Jumps => jumps;

        public Story(List<Sentence> sentences, List<string> characters, List<string> jumps)
        {
            this.sentences = sentences;
            this.characters = characters;
            this.jumps = jumps;
        }

        public Sentence GetSentence(int index) => sentences[index];
    }
}