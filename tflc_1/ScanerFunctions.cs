using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class ScanerFunctions
    {
        protected Dictionary<int, string> codes_value = new Dictionary<int, string>()
        {
            { -1, "ERROR" },
            { 1, "IDENTIFIER" },
            { 2, "SEPARATOR" },
            { 3, "SEPARATOR" },
            { 4, "OPERATOR" },
            { 5, "OPERATOR" },
            { 6, "OPERATOR" },
            { 7, "OPERATOR" },
            { 8, "OPERATOR" },
            { 9, "SEPARATOR" },
            { 10, "SEPARATOR" },
            { 11, "INTEGER" },
            { 12, "DOUBLE" },
        };

        protected (int[], string[], int[], int[]) Find_All_Tokens(RichTextBox richTextBox)
        {
            int line = 0;

            List<int> codes = new List<int>();
            List<string> tokens = new List<string>();
            List<int> lines = new List<int>();
            List<int> positions = new List<int>();

            foreach (string text in richTextBox.Text.Split('\n'))
            {
                line++;
                (int[] numbers, string[] token_all, int[] idx) = Scaner(text);

                for (int i = 0; i < numbers.Length; i++)
                {
                    codes.Add(numbers[i]);
                    tokens.Add(token_all[i]);
                    lines.Add(line);
                    positions.Add(idx[i]);
                }
            }

            return (codes.ToArray(), tokens.ToArray(), lines.ToArray(), positions.ToArray());
        }

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
                    case char _ when char.IsLetter(token):

                        string letter = token.ToString();

                        if ((i + 1) < text.Length)
                        {
                            while (char.IsLetterOrDigit(text[i + 1]) || text[i + 1] == '_')
                            {
                                letter += text[i + 1].ToString();
                                i++;

                                if ((i + 1) >= text.Length) break;
                            }
                        }

                        token_numbers.Add(1);
                        token_all.Add(letter);
                        token_idx.Add(i - letter.Length + 2);

                        break;

                    case char _ when char.IsDigit(token):

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
                                    token_numbers.Add(15);
                                    token_all.Add(digit);
                                    token_idx.Add(i - digit.Length + 2);
                                    break;
                                }

                                while (!char.IsWhiteSpace(text[i + 1]))
                                {
                                    if (text[i + 1] == ';') break;
                                    if (text[i + 1] == ')') break;

                                    digit += text[i + 1].ToString();
                                    i++;

                                    if ((i + 1) >= text.Length) break;
                                }
                            }
                        }

                        if (digit.IndexOf('.') != -1)
                        {
                            token_numbers.Add(12);
                            token_all.Add(digit);
                            token_idx.Add(i - digit.Length + 2);
                        }
                        else
                        {
                            token_numbers.Add(11);
                            token_all.Add(digit);
                            token_idx.Add(i - digit.Length + 2);
                        }

                        break;

                    case char _ when char.IsWhiteSpace(token):
                        token_numbers.Add(2);
                        token_all.Add("space");
                        token_idx.Add(i + 1);
                        break;

                    case ';':
                        token_numbers.Add(3);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '+':
                        token_numbers.Add(4);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '-':
                        token_numbers.Add(5);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '*':
                        token_numbers.Add(6);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '/':
                        token_numbers.Add(7);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '%':
                        token_numbers.Add(8);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case '(':
                        token_numbers.Add(9);
                        token_all.Add(token.ToString());
                        token_idx.Add(i + 1);
                        break;

                    case ')':
                        token_numbers.Add(10);
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
