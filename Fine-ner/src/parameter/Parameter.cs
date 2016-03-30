﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using pml.type;
using System.IO;
using pml.file.reader;

namespace msra.nlp.tr
{

    /// <summary>
    /// The parameters within this class can be assigned by the user
    /// however, they are not rewritable once assigned.
    /// This can also be initiated by config file.
    /// </summary>
    class Parameter
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
            public const string four_classes_model_file = "four_classes_model_file";
            public const string test_result_file = "result_file";
            public const string stanford_model_dir = "stanford_model_dir";
            public const string method = "method";
            public const string word_shape_table_file = "word_shape_table_file";
            public const string posTag_table_file = "posTag_table_file";
            public const string word_id_file = "word_id_file";
            public const string mention_id_file = "mention_id_file";
            public const string opennlp_model_dir = "opennlp_model_dir";
            public const string dbpedia_type_file = "dbpedia_type_file";
            public const string dbpedia_redirect_file = "dbpedia_redirect_file";
            public const string dbpedia_abstract_file = "dbpedia_abstract_file";
            public const string dbpedia_abstract_df_file = "dbpedia_abstract_df_file";
            public const string dbpedia_abstract_word_table = "dbpedia_abstract_word_table";
            public const string dbpedia_abstract_num = "dbpedia_abstract_num";
            public const string page_indegree_file = "page_indegree_file";
            public const string keyword_file = "keyword_file";
            public const string disambiguous_file = "disambiguous_file";
            public const string page_anchor_file = "page_anchor_file";
            public const string activateParser = "activateParser";
            public const string activateNer = "activateNer";
            public const string activateDbpedia = "activateDbpedia";
            public const string activateMIKeyword = "activateMIKeyword";
        };

        private static Dictionary<object, object> parameters = new Dictionary<object, object>();

        private static Dictionary<int, string> label2Type = new Dictionary<int, string>();

        private static Dictionary<string, int> type2Label = new Dictionary<string, int>();

        /// <summary>
        /// Get parameter with given key
        /// </summary>
        /// <param name="key">
        /// Parameter name
        /// </param>
        /// <returns>
        /// An object with the value of give parameter name
        /// </returns>
        internal static Object GetParameter(Object key)
        {
            Object value;
            return parameters.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        /// Get integer label of type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// An integer label corresponding to the given type
        /// </returns>
        internal static int GetTypeLabel(string type)
        {
            try
            {
                return type2Label[type];
            }
            catch (Exception)
            {
                throw new Exception("There is no given type:" + type);
            }
        }

        /// <summary>
        /// Get string type corresponding to the given label
        /// </summary>
        /// <param name="label">
        /// An integer value
        /// </param>
        /// <returns></returns>
        internal static string GetTypeByLabel(int label)
        {
            try
            {
                return label2Type[label];
            }
            catch (Exception)
            {
                throw new Exception("No type corresponds to given label!");
            }
        }

        internal static void SetParameter(Object key, Object value)
        {
            parameters[key] = value;
        }

        internal static void SetTypeLabel(string type, int label)
        {
            type2Label[type] = label;
            label2Type[label] = type;
        }

        internal static void SetParameters(string configFile, Property props = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);
            var nodes = doc.DocumentElement.SelectNodes("/config/parameters/parameter");
            string basedir = null;
            // Set Parameters
            foreach (XmlNode node in nodes)
            {
                var name = node.Attributes["name"].Value;
                var type = node.Attributes["type"].Value;
                var value = node.Attributes["value"].Value;
                if(type.Equals("base path"))
                {
                    basedir = value; 
                }
                else if(type.Equals("relative path"))
                {
                    value = Path.Combine(basedir,value);
                    parameters[name] = value;
                }
                else if(type.Equals("int"))
                {
                    parameters[name] = int.Parse(value);
                }
                else if (type.Equals("float"))
                {
                    parameters[name] = float.Parse(value);
                }
                else if (type.Equals("double"))
                {
                    parameters[name] = double.Parse(value);
                }
                else
                {
                    parameters[name] = value;
                }
            }
            // Set Type Maps
            nodes = doc.DocumentElement.SelectNodes("/config/types/map");
            foreach(XmlNode node in nodes)
            {
                var type = node.Attributes["type"].Value;
                var label = int.Parse(node.Attributes["label"].Value);
                type2Label[type] = label;
                label2Type[label] = type;
            }
            if (props != null)
            {
                SetParameters(props);
            }
        }

        internal static void SetParameters(Property props)
        {
            foreach (var key in props.Keys)
            {
                parameters[key] = props[key];
            }
        }

        private Parameter() { }
    }
}