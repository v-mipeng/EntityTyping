using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pml.file.writer
{
   public  interface FileWriter
    {
        /*Throw Exception if file cannot open!
           */
       void Open(String filePath);
    

        /*Throw Exception if file cannot open!
        */
       void Open(String filePath, FileMode fileModel);


        /*Throw Exception if file cannot open!
        */
       void Open(String filePath, FileMode fileModel, FileShare fileShare);

        /*Throw Exception if file cannot open!
        */
       void Open(String filePath, FileMode fileModel, FileShare fileShare, Encoding encoding);

       void Close();

        /* Write an object to the file. object.ToString() will be invoked if necessary
         */
        void Write(Object output);

        void WriteLine(Object output);


        /* Cache the output. 
         * You can obsolete cached object by invoking ClearCache()
         * or you can write it to the file with WriteCache()
         */
        void Cache(Object output);

        /* Cache the output and append a default line terminal to it. 
         * You can obsolete cached object by invoking ClearCache()
         * or you can write it to the file with WriteCache()
         */
        void CacheLine(Object output);

        /* Clear the Cached object
         */
        void ClearCache();

        /* Write the cached object to the file stream
         */
        void WriteCache();

    }
}
