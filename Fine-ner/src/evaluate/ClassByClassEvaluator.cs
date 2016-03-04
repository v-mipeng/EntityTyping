using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.file.reader;
using pml.file.writer;
using System.IO;

namespace msra.nlp.tr.eval
{
    public class ClassByClassEvaluator : Evaluator
    {

        string resultFile = null;
        string evaluationFile = null;

        public ClassByClassEvaluator()
        {

        }

        public ClassByClassEvaluator(string resultFile, string evaluationFile)
        {
            this.resultFile = resultFile;
            this.evaluationFile = evaluationFile;
        }


        public void EvaluateResult()
        {
            EvaluateResult(this.resultFile, this.evaluationFile);
        }

        public void EvaluateResult(string resultFile, string evaluationFile)
        {
            var reader = new LargeFileReader(resultFile);
            var line = "";
            var result = new Dictionary<string, Dictionary<string, int>>();  // class-->(predicted class --> number)
            int times = 0;
            var trueLabelIndex = 1;
            var predictLabelIndex = 2;
            var writer = new LargeFileWriter(evaluationFile, FileMode.Create);
            Dictionary<string, int> dic = null;
            line = reader.ReadLine();
            var keys = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                var array = line.Split('\t');
                keys.Add(array[trueLabelIndex]);
                keys.Add(array[predictLabelIndex]);
                try
                {
                    dic = result[array[trueLabelIndex]];
                    try
                    {
                        times = dic[array[predictLabelIndex]];
                        dic[array[predictLabelIndex]] = times + 1;
                    }
                    catch (Exception)
                    {
                        dic[array[predictLabelIndex]] = 1;
                    }
                }
                catch (Exception)
                {
                    dic = new Dictionary<string, int>();
                    dic[array[2]] = 1;
                    result[array[trueLabelIndex]] = dic;
                }
            }
            reader.Close();
            writer.Write("True|Predict");
            foreach (var key in keys)
            {
                writer.Write("\t" + key);
            }
            writer.WriteLine("");
            foreach (var key in keys)
            {
                try
                {
                    var info = result[key];
                    writer.Write(key);
                    foreach (var k in keys)
                    {
                        if (info.TryGetValue(k, out times))
                        {
                            writer.Write("\t" + times);
                        }
                        else
                        {
                            writer.Write("\t" + 0);
                        }
                    }
                    writer.WriteLine("");
                }
                catch (Exception)
                {
                    continue;
                }
            }
            var macroPre = Util.GetMacroPrecision(result);
            var macroRec = Util.GetMacroRecall(result);
            var macroF1 = Util.GetF1(macroPre, macroRec);
            writer.WriteLine("macro-precision: " + macroPre);
            writer.WriteLine("macro-recall   : " + macroRec);
            writer.WriteLine("macro-F1       : " + macroF1);
            var microPre = Util.GetMicroPrecision(result);
            writer.WriteLine("micro-precision: " + microPre);
            writer.Close();
        }


    }
}
