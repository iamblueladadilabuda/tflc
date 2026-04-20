using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace tflc_1
{
    internal class SyntaxHighlighting : ScanerFunctions
    {
        private string text = "";
        private int end = 0;

        public void Syntax_Highlighting(RichTextBox richTextBox, bool new_tool)
        {
            if (string.IsNullOrEmpty(richTextBox.Text))
            {
                end = 0;
                text = "";
                return;
            }

            if (new_tool)
            {
                end = 0;
                text = "";
            }

            int selection_start = richTextBox.SelectionStart;
            int selection_length = richTextBox.SelectionLength;

            if (text != "" || text != richTextBox.Text)
            {
                if (end >= richTextBox.Text.Length || !richTextBox.Text.StartsWith(text))
                {
                    end = 0;
                    richTextBox.SelectAll();
                    if (richTextBox.BackColor == SystemColors.Control)
                    {
                        richTextBox.SelectionColor = Color.Black;
                    }  
                    else
                    {
                        richTextBox.SelectionColor = Color.White;
                    }  
                    richTextBox.Select(selection_start, selection_length);
                }
            }

            int is_updating = 0;

            is_updating += Coloring(richTextBox, Color.DeepSkyBlue, Scaner_Array(richTextBox.Text, 1));
            is_updating += Coloring(richTextBox, Color.YellowGreen, Scaner_Array(richTextBox.Text, 11));
            is_updating += Coloring(richTextBox, Color.YellowGreen, Scaner_Array(richTextBox.Text, 12));

            if (is_updating != 0) end = text.Length;

            int count = 0;
            string[] lines = richTextBox.Text.Split('\n');
            text = "";
            foreach (string line in lines)
            {
                count++;
                if (count == lines.Count()) break;
                text += line + "\n";
            }

            richTextBox.Select(selection_start, selection_length);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }

        private int Coloring(RichTextBox richTextBox, Color color, string[] colors)
        {
            int is_updating = 0;

            foreach (string word in colors)
            {
                is_updating += Highlight_Word(richTextBox, color, word);
            }

            return is_updating;
        }

        private int Highlight_Word(RichTextBox richTextBox, Color color, string word)
        {
            int index, start = end;
            int is_updating = 0;

            while ((index = richTextBox.Text.IndexOf(word, start, StringComparison.Ordinal)) != -1)
            {
                if (word.IndexOf('(') != -1 || word.IndexOf(';') != -1)
                {
                    word = word.Substring(0, word.Length - 1);
                }
                richTextBox.Select(index, word.Length);
                richTextBox.SelectionColor = color;
                is_updating = 1;
                start = index + word.Length;
            }

            return is_updating;
        }

        private string[] Scaner_Array(string rb, int number)
        {
            List<string> array = new List<string>();
            (int[] numbers, string[] token_all, int[] _) = Scaner(rb);

            for (int i = 0; i < token_all.Length; i++)
            {
                if (numbers[i] == number)
                {
                    array.Add(token_all[i]);
                }
            }

            return array.ToArray();
        }
    }
}
