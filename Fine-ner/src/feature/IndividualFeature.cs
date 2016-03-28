#define debug
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
        // feature list
        private List<string> feature = new List<string>();
        // input instance
        Instance instance = null;
        // context of mention
        string context = null;
        // context surface and pos tag pairs
        List<Pair<string, string>> contextTokenPairs = null;

        Pair<int, int> mentionIndexPair = null;

        List<string> mentionTokens = null;

        List<string> contextTokens = null;

        static System.Text.RegularExpressions.Regex allCharRegex = new System.Text.RegularExpressions.Regex(@"\W");


        public IndividualFeature() : base() { }


        /// <summary>
        ///  Extract feature from the input, and the feature is clustered by field
        /// </summary>
        /// <param name="instance">
        /// An instance of query with label or not.
        /// </param>
        /// <returns>
        /// A list of features including: Please refer Event to get the order of features
        ///     Mention words  
        ///     Mention shapes
        ///     Mention word cluster IDs
        ///     Mention length         
        ///     Mention cluster ID                      
        ///     Last token
        ///     Last token pos tag
        ///     Last token cluster ID                   
        ///     Next token
        ///     Next token pos tag
        ///     Next token cluster ID                   
        ///     Dictionary                      :Dbpedia
        ///     Topic(Define topic)             :MI keyword
        /// </returns>
        public List<string> ExtractFeature(Instance instance, bool filterContext = true)
        {
            this.instance = instance;
            this.feature.Clear();
            Tokenizer();
            if (filterContext)
            {
                FilterContext();
            }
            else
            {
                this.context = instance.Context;
            }

            var posTagger = PosTaggerPool.GetPosTagger();
            try
            {
                contextTokenPairs = posTagger.TagString(context);
                mentionIndexPair = GetIndexOfMention(contextTokenPairs, mentionTokens);
                PosTaggerPool.ReturnPosTagger(posTagger);
            }
            catch (Exception e)
            {
                PosTaggerPool.ReturnPosTagger(posTagger);
                posTagger = null;
                throw e;
            }
            if (mentionIndexPair.first == -1)
            {
                throw new NotFindMentionException("Cannot find mention by token within context!");
            }

            #region last word
            {
                var index = GetLastTokenIndex();
                if (index >= 0)
                {
                    var word = contextTokenPairs.ElementAt(index).first;
                    var posTag = contextTokenPairs.ElementAt(index).second;
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
                var index = GetNextTokenIndex();
                if (index < contextTokenPairs.Count)
                {
                    var word = contextTokenPairs.ElementAt(index).first;
                    var posTag = contextTokenPairs.ElementAt(index).second;
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
                for (int i = mentionIndexPair.first; i <= mentionIndexPair.second; i++)
                {
                    if (contextTokenPairs.ElementAt(i).second.StartsWith("N"))
                    {
                        // last noun
                        head = contextTokenPairs.ElementAt(i).first;
                        posTag = contextTokenPairs.ElementAt(i).second;
                    }
                    else if (contextTokenPairs.ElementAt(i).second.Equals("IN") || contextTokenPairs.ElementAt(i).second.Equals(","))
                    {
                        // before IN
                        break;
                    }
                }
                if (head == null)
                {
                    head = mentionTokens[mentionTokens.Count - 1];
                    posTag = contextTokenPairs.ElementAt(mentionIndexPair.second).second;
                }
                AddFieldToFeture(head, posTag);
            }
            #endregion

            #region Mention Words
            {
                // mention surfaces
                var mentionWords = new StringBuilder();
                foreach (var word in mentionTokens)
                {
                    if (mentionWords.Length == 0)
                    {
                        mentionWords.Append(Generalizer.Generalize(word));
                    }
                    else
                    {
                        mentionWords.Append("," + Generalizer.Generalize(word));
                    }
                }
                // add mention surface
                feature.Add(string.Join(",", mentionTokens));
                // add stemmed mention surface
                feature.Add(mentionWords.ToString());
                // mention tags
                var mentionTags = mentionWords.Clear();
                for (var i = mentionIndexPair.first; i <= mentionIndexPair.second; i++)
                {
                    if (mentionTags.Length == 0)
                    {
                        mentionTags.Append(contextTokenPairs.ElementAt(i).second);
                    }
                    else
                    {
                        mentionTags.Append("," + contextTokenPairs.ElementAt(i).second);
                    }
                }
                feature.Add(mentionTags.ToString());
                // mention IDs
                var mentionIDs = mentionTags.Clear();
                foreach (var word in mentionTokens)
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
                foreach (var word in mentionTokens)
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
                feature.Add(DataCenter.GetMentionClusterID(this.instance.Mention).ToString());
            }
            #endregion

            #region mention length
            {
                feature.Add(mentionTokens.Count.ToString());
            }
            #endregion

            #region DBpedia dictionary
            {
                var types = string.Join(",", DataCenter.GetDBpediaTypeWithIndegree(this.instance.Mention));
                feature.Add(types);
                types = string.Join(",", DataCenter.GetDBpediaTypeWithAbstract(this.instance.Mention, this.instance.Context));
                feature.Add(types);
            }
            #endregion

            #region Key words
            {
                var keyWords = DataCenter.ExtractKeyWords(context);
                feature.Add(string.Join(",", keyWords));
            }
            #endregion

            feature.Add(context);
            return feature;
        }

        public List<string> AddFeature(Event e)
        {
            var rawFeature = (List<string>)e.Feature;
            var mention = rawFeature.ElementAt((int)Event.Field.mentionSurfaces).Replace(',', ' ');
            var context = rawFeature.ElementAt((int)Event.Field.sentenceContext);
            #region Stanford NER
            if (false)
            {

                var ner = StanfordNerPool.GetStanfordNer();
                ner.FindNer(context);
                var type = ner.GetNerType(mention);
                StanfordNerPool.ReturnStanfordNer(ner);
                ner = null;
                feature.Add(type);
            }
            #endregion

            #region OpenNLP NER
            if (false)
            {
                var ner = OpenNerPool.GetOpenNer();
                ner.FindNer(context);
                var type = ner.GetNerType(mention);
                OpenNerPool.ReturnOpenNer(ner);
                ner = null;
                rawFeature[(int)Event.Field.opennlpNerType] = type;
            }
            #endregion

            #region DBpedia dictionary
            if(true)
            {
                var types = string.Join(",", DataCenter.GetDBpediaTypeWithIndegree(mention));
                rawFeature[(int)Event.Field.dbpediaTypesWithIndegree] = types;
                types = string.Join(",", DataCenter.GetDBpediaTypeWithAbstract(mention, context));
                rawFeature[(int)Event.Field.dbpediaTypesWithAbstract] = types;
            }
            #endregion

            List<Pair<string, string>> pairs = null;
            Pair<int, int> pair = null;

            #region Modify last word
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\W");

            if (false)
            {
                var lastWord = rawFeature.ElementAt((int)Event.Field.lastWord);
                if (lastWord.Equals("##") || lastWord.Equals(".") || lastWord.Equals("!") || lastWord.Equals("?") || lastWord.Equals(";"))
                {
                    rawFeature[(int)Event.Field.lastWord] = "NULL";
                    rawFeature[(int)Event.Field.lastWordStemmed] = "NULL";
                    rawFeature[(int)Event.Field.lastWordTag] = "NULL";
                    rawFeature[(int)Event.Field.lastWordID] = "100";
                    rawFeature[(int)Event.Field.lastWordShape] = "NULL";
                }
                else if (!lastWord.Equals("'s") && regex.IsMatch(lastWord))
                {
                    var pos = PosTaggerPool.GetPosTagger();
                    try
                    {
                        pairs = pos.TagString(context);
                        PosTaggerPool.ReturnPosTagger(pos);
                        pair = GetIndexOfMention(pairs, mention);
                        var index = pair.first - 1;
                        while (index >= 0)
                        {
                            if (pairs[index].first.Equals("##") || pairs[index].first.Equals(".") || pairs[index].first.Equals("!") || pairs[index].first.Equals("?") || pairs[index].first.Equals(";"))
                            {
                                index = -1;
                                break;
                            }
                            else if (!pairs[index].first.Equals("'s") && regex.IsMatch(pairs[index].first))
                            {
                                index--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (index >= 0)
                        {
                            var word = pairs.ElementAt(index).first;
                            var posTag = pairs.ElementAt(index).second;
                            var wordStemmed = Generalizer.Generalize(word);
                            var ID = DataCenter.GetWordClusterID(word).ToString();    // id should use original surface
                            var shape = GetWordShape(word);

                            rawFeature[(int)Event.Field.lastWord] = word;
                            rawFeature[(int)Event.Field.lastWordStemmed] = wordStemmed;
                            rawFeature[(int)Event.Field.lastWordTag] = posTag;
                            rawFeature[(int)Event.Field.lastWordID] = ID;
                            rawFeature[(int)Event.Field.lastWordShape] = shape;
                        }
                        else
                        {
                            rawFeature[(int)Event.Field.lastWord] = "NULL";
                            rawFeature[(int)Event.Field.lastWordStemmed] = "NULL";
                            rawFeature[(int)Event.Field.lastWordTag] = "NULL";
                            rawFeature[(int)Event.Field.lastWordID] = "100";
                            rawFeature[(int)Event.Field.lastWordShape] = "NULL";
                        }
                        PosTaggerPool.ReturnPosTagger(pos);
                    }
                    catch (Exception ex)
                    {
                        PosTaggerPool.ReturnPosTagger(pos);
                        throw ex;
                    }
                }

            }
            #endregion

            #region Modify next word
            if (false)
            {
                var nextWord = rawFeature.ElementAt((int)Event.Field.nextWord);
                if (nextWord.Equals("##") || nextWord.Equals(".") || nextWord.Equals("!") || nextWord.Equals("?") || nextWord.Equals(";"))
                {
                    rawFeature[(int)Event.Field.nextWord] = "NULL";
                    rawFeature[(int)Event.Field.nextWordStemmed] = "NULL";
                    rawFeature[(int)Event.Field.nextWordTag] = "NULL";
                    rawFeature[(int)Event.Field.nextWordID] = "100";
                    rawFeature[(int)Event.Field.nextWordShape] = "NULL";
                }
                else if (!nextWord.Equals("'s") && regex.IsMatch(nextWord))
                {
                    if (pairs == null)
                    {
                        var pos = PosTaggerPool.GetPosTagger();
                        try
                        {
                            pairs = pos.TagString(context);
                            PosTaggerPool.ReturnPosTagger(pos);
                            pair = GetIndexOfMention(pairs, mention);
                        }
                        catch (Exception ex)
                        {
                            PosTaggerPool.ReturnPosTagger(pos);
                            throw ex;
                        }
                    }
                    var index = pair.second + 1;
                    while (index < pairs.Count)
                    {
                        if (pairs[index].first.Equals("##") || pairs[index].first.Equals(".") || pairs[index].first.Equals("!") || pairs[index].first.Equals("?") || pairs[index].first.Equals(";"))
                        {
                            index = pairs.Count;
                            break;
                        }
                        else if (!pairs[index].first.Equals("'s") && regex.IsMatch(pairs[index].first))
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (index < pairs.Count)
                    {
                        var word = pairs.ElementAt(index).first;
                        var posTag = pairs.ElementAt(index).second;
                        var wordStemmed = Generalizer.Generalize(word);
                        var ID = DataCenter.GetWordClusterID(word).ToString();    // id should use original surface
                        var shape = GetWordShape(word);

                        rawFeature[(int)Event.Field.nextWord] = word;
                        rawFeature[(int)Event.Field.nextWordStemmed] = wordStemmed;
                        rawFeature[(int)Event.Field.nextWordTag] = posTag;
                        rawFeature[(int)Event.Field.nextWordID] = ID;
                        rawFeature[(int)Event.Field.nextWordShape] = shape;
                    }
                    else
                    {
                        rawFeature[(int)Event.Field.nextWord] = "NULL";
                        rawFeature[(int)Event.Field.nextWordStemmed] = "NULL";
                        rawFeature[(int)Event.Field.nextWordTag] = "NULL";
                        rawFeature[(int)Event.Field.nextWordID] = "100";
                        rawFeature[(int)Event.Field.nextWordShape] = "NULL";
                    }
                }
            }
            #endregion

            #region   Modify mention ID
            if (false)
            {
                //var mentionID = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionID));
                //var mentionClusterNum = DataCenter.GetMentionClusterNumber();
                //if (mentionID == mentionClusterNum)
                //{
                var mentionID = DataCenter.GetMentionClusterID(mention);
                rawFeature[(int)Event.Field.mentionID] = mentionID.ToString();
                //}
            }
            #endregion

            #region Key words
            if (false)
            {
                var keyWords = DataCenter.ExtractKeyWords(context);
                rawFeature[(int)Event.Field.sentenceContext] = string.Join(",", keyWords);

                rawFeature.Add(context);
            }
            #endregion

            #region Word ID
            if(false)
            {
                var word = rawFeature[(int)Event.Field.lastWord];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.lastWordID] = id.ToString();
                }
                word = rawFeature[(int)Event.Field.nextWord];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.nextWordID] = id.ToString();
                } 
                word = rawFeature[(int)Event.Field.mentionAction];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.mentionActionID] = id.ToString();
                } 
                word = rawFeature[(int)Event.Field.mentionAdjModifier];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.mentionAdjModifierID] = id.ToString();
                } 
                word = rawFeature[(int)Event.Field.mentionDriver];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.mentionDriverID] = id.ToString();
                } 
                word = rawFeature[(int)Event.Field.mentionHead];
                if (!word.Equals("NULL"))
                {
                    var id = DataCenter.GetWordClusterID(word);
                    rawFeature[(int)Event.Field.mentionHeadID] = id.ToString();
                }
                var words = rawFeature[(int)Event.Field.mentionSurfaces].Split(',');
                var ids = new StringBuilder();
                foreach(var w in words)
                {
                    var id = DataCenter.GetWordClusterID(w);
                    if (ids.Length == 0)
                    {
                        ids.Append(id);
                    }
                    else
                    {
                        ids.Append("," + id);
                    }
                }
                rawFeature[(int)Event.Field.mentionIDs] = ids.ToString(); ;
            }
            #endregion

            return rawFeature;
        }

        #region Private Methods
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
                AddToFeature("NULL", "NULL", "NULL", DataCenter.GetClusterNumber().ToString(), "NULL");
            }
        }

        private void AddToFeature(params string[] objs)
        {
            foreach (var par in objs)
            {
                this.feature.Add(par);
            }
        }

        private void Tokenizer()
        {
            var tokenizer = TokenizerPool.GetTokenizer();
            this.mentionTokens = new List<string>();
            this.contextTokens = new List<string>();
            try
            {
                var ts = tokenizer.Tokenize(this.instance.Mention);
                for (var i = 0; i < ts.Count; i++)
                {
                    if (ts[i].Equals(".") && i > 0 && ts[i - 1].EndsWith("."))
                    {
                        continue;
                    }
                    this.mentionTokens.Add(ts[i]);
                }
                ts = tokenizer.Tokenize(this.instance.Context);
                for (var i = 0; i < ts.Count; i++)
                {
                    if (ts[i].Equals(".") && i > 0 && ts[i - 1].EndsWith("."))
                    {
                        continue;
                    }
                    this.contextTokens.Add(ts[i]);
                }
                TokenizerPool.ReturnTokenizer(tokenizer);
            }
            catch (Exception e)
            {
                TokenizerPool.ReturnTokenizer(tokenizer);   // return tokenizer to tokenizer pool
                throw e;
            }
        }

        /// <summary>
        /// Reture first sentence contains mention in context.
        /// </summary>
        private void FilterContext()
        {
            List<string> sentences = null;
            var sspliter = SSpliterPool.GetSSpliter();
            try
            {
                sentences = sspliter.SplitSequence(contextTokens);
                SSpliterPool.ReturnSSpliter(sspliter);
            }
            catch (Exception e)
            {
                SSpliterPool.ReturnSSpliter(sspliter);
                throw e;
            }
            this.context = GetSentenceCoverMention(sentences, mentionTokens);
            if (this.context == null)
            {
                throw new Exception("Cannot find mention by token within context!");
            }
        }

        private int GetLastTokenIndex()
        {
            var index = mentionIndexPair.first - 1;
            while (index >= 0)
            {

                if (contextTokenPairs[index].first.Equals("##") || contextTokenPairs[index].second.Equals(".") || contextTokenPairs[index].first.Equals(";"))
                {       // if it is sentence terminator
                    index = -1;
                    break;
                }
                else if (!contextTokenPairs[index].first.Equals("'s") && allCharRegex.IsMatch(contextTokenPairs[index].first)) // skip "(,",'"
                {
                    index--;
                }
                else
                {
                    break;
                }
            }
            return index;
        }

        private int GetNextTokenIndex()
        {
            var index = mentionIndexPair.second + 1;
            while (index < contextTokenPairs.Count)
            {
                if (contextTokenPairs[index].first.Equals("##") || contextTokenPairs[index].second.Equals(".") || contextTokenPairs[index].first.Equals(";"))
                {
                    index = contextTokenPairs.Count;
                    break;
                }
                else if (!contextTokenPairs[index].first.Equals("'s") && allCharRegex.IsMatch(contextTokenPairs[index].first))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }
            return index;
        }

        #endregion
    }
}
