using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace tflc_1
{
    public class TACInstruction
    {
        public string Result { get; set; }
        public string Op { get; set; }
        public string Arg_1 { get; set; }
        public string Arg_2 { get; set; }
        public bool Optimized { get; set; }
        public string Original_Text { get; set; }

        public TACInstruction(string result, string op, string arg1, string arg2 = null, string comment = null)
        {
            Result = result;
            Op = op;
            Arg_1 = arg1;
            Arg_2 = arg2;
            Optimized = false;
            Original_Text = ToString();
        }

        public override string ToString()
        {
            if (Op == "=") return $"{Result} = {Arg_1}";
            else if (Op == "param") return $"param {Arg_1}";
            else if (Op == "call") return $"{Result} = call {Arg_1}, {Arg_2}";
            else if (Arg_2 == null) return $"{Result} = {Op} {Arg_1}";
            else return $"{Result} = {Arg_1} {Op} {Arg_2}";
        }
    }

    public class TACFunctions
    {
        private List<TACInstruction> tac_code;
        private int temp_counter;
        private Dictionary<string, string> constant_values;
        private Dictionary<string, bool> used_variables;
        private int fold_count;
        private int dead_code_count;

        public TACFunctions()
        {
            tac_code = new List<TACInstruction>();
            temp_counter = 0;
            constant_values = new Dictionary<string, string>();
            used_variables = new Dictionary<string, bool>();
            fold_count = 0;
            dead_code_count = 0;
        }

        private string New_Temp()
        {
            return $"t{temp_counter++}";
        }

        public void TAC(RichTextBox ast_text, string source_code)
        {
            try
            {
                tac_code.Clear();
                temp_counter = 0;
                constant_values.Clear();
                used_variables.Clear();
                fold_count = 0;
                dead_code_count = 0;

                string[] lines = source_code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                MacroDefinition macro = null;
                string macro_call = null;
                Dictionary<string, string> variables = new Dictionary<string, string>();

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("#define"))
                    {
                        macro = Parse_Macro(line);
                    }
                    else if (macro != null && trimmed.Contains(macro.Name + "("))
                    {
                        macro_call = trimmed;
                    }
                    else if (trimmed.Contains("=") && !trimmed.StartsWith("#"))
                    {
                        Parse_Assignment(trimmed, variables);
                    }
                }

                if (macro == null)
                {
                    ast_text.Text = "Ошибка: не удалось распознать макрос\n";
                    return;
                }

                if (string.IsNullOrEmpty(macro_call))
                {
                    ast_text.Text = "Ошибка: не найден вызов макроса\n";
                    return;
                }

                string args = Extract_Macro_Arguments(macro_call, macro.Name);

                StringBuilder sb = new StringBuilder();

                Generate_TAC_For_Macro_Call(macro, args, variables);

                sb.AppendLine("=== ИСХОДНЫЙ ТРЁХАДРЕСНЫЙ КОД (TAC) ===");
                sb.AppendLine($"Макрос {macro.Name}({string.Join(", ", macro.Parameters)}) с аргументом ({args})");
                foreach (var var in variables)
                {
                    sb.AppendLine($"Переменная: {var.Key} = {var.Value}");
                }
                sb.AppendLine($"Подстановка: {macro.Body} -> с заменой {macro.Parameters[0]} на ({args})\n");

                for (int i = 0; i < tac_code.Count; i++)
                {
                    sb.AppendLine($"{i + 1,2}: {tac_code[i]}");
                }

                Optimize_Constant_Folding();
                sb.AppendLine($"\n=== ОПТИМИЗАЦИЯ 1: СВЁРТКА КОНСТАНТ (Constant Folding) ===");
                sb.AppendLine($"Применено оптимизаций: {fold_count}\n");

                for (int i = 0; i < tac_code.Count; i++)
                {
                    sb.AppendLine($"{i + 1,2}: {tac_code[i]}");
                }

                int before_count = tac_code.Count;
                Optimize_Dead_Code_Elimination();
                int after_count = tac_code.Count;
                dead_code_count = before_count - after_count;

                sb.AppendLine($"\n=== ОПТИМИЗАЦИЯ 2: УДАЛЕНИЕ МЁРТВОГО КОДА (Dead Code Elimination) ===");
                sb.AppendLine($"Удалено инструкций: {dead_code_count}\n");

                for (int i = 0; i < tac_code.Count; i++)
                {
                    sb.AppendLine($"{i + 1,2}: {tac_code[i]}");
                }

                sb.AppendLine($"\n=== ИТОГОВЫЙ РЕЗУЛЬТАТ ===");
                sb.AppendLine($"Всего оптимизаций применено: {fold_count + dead_code_count}");
                sb.AppendLine($"Размер кода: {after_count} инструкций");

                var result_instr = tac_code.LastOrDefault();
                if (result_instr != null && result_instr.Result == "result")
                {
                    sb.AppendLine($"\nРЕЗУЛЬТАТ ВЫЧИСЛЕНИЯ {macro.Name}({args}) = {result_instr.Arg_1}");
                }

                ast_text.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ast_text.Text = $"Ошибка генерации TAC: {ex.Message}\n{ex.StackTrace}";
            }
        }

        private class MacroDefinition
        {
            public string Name { get; set; }
            public List<string> Parameters { get; set; }
            public string Body { get; set; }
        }

        private MacroDefinition Parse_Macro(string source_line)
        {
            if (string.IsNullOrWhiteSpace(source_line)) return null;

            string trimmed = source_line.Trim();
            if (!trimmed.StartsWith("#define")) return null;

            string remaining = trimmed.Substring(7).Trim();

            int paren_index = remaining.IndexOf('(');
            if (paren_index <= 0) return null;

            string name = remaining.Substring(0, paren_index).Trim();
            remaining = remaining.Substring(paren_index);

            int close_paren_index = Find_Matching_Paren(remaining, 0);
            if (close_paren_index <= 0) return null;

            string params_str = remaining.Substring(1, close_paren_index - 1);
            var parameters = params_str.Split(',').Select(p => p.Trim()).ToList();

            string body = remaining.Substring(close_paren_index + 1).Trim();

            if (body.EndsWith(";"))
            {
                body = body.Substring(0, body.Length - 1);
            }

            MacroDefinition macro = new MacroDefinition
            {
                Name = name,
                Parameters = parameters,
                Body = body
            };

            return macro;
        }

        private int Find_Matching_Paren(string str, int start)
        {
            int balance = 0;

            for (int i = start; i < str.Length; i++)
            {
                if (str[i] == '(') balance++;
                if (str[i] == ')') balance--;
                if (balance == 0) return i;
            }

            return -1;
        }

        private void Parse_Assignment(string line, Dictionary<string, string> variables)
        {
            string trimmed = line.Trim();
            if (trimmed.EndsWith(";"))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);

            string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string rest = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : trimmed;

            int equal_index = rest.IndexOf('=');
            if (equal_index > 0)
            {
                string var_name = rest.Substring(0, equal_index).Trim();
                string var_value = rest.Substring(equal_index + 1).Trim();

                if (int.TryParse(var_value, out _))
                {
                    variables[var_name] = var_value;
                }
            }
        }

        private string Extract_Macro_Arguments(string macro_call, string macro_name)
        {
            int start_index = macro_call.IndexOf(macro_name + "(");
            if (start_index == -1) return "";

            start_index = macro_call.IndexOf('(', start_index) + 1;
            int paren_level = 1;
            int end_index = start_index;

            for (int i = start_index; i < macro_call.Length; i++)
            {
                if (macro_call[i] == '(') paren_level++;
                if (macro_call[i] == ')')
                {
                    paren_level--;
                    if (paren_level == 0)
                    {
                        end_index = i;
                        break;
                    }
                }
            }

            string args = macro_call.Substring(start_index, end_index - start_index);

            if (args.EndsWith(";")) args = args.Substring(0, args.Length - 1);

            return args.Trim();
        }

        private void Generate_TAC_For_Macro_Call(MacroDefinition macro, string arguments, Dictionary<string, string> variables)
        {
            string processed_args = arguments;
            foreach (var var in variables)
            {
                processed_args = processed_args.Replace(var.Key, var.Value);
            }

            var args = Parse_Arguments(processed_args);

            string expanded = macro.Body;

            for (int i = 0; i < macro.Parameters.Count && i < args.Count; i++)
            {
                expanded = expanded.Replace(macro.Parameters[i], $"({args[i]})");
            }

            string result = Generate_TAC_From_Expression(expanded, variables);

            tac_code.Add(new TACInstruction("result", "=", result));
        }

        private List<string> Parse_Arguments(string args_str)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(args_str))
                return result;

            int paren_level = 0;
            int start = 0;

            for (int i = 0; i < args_str.Length; i++)
            {
                if (args_str[i] == '(') paren_level++;
                if (args_str[i] == ')') paren_level--;

                if (paren_level == 0 && args_str[i] == ',')
                {
                    result.Add(args_str.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            result.Add(args_str.Substring(start).Trim());
            return result;
        }

        private string Generate_TAC_From_Expression(string expr, Dictionary<string, string> variables)
        {
            expr = expr.Trim();

            if (Is_Constant(expr))
            {
                return expr;
            }

            if (variables != null && variables.ContainsKey(expr))
            {
                return variables[expr];
            }

            while (expr.StartsWith("(") && expr.EndsWith(")") && Is_Balanced(expr.Substring(1, expr.Length - 2)))
            {
                expr = expr.Substring(1, expr.Length - 2).Trim();
            }

            int op_index = Find_Operator_Outside_Parens(expr);

            if (op_index >= 0 && op_index < expr.Length - 1)
            {
                string left = expr.Substring(0, op_index).Trim();
                string op = expr[op_index].ToString();
                string right = expr.Substring(op_index + 1).Trim();

                string left_temp = Generate_TAC_From_Expression(left, variables);
                string right_temp = Generate_TAC_From_Expression(right, variables);

                string result = New_Temp();
                tac_code.Add(new TACInstruction(result, op, left_temp, right_temp));
                return result;
            }

            return expr;
        }

        private bool Is_Constant(string expr)
        {
            return int.TryParse(expr, out _) || double.TryParse(expr, out _);
        }

        private int Find_Operator_Outside_Parens(string expr)
        {
            int paren_level = 0;
            int lowest_prec = int.MaxValue;
            int idx = -1;

            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];

                if (c == '(') paren_level++;
                if (c == ')') paren_level--;
                if (paren_level > 0) continue;

                int prec = Get_Precedence(c);
                if (prec > 0 && prec <= lowest_prec)
                {
                    lowest_prec = prec;
                    idx = i;
                }
            }

            return idx;
        }

        private bool Is_Balanced(string expr)
        {
            int balance = 0;
            foreach (char c in expr)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0) return false;
            }
            return balance == 0;
        }

        private int Get_Precedence(char op)
        {
            switch (op)
            {
                case '+':
                case '-':
                    return 1;
                case '*':
                case '/':
                    return 2;
                default:
                    return 0;
            }
        }

        private void Optimize_Constant_Folding()
        {
            for (int i = 0; i < tac_code.Count; i++)
            {
                var instr = tac_code[i];

                if (instr.Op != "=" && instr.Arg_1 != null && instr.Arg_2 != null)
                {
                    bool is_const1 = Is_Constant_Value(instr.Arg_1);
                    bool is_const2 = Is_Constant_Value(instr.Arg_2);

                    if (is_const1 && is_const2)
                    {
                        double val1 = double.Parse(instr.Arg_1);
                        double val2 = double.Parse(instr.Arg_2);
                        double result = 0;
                        bool computed = true;

                        switch (instr.Op)
                        {
                            case "+": result = val1 + val2; break;
                            case "-": result = val1 - val2; break;
                            case "*": result = val1 * val2; break;
                            case "/":
                                if (val2 != 0) result = val1 / val2;
                                else computed = false;
                                break;
                            default: computed = false; break;
                        }

                        if (computed)
                        {
                            string const_value = result.ToString();
                            var new_instr = new TACInstruction(instr.Result, "=", const_value);
                            new_instr.Optimized = true;
                            new_instr.Original_Text = instr.Original_Text;
                            tac_code[i] = new_instr;
                            fold_count++;

                            Replace_Variable_With_Constant(instr.Result, const_value, i + 1);
                        }
                    }
                }
            }
        }

        private bool Is_Constant_Value(string value)
        {
            return double.TryParse(value, out _);
        }

        private void Replace_Variable_With_Constant(string var_name, string const_value, int start_index)
        {
            for (int i = start_index; i < tac_code.Count; i++)
            {
                if (tac_code[i].Arg_1 == var_name) tac_code[i].Arg_1 = const_value;
                if (tac_code[i].Arg_2 == var_name) tac_code[i].Arg_2 = const_value;
            }
        }

        private void Optimize_Dead_Code_Elimination()
        {
            used_variables.Clear();
            used_variables["result"] = true;

            foreach (var instr in tac_code)
            {
                if (instr.Arg_1 != null && !Is_Constant_Value(instr.Arg_1))
                {
                    used_variables[instr.Arg_1] = true;
                }

                if (instr.Arg_2 != null && !Is_Constant_Value(instr.Arg_2))
                {
                    used_variables[instr.Arg_2] = true;
                }
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                List<int> to_remove = new List<int>();

                for (int i = 0; i < tac_code.Count; i++)
                {
                    var instr = tac_code[i];
                    if (instr.Result != "result" &&
                        !used_variables.ContainsKey(instr.Result) &&
                        instr.Result != null &&
                        instr.Result.StartsWith("t"))
                    {
                        to_remove.Add(i);
                        changed = true;
                    }
                }

                for (int j = to_remove.Count - 1; j >= 0; j--)
                {
                    tac_code.RemoveAt(to_remove[j]);
                }
            }
        }
    }
}