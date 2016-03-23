using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class NotFindMentionException :ApplicationException
    {
        String message = null;

        public NotFindMentionException(String message)
            : base(message)
        {
            this.message = message;
        }

        public NotFindMentionException(Exception inner)
            : base(inner.Message, inner)
        {
            message = inner.Message;
        }

        public String GetMessage()
        {
            return message;
        }
    }
}
