using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using pml.type;

namespace msra.nlp.tr
{
    class SVMFeature  : Feature
    {
        public SVMFeature():base(){}

        public Pair<string, Dictionary<int,int>> ExtractFeatureWithLable(String[] input)
        {
            return new Pair<string, Dictionary<int, int>>(input[1], ExtractFeature(new string[]{input[0], input[2]}));
        }

        /*   Extract feature from the input, and the feature is clustered by field
         *   The input should contains two items:
         *      Mention surface:   the surface text of the mention             // input[0]
         *      Mention context:   the context contains the mention         // input[1]
         *   The output are a list of pairs store the features' index and value:                                   
         *      Mention surface  
         *      Mention Shape
         *      Cluster ID of mention words     
         *      Mention length         
         *      Mention ID                      
         *      Last token
         *      Last token pos tag
         *      Last token ID                   
         *      Next token
         *      Next token pos tag
         *      Next token ID                   
         *      Parent in dependency tree(stanford corenlp)   
         *      Dictionary                      :TODO
         *      Topic(Define topic)             :TODO: I am going to work with document cluster
         * 
         */
        public Dictionary<int,int> ExtractFeature(String[] input)
        {
            var mention = input[0];
            var context = input[1];
            int wordTableSize = DataCenter.GetWordTableSize();
            var feature = new Dictionary<int,int>();
            String[] words = Tokenizer.Tokenize(mention).ToArray();
            // pos tags of mention words
            var pairs = GetPosTags(mention, context);
            var pair = GetMentionRange(pairs, mention);
            
            int offset = 0;
            /**************Word Level****************/
            // last word
            var lastWord = GetLastToken(mention, context);
            if (lastWord != null)
            {
                // last word surface
                var word = Generalizer.Generalize(lastWord);
                feature[offset + DataCenter.GetWordIndex(word)] = 1;
                offset += DataCenter.GetWordTableSize() + 1;
                // Cluster id of last word
                feature[offset] = DataCenter.GetWordClusterID(lastWord);
                offset++;
                // last word shape
                var shape = GetWordShape(lastWord);
                feature[offset + DataCenter.GetWordShapeIndex(shape)] = 1;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                // last word pos tag
                var index = 0;
                index = GetLastWordIndex(pairs, lastWord, pair.first);
                var posTag = pairs.ElementAt(index).second;
                feature[offset + DataCenter.GetPosTagIndex(posTag)] = 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            else
            {
                offset += DataCenter.GetWordTableSize() + 1;
                offset++;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            // next word
            var nextWord = GetNextToken(mention, context);
            if (nextWord != null)
            {
                // next word surface
                var word = Generalizer.Generalize(nextWord);
                feature[offset + DataCenter.GetWordIndex(word)] = 1;
                offset += DataCenter.GetWordTableSize() + 1;
                // Cluster id of last word
                feature[offset] = DataCenter.GetWordClusterID(nextWord);
                offset++;
                // next word shape
                var shape = GetWordShape(nextWord);
                feature[offset + DataCenter.GetWordShapeIndex(shape)] = 1;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                // next word pos tag
                var index = 0;
                index = GetLastWordIndex(pairs, nextWord, pair.second);
                var posTag = pairs.ElementAt(index).second;
                feature[offset + DataCenter.GetPosTagIndex(posTag)] = 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            else
            {
                offset += DataCenter.GetWordTableSize() + 1;
                offset++;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            // mention words
            StringBuilder m = new StringBuilder();
            foreach(var w in words)
            {
                m.Append(w.ToLower());
            }
            feature[offset] = DataCenter.GetMentionClusterID(m.ToString());
            offset++;
            foreach(var w in words) // words surface
            {
                var word = Generalizer.Generalize(w);
                var index = offset + DataCenter.GetWordIndex(word);
                int value;
                feature.TryGetValue(index, out value);
                feature[index] = value + 1;
            }
            offset += DataCenter.GetWordTableSize() + 1;
            foreach (var w in words) // words' cluster id
            {

                var index = offset + DataCenter.GetWordClusterID(w);
                feature[index] = 1;
            }
            offset += DataCenter.GetClusterNumber() + 1;
            foreach (var w in words) // words shapes
            {
                var shape = GetWordShape(w);
                var index = offset + DataCenter.GetWordShapeIndex(shape);
                int value;
                feature.TryGetValue(index, out value);
                feature[index] = value + 1;
            }
            offset += DataCenter.GetWordShapeTableSize() + 1;
            for(var i=pair.first; i<=pair.second;i++)
            {
                var posTag = pairs.ElementAt(i).second;
                var index = offset + DataCenter.GetPosTagIndex(posTag);
                int value;
                feature.TryGetValue(index, out value);
                feature[index] = value + 1;
            }
            offset += DataCenter.GetPosTagTableSize() + 1;
            /**************Mention Level****************/
            // mention length
            feature[offset] = words.Length;
            offset++;
            // mention cluster id

            /**************Document Level****************/
            // topic

            /**************External Level****************/
            // dictionary
           return feature;
        }




    }
}
