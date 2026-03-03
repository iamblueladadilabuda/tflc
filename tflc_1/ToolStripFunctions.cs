using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace tflc_1
{
    internal class ToolStripFunctions : ListFileFunctions
    {
        public (string, string) New_ToolStrip(MenuStrip menuStrip)
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

        public void Create_ToolStrip(MenuStrip menuStrip, string filename, string name)
        {
            ToolStripMenuItem tool_strip = new ToolStripMenuItem();
            tool_strip.Text = filename;
            tool_strip.Name = name;
            menuStrip.Items.Add(tool_strip);
        }

        public void Click_Strip(MenuStrip menuStrip, string prev_tool, string new_tool)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (prev_tool == null)
                {
                    item.BackColor = Color.CornflowerBlue;
                    return;
                }
                if (item.Text == prev_tool)
                {
                    item.BackColor = SystemColors.Control;
                }
                if (item.Text == new_tool)
                {
                    item.BackColor = Color.CornflowerBlue;
                }
            }
        }

        public void Change_Name_ToolStrip(MenuStrip menuStrip, string old_filename, string new_filename)
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

        public int Delete_ToolStrip(MenuStrip menuStrip, int file_idx, List<(string[], string[], int)> files)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == files.ElementAt(file_idx).Item1[0])
                {
                    files.RemoveAt(file_idx);
                    menuStrip.Items.Remove(item);
                    return files.Count() - 1;
                }
            }
            return -2;
        }

        public int ToolStrip_For_Close(string tool_name, List<(string[], string[], int)> files)
        {
            if (tool_name == "") return -2;
            int index = Find_File(files, tool_name);
            if (index == -1)
            {
                MessageBox.Show("Error in Find_File function: index = -1");
                return -1;
            }

            if (files.ElementAt(index).Item1[2] != files.ElementAt(index).Item1[3])
            {
                return index;
            }

            return -1;
        }

        public int Close_ToolStrip(int file_idx, MenuStrip menuStrip, Form form, 
            RichTextBox richTextBox, Label condition, List<(string[], string[], int)> files)
        {
            FileFunctions file_functions = new FileFunctions();

            string prev_tool = files.ElementAt(file_idx).Item1[0];

            (string new_tool, string path) = file_functions.Save_How(form, prev_tool, richTextBox.Text);

            Change_Name_ToolStrip(menuStrip, prev_tool, new_tool);

            string text = richTextBox.Text;
            string[] file = { new_tool, path, text, text };
            Save_List_Files(file_idx, file, files);

            condition.Text = "Successful file saving how!";

            if (new_tool != null || path != null)
            {
                file_idx = Delete_ToolStrip(menuStrip, file_idx, files);
            }

            return file_idx;
        }

        public void Close_All_ToolStrip(MenuStrip menuStrip, Form form, RichTextBox richTextBox, 
            Label condition, List<(string[], string[], int)> files)
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
                Close_ToolStrip(index, menuStrip, form, richTextBox, condition, files);
            }
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
    }
}
