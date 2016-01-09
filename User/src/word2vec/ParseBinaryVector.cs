using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;
using pml.type;

namespace User.src
{
    class ParseBinaryVector
    {
        private BinaryReader reader = null;
        private int size = 0;
        private int dimension = 0;
        private int count = 0;


        // get the vocabular size
        public int Size
        {
            get
            {
                return size;
            }
        }

        // get the dimension the word vector
        public int Dimension
        {
            get
            {
                return dimension;
            }
        }

        // check if reach file end
        public bool EOF
        {
            get
            {
                return (reader.BaseStream.Position == reader.BaseStream.Length || count >= size);
            }
        }

        public ParseBinaryVector(string binaryWord2VectorFile)
        {
           this.reader = new BinaryReader(File.Open(binaryWord2VectorFile, FileMode.Open),Encoding.UTF8);
           try
           {
               GetTableInfo(reader);
           }
            catch(Exception)
           {
               Console.Error.WriteLine("Cannot parse the vocabulary information\rPlease check the format of the file:" + binaryWord2VectorFile);
           }
        }

        /// <summary>
        ///  Read one word and its corresponding vector
        /// </summary>
        /// <returns>
        ///  A pair with pair.first= word and pair.second = word's vector
        /// </returns>
        public Pair<string,List<float>> GetNextVector()
        {
            this.count++;
            try
            {
                char c;
                StringBuilder buffer = new StringBuilder();
                List<float> vector = new List<float>();

                // read word
                while ((c = reader.ReadChar()) != ' ')
                {
                    buffer.Append(c);
                }
                var word = buffer.ToString();
                // read vector
                for (int i = 0; i < dimension; i++)
                {
                    //vector.Add(reader.ReadDouble());
                    var bs = reader.ReadBytes(4);
                    vector.Add(BitConverter.ToSingle(bs, 0));
                }
                //var end = reader.ReadChar();// skip \n (However, for google's word2vec binary file, there is not \n)
                return new Pair<string, List<float>>(word, vector);
            }
            catch(Exception e)
            {
                if(EOF)
                {
                    throw new Exception("Reach the end of binary file");
                }
                else
                {
                    throw e;
                }
            }
        }

        private void GetTableInfo(BinaryReader reader)
        {
            // read word number and vector dimension
            char c;
            StringBuilder buffer = new StringBuilder();
            while ((c = reader.ReadChar()) != '\n')
            {
                buffer.Append(c);
            }
            var temp = buffer.ToString().Trim();
            var array = temp.Split(' ');
            this.size = int.Parse(array[0]);
            this.dimension = int.Parse(array[1]);
        }
    }
}
