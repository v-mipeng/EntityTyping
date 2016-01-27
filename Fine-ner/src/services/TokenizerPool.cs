using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class TokenizerPool
    {

        static List<Tokenizer> tokenizers = new List<Tokenizer>();
        static HashSet<int> availableTokenizers = new HashSet<int>();
        readonly static int maxTokenizerNum = 100;
        static object locker = new object();

        /// <summary>
        /// Get a tokenizer from tokenizer pool
        /// </summary>
        /// <returns></returns>
        public static Tokenizer GetTokenizer()
        {
            lock (locker)
            {
                lock (availableTokenizers)
                {
                    if (availableTokenizers.Count > 0)
                    {
                        var index = availableTokenizers.First();
                        availableTokenizers.Remove(index);
                        return tokenizers[index];
                    }
                    else if (tokenizers.Count < maxTokenizerNum)
                    {
                        if (availableTokenizers.Count == 0)
                        {
                            var tokenizer = new Tokenizer();
                            tokenizers.Add(tokenizer);
                            return tokenizer;
                        }
                        else
                        {
                            var index = availableTokenizers.First();
                            availableTokenizers.Remove(index);
                            return tokenizers[index];
                        }
                    }
                }
                {
                    while (availableTokenizers.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                    var index = availableTokenizers.First();
                    availableTokenizers.Remove(index);
                    return tokenizers[index];
                }
            }
        }

        /// <summary>
        /// return tokenizer to the tokenizer pool
        /// </summary>
        /// <param name="parser"></param>
        public static void ReturnTokenizer(Tokenizer tokenizer)
        {
            for (var i = 0; i < tokenizers.Count; i++)
            {
                if (tokenizer == tokenizers[i])
                {
                    lock(availableTokenizers)
                    {
                        availableTokenizers.Add(i);
                    }
                    break;
                }
            }
        }
    }
}
