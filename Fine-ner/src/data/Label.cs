using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Label
    {
        string stringLabel = null;
        int intLabel = -1;

        public Label(string label)
        {
            this.stringLabel = label;
        }

        public int IntLabel
        {
            get
            {
                if(this.stringLabel == null)
                {
                    throw new Exception("Label is null!");
                }
                return stringLabel.GetHashCode();
            }
            private set
            {

            }

        }

        public string StringLabel
        {
            get
            {
                if (this.stringLabel == null)
                {
                    throw new Exception("Label is null!");
                }
                return stringLabel;
            }
            private set
            {

            }
        }

    }
}
