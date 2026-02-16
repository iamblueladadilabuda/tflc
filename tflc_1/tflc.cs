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
        private FileFunctions file_functions = new FileFunctions();
        private ToolStripFunctions tool_functions = new ToolStripFunctions();
        private NumberingLines numbering = new NumberingLines();
        private OpenApp open = new OpenApp();
        TextFunctions text_functions = new TextFunctions();

        private const int HISTORY_SIZE = 10;

        private List<(string, string, string, string[], int)> files = new List<(string, string, string, string[], int)>();
        private int width, height;
        private string tool_name = "", path = "", buffer = "";
        private bool closing = false;
        private string[] history = new string[HISTORY_SIZE];
        int idx = 1;

        public Compiler()
        {
            InitializeComponent();
            Update_Panels_Sizes();
            Change_Language(1);
            open.Clean();
            (tool_name, path, history) = open.Open(menuStrip3, richTextBox, files);
            numbering.Numbering_Lines(richTextBox, numberBox);
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
            history = new string[history.Length];
            history[0] = "";
            files.Add((tool_name, path, richTextBox.Text, history, 1));
        }

        private void create2_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Create(menuStrip3);
            richTextBox.Text = null;
            history = new string[history.Length];
            history[0] = "";
            files.Add((tool_name, path, richTextBox.Text, history, 1));
        }

        private void open1_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Open(this, openFileDialog, richTextBox, menuStrip3);
            history = new string[history.Length];
            history[0] = richTextBox.Text;
            files.Add((tool_name, path, richTextBox.Text, history, 1));
        }

        private void open2_Click(object sender, EventArgs e)
        {
            (tool_name, path) = file_functions.Open(this, openFileDialog, richTextBox, menuStrip3);
            history = new string[history.Length];
            history[0] = richTextBox.Text;
            files.Add((tool_name, path, richTextBox.Text, history, 1));
        }

        private void save1_Click(object sender, EventArgs e)
        {
            string _;
            int index = file_functions.Find_File(files, tool_name);
            if (index == -1) return;

            (path, _, history, idx) = tool_functions.Click_Strip(index, path, files);
            if (path == null) MessageBox.Show("Error: file path is null!");
            file_functions.Save(richTextBox, path);
        }

        private void saveHow1_Click(object sender, EventArgs e)
        {
            int prev = files.IndexOf((tool_name, path, richTextBox.Text, history, idx));
            (tool_name, path) = file_functions.Save_How(this, saveFileDialog, richTextBox, menuStrip3, tool_name);
            files.RemoveAt(prev);
            files.Add((tool_name, path, richTextBox.Text, history, idx));
        }

        private void save2_Click(object sender, EventArgs e)
        {
            int prev = files.IndexOf((tool_name, path, richTextBox.Text, history, idx));
            (tool_name, path) = file_functions.Save_How(this, saveFileDialog, richTextBox, menuStrip3, tool_name);
            files.RemoveAt(prev);
            files.Add((tool_name, path, richTextBox.Text, history, idx));
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


        private void menuStrip3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripMenuItem clickedItem)
            {
                int index = file_functions.Find_File(files, tool_name);
                if (index == -1) return;
                files.RemoveAt(index);
                files.Add((tool_name, path, richTextBox.Text, history, idx));

                tool_name = clickedItem.Text;

                string text;
                history = new string[history.Length];
                index = file_functions.Find_File(files, tool_name);
                if (index == -1) return;
                (path, text, history, idx) = tool_functions.Click_Strip(index, path, files);
                richTextBox.Text = text;
            }
        }

        private void richTextBox_VScroll(object sender, EventArgs e)
        {
            numbering.Scroll(richTextBox, numberBox);
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            numbering.Numbering_Lines(richTextBox, numberBox);
            if (richTextBox.Text == history[idx - 1]) return;
            idx = text_functions.Add_History(history, richTextBox.Text, idx);
        }


        private void Change_Buffer(int func)
        {
            string buf = "";
            switch (func)
            {
                case 1:
                    buf = text_functions.Copy(richTextBox);
                    break;
                case 2:
                    buf = text_functions.Cut(richTextBox);
                    break;
            }
            if (buf != null) buffer = buf;
        }

        private void cancel1_Click(object sender, EventArgs e)
        {
            idx = text_functions.Cancel(richTextBox, history, idx);
        }

        private void left2_Click(object sender, EventArgs e)
        {
            idx = text_functions.Cancel(richTextBox, history, idx);
        }

        private void return1_Click(object sender, EventArgs e)
        {
            idx = text_functions.Repeat(richTextBox, history, idx);
        }

        private void rigth2_Click(object sender, EventArgs e)
        {
            idx = text_functions.Repeat(richTextBox, history, idx);
        }

        private void copy1_Click(object sender, EventArgs e)
        {
            Change_Buffer(1);
        }

        private void copy2_Click(object sender, EventArgs e)
        {
            Change_Buffer(1);
        }

        private void cut1_Click(object sender, EventArgs e)
        {
            Change_Buffer(2);
        }

        private void cut2_Click(object sender, EventArgs e)
        {
            Change_Buffer(2);
        }

        private void enter1_Click(object sender, EventArgs e)
        {
            text_functions.Paste(richTextBox, buffer);
        }

        private void enter2_Click(object sender, EventArgs e)
        {
            text_functions.Paste(richTextBox, buffer);
        }

        private void delete1_Click(object sender, EventArgs e)
        {
            text_functions.Delete(richTextBox);
        }

        private void select1_Click(object sender, EventArgs e)
        {
            text_functions.Select_All(richTextBox);
        }


        private void Panel7_VisibleChanged(object sender, EventArgs e)
        {
            if (!panel7.Visible && closing)
            {
                panel7.VisibleChanged -= Panel7_VisibleChanged;
                Close();
            }
        }

        private void Compiler_FormClosing(object sender, FormClosingEventArgs e)
        {
            open.Close(menuStrip3);

            if (!closing)
            {
                e.Cancel = true;

                panel7.Visible = true;
                closing = true;

                panel7.VisibleChanged += Panel7_VisibleChanged;
            }
        }

        private void rusLan1_Click(object sender, EventArgs e) => Change_Language(1);
        private void enLan1_Click(object sender, EventArgs e) => Change_Language(2);
        private void kazLan1_Click(object sender, EventArgs e) => Change_Language(3);

        private void Change_Language(int choice)
        {
            bool[] visible = new bool[3] { true, true, true };
            switch (choice)
            {
                case 1:
                    visible[0] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines("txt/ru.txt"));
                    break;
                case 2:
                    visible[1] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines("txt/en.txt"));
                    break;
                case 3:
                    visible[2] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines("txt/kaz.txt"));
                    break;
            }
        }

        private void Language_Visible(bool[] visible)
        {
            rusLan1.Visible = visible[0];
            enLan1.Visible = visible[1];
            kazLan1.Visible = visible[2];
        }

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
