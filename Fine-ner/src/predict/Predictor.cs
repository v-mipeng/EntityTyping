using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.type;

namespace msra.nlp.tr.predict
{
    interface Predictor
    {
        /// <summary>
        /// Predict the type of mention.
        /// </summary>
        /// <param name="mention">
        /// The mention on which prediction should be made.
        /// </param>
        /// <param name="context">
        /// The context information of mention
        /// </param>
        /// <returns>
        /// The most likely type of mention
        /// </returns>
        string Predict(string mention, string context);


        /// <summary>
        /// Predict the type of mention with probability information
        /// </summary>
        /// <param name="mention">
        /// The mention on which prediction should be made.
        /// </param>
        /// <param name="context">
        /// The context information of mention
        /// </param>
        /// <returns>
        /// A list of pairs with the type and its corresponding probability.
        /// </returns>
        List<Pair<string, float>> PredictWithProbability(string mention, string context);

    }
}
