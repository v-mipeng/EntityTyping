using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class SSpliterPool
    {
        static List<SentenceSpliter> sspliters = new List<SentenceSpliter>();
        static HashSet<int> availableSSpliters = new HashSet<int>();
        readonly static int maxSSpliterNum = 100;
        static object locker = new object();

        /// <summary>
        /// Get a sentence spliter from parser pool
        /// </summary>
        /// <returns></returns>
        public static SentenceSpliter GetSSpliter()
        {
            lock (locker)
            {
                lock(availableSSpliters)
                {
                    if (availableSSpliters.Count > 0)
                    {
                        var index = availableSSpliters.First();
                        availableSSpliters.Remove(index);
                        return sspliters[index];
                    }
                    else if (sspliters.Count < maxSSpliterNum)
                    {

                        if (availableSSpliters.Count == 0)
                        {
                            var sspliter = new SentenceSpliter();
                            sspliters.Add(sspliter);
                            return sspliter;
                        }
                        else
                        {
                            var index = availableSSpliters.First();
                            availableSSpliters.Remove(index);
                            return sspliters[index];
                        }
                    }
                }
                {
                    while (availableSSpliters.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                    var index = availableSSpliters.First();
                    availableSSpliters.Remove(index);
                    return sspliters[index];
                }
            }
        }

        /// <summary>
        /// return sentence spliter to the  pool
        /// </summary>
        /// <param name="sspliter"></param>
        public static void ReturnSSpliter(SentenceSpliter sspliter)
        {
            for (var i = 0; i < sspliters.Count; i++)
            {
                if (sspliter == sspliters[i])
                {
                    lock(availableSSpliters)
                    {
                        availableSSpliters.Add(i);
                    }
                    break;
                }
            }
        }
    }
}
