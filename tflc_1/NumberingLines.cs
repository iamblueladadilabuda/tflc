using System.Drawing;
using System.Windows.Forms;

namespace tflc_1
{
    internal class NumberingLines
    {
        private int num_line = 1;

        public void Numbering_Lines(RichTextBox richTextBox, RichTextBox numberBox)
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
                }

                numberBox.Text = null;
                for (int i = 1; i < num_line; i++)
                {
                    numberBox.Text += i.ToString() + "\n";
                }
            }
        }

        public void Scroll(RichTextBox richTextBox, RichTextBox numberBox)
        {
            int i = richTextBox.GetLineFromCharIndex(richTextBox.GetCharIndexFromPosition(new Point(1, 1)));
            numberBox.SelectionStart = numberBox.GetFirstCharIndexFromLine(i);
            numberBox.ScrollToCaret();
        }
    }
}
