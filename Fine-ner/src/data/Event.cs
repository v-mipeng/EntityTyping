using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Event
    {
        private Feature feature = null;
        private Label label = null;

        public Event() { }

        public Event(Label label) : this(label, null)
        {
            
        }

        public Event(Feature feature) : this(null, feature) { }

        public Event(Label label, Feature feature)
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

        public Feature Feature
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
