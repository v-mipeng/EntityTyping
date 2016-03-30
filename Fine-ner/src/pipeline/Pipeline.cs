﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Xml;
using edu.stanford.nlp.parser.lexparser;
using pml.collection.map;
using pml.type;
using pml.file.reader;
using pml.file.writer;
using msra.nlp.tr.predict;
using msra.nlp.tr.eval;

namespace msra.nlp.tr
{
    public class Pipeline
    {

        public Pipeline() 
        {
        }

        public Pipeline(Property props)
        {
            Parameter.SetParameters(props);
        }

        public Pipeline(string configFile, Property props = null)
        {
            Parameter.SetParameters(configFile, props);
        }

        public void SetProperty(object parameter, object value)
        {
            Parameter.SetParameter(parameter, value);
        }

        public void SetProperty(Property props)
        {
            Parameter.SetParameters(props);
        }


        #region Command Line Operations
        /*********************Interactive Interfac***********************/

        /// <summary>
        ///    /* Methods:
        ///      /ewt                       : extract word table
        ///      /ef                        : extract feature
        ///         -raw                    : extract raw features of queries     
        ///         -svm                    : extract vector features for model training
        ///         -all                    : extract featurs for training, develop and test data
        ///         -train                  : extract train data feature
        ///         -dev                    : extract develop data feature
        ///         -test                   : extract test data feature
        /// </summary>
        public void Execute()
        {
            string operation = null;    // command
            var options = new HashSet<string>();
            var method = (string)Parameter.GetParameter(Parameter.Field.method);
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

        static readonly HashSet<string> operations = new HashSet<string>(new string[] {"ewt","ef","out","tr","ts"});  //!! I should encode these information into config file
        private static bool IsValidOperation(string operation)
        {
            return operations.Contains(operation);
        }

        static IMap<string, HashSet<string>>  ope2opt = null;   //!! This is a global parameter, the information should be written into config file and intiated when the object created.

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
                options = new HashSet<string>(new string[] {"svm","all"});
            }
            if (options.Contains("bayes"))
            {
                // extract features for bayes model
                if (options.Contains("train") || options.Contains("all"))
                {
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                }
            }
            else if (options.Contains("svm"))
            {
                // extract features for svm model
                if (options.Contains("train") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.train_data_file),
                        (string)Parameter.GetParameter(Parameter.Field.train_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.develop_data_file),
                        (string)Parameter.GetParameter(Parameter.Field.develop_feature_file));
                    extractor.ExtractFeature();
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                    var extractor = new ParallelSVMFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.test_data_file),
                        (string)Parameter.GetParameter(Parameter.Field.test_feature_file));
                    extractor.ExtractFeature();
                }
            }
            else if (options.Contains("me"))
            {
                // extract features for svm model
                if (options.Contains("train") || options.Contains("all"))
                {

                }
                if (options.Contains("dev") || options.Contains("all"))
                {
                }
                if (options.Contains("test") || options.Contains("all"))
                {
                }
            }
            else if (options.Contains("raw"))
            {
                // extract raw features
                if (options.Contains("train") || options.Contains("all"))
                {
                    var extractor = new IndividualFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.train_data_file),
                       (string)Parameter.GetParameter(Parameter.Field.train_feature_file));
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
                    var extractor = new ParallelIndividualFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.develop_data_file),
                        (string)Parameter.GetParameter(Parameter.Field.develop_feature_file));
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
                    var extractor = new ParallelIndividualFeatureExtractor((string)Parameter.GetParameter(Parameter.Field.test_data_file),
                        (string)Parameter.GetParameter(Parameter.Field.test_feature_file));
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


        #endregion

        #region Train or Test: TODO

        private void Train(HashSet<string> options)
        {
        }

        private void Test(HashSet<string> options)
        {
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
            FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.train_data_file));
            FileWriter writer = new LargeFileWriter((string)Parameter.GetParameter(Parameter.Field.word_table_file), FileMode.Create);
            FileWriter wordShapeWriter = new LargeFileWriter((string)Parameter.GetParameter(Parameter.Field.word_shape_table_file), FileMode.Create);
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

                        if (!string.IsNullOrEmpty(w.first))   // w should not be empty
                        {
                            var shape = Feature.GetWordShape(w.first);
                            if (!wordShapeTable.Contains(shape))
                            {
                                wordShapeWriter.WriteLine(shape);
                                wordShapeTable.Add(shape);
                            }
                            var word = Generalizer.Generalize(w.first);
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

        #endregion


        /// <summary>
        ///Refresh the data, learn daily
        /// </summary>
        ~Pipeline()
        {
            try
            {
                DataCenter.RefreshStemDic();
            }
            catch(Exception)
            {

            }
        }

    }
}
