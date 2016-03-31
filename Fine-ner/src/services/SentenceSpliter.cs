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
using pml.type;

namespace msra.nlp.tr
{
    public class SentenceSpliter
    {
        StanfordCoreNLP pipeline = null;
        List<string[]> tokensBySentence = null;
        static edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
        static edu.stanford.nlp.ling.CoreAnnotations.CharacterOffsetBeginAnnotation indexObj = new edu.stanford.nlp.ling.CoreAnnotations.CharacterOffsetBeginAnnotation();

        internal SentenceSpliter()
        {
        }

        public  List<Pair<string,int>> SplitSequence(string sequence)
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
            var sentences = (ArrayList)document.get(senObj.getClass());
            tokensBySentence = new List<string[]>();
            var sens = new List<Pair<string, int>>();
            for (var i = 0; i < sentences.size();i++ )
            {
                var sen = (edu.stanford.nlp.pipeline.Annotation)sentences.get(i);
                var index = sen.get(indexObj.getClass());
                sens.Add(new Pair<string, int>(sen.toString(),int.Parse(index.ToString())));
            }
            return sens;
        }

        public List<Pair<string, int>> SplitSequence(IEnumerable<string> tokens)
        {
            if(tokens == null)
            {
                throw new Exception("Tokens should not be null for sentence splitting!");
            }
            return SplitSequence(string.Join(" ", tokens));
        }


        void Initial(string modelDir = null)
        {
            var props = new Properties();
            props.put("annotators", "tokenize,ssplit");
            //props.put("tokenizer.whitespace", "true");

            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(modelDir ?? (string)Parameter.GetParameter(Parameter.Field.stanford_model_dir));
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }



    }
}
