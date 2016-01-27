using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;

namespace msra.nlp.tr
{
    class IndividualFeature : Feature
    {
        private List<string> feature = new List<string>();
        int offset = 0;


        public static string[] fields = new string[]{
            // last word
            "lastWord",
            "lastStemWord",
            "lastWordTag",
            "lastWordID",
            "lastWordShape",
            // next word
            "nextWord",
            "nextStemWord",
            "nextWordTag",
            "nextWordID",
            "nextWordShape",
            // mention head
            "mentionHead",
            "mentionStemHead",
            "mentionHeadTag",
            "mentionHeadID",
            "mentionHeadShape",
            // mention driver
            "mentionDriver",
            "mentionStemDriver",
            "mentionDriverTag",
            "mentionDriverID",
            "mentionDriverShape",
            // mention adjective modifier
            "mentionAdjModifier",
            "mentionStemAdjModifier",
            "mentionAdjModifierTag",
            "mentionAdjModifierID",
            "mentionAdjModifierShape",
            // mention action
            "mentionAction",
            "mentionStemAction",
            "mentionActionTag",
            "mentionActionID",
            "mentionActionShape",
            // mention words
            "mentionSurfaces",
            "mentionStemSurfaces",
            "mentionTags",
            "mentionIDs",
            "mentionShapes",
            // context document
            //"documentID",
            // if name list contains
            
            // mention level
            "mentionID",
            "mentionLength",
        };

        public IndividualFeature() : base() { }

        /// <summary>
        ///      Get feature with label information for train and test
        /// </summary>
        /// <param name="input">
        ///      string array with input[0] the mention, input[1] the label and input[2] the context of mention
        /// </param>
        /// <returns>
        ///      A  pair with pair.first the label and pair.second the feature.
        /// </returns>
        internal Pair<string, List<string>> ExtractFeatureWithLabel(string[] input)
        {
            if (input.Length < 3)
            {
                throw new Exception("Invalid input in GetFeatureWithLabel(string[] input) function with input:\r" +
                                    string.Join(",", input));
            }
            return null;
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
        public List<string> ExtractFeature(Instance instance)
        {
            var mention = instance.Mention;
            var context = instance.Context;
            this.feature.Clear();
            string[] words = null;
            string[] tokens = null;
            var tokenizer = TokenizerPool.GetTokenizer();
            try
            {
                words = tokenizer.Tokenize(mention).ToArray();
                tokens = tokenizer.Tokenize(context).ToArray();
                TokenizerPool.ReturnTokenizer(tokenizer);
                tokenizer = null;
            }
            catch(Exception e)
            {
                TokenizerPool.ReturnTokenizer(tokenizer);
                throw e;
            }
            // select the first sentence contains mention. This will reduce the parse cost.
            List<string> sentences = null;
            var sspliter = SSpliterPool.GetSSpliter();
            try
            {
                sentences = sspliter.SplitSequence(tokens);
                SSpliterPool.ReturnSSpliter(sspliter);
            }
            catch(Exception e)
            {
                SSpliterPool.ReturnSSpliter(sspliter);
                Console.Clear();
                Console.WriteLine("Error in sentence spliter.");
                throw e;
            }
            context = GetSentenceCoverMention(sentences, words);
            if(context == null)
            {
                throw new Exception("Cannot find mention by token within context!");
            }
            // get a parser
            DependencyParser parser = null;
            try
            {
                parser = ParserPool.GetParser();
            }
            catch(Exception )
            {
                throw new Exception("Cannot get a parser!");
            }
            List<Pair<string, string>> pairs = null;
            Pair<int, int> pair = null;
            try                  
            {
                parser.Parse(context);

                pairs = parser.GetPosTags();
                pair = GetIndexOfMention(pairs, words);
                if (pair.first == -1)
                {
                    throw new Exception("Cannot find mention by token within context!");
                }
                this.offset = 0;

                #region last word
                {
                    var index = pair.first - 1;
                    if (index > 0)
                    {
                        var word = pairs.ElementAt(index).first;
                        var posTag = pairs.ElementAt(index).second;
                        AddFieldToFeture(word, posTag);
                    }
                    else
                    {
                        AddFieldToFeture(null, null);

                    }
                }
                #endregion

                #region next word
                {
                    var index = pair.second + 1;
                    if (index < pairs.Count)
                    {
                        var word = pairs.ElementAt(index).first;
                        var posTag = pairs.ElementAt(index).second;
                        AddFieldToFeture(word, posTag);
                    }
                    else
                    {
                        AddFieldToFeture(null, null);
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
                    int index = parser.GetDriver(pair.first, pair.second);
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
                    int index = parser.GetAdjModifier(pair.first, pair.second);
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
                    int index = parser.GetAction(pair.first, pair.second);
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
            }
            catch(Exception e)
            {
                if (parser != null)
                {
                    ParserPool.ReturnParser(parser);
                    parser = null;
                }
                throw e;
            }


            #region Mention Words
            {
                // mention surfaces
                var mentionWords = new StringBuilder();
                foreach(var word in words)
                {
                    if(mentionWords.Length == 0)
                    {
                        mentionWords.Append(Generalizer.Generalize(word));
                    }
                    else
                    {
                        mentionWords.Append("," + Generalizer.Generalize(word));
                    }
                }
                // add mention surface
                feature.Add(string.Join(",", words));
                // add stemmed mention surface
                feature.Add(mentionWords.ToString());
                // mention tags
                var mentionTags = mentionWords.Clear();     
                for(var i = pair.first; i<= pair.second;i++)
                {
                    if (mentionTags.Length == 0)
                    {
                        mentionTags.Append(pairs.ElementAt(i).second);
                    }
                    else
                    {
                        mentionTags.Append("," + pairs.ElementAt(i).second);
                    }
                }
                feature.Add(mentionTags.ToString());
               // mention IDs
                var mentionIDs = mentionTags.Clear();
                foreach(var word in words)
                {
                    if (mentionIDs.Length == 0)
                    {
                        mentionIDs.Append(DataCenter.GetWordClusterID(word));
                    }
                    else
                    {
                        mentionIDs.Append("," + DataCenter.GetWordClusterID(word));
                    }
                }
                feature.Add(mentionIDs.ToString());
              // mention shapes
                var mentionShapes = mentionIDs.Clear();
                foreach (var word in words)
                {
                    if (mentionShapes.Length == 0)
                    {
                        mentionShapes.Append(GetWordShape(word));
                    }
                    else
                    {
                        mentionShapes.Append("," + GetWordShape(word));
                    }
                }
                feature.Add(mentionShapes.ToString());
            }
            #endregion

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

            #region Other System

            #region Stanford NER
            {
                var ner = StanfordNerPool.GetStanfordNer();
                ner.FindNer(context);
                var type = ner.GetNerType(mention);
                feature.Add(type);
            }
            #endregion

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
                // pos tag
                AddToFeature(word, generalsurface, posTag ?? "NULL", ID, shape);
            }
            else
            {
                AddToFeature("NULL","NULL", "NULL", DataCenter.GetClusterNumber().ToString(), "NULL");
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
