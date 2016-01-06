using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file;
using pml.file.reader;
using pml.file.writer;
using System.Text.RegularExpressions;
using System.IO;

namespace msra.nlp.tr.HierarchyExtractor
{
    public class HierarchyParser
    {
        HashSet<string> interestTopTypes = null;
        HashSet<string> uninterestTopTypes = null;
        Dictionary<string, HashSet<string>> link = null;
        Dictionary<string,HashSet<string>> delink = null;


        public HierarchyParser() { }

        public void SetInterestTypes(string[] interestTopTypes)
        {
            if(interestTopTypes == null)
            {
                return;
            }
            this.interestTopTypes = new HashSet<string>(interestTopTypes);
            if(this.interestTopTypes.Count == 0)
            {
                this.interestTopTypes = null;
            }
        }

        /*Read interest top level types from file
         */
        public void SetInterestTypes(string filePath)
        {
            FileReader reader = new LargeFileReader(filePath);
            string line;
            this.interestTopTypes = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (Regex.IsMatch(line, @"\A\s*\Z"))
                {                 // empty line
                    continue;
                }
                this.interestTopTypes.Add(line.Trim());
            }
            reader.Close();
            if (this.interestTopTypes.Count == 0)
            {
                this.interestTopTypes = null;
            }
        }

        public void SetUninterestTypes(string[] uninterestTopTypes)
        {
            if(uninterestTopTypes == null)
            {
                return;
            }
            this.uninterestTopTypes = new HashSet<string>(uninterestTopTypes);
            if (this.uninterestTopTypes.Count == 0)
            {
                this.uninterestTopTypes = null;
            }
        }

        /*Read uninterested types from file
         */
        public void SetUnInterestTypes(string filePath)
        {
            FileReader reader = new LargeFileReader(filePath);
            string line;
            this.uninterestTopTypes = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (Regex.IsMatch(line, @"\A\s*\Z"))
                {                 // empty line
                    continue;
                }
                this.uninterestTopTypes.Add(line.Trim());
            }
            reader.Close();
            if (this.uninterestTopTypes.Count == 0)
            {
                this.uninterestTopTypes = null;
            }
        }

        public void SetLink(Dictionary<string,HashSet<string>> link)
        {
            this.link = new Dictionary<string,HashSet<string>>(link);
            if (this.link.Count == 0)
            {
                this.link = null;
            }
        }

        /*Read type link information from file abstract by filePath
         *File format:
         *type   TAB     subType
         */
        public void SetLink(string filePath)
        {
            FileReader reader = new LargeFileReader(filePath);
            string line;
            string[] array;
            this.link = new Dictionary<string, HashSet<string>>();
            HashSet<string> set = null;

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                try
                {
                    set = this.link[array[1]];
                }
                catch(Exception)
                {
                    set = new HashSet<string>();
                }
                set.Add(array[0]);
                this.link[array[1]] = set;
            }
            reader.Close();
            if (this.link.Count == 0)
            {
                this.link = null;
            }
        }
        /* Delete type links
        * Input format:
        * type map-to subtypes
        */ 
        public void DeleteLink(Dictionary<string,HashSet<string>> delink)
        {
            this.delink = new Dictionary<string,HashSet<string>>(delink);
            if (this.delink.Count == 0)
            {
                this.delink = null;
            }
        }

        /* Delete type links
         * File format:
         * type TAB subType
         */ 
        public void DeleteLink(string filePath)
        {
            FileReader reader = new LargeFileReader(filePath);
            string line;
            string[] array;
            this.delink = new Dictionary<string, HashSet<string>>();
            HashSet<string> set = new HashSet<string>();

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                try
                {
                    set =  this.delink[array[1]];
                }
                catch (Exception)
                {
                    set = new HashSet<string>();
                }
                set.Add(array[0]);
                this.delink[array[1]] = set;
            }
            reader.Close();
            if(this.delink.Count == 0)
            {
                this.delink = null;
            }
        }
     
        
        /************************************************************************/
        /* Extract hierachy of primary types;
         * INPUT:
         *  source: the satori ontoloy file path
        *  des: the file to store the extract hierarchy*/
        /************************************************************************/

        public void ExtractPrimaryHierachy(String source, string des, string internalDir = null)
        {
             if(internalDir == null)
             {
                 internalDir = "";
             }
            String tmpFile = Path.Combine(internalDir, "primary_type_info.xml");
            String tmpFile2 = Path.Combine(internalDir, "pairwiseHierachy.txt"); ;
            String tmpFile3 = "tmp3.txt";
            ExtractPrimaryItems(source, tmpFile);
            ExtractPairHierarchy(tmpFile, tmpFile2);
            ConstructFullHierarchy(tmpFile2, tmpFile3);
            SortByTopType(tmpFile3, des);
            if (internalDir.Length == 0)
            {
                File.Delete(tmpFile);
                File.Delete(tmpFile2);
                File.Delete(tmpFile3);
            }
        }

        /*Extract xml items of primary type
         */
        private void ExtractPrimaryItems(String source, String des)
        {
            FileReader reader = new LargeFileReader(source);
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            Dictionary<String, String> map = new Dictionary<string, string>();
            String line;
            String start = "<rdf:Description";
            String end = "</rdf:Description";
            List<String> lines = new List<String>();
            bool primary = false;
            bool begin = false;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith(start) || begin)
                {
                    begin = true;
                    lines.Add(line);
                    if (line.StartsWith("<mso:type.type.category"))
                    {
                        if (line.Contains("type.category.primary_entity"))
                        {
                            primary = true;
                        }
                        else
                        {
                            begin = false;
                            primary = false;
                            lines.Clear();
                            continue;
                        }
                    }
                    else if (line.StartsWith(end))
                    {
                        if (primary)
                        {
                            foreach (String item in lines)
                            {
                                writer.WriteLine(item);
                            }
                        }
                        begin = false;
                        primary = false;
                        lines.Clear();
                    }
                }
            }
            reader.Close();
            writer.Close();
        }

        /*Extract hierarchy pair
         */
        private void ExtractPairHierarchy(String source, String des)
        {
            FileReader reader = new LargeFileReader(source);
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            String line;
            String pattern = ".*/([^\"]*)\"";
            Regex regex = new Regex(pattern);
            String start = "<rdf:Description";
            String end = "</rdf:Description";
            String type = null;
            String highLevelType = null;
            List<string> list = new List<string>();
            bool begin = false;
            HashSet<string> set ;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith(start))
                {
                    // get entity type
                    Match match = regex.Match(line);
                    if (match != null)
                    {
                        type = match.Groups[1].ToString();
                        begin = true;
                        continue;
                    }
                }
                if (begin)
                {
                    if (line.Contains("type.type.includes"))
                    {
                        Match match = regex.Match(line);
                        if (match != null)
                        {
                            highLevelType = match.Groups[1].ToString();
                            list.Add(highLevelType);
                        }
                    }
                    else if (line.StartsWith(end))
                    {
                        if (delink != null)
                        {
                            // delete type link
                            try
                            {
                                set = delink[type];
                                foreach(string item in set)
                                {
                                    list.Remove(item);
                                }
                            }
                            catch (Exception) { }
                        }
                        if (list.Count > 0)
                        {
                            foreach (string item in list)
                            {
                                writer.Write(item + "\t");
                                writer.WriteLine(type);
                            }
                        }
                        else
                        {
                            writer.WriteLine(type);
                        }
                        begin = false;
                        list.Clear();
                    }
                }
            }
            if (link != null)
            {
                foreach (string subType in link.Keys)
                {              // write link to pairwise type hierarchy
                    foreach (string item in link[subType])
                    {
                        writer.WriteLine(item + "\t" + subType);
                    }
                }
            }
            reader.Close();
            writer.Close();
        }

        List<String> route = new List<string>();
        Dictionary<String, HashSet<String>> forwardMap = new Dictionary<String, HashSet<String>>();// record the direct backward 
        Dictionary<String, HashSet<String>> backwardMap = new Dictionary<String, HashSet<String>>();// record the direct backward 
        public void ConstructFullHierarchy(String source, String des)
        {
            FileReader reader = new LargeFileReader(source);
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            String line;
            String[] array;
            HashSet<string> interestTopTypes = this.interestTopTypes;
            HashSet<string> uninterestTopTypes = null;
            if (interestTopTypes == null)
            {
                uninterestTopTypes = this.uninterestTopTypes;
            }
            HashSet<String> set = null;
            HashSet<String> set2 = null;
            HashSet<String> s = null;

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    line = line.Trim();
                    array = line.Split('\t');
                    try
                    {
                        if (interestTopTypes != null)
                        {
                            if (interestTopTypes.Contains(array[1]))
                            {             // if top level is not interested types(higher level than the interest types), skip
                                continue;
                            }
                        }
                        else if (this.uninterestTopTypes != null && this.uninterestTopTypes.Contains(array[0]))
                        {             // skip uninterested types
                            continue;
                        }
                        set = forwardMap[array[1]];
                    }
                    catch (Exception)
                    {
                        set = new HashSet<string>();
                    }
                    set.Add(array[0]);
                    forwardMap[array[1]] = set;
                    try
                    {
                        set = backwardMap[array[0]];
                    }
                    catch (Exception)
                    {
                        set = new HashSet<string>();
                    }
                    set.Add(array[1]);
                    backwardMap[array[0]] = set;
                }
                catch (Exception e)
                {
                    continue;
                }
            }
            reader.Close();
            List<String> keys = forwardMap.Keys.ToList();
            foreach (String type in keys)
            {
                set = forwardMap[type];
                set2 = new HashSet<string>(set);
                foreach (String item in set)
                {
                    // get direct forward
                    try
                    {
                        s = forwardMap[item];
                        foreach (String item2 in s)
                        {
                            if (set2.Contains(item2))
                            {
                                set2.Remove(item2);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        continue;
                    }

                }
                forwardMap[type] = set2;
            }
            // output the hierarchy
            foreach (String type in forwardMap.Keys)
            {
                if (!backwardMap.ContainsKey(type))
                {
                    if(type.Equals("business.issue"))
                    {
                        Console.WriteLine("wait!");
                    }
                    Explore(type, forwardMap, writer);
                }
            }
            writer.Close();
        }
        private void Explore(String type, Dictionary<String, HashSet<String>> map, FileWriter writer)
        {
            route.Add(type);
            HashSet<String> set = null;
            map.TryGetValue(type, out set);
            if (set == null || set.Count == 0)
            {
                if (this.interestTopTypes != null)
                {
                    if (interestTopTypes.Contains(type))
                    {             // if top level is not interested types(higher level than the interest types), skip
                        WriteRoute(writer);
                    }
                    else
                    {
                        route.RemoveAt(route.Count - 1);
                        return;
                    }
                }
                else if (this.uninterestTopTypes != null && this.uninterestTopTypes.Contains(type))
                {             // skip uninterested types
                    route.RemoveAt(route.Count - 1);
                    return;
                }
                else
                {
                    WriteRoute(writer);
                }
            }
            else
            {
                foreach (String item in set)
                {
                    Explore(item, map, writer);
                }
            }
            route.RemoveAt(route.Count - 1);
        }

        private void WriteRoute(FileWriter writer)
        {
            for (int i = route.Count() - 1; i >= 0; i--)
            {
                writer.Write(route[i] + "\t");
            }
            writer.WriteLine("");
        }

        public void SortByTopType(String source, String des)
        {
            FileReader reader = new LargeFileReader(source);
            FileWriter writer = new LargeFileWriter(des, FileMode.Create);
            String line;
            String top;
            String[] array;
            Dictionary<String, List<String>> map = new Dictionary<String, List<String>>();
            List<String> list;

            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                top = array[0];
                try
                {
                    list = map[top];
                }
                catch (Exception)
                {
                    list = new List<string>();
                }
                list.Add(line);
                map[top] = list;
            }
            foreach (String type in map.Keys)
            {
                List<String> ls = map[type];
                foreach (String item in ls)
                {
                    writer.WriteLine(item);
                }
            }
            reader.Close();
            writer.Close();
        }

    }
}
