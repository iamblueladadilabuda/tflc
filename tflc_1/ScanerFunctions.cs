using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tflc_1
{
    internal class ScanerFunctions
    {
        protected Dictionary<int, string> tokens = new Dictionary<int, string>()
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

        protected (int[], string[], int[]) Scaner(string text)
        {
            List<int> token_numbers = new List<int>();
            List<string> token_all = new List<string>();
            List<int> token_idx = new List<int>();

            for (int i = 0; i < text.Length; i++)
            {
                char token = text[i];

                switch (token)
                {
                    case '#':
                        token_numbers.Add(1);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
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
                            token_numbers.Add(2);
                            token_all.Add(letter);
                            token_idx.Add(i - letter.Length + 2);
                        }
                        else
                        {
                            token_numbers.Add(3);
                            token_all.Add(letter);
                            token_idx.Add(i - letter.Length + 2);
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
                                    token_numbers.Add(-1);
                                    token_all.Add(digit);
                                    token_idx.Add(i - digit.Length + 2);
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

                                        token_numbers.Add(-1);
                                        token_all.Add(digit);
                                        token_idx.Add(i - digit.Length + 2);
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
                                token_numbers.Add(19);
                                token_all.Add(digit);
                                token_idx.Add(i - digit.Length + 2);
                            }
                            else
                            {
                                token_numbers.Add(17);
                                token_all.Add(digit);
                                token_idx.Add(i - digit.Length + 2);
                            }
                        }


                        break;

                    case char _ when char.IsWhiteSpace(token):
                        token_numbers.Add(18);
                        token_all.Add("space");
                        token_idx.Add(i + 1);
                        break;

                    case '(':
                        token_numbers.Add(4);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case ')':
                        token_numbers.Add(5);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '{':
                        token_numbers.Add(6);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '}':
                        token_numbers.Add(7);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '\\':
                        token_numbers.Add(8);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case ';':
                        token_numbers.Add(9);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '=':
                        token_numbers.Add(10);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '+':
                        token_numbers.Add(11);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '-':
                        token_numbers.Add(12);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '*':
                        token_numbers.Add(14);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '/':
                        token_numbers.Add(15);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '"':
                        token_numbers.Add(13);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case ',':
                        token_numbers.Add(16);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    default:
                        token_numbers.Add(-1);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;
                }
            }

            return (token_numbers.ToArray(), token_all.ToArray(), token_idx.ToArray());
        }
    }
}
