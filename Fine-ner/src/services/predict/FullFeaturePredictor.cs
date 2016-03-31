﻿using System;
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
    public class FullFeaturePredictor : Predictor
    {
        // trained model file
        string modelFile = null;
        IDataModel dataModel = null;
        IDataStats dataStats = null;
        IPredictor predictor = null;
        IndividualFeature rawFeatureExtractor = null;
        SVMFeature svmFeatureExtractor = null;

        /// <summary>
        /// Create type predictor with defaul model file.
        /// </summary>
        internal FullFeaturePredictor()
        {
            this.modelFile = (string)Parameter.GetParameter(Parameter.Field.model_file);
        }

        internal FullFeaturePredictor(string modelFile)
        {
            this.modelFile = modelFile;
        }

        public string Predict(string mention, string context)
        {
            return PredictWithProbability(mention, context)[0].first;
        }
                                                        
        public List<pml.type.Pair<string, float>> PredictWithProbability(string mention, string context)
        {
            if (svmFeatureExtractor == null)
            {
                Initial();
            }
            List<string> rawFeature = null;
            try
            {
                rawFeature = rawFeatureExtractor.ExtractFeature(new Instance(mention, context));
            }
            catch (NotFindMentionException)
            {
                rawFeature = rawFeatureExtractor.ExtractFeature(new Instance(mention, context),false);
            }
            var e = new Event(rawFeature);
            var svmFeature = svmFeatureExtractor.ExtractFeature(e);
            var floatFeature = ExpandFeatureToVector(svmFeature);
            var predictions = Predict(floatFeature);
            var pairs = new List<pml.type.Pair<string, float>>();
            for (var index = 0; index < predictions.Length; index++)
            {
                try
                {
                    pairs.Add(new pml.type.Pair<string, float>(Parameter.GetTypeByLabel(index), predictions[index]));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            pairs.Sort(pairs[0].GetBySecondReverseComparer());
            return pairs;
        }

        private float[] ExpandFeatureToVector(List<string> svmFeature)
        {
            var dimention = int.Parse(svmFeature[0]);  // get the dimension of feature
            var floatFeature = new float[dimention];   // convert sparse feature to dense feature representation
            for (var i = 1; i < svmFeature.Count; i++)
            {
                var array = svmFeature[i].Split(':');
                var index = int.Parse(array[0]);
                var value = float.Parse(array[1]);
                floatFeature[index] = value;
            }
            return floatFeature;
        }
                           
        protected void Initial()
        {
            rawFeatureExtractor = new IndividualFeature();
            svmFeatureExtractor = new SVMFeature();
            LoadModel();
        }

        /// <summary>
        /// Load Model from given file.
        /// </summary>
        private void LoadModel()
        {
            try
            {
                this.predictor = PredictorUtils.LoadPredictor<float[]>(out dataModel, out dataStats, modelFile);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
