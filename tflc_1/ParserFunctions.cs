using Microsoft.SqlServer.Server;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;

namespace tflc_1
{
    internal class ParserFunctions : ScanerFunctions
    {
        private readonly List<string> arquments = new List<string>();

        private Dictionary<int, (int, string)> errors = new Dictionary<int, (int, string)>();
        private int count_errors = -1;

        private const int id_code = 3;
        private const int int_code = 13;
        private const int double_code = 15;

        protected Dictionary<int, (int, string)> Parser(string[] tokens, int[] codes)
        {
            errors.Clear();
            count_errors = -1;

            if (tokens == null) return null;

            int i = 0;
            while (i < tokens.Length)
            {
                arquments.Clear();

                i = Start(tokens, codes, i);
                if (i >= tokens.Length) return errors;

                i = List_Param(tokens, codes, i);
                if (Dublicate_Arq()) errors.Add(++count_errors, (i - 1, $"Одинаковые параметры в агрументах"));
                if (i >= tokens.Length) return errors;

                i = Macros_Body(tokens, codes, i);
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (tokens.Length - 1, $"Ожидалась \";\" в конце строки"));
                    return errors;
                }

                i = Scaner_Error(tokens, codes, i);
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (tokens.Length - 1, $"Ожидалась \";\" в конце строки"));
                    return errors;
                }
                i = Airons_Tokens(tokens, i, ";");
                i += 1;
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

        private int Start(string[] tokens, int[] codes, int i)
        {
            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Tokens(tokens, i, "#");
            if (Is_End(tokens.Length, ++i)) return i;

            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Tokens(tokens, i, "define");
            if (Is_End(tokens.Length, ++i)) return i;

            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Codes(codes, i, "IDENTIFIER");
            if (Is_End(tokens.Length, ++i)) return i;

            return i;
        }

        private int List_Param(string[] tokens, int[] codes, int i)
        {
            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Tokens(tokens, i, "(");

            if (Is_End(tokens.Length, ++i)) return i;

            if (codes[i] != id_code)
            {
                i = Scaner_Error(tokens, codes, i);
                if (Is_End(tokens.Length, i)) return i;
                i = Airons_Tokens(tokens, i, ")");
                return ++i;
            }

            arquments.Add(tokens[i]);
            if (Is_End(tokens.Length, ++i)) return i;

            if (tokens[i] != ",")
            {
                i = Scaner_Error(tokens, codes, i);
                if (Is_End(tokens.Length, i)) return i;
                i = Airons_Tokens(tokens, i, ")");
                return ++i;
            }

            return Parameters(tokens, codes, i);
        }

        private int Parameters(string[] tokens, int[] codes, int i)
        {
            if (Is_End(tokens.Length, i)) return i;

            if (tokens[i] == ")") return ++i;

            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Tokens(tokens, i, ",");
            if (Is_End(tokens.Length, ++i)) return i;

            i = Scaner_Error(tokens, codes, i);
            if (Is_End(tokens.Length, i)) return i;
            i = Airons_Codes(codes, i, "IDENTIFIER");
            arquments.Add(tokens[i]);

            return Parameters(tokens, codes, ++i);
        }

        private int Macros_Body(string[] tokens, int[] codes, int i)
        {
            i = Scaner_Error(tokens, codes, i);
            if (i >= tokens.Length) return i;
            i = Airons_Tokens(tokens, i, "(");

            i = Expression(tokens, codes, i);
            if (i >= tokens.Length) return i;

            i = Scaner_Error(tokens, codes, i);
            if (i >= tokens.Length) return i;
            i = Airons_Tokens(tokens, i, ")");

            return ++i;
        }

        private int Expression(string[] tokens, int[] codes, int i)
        {
            i = Term(tokens, codes, ++i);
            if (i >= tokens.Length) return i;

            if (tokens[i - 1] == ")" && tokens[i] == ")") return ++i;

            return Expression_Body(tokens, codes, i);
        }

        private int Expression_Body(string[] tokens, int[] codes, int i)
        {
            if (tokens[i] == ")") return i;

            i = Scaner_Error(tokens, codes, i);
            if (i >= tokens.Length) return i;
            i = Airons_Codes(codes, i, "OPERATOR");

            if (++i >= tokens.Length) return i;

            i = Term(tokens, codes, i);

            return Expression_Body(tokens, codes, i);
        }

        private int Term(string[] tokens, int[] codes, int i)
        {
            if (tokens[i] == "(")
            {
                i = Term(tokens, codes, ++i);
                if (i >= tokens.Length) return i;

                if (tokens[i - 1] == ")" && tokens[i] == ")") return ++i;
                if (codes[i - 1] == id_code && tokens[i] == ")") return ++i;

                i = Expression(tokens, codes, i);
                if (i >= tokens.Length) return i;

                i = Scaner_Error(tokens, codes, i);
                if (Is_End(tokens.Length, i)) return i;
                i = Airons_Tokens(tokens, i, ")");

                return ++i;
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
                        return ++i;
                    }
                }

                errors.Add(++count_errors, (i, $"Неизвестный аргумент: {tokens[i]}"));
                i += 1;
            }

            if (codes[i] == int_code)
            {
                string digit = tokens[i];

                if (digit.Length > 1 && digit.StartsWith("0") && char.IsDigit(digit[1]))
                {
                    errors.Add(++count_errors, (i, $"Целое число не может начинаться с 0"));
                }

                return ++i;
            }

            if (codes[i] == double_code)
            {
                string digit = tokens[i];

                int j = digit.IndexOf('.');
                if ((j + 1) >= digit.Length)
                {
                    errors.Add(++count_errors, (i, $"Ожидалась цифра после десятичной точки"));
                }

                for (; j < digit.Length; j++)
                {
                    if ((j + 1) >= digit.Length) break;
                    
                    if (!char.IsDigit(digit[j + 1]))
                    {
                        errors.Add(++count_errors, (i, $"Ожидалась цифра после десятичной точки"));
                        break;
                    }
                }

                return ++i;
            }

            return i;
        }

        private int Airons_Tokens(string[] tokens, int i, string token)
        {
            if (tokens[i] != token) errors.Add(++count_errors, (i, $"Ожидался токен \"{token}\", но вместо него получен \"{tokens[i]}\""));

            while (i < tokens.Length)
            {
                if (tokens[i] == token) break;
                i += 1;
            }

            return i;
        }

        private int Airons_Codes(int[] codes, int i, string token)
        {
            string value = codes_value[codes[i]];

            if (value != token) errors.Add(++count_errors, (i, $"Ожидался {token}, но вместо него получен {value}"));

            while (i < codes.Length)
            {
                value = codes_value[codes[i]];
                if (value == token) break;
                i += 1;
            }

            return i;
        }

        private int Scaner_Error(string[] tokens, int[] codes, int i)
        {
            if (codes[i] != -1) return i;

            int pos = i;
            string error_token = "";

            while (codes[i] == -1)
            {
                error_token += tokens[i++];
            }

            errors.Add(++count_errors, (pos, $"Неизвестный токен: \"{error_token}\""));

            return i;
        }

        private bool Is_End(int length, int i)
        {
            if (i >= length)
            {
                errors.Add(++count_errors, (length - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return true;
            }
            return false;
        }

        private bool Dublicate_Arq()
        {
            int dubl_count = -1;

            foreach (string arq in arquments)
            {
                foreach (string dubl in arquments)
                {
                    if (arq == dubl) dubl_count++;

                    if (dubl_count > 0) return true;
                }
            }

            return false;
        }
    }
}
