using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pml.file.reader
{
    public interface FileReader
    {
        /* Open file defined by the given filePath with default encoding
       */
        bool Open(String filePath);

        /* Open file defined by the given filePath with given encoding
        */
        bool Open(String filePath, Encoding encoding);
        
        /*Close the fileReader and release the resource used by this object
         */ 
        bool Close();

        /* Read a character from the file stream
         */
        Char Read();

        /* Read all the text of the file
         */ 
        String ReadAll();

        /*Read a line of the file
         */ 
        String ReadLine();

        /**Read all the lines of the file
         * return a string array with every element stores a line.
         */ 
        string[] ReadAllLines();
        /*Read given number of lines
        */
        string[] ReadLines(int lineNum);
        /*Skip given number of words
        */
        void Skip(int charNum);
        /*Skip given number of lines
        */
        void SkipByLine(int lineNum);
    }
}
