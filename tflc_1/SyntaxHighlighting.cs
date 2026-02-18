using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.VisualStyles;

namespace tflc_1
{
    internal class SyntaxHighlighting
    {
        private const string path_syntax = "txt/syntax/";
        private string[] blue, violet;
        private int end = 0;

        public void Syntax_Highlighting(RichTextBox richTextBox, bool new_tool)
        {
            if (string.IsNullOrEmpty(richTextBox.Text))
            {
                end = 0;
                return;
            }

            if (new_tool) end = 0;

            int selectionStart = richTextBox.SelectionStart;
            int selectionLength = richTextBox.SelectionLength;

            if (end > richTextBox.Text.Length)
            {
                end = 0;
                richTextBox.SelectAll();
                richTextBox.SelectionColor = Color.Black;
                richTextBox.Select(selectionStart, selectionLength);
            }

            int is_updating = Coloring(richTextBox, Color.Blue, blue);
            is_updating += Coloring(richTextBox, Color.Purple, violet);

            if (is_updating != 0) end = richTextBox.Text.Length;

            richTextBox.Select(selectionStart, selectionLength);
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
                richTextBox.Select(index, word.Length);
                richTextBox.SelectionColor = color;
                is_updating = 1;
                start = index + word.Length;
            }

            return is_updating;
        }
    }
}
