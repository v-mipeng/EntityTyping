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
        Dictionary<int, int> feature = new Dictionary<int, int>();
        int offset = 0;

        public SVMFeature():base(){}

        public Pair<object, Dictionary<int,int>> ExtractFeatureWithLable(String[] input)
        {
            return new Pair<object, Dictionary<int, int>>(GetTypeValue(input[1]), ExtractFeature(new string[]{input[0], input[2]}));
        }

        /// <summary>
        /// Reture feature dimension
        /// </summary>
        public int FeatureDimension
        {
            get
            {
                return offset;
            }
            private set{}
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
         *      Parent in dependency tree(stanford corenlp) : driver, action, adject modifier(TO USE)  
         *      Dictionary                      :TODO
         *      Topic(Define topic)             :TODO: I am going to work with document cluster
         * 
         */
        private Dictionary<int,int> ExtractFeature(String[] input)
        {
            var mention = input[0];
            var context = input[1];
            this.feature = new Dictionary<int, int>();
            var tokenizer = TokenizerPool.GetTokenizer();
            String[] words = tokenizer.Tokenize(mention).ToArray();
            TokenizerPool.ReturnTokenizer(tokenizer);
            tokenizer = null;
            // pos tags of mention words
            var pairs = GetPosTags(mention, context);
            var pair = GetMentionRange(pairs, mention);
            if (pair.first == -1)
            {
                return null;
            }
            var contextTokens = new List<string>();
            foreach(var p in pairs)
            {
                contextTokens.Add(p.first);
            }
            // get a parser
            var parser = ParserPool.GetParser();
            parser.Parse(contextTokens);
            this.offset = 0;
            #region last word
            {
                var lastWord = GetLastToken(mention, context);  // TODO: make last word more accurate
                if (lastWord != null)
                {
                    var index = -1;
                    index = GetLastWordIndex(pairs, lastWord, pair.first);
                   
                    if (index != -1)
                    {
                        var posTag = pairs.ElementAt(index).second;
                        AddWordFieldToFeature(lastWord, posTag);
                    }
                    else
                    {
                        AddWordFieldToFeature(lastWord, null);
                    }
                }

            }
            #endregion

            #region next word
            {
                var nextWord = GetNextToken(mention, context);
                if (nextWord != null)
                {
                    var index = -1;
                    index = GetNextWordIndex(pairs, nextWord, pair.first);

                    if (index != -1)
                    {
                        var posTag = pairs.ElementAt(index).second;
                        AddWordFieldToFeature(nextWord, posTag);
                    }
                    else
                    {
                        AddWordFieldToFeature(nextWord, null);
                    }
                }
            }
            #endregion

            #region  mention head
            {
                string head = null, posTag = null;
                for (int i = pair.first; i <= pair.second; i++)
                {
                    if (pairs.ElementAt(i).second.StartsWith("N"))
                    {
                        // last noun
                        head = pairs.ElementAt(i).first;
                        posTag = pairs.ElementAt(i).second;
                    }
                    else if (pairs.ElementAt(i).second.Equals("IN") || pairs.ElementAt(i).second.Equals(","))
                    {
                        // before IN
                        break;
                    }
                }
                if (head == null)
                {
                    head = words[words.Length - 1];
                    posTag = pairs.ElementAt(pair.second).second;
                }
                AddWordFieldToFeature(head, posTag);
            }
            #endregion

            #region mention driver
            {
                int index = parser.GetDriver(pair.first, pair.second)-1;
                if (index >= 0)
                {
                    var driver = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddWordFieldToFeature(driver, posTag);
                }
                else
                {
                    AddWordFieldToFeature(null, null);
                }
            }
            #endregion

            #region mention adjective modifer
            {
                int index = parser.GetAdjModifier(pair.first, pair.second)-1;
                if (index >= 0)
                {
                    var adjModifier = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddWordFieldToFeature(adjModifier, posTag);
                }
                else
                {
                    AddWordFieldToFeature(null, null);
                }

            }
            #endregion

            #region mention action
            {
                int index = parser.GetAction(pair.first, pair.second)-1;
                if (index >= 0)
                {
                    var action = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddWordFieldToFeature(action, posTag);
                }
                else
                {
                    AddWordFieldToFeature(null, null);
                }
            }
            #endregion

            ParserPool.ReturnParser(parser);
            parser = null;


            #region mention words
            {
                foreach (var w in words) // words surface
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
                for (var i = pair.first; i <= pair.second; i++)   // words pos tags
                {
                    var posTag = pairs.ElementAt(i).second;
                    var index = offset + DataCenter.GetPosTagIndex(posTag);
                    int value;
                    feature.TryGetValue(index, out value);
                    feature[index] = value + 1;
                }
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            #endregion

            #region mention length
            {
                feature[offset] = words.Length;
                offset++;
            }
            #endregion

            #region mention cluster id
            {
                StringBuilder m = new StringBuilder();
                foreach (var w in words)
                {
                    if (m.Length == 0)
                    {
                        m.Append(w.ToLower());
                    }
                    else
                    {
                        m.Append("_" + w.ToLower());
                    }
                }
                var index = DataCenter.GetMentionClusterID(m.ToString());
                feature[offset + index] = 1;
                offset += DataCenter.GetMentionClusterNumber()+1;
            }
            #endregion

            #region TODO: topic
            {

            }
            #endregion

            #region TODO: dictionary
            {

            }
            #endregion

            return feature;
        }

        private void AddWordFieldToFeature(string originalWord, string posTag)
        {
            if (originalWord != null)
            {
                // word surface
                var word = Generalizer.Generalize(originalWord);
                feature[offset + DataCenter.GetWordIndex(word)] = 1;
                offset += DataCenter.GetWordTableSize() + 1;
                // word Cluster id
                var index = DataCenter.GetWordClusterID(originalWord);
                feature[offset+index] = 1;
                offset += DataCenter.GetClusterNumber()+1;
                // word shape
                var shape = GetWordShape(originalWord);
                feature[offset + DataCenter.GetWordShapeIndex(shape)] = 1;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                // word pos tag
                if (posTag != null)
                {
                    feature[offset + DataCenter.GetPosTagIndex(posTag)] = 1;
                }
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            else
            {
                offset += DataCenter.GetWordTableSize() + 1;
                offset += DataCenter.GetClusterNumber()+1;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
        }


    }
}
