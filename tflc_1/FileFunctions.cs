using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class FileFunctions
    {
        private (string, string) New_ToolStrip(MenuStrip menuStrip)
        {
            int new_number = 1;

            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                int number = Convert.ToInt32(item.Text.Split('-')[1]);
                if (number != new_number)
                {
                    new_number = number;
                    break;
                }
                else
                {
                    new_number++;
                }  
            }

            string filename = "Untitled-" + new_number.ToString();
            string name = "toolStrip_" + new_number.ToString();

            return (filename, name);
        }

        private string Create_ToolStrip(MenuStrip menuStrip)
        {
            ToolStripMenuItem tool_strip = new ToolStripMenuItem();
            (string filename, string name) = New_ToolStrip(menuStrip);
            tool_strip.Text = filename;
            tool_strip.Name = name;
            menuStrip.Items.Add(tool_strip);
            return filename;
        }

        private void Create_File(string filename)
        {
            File.Open("files/" + filename + ".txt", FileMode.Create);
        }

        public void Create(MenuStrip menuStrip)
        {
            string filename = Create_ToolStrip(menuStrip);
            Create_File(filename);
        }

        public void Open(Form form, OpenFileDialog openFileDialog, RichTextBox richTextBox)
        {
            if (openFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                string[] file_line = File.ReadAllLines(openFileDialog.FileName);

                string text = "";
                foreach (string line in file_line)
                {
                    text += line;
                }

                richTextBox.Text = text;
            }
        }

        public void Save(Form form, OpenFileDialog openFileDialog, RichTextBox richTextBox)
        {
            if (openFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                File.WriteAllText(openFileDialog.FileName, richTextBox.Text);
            }
        }
    }
}
