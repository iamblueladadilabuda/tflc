using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace tflc_1
{
    public partial class Compiler : Form
    {
        private const string path_language = "txt/language/";

        private readonly SyntaxHighlighting syntax_highlighting = new SyntaxHighlighting();
        private readonly ToolStripFunctions tool_functions = new ToolStripFunctions();
        private readonly ListFileFunctions ls_functions = new ListFileFunctions();
        private readonly TableFunctions table_functions = new TableFunctions();
        private readonly TextFunctions text_functions = new TextFunctions();
        private readonly FontFunctions font_functions = new FontFunctions();
        private readonly FileFunctions file_functions = new FileFunctions();
        private readonly NumberingLines numbering = new NumberingLines();
        private readonly OpenApp open = new OpenApp();

        private List<(string[], string[], int)> files = new List<(string[], string[], int)>();
        private bool closing = false, close_all = false, new_tool = true, escape = false;
        private readonly DataGridView table = new DataGridView();
        private int width, height, his_idx = 1, file_idx = 0;
        private readonly SplitContainer splitContainer;
        private string buffer = "";
        private int language = 1;


        public Compiler()
        {
            InitializeComponent();

            panel7.Visible = false;
            SplitContainerFunctions split = new SplitContainerFunctions();
            splitContainer = split.Create_SplitContainer(panel6, panel3, panel5);
            Update_Panels_Sizes();

            Change_Language(1);
            font_functions.Initialize_FontSizes(fontSizes1);

            numberBox.SelectionAlignment = HorizontalAlignment.Center;

            open.Clean();
            file_idx = open.Open(menuStrip3, richTextBox, files);
            if (file_idx != -1)
            {
                tool_functions.Click_Strip(menuStrip3, null, null);
            }

            numbering.Numbering_Lines(richTextBox, numberBox);

            if (menuStrip3.Items.Count == 0)
            {
                Create();
                menuStrip3.Items[0].BackColor = Color.CornflowerBlue;
            }

            table = table_functions.Initialize_Table(1);
            panel5.Controls.Add(table);
            table.CellClick += table_CellClick;

            syntax_highlighting.Syntax_Color();

            KeyPreview = true;
            KeyDown += new KeyEventHandler(Compiler_KeyDown);

            fontSizes1.SelectedIndexChanged += FontSizes1_SelectedIndexChanged;

            DragEnter += Compiler_DragEnter;
            DragDrop += Compiler_DragDrop;

            richTextBox.AllowDrop = true;
            richTextBox.DragEnter += Compiler_DragEnter;
            richTextBox.DragDrop += Compiler_DragDrop;

            Condition_Text("app_open", null);
        }


        private void Create()
        {
            (string filename, string name) = tool_functions.New_ToolStrip(menuStrip3);
            tool_functions.Create_ToolStrip(menuStrip3, filename, name);

            string prev_tool = null;
            if (files.Count() > 0)
            {
                prev_tool = files.ElementAt(file_idx).Item1[0];
                files.ElementAt(file_idx).Item1[2] = richTextBox.Text;
            }            

            string path = file_functions.Create(filename);

            if (files.Count() > 0) files.ElementAt(file_idx).Item1[2] = richTextBox.Text;

            richTextBox.Text = null;
            string[] file = { filename, path, richTextBox.Text, "" };
            file_idx = ls_functions.Add_List_Files(files, "", file);

            string new_tool = files.ElementAt(file_idx).Item1[0];
            tool_functions.Click_Strip(menuStrip3, prev_tool, new_tool);

            Condition_Text("create", null);
        }

        private void Open(string path)
        {
            string prev_tool = files.ElementAt(file_idx).Item1[0];
            files.ElementAt(file_idx).Item1[2] = richTextBox.Text;

            string filename = "", text = "";
            if (string.IsNullOrEmpty(path))
            {
                (filename, path, text) = file_functions.Open(this, menuStrip3);
            }
            else
            {
                (filename, text) = file_functions.Open_Drop_File(path, menuStrip3);
            }
                
            tool_functions.Click_Strip(menuStrip3, prev_tool, filename);

            string[] file = { filename, path, text, text };
            file_idx = ls_functions.Add_List_Files(files, text, file);

            new_tool = true;
            richTextBox.Text = files.ElementAt(file_idx).Item1[2];
            tool_functions.Click_Strip(menuStrip3, prev_tool, files.ElementAt(file_idx).Item1[0]);

            Condition_Text("open", null);
        }

        private void Save()
        {
            file_functions.Save(files.ElementAt(file_idx).Item1[1], richTextBox.Text);
            files.ElementAt(file_idx).Item1[2] = richTextBox.Text;
            Condition_Text("save", null);
        }

        private void Save_How()
        {
            string prev_tool = files.ElementAt(file_idx).Item1[0];

            (string new_tool, string path) = file_functions.Save_How(this, prev_tool, richTextBox.Text);

            tool_functions.Change_Name_ToolStrip(menuStrip3, prev_tool, new_tool);

            string text = richTextBox.Text;
            string[] file = { new_tool, path, text, text };
            ls_functions.Save_List_Files(file_idx, file, files);

            Condition_Text("save_how", null);
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(richTextBox.Text)) return;

            table_functions.Clear_Table(table);
            table_functions.Set_Path(files.ElementAt(file_idx).Item1[1]);

            int errors_count = table_functions.Fill_Table(table, richTextBox);
            Condition_Text("parser", errors_count.ToString());
        }

        private void Help()
        {
            Process.Start("https://docs.google.com/document/d/1fWNk5rWH6WQS7mHoRATFV-HjUk_kn4-cbsnOeN8V2jE/edit?usp=sharing");
        }


        private void Escape()
        {
            close_all = false;

            if (escape) return;
            escape = true;

            ToolStripFunctions tool_functions = new ToolStripFunctions();
            files.ElementAt(file_idx).Item1[2] = richTextBox.Text;
            if (tool_functions.ToolStrip_For_Close(files.ElementAt(file_idx).Item1[0], files) != -1)
            {
                panel7.Visible = true;
            }
            else
            {
                string prev_tool = files.ElementAt(file_idx).Item1[0];
                file_idx = tool_functions.Delete_ToolStrip(menuStrip3, file_idx, files);
                if (files.Count() != 0)
                {
                    richTextBox.Text = files.ElementAt(file_idx).Item1[2];
                    string new_tool = files.ElementAt(file_idx).Item1[0];
                    tool_functions.Click_Strip(menuStrip3, prev_tool, new_tool);
                }
                else richTextBox.Text = null;
            }

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += (s, ev) =>
            {
                escape = false;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }


        private void table_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = table.Rows[e.RowIndex];

            string line = row.Cells[3].Value.ToString();
            if (line != "")
            {
                int line_number = Convert.ToInt32(line.Split(',')[0].Split(' ')[1]) - 1;
                int idx = Convert.ToInt32(line.Split(',')[1].Split(' ')[2]) - 1;

                int char_idx = richTextBox.GetFirstCharIndexFromLine(line_number);

                if (char_idx >= 0)
                {
                    richTextBox.Select(char_idx + idx, 0);
                    richTextBox.ScrollToCaret();
                    richTextBox.Focus();
                }
            }
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
            height = panel1.ClientSize.Height - 50;

            if (splitContainer != null)
            {
                splitContainer.Size = new Size(width, height);
            }

            richTextBox.Width = panel3.Width - panel13.Width;

            panel4.Height = panel1.ClientSize.Height - height - 15;
            panel13.Width = 65;

            panel7.Width = width / 2 + width / 4;
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
                if (panel10.Height > 20) panel11.Height = panel10.Height + 5;
            }
            else
            {
                panel10.Height = 16;
                panel11.Height = 2;
            }
        }


        private void Compiler_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void Compiler_DragDrop(object sender, DragEventArgs e)
        {
            string[] drag_files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (drag_files.Length > 0)
            {
                foreach (string drag_path in drag_files)
                {
                    string path = files.ElementAt(file_idx).Item1[1];
                    path = drag_path;
                    Open(path);
                }
            }
        }

        private void create1_Click(object sender, EventArgs e) => Create();
        private void create2_Click(object sender, EventArgs e) => Create();
        private void open1_Click(object sender, EventArgs e) => Open(null);
        private void open2_Click(object sender, EventArgs e) => Open(null);
        private void save1_Click(object sender, EventArgs e) => Save();
        private void saveHow1_Click(object sender, EventArgs e) => Save_How();
        private void save2_Click(object sender, EventArgs e) => Save_How();
        private void start1_Click(object sender, EventArgs e) => Start();
        private void help1_Click(object sender, EventArgs e) => Help();
        private void help2_Click(object sender, EventArgs e) => Help();


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

            if (buf != null)
                buffer = buf;
        }

        private void cancel1_Click(object sender, EventArgs e) =>
            his_idx = text_functions.Cancel(richTextBox, files.ElementAt(file_idx).Item2, his_idx);
        private void left2_Click(object sender, EventArgs e) =>
            his_idx = text_functions.Cancel(richTextBox, files.ElementAt(file_idx).Item2, his_idx);
        private void return1_Click(object sender, EventArgs e) =>
            his_idx = text_functions.Repeat(richTextBox, files.ElementAt(file_idx).Item2, his_idx);
        private void rigth2_Click(object sender, EventArgs e) =>
            his_idx = text_functions.Repeat(richTextBox, files.ElementAt(file_idx).Item2, his_idx);
        private void copy1_Click(object sender, EventArgs e) => Change_Buffer(1);
        private void copy2_Click(object sender, EventArgs e) => Change_Buffer(1);
        private void cut1_Click(object sender, EventArgs e) => Change_Buffer(2);
        private void cut2_Click(object sender, EventArgs e) => Change_Buffer(2);
        private void enter1_Click(object sender, EventArgs e) => text_functions.Paste(richTextBox, buffer);
        private void enter2_Click(object sender, EventArgs e) => text_functions.Paste(richTextBox, buffer);
        private void delete1_Click(object sender, EventArgs e) => text_functions.Delete(richTextBox);
        private void delete2_Click(object sender, EventArgs e) => text_functions.Delete(richTextBox);
        private void select1_Click(object sender, EventArgs e) => text_functions.Select_All(richTextBox);
        private void select2_Click(object sender, EventArgs e) => text_functions.Select_All(richTextBox);


        private void settingTask1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1W_xG-8e9pEodegER7xnym3bayDsgoeizu2-OA2WSRPo/edit?tab=t.ybu2qljgfd3f");
        }

        private void grammar1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1W_xG-8e9pEodegER7xnym3bayDsgoeizu2-OA2WSRPo/edit?tab=t.0");
        }

        private void grammerClassification1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1W_xG-8e9pEodegER7xnym3bayDsgoeizu2-OA2WSRPo/edit?tab=t.cr5bz3jigrvd");
        }

        private void methodAnalyze1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1W_xG-8e9pEodegER7xnym3bayDsgoeizu2-OA2WSRPo/edit?tab=t.h5awbonb1i6m");
        }

        private void example1_Click(object sender, EventArgs e)
        {
            Open("txt/tests/test-1.txt");
            Open("txt/tests/test-2.txt");
            Open("txt/tests/test-3.txt");
            Open("txt/tests/test-4.txt");
        }

        private void literature1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1W_xG-8e9pEodegER7xnym3bayDsgoeizu2-OA2WSRPo/edit?tab=t.9daqaw1qsobw");
        }

        private void code1_Click(object sender, EventArgs e)
        {

        }


        private void quit1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            closing = false;
            panel7.Visible = false;
        }

        private void no_Click(object sender, EventArgs e)
        {
            if (close_all) Close();

            string prev_tool = files.ElementAt(file_idx).Item1[0];
            file_idx = tool_functions.Delete_ToolStrip(menuStrip3, file_idx, files);
            panel7.Visible = false;
            if (files.Count() == 0 || file_idx < 0)
            {
                richTextBox.Text = "Closing...";
                richTextBox.Text = null;
            }
            else
            {
                richTextBox.Text = files.ElementAt(file_idx).Item1[2];
                tool_functions.Click_Strip(menuStrip3, prev_tool, files.ElementAt(file_idx).Item1[0]);
            }
        }

        private void yes_Click(object sender, EventArgs e)
        {
            if (close_all)
            {
                tool_functions.Close_All_ToolStrip(menuStrip3, this, richTextBox, condition, files);
                Close();
            }
            else
            {
                string prev_tool = files.ElementAt(file_idx).Item1[0];
                file_idx = tool_functions.Close_ToolStrip(file_idx, menuStrip3, this, richTextBox, condition, files);
                panel7.Visible = false;
                if (files.Count() == 0 || file_idx < 0)
                {
                    richTextBox.Text = "Closing...";
                    richTextBox.Text = null;
                }
                else
                {
                    richTextBox.Text = files.ElementAt(file_idx).Item1[2];
                    tool_functions.Click_Strip(menuStrip3, prev_tool, files.ElementAt(file_idx).Item1[0]);
                }
            }
        }


        private void menuStrip3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripMenuItem clicked_item)
            {
                new_tool = true;

                files.ElementAt(file_idx).Item1[2] = richTextBox.Text;

                string prev_tool = files.ElementAt(file_idx).Item1[0];
                string tool_name = clicked_item.Text;
                tool_functions.Click_Strip(menuStrip3, prev_tool, tool_name);

                file_idx = ls_functions.Find_File(files, tool_name);
                richTextBox.Text = files.ElementAt(file_idx).Item1[2];
                if (file_idx == -1) MessageBox.Show("This file is not exists");

                richTextBox.Select(richTextBox.Text.Length, 0);
                syntax_highlighting.Syntax_Highlighting(richTextBox, new_tool);
            }
        }

        private void richTextBox_VScroll(object sender, EventArgs e) => 
            numbering.Scroll(richTextBox, numberBox);

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            if (menuStrip3.Items.Count == 0)
            {
                Create();
            }

            numbering.Numbering_Lines(richTextBox, numberBox);
            his_idx = text_functions.Add_History(files.ElementAt(file_idx).Item2, richTextBox.Text, his_idx);
            
            syntax_highlighting.Syntax_Highlighting(richTextBox, new_tool);
            new_tool = false;
        }


        private void Compiler_KeyDown(object sender, KeyEventArgs e)
        {
            if (files.Count() == 0)
            {
                Create();
            }

            KeyboardShortcuts keyboard = new KeyboardShortcuts();
            (buffer, his_idx) = keyboard.Keyboard_Shortcusts(e, richTextBox, files.ElementAt(file_idx).Item2, his_idx, buffer);
            if (e.KeyCode == Keys.Escape) Escape();
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

                panel7.Visible = false;
                closing = true;

                files.ElementAt(file_idx).Item1[2] = richTextBox.Text;
                foreach (ToolStripMenuItem item in menuStrip3.Items)
                {
                    int index = tool_functions.ToolStrip_For_Close(item.Text, files);
                    if (index != -1)
                    {
                        panel7.Visible = true;
                        break;
                    }
                }

                close_all = true;
                panel7.VisibleChanged += Panel7_VisibleChanged;
            }
        }


        private void FontSizes1_SelectedIndexChanged(object sender, EventArgs e)
        {
            condition.Text = font_functions.Selected_Item_FontSizes(fontSizes1, richTextBox, numberBox);
            string update = richTextBox.Text;
            richTextBox.Text = null;
            richTextBox.Text = update;
        }

        private void rusLan1_Click(object sender, EventArgs e) => Change_Language(1);
        private void enLan1_Click(object sender, EventArgs e) => Change_Language(2);
        private void kazLan1_Click(object sender, EventArgs e) => Change_Language(3);

        private void Change_Language(int choice)
        {
            bool[] visible = new bool[3] { true, true, true };

            language = choice;
            table_functions.Set_Language(language);

            switch (language)
            {
                case 1:
                    visible[0] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines(path_language + "ru.txt"));
                    break;
                case 2:
                    visible[1] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines(path_language + "en.txt"));
                    break;
                case 3:
                    visible[2] = false;
                    Language_Visible(visible);
                    Translate(File.ReadAllLines(path_language + "kaz.txt"));
                    break;
            }

            Condition_Text("language", null);
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
            settings1.Text = language[9];
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
            language1.Text = language[25];
            font1.Text = language[26];
            rusLan1.Text = language[27];
            enLan1.Text = language[28];
            kazLan1.Text = language[29];
            confExit.Text = language[30];
            confirmation.Text = language[31];
            yes.Text = language[32];
            no.Text = language[33];
        }

        private void Condition_Text(string type, string param)
        {
            switch (language)
            {
                case 1:
                    Errors_RU(type, param);
                    break;

                case 2:
                    Errors_EN(type, param);
                    break;

                case 3:
                    Errors_KAZ(type, param);
                    break;
            }
        }

        private void Errors_RU(string type, string param)
        {
            switch (type)
            {
                case "app_open":
                    condition.Text = "Успешное открытие приложения!";
                    break;

                case "create":
                    condition.Text = "Успешное создание файла!";
                    break;

                case "open":
                    condition.Text = "Успешное открытие файла!";
                    break;

                case "save":
                    condition.Text = "Успешное сохранение файла!";
                    break;

                case "save_how":
                    condition.Text = "Успешно выполнена функция \"Сохранить как\"!";
                    break;

                case "language":
                    condition.Text = "Успешная смена языка!";
                    break;

                case "parser":
                    condition.Text = $"Общее количество ошибок: {param}";
                    break;
            }
        }

        private void Errors_EN(string type, string param)
        {
            switch (type)
            {
                case "app_open":
                    condition.Text = "Successful application opening!";
                    break;

                case "create":
                    condition.Text = "Successful file creation!";
                    break;

                case "open":
                    condition.Text = "Successful file opening!";
                    break;

                case "save":
                    condition.Text = "Successful file saving!";
                    break;

                case "save_how":
                    condition.Text = "Successful file saving how!";
                    break;

                case "language":
                    condition.Text = "Successful change language!";
                    break;

                case "parser":
                    condition.Text = $"Total number of errors: {param}";
                    break;
            }
        }

        private void Errors_KAZ(string type, string param)
        {
            switch (type)
            {
                case "app_open":
                    condition.Text = "Қолданбаны сәтті ашу!";
                    break;

                case "create":
                    condition.Text = "Файлды сәтті құру!";
                    break;

                case "open":
                    condition.Text = "Файлды сәтті ашу!";
                    break;

                case "save":
                    condition.Text = "Файлды сәтті сақтау!";
                    break;

                case "save_how":
                    condition.Text = "\"Басқаша сақтау\" функциясы сәтті орындалды!";
                    break;

                case "language":
                    condition.Text = "Тілді сәтті өзгерту!";
                    break;

                case "parser":
                    condition.Text = $"Қателердің жалпы саны: {param}";
                    break;
            }
        }
    }
}
