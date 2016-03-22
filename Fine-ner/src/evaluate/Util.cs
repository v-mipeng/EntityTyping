using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace msra.nlp.tr.eval
{
    public class Util
    {

        /// <summary>
        /// Get macro precision:
        /// (p1/n1+p2/n2)/2
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double GetMacroPrecision(Dictionary<string, Dictionary<string, int>> result)
        {
            var keys = result.Keys.ToList();
            var macroPrecision = 0.0;

            for (var i = 0; i < keys.Count; i++)
            {
                var dic = result[keys[i]];
                var total = 0;
                var positive = 0;
                foreach (var key in dic.Keys)
                {
                    if (key.Equals(keys[i]))
                    {
                        positive = dic[key];
                    }
                    total += dic[key];
                }
                macroPrecision += 1.0 * positive / total;
            }
            macroPrecision /= keys.Count;
            return macroPrecision;
        }

        /// <summary>
        /// Get micro precision:
        /// (p1+p2)/(n1+n2)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double GetMicroPrecision(Dictionary<string, Dictionary<string, int>> result)
        {
            var keys = result.Keys.ToList();
            var positive = 0;
            var total = 0;

            for (var i = 0; i < keys.Count; i++)
            {
                var dic = result[keys[i]];
                foreach (var key in dic.Keys)
                {
                    if (key.Equals(keys[i]))
                    {
                        positive += dic[key];
                    }
                    total += dic[key];
                }
            }
            return 1.0 * positive / total;
        }

        public static double GetMacroRecall(Dictionary<string, Dictionary<string, int>> result)
        {
            var predictedNumDic = new Dictionary<string, int>();
            var positiveNumDic = new Dictionary<string, int>();
            var keys = result.Keys.ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                positiveNumDic[keys[i]] = 0;
                var dic = result[keys[i]];
                foreach (var key in dic.Keys)
                {
                    if (key.Equals(keys[i]))
                    {
                        positiveNumDic[key] = dic[key];
                    }
                    var times = 0;
                    predictedNumDic.TryGetValue(key, out times);
                    predictedNumDic[key] = times + dic[key];
                }
            }
            var macroRecall = 0.0;
            foreach (var key in keys)
            {
                macroRecall += 1.0 * positiveNumDic[key] / predictedNumDic[key];
            }
            return macroRecall / keys.Count;
        }

        public static double GetMicroRecall(Dictionary<string, Dictionary<string, int>> result)
        {
            //var predictedNumDic = new Dictionary<string, int>();
            //var positiveNumDic = new Dictionary<string, int>();
            //var keys = result.Keys.ToList();

            //for (var i = 0; i < keys.Count; i++)
            //{
            //    var dic = result[keys[i]];
            //    foreach (var key in dic.Keys)
            //    {
            //        if (key.Equals(keys[i]))
            //        {
            //            positiveNumDic[key] = dic[key];
            //        }
            //        var times = 0;
            //        predictedNumDic.TryGetValue(key, out times);
            //        predictedNumDic[key] = times + dic[key];
            //    }
            //}
            //var macroRecall = 0.0;
            //foreach (var key in keys)
            //{
            //    macroRecall += 1.0 * positiveNumDic[key] / predictedNumDic[key];
            //}
            //var positive = 0;
            //var 
            //foreach(var key in keys)
            //{

            //}
            //return macroRecall / keys.Count;
            return 0.0;
        }

        public static double GetMacroFn(Dictionary<string, Dictionary<string, int>> result, double belta)
        {
            var macroPre = GetMacroPrecision(result);
            var macroRec = GetMacroRecall(result);
            return GetFn(macroPre, macroRec, belta);
        }

        public static double GetMicroFn(Dictionary<string, Dictionary<string, int>> result, double belta)
        {
            var microPre = GetMicroPrecision(result);
            var microRec = GetMicroRecall(result);
            return GetFn(microPre, microRec, belta);
        }

        public static double GetMacroF1(Dictionary<string, Dictionary<string, int>> result)
        {
            return GetMacroFn(result, 1);
        }

        public static double GetMicroF1(Dictionary<string, Dictionary<string, int>> result)
        {
            return GetMicroFn(result, 1);
        }

        public static double GetF1(double precision, double recall)
        {
            return GetFn(precision, recall, 1);
        }
        public static double GetFn(double precision, double recall, double belta)
        {
            return (1 + belta * belta) * precision * recall / (belta * belta * precision + recall);
        }

    }
}
