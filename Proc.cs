using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fnr
{
    class Proc
    {
        public Proc()
        {

        }

        public string Dir { get; set; }
        public string File_mask { get; set; }
        public string Exclude_mask { get; set; }
        public string Find_block { get; set; }
        public string Replace_block { get; set; }
        public bool All_dir { get; set; }
        public bool Is_replace { get; set; }

        public class Files_result : Results
        {

        }

        public string[] Get_files()
        {
            string[] file_mask = File_mask.Split(',');
            return All_dir ? Get_sub_dir(file_mask) : Get_cur_dir(file_mask);
        }

        private string[] Get_sub_dir(string[] file_mask)
        {
            var files = new List<string>();
            foreach (var mask in file_mask)
                foreach (var file in Directory.GetFiles(Dir, mask, SearchOption.AllDirectories).Where(Check_exlсude).ToArray())
                    files.Add(file);
            return files.ToArray();
        }

        private string[] Get_cur_dir(string[] file_mask)
        {
            var files = new List<string>();
            foreach (var mask in file_mask)
                foreach (var file in Directory.GetFiles(Dir, mask, SearchOption.TopDirectoryOnly).Where(Check_exlсude).ToArray())
                    files.Add(file);
            return files.ToArray();
        }

        private bool Check_exlсude(string name)
        {
            if (string.IsNullOrWhiteSpace(Exclude_mask)) return true;
            string[] exclude_mask;
            exclude_mask = Exclude_mask.Split(',');
            var exclude_length = exclude_mask.Length;
            foreach (var mask in exclude_mask)
            {
                if (mask.Length == 0) { exclude_length--; continue; }
                if (mask[0] == ' ') mask.TrimStart(' ');
                Regex _mask = new Regex('.' + mask.Replace(".", "\\."));
                if (!_mask.IsMatch(name)) exclude_length--;
            }

            return exclude_length == 0;
        }

        public IEnumerable<Results> FNR(string[] received_files)
        {

            foreach (string file_path in received_files)
            {
                var result_item = Is_replace ? Replace(file_path) : Find(file_path);
                if (result_item.Check_include)  yield return result_item;
                else    yield return null;
            }
            yield break;
        }

        private Files_result Find(string file_path)
        {
            var result_item = new Files_result();
            using (StreamReader sr = new StreamReader(file_path, true))
            {
                var code = sr.CurrentEncoding.ToString();
                var text = File.ReadAllText(file_path, Encoding.GetEncoding(code));
                if (text.Contains(Find_block))
                {
                    result_item.File_path = text;
                    result_item.Success = true;
                    result_item.Conformity = new Regex(Find_block).Matches(text).Count;
                }
            }

            return result_item;
        }

        private Files_result Replace(string file_path)
        {
            var result_item = new Files_result();
            var text = File.ReadAllText(file_path);
            if (text.Contains(Find_block))
            {
                result_item.File_path = file_path;
                result_item.Success = true;
                result_item.Conformity = new Regex(Find_block).Matches(text).Count;
                var new_text = text.Replace(Find_block, Replace_block);
                File.WriteAllText(file_path, new_text);
            }
            return result_item;
        } 
    }
}
