using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class ToolStripFunctions
    {
        public void Create_ToolStrip(MenuStrip menuStrip, string filename, string name)
        {
            ToolStripMenuItem tool_strip = new ToolStripMenuItem();
            tool_strip.Text = filename;
            tool_strip.Name = name;
            menuStrip.Items.Add(tool_strip);
        }

        protected (string, string) New_ToolStrip(MenuStrip menuStrip)
        {
            List<int> numbers = new List<int>();
            int new_number = 1;

            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (!item.Text.StartsWith("Untitled-")) continue;
                numbers.Add(Convert.ToInt32(item.Text.Split('-')[1]));               
            }

            numbers.Sort();
            foreach (int number in numbers)
            {
                if (number != new_number) break;
                new_number++;
            }

            string filename = "Untitled-" + new_number.ToString();
            string name = "toolStrip_" + new_number.ToString();

            return (filename, name);
        }

        protected bool Find_File_In_ToolStrip(MenuStrip menuStrip, string filename)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == filename)
                {
                    return true;
                }
            }
            return false;
        }

        protected void Change_Name_ToolStrip(MenuStrip menuStrip, string old_filename, 
            string new_filename)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text == old_filename)
                {
                    item.Text = new_filename;
                    return;
                }
            }
        }
    }
}
