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
        /// Get the first sentence contains mention.
        /// </summary>
        /// <param name="sentences"></param>
        /// <param name="words"></param>
        /// <returns></returns>
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
