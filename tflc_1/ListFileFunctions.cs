using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public void Save_List_Files(string tool_name, string text, string save_text, int idx,
            List<(string[], string[], int)> files)
        {
            ToolStripFunctions tool_functions = new ToolStripFunctions();
            int index = Find_File(files, tool_name);
            if (index == -1) return;
            string[] old_history = files.ElementAt(index).Item2;
            tool_functions.Roll_Strip(tool_name, text, save_text, old_history, idx, files);
        }

        public string[] Add_List_Files(List<(string[], string[], int)> files, string his_text, 
            string[] file)
        {
            string[] history = new string[HISTORY_SIZE];
            history[0] = his_text;
            files.Add((file, history, 1));
            return history;
        }
    }
}
