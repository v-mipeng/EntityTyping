using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
   public class Event
    {
        // feature of event
        public enum Field
        {
            // last word
            lastWord,
            lastWordStemmed,
            lastWordTag,
            lastWordID,
            lastWordShape,
            // next word
            nextWord,
            nextWordStemmed,
            nextWordTag,
            nextWordID,
            nextWordShape,
            // mention head
            mentionHead,
            mentionHeadStemmed,
            mentionHeadTag,
            mentionHeadID,
            mentionHeadShape,
            // mention driver
            mentionDriver,
            mentionDriverStemmed,
            mentionDriverTag,
            mentionDriverID,
            mentionDriverShape,
            // mention adjective modifier
            mentionAdjModifier,
            mentionAdjModifierStemmed,
            mentionAdjModifierTag,
            mentionAdjModifierID,
            mentionAdjModifierShape,
            // mention action
            mentionAction,
            mentionActionStemmed,
            mentionActionTag,
            mentionActionID,
            mentionActionShape,
            // mention words
            mentionSurfaces,
            mentionSurfacesStemmed,
            mentionTags,
            mentionIDs,
            mentionShapes,
            // context document
            //documentID,
            // if name list contains

            // mention level
            mentionID,
            // mention length
            mentionLength,
            // stanford ner output
            stanfordNerType,
            // opennlp ner output
            opennlpNerType,
            // dbpedia type
            dbpediaTypes,
            // key words
            //keyWordsWithoutParser,
            keyWords,
            // mention context
            sentenceContext

        }


        private IEnumerable<string> feature = null;

        private Label label = null;

        public Event() { }

        public Event(Label label) : this(label, null)
        {
            
        }

        public Event(IEnumerable<string> feature) : this(null, feature) { }

        public Event(Label label, IEnumerable<string> feature)
        {
            this.label = label;
            this.feature = feature;
        }

        /// <summary>
        /// Get event label
        /// </summary>
        public Label Label
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        /// <summary>
        /// Get event feature
        /// </summary>
        public IEnumerable<string> Feature
        {
            get
            {
                return feature.ToList();
            }
            set
            {
                feature = value;
            }
        }

    }
}
