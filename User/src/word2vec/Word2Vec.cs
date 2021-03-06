﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;


namespace User.src
{
    public class Word2Vec
    {
        //refine word2vec: select necessary words 
        public static void Mains(string[] args)
        {
            //SelectInterestWordVector(@"D:\Codes\C#\EntityTyping\word2vec\wordTable.txt",
            //                         @"D:\Codes\C#\EntityTyping\word2vec\test\vectors.bin", 
            //                         @"D:\Codes\C#\EntityTyping\word2vec\google-vectors.txt");
            ParseWord2Vec(@"E:\Users\v-mipeng\Data\Google-word2vec\freebase-mention-vectors", @"E:\Users\v-mipeng\Data\Google-word2vec\freebase-mention-vectors.txt");
        }

        public static void SelectInterestWordVector(string interestWordFile, string word2vecFile, string compressedWord2VectorFile)
        {
            var reader = new LargeFileReader(interestWordFile);
            string line;
            var set = new HashSet<string>();

            while((line = reader.ReadLine())!=null)
            {
                set.Add(line.Trim());
            }
            reader.Close();
            var writer = new LargeFileWriter(compressedWord2VectorFile, FileMode.Create);
            var parser = new ParseBinaryVector(word2vecFile);
            int count = 0;
            while (!parser.EOF)
            {
                if (++count % 1000 == 0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    var pair = parser.GetNextVector();
                    if (set.Contains(pair.first))
                    {
                        writer.Write(pair.first);
                        foreach (var value in pair.second)
                        {
                            writer.Write(string.Format(" {0}", value));
                        }
                        writer.Write("\r");
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            writer.Close();
        }

        static void  ParseWord2Vec(string word2vecFile, string parsedFile)
        {
            var writer = new LargeFileWriter(parsedFile, FileMode.Create);
            var parser = new ParseBinaryVector(word2vecFile);
            int count = 0;
            while (!parser.EOF)
            {
                if (++count % 1000 == 0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    var pair = parser.GetNextVector();
                    var index = pair.first.IndexOf("en/");
                    writer.Write(pair.first.Substring(index+3));
                    foreach (var value in pair.second)
                    {
                        writer.Write(string.Format(" {0}", value));
                    }
                    writer.Write("\r");
                }
                catch(Exception )
                {
                    continue;
                }
            }
            writer.Close();
        }

    }
}
