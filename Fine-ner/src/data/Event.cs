using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    /// <summary>
    /// An event storing query's features and label(for test)
    /// </summary>
   public class Event
    {

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
