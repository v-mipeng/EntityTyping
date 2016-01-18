﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpEntropy;
using SharpEntropy.IO;

namespace MaxEntropy
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader trainDataStreamReader = new StreamReader("");
            ITrainingEventReader eventReader = new BasicEventReader(new PlainTextByLineDataReader(trainDataStreamReader));    // reader training pairs
            GisTrainer trainer = new GisTrainer();
            trainer.TrainModel(eventReader);
            var maxEntModel = new GisModel(trainer);
            var context = new List<string>();
            maxEntModel.Evaluate((string[])(context.ToArray()));
            PlainTextGisModelReader modelReader = new PlainTextGisModelReader("");
            //PlainTextGisModelWriter modelWriter = new PlainTextGisModelWriter("");

        }
    }
}
