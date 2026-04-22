using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tflc_1
{
    internal class PolisFunctions
    {
        private int language = 1;

        public string[] Polis(List<string[]> tetrads, int lang)
        {
            language = lang;
            List<string> polis = new List<string>();
            Dictionary<string, int> t_values = new Dictionary<string, int>();

            foreach (var tetrad in tetrads)
            {
                string op = tetrad[0];
                string operand_1 = tetrad[1];
                string operand_2 = tetrad[2];
                string result = tetrad[3];

                int val_1 = Get_Value(operand_1, t_values);
                int val_2 = Get_Value(operand_2, t_values);

                int calc_res = 0;
                switch (op)
                {
                    case "+":
                        calc_res = val_1 + val_2;
                        break;

                    case "-":
                        calc_res = val_1 - val_2;
                        break;

                    case "*":
                        calc_res = val_1 * val_2;
                        break;

                    case "/":
                        if (val_2 == 0)
                        {
                            Print_Message("zero");
                            calc_res = 0;
                            break;
                        }

                        calc_res = val_1 / val_2;
                        break;

                    case "%":
                        if (val_2 == 0)
                        {
                            Print_Message("zero");
                            calc_res = 0;
                            break;
                        }

                        calc_res = val_1 % val_2;
                        break;
                }

                t_values[result] = calc_res;

                polis.Add(operand_1);
                polis.Add(operand_2);
                polis.Add(op);
                polis.Add(calc_res.ToString());
            }

            if (tetrads.Count > 0)
            {
                Print_Message("result", t_values[tetrads[tetrads.Count - 1][3]].ToString());
            }

            return polis.ToArray();
        }

        private int Get_Value(string operand, Dictionary<string, int> t_values)
        {
            if (operand.EndsWith("t")) return t_values[operand];
            return Convert.ToInt32(operand);
        }

        private void Print_Message(string type, string value = null)
        {
            switch (language)
            {
                case 1: Print_Message_RU(type, value); break;
                case 2: Print_Message_EN(type, value); break;
                case 3: Print_Message_KUZ(type, value); break;
            }
        }

        private void Print_Message_RU(string type, string value = null)
        {
            switch (type)
            {
                case "result":
                    MessageBox.Show($"Результат выражения: {value}");
                    break;

                case "zero":
                    MessageBox.Show("Деление на ноль!");
                    break;
            }
        }

        private void Print_Message_EN(string type, string value = null)
        {
            switch (type)
            {
                case "result":
                    MessageBox.Show($"The result of the expression: {value}");
                    break;

                case "zero":
                    MessageBox.Show("Division by zero!");
                    break;
            }
        }

        private void Print_Message_KUZ(string type, string value = null)
        {
            switch (type)
            {
                case "result":
                    MessageBox.Show($"Өрнектің нәтижесі: {value}");
                    break;

                case "zero":
                    MessageBox.Show("Нөлге бөлу!");
                    break;
            }
        }
    }
}
