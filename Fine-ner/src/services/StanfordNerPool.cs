using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class StanfordNerPool
    {
        static List<StanfordNer> ners = new List<StanfordNer>();
        static HashSet<int> availableNers = new HashSet<int>();
        readonly static int maxNerNum = 50;
        static object locker = new object();

        /// <summary>
        /// Get a stanford ner from ner pool
        /// </summary>
        /// <returns></returns>
        public static StanfordNer GetStanfordNer()
        {
            lock (locker)
            {
                lock (availableNers)
                {
                    if (availableNers.Count > 0)
                    {
                        try
                        {
                            var index = availableNers.First();
                            availableNers.Remove(index);
                            return ners[index];
                        }
                        catch (Exception e)
                        {
                            Console.Clear();
                            Console.WriteLine("Ner pool is empty!");
                            Console.WriteLine(availableNers.Count);
                            Console.WriteLine(e.Message);
                            throw e;
                        }
                    }
                    else if (ners.Count < maxNerNum)
                    {
                        if (availableNers.Count == 0)
                        {
                            var ner = new StanfordNer();
                            ners.Add(ner);
                            return ner;
                        }
                        else
                        {
                            var index = availableNers.First();
                            availableNers.Remove(index);
                            return ners[index];
                        }
                    }
                }
                {
                    while (availableNers.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                    var index = availableNers.First();
                    availableNers.Remove(index);
                    return ners[index];
                }
            }
        }

        /// <summary>
        /// return ner to the parser pool
        /// </summary>
        /// <param name="parser"></param>
        public static void ReturnStanfordNer(StanfordNer ner)
        {
            for (var i = 0; i < ners.Count; i++)
            {
                if (ner == ners[i])
                {
                    lock (availableNers)
                    {
                        availableNers.Add(i);
                    }
                    break;
                }
            }
        }

    }
}
