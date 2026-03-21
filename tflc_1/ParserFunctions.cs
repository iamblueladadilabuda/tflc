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
            arquments.Clear();

            if (tokens == null) return null;

            int i = 0;
            while (i < tokens.Length)
            {
                i = Start(tokens, codes, i);
                if (i >= tokens.Length) return errors;

                i = List_Param(tokens, codes, i);
                if (Dublicate_Arq()) errors.Add(++count_errors, (i - 1, $"Одинаковые параметры в агрументах"));
                if (i >= tokens.Length) return errors;

                i = Macros_Body(tokens, codes, i);
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (i - 1, $"Ожидалась \";\""));
                    return errors;
                }

                if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (i - 1, $"Ожидалась \";\""));
                    return errors;
                }
                if (tokens[i] != ";") errors.Add(++count_errors, (i, $"Ожидалась \";\", но вместо неё получена \"{tokens[i]}\""));
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

        private int Start(string[] tokens, int[] codes, int i)
        {
            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != "#") errors.Add(++count_errors, (i, $"Ожидался \"#\", но вместо него получен \"{tokens[i]}\""));
            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != "define") errors.Add(++count_errors, (i, $"Ожидался \"define\", но вместо него получен \"{tokens[i]}\""));
            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (codes[i] != id_code) errors.Add(++count_errors, (i, $"Ожидался IDENTIFIER, но вместо него получен \"{codes_value[codes[i]]}\""));
            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            return i;
        }

        private int List_Param(string[] tokens, int[] codes, int i)
        {
            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != "(") errors.Add(++count_errors, (i, $"Ожидалась \"(\", но вместо неё получена \"{tokens[i]}\""));

            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (codes[i] != id_code)
            {
                if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                    return i;
                }
                if (tokens[i] != ")") errors.Add(++count_errors, (i, $"Ожидалась \")\", но вместо неё получена \"{tokens[i]}\""));
                return ++i;
            }

            arquments.Add(tokens[i]);
            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (tokens[i] != ",")
            {
                if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                    return i;
                }
                if (tokens[i] != ")") errors.Add(++count_errors, (i, $"Ожидалась \")\", но вместо неё получена \"{tokens[i]}\""));
                return ++i;
            }

            return Parameters(tokens, codes, i);
        }

        private int Parameters(string[] tokens, int[] codes, int i)
        {
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (tokens[i] == ")") return ++i;

            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != ",") errors.Add(++count_errors, (i, $"Ожидалась \",\", но вместо неё получена \"{tokens[i]}\""));
            if (++i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (codes[i] != id_code) errors.Add(++count_errors, (i, $"Ожидался IDENTIFIER, но вместо него получен \"{codes_value[codes[i]]}\""));
            arquments.Add(tokens[i]);

            return Parameters(tokens, codes, ++i);
        }

        private int Macros_Body(string[] tokens, int[] codes, int i)
        {
            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != "(") errors.Add(++count_errors, (i, $"Ожидалась \"(\", но вместо неё получена \"{tokens[i]}\""));

            i = Term(tokens, codes, ++i);
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (tokens[i - 1] == ")" && tokens[i] == ")") return i++;

            i = Expression(tokens, codes, i);
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }

            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (tokens[i] != ")") errors.Add(++count_errors, (i, $"Ожидалась \")\", но вместо неё получена \"{tokens[i]}\""));

            return ++i;
        }

        private int Expression(string[] tokens, int[] codes, int i)
        {
            if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
            if (i >= tokens.Length)
            {
                errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                return i;
            }
            if (codes_value[codes[i]] != "OPERATOR") errors.Add(++count_errors, (i, $"Ожидался OPERATOR, но вместо него получен \"{codes_value[codes[i]]}\""));

            if (++i >= tokens.Length) return i;

            return Term(tokens, codes, i);
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

                if (codes[i] == -1) errors.Add(++count_errors, (i, $"Неизвестный токен: \"{tokens[i++]}\""));
                if (i >= tokens.Length)
                {
                    errors.Add(++count_errors, (i - 1, "Ожидалась строка формата \"#define <name_function>() <function>;\""));
                    return i;
                }
                if (tokens[i] != ")") errors.Add(++count_errors, (i, $"Ожидалась \")\", но вместо неё получена \"{tokens[i]}\""));

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
    }
}
