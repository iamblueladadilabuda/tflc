using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace tflc_1
{
    internal class FileFunctions : ToolStripFunctions
    {
        private const int HISTORY_SIZE = 10;

        public (string, string, string[], string) Create(MenuStrip menuStrip, RichTextBox richTextBox,
            string tool_name, string save_text, List<(string[], string[], int)> files, int idx)
        {
            (string filename, string name) = New_ToolStrip(menuStrip);
            Create_ToolStrip(menuStrip, filename, name);
            string path = "cache/" + filename + ".txt";
            File.Create(path).Close();

            Save_List_Files(tool_name, richTextBox.Text, save_text, idx, files);

            richTextBox.Text = null;
            string[] history = new string[HISTORY_SIZE];
            history[0] = "";
            string[] file = { filename, path, richTextBox.Text, null };
            files.Add((file, history, 1));

            return (filename, path, history, "");
        }

        public (string, string, string[], string, string) Open(Form form, OpenFileDialog openFileDialog, 
            RichTextBox richTextBox, MenuStrip menuStrip, string tool_name, string save_text, 
            List<(string[], string[], int)> files, int idx)
        {
            Save_List_Files(tool_name, richTextBox.Text, save_text, idx, files);

            string filename = "", text = "";
            if (openFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                text = File.ReadAllText(openFileDialog.FileName);

                filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                if (!Find_File_In_ToolStrip(menuStrip, filename))
                {
                    Create_ToolStrip(menuStrip, filename, "file");
                }
            }

            string[] history = new string[HISTORY_SIZE];
            history[0] = text;
            save_text = text;
            string[] file = { filename, openFileDialog.FileName, text, save_text };
            files.Add((file, history, 1));

            return (filename, openFileDialog.FileName, history, text, save_text);
        }

        public string Save(RichTextBox richTextBox, string tool_name, string filename, 
            List<(string[], string[], int)> files)
        {
            ToolStripFunctions tool_functions = new ToolStripFunctions();
            string text; string[] his = new string[HISTORY_SIZE]; int idx;

            (filename, his, idx, text) = tool_functions.Click_Strip(tool_name, filename, files);
            if (filename == null) MessageBox.Show("Error: file path is null!");

            File.WriteAllText(filename, richTextBox.Text);
            return filename;
        }

        public (string, string, string) Save_How(Form form, SaveFileDialog saveFileDialog, RichTextBox richTextBox,
            MenuStrip menuStrip, string filename, List<(string[], string[], int)> files)
        {
            int prev = Find_File(files, filename);
            if (prev == -1)
            {
                MessageBox.Show("Nothing to save");
                return (null, null, null);
            }
            string[] history = files.ElementAt(prev).Item2;
            int idx = files.ElementAt(prev).Item3;
            saveFileDialog.FileName = filename;

            if (saveFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName + ".txt", richTextBox.Text);
            }

            string new_filename = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
            Change_Name_ToolStrip(menuStrip, filename, new_filename);

            string save_text = richTextBox.Text;
            files.RemoveAt(prev);
            string[] file = { new_filename, saveFileDialog.FileName, save_text, save_text };
            files.Add((file, history, idx));

            return (new_filename, saveFileDialog.FileName, save_text);
        }

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

        private void Save_List_Files(string tool_name, string text, string save_text, int idx,
            List<(string[], string[], int)> files)
        {
            int index = Find_File(files, tool_name);
            if (index == -1) return;
            string[] old_history = files.ElementAt(index).Item2;
            Roll_Strip(tool_name, text, save_text, old_history, idx, files);
        }
    }
}
