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
            public const string word_table_file = "word_table_file";
            public const string train_data_file = "train_data_file";
            public const string develop_data_file = "develop_data_file";
            public const string test_data_file = "test_data_file";
            public const string train_feature_file = "train_feature_file";
            public const string develop_feature_file = "develop_feature_file";
            public const string test_feature_file = "test_feature_file";
            public const string dic_file = "dic_file";
            public const string dic_type_value_file = "dic_type_value_file";
            public const string name_list_file = "name_list_file";
            public const string preposition_list_file = "preposition_list_file";
            public const string stem_map = "stem_map";
            public const string model_file = "model_file";
            public const string test_result_file = "result_file";
            public const string tagger_model_file = "tagger_model_file";
            public const string sentence_split_model_dir = "sentence_split_model_dir";
            public const string method = "method";
            public const string word_shape_table_file = "shape_table_file";
            public const string posTag_table_file = "posTag_table_file";
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
            string basedir = "../../";
            parameters = new Dictionary<object, object>();
            parameters[Field.word_table_file] = Path.Combine(basedir,@"input\word table\wordTable.txt");
            parameters[Field.train_data_file] = Path.Combine(basedir,@"input\train.txt");
            parameters[Field.develop_data_file] = Path.Combine(basedir,@"input\develop.txt");
            parameters[Field.test_data_file] = Path.Combine(basedir,@"input\test.txt");
            parameters[Field.train_feature_file] = Path.Combine(basedir,@"input\test.txt");
            parameters[Field.develop_feature_file] = Path.Combine(basedir,@"output\developFeature.txt");
            parameters[Field.test_feature_file] = Path.Combine(basedir,@"output\testFeature.txt");
            parameters[Field.dic_file] = Path.Combine(basedir,@"input\dictionary\UIUC.txt");
            parameters[Field.dic_type_value_file] = Path.Combine(basedir,@"inlput\dictionary\UIUC-Type-Value.txt");
            parameters[Field.stem_map] = Path.Combine(basedir,@"input\word stem map\stem-map.txt");
            parameters[Field.method]= Method;
            parameters[Field.model_file] = Path.Combine(basedir,@"output\model\model.txt");
            parameters[Field.test_result_file] = Path.Combine(basedir,@"output\result\result.txt");
            basedir = "../../input";
            parameters[Field.tagger_model_file] = @"E:/Users/v-mipeng/Codes/Java/ASE/nlp_pos/package/pos-tagger/english-left3words/english-left3words-distsim.tagger";
            parameters[Field.sentence_split_model_dir] = basedir;
            parameters[Field.word_shape_table_file] = Path.Combine(basedir,@"shape-table-file.txt");
            parameters[Field.posTag_table_file] = Path.Combine(basedir,@"posTag-table-file.txt");
        }
        private DefaultParameter() { }
    }


}
