using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace package_pml.file.util
{
    public class FileChecker
    {
        private FileChecker() { }

        public static bool IsValidPath(String path)
        {
            char[] array = Path.GetInvalidPathChars();
            if(array.Count()>0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsFile(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool IsDir(String filePath)
        {
            return Directory.Exists(filePath);
        }



    }
}
