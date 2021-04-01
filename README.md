# DictionaryFinder

Simple UI application for searching words from dictionary (https://raw.githubusercontent.com/dwyl/english-words/master/words.txt)

The result word list is formed from all words of dictionary which have search request as *substring* or *subsequence* (need to mark corresponding checkbox)

Resulting word list is shown incrementaly: the first **N** words from it are shown by the start and more will be shown every time you scroll them down

Number **N** depends on the current window height

When search is in progress you will see "Search in progress..." text in the status bar (bottom).

**Note:** Make sure that *dict.txt* file is in the application folder before the start. You can replace words in *dict.txt* with your dictionary
