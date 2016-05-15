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
            Temp();
            ////Analyze(@"D:\Codes\Project\EntityTyping\Fine-ner\analysis\model.txt",
            ////    @"D:\Codes\Project\EntityTyping\Fine-ner\analysis\weight span.txt");
            //var pipeline = new Pipeline(@"D:\Codes\Project\EntityTyping\release package\config for 5 class liblinear model.xml");
            //var writer = new LargeFileWriter(@"D:\Codes\Project\EntityTyping\Fine-ner\analysis\type2index.txt", FileMode.Create);
            //var dbpediaType2Indexes = DataCenter.GetDBpediaTypes();
            //var pairs = new List<Pair<string, int>>();
            //foreach (var item in dbpediaType2Indexes)
            //{
            //    pairs.Add(new Pair<string, int>(item.Key, item.Value));
            //}
            //pairs.Sort(pairs[0].GetBySecondComparer());
            //foreach (var pair in pairs)
            //{
            //    writer.WriteLine(pair.first);
            //}
            //writer.Close();
        }

        public static void Analyze(string modelFile, string spanFile)
        {
            var writer = new LargeFileWriter(@"D:\Codes\Project\EntityTyping\Fine-ner\analysis\weight analysis.txt", FileMode.Create);
            var reader = new LargeFileReader(spanFile);
            var offsets = new List<int>();
            string line;
            while((line = reader.ReadLine())!=null)
            {
                var offset = int.Parse(line.Split('~')[1]);
                offsets.Add(offset);
            }
            reader.Open(modelFile);
            var parameters = new Dictionary<int, List<double>>();
            var paraStatistic = new Dictionary<int, List<Pair<double, double>>>();
            for(var i = 0;i<15;i++)
            {
                parameters[i] = new List<double>(offsets[offsets.Count-1]+1);
                paraStatistic[i] = new List<Pair<double, double>>();
                for(var j = 0;j < offsets[offsets.Count-1]+1;j++)
                {
                    parameters[i].Add(0);
                }
            }
            reader.Close();

            var regex = new System.Text.RegularExpressions.Regex(@"(\d{1,2})\+f(\d+)\t(.+)");
            var heads = new string[] {"last word surface",
                                    "last word tag",
                                    "last word cluster id",
                                    "last word shape",
                                    "next word surface",
                                    "next word tag",
                                    "next word cluster id",
                                    "next word shape",
                                    "mention head surface",
                                    "mention head tag",
                                    "mention head cluster id",
                                    "mention head shape",
                                    "mention words surfaces",
                                    "mention words tags",
                                    "mention words cluster ids",
                                    "mention words shapes",
                                    "mention cluster id",
                                    "mention length",
                                    "dbpedia types 1",
                                    "dbpedia types 2",
                                    "keywords" };
            while ((line = reader.ReadLine()) != null)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var label = int.Parse(match.Groups[1].Value);
                    var offset = int.Parse(match.Groups[2].Value);
                    var weight = double.Parse(match.Groups[3].Value);
                    parameters[label][offset] = weight;
                }
            }
            int lastOffset = 0;

            for(var i = 0;i<15;i++)
            {
                var weights = parameters[i];
                lastOffset = 0;
                writer.WriteLine("Weights of class " + i+ ":");
                int j = 0;
                foreach(var offset in offsets)
                {
                    var mean = Mean(weights, lastOffset, offset);
                    var variance = Var(weights, lastOffset, offset);
                    paraStatistic[i].Add(new Pair<double, double>(mean, variance));
                    lastOffset = offset + 1;
                    writer.Write(string.Format("{0, -25} : ",heads[j++]));
                    writer.WriteLine(mean + ":" + variance);
                }
                writer.WriteLine("");
            }
            writer.Close();
        }

        public static double Mean(IEnumerable<double> weights, int begin, int end)
        {
            var weightCopy = new List<double>();
            for (var i = begin; i <= end;i++ )
            {
                if (weights.ElementAt(i) != 0)
                {
                    weightCopy.Add(weights.ElementAt(i));
                }
            }
            if(weightCopy.Count == 0)
            {
                return 0;
            }
            return Statistics.HarmonicMean(weightCopy);
        }

        public static double Var(IEnumerable<double> weights, int begin, int end)
        {
            var weightCopy = new List<double>();
            for (var i = begin; i <= end; i++)
            {
                if (weights.ElementAt(i) != 0)
                {
                    weightCopy.Add(weights.ElementAt(i));
                }
            }
            if (weightCopy.Count == 0)
            {
                return 0;
            }
            return Statistics.Variance(weightCopy);
        }

        public static void Temp()
        {
            var regex = new Regex(@"(?<!/)(\w+)/(\w+)(?!/)");
            var regex2 = new Regex(@"(\w+)-(?=\w+)");
            var basedir = @"D:\Codes\Project\EntityTyping\Neural Entity Typing\input\test\";
            var files = Directory.GetFiles(Path.Combine(basedir,"sen"));
            if(!Directory.Exists(Path.Combine(basedir, "hypen")))
            {
                Directory.CreateDirectory(Path.Combine(basedir, "hypen"));
            }
            foreach(var file in files)
            {
                var reader = new LargeFileReader(file);
                var writer = new LargeFileWriter(Path.Combine(basedir, "hypen",Path.GetFileName(file)),FileMode.Create);
                string line;
                while((line = reader.ReadLine())!=null)
                {
                     var str = regex.Replace(line,@"$1 / $2");
                     str = regex2.Replace(str, @"$1 - ");
                     writer.Write(str + "\n");
                }
                reader.Close();
                writer.Close();
            }
        }
    }

}

