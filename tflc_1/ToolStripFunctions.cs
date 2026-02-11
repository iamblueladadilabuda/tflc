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
        protected void Create_ToolStrip(MenuStrip menuStrip, string filename, string name)
        {
            ToolStripMenuItem tool_strip = new ToolStripMenuItem();
            tool_strip.Text = filename;
            tool_strip.Name = name;
            menuStrip.Items.Add(tool_strip);
        }

        protected (string, string) New_ToolStrip(MenuStrip menuStrip)
        {
            int new_number = 1;

            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                if (item.Text.IndexOf('-') == -1)
                {
                    new_number++;
                    continue;
                }

                int number = Convert.ToInt32(item.Text.Split('-')[1]);
                if (number != new_number)
                {
                    new_number = number;
                    break;
                }
                else
                {
                    new_number++;
                }
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
