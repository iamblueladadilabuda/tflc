using System.Drawing;
using System.Windows.Forms;

namespace tflc_1
{
    internal class TableFunctions
    {
        private string path;
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
                        Add_Row_Table(table, line, "OPERATOR: #");
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
                            Add_Row_Table(table, line, "KEYWORD: define");
                        }
                        else
                        {
                            Add_Row_Table(table, line, "IDENTIFIER: " + letter);
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

                                if ((i + 1) >= text.Length)
                                {
                                    Add_Row_Table(table, line, "ERROR: " + digit);
                                    error = true;
                                    break;
                                }

                                while (!char.IsWhiteSpace(text[i + 1]))
                                {
                                    if (!char.IsDigit(text[i + 1]))
                                    {
                                        while (!char.IsWhiteSpace(text[i + 1]) || text[i + 1] != ';')
                                        {
                                            digit += text[i + 1].ToString();
                                            i++;
                                            if ((i + 1) >= text.Length) break;
                                        }

                                        Add_Row_Table(table, line, "ERROR: " + digit);
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
                                Add_Row_Table(table, line, "DOUBLE: " + digit);
                            }
                            else
                            {
                                Add_Row_Table(table, line, "INTEGER: " + digit);
                            }
                        }


                        break;

                    case char _ when char.IsWhiteSpace(token):
                        Add_Row_Table(table, line, "SEPARATOR: space");
                        break;

                    case '(':
                        Add_Row_Table(table, line, "SEPARATOR: (");
                        break;

                    case ')':
                        Add_Row_Table(table, line, "SEPARATOR: )");
                        break;

                    case '{':
                        Add_Row_Table(table, line, "SEPARATOR: {");
                        break;

                    case '}':
                        Add_Row_Table(table, line, "SEPARATOR: }");
                        break;

                    case '\\':
                        Add_Row_Table(table, line, "SEPARATOR: \\");
                        break;

                    case ';':
                        Add_Row_Table(table, line, "SEPARATOR: ;");
                        break;

                    case '=':
                        Add_Row_Table(table, line, "OPERATOR: =");
                        break;

                    case '+':
                        Add_Row_Table(table, line, "OPERATOR: +");
                        break;

                    case '-':
                        Add_Row_Table(table, line, "OPERATOR: -");
                        break;

                    case '*':
                        Add_Row_Table(table, line, "OPERATOR: *");
                        break;

                    case '/':
                        Add_Row_Table(table, line, "OPERATOR: /");
                        break;

                    case '"':
                        Add_Row_Table(table, line, "QUOTE: \"");
                        break;

                    case ',':
                        Add_Row_Table(table, line, "SEPARATOR: ,");
                        break;

                    default:
                        Add_Row_Table(table, line, "ERROR: " + token);
                        break;
                }
            }
        }

        private void Add_Row_Table(DataGridView table, int line, string message)
        {
            table.Rows.Add(++table_count, path, line, message);

            if (message.StartsWith("ERROR"))
            {
                table.Rows[table_count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                table.Rows[table_count - 1].DefaultCellStyle.ForeColor = Color.DarkRed;
                table.Rows[table_count - 1].DefaultCellStyle.Font = new Font(table.Font, FontStyle.Bold);
            }
        }
    }
}