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
            if (useWordTag)
            {
                var posTagger = PosTaggerPool.GetPosTagger();
                try
                {
                    contextTokenPairs = posTagger.TagString(string.Join(" ", contextTokens));
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

            #region Mention Words
            if(useMentionSurfaces)
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
                // pos tag
                if (Parameter.UseFeature("wordTag"))
                {
                    this.feature.Add(posTag ?? "NULL");
                }
            }
            else
            {
                this.feature.Add("NULL");
                // stemmed word
                this.feature.Add("NULL");
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
                // pos tag
                if (Parameter.UseFeature("wordTag"))
                {
                    this.feature.Add("NULL");
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
