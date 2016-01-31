using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    public class MI
    {
        public static double GetMI(int classOneNum, int classTwoNum, int classOneAndEventOneNum, int classTwoAndEventTwoNum)
        {
            if(classOneAndEventOneNum <10 && classTwoAndEventTwoNum < 10)
            {
                return 0;
            }
            var N1 = classOneNum;
            var N0 = classTwoNum;
            var N11 = classOneAndEventOneNum +1;
            var N01 = classTwoAndEventTwoNum +1;
            var N = N1 + N0;
            var N_1 = N11 + N01;
            var N_0 = N - N_1;
            var N10 = N1 - N11;
            var N00 = N0 - N01;
            var value = 0.0;
            value += 1.0 * N11 * Math.Log(1.0 * N * N11 / N1 / N_1) / Math.Log(2);
            value += 1.0 * N01 * Math.Log(1.0 * N * N01 / N0 / N_1) / Math.Log(2);
            value += 1.0 * N10 * Math.Log(1.0 * N * N10 / N1 / N_0) / Math.Log(2);
            value += 1.0 * N00 * Math.Log(1.0 * N * N00 / N0 / N_0) / Math.Log(2);
            value /= 1.0 * N;
            return value;
        }
    }
}
