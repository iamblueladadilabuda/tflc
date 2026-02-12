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
        public (string, string) Create(MenuStrip menuStrip)
        {
            (string filename, string name) = New_ToolStrip(menuStrip);
            Create_ToolStrip(menuStrip, filename, name);
            string path = "files/" + filename + ".txt";
            File.Create(path).Close();
            return (filename, path);
        }

        public (string, string) Open(Form form, OpenFileDialog openFileDialog, RichTextBox richTextBox, 
            MenuStrip menuStrip)
        {
            string filename = "";
            if (openFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                string[] file_line = File.ReadAllLines(openFileDialog.FileName);

                string text = "";
                foreach (string line in file_line)
                {
                    text += line;
                }

                richTextBox.Text = text;

                filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                if (!Find_File_In_ToolStrip(menuStrip, filename))
                {
                    Create_ToolStrip(menuStrip, filename, "file");
                }
            }
            return (filename, openFileDialog.FileName);
        }

        public void Save(RichTextBox richTextBox, string filename)
        {
            File.WriteAllText(filename, richTextBox.Text);
        }

        public (string, string) Save_How(Form form, SaveFileDialog saveFileDialog, RichTextBox richTextBox,
            MenuStrip menuStrip, string filename)
        {
            saveFileDialog.FileName = filename;

            if (saveFileDialog.ShowDialog(form) == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, richTextBox.Text);
            }

            string new_filename = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
            Change_Name_ToolStrip(menuStrip, filename, new_filename);

            return (new_filename, saveFileDialog.FileName);
        }
    }
}
