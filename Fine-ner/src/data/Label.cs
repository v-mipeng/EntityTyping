using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class Label
    {
        private string stringLabel = null;

        private int intLabel = -1;

        public Label(string label)
        {
            this.stringLabel = label;
        }

        public Label(Label label)
        {
            this.stringLabel = label.StringLabel;
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

        
        public override string ToString()
        {
            return stringLabel; 
        }
    }
}
