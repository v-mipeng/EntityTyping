using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.file
{
    public class FileException : ApplicationException
    {
        String message = null;

        public FileException(String message)   :base(message)
        {
            this.message = message;
        }

        public FileException(Exception inner)   :base(inner.Message,inner)
        {
            message = inner.Message;
        }

        public String GetMessage()
        {
            return message;
        }
    }
}
