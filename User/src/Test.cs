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
using pml.collection.map;
using pml.file.reader;
using pml.file.writer;

namespace User
{
    public class Tokenizer
    {
        internal Tokenizer(string modelDir)
        {
            Initial(modelDir);
        }

        StanfordCoreNLP pipeline = null;
        edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();

        public List<string> Tokenize(string sequence)
        {
            if (sequence == null)
            {
                throw new Exception("Sequence should not be null for tokenizer.");
            }
            var document = new Annotation(sequence);
            pipeline.annotate(document);

            var tokens = (ArrayList)document.get(tokenObj.getClass());
            var indexObj = new edu.stanford.nlp.ling.CoreAnnotations.CharacterOffsetBeginAnnotation();

            foreach (edu.stanford.nlp.ling.CoreLabel token in tokens)
            {
                Console.WriteLine(token.GetType());
                Console.WriteLine(token.get(indexObj.getClass()));
            }
            return (from CoreMap token in tokens select token.ToString()).ToList();
        }

        private void Initial(string modelDir)
        {
            var props = new Properties();
            props.put("annotators", "tokenize");
            //props.put("invertible", "true");
            props.setProperty("ner.useSUTime", "false");
            var dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(modelDir);
            pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(dir);
        }

    }
}
