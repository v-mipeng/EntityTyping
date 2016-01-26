using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Event
    {
        // feature of event
        private object feature = null;

        private Label label = null;

        public Event() { }

        public Event(Label label) : this(label, null)
        {
            
        }

        public Event(object feature) : this(null, feature) { }

        public Event(Label label, object feature)
        {
            this.label = label;
            this.feature = feature;
        }

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

        public object Feature
        {
            get
            {
                return feature;
            }
            set
            {
                feature = value;
            }
        }

    }
}
