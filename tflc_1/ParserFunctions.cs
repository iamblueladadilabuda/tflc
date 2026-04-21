using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace tflc_1
{
    internal class ParserFunctions : ScanerFunctions
    {
        private Dictionary<int, (int, string)> errors = new Dictionary<int, (int, string)>();
        private int count_errors = -1;

        private int id_code = 1;
        private int int_code = 11;
        private int double_code = 12;

        private int language = 1;

        protected Dictionary<int, (int, string)> Parser(string[] tokens, int[] codes, int lang)
        {
            if (tokens == null) return null;

            int i = 0;
            errors.Clear();
            count_errors = -1;
            language = lang;

            Get_Codes_Value();

            while (i < tokens.Length)
            {
                i = Expression(tokens, codes, i);

                int end = i;
                if (end >= tokens.Length) end = tokens.Length - 1;
            }

            return errors;
        }

        protected (string[], int[], int[], int[]) Space_Clean(string[] tokens, int[] codes, int[] lines, int[] pos)
        {
            List<string> new_tokens = new List<string>();
            List<int> new_codes = new List<int>();
            List<int> new_lines = new List<int>();
            List<int> new_pos = new List<int>();

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "space") continue;

                new_tokens.Add(tokens[i]);
                new_codes.Add(codes[i]);
                new_lines.Add(lines[i]);
                new_pos.Add(pos[i]);
            }

            return (new_tokens.ToArray(), new_codes.ToArray(), new_lines.ToArray(), new_pos.ToArray());
        }

        private void Get_Codes_Value()
        {
            for (int i = 1; i < codes_value.Count; i++)
            {
                if (codes_value[i] == "IDENTIFIER") id_code = i;
                if (codes_value[i] == "INTEGER") int_code = i;
                if (codes_value[i] == "DOUBLE") double_code = i;
            }
        }

        private int Expression(string[] tokens, int[] codes, int i)
        {
            i = Error_Token(tokens, codes, i);
            if (i >= tokens.Length)
            {
                Add_Error(tokens[tokens.Length - 1], ";", tokens.Length - 1, "end");
                return i;
            }

            if (tokens[i] == ")")
            {
                Add_Error(tokens[i], ")", i, "bracket");
                i += 1;
            }
            if (i >= tokens.Length)
            {
                Add_Error(tokens[tokens.Length - 1], ";", tokens.Length - 1, "end");
                return i;
            }
            if (tokens[i] == ";") return i + 1;

            i = Operand(tokens, codes, i);
            if (i >= tokens.Length)
            {
                Add_Error(tokens[tokens.Length - 1], ";", tokens.Length - 1, "end");
                return i;
            }
            if (tokens[i] == ";") return i + 1;

            i = Expression_Body(tokens, codes, i, false);

            i = End(tokens, i);
            if (i >= tokens.Length) return i;

            return i;
        }

        private int Expression_Body(string[] tokens, int[] codes, int i, bool bracket)
        {
            if (i >= tokens.Length) return i;
            if (tokens[i] == ";") return i;
            if (bracket && tokens[i] == ")") return i;

            i = Operator(tokens, codes, i, bracket);
            if (i >= tokens.Length) return i;
            if (tokens[i] == ";") return i;
            if (bracket && tokens[i] == ")") return i;

            i = Operand(tokens, codes, i);
            if (i >= tokens.Length) return i;

            return Expression_Body(tokens, codes, i, bracket);
        }

        private int Operator(string[] tokens, int[] codes, int i, bool bracket)
        {
            if (Is_Operator(tokens[i])) i++;
            else i = Skipping_Codes(tokens, codes, i, "operator", bracket);

            if (i >= tokens.Length)
            {
                int end = tokens.Length - 1;
                if (Is_Operator(tokens[end]))
                {
                    Add_Error(tokens[end], tokens[end], end, "operand");
                }
                return i;
            }

            if (tokens[i] == ";" || tokens[i] == ")")
            {
                if (Is_Operator(tokens[i]))
                {
                    Add_Error(tokens[i], tokens[i], i, "operand");
                }

                if (tokens[i] == ")") return i + 1;
                return i;
            }

            return i;
        }

        private int Operand(string[] tokens, int[] codes, int i)
        {
            if (Is_Operand(codes[i])) return i + 1;
            else if (tokens[i] == "(") return Expression_In_Bracket(tokens, codes, i);
            else i = Skipping_Codes(tokens, codes, i, "operand", false);

            if (tokens[i] == "(") return Expression_In_Bracket(tokens, codes, i);

            if (codes[i] == int_code)
            {
                string digit = tokens[i];

                if (digit.Length > 1 && digit.StartsWith("0") && char.IsDigit(digit[1]))
                {
                    Add_Error(null, null, i, "int");
                }
            }

            if (codes[i] == double_code)
            {
                string digit = tokens[i];

                int j = digit.IndexOf('.');
                if ((j + 1) >= digit.Length)
                {
                    Add_Error(null, null, i, "double");
                }

                for (; j < digit.Length; j++)
                {
                    if ((j + 1) >= digit.Length) break;

                    if (!char.IsDigit(digit[j + 1]))
                    {
                        Add_Error(null, null, i, "double");
                        break;
                    }
                }
            }

            return i;
        }

        private int Expression_In_Bracket(string[] tokens, int[] codes, int i)
        {
            i = Operand(tokens, codes, i + 1);
            if (tokens[i] == ";")
            {
                Add_Error(tokens[i], ")", i, "bracket_end");
                return i;
            }
            if (i >= tokens.Length) return i;

            i = Expression_Body(tokens, codes, i, true);

            if (i >= tokens.Length)
            {
                int end = tokens.Length - 1;
                Add_Error(tokens[end], ")", end, "bracket_end");
                return i;
            }

            i = Skipping_Token(tokens, codes, ")", i);

            return i;
        }

        private int End(string[] tokens, int i)
        {
            if (i >= tokens.Length)
            {
                int end = tokens.Length - 1;
                Add_Error(tokens[end], ";", end, "end");
                return i;
            }

            if (tokens[i] == ";") return i + 1;

            while (tokens[i] != ";" && i < tokens.Length) i++;
            if (i < tokens.Length) Add_Error("", ";", i, "end");
            else Add_Error("", ";", tokens.Length - 1, "end");

            return i + 1;
        }


        private int Skipping_Token(string[] tokens, int[] codes, string token_need, int i)
        {
            if (tokens[i] == token_need) return i + 1;

            int prev_i = i;
            string err = tokens[i];

            while (tokens[i] != token_need && i < tokens.Length)
            {
                int i_clone;
                if (i != prev_i) i_clone = i;
                else i_clone = i + 1;

                i = Error_Token(tokens, codes, i);
                if (i >= tokens.Length)
                {
                    for (; i_clone < tokens.Length; i_clone++) err += tokens[i_clone];
                    break;
                }

                if (tokens[i] == ";") break;
                if (tokens[i] == "(") break;
                if (Is_Operand(codes[i])) break;
                if (Is_Operator(tokens[i])) break;

                if (i != prev_i) err += tokens[i];
                i++;
            }

            Add_Error(err, token_need, prev_i, "token");

            if (i < tokens.Length)
            {
                if (tokens[i] == token_need) return i + 1;
            }

            return i;
        }

        private int Skipping_Codes(string[] tokens, int[] codes, int i, string type, bool bracket)
        {
            int prev_i = i;
            string err = tokens[i];

            while (i < tokens.Length)
            {
                int i_clone;
                if (i != prev_i) i_clone = i;
                else i_clone = i + 1;

                i = Error_Token(tokens, codes, i);
                if (i >= tokens.Length)
                {
                    for (; i_clone < tokens.Length; i_clone++) err += tokens[i_clone];
                    break;
                }

                if (tokens[i] == ";") break;
                if (tokens[i] == "(") break;
                if (tokens[i] == ")" && bracket) break;
                if (Is_Operand(codes[i])) break;
                if (Is_Operator(tokens[i])) break;

                if (i != prev_i) err += tokens[i];
                i++;
            }

            Add_Error(err, "", prev_i, type);

            if (i < tokens.Length)
            {
                if (type == "operand" && Is_Operand(codes[i])) return i + 1;
                if (type == "operator" && Is_Operator(tokens[i])) return i + 1;
            }

            return i;
        }

        private int Error_Token(string[] tokens, int[] codes, int i)
        {
            if (codes[i] != -1) return i;

            int prev_i = i;
            string error_token = "";

            while (i < codes.Length)
            {
                if (codes[i] == -1) error_token += tokens[i++];
                else break;
            }

            Add_Error(error_token, null, prev_i, "unknown_token");

            return i;
        }

        private bool Is_Operand(int op)
        {
            return op == id_code || op == int_code || op == double_code;
        }

        private bool Is_Operator(string op)
        {
            return op == "+" || op == "-" || op == "*" || op == "/" || op == "%";
        }

        private void Add_Error(string error, string correct, int i, string type)
        {
            switch (language)
            {
                case 1:
                    Errors_RU(error, correct, i, type);
                    break;

                case 2:
                    Errors_EN(error, correct, i, type);
                    break;

                case 3:
                    Errors_KAZ(error, correct, i, type);
                    break;
            }
        }

        private void Errors_RU(string error, string correct, int i, string type)
        {
            switch (type)
            {
                case "end":
                    errors.Add(++count_errors, (i, "Не хватает \";\""));
                    break;

                case "token":
                    errors.Add(++count_errors, (i, $"Ожидался токен \"{correct}\", но вместо него получен \"{error}\""));
                    break;

                case "code":
                    errors.Add(++count_errors, (i, $"Ожидался {correct}, но вместо него получен {error}"));
                    break;

                case "unknown_token":
                    errors.Add(++count_errors, (i, $"Неизвестный токен: \"{error}\""));
                    break;

                case "int":
                    errors.Add(++count_errors, (i, $"Целое число не может начинаться с 0"));
                    break;

                case "double":
                    errors.Add(++count_errors, (i, $"Ожидалась цифра после десятичной точки"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Лишняя \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Не хватает \")\""));
                    break;

                case "operand":
                    errors.Add(++count_errors, (i, $"Ожидался операнд, но вместо него получен \"{error}\""));
                    break;

                case "operator":
                    errors.Add(++count_errors, (i, $"Ожидался оператор, но вместо него получен \"{error}\""));
                    break;
            }
        }

        private void Errors_EN(string error, string correct, int i, string type)
        {
            switch (type)
            {
                case "end":
                    errors.Add(++count_errors, (i, "Missing \";\""));
                    break;

                case "token":
                    errors.Add(++count_errors, (i, $"Token \"{correct}\" was expected, but \"{error}\" was received instead"));
                    break;

                case "code":
                    errors.Add(++count_errors, (i, $"{correct}, was expected, but \"{error}\" was received instead"));
                    break;

                case "unknown_token":
                    errors.Add(++count_errors, (i, $"Unknown token: \"{error}\""));
                    break;

                case "int":
                    errors.Add(++count_errors, (i, "An integer can`t start with 0"));
                    break;

                case "double":
                    errors.Add(++count_errors, (i, "The number after the decimal point was expected"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Extra \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Missing \")\""));
                    break;

                case "operand":
                    errors.Add(++count_errors, (i, $"The operand was expected, but \"{error}\" was received instead"));
                    break;

                case "operator":
                    errors.Add(++count_errors, (i, $"The operator was expected, but \"{error}\" was received instead"));
                    break;
            }
        }

        private void Errors_KAZ(string error, string correct, int i, string type)
        {
            switch (type)
            {
                case "end":
                    errors.Add(++count_errors, (i, "Жетіспейді \";\""));
                    break;

                case "token":
                    errors.Add(++count_errors, (i, $"\"{correct}\" таңбалауышы күтілді, бірақ оның орнына \"{error}\"алынды"));
                    break;

                case "code":
                    errors.Add(++count_errors, (i, $"{correct} күтілді, бірақ оның орнына {error} алынды"));
                    break;

                case "unknown_token":
                    errors.Add(++count_errors, (i, $"Белгісіз белгі: \"{error}\""));
                    break;

                case "int":
                    errors.Add(++count_errors, (i, $"Бүтін сан 0-ден басталуы мүмкін емес"));
                    break;

                case "double":
                    errors.Add(++count_errors, (i, $"Ондық бөлшектен кейін сан күтілді"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Қосымша \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Жетіспейді \")\""));
                    break;

                case "operand":
                    errors.Add(++count_errors, (i, $"Операнд күтілді, бірақ оның орнына \"{error}\" алынды"));
                    break;

                case "operator":
                    errors.Add(++count_errors, (i, $"Оператор күтілді, бірақ оның орнына \"{error}\" алынды"));
                    break;
            }
        }
    }
}