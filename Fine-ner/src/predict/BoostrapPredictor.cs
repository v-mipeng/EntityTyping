using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MachineLearning;
using Microsoft.MachineLearning.CommandLine;
using Microsoft.MachineLearning.Learners;
using Microsoft.MachineLearning.Model;
using Microsoft.TMSN.TMSNlearn;
using Microsoft.MachineLearning.Data;

namespace msra.nlp.tr.predict
{
    public class BoostrapPredictor
    {
        // trained model file
        string modelFile = null;
        IDataModel dataModel = null;
        IDataStats dataStats = null;
        IPredictor predictor = null;

        /// <summary>
        /// Create type predictor with defaul model file.
        /// </summary>
        public BoostrapPredictor()
        {
            this.modelFile = (string)DefaultParameter.Get(DefaultParameter.Field.model_file);

        }

        public BoostrapPredictor(string modelFile)
        {
            this.modelFile = modelFile;
        }

        public string Predict(IEnumerable<string> feature)
        {
            return PredictWithProbability(feature)[0].first;
        }

        public List<pml.type.Pair<string, float>> PredictWithProbability(IEnumerable<string> feature)
        {
            if (predictor == null)
            {
                Initial();
            }

            var svmFeature = feature;
            var dimention = int.Parse(svmFeature.ElementAt(0));  // get the dimension of feature
            var floatFeature = new float[dimention];   // convert sparse feature to dense feature representation
            for (var i = 1; i < svmFeature.Count(); i++)
            {
                var array = svmFeature.ElementAt(i).Split(':');
                var index = int.Parse(array[0]);
                var value = float.Parse(array[1]);
                floatFeature[index] = value;
            }
            var predictions = Predict(floatFeature);
            var pairs = new List<pml.type.Pair<string, float>>();
            try
            {
                for (var index = 0; index < predictions.Length; index++)
                {
                    pairs.Add(new pml.type.Pair<string, float>(SVMFeatureExtractor.types[index], predictions[index]));
                }
            }
            catch (Exception)
            {
                throw new Exception("The versions of model and feature extraction do not match!");
            }
            pairs.Sort(pairs[0].GetBySecondReverseComparer());
            return pairs;
        }
                                                  
        private void Initial()
        {
            LoadModel();
        }

        /// <summary>
        /// Load Model from given file.
        /// </summary>
        private void LoadModel()
        {
            // Initialize the environment.
            this.predictor = PredictorUtils.LoadPredictor(out dataModel, out dataStats, modelFile);
            if (predictor == null)
            {
                throw new Exception("Predictor is not a binary classifier");
            }
        }

        /// <summary>
        /// Predict with pretrained model with tlc client
        /// </summary>
        /// <param name="svmFeature"></param>
        /// <returns></returns>
        private float[] Predict(float[] svmFeature)
        {
            var instance = new Microsoft.TMSN.TMSNlearn.Instance(svmFeature);
            return (float[])predictor.Predict(instance);
        }

    }
}
