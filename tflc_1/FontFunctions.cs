using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class FontFunctions
    {
        public void Initialize_FontSizes(ToolStripComboBox fontSizes) 
        {
            for (int i = 7; i <= 20; i++)
            {
                fontSizes.Items.Add(i.ToString());
            }
        }

        public string Selected_Item_FontSizes(ToolStripComboBox fontSizes, 
            RichTextBox richTextBox, RichTextBox numberBox)
        {
            if (fontSizes.SelectedItem != null)
            {
                int font_size = Convert.ToInt32(fontSizes.SelectedItem.ToString());
                richTextBox.Font = new Font(richTextBox.Font.FontFamily, font_size);
                numberBox.Font = new Font(numberBox.Font.FontFamily, font_size);
                return "Successful change font size!";
            }
            return "Error: font size cant`t be null!";
        }
    }
}
