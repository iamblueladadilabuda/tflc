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
        private int language = 1;
        private string des;

        public DataGridView Initialize_Table(int lang)
        {
            DataGridView table = new DataGridView();

            language = lang;
            string[] columns_text = { "№", "Путь", "Неверный фрагмент", "Позиция", "Описание" };

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
            Column_Language(table);

            (int[] codes_all, string[] tokens_all, int[] lines_all, int[] positions) = Find_All_Tokens(richTextBox);

            (string[] tokens, int[] codes, int[] lines, int[] pos) = Space_Clean(tokens_all, codes_all, lines_all, positions);
            Dictionary<int, (int, string)> errors = Parser(tokens, codes, language);

            if (errors.Count == 0)
            {
                switch (language)
                {
                    case 1:
                        table.Rows.Add(++table_count, path, "Успешно!", "", "Ошибок не обнаружено!");
                        break;

                    case 2:
                        table.Rows.Add(++table_count, path, "Successfully!", "", "No errors were found!");
                        break;

                    case 3:
                        table.Rows.Add(++table_count, path, "Сәтті!", "", "Қателер табылған жоқ!");
                        break;
                }
            }

            string line = "";
            for (int j = 0; j < errors.Count; j++)
            {
                (int i, string description) = errors[j];

                if (j > 0)
                {
                    if (Get_Line(lines[i], pos[i]) == line)
                    {
                        continue;
                    }
                }

                line = Get_Line(lines[i], pos[i]);

                if (description.StartsWith(des))
                {
                    string token = description.Split('"')[1];
                    table.Rows.Add(++table_count, path, token, line, description);
                }
                else
                {
                    
                    table.Rows.Add(++table_count, path, tokens[i], line, description);
                }
            }

            return errors.Count;
        }

        private string Get_Line(int line, int idx)
        {
            switch (language)
            {
                case 1:
                    return $"строка {line}, позиция {idx}";

                case 2:
                    return $"line {line}, position {idx}";

                case 3:
                    return $"жол {line}, позиция {idx}";
            }

            return $"{line}, {idx}";
        }



        public void Set_Language(int lang)
        {
            language = lang;
        }

        private void Column_Language(DataGridView table)
        {
            string[] columns_text = new string[5];

            switch (language)
            {
                case 1:

                    columns_text[0] = "№";
                    columns_text[1] = "Путь";
                    columns_text[2] = "Неверный фрагмент";
                    columns_text[3] = "Позиция";
                    columns_text[4] = "Описание";

                    des = "Неизвестный токен";

                    break;

                case 2:

                    columns_text[0] = "№";
                    columns_text[1] = "Path";
                    columns_text[2] = "Invalid code";
                    columns_text[3] = "Line";
                    columns_text[4] = "Description";

                    des = "Unknown token";

                    break;

                case 3:

                    columns_text[0] = "№";
                    columns_text[1] = "Жол";
                    columns_text[2] = "Жарамсыз код";
                    columns_text[3] = "Сызық";
                    columns_text[4] = "Сипаттамасы";

                    des = "Белгісіз белгі";

                    break;
            }

            table.Columns.Clear();
            foreach (string col_text in columns_text)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderText = col_text;
                table.Columns.Add(column);
            }
        }
    }
}