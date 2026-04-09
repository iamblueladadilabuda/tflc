using Microsoft.SqlServer.Server;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Hosting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;

namespace tflc_1
{
    internal class ParserFunctions : ScanerFunctions
    {
        private readonly List<string> arquments = new List<string>();

        private Dictionary<int, (int, string)> errors = new Dictionary<int, (int, string)>();
        private int count_errors = -1;

        private int id_code = 3;
        private int int_code = 13;
        private int double_code = 15;

        private int language = 1;

        private int balance = 0;
        private bool is_end = false;

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
                balance = 0;
                is_end = false;
                arquments.Clear();

                i = Macros(tokens, codes, i);

                if (Is_End(tokens.Length, i)) return errors;
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

        private int Macros(string[] tokens, int[] codes, int i)
        {
            i = Start(tokens, codes, i);
            if (Is_End(tokens.Length, i) || Is_Semicolon_Point(tokens, i) || is_end)
            {
                return i + 1;
            }

            balance = 1000;

            i = Function_Call(tokens, codes, i);
            if (Is_End(tokens.Length, i) || Is_Semicolon_Point(tokens, i) || is_end)
            {
                return i + 1;
            }

            balance = 0;

            i = Expression(tokens, codes, i);

            if (balance > 0)
            {
                if (i >= tokens.Length) Add_Error(")", ")", tokens.Length - 1, "bracket_end");
                else Add_Error(")", ")", i, "bracket_end");
            }

            if (Is_End(tokens.Length, i))
            {
                Add_Error(";", ";", tokens.Length - 1, "end");
                return i;
            }

            i = Error_Token(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;

            if (Is_Correct_Token(tokens, codes, i, ";", false)) return i + 1;

            while (tokens[i] != ";")
            {
                i += 1;
                if (i >= tokens.Length) return i;
            }

            return i + 1;
        }

        private int Start(string[] tokens, int[] codes, int i)
        {
            string[] ends_1 = { "define" };
            int[] choices_1 = { 1 };
            i = Airon_Method(tokens, codes, i, "#", ends_1, choices_1);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            string[] ends_2 = { id_code.ToString() };
            int[] choices_2 = { 2 };
            i = Airon_Method(tokens, codes, i, "define", ends_2, choices_2);

            return i;
        }

        private int Function_Call(string[] tokens, int[] codes, int i)
        {
            string[] ends_1 = { "(" };
            int[] choices_1 = { 4 };
            i = Airon_Method(tokens, codes, i, id_code.ToString(), ends_1, choices_1);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            i = List_Param(tokens, codes, i);
            if (Dublicate_Arq()) Add_Error(null, null, i - 1, "dublicate_arq");

            return i;
        }

        private int List_Param(string[] tokens, int[] codes, int i)
        {
            string[] ends_1 = { ")", id_code.ToString() };
            int[] choices_1 = { 1, 2 };
            i = Airon_Method(tokens, codes, i, "(", ends_1, choices_1);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;
            if (tokens[i] == ")")
            {
                Balance_Bracket(tokens[i], i);
                return i + 1;
            }

            arquments.Add(tokens[i]);

            string[] ends_2 = { ")", "," };
            int[] choices_2 = { 4, 4 };
            i = Airon_Method(tokens, codes, i, id_code.ToString(), ends_2, choices_2);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;
            if (tokens[i] == ")")
            {
                Balance_Bracket(tokens[i], i);
                return i + 1;
            }

            return Parameters(tokens, codes, i);
        }

        private int Parameters(string[] tokens, int[] codes, int i)
        {
            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;
            if (tokens[i] == ")")
            {
                Balance_Bracket(tokens[i], i);
                return i + 1;
            }

            string[] ends_1 = { id_code.ToString() };
            int[] choices_1 = { 2 };
            i = Airon_Method(tokens, codes, i, ",", ends_1, choices_1);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            arquments.Add(tokens[i]);

            string[] ends_2 = { ")", "," };
            int[] choices_2 = { 4, 4 };
            i = Airon_Method(tokens, codes, i, id_code.ToString(), ends_2, choices_2);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            return Parameters(tokens, codes, i);
        }

        private int Expression(string[] tokens, int[] codes, int i)
        {
            string[] ends_1 = { id_code.ToString(), int_code.ToString(), double_code.ToString(), "(" };
            int[] choices_1 = { 2, 2, 2, 1 };
            i = Airon_Method(tokens, codes, i, "(", ends_1, choices_1);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            i = Error_Token(tokens, codes, i);
            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end)
            {
                Add_Error(tokens[i], "term", i, "token");
                return i;
            }

            i = Term(tokens, codes, i);

            is_end = Is_Semicolon_Point(tokens, i);
            if (Is_End(tokens.Length, i) || is_end) return i;

            i = Expression_Body(tokens, codes, i, true);
            if (Is_End(tokens.Length, i) || is_end) return i;

            string[] ends_2 = { ";" };
            int[] choices_2 = { 1 };
            i = Airon_Method(tokens, codes, i, ")", ends_2, choices_2);

            return i;
        }

        private int Expression_Body(string[] tokens, int[] codes, int i, bool equal)
        {
            if (Is_End(tokens.Length, i)) return i;
            i = Error_Token(tokens, codes, i);
            if (tokens[i] == ")") return i;
            is_end = Is_Semicolon_Point(tokens, i);
            if (is_end) return i;

            i = Operator(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;

            if (tokens[i - 1] == "=")
            {
                if (!equal) Add_Error(null, null, i, "equal");
            }

            i = Term(tokens, codes, i);

            return Expression_Body(tokens, codes, i, false);
        }

        private int Operator(string[] tokens, int[] codes, int i)
        {
            i = Error_Token(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;

            if (codes_value[codes[i]] == "OPERATOR")
            {
                return i + 1;
            }

            Add_Error(codes_value[codes[i]], "OPERATOR", i, "code");

            int prev_i = i, prev_balance = balance;
            for (; i < tokens.Length; i++)
            {
                if (Is_End(tokens.Length, i)) return i;
                is_end = Is_Semicolon_Point(tokens, i);
                if (is_end)
                {
                    Balance_Bracket(tokens[i], i);
                    return i;
                }

                if (codes_value[codes[i]] == "OPERATOR")
                {
                    Balance_Bracket(tokens[i], i);
                    return i + 1;
                }

                if (codes[i] == id_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return i;
                }

                if (codes[i] == int_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return i;
                }

                if (codes[i] == double_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return i;
                }

                if (tokens[i] == "(")
                {
                    return i;
                }
            }

            balance = prev_balance;
            Balance_Bracket(tokens[prev_i], i);

            return prev_i + 1;
        }

        private int Term(string[] tokens, int[] codes, int i)
        {
            i = Error_Token(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;

            if (tokens[i] == "(")
            {
                return Expression(tokens, codes, i);
            }

            if (codes[i] == id_code)
            {
                if (tokens[i + 1] == "(")
                {
                    return List_Param(tokens, codes, ++i);
                }

                foreach (string arq in arquments)
                {
                    if (tokens[i] == arq)
                    {
                        return i + 1;
                    }
                }

                Add_Error(tokens[i], null, i, "unknown_arq");
                return i + 1;
            }

            if (codes[i] == int_code)
            {
                string digit = tokens[i];

                if (digit.Length > 1 && digit.StartsWith("0") && char.IsDigit(digit[1]))
                {
                    Add_Error(null, null, i, "int");
                }

                return i + 1;
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

                return i + 1;
            }

            Add_Error(tokens[i], "term", i, "token");

            int prev_i = i, prev_balance = balance;
            for (; i < tokens.Length; i++)
            {
                if (tokens[i] == ")") balance--;

                if (Is_End(tokens.Length, i)) break;
                is_end = Is_Semicolon_Point(tokens, i);
                if (is_end)
                {
                    Balance_Bracket(tokens[i], i); 
                    return i;
                }

                if (codes_value[codes[i]] == "OPERATOR")
                {
                    Balance_Bracket(tokens[i], i);
                    return i;
                }

                if (codes[i] == id_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return Term(tokens, codes, i);
                }

                if (codes[i] == int_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return Term(tokens, codes, i);
                }

                if (codes[i] == double_code)
                {
                    Balance_Bracket(tokens[i], i);
                    return Term(tokens, codes, i);
                }

                if (tokens[i] == "(")
                {
                    return Term(tokens, codes, i);
                }
            }

            balance = prev_balance;

            return prev_i + 1;
        }



        // 1) token -> token
        // 2) token -> code
        // 3) code -> code
        // 4) code -> token
        private int Airon_Method(string[] tokens, int[] codes, int i, string token_need, string[] token_end, int[] choice)
        {
            bool is_code = false;
            for (int j = 0; j < choice.Length; j++)
            {
                if (choice[j] == 3 || choice[j] == 4)
                {
                    is_code = true;
                    break;
                }
            }

            i = Error_Token(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;

            if (Is_Correct_Token(tokens, codes, i, token_need, is_code))
            {
                if (token_need == "(" || token_need == ")")
                {
                    Balance_Bracket(token_need, i);
                }

                return i + 1;
            }

            List<int> res_i = new List<int>();
            int prev_i = i;
            for (int j = 0; j < token_end.Length; j++, i = prev_i)
            {
                switch (choice[j])
                {
                    case 1:

                        if (Is_Semicolon_Point(tokens, i))
                        {
                            res_i.Add(i);
                            break;
                        }

                        if (tokens[i] == token_end[j])
                        {
                            res_i.Add(i);
                            break;
                        }

                        while (tokens[i] != token_end[j])
                        {
                            i = Skipping_Tokens(tokens, i);

                            if (is_end)
                            {
                                is_end = false;
                                res_i.Add(i);
                                break;
                            }

                            if (tokens[i] == token_need)
                            {
                                res_i.Add(i);
                                break;
                            }
                        }

                        if (tokens[i] == token_end[j])
                        {
                            res_i.Add(i);
                        }

                        break;

                    case 2:

                        if (Is_Semicolon_Point(tokens, i))
                        {
                            res_i.Add(i);
                            break;
                        }

                        if (codes[i].ToString() == token_end[j])
                        {
                            res_i.Add(i);
                            break;
                        }

                        while (codes[i].ToString() != token_end[j])
                        {
                            i = Skipping_Tokens(tokens, i);

                            if (is_end)
                            {
                                is_end = false;
                                res_i.Add(i);
                                break;
                            }

                            if (tokens[i] == token_need)
                            {
                                res_i.Add(i);
                                break;
                            }
                        }

                        if (codes[i].ToString() == token_end[j])
                        {
                            res_i.Add(i);
                        }

                        break;

                    case 3:

                        if (Is_Semicolon_Point(tokens, i))
                        {
                            res_i.Add(i);
                            break;
                        }

                        if (codes[i].ToString() == token_end[j])
                        {
                            res_i.Add(i);
                            break;
                        }

                        while (codes[i].ToString() != token_end[j])
                        {
                            i = Skipping_Tokens(tokens, i);

                            if (is_end)
                            {
                                is_end = false;
                                res_i.Add(i);
                                break;
                            }

                            if (codes[i].ToString() == token_need)
                            {
                                res_i.Add(i);
                                break;
                            }
                        }

                        if (codes[i].ToString() == token_end[j])
                        {
                            res_i.Add(i);
                        }

                        break;

                    case 4:

                        if (Is_Semicolon_Point(tokens, i))
                        {
                            res_i.Add(i);
                            break;
                        }

                        if (tokens[i] == token_end[j])
                        {
                            res_i.Add(i);
                            break;
                        }

                        while (tokens[i] != token_end[j])
                        {
                            i = Skipping_Tokens(tokens, i);

                            if (is_end)
                            {
                                is_end = false;
                                res_i.Add(i);
                                break;
                            }

                            if (codes[i].ToString() == token_need)
                            {
                                res_i.Add(i);
                                break;
                            }
                        }

                        if (tokens[i] == token_end[j])
                        {
                            res_i.Add(i);
                        }

                        break;
                }
            }

            if (res_i != null)
            {
                res_i.Sort();

                foreach (int res in res_i)
                {
                    if (res < prev_i)
                    {
                        continue;
                    }
                    else
                    {
                        for (; prev_i < res; prev_i++)
                        {
                            if (tokens[prev_i] == "(" || tokens[prev_i] == ")")
                            {
                                Balance_Bracket(tokens[prev_i], i);
                            }
                        }
                        return res;
                    }
                }
            }

            return prev_i + 1;
        }

        private bool Is_Correct_Token(string[] tokens, int[] codes, int i, string token, bool is_code)
        {
            is_end = Is_Semicolon_Point(tokens, i);

            if (token != ";" && is_end)
            {
                if (!is_code)
                {
                    Add_Error(tokens[i], token, i, "token");
                    return false;
                }
                else
                {
                    int code = Convert.ToInt32(token);
                    Add_Error(codes_value[codes[i]], codes_value[code], i, "code");
                    return false;
                }
            }

            if (token == ";" && is_end) return true;

            if (!is_code)
            {
                if (tokens[i] != token)
                {
                    Add_Error(tokens[i], token, i, "token");
                    return false;
                }

                return true;
            }
            else
            {
                int code = Convert.ToInt32(token);

                if (codes[i] != code)
                {
                    Add_Error(codes_value[codes[i]], codes_value[code], i, "code");
                    return false;
                }

                return true;
            }
        }

        private void Balance_Bracket(string bracket, int i)
        {
            if (bracket == "(") balance++;
            if (bracket == ")") balance--;

            if (balance < 0)
            {
                Add_Error(")", ")", i, "bracket");
            }
        }

        private int Skipping_Tokens(string[] tokens, int i)
        {
            i += 1;

            if (Is_End(tokens.Length, i)) return i;
            is_end = Is_Semicolon_Point(tokens, i);
            if (is_end) return i;

            return i;
        }

        private bool Is_End(int length, int i)
        {
            if (i >= length)
            {
                is_end = true;
                return true;
            }
            return false;
        }

        private bool Is_Semicolon_Point(string[] tokens, int i)
        {
            if (tokens[i] == ";") return true;
            else return false;
        }

        private int Error_Token(string[] tokens, int[] codes, int i)
        {
            if (codes[i] != -1) return i;

            int pos = i;
            string error_token = "";

            while (i < codes.Length)
            {
                if (codes[i] == -1) error_token += tokens[i++];
                else break;
            }

            Add_Error(error_token, null, pos, "unknown_token");

            return i;
        }

        private bool Dublicate_Arq()
        {
            if (arquments.Count == 0) return false;

            foreach (string arq in arquments)
            {
                if (arquments.Count(x => x == arq) > 1) return true;
            }

            return false;
        }



        private void Error_End(int length, int i)
        {
            if (i + 1 >= length)
            {
                i = length;
                Add_Error(null, null, i - 1, "end");
            }
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

                case "dublicate_arq":
                    errors.Add(++count_errors, (i, $"Одинаковые параметры в агрументах"));
                    break;

                case "unknown_arq":
                    errors.Add(++count_errors, (i, $"Неизвестный аргумент: {error}"));
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

                case "equal":
                    errors.Add(++count_errors, (i, $"Токен \"=\" может находиться только в левой части выражения"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Лишняя \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Не хватает \")\""));
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

                case "dublicate_arq":
                    errors.Add(++count_errors, (i, "The same parameters in the arquments"));
                    break;

                case "unknown_arq":
                    errors.Add(++count_errors, (i, $"Unknown argument: {error}"));
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

                case "equal":
                    errors.Add(++count_errors, (i, "The \"=\" token can only be found on the left side of the expression"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Extra \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Missing \")\""));
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

                case "dublicate_arq":
                    errors.Add(++count_errors, (i, $"Агрументтердегі бірдей параметрлер"));
                    break;

                case "unknown_arq":
                    errors.Add(++count_errors, (i, $"Белгісіз дәлел: {error}"));
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

                case "equal":
                    errors.Add(++count_errors, (i, $"\"=\" таңбалауышы өрнектің сол жағында ғана болуы мүмкін"));
                    break;

                case "bracket":
                    errors.Add(++count_errors, (i, $"Қосымша \")\""));
                    break;

                case "bracket_end":
                    errors.Add(++count_errors, (i, $"Жетіспейді \")\""));
                    break;
            }
        }
    }
}