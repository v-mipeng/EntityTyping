using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr.predict
{
   public class FourClassPredictor : FullFeaturePredictor
    {
        internal FourClassPredictor()
            : base((string)Parameter.GetParameter(Parameter.Field.four_classes_model_file))
        {
        }

        internal FourClassPredictor(string modelFile)
            : base(modelFile)
        {

        }
    }

   
}
