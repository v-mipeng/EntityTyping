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

        // Label mention index
        // Mention start from with offset which begins with 0 in the context
        protected int mentionOffset = -1;

        protected int mentionLength = -1;


        public Instance(string mention, string context) 
        {
            this.mention = mention;
            this.context = context;
            this.mentionLength = mention.Length;
            this.mentionOffset = context.IndexOf(mention);
        }

        public Instance(string context, int mentionOffset, int mentionLength)
        {
            this.context = context;
            this.mentionOffset = mentionOffset;
            this.mentionLength = mentionLength;
        }

        public Instance(string context, int mentionOffset, int mentionLength, string label):
            this(context, mentionOffset, mentionLength)
        {
            this.label = new Label(label);
            this.mention = context.Substring(mentionOffset, mentionLength);
        }

        public Instance(string context, int mentionOffset, int mentionLength, Label label) :
            this(context, mentionOffset, mentionLength)
        {
            this.label = label;
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

        public int MentionOffset
        {
            get
            {
                return mentionOffset;
            }
        }

        public int MentionLength
        {
            get
            {
                return mentionLength;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", mention,label,context);
        }
    }
}
