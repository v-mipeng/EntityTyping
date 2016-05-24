using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using pml.type;
using pml.file.writer;
using System.IO;

namespace msra.nlp.tr
{
    class SVMFeature : Feature
    {
        LargeFileWriter writer = new LargeFileWriter(@"D:\Codes\Project\EntityTyping\Fine-ner\analysis\type2index.txt", FileMode.Create);
        int lastOffset = 0;
        List<string> feature = new List<string>();
        int offset = 0;

        internal SVMFeature()
            : base()
        {

        }


        /// <summary>
        /// Reture feature dimension
        /// For sparse expression
        /// </summary>
        public int FeatureDimension
        {
            get
            {
                return offset;
            }
            private set { }
        }


        /// <summary>
        ///  Extract feature from the input, and the feature is clustered by field
        /// </summary>
        /// <param name="Event">
        /// An Envent with features of query.
        /// </param>
        /// <returns>
        /// A list of features including: Please refer Event to get the order of features
        ///     Mention words  
        ///     Mention shapes
        ///     Mention word cluster IDs
        ///     Mention length         
        ///     Mention cluster ID                      
        ///     Last token
        ///     Last token pos tag
        ///     Last token cluster ID                   
        ///     Next token
        ///     Next token pos tag
        ///     Next token cluster ID                   
        ///     Dictionary                      :Dbpedia
        ///     Topic(Define topic)             :MI keyword
        /// </returns>
        public List<string> ExtractFeature(Event e, bool isLiblinear = true)
        {
            var basedir = @"D:\Codes\Project\EntityTyping\Fine-ner\analysis\features";
            var writer = new LargeFileWriter();
            this.feature.Clear();
            var rawFeature = e.Feature;
            if (!isLiblinear)
            {
                this.offset = 0;
                feature.Add("0");
            }
            else
            {
                this.offset = 1;
            }
            this.offset = 0;

            DataCenter.GetWordShapeTableSize();
            DataCenter.GetPosTagTableSize();
            DataCenter.GetWordTableSize();
            DataCenter.GetClusterNumber();
            DataCenter.GetMentionClusterNumber();
            #region last word (make last word more accurate)
            if (useLastWord)
            {
                var word2index = DataCenter.word2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "lastWordSurf.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordTableSize());
                foreach (var item in word2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordTableSize();
                var tag2index = DataCenter.posTag2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "lastWordTag.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetPosTagTableSize());
                foreach (var item in tag2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetPosTagTableSize();
                writer = new LargeFileWriter(Path.Combine(basedir, "lastWordID.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetClusterNumber() + 1);

                for (var i = 0; i < DataCenter.GetClusterNumber()+1; i++)
                {
                    writer.WriteLine(i);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                writer = new LargeFileWriter(Path.Combine(basedir, "lastWordShape.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordShapeTableSize());
                foreach (var item in DataCenter.wordShape2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordShapeTableSize();
            }
            #endregion


            #region next word
            if (useNextWord)
            {
                var word2index = DataCenter.word2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "nextWordSurf.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordTableSize());
                foreach (var item in word2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordTableSize();
                var tag2index = DataCenter.posTag2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "nextWordTag.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetPosTagTableSize());
                foreach (var item in tag2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetPosTagTableSize();
                writer = new LargeFileWriter(Path.Combine(basedir, "nextWordID.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetClusterNumber() + 1);

                for (var i = 0; i < DataCenter.GetClusterNumber()+1; i++)
                {
                    writer.WriteLine(i);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                writer = new LargeFileWriter(Path.Combine(basedir, "nextWordShape.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordShapeTableSize());
                foreach (var item in DataCenter.wordShape2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordShapeTableSize();
            }
            #endregion

            #region  mention head
            if (useMentionHead)
            {
                var word2index = DataCenter.word2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionHeadSurf.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordTableSize());
                foreach (var item in word2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordTableSize();
                var tag2index = DataCenter.posTag2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionHeadTag.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetPosTagTableSize());
                foreach (var item in tag2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetPosTagTableSize();
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionHeadID.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetClusterNumber() + 1);

                for (var i = 0; i < DataCenter.GetClusterNumber()+1; i++)
                {
                    writer.WriteLine(i);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionHeadShape.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordShapeTableSize());
                foreach (var item in DataCenter.wordShape2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordShapeTableSize();

            }
            #endregion

            #region mention words
            if (useMentionSurfaces)
            {
                var word2index = DataCenter.word2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionWordsSurf.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordTableSize());
                foreach (var item in word2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordTableSize();
                var tag2index = DataCenter.posTag2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionWordsTag.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetPosTagTableSize());
                foreach (var item in tag2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetPosTagTableSize();
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionWordsID.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetClusterNumber() + 1);
                for (var i = 0; i < DataCenter.GetClusterNumber()+1; i++)
                {
                    writer.WriteLine(i);
                }
                offset += DataCenter.GetClusterNumber() + 1;
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionWordsShape.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetWordShapeTableSize());
                foreach (var item in DataCenter.wordShape2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetWordShapeTableSize();
            }
            #endregion

            #region mention cluster id
            if (useMentionID)
            {
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionID.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + DataCenter.GetMentionClusterNumber() + 1);
                for (var i = 0; i < DataCenter.GetMentionClusterNumber()+1; i++)
                {
                    writer.WriteLine(i);
                }
                offset += DataCenter.GetMentionClusterNumber() + 1;
            }
            #endregion


            #region mention length: 1,2,3,4 or longer than 5
            if (useMentionLength)
            {
                writer = new LargeFileWriter(Path.Combine(basedir, "mentionLength.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + (offset + 4));
                for (var i = 0; i < 5; i++)
                {
                    writer.WriteLine(i);
                }
                offset += 5;
            }
            #endregion


            #region DBpedia types
            {
                var type2index = DataCenter.dbpediaType2index;
                writer = new LargeFileWriter(Path.Combine(basedir, "DbpediaType.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + (DataCenter.GetDBpediaTypeNum() * 2));
                foreach (var item in type2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                foreach (var item in type2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetDBpediaTypeNum() * 2;
            }
            #endregion


            #region Key words
            if (useKeywords)
            {
                var keyword2index = DataCenter.keyWords;
                writer = new LargeFileWriter(Path.Combine(basedir, "Keywords.txt"), FileMode.Create);
                writer.WriteLine(offset + "\t" + (offset+DataCenter.GetKeyWordNumber()));
                foreach (var item in keyword2index.OrderBy(i => i.Value))
                {
                    writer.WriteLine(item.Key);
                }
                offset += DataCenter.GetKeyWordNumber();
            }
            #endregion
            Console.WriteLine("Done!");
            writer.Close();

            //set feature dimension
            if (!isLiblinear)
            {
                feature[0] = FeatureDimension.ToString();
            }
            Console.WriteLine("Done!");
            return feature;
        }


        private void AddWordFieldToFeature(string stemmedWord, string posTag, string ID, string shape)
        {
            if (stemmedWord != null)
            {
                // word surface
                feature.Add((offset + DataCenter.GetWordIndex(stemmedWord)) + ":1");
                offset += DataCenter.GetWordTableSize() + 1;
                writer.WriteLine(lastOffset + "~" + (offset - 1));
                lastOffset = offset;
            }
            // word pos tag
            if (posTag != null)
            {
                feature.Add((offset + DataCenter.GetPosTagIndex(posTag)) + ":1");
                offset += DataCenter.GetPosTagTableSize() + 1;
                writer.WriteLine(lastOffset + "~" + (offset - 2));
                lastOffset = offset;
            }
            // word Cluster id
            if (ID != null)
            {
                feature.Add((offset + int.Parse(ID)) + ":1");
                offset += DataCenter.GetClusterNumber() + 1;
                writer.WriteLine(lastOffset + "~" + (offset - 2));
                lastOffset = offset;
            }
            // word shape
            if (shape != null)
            {
                feature.Add((offset + DataCenter.GetWordShapeIndex(shape)) + ":1");
                offset += DataCenter.GetWordShapeTableSize() + 1;
                writer.WriteLine(lastOffset + "~" + (offset - 2));
                lastOffset = offset;
            }
        }

    }
}
