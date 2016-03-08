using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.file.reader;
using pml.file.writer;
using System.IO;

namespace msra.nlp.tr.dp
{
    class DataExtraction
    {
        public static void ExtractMention(string source, string des)
        {
            var reader = new LargeFileReader(source);
            var writer = new LargeFileWriter(des, FileMode.Create);
            string line;
            string sentence = null;
            var buffer = new StringBuilder();
            var mention = new StringBuilder();
            bool end = false;
            var mentions = new List<string>();
            var types = new List<string>();
            string lastType = "";

            while((line = reader.ReadLine())!=null)
            {
                if (string.IsNullOrEmpty(line) && !end)
                {
                    sentence = buffer.ToString().TrimEnd();
                    buffer.Clear();
                    end = true;
                }
                if (end)
                {
                    for (var i = 0; i < mentions.Count; i++)
                    {
                        writer.WriteLine(mentions[i] + "\t" + types[i] + "\t" + sentence);
                    }
                    mentions.Clear();
                    types.Clear();
                    end = false;
                }
                else
                {
                    // within a sentence
                    var array = line.Split(' ');
                    buffer.Append(array[0] + ' ');// remember to trim buffer
                    if (mention.Length > 0)
                    {
                        if (lastType == array[3])
                        {
                            mention.Append(array[0]+" ");
                        }
                        else
                        {
                            mentions.Add(mention.ToString().TrimEnd());
                            if (lastType.Contains("ORG"))
                            {
                                types.Add("organization.organization");
                            }
                            else if (lastType.Contains("LOC"))
                            {
                                types.Add("location.location");
                            }
                            else
                            {
                                types.Add("people.person");
                            }
                            mention.Clear();
                        }
                    }
                    else
                    {
                        if ((array[3].Contains("ORG") || array[3].Contains("LOC") || array[3].Contains("PER")))
                        {
                            mention.Append(array[0]);
                        }
                    }
                    lastType = array[3];
                }
            }
            reader.Close();
            writer.Close();
        }

        public static void Main(string[] args)
        {
            var sourceDir = @"D:\Data\CoNLL";
           var desDir = @"D:\Codes\Project\EntityTyping\Fine-ner\input\conll\";
            var files = Directory.GetFiles(sourceDir);

            foreach(var file in files)
            {
                var des = Path.Combine(desDir, Path.GetFileName(file));
                ExtractMention(file, des);
            }
        }
    }
}
