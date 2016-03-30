using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.dcoref;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using java.util;

namespace msra.nlp.tr
{
    public class SentenceSpliter
    {
        StanfordCoreNLP pipeline = null;
        List<string[]> tokensBySentence = null;
        internal SentenceSpliter()
        {
        }

        public  List<string> SplitSequence(string sequence)
        {
            if (pipeline == null)
            {
                Initial();
            }
            if (sequence == null)
            {
                throw new Exception("Sequence should not be null for sentence splitting!");
            }
            var document = new Annotation(sequence);
            pipeline.annotate(document);
            var senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
            var sentences = (ArrayList)document.get(senObj.getClass());
            tokensBySentence = new List<string[]>();
            for (var i = 0; i < sentences.size();i++ )
            {
                var sen = sentences.get(i);

            }
                return (from CoreMap sentence in sentences select sentence.ToString()).ToList();
            
        }

        public List<string> SplitSequence(IEnumerable<string> tokens)
        {
            if(tokens == null)
            {
                throw new Exception("Tokens should not be null for sentence splitting!");
            }
            var sequence = new StringBuilder();
            sequence.Append(tokens.ElementAt(0));
            for(var i =1;i<tokens.Count();i++)
            {
                sequence.Append(" "+tokens.ElementAt(i));
            }
            return SplitSequence(sequence.ToString());
        }


        void Initial(string modelDir = null)
        {
            var props = new Properties();
            props.put("annotators", "tokenize,ssplit");
            props.put("tokenizer.whitespace", "true");

            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(modelDir ?? (string)Parameter.GetParameter(Parameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }



    }
}
