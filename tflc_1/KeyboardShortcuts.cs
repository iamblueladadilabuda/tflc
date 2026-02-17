using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace tflc_1
{
    internal class KeyboardShortcuts : TextFunctions
    {
        public (string, int) Keyboard_Shortcusts(KeyEventArgs e, RichTextBox richTextBox,
            string[] history, int idx, string buffer)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:    buffer = Copy(richTextBox);               break;
                    case Keys.V:    Paste(richTextBox, buffer);               break;
                    case Keys.X:    buffer = Cut(richTextBox);                break;
                    case Keys.A:    Select_All(richTextBox);                  break;
                    case Keys.Z:    idx = Cancel(richTextBox, history, idx);  break;
                    case Keys.Y:    idx = Repeat(richTextBox, history, idx);  break;
                }
            }
            return (buffer, idx);
        }
    }
}
