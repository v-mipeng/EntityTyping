using pml.file.reader;
using pml.file.writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class OpenNer
    {

        opennlp.tools.namefind.NameFinderME locationNameFinder = null;
        opennlp.tools.namefind.NameFinderME personNameFinder = null;
        opennlp.tools.namefind.NameFinderME organizationNameFinder = null;
        List<pml.type.Pair<string, string>> entities = null;

        public OpenNer() { }

        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
        public void FindNer(string context)
        {
            if(locationNameFinder == null)
            {
                Initial();
            }
            var tokens = regex.Split(context);
            // Find location names
            var locations = new List<string>();
            opennlp.tools.util.Span[] locationSpan = locationNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            locationNameFinder.clearAdaptiveData();
            locations.AddRange(opennlp.tools.util.Span.spansToStrings(locationSpan, tokens).AsEnumerable());
            // Find person names
            var persons = new List<string>();
            opennlp.tools.util.Span[] personSpan = personNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            personNameFinder.clearAdaptiveData();
            persons.AddRange(opennlp.tools.util.Span.spansToStrings(personSpan, tokens).AsEnumerable());
            // Find organization names
            var organizations = new List<string>();
            opennlp.tools.util.Span[] organizationSpan = organizationNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            organizationNameFinder.clearAdaptiveData();
            organizations.AddRange(opennlp.tools.util.Span.spansToStrings(organizationSpan, tokens).AsEnumerable());

            entities = new List<pml.type.Pair<string, string>>();
            foreach (var location in locations)
            {
                entities.Add(new pml.type.Pair<string, string>(location, "LOCATION"));
            }
            foreach (var person in persons)
            {
                entities.Add(new pml.type.Pair<string, string>(person, "PERSON"));
            }
            foreach (var organization in organizations)
            {
                entities.Add(new pml.type.Pair<string, string>(organization, "ORGANIZATION"));
            }
        }

        public List<pml.type.Pair<string, string>> GetEntities()
        {
            return entities;
        }

        public string GetNerType(string mention)
        {
            if (mention == null)
            {
                throw new Exception("Mention should not be null for finding ner type");
            }
            mention = regex.Replace(mention, "").ToLower();
            foreach (var entity in entities)
            {
                var str1 = regex.Replace(entity.first, "").ToLower();
                if (str1.Contains(mention) || mention.Contains(str1))
                {
                    return entity.second;
                }
            }
            return "UNKNOW";
        }


        private void Initial()
        {
            var basedir = @"D:\Codes\C#\EntityTyping\Fine-ner\input\opennlp models";
            var modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-location.bin")); //load the name model into a stream
            var model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream); //load the model
            locationNameFinder = new opennlp.tools.namefind.NameFinderME(model);                   //create the namefinder
            modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-person.bin"));
            model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream);
            personNameFinder = new opennlp.tools.namefind.NameFinderME(model);
            modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-organization.bin"));
            model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream);
            organizationNameFinder = new opennlp.tools.namefind.NameFinderME(model);
        }

        public static void Temp()
        {
            var source = @"D:\Data\DBpedia\mapping based types";

            var desDir = "";
            var dic = new Dictionary<string, FileWriter>();
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Data\DBpedia\entity type pairs.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            System.Text.RegularExpressions.Regex entityRegex = new System.Text.RegularExpressions.Regex(@"/([^>/]+)>\s<");
            System.Text.RegularExpressions.Regex typeRegex = new System.Text.RegularExpressions.Regex(@"ontology/(\w+)>\s\.$");
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {

                string entity = null;
                string type = null;
                if (entityRegex.IsMatch(line))
                {
                    var match = entityRegex.Match(line);
                    entity = match.Groups[1].Value;
                }
                if (typeRegex.IsMatch(line))
                {
                    var match = typeRegex.Match(line);
                    type = match.Groups[1].Value;
                }
                if (entity != null && type != null)
                {
                    if (++count % 10000 == 0)
                    {
                        Console.WriteLine(count);
                    }
                    writer.WriteLine(entity + "\t" + type);
                }
            }
            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// map types
        /// </summary>
        public static void Temp2()
        {
            var dbpediaToSatoriDic = new Dictionary<string, Dictionary<string,int>>();
            var satoriMentionDic = new Dictionary<string, string>();
            var satoriEntityDic = new Dictionary<string, string>();
            var dbpedia = @"D:\Data\DBpedia\entity type pairs.txt";
            var satori = @"D:\Codes\C#\EntityTyping\Fine-ner\input\feature\train.txt";
            var des = @"D:\Codes\C#\EntityTyping\Fine-ner\input\db2satori.txt";
            var dbpediaReader = new LargeFileReader(dbpedia);
            var satoriReader = new LargeFileReader(satori);
            var writer = new LargeFileWriter(des, FileMode.Create);
            var line = "";
            Dictionary<string, int> dic = null;

            while((line = satoriReader.ReadLine())!= null)
            {
                var array = line.Split('\t');
                satoriMentionDic[array[0]] = array[2];
                satoriEntityDic[array[1]] = array[2];
            }
            satoriReader.Close();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
            System.Text.RegularExpressions.Regex deleteBrace = new System.Text.RegularExpressions.Regex(@"\(\w+\)");
            var count = 0;

            while((line = dbpediaReader.ReadLine())!=null)
            {
                if(++count%10000 == 0)
                {
                    Console.WriteLine(count);
                }
                var array = line.Split('\t');
                var entity = deleteBrace.Replace(array[0],"");
                entity = regex.Replace(entity, " ").Trim();
                if (satoriEntityDic.ContainsKey(entity))
                {

                    try
                    {
                        dic = dbpediaToSatoriDic[array[1]];
                    }
                    catch (Exception)
                    {
                        dic = new Dictionary<string, int>();
                        dbpediaToSatoriDic[array[1]] = dic;
                    }
                    try
                    {
                        dic[satoriEntityDic[entity]] += 1;
                    }
                    catch (Exception)
                    {
                        dic[satoriEntityDic[entity]] = 1;
                    }
                }
                else if(satoriMentionDic.ContainsKey(entity))
                {
                    
                    try
                    {
                        dic = dbpediaToSatoriDic[array[1]];
                    }
                    catch(Exception)
                    {
                        dic = new Dictionary<string, int>();
                        dbpediaToSatoriDic[array[1]] = dic;
                    }
                    try
                    {
                        dic[satoriMentionDic[entity]] += 1;
                    }
                    catch(Exception)
                    {
                        dic[satoriMentionDic[entity]] = 1;
                    }
                }
            }
            dbpediaReader.Close();
            foreach (var item in dbpediaToSatoriDic)
            {
                foreach (var d in item.Value)
                {
                    writer.WriteLine(item.Key + "\t" + d.Key + "\t" + d.Value);
                }
            }
            writer.Close();

        }

        public static void Temp3()
        {
            var source = @"D:\Data\DBpedia\redirects.ttl";
            var reader = new pml.file.reader.LargeFileReader(source);
            var des = @"D:\Data\DBpedia\redirects.txt";
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            System.Text.RegularExpressions.Regex firstRegex = new System.Text.RegularExpressions.Regex(@"/([^>/]+)>\s<");
            System.Text.RegularExpressions.Regex secondRegex = new System.Text.RegularExpressions.Regex(@"/(\w+)>\s\.$");
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                    string first = null;
                    string second = null;

                    if (firstRegex.IsMatch(line))
                    {
                        var match = firstRegex.Match(line);
                        first = match.Groups[1].Value;
                    }
                    if (secondRegex.IsMatch(line))
                    {
                        var match = secondRegex.Match(line);
                        second = match.Groups[1].Value;
                    }
                    if (first != null && second != null)
                    {
                        if (++count % 10000 == 0)
                        {
                            Console.WriteLine(count);
                        }
                    writer.WriteLine(first + "\t" + second);
                }
            }
            reader.Close();
            writer.Close();

        }
         

        public static void Main(string[] args)
        {
            Temp3();
            //var input = "I like Beijing";
            //var ner = new OpenNer();
            //ner.FindNer(input);
            //var type = ner.GetNerType("Beijing");
            //var source = @"D:\Codes\C#\EntityTyping\Fine-ner\input\dictionaries\tmp.txt";
            //var desDir = "";
            //var dic = new Dictionary<string, int>();
            //var reader = new pml.file.reader.LargeFileReader(source);
            //string line;
            //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("/");
            //var count = 0;

            //while((line =reader.ReadLine())!=null)
            //{
            //    if(++count%1000 == 0)
            //    {
            //        Console.WriteLine(count);
            //    }
            //    try
            //    {
            //        dic[line] += 1;
            //    }
            //    catch(Exception)
            //    {
            //        dic[line] = 1;
            //    }
            //}
            //reader.Close();
            //var des = @"D:\Codes\C#\EntityTyping\Fine-ner\input\dictionaries\tmp2.txt";
            //var writer = new LargeFileWriter(des, FileMode.Create);

            //foreach(var item in dic)
            //{
            //    writer.WriteLine(item.Key + "\t" + item.Value);
            //}
            //writer.Close();

        }

    }
}
