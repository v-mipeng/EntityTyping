using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.file.reader;
using pml.file.writer;

namespace msra.nlp.tr.dp
{
    class DBpediaProcessor
    {
        /// <summary>
        /// Refine disambiguations file download from dbpedia
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="desFile"></param>
        public static void RefineAmbiguousItem(string sourceFile, string desFile)
        {
            var reader = new LargeFileReader(sourceFile);
            var writer = new LargeFileWriter(desFile, System.IO.FileMode.Create);
            var line = "";
            System.Text.RegularExpressions.Regex sourceRegex = new System.Text.RegularExpressions.Regex(@"/([^/>]+)>");
            System.Text.RegularExpressions.Regex deleteBraceRegex = new System.Text.RegularExpressions.Regex(@"_?\([^\)]+\)");

            System.Text.RegularExpressions.Regex desRegex = new System.Text.RegularExpressions.Regex(@"/([^/>]+)>\s\.$");
            var dic = new Dictionary<string, List<string>>(300000);
            List<string> list = null;
            reader.ReadLine();

            while((line = reader.ReadLine())!=null)
            {
                var sourceMatch = sourceRegex.Match(line);
                var source = sourceMatch.Groups[1].Value;
                source = deleteBraceRegex.Replace(source, "");
                var desMatch = desRegex.Match(line);
                if (dic.TryGetValue(source, out list))
                {
                    list.Add(desMatch.Groups[1].Value);
                }
                else
                {
                    list = new List<string>();
                    list.Add(desMatch.Groups[1].Value);
                    dic[source] = list;
                }
            }
            reader.Close();
            foreach(var item in dic)
            {
                writer.Write(item.Key);
                foreach(var des in item.Value)
                {
                    writer.Write("\t" + des);
                }
                writer.WriteLine("");
            }
            writer.Close();
        }

        public static void RefinePageAbstruct(string sourceFile, string desFile)
        {

        }
    }
}
