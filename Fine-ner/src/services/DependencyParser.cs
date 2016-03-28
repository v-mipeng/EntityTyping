using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.trees;
using edu.stanford.nlp;
using edu.stanford.nlp.util;
using java.util;
using System.IO;
using pml.type;
using edu.stanford.nlp.semgraph;
using System.Text.RegularExpressions;
using System.Threading;

namespace msra.nlp.tr
{
    /// <summary>
    /// Parse a sentence with stanford parser.
    /// If you want to get the parse information, you should invoke Parse() function first.
    /// </summary>
    public class DependencyParser
    {
        StanfordCoreNLP pipeline = null;
        ArrayList tokens = null;
        List<Pair<string,string>> posTags = null;
        private SemanticGraph dependencies = new SemanticGraph();

        edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
        edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
        edu.stanford.nlp.semgraph.SemanticGraphCoreAnnotations.BasicDependenciesAnnotation depObj = new edu.stanford.nlp.semgraph.SemanticGraphCoreAnnotations.BasicDependenciesAnnotation();


        internal DependencyParser()
        {
        }

        void Initial()
        {
            var props = new Properties();
            props.put("annotators", "tokenize,ssplit, pos,depparse");
            props.setProperty("tokenizer.whitespace", "true");
            props.setProperty("ssplit.isOneSentence", "true");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory((string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }

        public void Parse(string sentence)
        {
            if(pipeline == null)
            {
                Initial();
            }
            Annotation context = new Annotation(sentence);
            pipeline.annotate(context);
            this.tokens = (ArrayList)context.get(tokenObj.getClass());
            var sentences = (ArrayList)context.get(senObj.getClass());
            foreach (CoreMap sen in sentences)
            {
                this.dependencies = (SemanticGraph)sen.get(depObj.getClass());
                break;
            }
        }

        public void Parse(IEnumerable<string> tokens)
        {
            var sentence = new StringBuilder();
            sentence.Append(tokens.ElementAt(0));
            for (var i = 1; i < tokens.Count(); i++)
            {
                sentence.Append(" "+tokens.ElementAt(i));
            }
            Parse(sentence.ToString());
        }

        public string GetWord(int index)
        {
            return (string)tokens.get(index);
        }

        /// <summary>
        /// Reture the pos tags of words within the parsed sentence.
        /// </summary>
        /// <returns>
        /// A list of word,posTag pairs.
        /// </returns>
        public List<Pair<string, string>> GetPosTags()
        {
            var tags = this.dependencies.vertexListSorted();
            posTags = new List<Pair<string, string>>(tags.size());
            string[] array = new string[2];
            for (var i = 0; i < tags.size(); i++)
            {
                var str = tags.get(i).ToString();
                var index = str.LastIndexOf("/");
                try
                {
                    array[0] = str.Substring(0, index);
                    array[1] = str.Substring(index + 1);
                     posTags.Add(new Pair<string, string>(array[0], array[1]));
                }
                catch (Exception)
                {
                    throw new Exception("Invalid pos tag : " + tags.get(i));
                }
            }
            return posTags;
        }

        /// <summary>
        /// Get the lexical head of mention
        /// <example>
        /// I like the movie. 
        /// For "I" this function will return "like"
        /// </example>
        /// </summary>
        /// <param name="begin"></param>
        /// The index of the first word of mention
        /// <param name="end"></param>
        /// The index of the last word of the mention
        /// <returns>
        ///     Expected Object index or -1 if not exist.
        /// </returns>
        public int GetAction(int begin, int end)
        {
            return GetInterestDep("nsubj", begin, end);
        }

        /// <summary>
        /// Get the adjective modifier of the mention
        /// Example:
        ///        I like this wonderful movie.
        /// For "movie" this function will return the index of "wonderful" begining with 0.
        /// </summary>
        /// <param name="begin"></param>
        /// The index of the first word of mention
        /// <param name="end"></param>
        /// The index of the last word of the mention
        /// <returns>
        ///     Expected Object or "NULL" if not exist.
        /// </returns>
        public int GetAdjModifier(int begin, int end)
        {
            foreach (SemanticGraphEdge dep in this.dependencies.edgeListSorted().toArray())
            {
                if (dep.getRelation().toString().Equals("amod"))
                {
                    if (dep.getGovernor().index() >= begin + 1 && dep.getGovernor().index() <= end + 1)
                    {
                        return dep.getDependent().index() - 1;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Get verb of which the mention is the object
        /// Example:
        ///     I see the movie
        /// for "movie" this function will return the index of "see" begining with 0
        /// </summary>
        /// <param name="begin"></param>
        /// The index of the first word of mention begining with 0
        /// <param name="end"></param>
        /// The index of the last word of the mention begining with 0
        /// <returns>
        ///     Expected Object index or -1 if not exist.
        /// </returns>
        public int GetDriver(int begin, int end)
        {
            return GetInterestDep("dobj", begin, end);
        }

        /// <summary>
        /// Get tokens related to given mention with dependency parser.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<int> GetRelatedToken(int begin, int end)
        {
            var list = new List<int>();
            foreach (SemanticGraphEdge dep in this.dependencies.edgeListSorted().toArray())
            {
                if ((dep.getDependent().index() >= begin + 1 && dep.getDependent().index() <= end + 1) || (dep.getGovernor().index() >= begin + 1 && dep.getGovernor().index() <= end + 1))
                {
                    list.Add(dep.getGovernor().index() - 1);
                }
            }
            return list;
        }

        private int GetInterestDep(string depType, int begin, int end)
        {

            foreach (SemanticGraphEdge dep in this.dependencies.edgeListSorted().toArray())
            {
                if (dep.getRelation().toString().Equals(depType))
                {
                    if (dep.getDependent().index() >= begin + 1 && dep.getDependent().index() <= end + 1)
                    {
                        return dep.getGovernor().index() - 1;
                    }
                }
            }
            return -1;
        }

        public static void Main(string[] args)
        {
            DependencyParser parser = new DependencyParser();
            parser.Parse("I like Beijing.");
            int index = parser.GetDriver(2, 2);
        }
    }

}
