using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class Generalizer
    {
        private Generalizer() { }

        /// <summary>
        /// Generalize word:
        ///     make word to lower case
        ///     stem word
        ///     map number to 0 and compress number
        /// </summary>
        /// <param name="input">
        ///     Word to be generalized
        /// </param>
        /// <returns>
        ///     Generalized word
        /// </returns>
        public static string Generalize(string input)
        {
            if(input == null)
            {
                return null;
            }
            var word = Stemmer.Stem(input.ToLower())[0];
            // map number to 0 and compress number
            var buffer = word.ToArray();
            var newBuffer = new StringBuilder();
            var c = (char)0;
            for(int i = 0;i<buffer.Length;i++)
            {
                if (buffer[i] <= '9' && buffer[i] >= '0')
                {
                    if (c != '0')
                    {
                        newBuffer.Append('0');
                        c = '0';
                    }
                }
                else
                {
                    newBuffer.Append(buffer[i]);
                    c = buffer[i];
                }
            }
            return newBuffer.ToString();
        }
    }
}
