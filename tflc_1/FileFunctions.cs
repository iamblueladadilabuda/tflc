using System;
using System.IO;
using System.Windows.Forms;

namespace tflc_1
{
    internal class FileFunctions : ToolStripFunctions
    {
        private readonly ListFileFunctions ls_file = new ListFileFunctions();

        public string Create(string filename)
        {
            try
            {
                string path = "cache/" + filename + ".txt";
                File.Create(path).Close();
                return path;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in the Create function (FileFunctions): " + e.Message);
                return null;
            }
        }

        public (string, string) Open_Drop_File(string path, MenuStrip menuStrip)
        {
            try
            {
                string text = File.ReadAllText(path);
                string filename = Path.GetFileNameWithoutExtension(path);

                if (!Find_File_In_ToolStrip(menuStrip, filename))
                {
                    Create_ToolStrip(menuStrip, filename, "file");
                }

                return (filename, text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in the Open_Drop_File function (FileFunctions): " + e.Message);
                return (null, null);
            }
        }

        public (string, string, string) Open(Form form, MenuStrip menuStrip)
        {
            try
            {
                string filename = "", text = "";
                OpenFileDialog openFileDialog = new OpenFileDialog();

                string base_directory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog.InitialDirectory = Path.Combine(base_directory, "txt", "tests");
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog(form) == DialogResult.OK)
                {
                    text = File.ReadAllText(openFileDialog.FileName);

                    filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    if (!Find_File_In_ToolStrip(menuStrip, filename))
                    {
                        Create_ToolStrip(menuStrip, filename, "file");
                    }
                }

                return (filename, openFileDialog.FileName, text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in the Open function (FileFunctions): " + e.Message);
                return (null, null, null);
            }
        }

        public void Save(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in the Save function (FileFunctions): " + e.Message);
            }
            return;
        }

        public (string, string) Save_How(Form form, string filename, string text)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = filename;

                if (saveFileDialog.ShowDialog(form) == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, text);
                }

                string new_filename = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                return (new_filename, saveFileDialog.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in the Save_How function (FileFunctions): " + e.Message);
                return (null, null);
            }
        }
    }
}
