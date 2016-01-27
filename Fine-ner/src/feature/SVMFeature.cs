﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using pml.type;

namespace msra.nlp.tr
{
    class SVMFeature  : Feature
    {
        List<string> feature = new List<string>();
        int offset = 0;

        public SVMFeature():base(){}

        public Pair<object, Dictionary<int,int>> ExtractFeatureWithLable(String[] input)
        {
            return new Pair<object, Dictionary<int, int>>(GetTypeValue(input[1]), null);
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

            #region last word TODO: make last word more accurate
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

            #region mention words
            {
                string[] words = null;
                try 
                {
                    words = rawFeature.ElementAt((int)Event.Field.mentionSurfacesStemmed).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch(Exception)
                {
                    throw new Exception("Mention words is null");
                }
                string[] IDs = null;
                try {
                    IDs = rawFeature.ElementAt((int)Event.Field.mentionIDs).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch(Exception)
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
                foreach (var w in words) // words surface
                {
                    var index = offset + DataCenter.GetWordIndex(w);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetWordTableSize() + 1;
                dic.Clear();
                foreach (var ID in IDs) // words' cluster id
                {
                    var index = offset + int.Parse(ID);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                dic.Clear();
                foreach (var shape in shapes) // words shapes
                {
                    var index = offset + DataCenter.GetWordShapeIndex(shape);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetWordShapeTableSize() + 1;
                dic.Clear();
                 foreach (var tag in tags)
                {   // words pos tags
                    var index = offset + DataCenter.GetPosTagIndex(tag);
                    dic.TryGetValue(index, out value);
                    dic[index] = value + 1;
                }
                foreach (var item in dic)
                {
                    feature.Add(item.Key + ":" + item.Value);
                }
                offset += DataCenter.GetPosTagTableSize() + 1;
            }
            #endregion

            #region mention length: 1,2,3,4 or longer than 5
            {
                var length = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionLength));
                if(length > 5)
                {
                    length = 5;
                }
                feature.Add((offset + length - 1) + ":1");
                offset += 5;
            }
            #endregion

            #region mention cluster id   TODO: do entity linking to match mention.
            {
                var mentionID = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionID));
                feature.Add((offset + mentionID) + ":1");
                offset += DataCenter.GetMentionClusterNumber() + 1;
            }
            #endregion

            #region TODO: topic
            {

            }
            #endregion

            #region TODO: dictionary
            {

            }
            #endregion

            return feature;
        }

        private void AddWordFieldToFeature(string stemmedWord,string ID,string shape, string posTag)
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
                feature.Add((offset + ID) + ":1");
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

        private void AddToFeature(params string[] objs)
        {
            foreach (var par in objs)
            {
                this.feature.Add(par);
            }
        }

    }
}
