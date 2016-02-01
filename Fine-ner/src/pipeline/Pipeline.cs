using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using edu.stanford.nlp.parser.lexparser;
using pml.collection.map;
using pml.type;

namespace msra.nlp.tr
{
    public class Pipeline
    {

        static Feature  featureExtractor = null;

        public Pipeline() 
        {
            Initial(new Property());
        }

        public Pipeline(Property props)
        {
            Initial(props);
        }

        private static void Initial(Property props)
        {
            foreach (var key in DefaultParameter.GetParameterSet())
            {
                GlobalParameter.Set(key, props.GetOrDefault(key, DefaultParameter.Get(key)));
            }
        }

        public void SetProperty(Property props)
        {
            foreach(var key in props.Keys)
            {
                GlobalParameter.Set(key, props.GetOrDefault(key, DefaultParameter.Get(key)));
            }
        }

        /*********************Interactive Interface***********************/

        /// <summary>
        ///    /* Methods:
        ///      /ewt                 : extract word table
        ///      /ef         : extract feature
        ///         -b               :    extract feature for bayes model (default)
        ///         -s                :    extract feature for svm model
        ///         -all             :    extract all data feature (default)
        ///         -train         :    extract train data feature
        ///         -dev           :    extract develop data feature
        ///         -test           :    extract test data feature
        ///     /out
        ///        -dt     : output dictionary type and value
        ///     /tr
        ///        -b      : train extracted feature with Bayes Model (default)
        ///     /ts
        ///       -b      : test extracted features with Bayes Model (default)
        /// </summary>
        public void Execute()
        {
            string operation = null;    // command
            var options = new HashSet<string>();
            var method = (string)GlobalParameter.Get(DefaultParameter.Field.method);
            var array = Regex.Split(method, @"\s+");

            for(var i= 0;i<array.Length;i++)
            {
                if (array[i].StartsWith("/"))
                {
                    //execute one operaton
                    if (operation != null)
                    {
                        Invoke(operation, options);
                    }
                    operation = array[i].Substring(1, array[i].Length - 1);
                    // Encounter invalid operation
                    if (!IsValidOperation(operation))
                    {
                        Console.WriteLine(operation+" is not a valid operation!");
                        // skip invalid operation
                        i++;
                        while (i<array.Length && !array[i].StartsWith("/"))
                        {
                            i++;
                        }
                        i--;
                    }
                }
                else if (array[i].StartsWith("-"))
                {
                    var option = array[i].Substring(1, array[i].Length - 1);
                    // Check if option is valid
                    if (IsValidOption(operation, option))
                    {
                        options.Add(option);
                    }
                    else
                    {
                        Console.Error.WriteLine(option+" is invalid for operation:"+operation);
                    }
                }
            }
            // Invoke the last operation
            if (operation != null)
            {
                Invoke(operation, options);
            }
        }

        static readonly HashSet<string> operations = new HashSet<string>(new string[] {"ewt","ef","out","tr","ts"});
        private static bool IsValidOperation(string operation)
        {
            return operations.Contains(operation);
        }

        static IMap<string, HashSet<string>>  ope2opt = null;

        private static bool IsValidOption(string operation, string option)
        {
            if (!IsValidOperation(operation))
            {
                return false;
            }
            if (ope2opt == null)
            {
                ope2opt = new HashMap<string, HashSet<string>>();
                ope2opt["ewt"] = new HashSet<string>();
                ope2opt["ef"] = new HashSet<string>(new string[] {"raw","bayes","svm","me","add","all","train","test","dev"});
                ope2opt["out"] = new HashSet<string>(new string[] {"dt"});
                ope2opt["tr"] = new HashSet<string>(new string[] {"b"});
                ope2opt["ts"] = new HashSet<string>(new string[] {"b"});
            }
            return ope2opt[operation].Contains(option);
        }

        private void Invoke(string operation, HashSet<string> options)
        {
            switch (operation)
            {
                case "ewt":
                    ExtractWordTable();
                    break;
                case "ef":
                    ExtractFeature(options);
                    break;
                case "out":
                         // TODO: add operation
                    break;
                case "tr":
                    Train(options);
                    break;
                case "ts":
                    Test(options);
                    break;
                default:
                    break;
            }
        }

        #region Extract Feature
        /// <summary>
        ///  Extract feature 
        /// </summary>
        /// <param name="options">
        ///        b               :    extract feature for bayes model (default)
        ///        s                :    extract feature for svm model
        ///        all             :    extract all data feature (default)
        ///        train         :    extract train data feature
        ///        dev           :    extract develop data feature
        ///        test           :    extract test data feature
        /// </param>
        private void ExtractFeature(HashSet<string> options)
        {
            if (options == null)
            {
                // set default options
                options = new HashSet<string>(new string[] {"bayes","all"});
            }
            if (options.Contains("bayes"))
            {
                // extract features for bayes model
                if (options.Contains("train") || options.Contains("all"))
                {
                    ExtractBayesFeature((string)GlobalParameter.Get(DefaultParameter.Field.train_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.train_feature_file));
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                    ExtractBayesFeature((string) GlobalParameter.Get(DefaultParameter.Field.develop_data_file),
                        (string) GlobalParameter.Get(DefaultParameter.Field.develop_feature_file));
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                    ExtractBayesFeature((string)GlobalParameter.Get(DefaultParameter.Field.test_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.test_feature_file));
                }
            }
            else if (options.Contains("svm"))
            {
                // extract features for svm model
                if (options.Contains("train") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.train_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.train_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.develop_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.develop_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.test_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.test_feature_file));
                    extractor.ExtractFeature();
                }
            }
            else if (options.Contains("me"))
            {
                // extract features for svm model
                if (options.Contains("train") || options.Contains("all"))
                {

                    var extractor = new ParallelMaxEntFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.train_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.train_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                    var extractor = new ParallelMaxEntFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.develop_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.develop_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                    var extractor = new ParallelMaxEntFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.test_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.test_feature_file));
                    extractor.ExtractFeature();
                }
            }
            else if (options.Contains("raw"))
            {
                // extract raw features
                if (options.Contains("train") || options.Contains("all"))
                {
                    var extractor = new ParallelIndividualFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.train_data_file),
                       (string)GlobalParameter.Get(DefaultParameter.Field.train_feature_file));
                    if (options.Contains("add"))
                    {
                        extractor.AddFeature();
                    }
                    else
                    {
                        extractor.ExtractFeature();
                    }
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                    var extractor = new IndividualFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.develop_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.develop_feature_file));
                    if (options.Contains("add"))
                    {
                        extractor.AddFeature();
                    }
                    else
                    {
                        extractor.ExtractFeature();
                    }
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                    var extractor = new ParallelIndividualFeatureExtractor((string)GlobalParameter.Get(DefaultParameter.Field.test_data_file),
                        (string)GlobalParameter.Get(DefaultParameter.Field.test_feature_file));
                    if (options.Contains("add"))
                    {
                        extractor.AddFeature();
                    }
                    else
                    {
                        extractor.ExtractFeature();
                    }
                }
            }
        }

        #region Extract Bayes Feature
        /// <summary>
         ///    Extract features for bayes model
         /// </summary>
         /// <param name="source">
         ///    File path storing the data from which this program extract features.
         /// </param>
         /// <param name="des">
         ///    File path to store the extracted features.
         /// </param>
        private static void ExtractBayesFeature(string source, string des)
        {
            FileReader reader = new LargeFileReader(source);
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            var lines = reader.ReadAllLines().ToList();
            const int numPerThread = 10000;
            var threadNum = (int)Math.Ceiling(1.0*lines.Count/ numPerThread);
            var childThreads = new Thread[threadNum];
             var tmpFiles = new string[threadNum];
            for (var i = 0; i < threadNum; i++)
            {
                tmpFiles[i] = "./tmp" + i+".txt";
                var threadClass = new BayesFeatureThread(lines.GetRange(numPerThread*i, Math.Min(numPerThread, lines.Count-numPerThread*i)), tmpFiles[i]);
                childThreads[i] = new Thread(threadClass.ThreadMain);
                childThreads[i].Name = "thread " + i;
                childThreads[i].Start();
            }
            for (var i = 0; i < threadNum; i++)
            {
                childThreads[i].Join();
            }
             foreach (var tmpFile in tmpFiles)
             {
                 var text = File.ReadAllText(tmpFile);
                writer.Write(text);
                File.Delete(tmpFile);
             }
        }

        private class BayesFeatureThread
        {
            readonly IEnumerable<string> lines = null;
            readonly string des = null;

            public BayesFeatureThread(IEnumerable<string> lines, string des)
            {
                this.lines = lines;
                this.des = des;
            }

            public void ThreadMain()
            {
                FileWriter writer = new LargeFileWriter(this.des, FileMode.Create);
                var count = 0;
                foreach (var line in this.lines)
                {
                    if ((++count)%1000 == 0)
                    {
                        Console.WriteLine(Thread.CurrentThread.Name+" has processed "+count);
                    }
                    try
                    {
                        var feature = ExtractBayesFeature(line);
                        writer.Cache(feature.first);
                        var dic = feature.second;
                        foreach (var field in dic.Keys)
                        {
                            writer.Cache("\t" + field + ":{");
                            var values = dic[field];
                            if (values is IEnumerable && !(values is string))
                            {
                                var begin = true;
                                foreach (var value in (IEnumerable)values)
                                {
                                    if (begin)
                                    {
                                        writer.Cache(value);
                                        begin = false;
                                    }
                                    else
                                    {
                                        writer.Cache("," + value);
                                    }
                                }
                                writer.Cache("}");
                            }
                            else
                            {
                                writer.Cache(values + "}");
                            }
                        }
                        writer.Cache("\r");
                        writer.WriteCache();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(line);
                        Console.WriteLine(e.Message);
                        writer.ClearCache();
                    }
                }
                writer.Close();
            }
            }

        private static Pair<string,Dictionary<string,object>>  ExtractBayesFeature(string line)
        {
            if(!(featureExtractor is BayesFeature))
            {
                featureExtractor = new BayesFeature();
            }
            try
            {
               return ((BayesFeature)featureExtractor).GetFeatureWithLabel(line.Split('\t'));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion



        #endregion

        #region Train or Test

        private void Train(HashSet<string> options)
        {
            if (options == null)
            {
                options = new HashSet<string>(new string [] {"b"});
            }
            // train with bayes model
            if (options.Contains("b"))
            {
                var trainer = new BayesModel((string)GlobalParameter.Get(DefaultParameter.Field.train_feature_file),
                   (string)GlobalParameter.Get(DefaultParameter.Field.model_file));
                try
                {
                    trainer.Train();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occur during train for "+e.Message);
                    throw new Exception();
                }
            }
        }

        private void Test(HashSet<string> options)
        {
            if (options == null)
            {
                options = new HashSet<string>(new string[] { "b" });
            }
            // test with bayes model        
            if (options.Contains("b"))
            {
                var tester = new BayesTest((string)GlobalParameter.Get(DefaultParameter.Field.model_file),
                   (string)GlobalParameter.Get(DefaultParameter.Field.develop_feature_file),
                  (string)GlobalParameter.Get(DefaultParameter.Field.test_result_file));
                try
                {
                    tester.Test();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurs during test for "+e.Message);
                    throw new Exception();
                }
            }
        }

        #endregion

        #region Extract Word Table or Word Shape Table
        /* Train file format:
         *      Mention     Type    Context
         * Extract word table and word shape table from train data
         * Every word is converted to lowercase and stemmed
        /************************************************************************/
        public void ExtractWordTable()
        {
            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.train_data_file));
            FileWriter writer = new LargeFileWriter((string)GlobalParameter.Get(DefaultParameter.Field.word_table_file), FileMode.Create);
            FileWriter wordShapeWriter = new LargeFileWriter((string)GlobalParameter.Get(DefaultParameter.Field.word_shape_table_file), FileMode.Create);
            //FileWriter wordShapeWriter = new LargeFileWriter("../../../Fine-ner/input/shape-table-file.txt", FileMode.Create);

            string line = null;
            var wordTable = new HashSet<string>();
            var wordShapeTable = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var array = line.Split('\t');
                    var tokenizer = TokenizerPool.GetTokenizer();
                    var words = tokenizer.Tokenize(array[2]);
                    TokenizerPool.ReturnTokenizer(tokenizer);
                    foreach (var w in words)
                    {

                        if (!string.IsNullOrEmpty(w))   // w should not be empty
                        {
                            var shape = Feature.GetWordShape(w);
                            if (!wordShapeTable.Contains(shape))
                            {
                                wordShapeWriter.WriteLine(shape);
                                wordShapeTable.Add(shape);
                            }
                            var word = Generalizer.Generalize(w);
                            if (!wordTable.Contains(word))
                            {
                                writer.WriteLine(word);
                                wordTable.Add(word);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("=================error!===============");
                    Console.WriteLine("\t"+e.Message);
                    Console.WriteLine("\t"+e.StackTrace);
                    Console.WriteLine("=================error!===============");
                    continue;
                }
                
            }
            reader.Close();
            writer.Close();
        }

        #endregion
        private void OutputDicTypeValue()
        {
            var dic = DataCenter.GetDicTyeMap();
            var writer = new LargeFileWriter((string)GlobalParameter.Get(DefaultParameter.Field.dic_type_value_file), FileMode.OpenOrCreate);

            foreach (var key in dic.Keys)
            {
                if (GlobalParameter.featureNum != 0)
                {
                    writer.WriteLine(key + "\t" + (GlobalParameter.featureNum - DataCenter.GetDicTypeNum() + dic[key]));
                }
                else
                {
                    writer.WriteLine(key + "\t" + dic[key]);
                }
            }
            writer.Close();
        }

        /* Refresh the data, learn daily
         */
        ~Pipeline()
        {
            //DataCenter.RefreshStemDic();
        }

    }
}
