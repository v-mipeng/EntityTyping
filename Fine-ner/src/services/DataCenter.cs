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
    public class DataCenter
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
                    reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.word_table_file));
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

                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.word_shape_table_file));
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

                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.posTag_table_file));
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

        public static List<String> GetTypeInDic(String mention)
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
                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.dic_file));
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
            FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.name_list_file));
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
                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.stem_map));
                    //FileReader reader = new LargeFileReader(@"D:\Codes\Project\EntityTyping\Fine-ner\input\tables\stem-word-table.txt");
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

        public static void RefreshStemDic(string des = null)
        {
            lock (stemmerLocker)
            {
                if (stemWordDic != null)
                {
                    if (des == null)
                    {
                        des = (string)Parameter.GetParameter(Parameter.Field.stem_map);
                    }
                    if (stemWordDic == null) return;
                    FileWriter writer = new LargeFileWriter(des, FileMode.Create);

                    foreach (var word in stemWordDic.Keys)
                    {
                        writer.WriteLine(word + "\t" + stemWordDic[word]);
                    }
                    writer.Close();
                    stemWordDic = null;
                }
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
                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.word_id_file));
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
        /// Get the cluster id of a mention. Mention should be seperated by "_"
        /// The id of mentions begin with 0.Return cluster number if mention does not exist in the mention table.
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
            try
            {
                id = mentionIdDic[mention];
                return id;
            }
            catch (Exception)
            {
                //var mentions = GetRedirect(mention);
                //foreach (var m in mentions)
                //{
                //    try
                //    {
                //        id = mentionIdDic[m];
                //    }
                //    catch
                //    {
                //        return mentionClusterSize;
                //    }
                //}
                return mentionClusterSize;
            }
        }

        public static int GetMentionClusterNumber()
        {
            if (mentionIdDic == null)
            {
                LoadMentionClusterID();
            }
            return mentionClusterSize;
        }

        /// <summary>
        /// Mention words are seperated by "_"
        /// </summary>
        private static void LoadMentionClusterID()
        {
            lock (mentionIDLocker)
            {
                if (mentionIdDic == null)
                {
                    var dic = new Dictionary<string, int>();
                    FileReader reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.mention_id_file));
                    string line;
                    string[] array;
                    HashSet<int> ids = new HashSet<int>();
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"_+");

                    while ((line = reader.ReadLine()) != null)
                    {
                        array = line.Split('\t');
                        try
                        {
                            var id = int.Parse(array[1]);
                            ids.Add(id);
                            array[0] = regex.Replace(array[0], " ");
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

        #region Stanford Ner system

        static Dictionary<string, int> stanfordNerTypeDic = null;
        static int stanfordNerTypeNum = 0;
        static object stanfordNerLocker = new object();
        static string[] stanfordNerTypes = new string[] { "PERSON", "LOCATTION", "ORGANIZATION", "UNKNOW" };

        /// <summary>
        /// Get the cluster id of a mention
        /// The id of mentions begin with 0. If the mention table does not contains the mention, it return number of clusters.
        /// </summary>
        /// <param name="mention"></param>
        /// <returns></returns>
        /// TODO: modify mention matching theory.

        public static int GetStanfordTypeIndex(string nerType)
        {
            if (stanfordNerTypeDic == null)
            {
                lock (stanfordNerLocker)
                {
                    if (stanfordNerTypeDic == null)
                    {
                        var dic = new Dictionary<string, int>();
                        foreach (var type in stanfordNerTypes)
                        {
                            dic[type] = dic.Count;
                        }
                        stanfordNerTypeDic = dic;
                    }
                }
            }
            int id;
            stanfordNerTypeDic.TryGetValue(nerType, out id);
            try
            {
                id = stanfordNerTypeDic[nerType];
            }
            catch (Exception)
            {
                id = stanfordNerTypeDic.Count;
            }
            return id;
        }

        public static int GetStanfordNerNumber()
        {
            if (stanfordNerTypeDic == null)
            {
                lock (stanfordNerLocker)
                {
                    if (stanfordNerTypeDic == null)
                    {
                        var dic = new Dictionary<string, int>();
                        foreach (var type in stanfordNerTypes)
                        {
                            dic[type] = dic.Count;
                        }
                        stanfordNerTypeDic = dic;
                    }
                }
            }
            return stanfordNerTypeDic.Count;
        }

        #endregion

        #region OpenNLP Ner system

        static Dictionary<string, int> openNLPNerTypeDic = null;
        static int openNLPNerTypeNum = 0;
        static object openNLPNerLocker = new object();
        //static string[] stanfordNerTypes = new string[] { "PERSON", "LOCATTION", "ORGANIZATION", "UNKNOW" };

        /// <summary>
        /// Get the cluster id of a mention
        /// The id of mentions begin with 0. If the mention table does not contains the mention, it return number of clusters.
        /// </summary>
        /// <param name="mention"></param>
        /// <returns></returns>
        /// TODO: modify mention matching theory.

        public static int GetOpenNLPTypeIndex(string nerType)
        {
            if (openNLPNerTypeDic == null)
            {
                lock (openNLPNerLocker)
                {
                    if (openNLPNerTypeDic == null)
                    {
                        var dic = new Dictionary<string, int>();
                        foreach (var type in stanfordNerTypes)
                        {
                            dic[type] = dic.Count;
                        }
                        openNLPNerTypeDic = dic;
                    }
                }
            }
            int id;
            openNLPNerTypeDic.TryGetValue(nerType, out id);
            try
            {
                id = openNLPNerTypeDic[nerType];
            }
            catch (Exception)
            {
                id = openNLPNerTypeDic.Count;
            }
            return id;
        }

        public static int GetOpenNLPNerNumber()
        {
            if (openNLPNerTypeDic == null)
            {
                lock (openNLPNerLocker)
                {
                    if (openNLPNerTypeDic == null)
                    {
                        var dic = new Dictionary<string, int>();
                        foreach (var type in stanfordNerTypes)
                        {
                            dic[type] = dic.Count;
                        }
                        openNLPNerTypeDic = dic;
                    }
                }
            }
            return openNLPNerTypeDic.Count;
        }

        #endregion

        #region DBpedia dictionary     (mention should be lowercase and without space)

        static Dictionary<string, string> dbpediaEntity2Type = null;                   // entity type in dbpedia  (serve for indivisual feature); trimed entity -->(type<-->untrimed entity)
        static Dictionary<string, int> dbpediaType2index = null;                                                // mapping dbpedia type to integer
        static Dictionary<string, List<string>> disambiguousDic = null;                                         // 
        static Dictionary<string, List<string>> pageAnchorsDic = null;                                          // recoding page anchors of articles
        static Dictionary<string, Dictionary<int, double>> pageAbstract = null;                                 // page absctract: sparse vector
        static Dictionary<string, int> pageIndegree = null;


        static object dbpediaDicLocker = new object();
        static object dbpediaType2IndexLocker = new object();
        static object disambiguousLocker = new object();
        static object pageAnchorLocker = new object();
        static object pageAbstractLocker = new object();
        static object pageIndegreeLocker = new object();


        /// <summary>
        /// Get mention's type in dbpedia database.
        /// </summary>
        /// <param name="mention">
        /// The queried mention. Mention can contain space.
        /// </param>
        /// <returns></returns>
        public static List<string> GetDBpediaType(string mention, string context = null)
        {

            if (dbpediaEntity2Type == null)
            {
                LoadDBpediaType();
            }
            var list = new List<string>();
            if (mention != null)
            {
                mention = TransformQuery(mention); // preprocess mention to meet the format of dbpedia data.
                var redirects = GetRedirect(mention);
                if (redirects == null)
                {
                    redirects = new HashSet<string>();
                    redirects.Add(mention);
                }
                else
                {
                    redirects.Add(mention);
                }
                int totalIndegree = 0;
                var types = new List<string>();  // normalize indegree
                var indegrees = new List<int>();
                foreach (var m in redirects)
                {
                    try
                    {
                        var indegree = GetPageIndegree(m);  // weight by indegree
                        var type = dbpediaEntity2Type[m];
                        types.Add(type);
                        indegrees.Add(indegree);
                        totalIndegree += indegree;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                for (var i = 0; i < types.Count; i++)
                {
                    list.Add(types[i] + ":" + (1.0 * indegrees[i] / totalIndegree));
                }
                if (list.Count > 0)
                {
                    return list;
                }
                else
                {
                    list.Add("UNKNOW");
                    return list;
                }
            }
            else
            {
                throw new Exception("Mention is null for finding dbpedia type!");
            }
        }

        public static List<string> GetDBpediaTypeWithIndegree(string mention)
        {
            if (dbpediaEntity2Type == null)
            {
                LoadDBpediaType();
            }
            var list = new List<string>();
            var maxIndegree = -1;
            var types = new List<string>();
            var indegrees = new List<int>();
            if (mention != null)
            {
                mention = TransformQuery(mention); // preprocess mention to meet the format of dbpedia data.
                var redirects = GetRedirect(mention);
                if (redirects == null)
                {
                    redirects = new HashSet<string>();
                    redirects.Add(mention);
                }
                else
                {
                    redirects.Add(mention);
                }
                foreach (var m in redirects)
                {
                    try
                    {
                        var indegree = GetPageIndegree(m);
                        if(indegree == 0)
                        {
                            continue;
                        }
                        var type = dbpediaEntity2Type[m];
                        if (indegree > maxIndegree)
                        {
                            maxIndegree = indegree;
                        }
                        types.Add(type);
                        indegrees.Add(indegree);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                if (maxIndegree == 0)
                {
                    Console.WriteLine("max indegree is 0!");
                    Console.ReadKey();
                }
                for (var i = 0; i < types.Count; i++)
                {
                    list.Add(types[i] + ":" + (1.0 * indegrees[i] / maxIndegree));
                }

                if (list.Count > 0)
                {
                    return list;
                }
                else
                {
                    list.Add("UNKNOW");
                    return list;
                }
            }
            else
            {
                throw new Exception("Mention is null for finding dbpedia type!");
            }
        }

        public static List<string> GetDBpediaTypeWithAbstract(string mention, string context)
        {
            if (dbpediaEntity2Type == null)
            {
                LoadDBpediaType();
            }
            var list = new List<string>();
            if (mention != null)
            {
                mention = TransformQuery(mention); // preprocess mention to meet the format of dbpedia data.
                var redirects = GetRedirect(mention);
                if (redirects == null)
                {
                    redirects = new HashSet<string>();
                    redirects.Add(mention);
                }
                else
                {
                    redirects.Add(mention);
                }
                var maxValue = -1.0;
                var contextVec = TfIdf.GetDocTfIdf(context.Split(' ').ToList());
                var types = new List<string>();
                var matchValues = new List<double>();
                foreach (var entity in redirects)
                {
                    try
                    {
                        var type = dbpediaEntity2Type[entity];
                        if (redirects.Count == 1)
                        {
                            types.Add(type);
                            matchValues.Add(1);
                            maxValue = 1;
                            break;
                        }
                        var entityVec = GetPageAbstract(entity);
                        if (entityVec == null)
                        {
                            continue;
                        }
                        var matchValue = pml.math.VectorDistance.SparseCosinDistance(contextVec, entityVec);
                        if(matchValue == 0)
                        {
                            continue;
                        }
                        if (maxValue < matchValue)
                        {
                            maxValue = matchValue;
                        }
                        types.Add(type);
                        matchValues.Add(matchValue);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                for (var i = 0; i < types.Count; i++)
                {
                    list.Add(types[i] + ":" + (matchValues[i] / maxValue));
                }
                if (list.Count > 0)
                {
                    return list;
                }
                else
                {
                    list.Add("UNKNOW");
                    return list;
                }
            }
            else
            {
                throw new Exception("Mention is null for finding dbpedia type!");
            }

        }


        /// <summary>
        /// Get the index of type in the dbpedia type set.
        /// Throw exception if type does not exist in dbpedia type set
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetDBpediaTypeIndex(string type)
        {
            if (dbpediaType2index == null)
            {
                ConstructTypeIndex();
            }
            try
            {
                return dbpediaType2index[type];
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get dbpeida type number
        /// </summary>
        /// <returns></returns>
        public static int GetDBpediaTypeNum()
        {

            if (dbpediaType2index == null)
            {
                ConstructTypeIndex();
            }
            return dbpediaType2index.Count;
        }

        public static int GetPageIndegree(string page)
        {
            if (pageIndegree == null)
            {
                LoadPageIndegree();
            }
            int indegree = 0;
            pageIndegree.TryGetValue(page, out indegree);
            return indegree;
        }

        #region Page Anchor: TODO

        /// <summary>
        /// Get possible entities of given mention.
        /// </summary>
        /// <param name="mention"></param>
        /// <returns></returns>
        private static List<string> GetAmbiguousEntities(string mention)
        {
            if (disambiguousDic == null)
            {
                LoadDisambiguous();
            }
            try
            {
                return disambiguousDic[mention];
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Find the exatcly entity from the ambiguous entities with context information.
        /// This is done by compare anchors within a page corresponding to the given entity with context.
        /// Return the entity which match most achors with context.
        /// </summary>
        /// <param name="ambiguousEntities"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string Disambiguation(List<string> ambiguousEntities, string context)
        {
            var anchorMatchNum = new int[ambiguousEntities.Count];
            for (var i = 0; i < ambiguousEntities.Count; i++)
            {
                var anchors = GetPageAnchors(ambiguousEntities[i]);
                if (anchors != null)
                {
                    foreach (var anchor in anchors)
                    {
                        if (context.Contains(anchor))
                        {
                            anchorMatchNum[i] += 1;
                        }
                    }
                }
            }
            var index = 0;
            var maxNum = -1;
            for (var i = 0; i < anchorMatchNum.Length; i++)
            {
                if (maxNum < anchorMatchNum[i])
                {
                    maxNum = anchorMatchNum[i];
                    index = i;
                }
            }
            return ambiguousEntities[index];
        }

        /// <summary>
        /// Get anchors in page abstract corresponding to the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static List<string> GetPageAnchors(string entity)
        {
            if (pageAnchorsDic == null)
            {
                LoadPageAnchors();
            }
            try
            {
                return pageAnchorsDic[entity];
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
        private static Dictionary<int, double> GetPageAbstract(string entity)
        {
            if (pageAbstract == null)
            {
                LoadPageAbstract();
            }
            try
            {
                return pageAbstract[entity];
            }
            catch (Exception)
            {
                return null;
            }
        }

        static private void ConstructTypeIndex()
        {
            lock (dbpediaType2IndexLocker)
            {
                if (dbpediaType2index == null)
                {
                    var dic = new Dictionary<string, int>();
                    if (dbpediaEntity2Type == null)
                    {
                        LoadDBpediaType();
                    }
                    var set = new HashSet<string>();
                    foreach (var type in dbpediaEntity2Type.Values)
                    {
                        set.Add(type);
                    }
                    foreach (var type in set)
                    {
                        dic[type] = dic.Count;
                    }
                    dic["UNKNOW"] = dic.Count;
                    dbpediaType2index = dic;
                }
            }
        }

        public static void LoadDBpediaType()
        {
            lock (dbpediaDicLocker)
            {
                if (dbpediaEntity2Type == null)
                {
                    var dic = new Dictionary<string, string>();
                    LinkedList<Pair<string, string>> types = null;             // type<-->entity surface
                    var reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.dbpedia_type_file));
                    var line = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');                       // array[0]: entity array[1]:type              
                        dic[array[0]] = array[1];
                    }
                    reader.Close();
                    dbpediaEntity2Type = dic;
                }
            }
        }

        private static void LoadPageAnchors()
        {
            lock (pageAnchorLocker)
            {
                if (pageAnchorsDic == null)
                {
                    var reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.page_anchor_file));
                    var line = "";
                    var dic = new Dictionary<string, List<string>>();
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"_+");

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var list = array.ToList();   // TODO: delete space(_)
                        dic[array[0]] = list;       // Depend on file format
                        list.RemoveAt(0);
                    }
                    reader.Close();
                    pageAnchorsDic = dic;
                }
            }
        }

        private static void LoadDisambiguous()
        {
            lock (disambiguousLocker)
            {
                if (disambiguousDic == null)
                {
                    var dic = new Dictionary<string, List<string>>();
                    var reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.disambiguous_file));
                    var line = "";
                    System.Text.RegularExpressions.Regex deleteUnderline = new System.Text.RegularExpressions.Regex(@"_+");

                    while ((line = reader.ReadLine()) != null)
                    {
                        var l = deleteUnderline.Replace(line, "");
                        var array = line.Split('\t').ToList();
                        dic[array[0]] = array;
                        array.RemoveAt(0);
                    }
                    reader.Close();
                    disambiguousDic = dic;
                }
            }
        }

        private static void LoadPageAbstract()
        {
            lock (pageAbstractLocker)
            {
                if (pageAbstract == null)
                {
                    var path = (string)Parameter.GetParameter(Parameter.Field.dbpedia_abstract_file);
                    var dic = new Dictionary<string, Dictionary<int, double>>();
                    var reader = new LargeFileReader(path);
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var array = line.Split('\t');
                        var dic2 = new Dictionary<int, double>();
                        for (var i = 1; i < array.Length; i++)
                        {
                            var array2 = array[i].Split(':');
                            dic2[int.Parse(array2[0])] = double.Parse(array2[1]);
                        }
                        array[0] = array[0].ToLower();
                        dic[array[0]] = dic2;
                    }
                    reader.Close();
                    pageAbstract = dic;
                }
            }
        }

        private static void LoadPageIndegree()
        {
            lock (pageIndegreeLocker)
            {
                if (pageIndegree == null)
                {
                    var source = (string)Parameter.GetParameter(Parameter.Field.page_indegree_file);
                    var reader = new LargeFileReader(source);
                    var dic = new Dictionary<string, int>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');                       // array[0]: entity array[1]:type 
                        int indegree = LogIndegree(int.Parse(array[1]));
                        if (dic.ContainsKey(array[0]))
                        {
                            if (dic[array[0]] < indegree)
                            {
                                dic[array[0]] = indegree;
                            }
                        }
                        else
                        {
                            dic[array[0]] = indegree;
                        }
                    }
                    reader.Close();
                    pageIndegree = dic;
                }
            }
        }

        private static int LogIndegree(int indegree)
        {
            if(indegree == 0)
            {
                return 0;
            }
            var value = (int)Math.Ceiling(Math.Log(indegree));
            if(value == 0)
            {
                value = 1;
            }
            if (value > 12)
            {
                value = 12;
            }
            return value;
        }

        /// <summary>
        /// Tranform the format of query making if meet the requirement of dbpedia data.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string TransformQuery(string input)
        {
            input = input.ToLower().Replace("-lrb-", "(");   // recover input
            input = input.Replace("-rrb-", ")");
            input = input.Replace(" ", "_");
            return input;
        }

        #endregion

        #region DBpedia redirects

        static Dictionary<string, HashSet<string>> redirects = null;
        static object redirectLocker = new object();

        private static HashSet<string> GetRedirect(string mention)
        {
            if (redirects == null)
            {
                LoadRedirect();
            }
            try
            {
                return redirects[mention];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void LoadRedirect()
        {
            lock (redirectLocker)
            {
                if (redirects == null)
                {
                    var dic = new Dictionary<string, HashSet<string>>();
                    var set = new HashSet<string>();
                    var reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.dbpedia_redirect_file));
                    var line = "";
                    System.Text.RegularExpressions.Regex braceRegex = new System.Text.RegularExpressions.Regex(@"\(\w+\)");

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');

                        if (dic.TryGetValue(array[0], out set))
                        {
                            set.Add(array[1]);
                        }
                        else
                        {
                            set = new HashSet<string>();
                            set.Add(array[1]);
                            dic[array[0]] = set;
                        }
                    }
                    reader.Close();
                    reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.dbpedia_type_file));
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.ToLower();
                        var array = line.Split('\t');
                        var temp = braceRegex.Replace(array[0], "");
                        if (array[0].Length != temp.Length)
                        {
                            if (dic.TryGetValue(temp, out set))
                            {
                                set.Add(array[0]);
                            }
                            else
                            {
                                set = new HashSet<string>();
                                set.Add(array[0]);
                                dic[temp] = set;
                            }
                        }
                    }
                    reader.Close();
                    redirects = dic;
                }
            }
        }

        #endregion

        #region Keywords Table
        static Dictionary<string, int> keyWords = null;
        static object keyWordLocker = new object();

        static System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\b[\w]{2,}\b");

        private static List<string> Tokenize(string sequence)
        {
            var matchCollection = regex.Matches(sequence);
            List<string> list = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                list.Add(match.Groups[0].Value);
            }
            return list;
        }


        public static List<string> ExtractKeyWords(string context)
        {
            return ExtractKeyWords(Tokenize(context));
        }

        public static List<string> ExtractKeyWords(List<string> tokens)
        {
            if (keyWords == null)
            {
                LoadKeyWords();
            }
            var set = new HashSet<string>();
            foreach (var token in tokens)
            {
                var tokenStemmed = Generalizer.Generalize(token);
                if (keyWords.ContainsKey(tokenStemmed))
                {
                    set.Add(tokenStemmed);
                }
            }
            if (set.Count == 0)
            {
                set.Add("NONE");
            }
            return set.ToList();
        }

        /// <summary>
        /// If word is not key word, throw error.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static int GetKeyWordIndex(string word)
        {
            if (keyWords == null)
            {
                LoadKeyWords();
            }
            try
            {
                return keyWords[word];
            }
            catch (Exception)
            {
                throw new Exception(word + " is not key word!");
            }
        }

        public static int GetKeyWordNumber()
        {
            if (keyWords == null)
            {
                LoadKeyWords();
            }
            return keyWords.Count;
        }

        private static void LoadKeyWords()
        {
            lock (keyWordLocker)
            {
                if (keyWords == null)
                {
                    var reader = new LargeFileReader((string)Parameter.GetParameter(Parameter.Field.keyword_file));
                    var line = "";
                    var dic = new Dictionary<string, int>();
                    var token = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!dic.ContainsKey((token = line.Trim())))
                        {
                            dic[token] = dic.Count;
                        }
                    }
                    reader.Close();
                    dic["NONE"] = dic.Count;
                    keyWords = dic;
                }
            }
        }



        #endregion
        private DataCenter() { }
    }
}
