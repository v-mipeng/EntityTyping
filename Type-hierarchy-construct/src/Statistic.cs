using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using pml.type;

namespace msra.nlp.tr.HierarchyExtractor
{
    class Statistic
    {
        protected static class FileOperator
        {
            public static FileReader reader = null;
            public static bool reachFileEnd = false;
            static int count = 0;
            static readonly int docNum = 4000000;
            static readonly String beginLabelString = "#N#";


            public static void SetFilePath(String filePath = null)
            {
                if(filePath == null)
                {
                    filePath = (string)GlobalParameter.Get("satori_raw_data_file");
                }
                if (reader == null)
                {
                    reader = new LargeFileReader(filePath);
                }
                else
                {
                    reader.Open(filePath);
                }
            }

            public static Dictionary<string, string> ReadOneItem()
            {
                if (reader == null)
                {
                    throw new Exception("No file assigned to read");
                }
                if(reachFileEnd)
                {
                    return null;
                }
                count++;
                if (count % 10000 == 0)
                {
                    Console.Error.WriteLine("Deal number: " + count);
                }
                if (count == docNum)
                {
                    reachFileEnd = true;
                    reader.Close();
                }
                reader.Skip(3);
                return ParseItem(reader.ReadLine());
            }

            public static void SkipItem(int number)
            {
                String line;
                int count = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#N#"))
                    {
                        count++;
                    }
                    if (count == number)
                    {
                        break;
                    }
                }
            }

            private static Dictionary<string, string> ParseItem(String item)
            {

                Dictionary<String, String> dic = new Dictionary<String, String>();
                if (item == null)
                {
                    return dic;
                }
                try
                {
                    String[] array = item.Split('\t');
                    dic["description"] = array[0];
                    dic["mention"] = array[1];
                    dic["entity"] = array[2];
                    array = array[3].Split(new string[] { "#TAB" }, StringSplitOptions.None);
                    dic["type"] = array[0];
                    return dic;
                }
                catch (Exception)
                {
                    return dic;
                }
            }
        }

        public static void TrimSatori()
        {
            TrimSatori(null, null);
            
        }
        public static void TrimSatori(string sourcePath)
        {
            TrimSatori(sourcePath, null);
        }
        public static void TrimSatori(string sourcePath, string desPath)
        {
            if(sourcePath == null)
            {
                sourcePath = (string)GlobalParameter.Get("satori_raw_data_file");
            }
            if (desPath == null)
            {
                desPath = (string)GlobalParameter.Get("satori_trim_data_file");
            }
            FileOperator.SetFilePath(sourcePath);
            FileWriter writer = new LargeFileWriter(desPath);
            Dictionary<string, string> dic = null;

            while (!FileOperator.reachFileEnd)
            {
                dic = FileOperator.ReadOneItem();
                writer.Write(dic["mention"]);
                writer.Write(dic["entity"]);
                writer.Write(dic["type"]);
                writer.Write(dic["description"] + "\r");
            }
            writer.Close();
        }

        static Property props = null;
        public static void SetProperty(Property propertiess)
        {
            props = propertiess;
        }

        /************************************************************************/
        /* Get statistic result with exist encountered mention and entity number information stored in files abstract 
         * by "numByMentionFile"   and "numByEntityFile". Store the statistic report into the file abstracted 
         * by "reportFile" and return  the organized type number by mention and by entity.
        /************************************************************************/
        public static Pair<Dictionary<string, int>, Dictionary<string, int>> StatisticNumByType(string numByMentionFile, string numByEntityFile, string reportFile)
        {
            Dictionary<string, int> type2numByMention = LoadDic(numByMentionFile);
            Dictionary<string, int> type2numByEntity = LoadDic(numByEntityFile);
            type2numByMention = OrganizeType(type2numByMention);
            type2numByEntity = OrganizeType(type2numByEntity);

            FileWriter writer = new LargeFileWriter(reportFile, FileMode.Create);
            writer.Write(ReportStatisticResult(type2numByMention, type2numByEntity));
            writer.Close();
           return new Pair<Dictionary<string, int>, Dictionary<string, int>>(type2numByMention, type2numByEntity);
        }

        /************************************************************************/
        /* Get statistic result with raw satori data(extracted by Wengong JIn)  stored in file abstracted by "satoriDataFile".
         *  Store the encountered mention and entity number by type into files abstracted by countMentionNumFile and countEntityNumFile respectively
         * Store the statistic report into the file abstracted by "reportFile" and return  the organized type number by mention and by entity.
        /************************************************************************/
        public static Pair<Dictionary<string, int>, Dictionary<string, int>> StatisticNumByType(string satoriDataFile, string countMentionNumFile, string countEntityNumFile, string reportFile)
        {
            FileReader reader = new LargeFileReader(satoriDataFile);
            Dictionary<string, int> type2numByMention = new Dictionary<string, int>();
            Dictionary<string, int> type2numByEntity = new Dictionary<string, int>();
            HashSet<string> mentionSet = new HashSet<string>();
            HashSet<string> entitySet = new HashSet<string>();
            string line;
            string type;
            string[] array;
            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                count++;
                if (count % 10000 == 0)
                {
                    Console.WriteLine(count);
                }
                try
                {
                    array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length != 4)
                    {
                        continue;
                    }
                    type = array[2].Split('#')[0];
                    if (!mentionSet.Contains(array[0]))        // array[0]: mention
                    {
                        mentionSet.Add(array[0]);
                        try
                        {
                            type2numByMention[type] += 1;
                        }
                        catch (Exception)
                        {
                            type2numByMention[type] = 1;
                        }
                    }
                    if (!entitySet.Contains(array[1]))
                    {
                        entitySet.Add(array[1]);
                        try
                        {
                            type2numByEntity[type] += 1;
                        }
                        catch (Exception)
                        {
                            type2numByEntity[type] = 1;
                        }
                    }
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
            reader.Close();
            // organize number by type hierarchy
            type2numByMention = OrganizeType(type2numByMention);
            type2numByEntity = OrganizeType(type2numByEntity);
            // Store counted type number with unique mentions and entities
            OutputStatisticData(countMentionNumFile, type2numByMention);
            OutputStatisticData(countEntityNumFile, type2numByEntity);
            FileWriter writer = new LargeFileWriter(reportFile, FileMode.Create);
            writer.WriteLine(ReportStatisticResult(type2numByMention, type2numByEntity));
            writer.Close();
            return new Pair<Dictionary<string, int>, Dictionary<string, int>>(type2numByMention, type2numByEntity);
        }

        /************************************************************************/
        /* Add counted number information of by into hierarchy  
         * Input: 
         *     type2num: organized encountered number by type
         *     hierarchyFile: original hierarchy file
         *     desFile : file to store the hierarchy with number information*/
        /************************************************************************/
        public static void FormatHierarchy(Dictionary<string, int> type2num, string hierarchyFile, string desFile)
        {
            FileReader reader = new LargeFileReader();
            Dictionary<string, int> numByType = type2num;
            string line;
            string[] array;
            int value;
            bool ishead = true;
            reader.Open(hierarchyFile);
            FileWriter writer = new LargeFileWriter(desFile, FileMode.Create);
            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                ishead = true;
                foreach (string type in array)
                {
                    try
                    {
                        if (ishead)
                        {
                            writer.Write(string.Format("{0}({1})", type, numByType[type]));
                            ishead = false;
                        }
                        else
                        {
                            writer.Write(string.Format("-->{0}({1})", type, numByType[type]));
                        }
                    }
                    catch (Exception)
                    {
                        if (ishead)
                        {
                            writer.Write(string.Format("{0}({1})", type, "Unknown"));
                            ishead = false;
                        }
                        else
                        {
                            writer.Write(string.Format("-->{0}({1})", type, "Unknown"));
                        }

                    }
                }
                writer.Write("\r");
            }
            reader.Close();
            writer.Close();
        }

        /************************************************************************/
        /* The same as above function, but type2num information is given by a file.
         * The file format is:
         * type     TAB     number
        /************************************************************************/
        public static void FormatHierarchy(string type2NumFile, string typeHierarchyFile, string des)
         {
             FileReader reader = new LargeFileReader(type2NumFile);
             Dictionary<string, int> numByType = new Dictionary<string, int>();
             string line;
             string[] array;
             int value;
             // reader entity number by type
             while ((line = reader.ReadLine()) != null)
             {
                 array = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                 if (array.Length != 2)
                 {
                     continue;
                 }
                 if (int.TryParse(array[1], out value))
                 {
                     numByType[array[0]] = value;
                 }
             }
             reader.Close();
             FormatHierarchy(numByType, typeHierarchyFile, des);
         }

      public enum TypeOption {Top1,Top3}

        public static Dictionary<string,int> StatisticTypeNumByUniqueEntity(string sourceFile, TypeOption option = TypeOption.Top3)
        {
            Dictionary<string, int> typeNumByEntity = new Dictionary<string, int>();
            List<string> types = null;
            FileReader reader = new LargeFileReader(sourceFile);
            string line;
            int typeNumLimit = 3;
            if(option == TypeOption.Top1)
            {
                typeNumLimit = 1;
            }

            while((line = reader.ReadLine())!=null)
            {
                types = line.Split('\t')[1].Split('#').ToList();
                for(int i = 0; i< typeNumLimit && i<types.Count; i++)
                {
                    try
                    {
                        typeNumByEntity[types[i]] += 1;
                    }
                    catch(Exception)
                    {
                        typeNumByEntity[types[i]] = 1;
                    }
                }
            }
            reader.Close();
            return typeNumByEntity;
        }

        public static void StatisticTypeNumByUniqueEntity(string sourceFile, string desFile, TypeOption option = TypeOption.Top3)
        {
            FileWriter writer = new LargeFileWriter(desFile, FileMode.Create);
            Dictionary<string, int> dic = StatisticTypeNumByUniqueEntity(sourceFile, option);
            List<string> keys = SortKeysByNum(dic);

            foreach(string key in keys)
            {
                writer.WriteLine(key + "\t" + dic[key]);
            }
            writer.Close();
        }

        public static HashSet<string>  FindTypeWithMultiParents(Dictionary<string,HashSet<string>> typeHierarchy, HashSet<string> topTypes)
        {
            HashSet<string> types = new HashSet<string>();
            HashSet<string> set = null;
            HashSet<string> setCopy = null;
            HashSet<string> setCopy2 = null;

            bool once = false;
            bool changed = true;
            int setSize = 0;

            foreach(string key in typeHierarchy.Keys)
            {
                set = typeHierarchy[key];
                changed = true;
                while (changed)
                {
                    changed = false;
                    setCopy = new HashSet<string>(set);
                    setSize = set.Count;
                    foreach (string type in setCopy)
                    {
                        try
                        {
                            setCopy2 = typeHierarchy[type];
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        foreach (string item in setCopy2)
                        {
                            set.Add(item);
                        }
                    }
                    if(setSize < set.Count)
                    {
                        changed = true;
                    }
                }
                once = false;
                foreach (string type in set)
                {
                    if (topTypes.Contains(type))
                    {
                        if (once)
                        {
                            types.Add(key);
                            break;
                        }
                        else
                        {
                            once = true;
                        }
                    }
                }
            }
            return types;
        }

        public static void  FindTypeWithMultiParents(Dictionary<string,HashSet<string>> typeHierarchy, HashSet<string> topTypes, string desFile)
        {

            HashSet<string> types = FindTypeWithMultiParents(typeHierarchy, topTypes);
            FileWriter writer = new LargeFileWriter(desFile, FileMode.Create);

            foreach(string type in types)
            {
                writer.WriteLine(type);
            }
            writer.Close();
        }

        public static Dictionary<string, HashSet<string>> LoadHierarchy(string filePath)
        {
            LoadPairwiseHierarchy(filePath);
            return lower2higher;
        }

        public static HashSet<string> LoadSet(string sourceFile)
        {
            HashSet<string> set = new HashSet<string>();
            FileReader reader = new LargeFileReader(sourceFile);
            string line;

            while((line = reader.ReadLine())!=null)
            {
                set.Add(line);
            }
            return set;
        }


        /************************************************************************/
        /* Turn the organized mention and entity number by type into string                                                                     */
        /************************************************************************/
         private static string ReportStatisticResult(Dictionary<string, int> type2numByMention, Dictionary<string, int> type2numByEntity)
         {
             // output statistic result;
             StringBuilder buffer = new StringBuilder();
             //buffer.Append("Mention number: " + mentionSet.Count+"\r");
             //buffer.Append("Entity number: " + entitySet.Count+"\r");
             buffer.Append("\r");
             buffer.Append("Mention number by type:\r");
             buffer.Append(string.Format("{0,-50}{1,10}\r", "type", "number"));
             List<string> keys = SortKeysByNum(type2numByMention);
             foreach (string key in keys)
             {
                 buffer.Append(string.Format("{0,-50}{1,10}\r", key, type2numByMention[key]));
             }
             buffer.Append("\r");
             buffer.Append("Entity number by type:\r");
             buffer.Append(string.Format("{0,-50}{1,10}\r", "type", "number"));
             keys = SortKeysByNum(type2numByEntity);
             foreach (string key in keys)
             {
                 buffer.Append(string.Format("{0,-50}{1,10}\r", key, type2numByEntity[key]));
             }
             return buffer.ToString();
         }

        /************************************************************************/
        /* Read counted mention or entity number by type information from file                                                                    */
        /************************************************************************/
         static Dictionary<string, int> LoadDic(string filePath)
         {
             FileReader reader = new LargeFileReader(filePath);
             Dictionary<string, int> dic = new Dictionary<string, int>();
             string line;
             string[] array;

            while((line = reader.ReadLine())!=null)
            {
                array = line.Split('\t');
                if(array.Length<2)
                {
                    break;
                }
                dic[array[0]] = int.Parse(array[1]);
            }
            reader.Close();
            return dic;
         }

        /************************************************************************/
        /* Sort the dictionary's keys by their mapped int value                                                                     */
        /************************************************************************/
         static List<string> SortKeysByNum(Dictionary<string, int> dic)
         {
             List<string> keys = dic.Keys.ToList();
             Comparer<String> comparer = new DiComparer(dic);
             keys.Sort(comparer);
             return keys;
         }

        public  class DiComparer : Comparer<string>
        {
            Dictionary<string, int> dic;
            public DiComparer(Dictionary<string,int> dic)
            {
                this.dic = dic;
            }
            public override int Compare(string str1, string str2)
            {
                return -((IComparable<int>)dic[str1]).CompareTo(dic[str2]);
            }
    }


        // lower level type to higher level type
         static Dictionary<string, HashSet<string>> lower2higher = null;
         static Dictionary<string, HashSet<string>> higher2lower = null;
 

        /*Load Pairwise type Hierarchy
         */ 
       static void LoadPairwiseHierarchy(string sourcePath = null)
         {
             if (sourcePath == null)
               {
                   sourcePath = (string)GlobalParameter.Get("hierarchy_file");
               }
             FileReader reader = new LargeFileReader(sourcePath);
             lower2higher = new Dictionary<string, HashSet<string>>();
             higher2lower = new Dictionary<string, HashSet<string>>();
             string line;
             string[] array;
             HashSet<string> set = null;

            while((line = reader.ReadLine())!=null)
            {
                array = line.Split('\t');
                if (array.Length < 2)
                {
                    continue;
                }
                for (int i = array.Length - 1; i > 0;i-- )
                {
                    if(array[i].Equals(array[i-1]))
                    {
                        continue;
                    }
                    // update lower2higher dictionary
                    try
                    {
                        set = lower2higher[array[i]];
                        set.Add(array[i - 1]);
                    }
                    catch (Exception)
                    {
                        set = new HashSet<string>();
                        set.Add(array[i - 1]);
                        lower2higher[array[i]] = set;
                    }
                    // update higher2lower dictionary
                    try
                    {
                        set = higher2lower[array[i-1]];
                        set.Add(array[i]);
                    }
                    catch (Exception)
                    {
                        set = new HashSet<string>();
                        set.Add(array[i]);
                        higher2lower[array[i - 1]] = set;
                    }
                }
            }
            reader.Close();
         }

        /* Organize statistic information with type hierarchy
         */ 
        static Dictionary<string,int> OrganizeType(Dictionary<string,int> dic)
         {
             Dictionary<string, int> newDic = AddHigherLevelType(dic);
            HashSet<string> keys = new HashSet<string>();
            foreach(string key in newDic.Keys)
            {
                  keys.Add(key);
            }
            Dictionary<string,bool> done = new Dictionary<string,bool>();
            HashSet<string> set = null;
            HashSet<string> setCopy = null;

             bool changed = true;
             bool skip = false;
             string type;
            // initial done
            for (int i = 0; i < keys.Count; i++)
            {
                type = keys.ElementAt(i);
                 // delete items not included in keys
                try
                {       
                   set = higher2lower[type];
                    setCopy = new HashSet<string>(set);
                    foreach (string item in setCopy)
                    {
                        if(!keys.Contains(item))
                        {
                            set.Remove(item);
                        }
                    }
                    if(set.Count ==0)
                    {
                        higher2lower.Remove(type);
                    }
                    else
                    {
                        higher2lower[type] = set;
                    }
                }
                catch(Exception) 
                {
                  done[type] = true;
                  continue;
                }

                if (higher2lower.ContainsKey(type))
                {
                    done[type] = false;
                }
                else
                {
                    done[type] = true;
                }
            }
            // iterate update number of type
            while(changed)
            {
                changed = false;
       
                for (int i = 0; i < keys.Count; i++)       // for each type
                {
                    type = keys.ElementAt(i);
                    if(done[type])
                    {
                        continue;
                    }
                    else
                    {  // if not done
                        set = higher2lower[type];
                        foreach(string item in set)
                        {
                            if(!done[item])
                            {            // if subtype not done, skip
                                skip = true;
                                break;
                            }
                        }
                         if(skip)
                         {
                             skip = false;
                             continue;
                         }
                         else
                         {     // update number of type
                                foreach(string item in set)
                                {
                                    newDic[type] += newDic[item];
                                }
                                done[type] = true;
                                changed = true;
                         }
                    }
                }
            }
             return newDic;
         }
        /*Check if the "higherLevel" type is parent type of the "lowerLevel" type
         */ 
        static bool IsHigherLevel(string higherLevel, string lowerLevel)
        {
            if(higherLevel.Equals(lowerLevel))
            {
                return false;
            }
            if (lower2higher == null)
            {
                LoadPairwiseHierarchy();
            }
            string type = lowerLevel;
            string lastType = type;
            Stack<string> stack = new Stack<string>();        //make a stack
            stack.Push(lowerLevel);
            HashSet<string> set;
            while(true)
            {
                try
                {
                    set = lower2higher[type];
                    if(set.Contains(higherLevel))
                    {
                        return true;
                    }
                    foreach (string str in set)
                    {
                        stack.Push(str);
                    }
                    if (stack.Count == 0)
                    {
                        return false;
                    }
                    type = stack.Pop();
                    if (type.Equals(lastType))
                    {
                        type = stack.Pop();
                    }
                    lastType = type;
                }
                catch(Exception)
                {
                    if(stack.Count == 0)
                    {
                        return false;
                    }
                    type = stack.Pop();
                    continue;
                }
            }
        }

        static bool IsDirectPrecursor(string higherLevel, string lowerLevel)
        {
            if (higherLevel.Equals(lowerLevel))
            {
                return false;
            }
            if (lower2higher == null)
            {
                LoadPairwiseHierarchy();
            }
            HashSet<string> set;
            try
            {
                set = lower2higher[lowerLevel];
                if(set.Contains(higherLevel))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        static Dictionary<string, int> AddHigherLevelType(Dictionary<string, int> dic)
        {
            Dictionary<string, int> newDic = new Dictionary<string, int>(dic);
            if (lower2higher == null)
            {
                LoadPairwiseHierarchy();
            }
            Dictionary<string, int>.KeyCollection keys = dic.Keys;
            string type;
            HashSet<string> set;
            HashSet<string> visited = new HashSet<string>();
            Stack<string> stack = new Stack<string>();        

            foreach (string key in keys)
            {
                stack.Clear();
                visited.Clear();
                type = key;
                while (true)
                {
                    try
                    {
                        set = lower2higher[type];
                        foreach (string str in set)
                        {
                            if (!visited.Contains(str))
                            {
                                visited.Add(str);
                                stack.Push(str);
                            }
                        }
                        if(stack.Count == 0)
                        {
                            break;
                        }
                        type = stack.Pop();
                        if (!newDic.ContainsKey(type))
                        {
                            newDic[type] = 0;
                        }
                    }
                    catch (Exception)
                    {
                        if (stack.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            type = stack.Pop();
                        }
                    }
                }
            }
            return newDic;
        }

         static void OutputStatisticData(string des, Dictionary<string,int> dic)
        {
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            foreach (string key in dic.Keys)
            {
                writer.WriteLine(key + "\t" + dic[key]);
            }
            writer.Close();
        }

        static void StatisticMentionNumByType(string sourceFile, string des)
        {
            var reader = new LargeFileReader(sourceFile);
            var dic = new Dictionary<string, int>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    var array = line.Split('\t');
                    var types = array[2];
                    array = types.Split('#');
                    try
                    {
                        dic[array[1]] += 1;
                    }
                    finally
                    {
                        dic[array[1]] = 1;
                    }
                }
                finally
                {
                }
            }
            reader.Close();
            var writer = new LargeFileWriter(des, FileMode.Create);
            foreach (var key in dic.Keys)
            {
                writer.WriteLine(key + "\t" + dic[key]);
            }
            writer.Close();
        }
    }
}
