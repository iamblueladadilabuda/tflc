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
        private const string path_syntax = "txt/syntax/";
        private string[] blue, violet;
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
                    richTextBox.SelectionColor = Color.Black;
                    richTextBox.Select(selection_start, selection_length);
                }
            }

            int is_updating = 0;

            is_updating += Coloring(richTextBox, Color.Purple, Scaner_Array(richTextBox.Text, 2));
            is_updating += Coloring(richTextBox, Color.DeepSkyBlue, Scaner_Array(richTextBox.Text, 3));
            is_updating += Coloring(richTextBox, Color.YellowGreen, Scaner_Array(richTextBox.Text, 13));
            is_updating += Coloring(richTextBox, Color.YellowGreen, Scaner_Array(richTextBox.Text, 15));

            int func = Find_Functions(richTextBox, Color.Yellow);
            if (func != -1) is_updating += func;

            is_updating += Coloring(richTextBox, Color.Blue, blue);
            is_updating += Coloring(richTextBox, Color.Purple, violet);

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

        public void Syntax_Color()
        {
            blue = File.ReadAllLines(path_syntax + "blue.txt");
            violet = File.ReadAllLines(path_syntax + "violet.txt");
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

        private int Find_Functions(RichTextBox richTextBox, Color color)
        {
            string rb = richTextBox.Text;
            if (rb.IndexOf('(') == -1 || rb.IndexOf(')') == -1) return -1;
            if (Count_Staples(rb, '(') != Count_Staples(rb, ')')) return -1;

            List<int> idx = new List<int>();
            for (int i = rb.IndexOf('(', 0); rb.IndexOf('(', i) != -1;)
            {
                idx.Add(i);
                i = rb.IndexOf("(", i + 1);
                if (i == -1) break;
            }

            for (int i = idx.Count - 1; i >= 0; i--)
            {
                int idx_func = idx.ElementAt(i);
                if (idx_func - 1 >= 0)
                {
                    idx_func -= 1;
                    idx_func = Idx_Func(idx_func, rb, true);
                    if (idx_func < 0) break;
                    idx_func = Idx_Func(idx_func, rb, false);
                    idx_func += 1;
                }

                richTextBox.Select(idx_func, idx.ElementAt(i) - idx_func);
                richTextBox.SelectionColor = color;
            }

            return 1;
        }

        private int Count_Staples(string rb, char staple)
        {
            int count = -1;
            for (int i = 0; rb.IndexOf(staple, i) != -1;)
            {
                count++;
                i = rb.IndexOf(staple, i + 1);
                if (i == -1) break;
            }
            return count;
        }

        private int Idx_Func(int idx_func, string rb, bool space)
        {
            while (char.IsWhiteSpace(rb[idx_func]) == space)
            {
                if (!space)
                {
                    if (!(char.IsLetterOrDigit(rb[idx_func]) ||
                    rb[idx_func] == '-' ||
                    rb[idx_func] == '_'))
                        break;
                }

                idx_func--;
                if (idx_func < 0) break;
            }

            return idx_func;
        }
    }
}
