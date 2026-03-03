using System.Collections.Generic;
using System.Linq;

namespace tflc_1
{
    internal class ListFileFunctions 
    {
        private const int HISTORY_SIZE = 10;

        public int Find_File(List<(string[], string[], int)> files, string tool_name)
        {
            for (int index = 0; index < files.Count; index++)
            {
                if (files.ElementAt(index).Item1[0] == tool_name)
                {
                    return index;
                }
            }
            return -1;
        }

        public void Save_List_Files(int file_idx, string[] file, List<(string[], string[], int)> files)
        {
            files.ElementAt(file_idx).Item1[0] = file[0];
            files.ElementAt(file_idx).Item1[1] = file[1];
            files.ElementAt(file_idx).Item1[2] = file[2];
            files.ElementAt(file_idx).Item1[3] = file[3];
        }

        public int Add_List_Files(List<(string[], string[], int)> files, string his_text, 
            string[] file)
        {
            string[] history = new string[HISTORY_SIZE];
            history[0] = his_text;

            files.Add((file, history, 1));

            return files.Count() - 1;
        }
    }
}
