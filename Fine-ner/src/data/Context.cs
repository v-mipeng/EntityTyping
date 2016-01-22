using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Context
    {
        private string context = null;

        public Context(string context)
        {
            this.context = context;
        }

        public string Context
        {
            get
            {
                return this.context;
            }
            private set
            {

            }
        }
    }
}
