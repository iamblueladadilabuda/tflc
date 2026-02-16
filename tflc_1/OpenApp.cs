using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace tflc_1
{
    internal class OpenApp : ToolStripFunctions
    {
        public void Close(MenuStrip menuStrip)
        {
            string delete_lines = "";
            string open_lines = "";
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text.StartsWith("Untitled-"))
                {
                    string filename = "cache/" + item.Text + ".txt";
                    if (File.Exists(filename))
                    {
                        if (string.IsNullOrEmpty(File.ReadAllText(filename)))
                        {
                            delete_lines += filename + "\n";
                        }
                        else
                        {
                            open_lines += filename + "\n";
                        }
                    }
                }
            }
            File.WriteAllText("cache/delete.txt", delete_lines);
            File.WriteAllText("cache/open.txt", open_lines);
        }

        public void Clean()
        {
            if (File.Exists("cache/delete.txt"))
            {
                string[] delete = File.ReadAllLines("cache/delete.txt");
                foreach (string line in delete)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        File.Delete(line);
                    }
                }
            }
        }

        public (string, string, string[]) Open(MenuStrip menuStrip, RichTextBox richTextBox,
            List<(string[], string[], int)> files)
        {
            string tool_name = "", path = "";
            string[] history_def = new string[10];
            history_def[0] = "";

            if (File.Exists("cache/open.txt"))
            {
                string[] open = File.ReadAllLines("cache/open.txt");
                foreach (string filename in open)
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        string text = File.ReadAllText(filename);
                        tool_name = filename.Split('/')[1].Split('.')[0];
                        path = filename;

                        string[] history = new string[10];
                        history[0] = text;
                        history_def = history;

                        richTextBox.Text = text;
                        string[] file = new string[] { tool_name, path, text };
                        files.Add((file, history, 1));
                        Create_ToolStrip(menuStrip, tool_name, "file");
                    }
                }
            }

            return (tool_name, path, history_def);
        }
    }
}
