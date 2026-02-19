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
        private readonly ListFileFunctions ls_file = new ListFileFunctions();

        public (string, string, string[], string, string) Create(MenuStrip menuStrip, 
            RichTextBox richTextBox, string tool_name, string save_text, 
            List<(string[], string[], int)> files, int idx)
        {
            (string filename, string name) = New_ToolStrip(menuStrip);
            Create_ToolStrip(menuStrip, filename, name);
            string path = "cache/" + filename + ".txt";
            File.Create(path).Close();

            ls_file.Save_List_Files(tool_name, richTextBox.Text, save_text, idx, files);

            richTextBox.Text = null;
            string[] file = { filename, path, richTextBox.Text, null };
            string[] history = ls_file.Add_List_Files(files, "", file);

            return (filename, path, history, "", "Successful file creation!");
        }

        public (string, string[], bool, string, string, string) Open_Drop_File(string path,
            RichTextBox richTextBox, MenuStrip menuStrip, string tool_name, string save_text,
            List<(string[], string[], int)> files, int idx)
        {
            ls_file.Save_List_Files(tool_name, richTextBox.Text, save_text, idx, files);

            string filename = "", text = "";
            text = File.ReadAllText(path);

            filename = Path.GetFileNameWithoutExtension(path);
            if (!Find_File_In_ToolStrip(menuStrip, filename))
            {
                Create_ToolStrip(menuStrip, filename, "file");
            }

            save_text = text;
            string[] file = { filename, path, text, save_text };
            string[] history = ls_file.Add_List_Files(files, text, file);

            return (filename, history, false, text, save_text, "Successful file opening!");
        }

        public (string, string, string[], bool, string, string, string) Open(Form form, 
            RichTextBox richTextBox, MenuStrip menuStrip, string tool_name, string save_text, 
            List<(string[], string[], int)> files, int idx)
        {
            ls_file.Save_List_Files(tool_name, richTextBox.Text, save_text, idx, files);

            string filename = "", text = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                text = File.ReadAllText(openFileDialog.FileName);

                filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                if (!Find_File_In_ToolStrip(menuStrip, filename))
                {
                    Create_ToolStrip(menuStrip, filename, "file");
                }
            }

            save_text = text;
            string[] file = { filename, openFileDialog.FileName, text, save_text };
            string[] history = ls_file.Add_List_Files(files, text, file);

            return (filename, openFileDialog.FileName, history, false, text, save_text, "Successful file opening!");
        }

        public (string, string) Save(RichTextBox richTextBox, string tool_name, string filename, 
            List<(string[], string[], int)> files)
        {
            ToolStripFunctions tool_functions = new ToolStripFunctions();
            string text; string[] his; int idx;

            (filename, his, idx, text) = tool_functions.Click_Strip(tool_name, filename, files);
            if (filename == null) MessageBox.Show("Error: file path is null!");

            File.WriteAllText(filename, richTextBox.Text);
            return (filename, "Successful file saving!");
        }

        public (string, string, string, string) Save_How(Form form, 
            RichTextBox richTextBox, MenuStrip menuStrip, string filename, 
            List<(string[], string[], int)> files)
        {
            int prev = ls_file.Find_File(files, filename);
            if (prev == -1)
            {
                MessageBox.Show("Nothing to save");
                return (null, null, null, null);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
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

            return (new_filename, saveFileDialog.FileName, save_text, "Successful file saving how!");
        }
    }
}
