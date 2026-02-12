using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace tflc_1
{
    public partial class Compiler : Form
    {
        int width, height, num_line = 1;
        FileFunctions file_functions = new FileFunctions();
        ToolStripFunctions tool_functions = new ToolStripFunctions();
        List<(string, string, string)> files = new List<(string, string, string)>();
        string tool_name = "", path = "";

        public Compiler()
        {
            InitializeComponent();
            Update_Panels_Sizes();
            Change_Language(1);
            Clean();
            Open();
            panel7.Visible = false;
            numberBox.SelectionAlignment = HorizontalAlignment.Center;
        }

        private void Compiler_SizeChanged(object sender, EventArgs e)
        {
            if ((panel1.ClientSize.Height - 30) != height || ClientSize.Width != width)
            {
                Update_Panels_Sizes();
            }
        }

        private void Update_Panels_Sizes()
        {
            width = ClientSize.Width;
            height = panel1.ClientSize.Height - 100;
            panel3.Height = height / 2;
            panel5.Height = height / 2;
            panel4.Height = panel1.ClientSize.Height - height - 70;
            panel13.Width = 65;

            panel7.Width = panel3.Width / 2 + panel3.Width / 4;
            panel7.Height = height / 3;
            panel7.Location = new Point((ClientSize.Width / 2 - panel7.Width / 2), ClientSize.Height / 3);
            panel8.Height = panel7.Height - 20;
            panel9.Height = panel8.Height / 3;

            if (panel7.Width > 640)
            {
                panel12.Width = 5;
                exit.Width = panel7.Width / 16;
                yes.Width = exit.Width + exit.Width / 4;
                no.Width = yes.Width;
            }
            else
            {
                panel12.Width = 3;
                exit.Width = 40;
                yes.Width = 50;
                no.Width = 50;
            }

            if (panel9.Height > 30)
            {
                panel10.Height = panel7.Height / 7;
                panel11.Height = panel10.Height / 2;
            }
            else
            {
                panel10.Height = 16;
                panel11.Height = 2;
            }
        }

        private void create1_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Create(menuStrip3);
            richTextBox.Text = null;
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void create2_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Create(menuStrip3);
            richTextBox.Text = null;
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void open1_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Open(this, openFileDialog, richTextBox, menuStrip3);
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void open2_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Open(this, openFileDialog, richTextBox, menuStrip3);
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void save1_Click(object sender, EventArgs e)
        {
            string _;
            (path, _) = Find_File();
            if (path == null) MessageBox.Show("Error: file path is null!");
            file_functions.Save(richTextBox, path);
        }

        private void saveHow1_Click(object sender, EventArgs e)
        {
            int prev = files.IndexOf((tool_name, path, richTextBox.Text));
            (tool_name, path) = file_functions.Save_How(this, saveFileDialog, richTextBox, menuStrip3, tool_name);
            files.RemoveAt(prev);
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void save2_Click(object sender, EventArgs e)
        {
            int prev = files.IndexOf((tool_name, path, richTextBox.Text));
            (tool_name, path) = file_functions.Save_How(this, saveFileDialog, richTextBox, menuStrip3, tool_name);
            files.RemoveAt(prev);
            files.Add((tool_name, path, richTextBox.Text));
        }

        private void help1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1fWNk5rWH6WQS7mHoRATFV-HjUk_kn4-cbsnOeN8V2jE/edit?usp=sharing");
        }

        private void quit1_Click(object sender, EventArgs e)
        {
            panel7.Visible = true;
        }

        private void exit_Click(object sender, EventArgs e)
        {
            panel7.Visible = false;
        }

        private void no_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void yes_Click(object sender, EventArgs e)
        {
            file_functions.Save_How(this, saveFileDialog, richTextBox, menuStrip3, tool_name);
            Close();
        }

        private (string, string) Find_File()
        {
            for (int index = 0; index < files.Count; index++)
            {
                if (files.ElementAt(index).Item1 == tool_name)
                {
                    path = files.ElementAt(index).Item2;
                    string text = files.ElementAt(index).Item3;
                    return (path, text);
                }
            }
            return (null, null);
        }

        private void menuStrip3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripMenuItem clickedItem)
            {
                tool_name = clickedItem.Text;
                string text;
                (path, text) = Find_File();
                richTextBox.Text = text;
            }
        }

        private void richTextBox_VScroll(object sender, EventArgs e)
        {
            int i = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(new Point(1, 1)));
            numberBox.SelectionStart = numberBox.GetFirstCharIndexFromLine(i);
            numberBox.ScrollToCaret();
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            int count_line = richTextBox.Lines.Length + 1;
            if (count_line > num_line)
            {
                while (count_line != num_line)
                {
                    numberBox.Text += num_line.ToString() + "\n";
                    num_line++;
                }
            }
            else if (count_line < num_line)
            {
                while (count_line != num_line)
                {
                    if (num_line == 2) break;
                    num_line--;
                    numberBox.Text = "";
                    for (int i = 1; i < num_line; i++)
                    {
                        numberBox.Text += i.ToString() + "\n";
                    }
                }
            }
        }

        private void Compiler_FormClosing(object sender, FormClosingEventArgs e)
        {
            string delete_lines = "";
            string open_lines = "";
            foreach (ToolStripMenuItem item in menuStrip3.Items)
            {
                if (item.Text.StartsWith("Untitled-"))
                {
                    string filename = "files/" + item.Text + ".txt";
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
            File.WriteAllText("files/delete.txt", delete_lines);
            File.WriteAllText("files/open.txt", open_lines);
        }

        private void Clean()
        {
            if (File.Exists("files/delete.txt"))
            {
                string[] delete = File.ReadAllLines("files/delete.txt");
                foreach (string line in delete)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        File.Delete(line);
                    }
                }
            }
        }

        private void Open()
        {
            if (File.Exists("files/open.txt"))
            {
                string[] open = File.ReadAllLines("files/open.txt");
                foreach (string filename in open)
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        string text = File.ReadAllText(filename);
                        richTextBox.Text = text;
                        path = filename;
                        tool_name = filename.Split('/')[1].Split('.')[0];
                        tool_functions.Create_ToolStrip(menuStrip3, tool_name, "file");
                        files.Add((tool_name, path, text));
                    }
                }
            }
        }

        private string[] Read_Language(string filename) => File.ReadAllLines(filename);

        private void Change_Language(int choice)
        {
            bool[] visible = new bool[3] { true, true, true };
            switch (choice)
            {
                case 1:
                    visible[0] = false;
                    Language_Visible(visible);
                    Translate(Read_Language("txt/ru.txt"));
                    break;
                case 2:
                    visible[1] = false;
                    Language_Visible(visible);
                    Translate(Read_Language("txt/en.txt"));
                    break;
                case 3:
                    visible[2] = false;
                    Language_Visible(visible);
                    Translate(Read_Language("txt/kaz.txt"));
                    break;
            }
        }

        private void Language_Visible(bool[] visible)
        {
            rusLan1.Visible = visible[0];
            enLan1.Visible = visible[1];
            kazLan1.Visible = visible[2];
        }

        private void rusLan1_Click(object sender, EventArgs e) => Change_Language(1);

        private void enLan1_Click(object sender, EventArgs e) => Change_Language(2);

        private void kazLan1_Click(object sender, EventArgs e) => Change_Language(3);

        private void Translate(string[] language)
        {
            file1.Text = language[0];
            correction1.Text = language[1];
            text1.Text = language[2];
            start1.Text = language[3];
            help1.Text = language[4];
            create1.Text = language[5];
            open1.Text = language[6];
            save1.Text = language[7];
            saveHow1.Text = language[8];
            language1.Text = language[9];
            quit1.Text = language[10];
            cancel1.Text = language[11];
            return1.Text = language[12];
            cut1.Text = language[13];
            copy1.Text = language[14];
            enter1.Text = language[15];
            delete1.Text = language[16];
            select1.Text = language[17];
            settingTask1.Text = language[18];
            grammar1.Text = language[19];
            grammerClassification1.Text = language[20];
            methodAnalyze1.Text = language[21];
            example1.Text = language[22];
            literature1.Text = language[23];
            code1.Text = language[24];
            rusLan1.Text = language[25];
            enLan1.Text = language[26];
            kazLan1.Text = language[27];
            confExit.Text = language[28];
            confirmation.Text = language[29];
            yes.Text = language[30];
            no.Text = language[31];
        }
    }
}
