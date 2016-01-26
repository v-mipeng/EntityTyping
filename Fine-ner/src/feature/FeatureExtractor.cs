//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;
//using System.Collections;
//using pml.type;

//namespace msra.nlp.tr
//{
//    class FeatureExtractor
//    {
//        public FeatureExtractor() { }


//        public Pair<int,IList> ExtractFeatureWithLable(String[] input)
//        {
//            Pair<int, IList> pair = new Pair<int, IList>();
//            pair.first = GetTypeValue(input[1]);
//           List<Pair<int,int>> list = (List<Pair<int,int>>)ExtractFeature(new string[] { input[0], input[2] });
//           Comparer<Pair<int,int>> comparer = new Pair<int,int>().GetByFirstComparer();
//            list.Sort(comparer);
//            pair.second = list;
//            return pair;
//        }

//        /*   Extract feature from the input
//         *   The input should contains two items:
//         *      Mention surface:   the surface text of the mention             // input[0]
//         *      Mention context:   the context contains the mention         // input[1]
//         *   The output are a list of pairs store the features' index and value:                                   
//         *      Mention surface:    [s0 e0]
//         *      Mention Shape:      [s0 e0] 
//         *                          [AA, A., (A|a)'(s), d-d, dd]
//         *      Mention length
//         *      Last token  index in word table
//         *      Next token  index in word table
//         *      Matched entity type within UIUC: type code
//         * 
//         */
//        public IList  ExtractFeature(String[] input)
//        {
//            int wordTableSize = DataCenter.GetWordTableSize();
//            IList  feature = new List<Pair<int, int>>();
//            String[] words = input[0].Split(' ');
//            for (int i = 0; i < words.Length; i++)
//            {
//                words[i] = StemWord(words[i]);
//            }
//            int offset = 0;
//            // mention length
//            feature.Add(new Pair<int,int>(offset++,words.Length));
//            // mention shape: s0 e0
//            feature.Add(new Pair<int, int>(offset++, GetWordShape(words[0])));
//            feature.Add(new Pair<int, int>(offset++, GetWordShape(words[words.Length - 1])));
//            // mention surface: s0 e0
//            offset = feature.Count;
//            feature.Add(new Pair<int, int>(offset+DataCenter.GetWordIndex(words[0]),1));           
//            offset += wordTableSize+1;
//            feature.Add(new Pair<int, int>(offset+DataCenter.GetWordIndex(words[words.Length - 1]),1));
//            // last token
//            offset += wordTableSize+1;
//            //String lastToken = GetLastToken(input[1], input[0]);
//            String lastToken = StemWord(GetLastToken(input[1], input[0]));
//            if(lastToken != null)
//            {
//                feature.Add(new Pair<int,int>(offset+DataCenter.GetWordIndex(lastToken),1));
//            }
//            // next token
//            offset += wordTableSize+1; // point to the first empty index
//            //String nextToken = GetNextToken(input[1], input[0]);
//            String nextToken = StemWord(GetNextToken(input[1], input[0]));
//            if(nextToken !=null)
//            {
//                 feature.Add(new Pair<int,int>(offset+DataCenter.GetWordIndex(nextToken),1));
//            }
//            //// types of the matched entity
//            offset += wordTableSize+1;

//            //List<int> indexes = (List<int>)GetDicTypeOfMention(input[0]);
//            //if (indexes != null)
//            //{
//            //    indexes.Sort();
//            //    indexes.ForEach(x => feature.Add(new Pair<int, int>(offset + x, 1)));
//            //}
//            //// name list

//            //offset += DataCenter.GetDicTypeNum();
//            //feature.Add(new Pair<int, int>(offset, GetNameMatchValue(input[0]) + 1));
//            //offset++;

//            // get feature dimension
//            if (GlobalParameter.featureNum == 0)
//            {
//                GlobalParameter.featureNum = offset;
//            }
//            return feature;
//        }

//        /*******************Type label****************************/
//        Dictionary<string, int> type2index = null;

//        /* Make the type-->value map : define the interest type and others
//         */
//        protected void MakeTypeMap()
//        {
//            String[] types = new String[] { "organization.organization", "people.person", "location.location", "commerce.consumer_product", "sports.sport", "medicine.disease", "time.event", "book.written_work", "film.film", "broadcast.content", "visual_art.artwork","others"};
//            type2index = new Dictionary<string, int>();

//            for (int i = 0; i < types.Length; i++)
//            {
//                type2index[types[i]] = i;
//            }
//        }

//        /* Get the mapped int value of type
//         */
//        protected  int GetTypeValue(String type)
//        {
//           if(type2index == null)
//           {
//               MakeTypeMap();
//           }
//           try
//           {
//               int value = type2index[type];
//               return value;
//           }
//           catch (Exception)
//           {
//               return type2index["others"];
//           }

//        }

//        /**********************Mention information***************************/
//       /*If mention contains preposition, return 1
//         * else, return 0
//         */
//        protected  int HasPreposition(String mention)
//        {
//            string[] words = mention.Split('\t');
//            foreach (string word in words)
//            {
//                if (DataCenter.IsPreposition(word))
//                {
//                    return 1;
//                }
//            }
//            return 0;
//        }

//        protected  string StemWord(String word)
//        {
//            return DataCenter.GetStemmedWord(word);
//        }

//        /**********************Context of the mention***************************/

//        /*Get last token of the mention
//         */ 
//        static String pattern = @",[^,]*,";
//        static Regex regex = new Regex(pattern);
//        String[] seperator = new string[] { " ", "\t" };
//        public String GetLastToken(String context,String mention)
//        {
//            context = context.Trim();
//            mention = mention.Trim();
//            String head = context.Substring(0,context.IndexOf(mention));
//            head = regex.Replace(head, " ").TrimEnd();
//            String lastToken = null;
//            for (int i = head.Length-1; i >= 0; i--)
//            {
//                  if(head[i]==' ' || head[i]=='\t')
//                  {
//                      lastToken = head.Substring(i + 1);
//                      break;
//                  }
//            }
//            return lastToken;
//        }
    
//        /* Get next token of the mention
//         */ 
//      public  String GetNextToken(String context, String mention)
//        {
//            context = context.Trim();
//            mention = mention.Trim();
//            String tail = context.Substring(context.LastIndexOf(mention) + mention.Length);
//            tail = tail.TrimStart();
//            if(tail.StartsWith(",") || tail.Length == 0)
//            {
//                return null;
//            }
//            else
//            {
//                int index;
//                if ((index = tail.IndexOf(' ')) != -1)
//                {
//                    return tail.Substring(0, index);
//                }
//                else
//                {
//                    return tail;
//                }
//            }
//        }

//        /*******************Mention shape**************************/

//        /* [Aa or aa,AA, A., (A|a)'(s), d-d, dd]
//        */
//      protected  int GetWordShape(String word)
//        {
//            if (IsNormal(word))
//            {
//                return 0;
//            }
//            else if (IsNumber(word))
//            {
//                return 5;
//            }
//            else if (IsAllUpCase(word))
//            {
//                return 1;
//            }
//            else if(IsShortName(word))
//            {
//                return 2;
//            }
//            else if (IsConnectNumber(word))
//            {
//                return 4;
//            }
//            else
//            {
//                return 0;
//            }
            
//        }

//        /* Is shape: Aa, aa or other normal format
//         */
//        protected bool IsNormal(String input)
//        {
//           if(input.Length == 1)
//           {
//               return true;
//           }
//           else
//           {
//               for(int i = 1; i<input.Length-1; i++)
//               {
//                   if(input[i]<'a' || input[i] > 'z')
//                   {
//                       return false;
//                   }
//               }
//               return true;
//           }
//        }

//        protected bool IsAllUpCase(String input)
//        {
//            if(input.Length == 1)   // don't consider one character word like "A"
//            {
//                return false;
//            }
//            foreach(char item in input)
//            {
//                if(item>'Z' || item < 'A')
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//        internal bool IsFirstUpCase(string input)
//        {
//            if(input[0] > 'A' && input[0] < 'Z')
//            {
//                for(int i = 1; i<input.Length-1; i++)
//                {
//                    if(input[i] > 'z'|| input[i] < 'a')
//                    {
//                        return false;
//                    }
//                }
//                return true;
//            }
//            return false;
//        }

//        /* Is shape: Mr.
//         */
//        protected bool IsShortName(String input)
//        {
//            if(input.EndsWith(".") && input[0] < 'Z' && input[0] > 'A')
//            {
//                return true;
//            }
//            return false;
//        }

//        /* Is shape : Jason's or Jasons'
//         */ 
//        static String pattern2 = @"\w*'(s)?";
//        Regex regex2 = new Regex(pattern2);
//        protected bool IsPossessive(String input)
//        {
//            if (regex2.IsMatch(input))
//            {
//                return true;
//            }
//            return false;
//        }

//        /* Is shape: dd
//         */
//        static string pattern3 = @"^\d*$";
//        Regex regex3 = new Regex(pattern3);
//        protected bool IsNumber(string input)
//        {
//            if (regex3.IsMatch(input))
//            {
//                return true;
//            }
//            return false;
//        }

//        /* Is shape: d-d
//       */
//        static string pattern4 = @"^\d*-\d*$";
//         Regex regex4 = new Regex(pattern4);
//        protected bool IsConnectNumber(string input)
//        {
//            if (regex4.IsMatch(input))
//            {
//                return true;
//            }
//            return false;
//        }
       
//        /*************************Dictionary information*********************************/

//        /************************************************************************/
//        /* Get type indexes of matched entity in given dictionary
//         * An index represents a type in the dictionary's form*/
//        /************************************************************************/
//        protected static IList GetDicTypeOfMention(String mention)
//        {
//            List<String> list = DataCenter.GetTypeInDic(mention.ToLower());
//            if(list == null)
//            {
//                return null;
//            }
//            List<int> indexes = new List<int>();
//            foreach(var type  in list)
//            {
//                int value = DataCenter.GetDicTypeValue(type);
//                if (value != int.MinValue)
//                {
//                    indexes.Add (DataCenter.GetDicTypeValue(type));
//                }
//            }
//            if(indexes.Count ==0)
//            {
//                return null;
//            }
//            return indexes;
//        }

//        /************************************************************************/
//        /* If mention match an entire name, return 1
//         * if mention match part of a person name, return 2
//         * else return 0*/
//        /************************************************************************/
//        protected int GetNameMatchValue(String mention)
//        {
//            String[] words = mention.Split('\t');
//            if (DataCenter.IsFullNameMatch(mention))
//            {
//                return 2;
//            }
//            else if(DataCenter.IsPartNameMatch(mention) )
//            {
//                return 1;
//            }
//            return 0;
//        }

//    }
//}
                                                                 