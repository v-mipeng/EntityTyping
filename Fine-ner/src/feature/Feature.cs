using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using com.sun.org.apache.bcel.@internal.generic;
using com.sun.tools.@internal.xjc.reader.gbind;
using pml.type;

namespace msra.nlp.tr
{
    internal abstract class Feature
    {

        /// <summary>
        ///     Stem word
        /// </summary> 
        /// <remarks>
        ///     Input: string type original word
        /// </remarks>
        /// <returns>
        ///     stemmed word or null if word is null
        /// </returns>
        ///
        protected string StemWord(string word)
        {
            return word == null ? null : DataCenter.GetStemmedWord(word);
        }

        /*Get last token of the mention
         */
        static  Regex regex = new Regex(@",[^,]*,");
        string[] seperator = new string[] { " ", "\t" };
        public static string GetLastToken(string mention, string context)
        {
            lock (regex)
            {
                var head = context.Substring(0, context.IndexOf(mention));
                head = regex.Replace(head, " ").TrimEnd();
                string lastToken = null;
                for (var i = head.Length - 1; i >= 0; i--)
                {
                    if (head[i] == ' ' || head[i] == '\t')
                    {
                        lastToken = head.Substring(i + 1);
                        break;
                    }
                }
                return lastToken;
            }
        }

        public static string GetSentenceWithMention(IEnumerable<string> sentences, string mention)
        {
            return sentences.FirstOrDefault(sentence => sentence.Contains(mention));
        }

        /* Get next token of the mention
         */
        public static string GetNextToken(string mention, string context)
        {
            context = context.Trim();
            mention = mention.Trim();
            string tail = context.Substring(context.LastIndexOf(mention) + mention.Length);
            tail = tail.TrimStart();
            if (tail.StartsWith(",") || tail.Length == 0)
            {
                return null;
            }
            else
            {
                int index;
                return (index = tail.IndexOf(' ')) == -1 ? tail : tail.Substring(0, index);
            }
        }

        /* Map 
                    uppercase letter to "A"                      
                    lowercase letter to "a"
                    number to "0"
                    non-alpha letter to "-" 
           Compress the expresion           
           Example:
                    John's mother is 30-year-old   -->  Aa-a    a   a   0-a-a                       
        */
        public static string GetWordShape(string word)
        {
            var array = word.ToCharArray();
            for (var i = 0; i<array.Length;i++)
            {
                array[i] = MapChar(array[i]);
            }
            return CompressArray(array);
        }

        protected char MapChar(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return '0';
            }
            if (c >= 'A' && c <= 'Z')
            {
                return  'A';
            }
            else if (c >= 'a' && c <= 'z')
            {
                return  'a';
            }
            else
            {
                return '-';
            }
        }

        public static string CompressArray(IEnumerable array)
        {
            if (array == null) return null;
            var builder = new StringBuilder();
            var obj = (char)0;

            foreach (var item in array.Cast<char>().Where(item => obj == (char)0 || !item.Equals(obj)))
            {
                builder.Append(item);
                obj = item;
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get pos tag informaiton of mention and its context. The context is limited to a sentence contains this mention.
        /// </summary>
        /// <param name="context">The context of mention</param>
        /// <param name="mention">Mention</param>
        /// <returns>
        /// A pair with pair.first storing the pos tag information of mention's limited context(a sentence) 
        /// The tag informaiton is stored as a list of pairs with pair.first the string and pair.second the corresponding pos tag
        /// of the string.
        ///  And pair.second of the return is the index of mention counted by pairs number in pair.first.
        /// </returns>
        /// <example>
        /// context:I like Beijing  mention:Beijing
        /// ((I, NP) (like, VP) (Beijing, Np), 2)
        /// </example>
        protected IEnumerable<Pair<string, string>> GetPosTags(string mention, string context)
        {
            if (context == null)
            {
                return null;
            }
            context = context.Trim();
            if (mention == null)
            {
                return null;
            }
            mention = mention.Trim();
            var sentences = SentenceSplit.SplitSequence(context);
            var sentence = sentences.FirstOrDefault(item => item.Contains(mention));
            if (sentence == null)
            {
                return null;
            }
            var pairs = PosTagger.TagString(sentence);
            return pairs;
        }

        protected Pair<int, int> GetMentionRange(IEnumerable<Pair<string, string>> ps, string mention)
        {
            var pairs = ps.ToList();
            mention = Regex.Replace(mention, @"\s", "");
            var builder = new StringBuilder();
            for (var i = 0; i < pairs.Count(); i++)
            {
                builder.Append(pairs[i].first);
            }
            var sequence = builder.ToString();
            var start = 0;
            start = sequence.IndexOf(mention, StringComparison.Ordinal);
            var des = start + mention.Length;
            var offset = 0;
            var begin = -1;
            var end = -1;
            for (var i = 0; i < pairs.Count; i++)
            {
                if (begin == -1 && offset == start)
                {
                    begin = i;
                }
                offset += pairs[i].first.Length;
                if (begin != -1 && des == offset)
                {
                    end = i;
                    break;
                }
            }
            return new Pair<int, int>(begin, end);
        }

        protected int GetLastWordIndex(IEnumerable<Pair<string, string>> ps, string lastWord, int end)
        {
            var pairs = ps.ToList();
            if (end > pairs.Count())
            {
                throw new Exception("End index > pair.Cound()");
            }
            var builder = new StringBuilder();
            for (var i = 0; i < end; i++)
            {
                builder.Append(pairs[i].first);
            }
            var start = builder.ToString().LastIndexOf(lastWord, StringComparison.Ordinal);
            var offset = 0;
            var index = 0;
            for (var i = 0; i <end; i++)
            {
                if (offset == start)
                {
                    index = i;
                    break;
                }
                offset += pairs[i].first.Length;
            }
            return index;
        }

        protected int GetNextWordIndex(IEnumerable<Pair<string, string>> ps, string nextWord, int begin)
        {
            var pairs = ps.ToList();
            if (begin > pairs.Count())
            {
                throw new Exception("End index > pair.Cound()");
            }
            var builder = new StringBuilder();
            for (var i = begin+1; i < pairs.Count(); i++)
            {
                builder.Append(pairs[i].first);
            }
            var start = builder.ToString().IndexOf(nextWord, StringComparison.Ordinal);
            var offset = 0;
            var index = 0;
            for (var i = begin+1; i < pairs.Count(); i++)
            {
                if (offset == start)
                {
                    index = i;
                    break;
                }
                offset += pairs[i].first.Length;
            }
            return index;
        }


        protected IEnumerable<string> GetNGram(IEnumerable<object> ws, int n)
        {
            var words = ws.ToList();
                for (var i = words.Count(); i < n; i++)
                {
                    words.Add("null");
                }
            var nGrams = new List<string>();
            var buffer = new StringBuilder();
            for (var i = 0; i <= words.Count() - n; i++)
            {
                buffer.Clear();
                buffer.Append(words[i]);
                for (var j = i+1; j < i+n; j++)
                {
                    buffer.Append(" "+words[j]);
                }
                    nGrams.Add(buffer.ToString());
            }
                return nGrams;
        }

        /************************************************************************/
        /* Sort the dictionary's keys by their mapped int value                                                                     */
        /************************************************************************/
        public static List<string> SortKeysByNum(Dictionary<string, int> dic)
        {
            var keys = dic.Keys.ToList();
            Comparer<string> comparer = new DiComparer(dic);
            keys.Sort(comparer);
            return keys;
        }

        private class DiComparer : Comparer<string>
        {
            Dictionary<string, int> dic;
            public DiComparer(Dictionary<string, int> dic)
            {
                this.dic = dic;
            }
            public override int Compare(string key1, string key2)
            {
                return -((IComparable<int>)dic[key1]).CompareTo(dic[key2]);
            }
        }
    }
}
