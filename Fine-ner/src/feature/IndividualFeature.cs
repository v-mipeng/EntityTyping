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

        internal IndividualFeature() : base()
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
            this.feature.Clear();
            try
            {
                FilterContext();
            }
            catch(Exception)
            {
                this.instance = instance;
                instance = null;
            }
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
                var keywords = DataCenter.ExtractKeyWords(this.instance.Context);
                var distances = new List<Pair<string, double>>();
                var distance = 0.0;
                var totalDistance = 0.0;
                foreach (var keyword in keywords)
                {
                    distance = Math.Exp(-1.0 * GetKeywordDistance(keyword) / contextTokens.Count);
                    if (distance != -1)
                    {
                        distances.Add(new Pair<string, double>(keyword, distance));
                        totalDistance += distance;
                    }
                }
                var buffer = new StringBuilder();
                for (var i = 0; i < distances.Count; i++)
                {
                    if (buffer.Length == 0)
                    {
                        buffer.Append(distances[i].first + ":" + (distances[i].second / totalDistance));
                    }
                    else
                    {
                        buffer.Append("," + distances[i].first + ":" + (distances[i].second / totalDistance));
                    }
                }
                feature.Add(buffer.ToString());
            }
            #endregion

            feature.Add(string.Join(" ", contextTokens));
            return feature;
        }

        static System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\b\w{2,}\b");
        static System.Text.RegularExpressions.Regex deleteComma = new System.Text.RegularExpressions.Regex(@",+");
        public List<string> AddFeature(Event e)
        {
            var rawFeature = (List<string>)e.Feature;
            var mention = deleteComma.Replace(rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionSurfaces")), " ");
            var context = rawFeature.ElementAt(Parameter.GetFeatureIndex("sentenceContext")).Replace(" , "," ");
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
                rawFeature[Parameter.GetFeatureIndex("opennlpNerType")] = type;
            }
            #endregion

            #region DBpedia dictionary
            if (false)
            {
                var types = string.Join(",", DataCenter.GetDBpediaTypeWithIndegree(mention));
                rawFeature[Parameter.GetFeatureIndex("dbpediaTypesWithIndegree")] = types;
                types = string.Join(",", DataCenter.GetDBpediaTypeWithAbstract(mention, context));
                rawFeature[Parameter.GetFeatureIndex("dbpediaTypesWithAbstract")] = types;
            }
            #endregion

            List<Pair<string, string>> pairs = null;
            Pair<int, int> pair = null;

            #region Modify last word
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\W");

            if (false)
            {
                var lastWord = rawFeature.ElementAt(Parameter.GetFeatureIndex("lastWord"));
                if (lastWord.Equals("##") || lastWord.Equals(".") || lastWord.Equals("!") || lastWord.Equals("?") || lastWord.Equals(";"))
                {
                    rawFeature[Parameter.GetFeatureIndex("lastWord")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("lastWordStemmed")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("lastWordTag")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("lastWordID")] = "100";
                    rawFeature[Parameter.GetFeatureIndex("lastWordShape")] = "NULL";
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

                        //    rawFeature[Parameter.GetFeatureIndex("lastWord")] = word;
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordStemmed")] = wordStemmed;
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordTag")] = posTag;
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordID")] = ID;
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordShape")] = shape;
                        //}
                        //else
                        //{
                        //    rawFeature[Parameter.GetFeatureIndex("lastWord")] = "NULL";
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordStemmed")] = "NULL";
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordTag")] = "NULL";
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordID")] = "100";
                        //    rawFeature[Parameter.GetFeatureIndex("lastWordShape")] = "NULL";
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
                var nextWord = rawFeature.ElementAt(Parameter.GetFeatureIndex("nextWord"));
                if (nextWord.Equals("##") || nextWord.Equals(".") || nextWord.Equals("!") || nextWord.Equals("?") || nextWord.Equals(";"))
                {
                    rawFeature[Parameter.GetFeatureIndex("nextWord")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("nextWordStemmed")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("nextWordTag")] = "NULL";
                    rawFeature[Parameter.GetFeatureIndex("nextWordID")] = "100";
                    rawFeature[Parameter.GetFeatureIndex("nextWordShape")] = "NULL";
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

                        rawFeature[Parameter.GetFeatureIndex("nextWord")] = word;
                        rawFeature[Parameter.GetFeatureIndex("nextWordStemmed")] = wordStemmed;
                        rawFeature[Parameter.GetFeatureIndex("nextWordTag")] = posTag;
                        rawFeature[Parameter.GetFeatureIndex("nextWordID")] = ID;
                        rawFeature[Parameter.GetFeatureIndex("nextWordShape")] = shape;
                    }
                    else
                    {
                        rawFeature[Parameter.GetFeatureIndex("nextWord")] = "NULL";
                        rawFeature[Parameter.GetFeatureIndex("nextWordStemmed")] = "NULL";
                        rawFeature[Parameter.GetFeatureIndex("nextWordTag")] = "NULL";
                        rawFeature[Parameter.GetFeatureIndex("nextWordID")] = "100";
                        rawFeature[Parameter.GetFeatureIndex("nextWordShape")] = "NULL";
                    }
                }
            }
            #endregion

            #region Weight Keywords
            if (true)
            {
                var keywords = rawFeature.ElementAt(Parameter.GetFeatureIndex("keywords")).Split(',');
                this.instance = new Instance(mention, context);
                Tokenizer();
                var distances = new List<Pair<string, double>>();
                var distance = 0.0;
                var totalDistance = 0.0;
                foreach(var keyword in keywords)
                {
                    distance = Math.Exp(-1.0 * GetKeywordDistance(keyword) / contextTokens.Count);
                    if (distance != -1)
                    {
                        distances.Add(new Pair<string, double>(keyword, distance));
                        totalDistance += distance;
                    }
                }
                var buffer = new StringBuilder();
                for(var i = 0;i<distances.Count;i++)
                {
                    if (buffer.Length == 0)
                    {
                        buffer.Append(distances[i].first + ":" + (distances[i].second / totalDistance));
                    }
                    else
                    {
                        buffer.Append(","+distances[i].first + ":" + (distances[i].second / totalDistance));
                    }
                }
                rawFeature[Parameter.GetFeatureIndex("keywords")] = buffer.ToString();
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
            this.mentionIndexPair = null;
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
                    if(begin == -1 && ts[i].second<=instance.MentionOffset && (ts[i].second+ts[i].first.Length) >= instance.MentionOffset)
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
                if(mentionIndexPair == null)
                {
                    throw new Exception("Cannot find mention in context!");
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
            var end = -1;
            foreach (var sentence in sentences)
            {
                if(sentence.second <= instance.MentionOffset && (sentence.second+sentence.first.Length)>=(instance.MentionOffset+instance.MentionLength))  // sentence cover
                {
                    offset = sentence.second;
                    buffer.Append(sentence.first);
                    break;
                }
                if(((sentence.second+sentence.first.Length-1)>= instance.MentionOffset && (sentence.second+sentence.first.Length)<=(instance.MentionOffset+instance.MentionLength))
                    || (sentence.second>=instance.MentionOffset && sentence.second<=(instance.MentionOffset+instance.MentionLength-1)))
                {
                    if (end > -1)
                    {
                        for (var i = 1; i < sentence.second - end; i++)
                        {
                            buffer.Append(" ");
                        }
                    }
                    buffer.Append(sentence.first);
                    if(offset==-1)
                    {
                        offset = sentence.second;
                    }
                    end = sentence.second + sentence.first.Length - 1;
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
                if (this.contextTokens[index].Equals("##") || terminators.IsMatch(this.contextTokens[index]))
                {       // if it is sentence terminator
                    index = -1;
                    break;
                }
                else if (!this.contextTokens[index].Equals("'s") && allCharRegex.IsMatch(this.contextTokens[index])) // skip "(,",'"
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

        /// <summary>
        /// Get the index of keyword nearest to the mention
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private int GetKeywordDistance(string keyword)
        {
        
            var formerIndex = -1;
            var behindIndex = -1;
            for (var i = 0; i < mentionIndexPair.first;i++ )
            {
                if(Generalizer.Generalize(contextTokens[i]).Contains(keyword))
                {
                    formerIndex = i;
                }
            }
            for (var i = contextTokens.Count - 1; i > mentionIndexPair.second; i--)
            {
                if (Generalizer.Generalize(contextTokens[i]).Contains(keyword))
                {
                    behindIndex = i;
                }
            }
            if(formerIndex == -1 && behindIndex >0)
            {
                return behindIndex - mentionIndexPair.second;
            }
            else if (formerIndex != -1 && behindIndex == -1)
            {
                return mentionIndexPair.first - formerIndex;
            }
            else if(formerIndex != -1 && behindIndex != -1)
            {
                return (mentionIndexPair.first - formerIndex) > (behindIndex - mentionIndexPair.second) ? (behindIndex - mentionIndexPair.second) : (mentionIndexPair.first - formerIndex);
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }
}
