using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions : ParserFunctions
    {
        private string path;
        private int table_count = 0;
        private int language = 1;
        private string unknown_token;
        private Color theme = Color.FromArgb(32, 32, 32);

        public DataGridView Initialize_Table(DataGridView table, int lang, int choice)
        {
            language = lang;

            switch (choice)
            {
                case 1:
                    Column_Scaner(table);
                    break;

                case 2:
                    Column_Parser(table);
                    break;

                case 3:
                    Column_Tetrads(table);
                    break;

                case 4:
                    Column_Polis(table);
                    break;
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

        public int Fill_Table(int choice, DataGridView table, RichTextBox richTextBox)
        {
            switch (choice)
            {
                case 1:
                    return Fill_Table_Scaner(table, richTextBox);

                case 2:
                    return Fill_Table_Parser(table, richTextBox);

                case 3:
                    return Fill_Table_Tetrad(table, richTextBox);

                case 4:
                    return Fill_Table_POLIS(table, richTextBox);

                default:
                    return 0;
            }
        }

        private int Fill_Table_Scaner(DataGridView table, RichTextBox richTextBox)
        {
            table_count = 0;
            table = Initialize_Table(table, language, 1);

            (int[] codes, string[] tokens, int[] lines, int[] pos) = Find_All_Tokens(richTextBox);
            int error_count = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (codes[i] == -1) error_count++;
                table.Rows.Add(++table_count, codes[i], codes_value[codes[i]], tokens[i], Get_Line(lines[i], pos[i]));
            }

            return error_count;
        }

        private int Fill_Table_Parser(DataGridView table, RichTextBox richTextBox)
        {
            table_count = 0;
            table = Initialize_Table(table, language, 2);

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

                line = Get_Line(lines[i], pos[i]);
                table.Rows.Add(++table_count, path, tokens[i], line, description);
            }

            return errors.Count;
        }

        private int Fill_Table_Tetrad(DataGridView table, RichTextBox richTextBox)
        {
            table_count = 0;
            table = Initialize_Table(table, language, 3);

            (int[] codes_all, string[] tokens_all, int[] lines_all, int[] positions) = Find_All_Tokens(richTextBox);

            (string[] tokens, int[] codes, int[] lines, int[] pos) = Space_Clean(tokens_all, codes_all, lines_all, positions);
            Dictionary<int, (int, string)> errors = Parser(tokens, codes, language);

            if (errors.Count != 0)
            {
                switch (language)
                {
                    case 1:
                        MessageBox.Show("Перед разбором необходимо избавиться от ошибок в строке");
                        return errors.Count;

                    case 2:
                        MessageBox.Show("Before parsing, it is necessary to get rid of errors in the line");
                        return errors.Count;

                    case 3:
                        MessageBox.Show("Талдау алдында жолдағы қателіктерден арылу керек");
                        return errors.Count;
                }
            }

            TetradsFunctions tetrad = new TetradsFunctions();
            List<string[]> tetrads = tetrad.Tetrads(tokens);

            for (int i = 0; i < tetrads.Count; i++)
            {
                for (int j = 0; j < tetrads[i].Length; j += 4)
                {
                    table.Rows.Add(++table_count, tetrads[i][j], tetrads[i][j + 1], tetrads[i][j + 2], tetrads[i][j + 3]);
                }
            }

            return 0;
        }

        private int Fill_Table_POLIS(DataGridView table, RichTextBox richTextBox)
        {
            table_count = 0;
            table = Initialize_Table(table, language, 4);

            (int[] codes_all, string[] tokens_all, int[] lines_all, int[] positions) = Find_All_Tokens(richTextBox);

            foreach(int code in codes_all)
            {
                if (code == 1 || code == 12)
                {
                    switch (language)
                    {
                        case 1:
                            MessageBox.Show("Выражение должно состоять исключительно из целых чисел");
                            return 1;

                        case 2:
                            MessageBox.Show("The expression must consist only of integers");
                            return 1;

                        case 3:
                            MessageBox.Show("Өрнек тек бүтін сандардан тұруы керек");
                            return 1;
                    }
                }
            }

            (string[] tokens, int[] codes, int[] lines, int[] pos) = Space_Clean(tokens_all, codes_all, lines_all, positions);
            Dictionary<int, (int, string)> errors = Parser(tokens, codes, language);

            if (errors.Count != 0)
            {
                switch (language)
                {
                    case 1:
                        MessageBox.Show("Перед разбором необходимо избавиться от ошибок в строке");
                        return errors.Count;

                    case 2:
                        MessageBox.Show("Before parsing, it is necessary to get rid of errors in the line");
                        return errors.Count;

                    case 3:
                        MessageBox.Show("Талдау алдында жолдағы қателіктерден арылу керек");
                        return errors.Count;
                }
            }

            TetradsFunctions tetrad = new TetradsFunctions();
            List<string[]> tetrads = tetrad.Tetrads(tokens);

            PolisFunctions pol = new PolisFunctions();
            string[] polis = pol.Polis(tetrads, language);

            for (int i = 0; i < polis.Length; i += 4)
            {
                table.Rows.Add(++table_count, polis[i], polis[i + 1], polis[i + 2], polis[i + 3]);
            }

            return 0;
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

        public void Set_Theme(Color them)
        {
            theme = them;
        }


        private void Column_Scaner(DataGridView table)
        {
            string[] columns_text = new string[5];

            switch (language)
            {
                case 1:

                    columns_text[0] = "№";
                    columns_text[1] = "Условный код";
                    columns_text[2] = "Тип лексемы";
                    columns_text[3] = "Лексема";
                    columns_text[4] = "Местоположение";

                    break;

                case 2:

                    columns_text[0] = "№";
                    columns_text[1] = "Conditional code";
                    columns_text[2] = "Type of token";
                    columns_text[3] = "Token";
                    columns_text[4] = "Line";

                    break;

                case 3:

                    columns_text[0] = "№";
                    columns_text[1] = "Шартты код";
                    columns_text[2] = "Лексема түрі";
                    columns_text[3] = "Лексема";
                    columns_text[4] = "Сызық";

                    break;
            }

            Create_Column(table, columns_text);
        }

        private void Column_Parser(DataGridView table)
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

                    unknown_token = "Неизвестный токен";

                    break;

                case 2:

                    columns_text[0] = "№";
                    columns_text[1] = "Path";
                    columns_text[2] = "Invalid code";
                    columns_text[3] = "Line";
                    columns_text[4] = "Description";

                    unknown_token = "Unknown token";

                    break;

                case 3:

                    columns_text[0] = "№";
                    columns_text[1] = "Жол";
                    columns_text[2] = "Жарамсыз код";
                    columns_text[3] = "Сызық";
                    columns_text[4] = "Сипаттамасы";

                    unknown_token = "Белгісіз белгі";

                    break;
            }

            Create_Column(table, columns_text);
        }

        private void Column_Tetrads(DataGridView table)
        {
            string[] columns_text = new string[5];

            switch (language)
            {
                case 1:

                    columns_text[0] = "№";
                    columns_text[1] = "Оператор";
                    columns_text[2] = "Операнд 1";
                    columns_text[3] = "Операнд 2";
                    columns_text[4] = "Результат";

                    break;

                case 2:

                    columns_text[0] = "№";
                    columns_text[1] = "Operator";
                    columns_text[2] = "Operand 1";
                    columns_text[3] = "Operand 2";
                    columns_text[4] = "Result";

                    break;

                case 3:

                    columns_text[0] = "№";
                    columns_text[1] = "Оператор";
                    columns_text[2] = "Операнд 1";
                    columns_text[3] = "Операнд 2";
                    columns_text[4] = "Нәтижесі";

                    break;
            }

            Create_Column(table, columns_text);
        }

        private void Column_Polis(DataGridView table)
        {
            string[] columns_text = new string[5];

            switch (language)
            {
                case 1:

                    columns_text[0] = "№";
                    columns_text[1] = "Операнд 1";
                    columns_text[2] = "Операнд 2";
                    columns_text[3] = "Оператор";
                    columns_text[4] = "Результат";

                    break;

                case 2:

                    columns_text[0] = "№";
                    columns_text[1] = "Operand 1"; 
                    columns_text[2] = "Operand 2";
                    columns_text[3] = "Operator";
                    columns_text[4] = "Result";

                    break;

                case 3:

                    columns_text[0] = "№";
                    columns_text[1] = "Операнд 1"; 
                    columns_text[2] = "Операнд 2";
                    columns_text[3] = "Оператор";
                    columns_text[4] = "Нәтижесі";

                    break;
            }

            Create_Column(table, columns_text);
        }

        private void Create_Column(DataGridView table, string[] columns_text)
        {
            table.Columns.Clear();

            foreach (string col_text in columns_text)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.DefaultCellStyle.BackColor = theme;
                table.ColumnHeadersDefaultCellStyle.BackColor = theme;
                if (theme == Color.FromArgb(32, 32, 32))
                {
                    column.DefaultCellStyle.ForeColor = Color.White;
                    table.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                }
                else
                {
                    column.DefaultCellStyle.ForeColor = Color.Black;
                    table.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                }
                column.HeaderText = col_text;
                table.Columns.Add(column);
            }
        }
    }
}