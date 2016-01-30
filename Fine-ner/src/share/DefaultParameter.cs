using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace msra.nlp.tr
{
    class DefaultParameter
    {
        public struct Field
        {
            public const string word_table_file             = "word_table_file";
            public const string train_data_file             = "train_data_file";
            public const string develop_data_file           = "develop_data_file";
            public const string test_data_file              = "test_data_file";
            public const string train_feature_file          = "train_feature_file";
            public const string develop_feature_file        = "develop_feature_file";
            public const string test_feature_file           = "test_feature_file";
            public const string dic_file                    = "dic_file";
            public const string dic_type_value_file         = "dic_type_value_file";
            public const string name_list_file              = "name_list_file";
            public const string preposition_list_file       = "preposition_list_file";
            public const string stem_map                    = "stem_map";
            public const string model_file                  = "model_file";
            public const string test_result_file            = "result_file";
            public const string stanford_model_dir          = "stanford_model_dir";
            public const string method                      = "method";
            public const string word_shape_table_file       = "shape_table_file";
            public const string posTag_table_file           = "posTag_table_file";
            public const string word_id_file                = "word_id_file";
            public const string mention_id_file             = "mention_id_file";
            public const string opennlp_model_dir          = "opennlp_model_dir";
            public const string dbpedia_dic_file            = "dbpedia_dic_file";
        };

       
         /* Methods:
          *       /ewt                 : extract word table
          *      /ef         : extract feature
          *          -all              :    extract all data feature
          *          -train           :    extract train data feature
          *          -dev             :    extract develop data feature
          *          -test              :    extract test data feature
          *     /out
          *          -dt     : output dictionary type and value
          *     /tr
          *          -b      : train extracted feature with Bayes Model
          *     / ts
          *          -b      : test extracted features with Bayes Model
          */
         const string Method = "/ef -all /ewt /out -dt";

        static Dictionary<object, object> parameters = null;

        internal static Object Get(Object key)
        {
            if (parameters == null)
            {
                Initial();
            }
            Object value;
            return parameters.TryGetValue(key, out value) ? value : null;
        }

        internal static Dictionary<object, object>.KeyCollection GetParameterSet()
        {
            if (parameters == null)
            {
                Initial();
            }
            return parameters.Keys;
        }


        static void Initial()
        {
            parameters = new Dictionary<object, object>();
            var currentFolderPath = Environment.CurrentDirectory;
            var projectFolderPath = currentFolderPath.Substring(0, currentFolderPath.IndexOf("bin"));
            var basedir = new DirectoryInfo(projectFolderPath).Parent.FullName;
            basedir = Path.Combine(basedir,"Fine-ner");
            parameters[Field.stanford_model_dir]        = Path.Combine(basedir, @"input\stanford models");
            parameters[Field.opennlp_model_dir]         = Path.Combine(basedir, @"input\opennlp models");
            parameters[Field.method]                    = Method;
            parameters[Field.train_data_file]           = Path.Combine(basedir, @"input\satori\train.txt");
            parameters[Field.develop_data_file]         = Path.Combine(basedir, @"input\satori\develop.txt");
            parameters[Field.test_data_file]            = Path.Combine(basedir, @"input\satori\test.txt");
            parameters[Field.dic_file]                  = Path.Combine(basedir, @"input\dictionary\UIUC.txt");
            parameters[Field.dic_type_value_file]       = Path.Combine(basedir, @"input\dictionary\UIUC-Type-Value.txt");
            parameters[Field.word_table_file]           = Path.Combine(basedir, @"input\tables\wordTable.txt");
            parameters[Field.stem_map]                  = Path.Combine(basedir, @"input\tables\stem-word-table.txt");
            parameters[Field.word_shape_table_file]     = Path.Combine(basedir, @"input\tables\shape-table.txt");
            parameters[Field.posTag_table_file]         = Path.Combine(basedir, @"input\tables\pos-tag-table.txt");
            parameters[Field.word_id_file]              = Path.Combine(basedir, @"input\tables\wordID.txt");
            parameters[Field.mention_id_file]           = Path.Combine(basedir, @"input\tables\mentionID.txt");
            parameters[Field.train_feature_file]        = Path.Combine(basedir, @"output\train\trainFeature.txt");
            parameters[Field.develop_feature_file]      = Path.Combine(basedir, @"output\satori\developFeature.txt");
            parameters[Field.test_feature_file]         = Path.Combine(basedir, @"output\satori\testFeature.txt");
            parameters[Field.model_file]                = Path.Combine(basedir, @"output\model\model.txt");
            parameters[Field.test_result_file]          = Path.Combine(basedir, @"output\result\result.txt");
            parameters[Field.dbpedia_dic_file]          = Path.Combine(basedir, @"input\dictionaries\dbpedia entity type.txt");
        }
        private DefaultParameter() { }
    }


}
