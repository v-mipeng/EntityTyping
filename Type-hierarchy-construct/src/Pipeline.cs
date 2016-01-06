using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using pml.type;

namespace msra.nlp.tr.HierarchyExtractor
{
    class Pipeline
    {

        public Pipeline()
        {
            Initial(new Property());
        }

        public Pipeline(Property props)
        {
            Initial(props);
        }

        private void Initial(Property props)
        {
            foreach (Object key in DefaultParameter.GetParameterSet())
            {
                GlobalParameter.Set(key, props.GetOrDefault(key, DefaultParameter.Get(key)));
            }
        }

        public void SetProperty(Property props)
        {
            foreach (Object key in props.Keys)
            {
                GlobalParameter.Set(key, props.GetOrDefault(key, DefaultParameter.Get(key)));
            }
        }

        public void Execute()
        {
            string method = (string)GlobalParameter.Get("method");
            string operation = null;
            HashSet<string> options = new HashSet<string>();
            string option = null;

            string[] array = method.Split(new char[]{' '});

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].StartsWith("/"))
                {
                    if (operation != null && operation.Equals("e"))
                    {
                        ExtractHierarchy(options);
                    }
                    else if (operation != null && operation.Equals("s"))
                    {
                        StatisticHierarchyInfo(options);
                    }
                    operation = array[i].Substring(1, array[i].Length - 1);
                    options.Clear();
                }
                else if (array[i].StartsWith("-"))
                {
                    option = array[i].Substring(1, array[i].Length - 1);
                    if (operation == null)
                    {
                        throw new Exception("Invalid operation!");
                    }
                    if (operation.Equals("e"))
                    {
                        if (option.Equals("i") || option.Equals("ui") || option.Equals("l") || option.Equals("dl") || option.Equals("oi"))
                         {
                            options.Add(option);
                        }
                        else
                        {
                            Console.WriteLine("Invalid operation!");
                        }
                    }
                    else if (operation.Equals("s"))
                    {
                        if (option.Equals("r") || option.Equals("c") || option.Equals("oh") || option.Equals("we1") || option.Equals("we3")  || option.Equals("mp"))
                        {
                            options.Add(option);
                        }
                        else
                        {
                            Console.WriteLine("Invalid operation!");
                        }
                    }
                }
            }
            if (operation != null && operation.Equals("e"))
            {
                ExtractHierarchy(options);
            }
            else if (operation != null && operation.Equals("s"))
            {
                StatisticHierarchyInfo(options);
            }
        }

        void StatisticHierarchyInfo(HashSet<string> options)
        {
            Pair<Dictionary<string, int>, Dictionary<string, int>> pair = null;
            if(options.Contains("r"))
            {
                pair = Statistic.StatisticNumByType(
                    (string)GlobalParameter.Get("satori_raw_data_file"),
                    (string)GlobalParameter.Get("count_mention_num_by_type_file"),
                    (string)GlobalParameter.Get("count_entity_num_by_type_file"),
                    (string)GlobalParameter.Get("organized_num_by_type"));
                if(options.Contains("oh"))
                {
                    Statistic.FormatHierarchy(pair.first ,
                        (string)GlobalParameter.Get("hierarchy_file"),
                        (string)GlobalParameter.Get("hierarchy_with_mention_num_file")
                        );
                    Statistic.FormatHierarchy(pair.second,
                       (string)GlobalParameter.Get("hierarchy_file"),
                       (string)GlobalParameter.Get("hierarchy_with_entity_num_file")
                       );
                }
            }
            else if(options.Contains("c"))
            {
                pair = Statistic.StatisticNumByType(
                (string)GlobalParameter.Get("count_mention_num_by_type_file"),
                (string)GlobalParameter.Get("count_entity_num_by_type_file"),
                (string)GlobalParameter.Get("organized_num_by_type"));
               Statistic.FormatHierarchy(pair.first ,
                        (string)GlobalParameter.Get("hierarchy_file"),
                        (string)GlobalParameter.Get("hierarchy_with_mention_num_file")
                        );
                    Statistic.FormatHierarchy(pair.second,
                       (string)GlobalParameter.Get("hierarchy_file"),
                       (string)GlobalParameter.Get("hierarchy_with_entity_num_file")
                       );
                }
            else if(options.Contains("oh"))
            {
                Statistic.FormatHierarchy(
                      (string)GlobalParameter.Get("count_mention_num_by_type_file"),
                      (string)GlobalParameter.Get("hierarchy_file"),
                      (string)GlobalParameter.Get("hierarchy_with_mention_num_file")
                      );
                Statistic.FormatHierarchy(
                      (string)GlobalParameter.Get("count_entity_num_by_type_file"),
                      (string)GlobalParameter.Get("hierarchy_file"),
                      (string)GlobalParameter.Get("hierarchy_with_entity_num_file")
                      );
            }
            if(options.Contains("we3"))
            {
                Statistic.StatisticTypeNumByUniqueEntity((string)GlobalParameter.Get("wiki_entity_type_file"), (string)GlobalParameter.Get("top3_type_num_by_wiki_entity"));
            }
            else if (options.Contains("we1"))
            {
                Statistic.StatisticTypeNumByUniqueEntity((string)GlobalParameter.Get("wiki_entity_type_file"), (string)GlobalParameter.Get("top1_type_num_by_wiki_entity"), Statistic.TypeOption.Top1);
            }
            if (options.Contains("mp"))
            {
                Statistic.FindTypeWithMultiParents(Statistic.LoadHierarchy((string)GlobalParameter.Get("hierarchy_file")), Statistic.LoadSet((string)GlobalParameter.Get("interest_type_file")), (string)GlobalParameter.Get("type_with_multi_parents_file"));
            }
        }
  
        void ExtractHierarchy(HashSet<string> options)
        {
            HierarchyParser parser = new HierarchyParser();
            if (options.Contains("i"))
            {
                parser.SetInterestTypes((string)GlobalParameter.Get("interest_type_file"));
            }
            if (options.Contains("ui"))
            {
                parser.SetInterestTypes((string)GlobalParameter.Get("uninterest_type_file"));
            }
            if (options.Contains("l"))
            {
                parser.SetLink((string)GlobalParameter.Get("link_file"));
            }
            if (options.Contains("dl"))
            {
                parser.DeleteLink((string)GlobalParameter.Get("delink_file"));
            }
            if (options.Contains("oi"))
            {
                parser.ExtractPrimaryHierachy((string)GlobalParameter.Get("satori_type_xml_file"), 
                    (string)GlobalParameter.Get("hierarchy_file"), 
                    (string)GlobalParameter.Get("internal_directory"));
            }
            else
            {
                parser.ExtractPrimaryHierachy((string)GlobalParameter.Get("satori_type_xml_file"),
              (string)GlobalParameter.Get("hierarchy_file")
              );
              }
        }
    }
}
