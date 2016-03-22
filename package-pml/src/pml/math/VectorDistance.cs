using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.type;

namespace pml.math
{
    public class VectorDistance
    {
        private VectorDistance() { }

        public static double CosinDistance(int[] arrayOne, int[] arrayTwo)
        {
            double[] doubleArrayOne = new double[arrayOne.Length];
            double[] doubleArrayTwo = new double[arrayOne.Length];
            arrayOne.CopyTo(doubleArrayOne, 0);
            arrayTwo.CopyTo(doubleArrayTwo, 0);
            return CosinDistance(doubleArrayOne, doubleArrayTwo); 
        }

        public static double CosinDistance(float[] arrayOne, float[] arrayTwo)
        {
            double[] doubleArrayOne = new double[arrayOne.Length];
            double[] doubleArrayTwo = new double[arrayOne.Length];
            arrayOne.CopyTo(doubleArrayOne, 0);
            arrayTwo.CopyTo(doubleArrayTwo, 0);
            return CosinDistance(doubleArrayOne, doubleArrayTwo); 
        }

        public static double CosinDistance(double[] arrayOne, double[] arrayTwo)
        {
            double distance = 0.0;
            double arrayOneLength = 0.0;
            double arrayTwoLength = 0.0;
            for (int j = 0; j < arrayOne.Length; ++j)
            {
                distance += arrayOne[j] * arrayTwo[j];
                arrayOneLength += arrayOne[j] * arrayOne[j];
                arrayTwoLength += arrayTwo[j] * arrayTwo[j];
            }
            return distance / arrayOneLength / arrayTwoLength;
        }

        public static double SparseCosinDistance(Dictionary<int, double> vectorOne, Dictionary<int, double> vectorTwo)
        {
            double distance = 0.0;
            double vectorOneLength = 0.0;
            double vectorTwoLength = 0.0;

            foreach (var vector in vectorOne)
            {
                vectorOneLength += vector.Value * vector.Value;
                if (vectorTwo.ContainsKey(vector.Key))
                {
                    distance += vector.Value * vectorTwo[vector.Key];
                }
            }
            foreach (var vector in vectorTwo)
            {
                vectorTwoLength += vector.Value * vector.Value;
            }
            if (distance == 0)
            {
                return 0;
            }
            else
            {
                return distance / vectorOneLength / vectorTwoLength;
            }
        }

        public static double SparseCosinDistance(Dictionary<object, double> vectorOne, Dictionary<object, double> vectorTwo)
        {
            double distance = 0.0;
            double vectorOneLength = 0.0;
            double vectorTwoLength = 0.0;
            
            foreach(var vector in vectorOne)
            {
                vectorOneLength += vector.Value * vector.Value;
                if(vectorTwo.ContainsKey(vector.Key))
                {
                    distance += vector.Value * vectorTwo[vector.Key];
                }
            }
            foreach (var vector in vectorTwo)
            {
                vectorTwoLength += vector.Value * vector.Value;
            }
            return distance / vectorOneLength / vectorTwoLength;
        }
    }
}
