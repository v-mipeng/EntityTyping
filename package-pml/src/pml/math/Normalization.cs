using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pml.math
{
    public class Normalization
    {
        private Normalization() { }

        public static List<double> MinMaxNormalize(IEnumerable<double> vector)
        {
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach(var value in vector)   // look for minimal and maximal value
            {
                if(min > value)
                {
                    min = value;
                }
                if(max < value)
                {
                    max = value;
                }
            }
            var list = new List<double>();
            for(var i = 0;i<vector.Count();i++)
            {
                list.Add((vector.ElementAt(i) - min) / (max - min));
            }
            return list;
        }

        public static List<double> MaxNormalize(IEnumerable<double> vector)
        {
            var max = double.MinValue;
            foreach (var value in vector)   // look for minimal and maximal value
            {
                if (max < value)
                {
                    max = value;
                }
            }
            if(max == 0)
            {
                max = 1;
            }
            var list = new List<double>();
            for (var i = 0; i < vector.Count(); i++)
            {
                list.Add(vector.ElementAt(i)/max);
            }
            return list;
        }
    }
}
