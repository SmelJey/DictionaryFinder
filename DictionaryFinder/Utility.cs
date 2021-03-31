using System.Windows;
using System.Windows.Media;

namespace DictionaryFinder {
    public static class Utility {
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static bool Contains(this string text, string sequence, bool isSeqSearch)
        {
            if (!isSeqSearch)
            {
                return text.Contains(sequence);
            }

            int seqIdx = 0;
            foreach (char c in text)
            {
                if (c == sequence[seqIdx])
                {
                    seqIdx++;
                    if (seqIdx == sequence.Length)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
