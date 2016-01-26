using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// Pos Tagger Pool
    /// </summary>
   public class PosTaggerPool
    {
        static List<PosTagger> taggers = new List<PosTagger>();
        static HashSet<int> availableTaggers = new HashSet<int>();
        readonly static int maxTaggerNum = 20;
        static object locker = new object();

        /// <summary>
        /// Get a pos tagger from parser pool
        /// </summary>
        /// <returns></returns>
        public static PosTagger GetPosTagger()
        {
            lock(locker)
            {
                if (availableTaggers.Count > 0)
                {
                    var index = availableTaggers.First();
                    availableTaggers.Remove(index);
                    return taggers[index];
                }
                else if (taggers.Count < maxTaggerNum)
                {
                    // wait 1 second
                    //if (taggers.Count > 0)
                    //{
                    //    Thread.Sleep(1000);
                    //}
                    if (availableTaggers.Count == 0)
                    {
                        var tagger = new PosTagger();
                        taggers.Add(tagger);
                        return tagger;
                    }
                    else
                    {
                        var index = availableTaggers.First();
                        availableTaggers.Remove(index);
                        return taggers[index]; 
                    }
                }
                else
                {
                    while (availableTaggers.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                    var index = availableTaggers.First();
                    availableTaggers.Remove(index);
                    return taggers[index];
                }
            }
        }

        /// <summary>
        /// return pos tagger to the tagger pool
        /// </summary>
        /// <param name="tagger"></param>
        public static void ReturnPosTagger(PosTagger tagger)
        {
            for (var i = 0; i < taggers.Count; i++)
            {
                if (tagger == taggers[i])
                {
                    availableTaggers.Add(i);
                    break;
                }
            }
        }
    }
}
