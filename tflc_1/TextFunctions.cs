using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TextFunctions
    {
        private const int HISTORY_SIZE = 10;
        public bool remove = false;

        public int Get_Index(string[] history, int idx)
        {
            int index = 0;
            while (history[index] != null && index < idx && index < history.Length - 1) index++;
            return index;
        }

        public int Add_History(string[] history, string text, int idx)
        {
            if (remove)
            {
                remove = false;
                return idx;
            }

            if (idx > history.Length) idx = HISTORY_SIZE;
            int index = Get_Index(history, idx);

            if (index == idx - 1 && idx == HISTORY_SIZE)
            {
                for (int i = 0; i < idx - 1; i++)
                {
                    history[i] = history[i + 1];
                }
                history[index] = text;
            }
            else
            {
                if (!string.IsNullOrEmpty(text))
                {
                    history[index] = text;
                    for (int i = index + 1; i < HISTORY_SIZE; i++)
                    {
                        if (history[i] == null) break;
                        history[i] = null;
                    }
                }

                if (history[idx] != null && idx < HISTORY_SIZE - 1) idx += 1;
            }

            return idx;
        }

        public int Cancel(RichTextBox richTextBox, string[] history, int idx)
        {
            remove = true;

            int index = Get_Index(history, idx);

            if (index > 1)
            { 
                richTextBox.Text = history[index - 2];
                idx = index - 1;
            }
            else if (index == 1)
            {
                richTextBox.Text = history[index - 1];
                idx = 1;
            } 
            else
            {
                if (!string.IsNullOrEmpty(history[index]))
                {
                    richTextBox.Text = history[index];
                    idx = 1;
                }
            }
            return idx;
        }

        public int Repeat(RichTextBox richTextBox, string[] history, int idx)
        {
            remove = true;
            int index = Get_Index(history, idx);
            if (idx + 1 < history.Length) idx += 1;
            if (history[index] != null) richTextBox.Text = history[index];
            return idx;
        }

        public string Cut(RichTextBox richTextBox)
        {
            if (string.IsNullOrEmpty(richTextBox.Text)) return null;

            string cut = richTextBox.SelectedText;
            richTextBox.SelectedText = "";

            return cut;
        }

        public string Copy(RichTextBox richTextBox)
        {
            return (string.IsNullOrEmpty(richTextBox.Text)) ? null : richTextBox.SelectedText;
        }

        public void Paste(RichTextBox richTextBox, string buffer)
        {
            if (string.IsNullOrEmpty(buffer)) return;

            if (richTextBox.SelectedText != null)
            {
                richTextBox.SelectedText = buffer;
            }
            else
            {
                richTextBox.Text += buffer;
            } 
        }

        public void Delete(RichTextBox richTextBox)
        {
            richTextBox.Text = null;
        }

        public void Select_All(RichTextBox richTextBox)
        {
            richTextBox.SelectAll();
        }
    }
}
