using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictionaryFinder {
    class WordDictionary {
        public WordDictionary(string filepath)
        {
            using (var streamReader = new StreamReader(filepath))
            {
                while (!streamReader.EndOfStream)
                {
                    words.Add(streamReader.ReadLine());
                }
            }
        }

        public IEnumerable<string> GetMatches(string request)
        {
            foreach (var word in words)
            {
                if (word.Contains(request))
                {
                    yield return word;
                }
            }
        }

        private List<string> words = new List<string>();
    }
}
