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
            public const string word_shape_table_file       = "word_shape_table_file";
            public const string posTag_table_file           = "posTag_table_file";
            public const string word_id_file                = "word_id_file";
            public const string mention_id_file             = "mention_id_file";
            public const string opennlp_model_dir           = "opennlp_model_dir";
            public const string dbpedia_type_file            = "dbpedia_type_file";
            public const string dbpedia_redirect_file       = "dbpedia_redirect_file";
            public const string dbpedia_abstract_file       = "dbpedia_abstract_file";
            public const string dbpedia_abstract_df_file    = "dbpedia_abstract_df_file";
            public const string dbpedia_abstract_word_table = "dbpedia_abstract_word_table";
            public const string dbpedia_abstract_num        = "dbpedia_abstract_num";
            public const string page_indegree_file          = "page_indegree_file";
            public const string keyword_file                = "keyword_file";
            public const string disambiguous_file           = "disambiguous_file";
            public const string page_anchor_file            = "page_anchor_file";
            public const string activateParser              = "activateParser";
            public const string activateNer                 = "activateNer";
            public const string activateDbpedia             = "activateDbpedia";
            public const string activateMIKeyword           = "activateMIKeyword";
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

        }

        private DefaultParameter() { }
    }


}
