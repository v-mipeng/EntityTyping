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
        public SentenceSpliter()
        {
        }

        public  List<string> SplitSequence(string sequence)
        {
            if (pipeline == null)
            {
                Initial();
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
            Directory.SetCurrentDirectory(modelDir ?? (string)GlobalParameter.Get(DefaultParameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }



    }
}
