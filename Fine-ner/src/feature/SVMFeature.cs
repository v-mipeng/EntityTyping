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
        List<string> feature = new List<string>();
        int offset = 0;

        public SVMFeature():base(){}

        public Pair<object, Dictionary<int,int>> ExtractFeatureWithLable(String[] input)
        {
            return new Pair<object, Dictionary<int, int>>(GetTypeValue(input[1]), null);
        }

        /// <summary>
        /// Reture feature dimension
        /// For sparse expression
        /// </summary>
        public int FeatureDimension
        {
            get
            {
                return offset;
            }
            private set { }
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
        public List<string> ExtractFeature(Instance instance)
        {
            var mention = instance.Mention;
            var context = instance.Context;
            this.feature.Clear();
            var tokenizer = TokenizerPool.GetTokenizer();
            var words = tokenizer.Tokenize(mention).ToArray();
            var tokens = tokenizer.Tokenize(context).ToArray(); 
            TokenizerPool.ReturnTokenizer(tokenizer);
            tokenizer = null;
            // select the first sentence contains mention. This will reduce the parse cost.
            var sspliter = SSpliterPool.GetSSpliter();
            var sentences = sspliter.SplitSequence(tokens);
            SSpliterPool.ReturnSSpliter(sspliter);
            context = GetSentenceCoverMention(sentences, words);
            // get a parser
            var parser = ParserPool.GetParser();
            parser.Parse(context);
            var pairs = parser.GetPosTags();
            var pair = GetIndexOfMention(pairs, words);
            if (pair.first == -1)
            {
                throw new Exception("Cannot find mention by token within context!");
            }
            this.offset = 0;
            #region last word TODO: make last word more accurate
            {
                var lastWord = GetLastToken(mention, context);
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
                int index = parser.GetDriver(pair.first, pair.second);
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
                int index = parser.GetAdjModifier(pair.first, pair.second);
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
                int index = parser.GetAction(pair.first, pair.second);
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
                var dic = new Dictionary<int, int>();
                int value = 0;
                foreach (var w in words) // words surface
                {
                    var word = Generalizer.Generalize(w);
                    var index = offset + DataCenter.GetWordIndex(word);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":"+item.Value);
                }
                offset += DataCenter.GetWordTableSize() + 1;
                dic.Clear();
                foreach (var w in words) // words' cluster id
                {
                    var index = offset + DataCenter.GetWordClusterID(w);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                dic.Clear();
                foreach (var w in words) // words shapes
                {
                    var shape = GetWordShape(w);
                    var index = offset + DataCenter.GetWordShapeIndex(shape);
                    dic.TryGetValue(index, out value);
                    dic[index] =value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetWordShapeTableSize() + 1;
                dic.Clear();
                for (var i = pair.first; i <= pair.second; i++)   // words pos tags
                {
                    var posTag = pairs.ElementAt(i).second;
                    var index = offset + DataCenter.GetPosTagIndex(posTag);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            #endregion

            #region mention length: 1,2,3,4 or longer than 5
            {
                feature.Add((offset+words.Length-1)+":1");
                offset += 5;
            }
            #endregion

            #region mention cluster id   TODO: do entity linking to match mention.
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
                feature.Add((offset + index)+":1");
                offset += DataCenter.GetMentionClusterNumber() + 1;
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
                feature.Add((offset + DataCenter.GetWordIndex(word))+":1");
                offset += DataCenter.GetWordTableSize() + 1;
                // word Cluster id
                var index = DataCenter.GetWordClusterID(originalWord);
                feature.Add((offset + index)+":1");
                offset += DataCenter.GetClusterNumber() + 1;
                // word shape
                var shape = GetWordShape(originalWord);
                feature.Add((offset + DataCenter.GetWordShapeIndex(shape))+":1");
                offset += DataCenter.GetWordShapeTableSize() + 1;
                // word pos tag
                if (posTag != null)
                {
                    feature.Add((offset + DataCenter.GetPosTagIndex(posTag))+":1");
                }
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            else
            {
                offset += DataCenter.GetWordTableSize() + 1;
                offset += DataCenter.GetClusterNumber() + 1;
                offset += DataCenter.GetWordShapeTableSize() + 1;
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
        }

    }
}
