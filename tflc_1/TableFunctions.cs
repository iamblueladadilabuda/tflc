using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions
    {
        private string path;
        private int table_count = 0;

        private Dictionary<int, string> tokens = new Dictionary<int, string>()
        {
            { -1, "ERROR" },
            { 1, "OPERATOR" },
            { 2, "KEYWORD" },
            { 3, "IDENTIFIER" },
            { 4, "SEPARATOR" },
            { 5, "SEPARATOR" },
            { 6, "SEPARATOR" },
            { 7, "SEPARATOR" },
            { 8, "SEPARATOR" },
            { 9, "SEPARATOR" },
            { 10, "OPERATOR" },
            { 11, "OPERATOR" },
            { 12, "OPERATOR" },
            { 13, "QUOTE" },
            { 14, "OPERATOR" },
            { 15, "OPERATOR" },
            { 16, "SEPARATOR" },
            { 17, "INTEGER" },
            { 18, "SEPARATOR" },
            { 19, "DOUBLE" },
        };

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

        public void Scaner(DataGridView table, string text, int line)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char token = text[i];

                switch(token)
                {
                    case '#':
                        Add_Row_Table(table, 1, line, i + 1, "#");
                        break;

                    case char _ when char.IsLetter(token):

                        string letter = token.ToString();

                        if ((i + 1) < text.Length)
                        {
                            while (char.IsLetterOrDigit(text[i + 1]) || text[i + 1] == '_' || text[i + 1] == '-')
                            {
                                letter += text[i + 1].ToString();
                                i++;

                                if ((i + 1) >= text.Length) break;
                            }
                        }

                        if (letter == "define")
                        {
                            Add_Row_Table(table, 2, line, i - letter.Length + 2, "define");
                        }
                        else
                        {
                            Add_Row_Table(table, 3, line, i - letter.Length + 2, letter);
                        }

                        break;

                    case char _ when char.IsDigit(token):

                        bool error = false;
                        string digit = token.ToString();

                        if ((i + 1) < text.Length)
                        {
                            while (char.IsDigit(text[i + 1]))
                            {
                                digit += text[i + 1].ToString();
                                i++;

                                if ((i + 1) >= text.Length) break;
                            }

                            if ((i + 1) < text.Length && text[i + 1] == '.')
                            {
                                digit += text[i + 1].ToString();
                                i++;

                                if ((i + 1) >= text.Length || (!char.IsDigit(text[i + 1])))
                                {
                                    Add_Row_Table(table, -1, line, i - digit.Length + 2, digit);
                                    error = true;
                                    break;
                                }
                                
                                while (!char.IsWhiteSpace(text[i + 1]))
                                {
                                    if (text[i + 1] == ';') break;

                                    if (!char.IsDigit(text[i + 1]))
                                    {
                                        while (!char.IsWhiteSpace(text[i + 1]))
                                        {
                                            if (text[i + 1] == ';') break;

                                            digit += text[i + 1].ToString();
                                            i++;

                                            if ((i + 1) >= text.Length) break;
                                        }

                                        Add_Row_Table(table, -1, line, i - digit.Length + 2, digit);
                                        error = true;

                                        break;
                                    }

                                    digit += text[i + 1].ToString();
                                    i++;

                                    if ((i + 1) >= text.Length) break;
                                }
                            }
                        }

                        if (!error)
                        {
                            if (digit.IndexOf('.') != -1)
                            {
                                Add_Row_Table(table, 19, line, i - digit.Length + 2, digit);
                            }
                            else
                            {
                                Add_Row_Table(table, 17, line, i - digit.Length + 2, digit);
                            }
                        }


                        break;

                    case char _ when char.IsWhiteSpace(token):
                        Add_Row_Table(table, 18, line, i + 1, "space");
                        break;

                    case '(':
                        Add_Row_Table(table, 4, line, i + 1, "(");
                        break;

                    case ')':
                        Add_Row_Table(table, 5, line, i + 1, ")");
                        break;

                    case '{':
                        Add_Row_Table(table, 6, line, i + 1, "{");
                        break;

                    case '}':
                        Add_Row_Table(table, 7, line, i + 1, "}");
                        break;

                    case '\\':
                        Add_Row_Table(table, 8, line, i + 1, "\\");
                        break;

                    case ';':
                        Add_Row_Table(table, 9, line, i + 1, ";");
                        break;

                    case '=':
                        Add_Row_Table(table, 10, line, i + 1, "=");
                        break;

                    case '+':
                        Add_Row_Table(table, 11, line, i + 1, "+");
                        break;

                    case '-':
                        Add_Row_Table(table, 12 ,line, i + 1, "-");
                        break;

                    case '*':
                        Add_Row_Table(table, 14, line, i + 1, "*");
                        break;

                    case '/':
                        Add_Row_Table(table, 15, line, i + 1, "/");
                        break;

                    case '"':
                        Add_Row_Table(table, 13, line, i + 1, "\"");
                        break;

                    case ',':
                        Add_Row_Table(table, 16, line, i + 1, ",");
                        break;

                    default:
                        Add_Row_Table(table, -1, line, i + 1, token.ToString());
                        break;
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
            int end_idx = start_idx + token.Length - 1;
            return $"line {line}, {start_idx}-{end_idx}";
        }
    }
}