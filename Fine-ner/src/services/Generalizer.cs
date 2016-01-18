using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace msra.nlp.tr
{
    public class Generalizer
    {
        private Generalizer() { }

        private static Regex regex = new Regex(@"\d+");

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
            var word = DataCenter.GetStemmedWord(input.ToLower());
            // map number to 0 and compress number
            return regex.Replace(word, "0");
        }
    }
}
