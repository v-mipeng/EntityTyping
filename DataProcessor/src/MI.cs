using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr.dp
{
    public class MI
    {
        public static double GetMI(int classOneNum, int classTwoNum, int classOneAndEventOneNum, int classTwoAndEventTwoNum)
        {
            var N1 = classOneNum;
            var N0 = classTwoNum;
            var N11 = classOneAndEventOneNum;
            var N01 = classTwoAndEventTwoNum;
            var N = N1 + N0;
            var N_1 = N11 + N01;
            var N_0 = N - N_1;
            var N10 = N1 - N11;
            var N00 = N0 - N01;
            var value = 0.0;
            value += 1.0 * N11 * Math.Log(N * N11 / N1 / N_1) / Math.Log(2);
            value += 1.0 * N01 * Math.Log(N * N01 / N0 / N_1) / Math.Log(2);
            value += 1.0 * N10 * Math.Log(N * N10 / N1 / N_0) / Math.Log(2);
            value += 1.0 * N00 * Math.Log(N * N00 / N0 / N_0) / Math.Log(2);
            value /= 1.0 * N;
            return value;
        }
    }
}
