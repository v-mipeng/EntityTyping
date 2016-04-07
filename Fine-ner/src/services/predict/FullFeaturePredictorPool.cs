using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msra.nlp.tr.predict
{
   public class FullFeaturePredictorPool
    {
        static List<FullFeaturePredictor> predictors = new List<FullFeaturePredictor>();
        static HashSet<int> availablePredictors = new HashSet<int>();
        readonly static int maxPredictorNum = 50;
        static object locker = new object();

        /// <summary>
        /// Get a stanford ner from ner pool
        /// </summary>
        /// <returns></returns>
        public static FullFeaturePredictor GetPredictor()
        {
            lock (locker)
            {
                lock (availablePredictors)
                {
                    if (availablePredictors.Count > 0)
                    {
                        try
                        {
                            var index = availablePredictors.First();
                            availablePredictors.Remove(index);
                            return predictors[index];
                        }
                        catch (Exception e)
                        {
                            Console.Clear();
                            Console.WriteLine("Ner pool is empty!");
                            Console.WriteLine(availablePredictors.Count);
                            Console.WriteLine(e.Message);
                            throw e;
                        }
                    }
                    else if (predictors.Count < maxPredictorNum)
                    {
                        if (availablePredictors.Count == 0)
                        {
                            var predictor = new FullFeaturePredictor();
                            predictors.Add(predictor);
                            return predictor;
                        }
                        else
                        {
                            var index = availablePredictors.First();
                            availablePredictors.Remove(index);
                            return predictors[index];
                        }
                    }
                }
                {
                    while (availablePredictors.Count == 0)
                    {
                        Thread.Sleep(10);
                    }
                    var index = availablePredictors.First();
                    availablePredictors.Remove(index);
                    return predictors[index];
                }
            }
        }

        /// <summary>
        /// return FullFeaturePredictor to the pool
        /// </summary>
        /// <summary>
        /// return FullFeaturePredictor to the pool
        /// </summary>
        /// <param name="predictor"></param>
        /// <param name="predictor"></param>
        public static void ReturnPredictor(FullFeaturePredictor predictor)
        {
            for (var i = 0; i < predictors.Count; i++)
            {
                if (predictor == predictors[i])
                {
                    lock (availablePredictors)
                    {
                        availablePredictors.Add(i);
                    }
                    break;
                }
            }
        }
    }
}
