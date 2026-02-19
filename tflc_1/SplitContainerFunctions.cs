using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class SplitContainerFunctions
    {
        public SplitContainer Create_SplitContainer(Panel main, Panel top, Panel bottom)
        {
            Point location = main.Location;
            Size size = main.Size;
            Control parent = main.Parent;
            int index = parent.Controls.GetChildIndex(main);

            parent.Controls.Remove(main);

            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Location = location;
            splitContainer.Size = size;
            splitContainer.Dock = DockStyle.Top;

            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterWidth = 10;
            splitContainer.SplitterDistance = size.Width / 3;

            splitContainer.Panel1.Controls.Add(Initializate_Panel(top));
            splitContainer.Panel2.Controls.Add(Initializate_Panel(bottom));

            parent.Controls.Add(splitContainer);
            parent.Controls.SetChildIndex(splitContainer, index);

            return splitContainer;
        }

        private Panel Initializate_Panel(Panel old_panel)
        {
            Panel new_panel = new Panel();
            new_panel.Dock = DockStyle.Fill;

            old_panel.Parent.Controls.Remove(old_panel);
            old_panel.Dock = DockStyle.Fill;
            new_panel.Controls.Add(old_panel);

            return new_panel;
        }
    }
}
