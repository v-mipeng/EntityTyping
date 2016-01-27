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
    internal class Feature
    {

        protected Feature() { }

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

        public static string GetSentenceCoverMention(IEnumerable<string> sentences, string mention)
        {
            return sentences.FirstOrDefault(sentence => sentence.Contains(mention));
        }

        Regex regex3 = new Regex(@"\s");
        public string GetSentenceCoverMention(IEnumerable<string> sentences, IEnumerable<string> words)
        {
            var mention = new StringBuilder();
            for (var i = 0; i < words.Count(); i++)
            {
                mention.Append(words.ElementAt(i));
            }
            var m = mention.ToString();
            foreach (var sen in sentences)
            {
                var sentence = regex3.Replace(sen, "");
                if (sentence.IndexOf(m) != -1)
                {
                    return sen;
                }
            }
            return null;
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
        static Regex lowerRegex = new Regex(@"[a-z]+");
        static Regex upperRegex = new Regex(@"[A-Z]+");
        static Regex digitalRegex = new Regex(@"\d+");

        public static string GetWordShape(string word)
        {

            if(word == null)
            {
                return null;
            }
            else
            {
                word = lowerRegex.Replace(word, "a");
                word = upperRegex.Replace(word, "A");
                word = digitalRegex.Replace(word, "0");
                return word;
            }
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
            var sspliter = SSpliterPool.GetSSpliter();
            var sentences = sspliter.SplitSequence(context);
            SSpliterPool.ReturnSSpliter(sspliter);
            sspliter = null;
            var sentence = sentences.FirstOrDefault(item => item.Contains(mention));
            if (sentence == null)
            {
                return null;
            }
            var posTagger = PosTaggerPool.GetPosTagger();
            var pairs = posTagger.TagString(sentence);
            PosTaggerPool.ReturnPosTagger(posTagger);
            return pairs;
        }

        protected Pair<int, int> GetIndexOfMention(IEnumerable<Pair<string, string>> ps, string mention)
        {
            var words = mention.Split(' ');
            return GetIndexOfMention(ps, words);
        }

        public Pair<int, int> GetIndexOfMention(IEnumerable<Pair<string, string>> pairs, IEnumerable<string> words)
        {
            var c = new StringBuilder();
            foreach(var p in pairs)
            {
                c.Append(p.first);
            }
            var context = c.ToString();
            var m = new StringBuilder();
            foreach(var w in words)
            {
                m.Append(w);
            }
            var mention = m.ToString();
            var first = context.IndexOf(mention)+1;
            var last = first + mention.Length -1;
            var begin = -1;
            var end = -1;
            var offset = 0;
            for(var i = 0;i<pairs.Count();i++)
            {
                 if(offset <= first && (offset+pairs.ElementAt(i).first.Length) >= first)
                {
                    begin = i;
                }
                offset += pairs.ElementAt(i).first.Length;
                if (offset >= last)
                {
                    end = i;
                    return new Pair<int, int>(begin, end);
                }
            }
            //var pair = new Pair<int, int>();
            //int begin = -1;
            //int end = -1;
            //int offset = 0;
            //for (var i = 0; i < pairs.Count(); i++)
            //{
            //    if(pairs.ElementAt(i).first.Equals(words.ElementAt(offset)) || 
            //        (offset == 0 && pairs.ElementAt(i).first.EndsWith(words.ElementAt(offset))) ||
            //        (offset == words.Count()-1 && pairs.ElementAt(i).first.StartsWith(words.ElementAt(offset))))
            //    {
            //        if (begin == -1)
            //        {
            //            begin = i;
            //        }
            //        if (offset == words.Count() - 1)
            //        {
            //            end = i;
            //            return new Pair<int, int>(begin, end);
            //        }
            //        offset++;
            //    }
            //    else
            //    {
            //        i = i - offset;
            //        offset = 0;
            //        begin = -1;
            //    }
            //}
            return new Pair<int, int>(-1, -1);
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
            var index = -1;
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

        static string[] types = { 
                                    "people.person", 
                                    "location.location", 
                                    "organization.organization" ,
                                    "award.award",
                                    "body.part",
                                    "book.written_work",
                                    "broadcast.content",
                                    "chemicstry.chemistry",
                                    "commerce.consumer_product",
                                    "commerce.electronics_product",
                                    "computer.software",
                                    "food.food",
                                    "language.language",
                                    "music.music",
                                    "time.event"
                                };

        static protected int GetTypeValue(string type)
        {
          for(int i = 0;i<types.Length;i++)
          {
              if(type.Equals(types[i]))
              {
                  return i;
              }
          }
          return -1;
        }
    }
}
