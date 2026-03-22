using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions : ParserFunctions
    {
        private string path;
        private int table_count = 0;

        public DataGridView Initialize_Table()
        {
            DataGridView table = new DataGridView();
            string[] columns_text = { "№", "Path", "Invalid code", "Line", "Description" };
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

        public int Fill_Table(DataGridView table, RichTextBox richTextBox)
        {
            (int[] codes_all, string[] tokens_all, int[] lines_all, int[] positions) = Find_All_Tokens(richTextBox);

            (string[] tokens, int[] codes, int[] lines, int[] pos) = Space_Clean(tokens_all, codes_all, lines_all, positions);
            Dictionary<int, (int, string)> errors = Parser(tokens, codes);

            if (errors.Count == 0)
            {
                table.Rows.Add(++table_count, path, "Успешно!", "", "Ошибок не обнаружено!");
            }

            for (int j = 0; j < errors.Count; j++)
            {
                (int i, string description) = errors[j];
                if (description.StartsWith("Неизвестный токен"))
                {
                    string token = description.Split('"')[1];
                    table.Rows.Add(++table_count, path, token, Get_Line(lines[i], pos[i]), description);
                }
                else
                {
                    table.Rows.Add(++table_count, path, tokens[i], Get_Line(lines[i], pos[i]), description);
                }
            }

            return errors.Count;
        }

        private string Get_Line(int line, int idx)
        {
            return $"строка {line}, позиция {idx}";
        }
    }
}