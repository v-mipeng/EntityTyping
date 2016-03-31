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
        #region Feature Control

        protected readonly bool useLastWord = true;
        protected readonly bool useNextWord = true;
        protected readonly bool useMentionHead = true;
        protected readonly bool useMentionDriver = true;
        protected readonly bool useMentionAdjModifier = true;
        protected readonly bool useMentionAction = true;
        protected readonly bool useMentionSurfaces = true;
        protected readonly bool useMentionLength = true;
        protected readonly bool useMentionID = true;
        protected readonly bool useStanfordNer = true;
        protected readonly bool useOpennlpNer = true;
        protected readonly bool useDbpediaTypesWithIndegree = true;
        protected readonly bool useDbpediaTypesWithAbstract = true;
        protected readonly bool useKeywords = true;
        protected readonly bool useWordTag = true;
        protected readonly bool useWordID = true;
        protected readonly bool useWordShape = true;
        protected readonly bool useSentenceContext = true;

        #endregion

        protected Feature() 
        {
            useLastWord = Parameter.UseFeature("lastWord");
            useNextWord = Parameter.UseFeature("nextWord");
            useMentionHead = Parameter.UseFeature("mentionHead");
            useMentionDriver = Parameter.UseFeature("mentionDriver");
            useMentionAdjModifier = Parameter.UseFeature("mentionAdjModifier");
            useMentionAction = Parameter.UseFeature("mentionAction");
            useMentionSurfaces = Parameter.UseFeature("mentionSurfaces");
            useMentionLength = Parameter.UseFeature("mentionLength");
            useMentionID = Parameter.UseFeature("mentionID");
            useStanfordNer = Parameter.UseFeature("stanfordNer");
            useOpennlpNer = Parameter.UseFeature("opennlpNer");
            useDbpediaTypesWithIndegree = Parameter.UseFeature("dbpediaTypesWithIndegree");
            useDbpediaTypesWithAbstract = Parameter.UseFeature("dbpediaTypesWithAbstract");
            useKeywords = Parameter.UseFeature("keyWords");
            useWordTag = Parameter.UseFeature("wordTag");
            useWordID = Parameter.UseFeature("wordID");
            useWordShape = Parameter.UseFeature("wordShape");
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
