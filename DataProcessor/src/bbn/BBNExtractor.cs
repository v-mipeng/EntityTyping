using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using pml.file.reader;
using pml.file.writer;
using System.IO;

namespace DataProcessor.src.bbn
{
    class BBNExtractor
    {
        public static void Extract(string source, string des)
        {
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(des, FileMode.Create);
            var buffer = new StringBuilder();
            var regex = new Regex("(?i)<\\w+\\s?type=\"([^\"]+)\">\\s?([^\\<]+)\\s?</[^\\>]+>");
            string line;
            string document;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if(++count%100==0)
                {
                    Console.WriteLine(count);
                }
                if (line.StartsWith("<"))
                {
                    continue;
                }
                else
                {   // content of document
                    line = line.Trim();
                    var matches = regex.Matches(line);
                    document = regex.Replace(line, "$2");
                    foreach (Match match in matches)
                    {
                        writer.Write(match.Groups[2].Value.Trim());
                        writer.Write("\t" + match.Groups[1].Value);
                        writer.WriteLine("\t" + document);
                    }
                }
            }
            reader.Close();
            writer.Close();
        }

        public static void Filter(string source, string interestTypeFile, string des)
        {
            var reader = new LargeFileReader(interestTypeFile);
            var writer = new LargeFileWriter(des, FileMode.Create);
            var set = new HashSet<string>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                set.Add(line.Trim());
            }
            reader.Close();
            reader.Open(source);
            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                if (set.Contains(array[1].ToLower()))
                {
                    writer.WriteLine(line);
                }
            }
            reader.Close();
            writer.Close();
        }

        public static void TypeMap(string source, string typeMapFile, string des)
        {
            var reader = new LargeFileReader(typeMapFile);
            var writer = new LargeFileWriter(des, FileMode.Create);
            var dic = new Dictionary<string, string>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                dic[array[0]] = array[1];
            }
            reader.Close();
            reader.Open(source);
            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                writer.WriteLine(array[0]+"\t"+dic[array[1].ToLower()]+"\t"+array[2]);
            }
            reader.Close();
            writer.Close();
        }
    }
}
