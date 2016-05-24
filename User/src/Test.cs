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
using pml.type;
using msra.nlp.tr;
using MathNet.Numerics.Statistics;
using System.Text.RegularExpressions;
using msra.nlp.tr.eval;

namespace User
{
    ////public class Tokenizer
    //{
    //    internal Tokenizer(string modelDir)
    //    {
    //        Initial(modelDir);
    //    }

    //    StanfordCoreNLP pipeline = null;
    //    edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();

    //    public List<string> Tokenize(string sequence)
    //    {
    //        if (sequence == null)
    //        {
    //            throw new Exception("Sequence should not be null for tokenizer.");
    //        }
    //        var document = new Annotation(sequence);
    //        pipeline.annotate(document);

    //        var tokens = (ArrayList)document.get(tokenObj.getClass());
    //        var indexObj = new edu.stanford.nlp.ling.CoreAnnotations.CharacterOffsetBeginAnnotation();

    //        foreach (edu.stanford.nlp.ling.CoreLabel token in tokens)
    //        {
    //            Console.WriteLine(token.GetType());
    //            Console.WriteLine(token.get(indexObj.getClass()));
    //        }
    //        return (from CoreMap token in tokens select token.ToString()).ToList();
    //    }

    //    private void Initial(string modelDir)
    //    {
    //        var props = new Properties();
    //        props.put("annotators", "tokenize");
    //        //props.put("invertible", "true");
    //        props.setProperty("ner.useSUTime", "false");
    //        var dir = Directory.GetCurrentDirectory();
    //        Directory.SetCurrentDirectory(modelDir);
    //        pipeline = new StanfordCoreNLP(props);
    //        Directory.SetCurrentDirectory(dir);
    //    }

    //}

    public class SentenceSpliter
    {

        public static void Main(string[] args)
        {
            ExtractEmb();
        }
        public static void Evaluate()
        {
            var evaluator = new ClassByClassEvaluator();
            //evaluator.EvaluateResult(@"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\multi_time_lstm test on satori.txt",
            //    @"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\satori result.txt");
            evaluator.EvaluateResult(@"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\multi_time_lstm test on satori conll and bbn.txt",
    @"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\satori conll and bbn result.txt");
   //         evaluator.EvaluateResult(@"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\multi_time_lstm test on conll.txt",
   //@"D:\Codes\Project\EntityTyping\Neural Entity Typing\output\result\train on satori and bbn\conll result.txt");
        }

        public static void ExtractEmb()
        {
            var source = @"D:\Codes\Project\EntityTyping\Neural Entity Typing\input\tables\satori and bbn\word2id.txt";
            var reader = new LargeFileReader(source);
            var words = new HashSet<string>();
            string line;
            while((line = reader.ReadLine())!=null)
            {
                words.Add(line.Split('\t')[0]);
            }
            reader.Close();
            var writer = new LargeFileWriter(@"D:\Codes\Project\EntityTyping\Neural Entity Typing\input\tables\word embedding.txt", FileMode.Create);
            reader.Open(@"D:\Data\Google-word2vec\GoogleNews-vectors-negative300.txt");
            int count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if(++count%10000==0)
                {
                    Console.WriteLine(count);
                }
                var word = line.Split(new char[]{' '}, 2)[0];
                if(words.Contains(word))
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
        }
    }



}

