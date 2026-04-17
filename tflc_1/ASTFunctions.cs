using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Linq;

namespace tflc_1
{
    internal class ASTFunctions : ParserFunctions
    {
        private Form ast_form;
        private Panel drawing_panel;
        private Dictionary<object, Rectangle> node_bounds;
        private object root_node;
        private string[] tokens;

        private float node_width = 130;
        private float node_height = 60;
        private float horizontal_gap = 40;
        private float vertical_gap = 20;

        public void AST_Print(RichTextBox output)
        {
            if (root_node == null)
            {
                output.Text = "AST is empty";
                return;
            }

            output.Clear();
            output.Font = new Font("Consolas", 10);

            Print_Node(output, root_node);
        }

        private void Print_Node(RichTextBox output, object node)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";

            switch (node_type)
            {
                case "Define":
                    output.AppendText("Macros\n");
                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Print_Function_Call(output, children[i], "", i == children.Count - 1);
                    }
                    break;
            }
        }

        private void Print_Function_Call(RichTextBox output, object node, string indent, bool is_last)
        {
            string macros_name = node.GetType().GetProperty("MacroName")?.GetValue(node)?.ToString() ?? "?";
            string prefix = is_last ? "└── " : "├── ";
            output.AppendText(indent + prefix + "FunctionCallNode\n");

            string name_indent = indent + (is_last ? "    " : "│   ");
            output.AppendText(name_indent + "├── name: \"" + macros_name + "\"\n");

            var children = Get_Children(node);

            if (children.Count > 0)
            {
                var real_parameters = new List<object>();
                object body_node = null;

                foreach (var child in children)
                {
                    string param_name = child.GetType().GetProperty("ParameterName")?.GetValue(child)?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(param_name) && param_name != "?")
                    {
                        real_parameters.Add(child);
                    }

                    var param_children = Get_Children(child);
                    if (param_children.Count > 0 && body_node == null)
                    {
                        body_node = param_children[0];
                    }
                }

                if (real_parameters.Count > 0)
                {
                    output.AppendText(name_indent + "├── parameters:\n");
                    string param_indent = name_indent + "│   ";

                    for (int i = 0; i < real_parameters.Count; i++)
                    {
                        bool is_last_param = (i == real_parameters.Count - 1);
                        Print_Param_Node(output, real_parameters[i], param_indent, is_last_param);
                    }
                }

                if (body_node != null)
                {
                    string body_prefix = (real_parameters.Count > 0) ? "└── " : "├── ";
                    output.AppendText(name_indent + body_prefix + "MacrosBody:\n");
                    Print_Body_Node(output, body_node, name_indent + "    ", true);
                }
                else if (real_parameters.Count == 0 && children.Count > 0)
                {
                    output.AppendText(name_indent + "└── MacrosBody:\n");
                    Print_Body_Node(output, children[0], name_indent + "    ", true);
                }
            }
            else
            {
                output.AppendText(name_indent + "└── MacrosBody:\n");
                output.AppendText(name_indent + "    (empty)\n");
            }
        }

        private void Print_Param_Node(RichTextBox output, object node, string indent, bool is_last)
        {
            string param_name = node.GetType().GetProperty("ParameterName")?.GetValue(node)?.ToString() ?? "?";

            if (param_name == "?" || string.IsNullOrEmpty(param_name))
            {
                return;
            }

            string prefix = is_last ? "└── " : "├── ";
            output.AppendText(indent + prefix + "ParamNode\n");

            string name_indent = indent + (is_last ? "    " : "│   ");
            output.AppendText(name_indent + "└── name: \"" + param_name + "\"\n");
        }

        private void Print_Body_Node(RichTextBox output, object node, string indent, bool is_last)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";

            switch (node_type)
            {
                case "OP":
                    output.AppendText(indent + "OpNode\n");
                    string operator_token = node.GetType().GetProperty("Operator")?.GetValue(node)?.ToString() ?? "?";

                    string op_indent = indent + "    ";
                    output.AppendText(op_indent + "├── operator: \"" + operator_token + "\"\n");

                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Print_Expression_Node(output, children[i], op_indent, i == children.Count - 1);
                    }
                    break;

                default:
                    Print_Expression_Node(output, node, indent, is_last);
                    break;
            }
        }

        private void Print_Expression_Node(RichTextBox output, object node, string indent, bool is_last)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";
            string prefix = is_last ? "└── " : "├── ";

            switch (node_type)
            {
                case "OP":
                    output.AppendText(indent + prefix + "OpNode\n");
                    string operator_token = node.GetType().GetProperty("Operator")?.GetValue(node)?.ToString() ?? "?";

                    string op_indent = indent + (is_last ? "    " : "│   ");
                    output.AppendText(op_indent + "├── operator: \"" + operator_token + "\"\n");

                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Print_Expression_Node(output, children[i], op_indent, i == children.Count - 1);
                    }
                    break;

                case "Variable":
                    output.AppendText(indent + prefix + "VariableNode\n");
                    string var_name = node.GetType().GetProperty("Name")?.GetValue(node)?.ToString() ?? "?";

                    string var_indent = indent + (is_last ? "    " : "│   ");
                    output.AppendText(var_indent + "└── name: \"" + var_name + "\"\n");
                    break;

                case "Number":
                    output.AppendText(indent + prefix + "NumberNode\n");
                    int value = (int)(node.GetType().GetProperty("Value")?.GetValue(node) ?? 0);

                    string num_indent = indent + (is_last ? "    " : "│   ");
                    output.AppendText(num_indent + "└── value: " + value + "\n");
                    break;
            }
        }


        public void AST_Graphic(string text)
        {
            (int[] codes_all, string[] tokens_all, int[] lines_all, int[] positions) = Find_All_Tokens(text);
            (string[] tokens, int[] codes, int[] lines, int[] pos) = Space_Clean(tokens_all, codes_all, lines_all, positions);
            this.tokens = tokens;

            Print_AST();

            ast_form = new Form();
            ast_form.Text = "AST";
            ast_form.Size = new Size(900, 700);
            ast_form.StartPosition = FormStartPosition.CenterScreen;
            ast_form.Load += AstForm_Load;

            drawing_panel = new Panel();
            drawing_panel.BackColor = Color.White;
            drawing_panel.Dock = DockStyle.Fill;
            drawing_panel.Paint += DrawingPanel_Paint;
            drawing_panel.Resize += DrawingPanel_Resize;

            ast_form.Controls.Add(drawing_panel);
            ast_form.ShowDialog();
        }

        private void AstForm_Load(object sender, EventArgs e)
        {
            if (root_node != null && drawing_panel != null && drawing_panel.Width > 0)
            {
                if (node_bounds == null) node_bounds = new Dictionary<object, Rectangle>();
                node_bounds.Clear();
                Calculate_Positions(root_node, drawing_panel.Width / 2, 30);
                drawing_panel.Invalidate();
            }
        }

        private void Print_AST()
        {
            try
            {
                node_bounds = new Dictionary<object, Rectangle>();
                int position = 0;

                if (position < tokens.Length && tokens[position] == "#") position++;
                if (position < tokens.Length && tokens[position] == "define") position++;

                if (position >= tokens.Length) return;
                string macros_name = tokens[position];
                position++;

                var define_node = new { Type = "Define", MacroName = macros_name, Children = new List<object>() };
                var macros_call_node = new { Type = "Function_Call", MacroName = macros_name, Children = new List<object>() };

                ((List<object>)define_node.Children).Add(macros_call_node);

                position++;

                List<string> parameters = new List<string>();

                while (position < tokens.Length && tokens[position] != ")")
                {
                    if (tokens[position] == ",")
                    {
                        position++;
                        continue;
                    }

                    if (position < tokens.Length && Is_Parameter_Name(tokens[position]))
                    {
                        string parameter_name = tokens[position];
                        parameters.Add(parameter_name);
                        position++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (position < tokens.Length && tokens[position] == ")") position++;

                if (parameters.Count > 0)
                {
                    foreach (string param_name in parameters)
                    {
                        var parameter_node = new { Type = "Parameter", ParameterName = param_name, Children = new List<object>() };
                        ((List<object>)macros_call_node.Children).Add(parameter_node);
                    }

                    object expression = Parse_Expression(ref position, parameters);
                    if (expression != null)
                    {
                        foreach (var child in macros_call_node.Children)
                        {
                            var param_node = child;
                            var children_list = ((List<object>)param_node.GetType().GetProperty("Children").GetValue(param_node));
                            children_list.Add(expression);
                        }
                    }
                }
                else
                {
                    object expression = Parse_Expression(ref position, null);
                    if (expression != null)
                    {
                        ((List<object>)macros_call_node.Children).Add(expression);
                    }
                }

                root_node = define_node;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения AST: {ex.Message}");
            }
        }

        private object Parse_Expression(ref int position, List<string> parameters)
        {
            if (position >= tokens.Length) return null;

            object left = Parse_Primary(ref position, parameters);

            while (position < tokens.Length && Is_Operator(tokens[position]))
            {
                string operator_token = tokens[position];
                position++;

                object right = Parse_Primary(ref position, parameters);

                var op = new { Type = "OP", Operator = operator_token, Children = new List<object>() };
                ((List<object>)op.Children).Add(left);
                ((List<object>)op.Children).Add(right);

                left = op;
            }

            return left;
        }

        private object Parse_Primary(ref int position, List<string> parameters)
        {
            if (position >= tokens.Length) return null;

            string token = tokens[position];

            if (token == "(")
            {
                position++;
                object expression = Parse_Expression(ref position, parameters);

                if (position < tokens.Length && tokens[position] == ")") position++;

                return expression;
            }
            else if (parameters != null && parameters.Contains(token))
            {
                position++;
                return new { Type = "Variable", Name = token, Children = new List<object>() };
            }
            else if (Is_Number(token))
            {
                position++;
                return new { Type = "Number", Value = int.Parse(token), Children = new List<object>() };
            }
            else
            {
                position++;
                return new { Type = "Variable", Name = token, Children = new List<object>() };
            }
        }

        private bool Is_Parameter_Name(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            if (token == "(" || token == ")" || token == "," || token == "+" || token == "-" || token == "*" || token == "/")
                return false;

            return char.IsLetter(token[0]);
        }

        private bool Is_Operator(string token)
        {
            return token == "*" || token == "/" || token == "+" || token == "-";
        }

        private bool Is_Number(string token)
        {
            return int.TryParse(token, out _);
        }

        private void Calculate_Positions(object node, float x, float y)
        {
            if (node == null) return;
            if (node_bounds == null) node_bounds = new Dictionary<object, Rectangle>();

            node_bounds[node] = new Rectangle((int)(x - node_width / 2), (int)y, (int)node_width, (int)node_height);

            var children = Get_Children(node);
            if (children.Count == 0) return;

            float children_width = children.Count * node_width + (children.Count - 1) * horizontal_gap;
            float start = x - children_width / 2 + node_width / 2;

            for (int i = 0; i < children.Count; i++)
            {
                float child_x = start + i * (node_width + horizontal_gap);
                float child_y = y + node_height + vertical_gap;
                Calculate_Positions(children[i], child_x, child_y);
            }
        }

        private List<object> Get_Children(object node)
        {
            if (node == null) return new List<object>();

            var property = node.GetType().GetProperty("Children");
            if (property != null)
            {
                var children = property.GetValue(node) as List<object>;
                return children ?? new List<object>();
            }
            return new List<object>();
        }

        private string Get_Node_Text(object node)
        {
            if (node == null) return "?";

            var type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString();

            switch (type)
            {
                case "Define":
                    return "#define";
                case "Function_Call":
                    return node.GetType().GetProperty("MacroName")?.GetValue(node)?.ToString() ?? "?";
                case "Parameter":
                    string param_name = node.GetType().GetProperty("ParameterName")?.GetValue(node)?.ToString();
                    return $"param: {param_name}";
                case "OP":
                    return node.GetType().GetProperty("Operator")?.GetValue(node)?.ToString() ?? "?";
                case "Variable":
                    return node.GetType().GetProperty("Name")?.GetValue(node)?.ToString() ?? "?";
                case "Number":
                    return node.GetType().GetProperty("Value")?.GetValue(node)?.ToString() ?? "?";
                default:
                    return "?";
            }
        }

        private Color Get_Node_Color(object node)
        {
            if (node == null) return Color.LightGray;

            var type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString();

            switch (type)
            {
                case "Define":
                    return Color.Purple;
                case "Function_Call":
                    return Color.Yellow;
                case "Parameter":
                    return Color.DeepSkyBlue;
                case "OP":
                    return Color.OrangeRed;
                case "Variable":
                    return Color.DeepSkyBlue;
                case "Number":
                    return Color.YellowGreen;
                default:
                    return Color.LightGray;
            }
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            if (root_node == null)
            {
                e.Graphics.DrawString("Не удалось построить AST", new Font("Arial", 14), Brushes.Red, 50, 50);
                return;
            }

            if (node_bounds == null || node_bounds.Count == 0)
            {
                if (drawing_panel != null && drawing_panel.Width > 0)
                {
                    node_bounds = new Dictionary<object, Rectangle>();
                    Calculate_Positions(root_node, drawing_panel.Width / 2, 30);
                }
                else
                {
                    e.Graphics.DrawString("Вычисление позиций...", new Font("Arial", 14), Brushes.Gray, 50, 50);
                    return;
                }
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Draw_Connections(e.Graphics, root_node);
            Draw_Nodes(e.Graphics, root_node);
        }

        private void Draw_Connections(Graphics g, object node)
        {
            if (node == null || node_bounds == null || !node_bounds.ContainsKey(node)) return;

            Rectangle parentRect = node_bounds[node];
            Point startPoint = new Point(
                parentRect.X + parentRect.Width / 2,
                parentRect.Y + parentRect.Height
            );

            foreach (var child in Get_Children(node))
            {
                if (node_bounds != null && node_bounds.ContainsKey(child))
                {
                    Rectangle childRect = node_bounds[child];
                    Point endPoint = new Point(
                        childRect.X + childRect.Width / 2,
                        childRect.Y
                    );

                    using (Pen pen = new Pen(Color.FromArgb(100, 0, 0, 0), 2))
                    {
                        g.DrawLine(pen, startPoint, endPoint);
                    }

                    Draw_Connections(g, child);
                }
            }
        }

        private void Draw_Nodes(Graphics g, object node)
        {
            if (node == null || node_bounds == null || !node_bounds.ContainsKey(node)) return;

            Rectangle rect = node_bounds[node];
            string nodeType = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString();

            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                g.FillRectangle(shadowBrush, rect.X + 3, rect.Y + 3, rect.Width, rect.Height);
            }

            using (SolidBrush brush = new SolidBrush(Get_Node_Color(node)))
            {
                g.FillRectangle(brush, rect);
            }

            using (Pen pen = new Pen(Color.Black, 2))
            {
                pen.Width = 2.5f;
                g.DrawRectangle(pen, rect);
            }

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            FontStyle fontStyle = FontStyle.Bold;
            if (nodeType == "Parameter")
            {
                fontStyle = FontStyle.Bold | FontStyle.Italic;
            }

            using (Font font = new Font("Segoe UI", 11, fontStyle))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(Get_Node_Text(node), font, textBrush, rect, sf);
            }

            foreach (var child in Get_Children(node))
            {
                Draw_Nodes(g, child);
            }
        }

        private void DrawingPanel_Resize(object sender, EventArgs e)
        {
            if (root_node != null && drawing_panel != null && drawing_panel.Width > 0)
            {
                if (node_bounds == null) node_bounds = new Dictionary<object, Rectangle>();
                node_bounds.Clear();
                Calculate_Positions(root_node, drawing_panel.Width / 2, 30);
                drawing_panel.Invalidate();
            }
        }
    }
}