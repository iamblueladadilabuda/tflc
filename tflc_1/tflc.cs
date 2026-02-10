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

        public Compiler()
        {
            InitializeComponent();
            Update_Panels_Sizes();
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
            
        }

        private void Open()
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
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

        private void Save()
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(openFileDialog.FileName, richTextBox.Text);
            }
        }

        private void open1_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void open2_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void saveHow1_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void save2_Click(object sender, EventArgs e)
        {
            Save();
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
            Save();
            Close();
        }

        private void help1_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1fWNk5rWH6WQS7mHoRATFV-HjUk_kn4-cbsnOeN8V2jE/edit?usp=sharing");
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

        private void quit1_Click(object sender, EventArgs e)
        {
            panel7.Visible = true;
        }
    }
}
