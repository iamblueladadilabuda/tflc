using System;
using System.Collections.Generic;

namespace tflc_1
{
    internal class TetradsFunctions
    {
        private int t_count = 1;
        private List<string[]> tetrads = new List<string[]>();
        private Dictionary<string, string> expressions = new Dictionary<string, string>();

        public List<string[]> Tetrads(string[] tokens)
        {
            tetrads.Clear();

            int pos = 0;
            while (pos < tokens.Length && tokens[pos] != ";")
            {
                t_count = 1;
                expressions.Clear();

                (int new_pos, string _) = Parse_Add_Sub(tokens, pos);
                pos = new_pos + 1;
            }

            return tetrads;
        }

        private (int, string) Parse_Add_Sub(string[] tokens, int pos)
        {
            (int new_pos, string left) = Parse_Mult_Div(tokens, pos);
            pos = new_pos;

            while (pos < tokens.Length && (tokens[pos] == "+" || tokens[pos] == "-"))
            {
                string op = tokens[pos];
                pos++;
                (int new_pos_2, string right) = Parse_Mult_Div(tokens, pos);
                pos = new_pos_2;
                left = Get_Tetrad(op, left, right);
            }

            return (pos, left);
        }

        private (int, string) Parse_Mult_Div(string[] tokens, int pos)
        {
            (int new_pos, string left) = Parse_Operand(tokens, pos);
            pos = new_pos;

            while (pos < tokens.Length && (tokens[pos] == "*" || tokens[pos] == "/" || tokens[pos] == "%"))
            {
                string op = tokens[pos];
                pos++;
                (int new_pos_2, string right) = Parse_Operand(tokens, pos);
                pos = new_pos_2;
                left = Get_Tetrad(op, left, right);
            }

            return (pos, left);
        }

        private (int, string) Parse_Operand(string[] tokens, int pos)
        {
            if (tokens[pos] == "(")
            {
                pos++;
                (int new_pos, string result) = Parse_Add_Sub(tokens, pos);
                pos = new_pos + 1;
                return (pos, result);
            }

            string operand = tokens[pos];
            return (pos + 1, operand);
        }

        private string Get_Tetrad(string op, string left, string right)
        {
            string expr = $"{left}{op}{right}";
            if (expressions.ContainsKey(expr))
            {
                return expressions[expr];
            }

            string t = $"{t_count}t";
            t_count++;

            tetrads.Add(new string[] { op, left, right, t });
            expressions[expr] = t;

            return t;
        }
    }
}