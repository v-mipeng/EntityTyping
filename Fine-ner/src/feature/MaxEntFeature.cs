using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;

namespace msra.nlp.tr
{
    class MaxEntFeature : Feature
    {
        private List<string> feature = new List<string>();

        public static string[] fields = new string[]{
            // last word
            "lastWord",
            "lastWordTag",
            "lastWordID",
            "lastWordShape",
            // next word
            "nextWord",
            "nextWordTag",
            "nextWordID",
            "nextWordShape",
            // mention head
            "mentionHead",
            "mentionHeadTag",
            "mentionHeadID",
            "mentionHeadShape",
            // mention driver
            "mentionDriver",
            "mentionDriverTag",
            "mentionDriverID",
            "mentionDriverShape",
            // mention adjective modifier
            "mentionAdjModifier",
            "mentionAdjModifierTag",
            "mentionAdjModifierID",
            "mentionAdjModifierShape",
            // mention action
            "mentionAction",
            "mentionActionTag",
            "mentionActionID",
            "mentionActionShape",
            // context document
            "documentID",
            // if name list contains
            
            // mention level
            "mentionID",
            "mentionLength",
        };

        public MaxEntFeature() : base() { }

        /// <summary>
        ///      Get feature with label information for train and test
        /// </summary>
        /// <param name="input">
        ///      string array with input[0] the mention, input[1] the label and input[2] the context of mention
        /// </param>
        /// <returns>
        ///      A  pair with pair.first the label and pair.second the feature.
        /// </returns>
        internal Pair<string, List<string>> GetFeatureWithLabel(string[] input)
        {
            if (input.Length < 3)
            {
                throw new Exception("Invalid input in GetFeatureWithLabel(string[] input) function with input:\r" +
                                    string.Join(",", input));
            }
            return new Pair<string, List<string>>(input[1], GetFeature(input[0], input[2]));
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
        internal List<string> GetFeature(string mention, string context)
        {
            int wordTableSize = DataCenter.GetWordTableSize();
            feature.Clear();
            var tokenizer = TokenizerPool.GetTokenizer();
            String[] words = tokenizer.Tokenize(mention).ToArray();
            TokenizerPool.ReturnTokenizer(tokenizer);
            // pos tags of mention words
            var pairs = GetPosTags(mention, context);
            var pair = GetIndexOfMention(pairs, mention);
            if (pair.first == -1)
            {
                return null;
            }
            var contextTokens = new List<string>();
            foreach (var p in pairs)
            {
                contextTokens.Add(p.first);
            }
            // get a parser
            var parser = ParserPool.GetParser();
            parser.Parse(contextTokens);
          
            #region last word
            {
                var word = GetLastToken(mention, context);
                var index = 0;
                index = GetLastWordIndex(pairs, word, pair.first);
                if (index != -1)
                {
                    var posTag = pairs.ElementAt(index).second;
                    AddFieldToFeture(word, posTag);
                }
                else
                {
                    AddFieldToFeture(word, null);

                }
            }
            #endregion

            #region next word
            {
                var word = GetNextToken(mention, context);
                var index = 0;
                index = GetNextWordIndex(pairs, word, pair.second)-1;
                if (index > 0)
                {
                    var posTag = pairs.ElementAt(index).second;
                    AddFieldToFeture(word, posTag);
                }
                else
                {
                    AddFieldToFeture(word, null);
                }

            }
            #endregion

            #region mention head
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
                AddFieldToFeture(head, posTag);
            }
            #endregion

            #region mention driver
            {
                int index = parser.GetDriver(pair.first, pair.second)-1;
                if (index > 0)
                {
                    var driver = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddFieldToFeture(driver, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            #region mention adjective modifer
            {
                int index = parser.GetAdjModifier(pair.first, pair.second)-1;
                if (index > 0)
                {
                    var adjModifier = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddFieldToFeture(adjModifier, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            #region mention action
            {
                int index = parser.GetAction(pair.first, pair.second) -1;
                if (index > 0)
                {
                    var action = pairs.ElementAt(index).first;
                    var posTag = pairs.ElementAt(index).second;
                    AddFieldToFeture(action, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            ParserPool.ReturnParser(parser);
            parser = null;

            #region mention ID
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
                feature.Add(DataCenter.GetMentionClusterID(m.ToString()).ToString());
            }
            #endregion

            #region mention length
            {
                feature.Add(words.Length.ToString());
            }
            #endregion

            #region TDDO: topic
            {
                // TODO
            }
            #endregion

            #region TDDO: dictionary
            {
                // dictionary
                // TODO
            }
            #endregion
            return feature;
        }

        private void AddFieldToFeture(string word, string posTag)
        {
            if (word != null)
            {
                string generalsurface, ID, shape;
                // mention head
                generalsurface = Generalizer.Generalize(word);
                // Cluster id of last word
                ID = DataCenter.GetWordClusterID(word).ToString();
                // next word shape
                shape = GetWordShape(word);
                AddToFeature(generalsurface, posTag ?? "NULL", ID, shape);
            }
            else
            {
                AddToFeature("NULL", "NULL", DataCenter.GetClusterNumber().ToString(), "NULL");
            }
        }

        private void AddToFeature(params string[] objs)
        {
            foreach (var par in objs)
            {
                this.feature.Add(par);
            }
        }
    }
}
