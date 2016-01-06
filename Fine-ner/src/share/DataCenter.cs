using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using pml.file.reader;
using pml.file.writer;
using pml.file;
using pml.type;

namespace msra.nlp.tr
{
    class DataCenter
    {
        /************************************************************************/
        /* Word table data                                                                     */
        /************************************************************************/
        /*The word table of the train data
         */ 
        static Dictionary<String, int> word2index = null;

        /*Get size of the word table
         */ 
        public static int GetWordTableSize()
        {
            if (word2index == null)
            {
                LoadWordTable();
            }
            return word2index.Count;
        }

        public static bool InsertToWordTable(Pair<String,int> pair)
        {
            if(word2index == null)
            {
                LoadWordTable();
            }
            if(word2index.ContainsKey(pair.first))
            {
                return false;
            }
            else
            {
                word2index[pair.first] = pair.second;
                return true;
            }
        }

        /// <summary>
        /// Get word index in word table
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetWordIndex(String word)
        {
            if (word2index == null)
            {
                LoadWordTable();
            }
            int index;
            try
            {
                index = word2index[word];
                return index;
            }catch(Exception)
            {
                return word2index.Count;
            }
        }

        private static void LoadWordTable()
        {
            FileReader reader = null;
            reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.word_table_file));
            String line;
            word2index = new Dictionary<string, int>();

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var count = word2index.Count;
                    word2index[line] = count;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            reader.Close();
        }

        /************************************************************************/
        /* Word shpae table                                                                     */
        /************************************************************************/

        static Dictionary<string, int> wordShape2index = null;
        /*Get size of the word table
        */
        public static int GetWordShapeTableSize()
        {
            if (wordShape2index == null)
            {
                LoadWordShapeTable();
            }
            return wordShape2index.Count;
        }


        /// <summary>
        /// Get word shape index in word shpae table
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetWordShapeIndex(String shape)
        {
            if (wordShape2index == null)
            {
                LoadWordShapeTable();
            }
            int index;
            try
            {
                return wordShape2index[shape];
            }
            catch (Exception)
            {
                return wordShape2index.Count;
            }
        }

        private static void LoadWordShapeTable()
        {
            wordShape2index = new Dictionary<string, int>();

            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.shape_table_file));
            String line;

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var count = wordShape2index.Count;
                    wordShape2index[line] = count;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            reader.Close();
        }

        /************************************************************************/
        /* Pos tag table                                                                     */
        /************************************************************************/

        static Dictionary<string, int> posTag2index = null;
        /*Get size of the word table
        */
        public static int GetPosTagTableSize()
        {
            if (posTag2index == null)
            {
                LoadPosTagTable();
            }
            return posTag2index.Count;
        }


        /// <summary>
        /// Get word shape index in word shpae table
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetPosTagIndex(String posTag)
        {
            if (posTag2index == null)
            {
                LoadPosTagTable();
            }
            try
            {
                return posTag2index[posTag];
            }
            catch (Exception)
            {
                return posTag2index.Count;
            }
        }

        private static void LoadPosTagTable()
        {
            posTag2index = new Dictionary<string, int>();

            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.posTag_table_file));
            String line;

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var count = posTag2index.Count;
                    posTag2index[line] = count;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            reader.Close();
        }

        /************************************************************************/
        /* Dictionary like UIUC data                                                                     */
        /************************************************************************/
        private static Dictionary<String, int> dicTypeMap = null;
        private static Dictionary<String, List<String>> dics = null;

        public static  List<String> GetTypeInDic(String mention)
        {
            if (dics == null)
            {
                LoadDictionary();
            }
            List<String> types;
            return dics.TryGetValue(mention, out types) ? types : null;
        }

        /** Get the map of mention-->types
         */ 
        public static Dictionary<String, int> GetDicTyeMap()
        {
            if (dicTypeMap == null)
            {
                LoadDictionary();
            }
            return dicTypeMap;
        }

        /*Get type number within dictionary
         */ 
        public static int GetDicTypeNum()
        {
            if (dicTypeMap == null)
            {
                LoadDictionary();
            }
            return dicTypeMap.Count;
        }

        /*Get the mapped int value of mention type 
         * Start with 0
         */ 
        public static int GetDicTypeValue(String type)
        {
            if (dicTypeMap == null)
            {
                LoadDictionary();
            }
            int value;
            return dicTypeMap.TryGetValue(type, out value) ? value : int.MinValue;
        }

        /*Read Dictionary from  file
         */ 
        private static void LoadDictionary()
        {
            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.dic_file));
            String line;
            List<String> list;
            dics = new Dictionary<string, List<string>>();
            dicTypeMap = new Dictionary<String, int>();
            HashSet<String> set = new HashSet<String>();

            while((line = reader.ReadLine()) != null)
            {
                list = line.Split('\t').ToList();
                List<String> strs = list.GetRange(1, list.Count - 1);
                dics[list[0]] = strs;
                strs.ForEach(x => set.Add(x));
            }
            foreach(var type in set)
            {
                dicTypeMap[type] = dicTypeMap.Count;
            }
            reader.Close();
        }

        /************************************************************************/
        /* Dictionary like people name list                                                                     */
        /************************************************************************/
        private static HashSet<String> fullNameSet = null;
        private static HashSet<String> partNameSet = null;

        /*Is the mention match an item within the name list entirely
         */ 
        public static   bool IsFullNameMatch(String name)
        {
            if(fullNameSet == null)
            {
                LoadNameSet();
            }
            if(fullNameSet.Contains(name))
            {
                return true;
            }
            return false;
        }

        /*Is the mention only part of a name within the name list
         */ 
        public static bool IsPartNameMatch(String name)
        {
            if (fullNameSet == null)
            {
                LoadNameSet();
            }
            if (partNameSet.Contains(name))
            {
                return true;
            }
            return false;
        }

        /*Read name list from file
         */ 
        private static void LoadNameSet()
        {
            fullNameSet = new HashSet<string>();
            partNameSet = new HashSet<string>();
            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.name_list_file));
            String line;
            String[] array;

            while((line = reader.ReadLine())!=null)
            {
                array = line.Split(' ');
                fullNameSet.Add(line);
                foreach(var x in array)
                {
                    partNameSet.Add(x);
                }
            }
            reader.Close();
        }

        /*Proposition list
         */
       static  HashSet<String> prepositions = null;

        public static bool IsPreposition(String word)
       {
            if(prepositions == null)
            {
                LoadPreposition();
            }
            if(prepositions.Contains(word))
            {
                return true;
            }
            return false;
       }

        private static void LoadPreposition()
        {
            prepositions = new HashSet<String>();
            FileReader reader = new LargeFileReader();
            String line;

            while((line = reader.ReadLine())!=null)
            {
                prepositions.Add(line);
            }
        }

        /************************************************************************/
        /* Stem data and object                                                                     */
        /************************************************************************/
        private static object locker = new object();

        private static Dictionary<string, string> stemWordDic = null; 

        public static string GetStemmedWord(string word)
        {
            lock (locker)
            {
                if (stemWordDic == null)
                {
                    LoadStemMap();
                }
                string stemmedWord;
                if (!stemWordDic.TryGetValue(word, out stemmedWord))
                {
                    stemmedWord = Stemmer.Stem(word)[0];
                    stemWordDic[word] = stemmedWord;
                }
                return stemmedWord;
            }
        }

        private static void LoadStemMap()
        {
            stemWordDic = new Dictionary<string, string>();
            FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.stem_map));
            string line;
            string[] array;

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                try
                {
                    stemWordDic[array[0]] = array[1];
                }    
                catch(Exception)
                {
                    continue;
                }
            }
            reader.Close();
        }

        public static void RefreshStemDic(string des =null)
        {
            if (des == null)
            {
                des = (string)DefaultParameter.Get(DefaultParameter.Field.stem_map);
                //des = (string)GlobalParameter.Get(DefaultParameter.stem_map);
            }
            if (stemWordDic == null) return;
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);

            foreach (var word in stemWordDic.Keys)
            {
                writer.WriteLine(word + "\t" + stemWordDic[word]);
            }
            writer.Close();
        }


        private DataCenter() { }
    }
}
