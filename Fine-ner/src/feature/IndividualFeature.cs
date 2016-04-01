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
        // context surface and pos tag pairs
        List<Pair<string, string>> contextTokenPairs = null;

        Pair<int, int> mentionIndexPair = null;

        List<string> mentionTokens = null;

        List<string> contextTokens = null;


        static System.Text.RegularExpressions.Regex allCharRegex = new System.Text.RegularExpressions.Regex(@"\W");

        public IndividualFeature() : base()
        {
        }

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
        public List<string> ExtractFeature(Instance instance)
        {
            this.instance = instance;
            instance = null;
            this.feature.Clear();
            FilterContext();
            Tokenizer();
            if (mentionIndexPair.first == -1)
            {
                throw new NotFindMentionException("Cannot find mention by token within context!");
            }

            var MentionDriverIndex = 0;
            var MentionAdjModifierIndex = 0;
            var MentionActionIndex = 0;
            if (useMentionDriver || useMentionAction || useMentionAdjModifier)
            {
                var parser = ParserPool.GetParser();
                try
                {
                    parser.Parse(string.Join(" ",this.contextTokens));
                }
                catch (Exception e)
                {
                    ParserPool.ReturnParser(parser);
                    parser = null;
                    throw e;
                }
                contextTokenPairs = parser.GetPosTags();
                MentionDriverIndex = parser.GetDriver(this.mentionIndexPair.first, this.mentionIndexPair.second);
                MentionActionIndex = parser.GetAction(this.mentionIndexPair.first, this.mentionIndexPair.second);
                MentionAdjModifierIndex = parser.GetAdjModifier(this.mentionIndexPair.first, this.mentionIndexPair.second);
                ParserPool.ReturnParser(parser);
            }
            else if (useWordTag)
            {
                var posTagger = PosTaggerPool.GetPosTagger();
                try
                {
                    contextTokenPairs = posTagger.TagString(string.Join(" ", this.contextTokens));
                    PosTaggerPool.ReturnPosTagger(posTagger);
                }
                catch (Exception e)
                {
                    PosTaggerPool.ReturnPosTagger(posTagger);
                    posTagger = null;
                    throw e;
                }
            }

            #region last word
            if(useLastWord)
            {
                var index = GetLastTokenIndex();
                if (index >= 0)
                {
                    var word = contextTokens[index];
                    if (useWordTag)
                    {
                        var posTag = contextTokenPairs.ElementAt(index).second;
                        AddFieldToFeture(word, posTag);
                    }
                    else
                    {
                        AddFieldToFeture(word);
                    }
                }
                else
                {
                    AddFieldToFeture(null);
                }
            }
            #endregion

            #region next word
            if(useNextWord)
            {
                var index = GetNextTokenIndex();
                if (index < contextTokenPairs.Count)
                {
                    var word = contextTokenPairs.ElementAt(index).first;
                    if(useWordTag)
                    {
                        var posTag = contextTokenPairs.ElementAt(index).second;
                        AddFieldToFeture(word, posTag);
                    }
                    else
                    {
                        AddFieldToFeture(word);
                    }

                }
                else
                {
                    AddFieldToFeture(null, null);

                }
            }
            #endregion

            #region mention head
            if(useMentionHead)
            {
                string head = null, posTag = null;
                if (useWordTag)
                {
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
                }
                if (head == null)
                {
                    head = mentionTokens[mentionTokens.Count - 1];
                    if (useWordTag)
                    {
                        posTag = contextTokenPairs.ElementAt(mentionIndexPair.second).second;
                    }
                }
                AddFieldToFeture(head, posTag);
            }
            #endregion

            #region mention Driver
            if(useMentionDriver)
            {
                if (MentionDriverIndex > 0)
                {
                    var driver = contextTokenPairs.ElementAt(MentionDriverIndex).first;
                    var posTag = contextTokenPairs.ElementAt(MentionDriverIndex).second;
                    AddFieldToFeture(driver, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            #region mention Modifier
            if(useMentionAdjModifier)
            {
                if (MentionAdjModifierIndex > 0)
                {
                    var driver = contextTokenPairs.ElementAt(MentionAdjModifierIndex).first;
                    var posTag = contextTokenPairs.ElementAt(MentionAdjModifierIndex).second;
                    AddFieldToFeture(driver, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            #region mention Action
            if (useMentionAction)
            {
                if (MentionActionIndex > 0)
                {
                    var driver = contextTokenPairs.ElementAt(MentionActionIndex).first;
                    var posTag = contextTokenPairs.ElementAt(MentionActionIndex).second;
                    AddFieldToFeture(driver, posTag);
                }
                else
                {
                    AddFieldToFeture(null, null);
                }
            }
            #endregion

            #region Mention Words
            if (useMentionSurfaces)
            {
                // mention surfaces
                var buffer = new StringBuilder();
                foreach (var word in mentionTokens)
                {
                    if (buffer.Length == 0)
                    {
                        buffer.Append(Generalizer.Generalize(word));
                    }
                    else
                    {
                        buffer.Append("," + Generalizer.Generalize(word));
                    }
                }
                // add mention surface
                feature.Add(string.Join(",", mentionTokens));
                // add stemmed mention surface
                feature.Add(buffer.ToString());
                // mention tags
                if (useWordTag)
                {
                    buffer.Clear();
                    for (var i = mentionIndexPair.first; i <= mentionIndexPair.second; i++)
                    {
                        if (buffer.Length == 0)
                        {
                            buffer.Append(contextTokenPairs.ElementAt(i).second);
                        }
                        else
                        {
                            buffer.Append("," + contextTokenPairs.ElementAt(i).second);
                        }
                    }
                    feature.Add(buffer.ToString());
                }
                // mention IDs
                if (useWordID)
                {
                buffer.Clear();
                    foreach (var word in mentionTokens)
                    {
                        if (buffer.Length == 0)
                        {
                            buffer.Append(DataCenter.GetWordClusterID(word));
                        }
                        else
                        {
                            buffer.Append("," + DataCenter.GetWordClusterID(word));
                        }
                    }
                    feature.Add(buffer.ToString());
                }
                // mention shapes
                if (useWordShape)
                {
                    buffer.Clear();
                    foreach (var word in mentionTokens)
                    {
                        if (buffer.Length == 0)
                        {
                            buffer.Append(GetWordShape(word));
                        }
                        else
                        {
                            buffer.Append("," + GetWordShape(word));
                        }
                    }
                    feature.Add(buffer.ToString());
                }
            
            }
            #endregion

            #region mention ID
            if(useMentionID)
            {
                feature.Add(DataCenter.GetMentionClusterID(this.instance.Mention).ToString());
            }
            #endregion

            #region mention length
            if(useMentionLength)
            {
                feature.Add(mentionTokens.Count.ToString());
            }
            #endregion

            #region DBpedia dictionary
            {
                if (useDbpediaTypesWithIndegree)
                {
                    var types = string.Join(",", DataCenter.GetDBpediaTypeWithIndegree(this.instance.Mention));
                    feature.Add(types);
                }
                if (useDbpediaTypesWithAbstract)
                {
                    var types = string.Join(",", DataCenter.GetDBpediaTypeWithAbstract(this.instance.Mention, this.instance.Context));
                    feature.Add(types);
                }
            }
            #endregion

            #region Key words
            if(useKeywords)
            {
                var keyWords = DataCenter.ExtractKeyWords(this.instance.Context);
                feature.Add(string.Join(",", keyWords));
            }
            #endregion

            feature.Add(string.Join(" ", contextTokens));
            return feature;
        }

        public List<string> AddFeature(Event e)
        {
            var rawFeature = (List<string>)e.Feature;
            var mention = rawFeature.ElementAt(Parameter.GetTypeLabel("mentionSurfaces")).Replace(',', ' ');
            var context = rawFeature.ElementAt(Parameter.GetTypeLabel("sentenceContext"));
            #region Stanford NER
            if (true)
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
                rawFeature[Parameter.GetTypeLabel("opennlpNerType")] = type;
            }
            #endregion

            #region DBpedia dictionary
            if (false)
            {
                var types = string.Join(",", DataCenter.GetDBpediaTypeWithIndegree(mention));
                rawFeature[Parameter.GetTypeLabel("dbpediaTypesWithIndegree")] = types;
                types = string.Join(",", DataCenter.GetDBpediaTypeWithAbstract(mention, context));
                rawFeature[Parameter.GetTypeLabel("dbpediaTypesWithAbstract")] = types;
            }
            #endregion

            List<Pair<string, string>> pairs = null;
            Pair<int, int> pair = null;

            #region Modify last word
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\W");

            if (false)
            {
                var lastWord = rawFeature.ElementAt(Parameter.GetTypeLabel("lastWord"));
                if (lastWord.Equals("##") || lastWord.Equals(".") || lastWord.Equals("!") || lastWord.Equals("?") || lastWord.Equals(";"))
                {
                    rawFeature[Parameter.GetTypeLabel("lastWord")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("lastWordStemmed")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("lastWordTag")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("lastWordID")] = "100";
                    rawFeature[Parameter.GetTypeLabel("lastWordShape")] = "NULL";
                }
                else if (!lastWord.Equals("'s") && regex.IsMatch(lastWord))
                {
                    var pos = PosTaggerPool.GetPosTagger();
                    try
                    {
                        //pairs = pos.TagString(context);
                        //PosTaggerPool.ReturnPosTagger(pos);
                        //pair = GetIndexOfMention(pairs, mention);
                        //var index = pair.first - 1;
                        //while (index >= 0)
                        //{
                        //    if (pairs[index].first.Equals("##") || pairs[index].first.Equals(".") || pairs[index].first.Equals("!") || pairs[index].first.Equals("?") || pairs[index].first.Equals(";"))
                        //    {
                        //        index = -1;
                        //        break;
                        //    }
                        //    else if (!pairs[index].first.Equals("'s") && regex.IsMatch(pairs[index].first))
                        //    {
                        //        index--;
                        //    }
                        //    else
                        //    {
                        //        break;
                        //    }
                        //}
                        //if (index >= 0)
                        //{
                        //    var word = pairs.ElementAt(index).first;
                        //    var posTag = pairs.ElementAt(index).second;
                        //    var wordStemmed = Generalizer.Generalize(word);
                        //    var ID = DataCenter.GetWordClusterID(word).ToString();    // id should use original surface
                        //    var shape = GetWordShape(word);

                        //    rawFeature[Parameter.GetTypeLabel("lastWord")] = word;
                        //    rawFeature[Parameter.GetTypeLabel("lastWordStemmed")] = wordStemmed;
                        //    rawFeature[Parameter.GetTypeLabel("lastWordTag")] = posTag;
                        //    rawFeature[Parameter.GetTypeLabel("lastWordID")] = ID;
                        //    rawFeature[Parameter.GetTypeLabel("lastWordShape")] = shape;
                        //}
                        //else
                        //{
                        //    rawFeature[Parameter.GetTypeLabel("lastWord")] = "NULL";
                        //    rawFeature[Parameter.GetTypeLabel("lastWordStemmed")] = "NULL";
                        //    rawFeature[Parameter.GetTypeLabel("lastWordTag")] = "NULL";
                        //    rawFeature[Parameter.GetTypeLabel("lastWordID")] = "100";
                        //    rawFeature[Parameter.GetTypeLabel("lastWordShape")] = "NULL";
                        //}
                        //PosTaggerPool.ReturnPosTagger(pos);
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
                var nextWord = rawFeature.ElementAt(Parameter.GetTypeLabel("nextWord"));
                if (nextWord.Equals("##") || nextWord.Equals(".") || nextWord.Equals("!") || nextWord.Equals("?") || nextWord.Equals(";"))
                {
                    rawFeature[Parameter.GetTypeLabel("nextWord")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("nextWordStemmed")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("nextWordTag")] = "NULL";
                    rawFeature[Parameter.GetTypeLabel("nextWordID")] = "100";
                    rawFeature[Parameter.GetTypeLabel("nextWordShape")] = "NULL";
                }
                else if (!nextWord.Equals("'s") && regex.IsMatch(nextWord))
                {
                    if (pairs == null)
                    {
                        var pos = PosTaggerPool.GetPosTagger();
                        try
                        {
                            //pairs = pos.TagString(context);
                            //PosTaggerPool.ReturnPosTagger(pos);
                            //pair = GetIndexOfMention(pairs, mention);
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

                        rawFeature[Parameter.GetTypeLabel("nextWord")] = word;
                        rawFeature[Parameter.GetTypeLabel("nextWordStemmed")] = wordStemmed;
                        rawFeature[Parameter.GetTypeLabel("nextWordTag")] = posTag;
                        rawFeature[Parameter.GetTypeLabel("nextWordID")] = ID;
                        rawFeature[Parameter.GetTypeLabel("nextWordShape")] = shape;
                    }
                    else
                    {
                        rawFeature[Parameter.GetTypeLabel("nextWord")] = "NULL";
                        rawFeature[Parameter.GetTypeLabel("nextWordStemmed")] = "NULL";
                        rawFeature[Parameter.GetTypeLabel("nextWordTag")] = "NULL";
                        rawFeature[Parameter.GetTypeLabel("nextWordID")] = "100";
                        rawFeature[Parameter.GetTypeLabel("nextWordShape")] = "NULL";
                    }
                }
            }
            #endregion

            return rawFeature;
        }

        #region Private Methods
        private void AddFieldToFeture(string word, string posTag = null)
        {
            if (word != null)
            {
                string generalsurface, ID, shape;
                this.feature.Add(word);
                // stemmed word
                generalsurface = Generalizer.Generalize(word);
                this.feature.Add(generalsurface);
                // pos tag
                if (Parameter.UseFeature("wordTag"))
                {
                    this.feature.Add(posTag ?? "NULL");
                }
                // Cluster id of last word
                if (Parameter.UseFeature("wordID"))
                {
                    this.feature.Add(DataCenter.GetWordClusterID(word).ToString());
                }
                // next word shape
                if (Parameter.UseFeature("wordShape"))
                {
                    this.feature.Add(GetWordShape(word));
                }

            }
            else
            {
                this.feature.Add("NULL");
                // stemmed word
                this.feature.Add("NULL");
                // pos tag
                if (Parameter.UseFeature("wordTag"))
                {
                    this.feature.Add("NULL");
                }
                // Cluster id of last word
                if (Parameter.UseFeature("wordID"))
                {
                    this.feature.Add(DataCenter.GetClusterNumber().ToString());
                }
                // word shape
                if (Parameter.UseFeature("wordShape"))
                {
                    this.feature.Add(GetWordShape("NULL"));
                }

            }
        }

        private void Tokenizer()
        {
            var tokenizer = TokenizerPool.GetTokenizer();
            this.mentionTokens = new List<string>();
            this.contextTokens = new List<string>();
            int begin = -1;
            int end = -1;
            try
            {
                var ts = tokenizer.Tokenize(this.instance.Mention);
                for (var i = 0; i < ts.Count; i++)
                {
                    if (ts[i].first.Equals(".") && i > 0 && ts[i - 1].first.EndsWith("."))
                    {
                        continue;
                    }
                    this.mentionTokens.Add(ts[i].first);
                }
                ts = tokenizer.Tokenize(this.instance.Context);
                for (var i = 0; i < ts.Count; i++)
                {
                    if (ts[i].first.Equals(".") && i > 0 && ts[i - 1].first.EndsWith("."))
                    {
                        continue;
                    }
                    if(ts[i].second<=instance.MentionOffset && (ts[i].second+ts[i].first.Length) > instance.MentionOffset)
                    {
                        begin = i;
                    }
                    if(begin>-1 && end == -1)
                    {
                        if((ts[i].second+ts[i].first.Length)>=(instance.MentionOffset+instance.MentionLength))
                        {
                            end = i;
                            this.mentionIndexPair = new Pair<int, int>(begin, end);
                        }
                    }
                    this.contextTokens.Add(ts[i].first);
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
            List<Pair<string,int>> sentences = null;
            var sspliter = SSpliterPool.GetSSpliter();
            try
            {
                sentences = sspliter.SplitSequence(instance.Context);
                SSpliterPool.ReturnSSpliter(sspliter);
            }
            catch (Exception e)
            {
                SSpliterPool.ReturnSSpliter(sspliter);
                throw e;
            }
            var buffer = new StringBuilder();
            var offset = -1;
            foreach (var sentence in sentences)
            {
                if(sentence.second <= instance.MentionOffset && (sentence.second+sentence.first.Length)>=(instance.MentionOffset+instance.MentionLength))  // sentence cover
                {
                    offset = sentence.second;
                    buffer.Append(sentence.first);
                    break;
                }
                if((sentence.second+sentence.first.Length)>= instance.MentionOffset && (sentence.second+sentence.first.Length)<=(instance.MentionOffset+instance.MentionLength))
                {
                    buffer.Append(sentence);
                    if(offset==-1)
                    {
                        offset = sentence.second;
                    }
                }
            }
            this.instance = new Instance(buffer.ToString(), instance.MentionOffset - offset, instance.MentionLength);
        }

        static System.Text.RegularExpressions.Regex terminators = new System.Text.RegularExpressions.Regex("[.;!?]");

        private int GetLastTokenIndex()
        {
            var index = mentionIndexPair.first - 1;
            while (index >= 0)
            {

                if (contextTokens[index].Equals("##") ||
                    terminators.IsMatch(contextTokens[index]))
                {       // if it is sentence terminator
                    index = -1;
                    break;
                }
                else if (!contextTokens[index].Equals("'s") && allCharRegex.IsMatch(contextTokens[index])) // skip "(,",'"
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
            while (index < contextTokens.Count)
            {
                if (contextTokens[index].Equals("##") || 
                    terminators.IsMatch(contextTokens[index]))
                {
                    index = contextTokens.Count;
                    break;
                }
                else if (!contextTokens[index].Equals("'s") && allCharRegex.IsMatch(contextTokens[index]))
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
