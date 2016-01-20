using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.sun.org.apache.bcel.@internal.generic;
using pml.type;

namespace msra.nlp.tr
{
    class BayesFeature : Feature
    {

        public class Field
        {
            public const string lastWord = "lastWord";
            public const string nextWord = "nextWord";
            public const string mentionWords = "mentionWords";
            public const string lastWordShape = "lastWordShape";
            public const string nextWordShape = "nextWordShape";
            public const string mentionWordShapes = "mentionWordShapes";
            public const string lastWordTag = "lastWordTag";
            public const string nextWordTag = "nextWordTag";
            public const string mentionWordTags = "mentionWordTags";
            public const string mentionLength = "mentionLength";
            public const string gram2 = "gram2";
            public const string gram3 = "gram3";
        };

        public BayesFeature() : base() { }

       /// <summary>
       ///      Get feature with label information for train and test
       /// </summary>
       /// <param name="input">
       ///      string array with input[0] the mention, input[1] the label and input[2] the context of mention
       /// </param>
       /// <returns>
       ///      A  pair with pair.first the label and pair.second the feature.
       /// </returns>
        internal Pair<string, Dictionary<string,object>> GetFeatureWithLabel(string[] input)
        {
           if (input.Length < 3)
           {
                throw new Exception("Invalid input in GetFeatureWithLabel(string[] input) function with input:\r"+string.Join(",",input));
           }
           return new Pair<string, Dictionary<string, object>>(input[1],GetFeature(input[0],input[2]));
        }

        /*Input:
        * mention TAB context 
        */

        internal Dictionary<string, object> GetFeature(string mention, string context)
        {
            var sspliter = SSpliterPool.GetSSpliter();
            context = GetSentenceWithMention(sspliter.SplitSequence(context), mention);
            SSpliterPool.ReturnSSpliter(sspliter);
            sspliter = null;
            var feature = new Dictionary<string, object>();
            var words = mention.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            /**************Word Level****************/
            // last word shape
            var lastWord = GetLastToken(mention, context);
            if (lastWord == null)
            {
                feature[Field.lastWordShape] = "NULL";
            }
            else
            {
                feature[Field.lastWordShape] = GetWordShape(lastWord);
            }
            // next word shape
            var nextWord = GetNextToken(mention, context);
            if (nextWord == null)
            {
                feature[Field.nextWordShape] = "NULL";
            }
            else
            {
                feature[Field.nextWordShape] = GetWordShape(nextWord);
                //Console.WriteLine(feature[Field.nextWordShape]);
            }
            // mention words shape
            var list = new List<string>();
            list = (from string word in words select GetWordShape(word)).ToList();
            feature[Field.mentionWordShapes] = list;
            // pos tags of mention words
            var pairs = GetPosTags(mention, context);
            var pair = GetMentionRange(pairs, mention);
            list = new List<string>();
            for (var i = pair.first; i <= pair.second; i++)
            {
                if (pairs != null) list.Add(pairs.ElementAt(i).second);
            }
            feature[Field.mentionWordTags] = list;
            // pos tag of last word
            var index = 0;
            if (lastWord != null)
            {
                index = GetLastWordIndex(pairs, lastWord, pair.first);
                feature[Field.lastWordTag] = pairs.ElementAt(index).second;
            }
            else
            {
                feature[Field.lastWordTag] = "NULL";
            }
            // pos tag of next word
            if (nextWord != null)
            {
                index = GetNextWordIndex(pairs, nextWord, pair.second);
                feature[Field.nextWordTag] = pairs.ElementAt(index).second;
            }
            else
            {
                feature[Field.nextWordTag] = "NULL";
            }  
            //stem words
            lastWord = StemWord(lastWord);
            nextWord = StemWord(nextWord);
            words = (from string word in words select StemWord(word)).ToArray();
            // make word lowercase
            lastWord = lastWord.ToLower();
            nextWord = nextWord.ToLower();
            words = (from string word in words select word.ToLower()).ToArray();
            // stemmed last word surface
            feature[Field.lastWord] = (lastWord ?? "NULL");
            // stemmed next word surface
            feature[Field.nextWord] = (nextWord??"NULL");
            // stemmed mention words surface
            feature[Field.mentionWords] = words;
            /**************Mention Level****************/
            // mention length
            feature[Field.mentionLength] = words.Length.ToString();
            // mention words 2-gram
            var gram2 = GetNGram(words, 2);
            feature[Field.gram2] = gram2;
            // mention words 2-gram
            var gram3 = GetNGram(words, 3);
            feature[Field.gram3] = gram3;
            /**************Document Level****************/
            // TODO
            /**************External****************/
            // TODO

            return feature;
        }
       

    }
}
