using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public DependencyParser() 
        {
            Initial();
        }

        #region ParserPool

        static List<LexicalizedParser> parsers = new List<LexicalizedParser>();
        HashSet<int> availableParsers = new HashSet<int>();
        int maxParserNum = 20;


        public LexicalizedParser GetParser()
        {
            if (availableParsers.Count > 0)
            {
                return parsers[availableParsers.First()];
            }
            else if (parsers.Count < maxParserNum)
            {
                var parser = LexicalizedParser.loadModel(Path.Combine((string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir), @"edu\stanford\nlp\models\lexparser\englishPCFG.ser.gz"));
                parsers.Add(parser);
                availableParsers.Add(parsers.Count - 1);
                return parser;
            }
            else
            {
                return null; // the thread should sleep and check if new parser available
            }
        }

        public void ReturnParser(LexicalizedParser parser)
        {
            for (var i = 0; i < parsers.Count; i++)
            {
                if (parser == parsers[i])
                {
                    availableParsers.Add(i);
                    break;
                }
            }
        }
        #endregion

        private List<string> dependencies = new List<string>();

        private  LexicalizedParser parser = null;

        void Initial()
        {
            parser = LexicalizedParser.loadModel(Path.Combine((string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir), @"edu\stanford\nlp\models\lexparser\englishPCFG.ser.gz"));
        }

        /*Stem the given word with, return the stemmed word
         */
        public  void Parse(string sentence)
        {
            var tokenizer = TokenizerPool.GetTokenizer();
            var tokens = tokenizer.Tokenize(sentence);
            TokenizerPool.ReturnTokenizer(tokenizer);
            Parse(tokens);
        }

        public void Parse(IEnumerable<string> tokens)
        {
                if (parser == null)
                {
                    Initial();
                }
                var rawWords = Sentence.toCoreLabelList(tokens.ToArray());
                var tree = parser.apply(rawWords);
                var tlp = new PennTreebankLanguagePack();
                var gsf = tlp.grammaticalStructureFactory();
                var gs = gsf.newGrammaticalStructure(tree);
                var tdl = gs.typedDependenciesCCprocessed();
                dependencies.Clear();
                foreach (TypedDependency item in (ArrayList)tdl)
                {
                    // From item you can parse gov,reln,dep and token tags.
                    // You can view from debug 
                    dependencies.Add(item.ToString());
            }
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
        public  int GetAction(int begin, int end)
        {
            return GetInterestDep("nsubj", begin, end);
        }
        /// <summary>
        /// Get the adjective modifier of the mention
        /// Example:
        ///        I like this wonderful movie.
        /// For "movie" this function will return "wonderful".
        /// </summary>
        /// <param name="begin"></param>
        /// The index of the first word of mention
        /// <param name="end"></param>
        /// The index of the last word of the mention
        /// <returns>
        ///     Expected Object or "NULL" if not exist.
        /// </returns>
        public  int GetAdjModifier(int begin, int end)
        {
            foreach (var dep in dependencies)
            {
                if (dep.StartsWith("amod"))
                {
                    var tuple = ParseDep(dep);
                    if (tuple.GovIndex >= begin + 1 && tuple.GovIndex <= end + 1)
                    {
                        return tuple.DepIndex;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Get verb of which the mention is the object
        /// Example:
        ///     I see the movie
        /// for "movie" this function will return "see"
        /// </summary>
        /// <param name="begin"></param>
        /// The index of the first word of mention
        /// <param name="end"></param>
        /// The index of the last word of the mention
        /// <returns>
        ///     Expected Object index or -1 if not exist.
        /// </returns>
        public  int GetDriver(int begin, int end)
        {
            return GetInterestDep("dobj", begin, end);
        }

        private  int GetInterestDep(string depType,int begin,int end)
        {
            foreach (var dep in dependencies)
            {
                if (dep.StartsWith(depType))
                {
                    var tuple = ParseDep(dep);
                    if (tuple.DepIndex >= begin + 1 && tuple.DepIndex <= end + 1)
                    {
                        return tuple.GovIndex;
                    }
                }
            }
            return -1;
        }

        private  Regex regex = new Regex(@"([^\(]*)\(([^-]*)-(\d*),([^-]*)-(\d*)\)"); // obj(obama-2, pick-4)
        private  Tuple ParseDep(string dep)
        {
            Tuple tuple = new Tuple();
            Match match = regex.Match(dep);
            if(match.Success)
            {
                tuple.DepType = match.Groups[1].Value;
                tuple.Gov = match.Groups[2].Value;
                tuple.GovIndex = int.Parse(match.Groups[3].Value);
                tuple.Dep = match.Groups[4].Value;
                tuple.DepIndex = int.Parse(match.Groups[5].Value);
            }
            return tuple;
        }

        class Tuple
        {
            string depenType;
            string gov;
            int govIndex;
            string dependent;
            int depIndex;

            public Tuple() { }

            public Tuple(string depenType, string gov, string dependent, int govIndex, int depIndex)
            {
                this.depenType = depenType;
                this.gov = gov;
                this.dependent = dependent;
                this.govIndex = govIndex;
                this.depIndex = depIndex;
            }


            public string DepType
            {
                get
                {
                    return depenType;
                }
                set
                {
                    depenType = value;
                }
            }

            public string Gov
            {
                get
                {
                    return gov;
                }
                set
                {
                    gov = value;
                }
            }

            public int GovIndex
            {
                get
                {
                    return govIndex;
                }
                set
                {
                    govIndex = value;
                }
            }
            public string Dep
            {
                get
                {
                    return dependent;
                }
                set
                {
                    dependent = value;
                }
            }
            public int DepIndex
            {
                get
                {
                    return depIndex;
                }
                set
                {
                    depIndex = value;
                }
            }
        }
        
        public static void Main(string[] args)
        {
        }
    }

}
