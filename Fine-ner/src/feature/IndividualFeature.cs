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
        public List<string> ExtractFeature(Instance instance, bool filterContext = false)
        {
            this.instance = instance;
            instance = null;
            this.feature.Clear();
            FilterContext();
            Tokenizer();

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
                var keyWords = DataCenter.ExtractKeyWords(this.instance.Context);
                feature.Add(string.Join(",", keyWords));
            }
            #endregion

            feature.Add(string.Join(" ", contextTokens));
            return feature;
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
