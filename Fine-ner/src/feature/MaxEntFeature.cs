using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.type;

namespace msra.nlp.tr
{
    class MaxEntFeature : Feature
    {
        private List<string> feature = new List<string>();

        public static string[] fields = new string[]{
            // last word
            "lastWord",
            "lastWordTag",
            "lastWordID",
            "lastWordShape",
            // next word
            "nextWord",
            "nextWordTag",
            "nextWordID",
            "nextWordShape",
            // mention head
            "mentionHead",
            "mentionHeadTag",
            "mentionHeadID",
            "mentionHeadShape",
            // mention driver
            "mentionDriver",
            "mentionDriverTag",
            "mentionDriverID",
            "mentionDriverShape",
            // mention adjective modifier
            "mentionAdjModifier",
            "mentionAdjModifierTag",
            "mentionAdjModifierID",
            "mentionAdjModifierShape",
            // mention action
            "mentionAction",
            "mentionActionTag",
            "mentionActionID",
            "mentionActionShape",
            // context document
            "documentID",
            // if name list contains
            
            // mention level
            "mentionID",
            "mentionLength",
        };

        public MaxEntFeature() : base() { }


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
         *      Parent in dependency tree(stanford corenlp)   
         *      Dictionary                      :TODO
         *      Topic(Define topic)             :TODO: I am going to work with document cluster
         * 
         */
        internal List<string> ExtractFeature(Event e)
        {
            feature.Clear();
            var rawFeature = e.Feature;
            feature.Add("0");
          
            #region last word
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
                // words surfaces: first two and last words
                feature.Add(words[0]);
                if(words.Length > 1)
                {
                    feature.Add(words[1]);
                }
                feature.Add(words[words.Length - 1]);
                // words ids: first two and last words 
                feature.Add(IDs[0]);
                if (IDs.Length > 1)
                {
                    feature.Add(IDs[1]);
                }
                feature.Add(IDs[IDs.Length - 1]);
                // words shapes : first two and last words
                feature.Add(shapes[0]);
                if (shapes.Length > 1)
                {
                    feature.Add(shapes[1]);
                }
                feature.Add(shapes[shapes.Length - 1]);
                // words tags : first two and last words
                feature.Add(tags[0]);
                if (tags.Length > 1)
                {
                    feature.Add(tags[1]);
                }
                feature.Add(tags[tags.Length - 1]);
            }
            #endregion

            #region mention ID
            {
                var mentionID = rawFeature.ElementAt((int)Event.Field.mentionID);
                feature.Add(mentionID);
            }
            #endregion

            #region mention length
            {
                var length = int.Parse(rawFeature.ElementAt((int)Event.Field.mentionLength));
                if (length > 5)
                {
                    length = 5;
                }
                feature.Add((length - 1).ToString());
            }
            #endregion

            #region TDDO: topic
            {
                // TODO
            }
            #endregion

            #region TDDO: dictionary
            {
                // dictionary
                // TODO
            }
            #endregion
            return feature;
        }

        private void AddWordFieldToFeature(string stemmedWord, string ID, string shape, string posTag)
        {
            if (stemmedWord != null)
            {
                // word surface
                feature.Add(stemmedWord);
            }
            // word Cluster id
            if (ID != null)
            {
                feature.Add(ID);
            }
            // word shape
            if (shape != null)
            {
                feature.Add(shape);
            }
            // word pos tag
            if (posTag != null)
            {
                feature.Add(posTag);
            }
        }

    }
}
