using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace msra.nlp.tr.HierarchyExtractor
{
    class DefaultParameter
    {

        /* Methods:
        *       /e                 : extract hierarchy from xml file
        *          -i               :set interested types
         *         -ui             :set uninterested types
         *         -l               :add links  
         *         -dl             :delete links
         *         -oi              :output internal file    
         *      /s                  : statistic hierarchy information   
         *         -r              : extract information from raw satori data(this will save internal data auto)
         *         -c              :  extract organized type number from exist count mention and entity number information.
         *         -oh            : output hierarchy with mention and entity number  
         *        -we1            : extract wikipedia top 1 type number by entity
         *       -we3            : extract wikipedia top 3 type number by entity
         *          -mp             :extract types with multiple top level parent type
        */
        const String Method = "/e -i -ui -l /s -oh";

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

         internal static Dictionary<object, object>.KeyCollection GetParameterSet()
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
             string basedir = "../../Hierarchy Info/";
             // make a directory for output files
             if (!Directory.Exists(basedir))
             {
                 Directory.CreateDirectory(basedir);
             }
             parameters["satori_raw_data_file"] = @"E:\Users\v-mipeng\Data\Satori\Raw\Interlink.stype.tsv";
             parameters["satori_trim_data_file"] = @"E:\Users\v-mipeng\Data\Satori\Raw\satori-trimed.tsv";
             // hierarchy extraction parameters
             parameters["satori_type_xml_file"] = @"E:\Users\v-mipeng\Data\Satori\Type Info\satori-ontology.all.xml";
             parameters["hierarchy_file"] = Path.Combine(basedir, "extracted-hierarchy.txt");
             parameters["internal_directory"] = Path.Combine(basedir, "internal");
             parameters["interest_type_file"] = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\filter\interestTypes.txt";
             parameters["uninterest_type_file"] = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\filter\uninterestTypes.txt";
             parameters["link_file"] = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\filter\links.txt" ;
             parameters["delink_file"] = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\filter\delinks.txt";
             // statistic information parameters
             parameters["count_mention_num_by_type_file"] = Path.Combine(basedir, "count-mention-num-by-type.txt");
             parameters["count_entity_num_by_type_file"] = Path.Combine(basedir, "count-entity-num-by-type.txt");
             parameters["hierarchy_with_mention_num_file"] = Path.Combine(basedir, "hierarchy-with-mention-num.txt");
             parameters["hierarchy_with_entity_num_file"] = Path.Combine(basedir, "hierarchy-with-entity-num.txt");
             parameters["organized_num_by_type"] = Path.Combine(basedir, "organized-num-by-type.txt");
             parameters["method"] = Method;
            parameters["wiki_entity_type_file"] = @"E:\Users\v-mipeng\Data\Satori\Raw\WikiType.tsv";
            parameters["top1_type_num_by_wiki_entity"] = Path.Combine(basedir, "top1-type-num-by-wiki-entity.txt");
            parameters["top3_type_num_by_wiki_entity"] = Path.Combine(basedir, "top3-type-num-by-wiki-entity.txt");
            parameters["type_with_multi_parents_file"] = Path.Combine(basedir, "type-with-multi-parents.txt");

        }
        //public const string SATORI_RAW_FILE = @"E:\Users\v-mipeng\Data\Satori\Raw\tmp.txt";//Interlink.stype.tsv";
        //public const string SATORI_TRIM_FILE = @"E:\Users\v-mipeng\Data\Satori\Raw\Satori_trimed.tsv";
        //public const string STATISTIC_RESULT_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\NumByType.txt";
        //public const string PAIRWISE_HIERARCHY_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\internal\pairwiseHierachy.txt";
        //public const string SATORI_HIERARCHY_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\satori-hierarchy.txt";
        //public const string COUNT_NUM_BY_MENTION_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\count-num-by-mention.txt";
        //public const string COUNT_NUM_BY_ENTITY_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\count-num-by-entity.txt";
        //public const string HIERARCHY_WITH_MENTION_NUM_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\hierarchy-by-mention.txt";
        //public const string HIERARCHY_WITH_MENTION_NUM_FILE = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\hierarchy-by-entity.txt";
        private DefaultParameter() { }
    }
}
