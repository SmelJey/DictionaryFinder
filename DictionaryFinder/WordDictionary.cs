using System.Collections.Generic;
using System.IO;


namespace DictionaryFinder {
    class WordDictionary {
        public WordDictionary(string filepath)
        {
            myFilePath = filepath;
        }

        public IEnumerable<string> GetMatches(string request, bool isSeqSeach)
        {
            using (var streamReader = new StreamReader(myFilePath)) {
                while (!streamReader.EndOfStream) {
                    var currentWord = streamReader.ReadLine();
                    if (currentWord.Contains(request, isSeqSeach)) {
                        yield return currentWord;
                    }
                }
            }
        }

        private string myFilePath;
    }
}
