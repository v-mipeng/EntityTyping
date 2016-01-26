﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;


namespace msra.nlp.tr
{
    interface EventReader
    {
        void Open(string filePath);

        Event GetNextEvent();

        bool HasNext();

        bool Close();
    }
}
