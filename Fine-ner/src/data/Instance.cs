using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// An instance of query with: mention , context and label for test.
    /// </summary>
    public class Instance
    {
        protected string mention = null;

        protected Label label = null;

        protected string context = null;


        public Instance(string mention, string context) 
        {
            this.mention = mention;
            this.context = context;
        }

        public Instance(string mention, Label label, string context): this(mention, context)
        {
            this.label = label;
        }
        public Instance(string mention, string label, string context) : this(mention, context)
        {
            this.label = new Label(label);
        }

        public string Mention
        {
            get
            {
                return mention;
            }
            private set { }
        }

        public Label Label
        {
            get
            {
                return label;
            }
            private set { }
        }

        public string Context
        {
            get
            {
                return context;
            }
            private set { }
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", mention,label,context);
        }
    }
}
