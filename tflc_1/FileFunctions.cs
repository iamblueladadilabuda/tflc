using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace tflc_1
{
    internal class FileFunctions : ToolStripFunctions
    {
        public string Create(MenuStrip menuStrip)
        {
            (string filename, string name) = New_ToolStrip(menuStrip);
            Create_ToolStrip(menuStrip, filename, name);
            File.Open("files/" + filename + ".txt", FileMode.Create);
            return filename;
        }

        public void Open(Form form, OpenFileDialog openFileDialog, RichTextBox richTextBox, 
            MenuStrip menuStrip)
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

                string filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                if (!Find_File_In_ToolStrip(menuStrip, filename))
                {
                    Create_ToolStrip(menuStrip, filename, "file");
                } 
            }
        }

        public void Save(Form form, SaveFileDialog saveFileDialog, RichTextBox richTextBox)
        {

        }

        public void Save_How(Form form, SaveFileDialog saveFileDialog, RichTextBox richTextBox,
            MenuStrip menuStrip, string filename)
        {
            saveFileDialog.FileName = filename;

            if (saveFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, richTextBox.Text);
            }

            string new_filename = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
            Change_Name_ToolStrip(menuStrip, filename, new_filename);
        }
    }
}
