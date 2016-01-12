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
using Accord.MachineLearning;
using User.src;

namespace msra.nlp.tr
{
    public class SentenceSplit
    {
         
        //static StanfordCoreNLP pipeline = null;

        //public static List<string> SplitSequence(string sequence)
        //{
        //    if (pipeline == null)
        //    {
        //        Initial();
        //    }
        //    var document = new Annotation(sequence);
        //    pipeline.annotate(document);
        //    var senObj = new edu.stanford.nlp.ling.CoreAnnotations.SentencesAnnotation();
        //    var sentences = (ArrayList)document.get(senObj.getClass());
        //    return (from CoreMap sentence in sentences select sentence.ToString()).ToList();
        //}

        //public static List<string> Tokenize(string sequence)
        //{
        //    if (pipeline == null)
        //    {
        //        Initial();
        //    }
        //    var document = new Annotation(sequence);
        //    pipeline.annotate(document);
        //    var tokenObj = new edu.stanford.nlp.ling.CoreAnnotations.TokensAnnotation();
        //    var tokens = (ArrayList)document.get(tokenObj.getClass());
        //    return (from CoreMap token in tokens select token.ToString()).ToList();
        //}

        public static void Main(string[] args)
        {
           // //Console.WriteLine(SplitSequence("I like Beijing. I went there yesterday!"));
           //var tokens = Tokenize("I like china, which standing at southeast.");

           //var output = Generalizer.Generalize("1092-2322");
           //output = Generalizer.Generalize("Chinese2012");
           //Console.WriteLine(output);
           //Console.Read();
            var cluster = new VectorCluster(@"D:\Data\Google-word2vec\tmp.txt",//GoogleNews-vectors-negative300.txt",
                @"D:\Codes\C#\EntityTyping\Fine-ner\input\word table\centroids.txt",
                @"D:\Codes\C#\EntityTyping\Fine-ner\input\word table\wordID.txt");
            cluster.Cluster(100);
        }
        //static void Initial(string modelDir = null)
        //{
        //    var props = new Properties();
        //    props.put("annotators", "tokenize");
        //    var dir = Directory.GetCurrentDirectory();
        //    Directory.SetCurrentDirectory(@"D:\Software Install\CoreNLP");
        //    pipeline = new StanfordCoreNLP(props);
        //    Directory.SetCurrentDirectory(dir);
        //}

        ////static void Temp()
        ////{
        ////    FileReader reader = new LargeFileReader(@"E:\Users\v-mipeng\Temp\temp\entity.txt");
        ////    string line;
        ////    var dic = new Dictionary<string, int>();
        ////    var keys = new List<string>();

        ////    while ((line = reader.ReadLine()) != null)
        ////    {
        ////        var array = line.Split('\t');
        ////        keys.Add(array[0]);
        ////    }
        ////    reader.Open(@"E:\Users\v-mipeng\Temp\temp\count-num-by-mention.txt");
        ////    while ((line = reader.ReadLine()) != null)
        ////    {
        ////        var array = line.Split('\t');
        ////        dic[array[0]] = int.Parse(array[1]);
        ////    }
        ////    reader.Close();
        ////    var writer = new LargeFileWriter(@"E:\Users\v-mipeng\Temp\temp\count-num-by-mention.txt");

        ////    foreach (var key in keys)
        ////    {
        ////        writer.Write(key);
        ////        try
        ////        {
        ////            var value = dic[key];
        ////            writer.WriteLine("\t" + value);
        ////        }
        ////        catch (Exception)
        ////        {
        ////            writer.WriteLine("\t"+0);
        ////        }
        ////    }
        ////    writer.Close();
        ////}

        //public static void Mains(string[] args)
        //{
        //    //Console.WriteLine(SplitSequence("I like Beijing. I went there yesterday!"));
        //   var tokens = Tokenize("I like china, which standing at southeast.");

        //   var output = Generalizer.Generalize("1092-2322");
        //   output = Generalizer.Generalize("Chinese2012");
        //   Console.WriteLine(output);
        //   Console.Read();
        //}
>>>>>>> ef23a18181bc4b31a41c0e67e97319757d6e0d34
    }
}
