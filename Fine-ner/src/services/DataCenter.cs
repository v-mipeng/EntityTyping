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
        #region Word Surface Table

        /*The word table of the train data
         */ 
        static Dictionary<String, int> word2index = null;
        static Object wordTableLocker = new object();
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
            }
            catch (Exception)
            {
                return word2index.Count;
            }
        }

        private static void LoadWordTable()
        {
            lock (wordTableLocker)
            {
                if (word2index == null)
                {
                    FileReader reader = null;
                    reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.word_table_file));
                    String line;
                    var dic = new Dictionary<string, int>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        try
                        {
                            var count = dic.Count;
                            dic[array[0]] = count;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    word2index = dic;
                }
            }
        }

        #endregion

        #region Word Shpae Table

        static Dictionary<string, int> wordShape2index = null;
        static object wordShapeLocker = new object();
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
            lock (wordTableLocker)
            {
                if (wordShape2index == null)
                {
                    var dic = new Dictionary<string, int>();

                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.word_shape_table_file));
                    String line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            var count = dic.Count;
                            dic[line] = count;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    wordShape2index = dic;
                }
            }
        }

        #endregion

        #region Pos Tag Table
        static Dictionary<string, int> posTag2index = null;
        static object posTagLocker = new object();
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
            lock (posTagLocker)
            {
                if (posTag2index == null)
                {
                    var dic = new Dictionary<string, int>();

                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.posTag_table_file));
                    String line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            var count = dic.Count;
                            dic[line] = count;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    posTag2index = dic;
                }
            }
        }

        #endregion

        #region Dictionary Like UIUC

        private static Dictionary<String, int> dicTypeMap = null;
        private static Dictionary<String, List<String>> dics = null;
        static object dicLocker = new object();

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
            lock (dicLocker)
            {
                if (dicTypeMap == null)
                {
                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.dic_file));
                    String line;
                    List<String> list;
                    dics = new Dictionary<string, List<string>>();
                   var dic = new Dictionary<String, int>();
                    HashSet<String> set = new HashSet<String>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        list = line.Split('\t').ToList();
                        List<String> strs = list.GetRange(1, list.Count - 1);
                        dics[list[0]] = strs;
                        strs.ForEach(x => set.Add(x));
                    }
                    foreach (var type in set)
                    {
                        dic[type] = dic.Count;
                    }
                    reader.Close();
                    dicTypeMap = dic;
                }
            }
        }

        #endregion

        #region Dictionary like people name list

        private static HashSet<String> fullNameSet = null;
        private static HashSet<String> partNameSet = null;

        /*Is the mention match an item within the name list entirely
         */
        public static bool IsFullNameMatch(String name)
        {
            if (fullNameSet == null)
            {
                LoadNameSet();
            }
            if (fullNameSet.Contains(name))
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

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split(' ');
                fullNameSet.Add(line);
                foreach (var x in array)
                {
                    partNameSet.Add(x);
                }
            }
            reader.Close();
        }

        /*Proposition list
         */
        static HashSet<String> prepositions = null;

        public static bool IsPreposition(String word)
        {
            if (prepositions == null)
            {
                LoadPreposition();
            }
            if (prepositions.Contains(word))
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

            while ((line = reader.ReadLine()) != null)
            {
                prepositions.Add(line);
            }
        }

        #endregion

        #region Stemmed Word Table

        private static object stemmerLocker = new object();

        private static Dictionary<string, string> stemWordDic = null; 

        public static string GetStemmedWord(string word)
        {
                if (stemWordDic == null)
                {
                    LoadStemMap();
                }
                string stemmedWord;
                if (!stemWordDic.TryGetValue(word, out stemmedWord))
                {
                    var stemmer = StemmerPool.GetStemmer();
                    stemmedWord = stemmer.Stem(word)[0];
                    StemmerPool.ReturnStemmer(stemmer);
                    stemWordDic[word] = stemmedWord;
                }
                return stemmedWord;
        }

        private static void LoadStemMap()
        {
            lock (stemmerLocker)
            {
                if (stemWordDic == null)
                {
                    var dic = new Dictionary<string, string>();
                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.stem_map));
                    string line;
                    string[] array;

                    while ((line = reader.ReadLine()) != null)
                    {
                        array = line.Split('\t');
                        try
                        {
                            dic[array[0]] = array[1];
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    stemWordDic = dic;
                }
            }
        }

        public static void RefreshStemDic(string des =null)
        {
            lock(stemmerLocker)
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
        }

        #endregion

        #region Word Cluster ID

        static Dictionary<string, int> wordIdDic = null;
        static int wordClusterSize = 0;
        static object wordIDLocker = new object();

        /// <summary>
        /// Get the cluster id of a word
        /// The id of words begin with 0. If the word table does not contains the word, it return number of clusters.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetWordClusterID(string word)
        {
                if (wordIdDic == null)
                {
                    LoadWordClusterID();
                }
                int id;
                wordIdDic.TryGetValue(word, out id);
                try
                {
                    id = wordIdDic[word];
                }
                catch (Exception)
                {
                    id = wordClusterSize;
                }
                return id;
        }

        public static int GetClusterNumber()
        {
                if (wordIdDic == null)
                {
                    LoadWordClusterID();
                }
                return wordClusterSize;
        }

        private static void LoadWordClusterID()
        {
            lock (wordIDLocker)
            {
                if (wordIdDic == null)
                {
                   var dic = new Dictionary<string, int>();
                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.word_id_file));
                    string line;
                    string[] array;
                    HashSet<int> ids = new HashSet<int>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        array = line.Split('\t');
                        try
                        {
                            var id = int.Parse(array[1]);
                            ids.Add(id);
                            dic[array[0]] = id;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    wordClusterSize = ids.Count;
                    wordIdDic = dic;
                }
            }
        }

        #endregion

        #region Mention Cluster ID
        static Dictionary<string, int> mentionIdDic = null;
        static int mentionClusterSize = 0;
        static object mentionIDLocker = new object();
            

        /// <summary>
        /// Get the cluster id of a mention
        /// The id of mentions begin with 0. If the mention table does not contains the mention, it return number of clusters.
        /// </summary>
        /// <param name="mention"></param>
        /// <returns></returns>
        /// TODO: modify mention matching theory.
        
        public static int GetMentionClusterID(string mention)
        {
                if (mentionIdDic == null)
                {
                    LoadMentionClusterID();
                }
                int id;
                mentionIdDic.TryGetValue(mention, out id);
                try
                {
                    id = mentionIdDic[mention];
                }
                catch (Exception)
                {
                    id = mentionClusterSize;
                }
                return id;
        }

        public static int GetMentionClusterNumber()
        {
                if (mentionIdDic == null)
                {
                    LoadMentionClusterID();
                }
                return mentionClusterSize;
        }

        private static void LoadMentionClusterID()
        {
            lock (mentionIDLocker)
            {
                if (mentionIdDic == null)
                {
                    var dic = new Dictionary<string, int>();
                    FileReader reader = new LargeFileReader((string)GlobalParameter.Get(DefaultParameter.Field.mention_id_file));
                    string line;
                    string[] array;
                    HashSet<int> ids = new HashSet<int>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        array = line.Split('\t');
                        try
                        {
                            var id = int.Parse(array[1]);
                            ids.Add(id);
                            dic[array[0]] = id;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    reader.Close();
                    mentionClusterSize = ids.Count;
                    mentionIdDic = dic;
                }
            }
        }

        #endregion

        private DataCenter() { }
    }
}
