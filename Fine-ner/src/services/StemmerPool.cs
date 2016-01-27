using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class StemmerPool
    {

        static List<Stemmer> stemmers = new List<Stemmer>();
        static HashSet<int> availableStemmers = new HashSet<int>();
        readonly static int maxStemmerNum = 100;
        static object locker = new object();

        /// <summary>
        /// Get a stemmer  from stemmer pool
        /// </summary>
        /// <returns></returns>
        public static Stemmer GetStemmer()
        {
            lock (locker)
            {
                lock(availableStemmers)
                {
                    if (availableStemmers.Count > 0)
                    {
                        var index = availableStemmers.First();
                        availableStemmers.Remove(index);
                        return stemmers[index];
                    }
                    else if (stemmers.Count < maxStemmerNum)
                    {
                        if (availableStemmers.Count == 0)
                        {
                            var stemmer = new Stemmer();
                            stemmers.Add(stemmer);
                            return stemmer;
                        }
                        else
                        {
                            var index = availableStemmers.First();
                            availableStemmers.Remove(index);
                            return stemmers[index];
                        }
                    }
                }
                {
                    while (availableStemmers.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                    var index = availableStemmers.First();
                    availableStemmers.Remove(index);
                    return stemmers[index];
                }
            }
        }

        /// <summary>
        /// return stemmer to the stemmer pool
        /// </summary>
        /// <param name="stemmer"></param>
        public static void ReturnStemmer(Stemmer stemmer)
        {
            for (var i = 0; i < stemmers.Count; i++)
            {
                if (stemmer == stemmers[i])
                {
                    lock(availableStemmers)
                    {
                        availableStemmers.Add(i);
                    }
                    break;
                }
            }
        }
    }
}
