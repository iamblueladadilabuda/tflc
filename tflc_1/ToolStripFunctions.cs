using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace tflc_1
{
    internal class ToolStripFunctions : ListFileFunctions
    {
        public int Count_ToolStrip(MenuStrip menuStrip)
        {
            int count = 0;
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                count++;
            }
            return count;
        }

        public (string, string[], int, string) Click_Strip(string tool_name, string path,
            List<(string[], string[], int)> files)
        {
            FileFunctions file_functions = new FileFunctions();
            int index = file_functions.Find_File(files, tool_name);
            if (index == -1) return (null, null, -1, null);

            path = files.ElementAt(index).Item1[1];
            string text = files.ElementAt(index).Item1[2];
            string[] history = files.ElementAt(index).Item2;
            int idx = files.ElementAt(index).Item3;
            return (path, history, idx, text);
        }

        public void Roll_Strip(string tool_name, string text, string save, string[] history, 
            int idx, List<(string[], string[], int)> files)
        {
            FileFunctions file_functions = new FileFunctions();
            int index = file_functions.Find_File(files, tool_name);
            if (index == -1) return;

            string path = files.ElementAt(index).Item1[1];
            files.RemoveAt(index);
            string[] file = { tool_name, path, text, save };
            files.Add((file, history, idx));
        }

        public void Create_ToolStrip(MenuStrip menuStrip, string filename, string name)
        {
            ToolStripMenuItem tool_strip = new ToolStripMenuItem();
            tool_strip.Text = filename;
            tool_strip.Name = name;
            menuStrip.Items.Add(tool_strip);
        }

        public void Delete_ToolStrip(MenuStrip menuStrip, string tool_name,
            List<(string[], string[], int)> files)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == tool_name)
                {
                    int index = Find_File(files, tool_name);
                    if (index == -1) return;
                    files.RemoveAt(index);
                    menuStrip.Items.Remove(item);
                    break;
                }
            }
        }

        public int ToolStrip_For_Close(string tool_name, List<(string[], string[], int)> files)
        {
            if (tool_name == "") return -2;
            int index = Find_File(files, tool_name);
            if (index == -1) return -2;

            if (string.IsNullOrEmpty(files.ElementAt(index).Item1[3]))
            {
                return index;
            }

            return -1;
        }

        public void Close_ToolStrip(MenuStrip menuStrip, string tool_name, Form form, 
            SaveFileDialog saveFileDialog, RichTextBox richTextBox, Label condition,
            List<(string[], string[], int)> files)
        {
            FileFunctions file_functions = new FileFunctions();
            string filename, b, c;
            (filename, b, c, condition.Text) = file_functions.Save_How(form, richTextBox, menuStrip, tool_name, files);
            if (filename != null || b != null || c != null)
            {
                Delete_ToolStrip(menuStrip, filename, files);
            }
        }

        public void Close_All_ToolStrip(MenuStrip menuStrip, Form form,
            SaveFileDialog saveFileDialog, RichTextBox richTextBox, Label condition,
            List<(string[], string[], int)> files)
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                items.Add(item);
            }

            foreach (ToolStripMenuItem item in items)
            {
                int index = ToolStrip_For_Close(item.Text, files);
                if (index == -1) continue;
                Close_ToolStrip(menuStrip, item.Text, form, saveFileDialog, richTextBox, condition, files);
            }
        }

        protected (string, string) New_ToolStrip(MenuStrip menuStrip)
        {
            List<int> numbers = new List<int>();
            int new_number = 1;

            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (!item.Text.StartsWith("Untitled-")) continue;
                numbers.Add(Convert.ToInt32(item.Text.Split('-')[1]));               
            }

            numbers.Sort();
            foreach (int number in numbers)
            {
                if (number != new_number) break;
                new_number++;
            }

            string filename = "Untitled-" + new_number.ToString();
            string name = "toolStrip_" + new_number.ToString();

            return (filename, name);
        }

        protected bool Find_File_In_ToolStrip(MenuStrip menuStrip, string filename)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == filename)
                {
                    return true;
                }
            }
            return false;
        }

        protected void Change_Name_ToolStrip(MenuStrip menuStrip, string old_filename, 
            string new_filename)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == old_filename)
                {
                    item.Text = new_filename;
                    return;
                }
            }
        }
    }
}
