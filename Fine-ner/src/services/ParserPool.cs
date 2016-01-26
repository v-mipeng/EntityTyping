using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.trees;
using edu.stanford.nlp;
using edu.stanford.nlp.util;
using java.util;
using System.IO;
using pml.type;
using edu.stanford.nlp.semgraph;
using System.Text.RegularExpressions;
using System.Threading;

namespace msra.nlp.tr
{
    public class ParserPool
    {

        static List<DependencyParser> parsers = new List<DependencyParser>();
        static HashSet<int> availableParsers = new HashSet<int>();
        readonly static int maxParserNum = 20;
        static object locker = new object();

        /// <summary>
        /// Get a dependency parser from parser pool
        /// </summary>
        /// <returns></returns>
        public static DependencyParser GetParser()
        {
            lock (locker)
            {
                if (availableParsers.Count > 0)
                {
                    try
                    {
                        var index = availableParsers.First();
                        availableParsers.Remove(index);
                        return parsers[index];
                    }
                    catch(Exception e)
                    {
                        Console.Clear();
                        Console.WriteLine("Parsers pool is empty!");
                        Console.WriteLine(availableParsers.Count);
                        Console.WriteLine(e.Message);
                        Console.ReadKey();
                        throw e;
                    }
                }
                else if (parsers.Count < maxParserNum)
                {
                    if (availableParsers.Count == 0)
                    {
                        var parser = new DependencyParser();
                        parsers.Add(parser);
                        return parser;
                    }
                    else
                    {
                        var index = availableParsers.First();
                        availableParsers.Remove(index);
                        return parsers[index];
                    }
                }
                else
                {
                    while(availableParsers.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                    var index = availableParsers.First();
                    availableParsers.Remove(index);
                    return parsers[index];
                }
            }
        }

        /// <summary>
        /// return parser to the parser pool
        /// </summary>
        /// <param name="parser"></param>
        public static void ReturnParser(DependencyParser parser)
        {
            for (var i = 0; i < parsers.Count; i++)
            {
                if (parser == parsers[i])
                {
                    availableParsers.Add(i);
                    break;
                }
            }
        }

    }
}
