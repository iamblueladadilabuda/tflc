using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace tflc_1
{
    internal class RegexFunctions
    {
        public (string[], int[], int[]) Find_Regex(string pattern, string text, int line)
        {
            List<string> str = new List<string>();
            List<int> lines = new List<int>();
            List<int> index = new List<int>();

            Regex regex = new Regex(@pattern);
            MatchCollection matches = regex.Matches(text);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    str.Add(match.Value);
                    lines.Add(line);
                    index.Add(match.Index + 1);
                }
            }

            return (str.ToArray(), lines.ToArray(), index.ToArray());
        }

        public (string[], int[], int[]) Find_Regex_Automat(string pattern, string text, int line)
        {
            if (pattern.StartsWith("(#"))
            {
                return Automat_1(text, line);
            }
            else
            {
                return Automat_2(text, line);
            }
        }

        public (string[], int[], int[]) Automat_2(string text, int line)
        {
            List<string> str = new List<string>();
            List<int> lines = new List<int>();
            List<int> index = new List<int>();

            for (int i = 0; i < text.Length; i++)
            {
                string s = "";
                int start_idx = i + 1;
                bool correct_str = true;
                int count = 0;

                while (!char.IsWhiteSpace(text[i]))
                {
                    if (i + 1 >= text.Length) break;
                    if (!(Is_HEX(text[i]) && Is_HEX(text[i + 1]))) break;

                    s += text[i].ToString() + text[i + 1].ToString();
                    i += 2;

                    if (i + 2 > text.Length)
                    {
                        if (count + 1 != 6)
                        {
                            correct_str = false;
                        }
                        break;
                    }

                    if (count < 5 && text[i] != ':')
                    {
                        correct_str = false;
                        break;
                    }

                    if (text[i] == ':')
                    {
                        s += ":";
                        i += 1;
                        count += 1;

                        if (i + 1 >= text.Length)
                        {
                            correct_str = false;
                            break;
                        }
                    }
                }

                if (count + 1 == 6 && s.Length == 17 && correct_str)
                {
                    str.Add(s);
                    lines.Add(line);
                    index.Add(start_idx);
                }
            }

            return (str.ToArray(), lines.ToArray(), index.ToArray());
        }

        private bool Is_HEX(char symbol)
        {
            switch (symbol)
            {
                case char _ when char.IsDigit(symbol): return true;
                case 'A': return true;
                case 'B': return true;
                case 'C': return true;
                case 'D': return true;
                case 'E': return true;
                case 'F': return true;
                default: return false;
            }
        }

        public (string[], int[], int[]) Automat_1(string text, int line)
        {
            List<string> str = new List<string>();
            List<int> lines = new List<int>();
            List<int> index = new List<int>();

            string s;
            int start_idx;
            bool correct_str;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '#':

                        start_idx = i + 1;
                        s = "";

                        (i, s) = Get_Word(text, i, s, '\n');

                        str.Add(s);
                        lines.Add(line);
                        index.Add(start_idx);

                        break;

                    case '"':

                        s = "";
                        start_idx = i + 1;

                        (i, s, correct_str) = Quotation(text, i, s, '"');

                        if (correct_str)
                        {
                            str.Add(s);
                            lines.Add(line);
                            index.Add(start_idx);
                        }

                        break;

                    case '\'':

                        s = "";
                        start_idx = i + 1;

                        (i, s, correct_str) = Quotation(text, i, s, '\'');

                        if (correct_str)
                        {
                            str.Add(s);
                            lines.Add(line);
                            index.Add(start_idx);
                        }

                        break;
                }
            }

            return (str.ToArray(), lines.ToArray(), index.ToArray());
        }

        private (int, string, bool) Quotation(string text, int i, string s, char symbol)
        {
            s = "";
            bool correct_str = false;

            if (i + 2 < text.Length)
            {
                if (text[i + 1] == symbol && text[i + 2] == symbol)
                {
                    s += symbol.ToString() + symbol.ToString() + symbol.ToString();
                    i += 3;

                    if (i >= text.Length) return (i, s, correct_str);

                    (i, s) = Get_Word(text, i, s, symbol);

                    if (i + 2 < text.Length)
                    {
                        if (text[i + 1] == symbol && text[i + 2] == symbol)
                        {
                            s += symbol.ToString() + symbol.ToString() + symbol.ToString();
                            i += 3;
                            correct_str = true;
                        }
                    }
                }
            }

            return (i, s, correct_str);
        }

        private (int, string) Get_Word(string text, int i, string s, char end_symbol)
        {
            s += text[i];
            while (text[i] != end_symbol)
            {
                i++;
                if (i >= text.Length) break;
                s += text[i];
            }

            return (i, s);
        }
    }
}
