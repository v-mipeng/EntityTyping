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

        public SVMFeature() : base() { }


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


        /*   Extract feature from the input, and the feature is clustered by field
         *   The input should contains two items:
         *      Mention surface:   the surface text of the mention             // input[0]
         *      Mention context:   the context contains the mention         // input[1]
         *   The output are a list of pairs store the features' index and value:                                   
         *      Mention surface  
         *      Mention Shape
         *      Cluster ID of mention words     
         *      Mention length         
         *      Mention ID                      
         *      Last token
         *      Last token pos tag
         *      Last token ID                   
         *      Next token
         *      Next token pos tag
         *      Next token ID                   
         *      Parent in dependency tree(stanford corenlp) : driver, action, adject modifier(TO USE)  
         *      Dictionary                      :TODO
         *      Topic(Define topic)             :TODO: I am going to work with document cluster
         * 
         */
        public List<string> ExtractFeature(Event e)
        {

            this.feature.Clear();
            this.offset = 0;
            var rawFeature = e.Feature;
            feature.Add("0");

            #region last word (make last word more accurate)
            {
                AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.lastWordStemmed),
                    rawFeature.ElementAt((int)Event.Field.lastWordID),
                    rawFeature.ElementAt((int)Event.Field.lastWordShape),
                    rawFeature.ElementAt((int)Event.Field.lastWordTag));
            }
            #endregion

            #region next word
            {
                AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.nextWordStemmed),
                    rawFeature.ElementAt((int)Event.Field.nextWordID),
                    rawFeature.ElementAt((int)Event.Field.nextWordShape),
                    rawFeature.ElementAt((int)Event.Field.nextWordTag));
            }
            #endregion

            #region  mention head
            {
                AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.mentionHeadStemmed),
                     rawFeature.ElementAt((int)Event.Field.mentionHeadID),
                     rawFeature.ElementAt((int)Event.Field.mentionHeadShape),
                     rawFeature.ElementAt((int)Event.Field.mentionHeadTag));
            }
            #endregion

            if((bool)GlobalParameter.Get(DefaultParameter.Field.activateParser))
            {
                #region mention driver
                {
                    AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.mentionDriverStemmed),
                         rawFeature.ElementAt((int)Event.Field.mentionDriverID),
                         rawFeature.ElementAt((int)Event.Field.mentionDriverShape),
                         rawFeature.ElementAt((int)Event.Field.mentionDriverTag));
                }
                #endregion

                #region mention adjective modifer
                {
                    AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.mentionAdjModifierStemmed),
                          rawFeature.ElementAt((int)Event.Field.mentionAdjModifierID),
                          rawFeature.ElementAt((int)Event.Field.mentionAdjModifierShape),
                          rawFeature.ElementAt((int)Event.Field.mentionAdjModifierTag));

                }
                #endregion

                #region mention action
                {
                    AddWordFieldToFeature(rawFeature.ElementAt((int)Event.Field.mentionActionStemmed),
                            rawFeature.ElementAt((int)Event.Field.mentionActionID),
                            rawFeature.ElementAt((int)Event.Field.mentionActionShape),
                            rawFeature.ElementAt((int)Event.Field.mentionActionTag));
                }
                #endregion
            }

            #region mention words
            {
                string[] words = null;
                try
                {
                    words = rawFeature.ElementAt((int)Event.Field.mentionSurfacesStemmed).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    throw new Exception("Mention words is null");
                }
                string[] IDs = null;
                try
                {
                    IDs = rawFeature.ElementAt((int)Event.Field.mentionIDs).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    throw new Exception("Mention ids is null");
                }
                string[] shapes = null;
                try
                {
                    shapes = rawFeature.ElementAt((int)Event.Field.mentionShapes).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    throw new Exception("Mention shpaes is null");
                }
                string[] tags = null;
                try
                {
                    tags = rawFeature.ElementAt((int)Event.Field.mentionTags).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception)
                {
                    throw new Exception("Mention tags is null");
                }
                var dic = new Dictionary<int, int>();
                int value = 0;
                var dic2 = new SortedDictionary<int, int>();
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
            }
            #endregion

            #region mention cluster id
            {
                var mentionID = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionID));
                feature.Add((offset + mentionID) + ":1");
                offset += DataCenter.GetMentionClusterNumber() + 1;
            }
            #endregion

            #region mention length: 1,2,3,4 or longer than 5
            {
                var length = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionLength));
                if (length > 5)
                {
                    length = 5;
                }
                feature.Add((offset + length - 1) + ":1");
                offset += 5;
            }
            #endregion

            if ((bool)GlobalParameter.Get(DefaultParameter.Field.activateNer))
            {
                #region Stanford Ner system
                {
                    var stanfordNerType = rawFeature.ElementAt((int)Event.Field.stanfordNerType);
                    var index = DataCenter.GetStanfordTypeIndex(stanfordNerType);
                    feature.Add((offset + index) + ":1");
                    offset += DataCenter.GetStanfordNerNumber() + 1;
                }
                #endregion

                #region OpenNLP Ner system
                {
                    var openNLPNerType = rawFeature.ElementAt((int)Event.Field.opennlpNerType);
                    var index = DataCenter.GetOpenNLPTypeIndex(openNLPNerType);
                    feature.Add((offset + index) + ":1");
                    offset += DataCenter.GetOpenNLPNerNumber() + 1;
                }
                #endregion
            }

            if ((bool)GlobalParameter.Get(DefaultParameter.Field.activateDbpedia))
            {
                #region DBpedia types
                {
                    //var types = rawFeature.ElementAt((int)Event.Field.dbpediaTypesWithIndegree).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //if (types.Count() == 1 && !types[0].Contains(":"))
                    //{
                    //    var index = DataCenter.GetDBpediaTypeIndex(types[0]);
                    //    feature.Add((offset + index) + ":1");
                    //}
                    //else
                    //{
                    //    var dic = new Dictionary<int, string>();
                    //    foreach (var item in types)    // UNKNOW
                    //    {
                    //        var array = item.Split(':');
                    //        var type = array[0];
                    //        var distance = array[1];
                    //        try
                    //        {
                    //            var index = DataCenter.GetDBpediaTypeIndex(type);
                    //            dic[index] = distance;

                    //        }
                    //        catch(Exception)
                    //        {
                    //            Console.WriteLine(item);
                    //            Console.WriteLine(type);
                    //            Console.ReadKey();
                    //        }
                    //    }
                    //    var indexes = dic.Keys.ToList();
                    //    indexes.Sort();
                    //    foreach (var index in indexes)
                    //    {
                    //        feature.Add((offset + index) + ":" + dic[index]);
                    //    }
                    //}
                    //offset += DataCenter.GetDBpediaTypeNum(); // the index of typeNum will never occur.
                    //types = rawFeature.ElementAt((int)Event.Field.dbpediaTypesWithAbstract).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //if (types.Count() == 1 && !types[0].Contains(":"))
                    //{
                    //    var index = DataCenter.GetDBpediaTypeIndex(types[0]);
                    //    feature.Add((offset + index) + ":1");
                    //}
                    //else
                    //{
                    //    var dic = new Dictionary<int, string>();
                    //    foreach (var item in types)    // UNKNOW
                    //    {
                    //        var array = item.Split(':');
                    //        var type = array[0];
                    //        var distance = array[1];
                    //        var index = DataCenter.GetDBpediaTypeIndex(type);
                    //        dic[index] = distance;
                    //    }
                    //    var indexes = dic.Keys.ToList();
                    //    indexes.Sort();
                    //    foreach (var index in indexes)
                    //    {
                    //        feature.Add((offset + index) + ":" + dic[index]);
                    //    }
                    //}
                    //offset += DataCenter.GetDBpediaTypeNum(); // the index of typeNum will never occur.
                }
                #endregion
            }

            if ((bool)GlobalParameter.Get(DefaultParameter.Field.activateMIKeyword))
            {
                #region Key words
                {
                    var keywords = rawFeature.ElementAt((int)Event.Field.keyWords).Split(',');
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
            }

            //set feature dimension
            feature[0] = FeatureDimension.ToString();
            return feature;
        }


        private void AddWordFieldToFeature(string stemmedWord, string ID, string shape, string posTag)
        {
            if (stemmedWord != null)
            {
                // word surface
                feature.Add((offset + DataCenter.GetWordIndex(stemmedWord)) + ":1");
            }
            offset += DataCenter.GetWordTableSize() + 1;
            // word Cluster id
            if (ID != null)
            {
                feature.Add((offset + int.Parse(ID)) + ":1");
            }
            offset += DataCenter.GetClusterNumber() + 1;
            // word shape
            if (shape != null)
            {
                feature.Add((offset + DataCenter.GetWordShapeIndex(shape)) + ":1");
            }
            offset += DataCenter.GetWordShapeTableSize() + 1;
            // word pos tag
            if (posTag != null)
            {
                feature.Add((offset + DataCenter.GetPosTagIndex(posTag)) + ":1");
            }
            offset += DataCenter.GetPosTagTableSize() + 1;
        }

    }
}
