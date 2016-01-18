using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    interface Trainer
    {
        
        /// <summary>
        ///    Train model
        /// </summary>
        void Train();

        /// <summary>
        ///   Predict the class label of new input represented by feature
        /// </summary>
        /// <param name="feture"></param>
        /// <returns></returns>
        object Predict(object feture);

        /// <summary>
        ///     Save trained model into give file
        /// </summary>
        /// <param name="modelFilePath"></param>
        void SaveModel(string modelFilePath);

        /// <summary>
        ///     Load trained model from given file
        /// </summary>
        /// <param name="modelFilePath"></param>
        void LoadModel(string modelFilePath);
    }
}
