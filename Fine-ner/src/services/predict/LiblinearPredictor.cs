using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using de.bwaldvogel.liblinear;

namespace msra.nlp.tr.predict
{
    public class LiblinearPredictor : FullFeaturePredictor
    {
        Model model = null;

        internal LiblinearPredictor() : base()
        {
        }
        
        internal LiblinearPredictor(string modelFile) : base(modelFile)
        {
        }

        public override List<pml.type.Pair<string, float>> PredictWithProbability(string context, int mentionOffset, int mentionLength)
        {
            if (svmFeatureExtractor == null)
            {
                Initial();
            }
            List<string> rawFeature = null;
            try
            {
                rawFeature = rawFeatureExtractor.ExtractFeature(new Instance(context, mentionOffset, mentionLength));
            }
            catch (NotFindMentionException)
            {
                rawFeature = rawFeatureExtractor.ExtractFeature(new Instance(context, mentionOffset, mentionLength));
            }
            var e = new Event(rawFeature);
            var svmFeature = svmFeatureExtractor.ExtractFeature(e);
            var pairs = new List<pml.type.Pair<string, float>>();
            var feature = TransformFeature(svmFeature);
            var predictions = Predict(feature);
            int index = 0;
            foreach(var label in model.getLabels())
            {
                try
                {
                    pairs.Add(new pml.type.Pair<string, float>(Parameter.GetTypeByLabel(label), (float)predictions[index++]));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            pairs.Sort(pairs[0].GetBySecondReverseComparer());
            return pairs;
        }

        protected override void LoadModel()
        {
            model = Model.load(new java.io.File(base.modelFile));
        }

        private de.bwaldvogel.liblinear.Feature[] TransformFeature(List<string> svmFeature)
        {
            var feature = new de.bwaldvogel.liblinear.Feature[svmFeature.Count];
            for (var i = 0; i < svmFeature.Count; i++)
            {
                var array = svmFeature[i].Split(':');
                var index = int.Parse(array[0]);
                var value = double.Parse(array[1]);
                feature[i] = new FeatureNode(index, value);
            }
            return feature;
        }

        private double[] Predict(de.bwaldvogel.liblinear.Feature[] feature)
        {
            var probilities = new double[model.getLabels().Length];
            var value = Linear.predictProbability(this.model, feature, probilities);
            return probilities;
        }
    }
}
