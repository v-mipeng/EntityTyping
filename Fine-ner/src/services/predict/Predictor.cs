using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pml.type;

namespace msra.nlp.tr.predict
{
    public abstract class Predictor
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
        public string Predict(string mention, string context)
        {
            return PredictWithProbability(context, context.IndexOf(mention), mention.Length)[0].first;
        }

        /// <summary>
        /// Predict the type of the mention indexed by its offset in the given context and length
        /// </summary>
        /// <param name="mention">
        /// A string type of named entity
        /// </param>
        /// <param name="context"></param>
        /// <returns>
        /// The type of the mention
        /// </returns>         
        public string Predict(string context, int mentionOffset, int mentionLength)
        {
            return PredictWithProbability(context, mentionOffset, mentionLength)[0].first;
        }

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
        public List<Pair<string, float>> PredictWithProbability(string mention, string context)
        {
            return PredictWithProbability(context, context.IndexOf(mention), mention.Length);
        }

        /// <summary>
        /// Predict the type of the mention indexed by its offset and length in the given context
        /// </summary>
        /// <returns>
        /// A list of types with corresponding probabilities
        /// </returns>
        public abstract List<Pair<string, float>> PredictWithProbability(string context, int mentionOffset, int mentionLength);

    }
}
