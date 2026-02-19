using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions
    {
        private int table_count = 0;

        public DataGridView Initialize_Table()
        {
            DataGridView table = new DataGridView();
            string[] columns_text = { "№", "File path", "Line", "Message" };
            foreach (string col_text in columns_text)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderText = col_text;
                table.Columns.Add(column);
            }

            table.Dock = DockStyle.Fill;
            table.AllowUserToAddRows = false;
            table.ReadOnly = true;
            table.RowHeadersVisible = false;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            return table;
        }

        public void Add_Row_Table(RichTextBox richTextBox, DataGridView table, string path, string message)
        {
            table.Rows.Add(++table_count, path, Get_Line(richTextBox), message);
        }

        private int Get_Line(RichTextBox richTextBox)
        {
            // (!!!) Поменять функцию, когда начнёшь работать с ошибками
            return richTextBox.Text.IndexOf("error") + 1;
        }
    }
}