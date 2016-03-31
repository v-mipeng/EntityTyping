using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using pml.type;

namespace msra.nlp.tr
{
    class SVMFeature : Feature
    {
        List<string> feature = new List<string>();
        int offset = 0;

        public SVMFeature() : base() 
        {

        }


        /// <summary>
        /// Reture feature dimension
        /// For sparse expression
        /// </summary>
        public int FeatureDimension
        {
            get
            {
                return offset;
            }
            private set { }
        }


        /// <summary>
        ///  Extract feature from the input, and the feature is clustered by field
        /// </summary>
        /// <param name="Event">
        /// An Envent with features of query.
        /// </param>
        /// <returns>
        /// A list of features including: Please refer Event to get the order of features
        ///     Mention words  
        ///     Mention shapes
        ///     Mention word cluster IDs
        ///     Mention length         
        ///     Mention cluster ID                      
        ///     Last token
        ///     Last token pos tag
        ///     Last token cluster ID                   
        ///     Next token
        ///     Next token pos tag
        ///     Next token cluster ID                   
        ///     Dictionary                      :Dbpedia
        ///     Topic(Define topic)             :MI keyword
        /// </returns>
        public List<string> ExtractFeature(Event e)
        {

            this.feature.Clear();
            this.offset = 0;
            var rawFeature = e.Feature;
            feature.Add("0");

            #region last word (make last word more accurate)
            if(useLastWord)
            {
                AddWordFieldToFeature(rawFeature.ElementAt(Parameter.GetFeatureIndex("lastWordStemmed")),
                    useWordTag ? rawFeature.ElementAt(Parameter.GetFeatureIndex("lastWordTag")):null,
                    useWordID ? rawFeature.ElementAt(Parameter.GetFeatureIndex("lastWordID")):null,
                    useWordShape ? rawFeature.ElementAt(Parameter.GetFeatureIndex("lastWordShape")):null);
            }
            #endregion

            #region next word
            if(useNextWord)
            {
                AddWordFieldToFeature(rawFeature.ElementAt(Parameter.GetFeatureIndex("nextWordStemmed")),
                    useWordTag ? rawFeature.ElementAt(Parameter.GetFeatureIndex("nextWordTag")) : null,
                    useWordID ? rawFeature.ElementAt(Parameter.GetFeatureIndex("nextWordID")) : null,
                    useWordShape ? rawFeature.ElementAt(Parameter.GetFeatureIndex("nextWordShape")) : null);
            }
            #endregion

            #region  mention head
            if(useMentionHead)
            {
                AddWordFieldToFeature(rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionHeadStemmed")),
                    useWordTag ? rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionHeadTag")) : null,
                    useWordID ? rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionHeadID")) : null,
                    useWordShape ? rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionHeadShape")) : null);

            }
            #endregion

            #region mention words
            if(useMentionSurfaces)
            {
                var dic = new Dictionary<int, int>();
                int value = 0;
                var dic2 = new SortedDictionary<int, int>();

                #region mention stemmed words

                string[] words = null;
                try
                {
                    words = rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionSurfacesStemmed")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    throw new Exception("Mention words is null");
                }
                foreach (var w in words) // words surface
                {
                    var index = offset + DataCenter.GetWordIndex(w);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                var keys = dic.Keys.ToList();
                keys.Sort();
                foreach (var key in keys)
                {
                    feature.Add(key + ":" + dic[key]);
                }
                offset += DataCenter.GetWordTableSize() + 1;
                dic.Clear();

                #endregion

                #region mention word tags
                if (useWordTag)
                {
                    string[] tags = null;
                    try
                    {
                        tags = rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionTags")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Mention tags is null");
                    }
                    foreach (var tag in tags)
                    {   // words pos tags
                        var index = offset + DataCenter.GetPosTagIndex(tag);
                        dic.TryGetValue(index, out value);
                        dic[index] = value + 1;
                    }
                    keys = dic.Keys.ToList();
                    keys.Sort();
                    foreach (var key in keys)
                    {
                        feature.Add(key + ":" + dic[key]);
                    }
                    offset += DataCenter.GetPosTagTableSize() + 1;
                    dic.Clear();
                }
                #endregion

                #region mention word IDs
                if (useWordID)
                {
                    string[] IDs = null;
                    try
                    {
                        IDs = rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionIDs")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Mention ids is null");
                    }
                    foreach (var ID in IDs) // words' cluster id
                    {
                        var index = offset + int.Parse(ID);
                        dic.TryGetValue(index, out value);
                        dic[index] = value + 1;
                    }
                    keys = dic.Keys.ToList();
                    keys.Sort();
                    foreach (var key in keys)
                    {
                        feature.Add(key + ":" + dic[key]);
                    }
                    offset += DataCenter.GetClusterNumber() + 1;
                    dic.Clear();
                }
                #endregion

                #region mention word shapes
                if (useWordShape)
                {
                    string[] shapes = null;
                    try
                    {
                        shapes = rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionShapes")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Mention shpaes is null");
                    }
                    foreach (var shape in shapes) // words shapes
                    {
                        var index = offset + DataCenter.GetWordShapeIndex(shape);
                        dic.TryGetValue(index, out value);
                        dic[index] = value + 1;
                    }
                    keys = dic.Keys.ToList();
                    keys.Sort();
                    foreach (var key in keys)
                    {
                        feature.Add(key + ":" + dic[key]);
                    }
                    offset += DataCenter.GetWordShapeTableSize() + 1;
                    dic.Clear();
                }
                #endregion
            }
            #endregion

            #region mention cluster id
            if(useMentionID)
            {
                var mentionID = int.Parse(rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionID")));
                feature.Add((offset + mentionID) + ":1");
                offset += DataCenter.GetMentionClusterNumber() + 1;
            }
            #endregion

            #region mention length: 1,2,3,4 or longer than 5
            if(useMentionLength)
            {
                var length = int.Parse(rawFeature.ElementAt(Parameter.GetFeatureIndex("mentionLength")));
                if (length > 5)
                {
                    length = 5;
                }
                feature.Add((offset + length - 1) + ":1");
                offset += 5;
            }
            #endregion

            #region DBpedia types
            {
                if (useDbpediaTypesWithIndegree)
                {
                    var types = rawFeature.ElementAt(Parameter.GetFeatureIndex("dbpediaTypesWithIndegree")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (types.Count() == 1 && !types[0].Contains(":"))
                    {
                        var index = DataCenter.GetDBpediaTypeIndex(types[0]);
                        feature.Add((offset + index) + ":1");
                    }
                    else
                    {
                        var dic = new Dictionary<int, string>();
                        foreach (var item in types)    // UNKNOW
                        {
                            var array = item.Split(':');
                            var type = array[0];
                            var distance = array[1];
                            if (distance.ToLower().Equals("nan"))
                            {
                                continue;
                            }
                            try
                            {
                                var index = DataCenter.GetDBpediaTypeIndex(type);
                                dic[index] = distance;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(item);
                                Console.WriteLine(type);
                            }
                        }
                        var indexes = dic.Keys.ToList();
                        indexes.Sort();
                        foreach (var index in indexes)
                        {
                            feature.Add((offset + index) + ":" + dic[index]);
                        }
                    }
                    offset += DataCenter.GetDBpediaTypeNum(); // the index of typeNum will never occur.
                }
                if (useDbpediaTypesWithAbstract)
                {
                    var types = rawFeature.ElementAt(Parameter.GetFeatureIndex("dbpediaTypesWithAbstract")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (types.Count() == 1 && !types[0].Contains(":"))
                    {
                        var index = DataCenter.GetDBpediaTypeIndex(types[0]);
                        feature.Add((offset + index) + ":1");
                    }
                    else
                    {
                        var dic = new Dictionary<int, string>();
                        foreach (var item in types)    // UNKNOW
                        {
                            var array = item.Split(':');
                            var type = array[0];
                            var distance = array[1];
                            var index = DataCenter.GetDBpediaTypeIndex(type);
                            dic[index] = distance;
                        }
                        var indexes = dic.Keys.ToList();
                        indexes.Sort();
                        foreach (var index in indexes)
                        {
                            feature.Add((offset + index) + ":" + dic[index]);
                        }
                    }
                    offset += DataCenter.GetDBpediaTypeNum(); // the index of typeNum will never occur.
                }
            }
            #endregion

            #region Key words
            if(useKeywords)
            {
                var keywords = rawFeature.ElementAt(Parameter.GetFeatureIndex("keywords")).Split(',');
                var list = new List<int>();
                foreach (var word in keywords)
                {
                    var index = DataCenter.GetKeyWordIndex(word);
                    list.Add(offset + index);
                }
                list.Sort();
                foreach (var index in list)
                {
                    feature.Add(index + ":1");
                }
                offset += DataCenter.GetKeyWordNumber();
            }
            #endregion

            //set feature dimension
            feature[0] = FeatureDimension.ToString();
            return feature;
        }


        private void AddWordFieldToFeature(string stemmedWord, string posTag, string ID, string shape)
        {
            if (stemmedWord != null)
            {
                // word surface
                feature.Add((offset + DataCenter.GetWordIndex(stemmedWord)) + ":1");
                offset += DataCenter.GetWordTableSize() + 1;
            }
            // word pos tag
            if (posTag != null)
            {
                feature.Add((offset + DataCenter.GetPosTagIndex(posTag)) + ":1");
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            // word Cluster id
            if (ID != null)
            {
                feature.Add((offset + int.Parse(ID)) + ":1");
                offset += DataCenter.GetClusterNumber() + 1;
            }
            // word shape
            if (shape != null)
            {
                feature.Add((offset + DataCenter.GetWordShapeIndex(shape)) + ":1");
                offset += DataCenter.GetWordShapeTableSize() + 1;
            }
        }

    }
}
