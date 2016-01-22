using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Mention
    {
        private string mention = null;
        private List<string> tokens = null;

        public Mention(string mention)
        {
            this.mention = mention;
        }

        public string Mention
        {
            get
            {
                return Mention;
            }
            private set
            {

            }
        }

        /// <summary>
        /// Tokens of mention
        /// </summary>
        public List<string> Tokens
        {
            get
            {
                if (tokens == null)
                {
                    GetTokens();
                }
                return tokens;
            }
            private set
            {

            }
        }

        /// <summary>
        /// Tokenize mention
        /// </summary>
        private void GetTokens()
        {
            var tokenizer = TokenizerPool.GetTokenizer();
            this.tokens = tokenizer.Tokenize(this.mention);
            TokenizerPool.ReturnTokenizer(tokenizer);
        }
    }
}
