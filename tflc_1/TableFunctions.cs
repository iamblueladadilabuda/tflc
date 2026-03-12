using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions : ScanerFunctions
    {
        private string path;
        private int table_count = 0;

        public DataGridView Initialize_Table()
        {
            DataGridView table = new DataGridView();
            string[] columns_text = { "№", "File path", "Code", "Type of token", "Token", "Line" };
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
            table.ScrollBars = ScrollBars.Both;
            table.ReadOnly = true;
            table.RowHeadersVisible = false;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            return table;
        }

        public void Set_Path(string path)
        {
            this.path = path;
        }

        public void Clear_Table(DataGridView table)
        {
            table_count = 0;
            table.Rows.Clear();
        }

        public void Find_All_Tokens(DataGridView table, RichTextBox richTextBox)
        {
            int line = 0;
            foreach (string text in richTextBox.Text.Split('\n'))
            {
                line++;
                (int[] numbers, string[] token_all, int[] idx) = Scaner(text);

                for (int i = 0; i < token_all.Length; i++)
                {
                    Add_Row_Table(table, numbers[i], line, idx[i], token_all[i]);
                }
            }
        }

        private void Add_Row_Table(DataGridView table, int code, int line, int start_idx, string token)
        {
            table.Rows.Add(++table_count, path, code, tokens[code], token, Get_Line(line, start_idx, token));

            if (code == -1)
            {
                table.Rows[table_count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                table.Rows[table_count - 1].DefaultCellStyle.ForeColor = Color.DarkRed;
                table.Rows[table_count - 1].DefaultCellStyle.Font = new Font(table.Font, FontStyle.Bold);
            }
        }

        private string Get_Line(int line, int start_idx, string token)
        {
            if (token == "space") token = " ";
            int end_idx = start_idx + token.Length - 1;
            return $"line {line}, {start_idx}-{end_idx}";
        }
    }
}