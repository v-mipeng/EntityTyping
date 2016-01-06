using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.writer;
using pml.file.reader;
using System.IO;
using pml.type;
using msra.nlp.tr.HierarchyExtractor;

namespace msra.nlp.tr.User
{
    class Demo
    {
        public static void Main(String[] args)
        {
            //ParseHierarchy();
            StatisticTypeNum();
            //StatisticMentionNumByType(@"E:\Users\v-mipeng\Data\Satori\Raw\Interlink.stype.tsv",
                //@"E:\Users\v-mipeng\Data\Satori\Raw\trainNumByType.txt");
        }

        static void ParseHierarchy()
        {
            Property props = new Property();
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
            */
            props.Set("method", "/e -i -l -dl");
            // set file path stored the satori ontology file
            props.Set("satori_type_xml_file", @"E:\Users\v-mipeng\Data\Satori\Type Info\satori-ontology.all.xml");
            string basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\";
            // set the file to store the hierarchy extracted
            props.Set("hierarchy_file", Path.Combine(basedir, "satori-hierarchy.txt"));
            // set internal directory to store the internal files
            props.Set("internal_directory", Path.Combine(basedir, "internal/"));
            // set file path stored the interested top level types. It is recommended if method contains -i
            props.Set("interest_type_file", Path.Combine(basedir, "filter/interestTypes.txt"));
            // set file path stored the uninterested top level types you want to delete. It is recommended if method contains -ui
            props.Set("uninterest_type_file", Path.Combine(basedir, "filter/uninterestTypes.txt"));
            // set file path stored type links you want to add. It is recommended if method contains -l
            props.Set("link_file", Path.Combine(basedir, "filter/links.txt"));
            // set file path stored  type links you want to delete. It is recommended if method contains -dl
            props.Set("delink_file", Path.Combine(basedir, "filter/delinks.txt"));
            Pipeline pipeline = new Pipeline(props);
            pipeline.Execute();
        }

        static void StatisticTypeNum()
        {
            Property props = new Property();
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
              *        -mt             :extract types with multiple top level parent type
            */
            props.Set("method", "/s -c -oh");
            // set file path of satori hierarchy. Hierarchy is used to organized the number.
            props.Set("hierarchy_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\temp-hierarchy.txt");
            // set satori data file path. file format:   mention    TAB     entity      TAB     type list(separated by '#')      description
            //if method contains -r, this is recommended, otherwise using default path.
            props.Set("satori_raw_data_file", @"E:\Users\v-mipeng\Data\Satori\Raw\Interlink.stype.tsv");

            string basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\";
            // set file stored the extracted encountered mention number by type information. File format is : type      TAB    encountered number 
            // if method contains -r this file is used to store this information, is method contains -c, program will read the information for statistic
            props.Set("count_mention_num_by_type_file", Path.Combine(basedir,"count-num-by-mention.txt"));
            // set file stored the extracted encountered entity number by type information. File format is : type      TAB    encountered number 
            props.Set("count_entity_num_by_type_file", Path.Combine(basedir, "count-num-by-entity.txt"));
            // set file to stored the statistic information, this includes mention and entity number of every type information.
            basedir = @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\interest type hierarchy";
            props.Set("organized_num_by_type", Path.Combine(basedir, "numByType.txt"));
            // set file to store the hierarchy with mention number information
            props.Set("hierarchy_with_mention_num_file", Path.Combine(basedir, "hierarchy-by-mention.txt"));
            // set file to store the hierarchy with entity number information
            props.Set("hierarchy_with_entity_num_file", Path.Combine(basedir, "hierarchy-by-entity.txt"));
            // set file stored the wiki entity type informaiton
            props.Set("wiki_entity_type_file", @"E:\Users\v-mipeng\Data\Satori\Raw\WikiType.tsv");
            // set file to store the wiki top 3 type number by entity informaiton
            props.Set("top3_type_num_by_wiki_entity", Path.Combine(basedir, "top3-type-num-by-wiki-entity.txt"));
            // set file to store the wiki top 1 type number by entity informaiton
            props.Set("top1_type_num_by_wiki_entity", Path.Combine(basedir, "top1-type-num-by-wiki-entity.txt"));
            // set file to store types with multiple parents
            props.Set("type_with_multi_parents_file", @"E:\Users\v-mipeng\Codes\C#\NLP\Fine-ner\input\satori ontology\type-with-multi-parents.txt");
            Pipeline pipeline = new Pipeline(props);
            pipeline.Execute();
        }

        static void StatisticMentionNumByType(string sourceFile, string des)
        {
            var reader = new LargeFileReader(sourceFile);
            var dic = new Dictionary<string, int>();
            string line;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (++count%100000 == 0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    var array = line.Split('\t');
                    var types = array[2];
                    array = types.Split('#');
                    try
                    {
                        dic[array[0]] += 1;
                    }
                    catch(Exception)
                    {
                        dic[array[0]] = 1;
                    }
                }
                catch(Exception)
                {
                    continue;
                }
            }
            reader.Close();
            var writer = new LargeFileWriter(des, FileMode.Create);
            foreach (var key in dic.Keys)
            {
                writer.WriteLine(key + "\t" + dic[key]);
            }
            writer.Close();
        }
    }
}
