using System.Drawing;
using System.Windows.Forms;

namespace tflc_1
{
    internal class SplitContainerFunctions
    {
        public SplitContainer Create_SplitContainer(Panel main, Panel top, Panel bottom)
        {
            Point location = main.Location;
            Size size = main.Size;
            DockStyle dockStyle = main.Dock;
            AnchorStyles anchor = main.Anchor;
            Control parent = main.Parent;
            int index = parent.Controls.GetChildIndex(main);

            parent.Controls.Remove(main);

            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Location = location;
            splitContainer.Size = size;

            splitContainer.Dock = DockStyle.Fill;

            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterWidth = 10;

            splitContainer.SplitterDistance = size.Height / 3;

            splitContainer.Panel1MinSize = 50;
            splitContainer.Panel2MinSize = 50;

            splitContainer.Panel1.Controls.Add(Initialize_Panel(top));
            splitContainer.Panel2.Controls.Add(Initialize_Panel(bottom));

            parent.Controls.Add(splitContainer);
            parent.Controls.SetChildIndex(splitContainer, index);

            return splitContainer;
        }

        private Panel Initialize_Panel(Panel old_panel)
        {
            Panel new_panel = new Panel();
            new_panel.Dock = DockStyle.Fill;
            new_panel.BackColor = old_panel.BackColor;
            new_panel.Padding = old_panel.Padding;
            new_panel.Margin = old_panel.Margin;

            if (old_panel.Parent != null)
            {
                old_panel.Parent.Controls.Remove(old_panel);
            }

            old_panel.Dock = DockStyle.Fill;

            new_panel.Controls.Add(old_panel);

            return new_panel;
        }
    }
}