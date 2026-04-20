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

            Print_Node(output, root_node, "");
        }

        private void Print_Node(RichTextBox output, object node, string indent)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";

            switch (node_type)
            {
                case "Macros":
                    output.AppendText("Macros\n");
                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        bool is_last = (i == children.Count - 1);
                        Print_Child_Node(output, children[i], indent, is_last);
                    }
                    break;
            }
        }

        private void Print_Child_Node(RichTextBox output, object node, string indent, bool is_last)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";
            string prefix = is_last ? "└── " : "├── ";

            switch (node_type)
            {
                case "Define":
                    output.AppendText(indent + prefix + "Define: #define\n");
                    break;

                case "FunctionCallNode":
                    output.AppendText(indent + prefix + "FunctionCallNode\n");
                    Print_FunctionCallDetails(output, node, indent + (is_last ? "    " : "│   "));
                    break;
            }
        }

        private void Print_FunctionCallDetails(RichTextBox output, object node, string indent)
        {
            string macros_name = node.GetType().GetProperty("Name")?.GetValue(node)?.ToString() ?? "?";
            output.AppendText(indent + "├── name: \"" + macros_name + "\"\n");

            var children = Get_Children(node);

            object parameters_node = null;
            object macros_body_node = null;

            foreach (var child in children)
            {
                string child_type = child.GetType().GetProperty("Type")?.GetValue(child)?.ToString() ?? "";

                if (child_type == "parameters")
                {
                    parameters_node = child;
                }
                else if (child_type == "MacrosBody")
                {
                    macros_body_node = child;
                }
            }

            if (parameters_node != null)
            {
                var parameters = Get_Children(parameters_node);
                if (parameters.Count > 0)
                {
                    output.AppendText(indent + "├── parameters:\n");
                    string param_indent = indent + "│   ";

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        bool is_last_param = (i == parameters.Count - 1);
                        Print_ParamNode(output, parameters[i], param_indent, is_last_param);
                    }
                }
                else
                {
                    output.AppendText(indent + "├── parameters:\n");
                    output.AppendText(indent + "│   └── (empty)\n");
                }
            }

            if (macros_body_node != null)
            {
                string body_prefix = (parameters_node != null && Get_Children(parameters_node).Count > 0) ? "└── " : "├── ";
                output.AppendText(indent + body_prefix + "MacrosBody:\n");

                var body_children = Get_Children(macros_body_node);
                if (body_children.Count > 0)
                {
                    Print_BodyNode(output, body_children[0], indent + "    ", true);
                }
                else
                {
                    output.AppendText(indent + "    └── (empty)\n");
                }
            }
            else if (children.Count > 0 && parameters_node == null)
            {
                foreach (var child in children)
                {
                    string child_type = child.GetType().GetProperty("Type")?.GetValue(child)?.ToString() ?? "";
                    if (child_type != "name" && child_type != "parameters")
                    {
                        output.AppendText(indent + "└── MacrosBody:\n");
                        Print_BodyNode(output, child, indent + "    ", true);
                        break;
                    }
                }
            }
        }

        private void Print_ParamNode(RichTextBox output, object node, string indent, bool is_last)
        {
            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "";

            if (node_type == "ParamNode")
            {
                string prefix = is_last ? "└── " : "├── ";
                output.AppendText(indent + prefix + "ParamNode\n");

                var children = Get_Children(node);
                string name_indent = indent + (is_last ? "    " : "│   ");

                for (int i = 0; i < children.Count; i++)
                {
                    bool is_last_child = (i == children.Count - 1);
                    Print_NameNode(output, children[i], name_indent, is_last_child);
                }
            }
        }

        private void Print_NameNode(RichTextBox output, object node, string indent, bool is_last)
        {
            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "";

            if (node_type == "name")
            {
                string name = node.GetType().GetProperty("Name")?.GetValue(node)?.ToString() ?? "?";
                string prefix = is_last ? "└── " : "├── ";
                output.AppendText(indent + prefix + "name: \"" + name + "\"\n");
            }
        }

        private void Print_BodyNode(RichTextBox output, object node, string indent, bool is_last)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";

            switch (node_type)
            {
                case "OpNode":
                    output.AppendText(indent + "OpNode\n");
                    string operator_token = node.GetType().GetProperty("Operator")?.GetValue(node)?.ToString() ?? "?";

                    string op_indent = indent + "    ";
                    output.AppendText(op_indent + "├── operator: \"" + operator_token + "\"\n");

                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Print_ExpressionNode(output, children[i], op_indent, i == children.Count - 1);
                    }
                    break;

                default:
                    Print_ExpressionNode(output, node, indent, is_last);
                    break;
            }
        }

        private void Print_ExpressionNode(RichTextBox output, object node, string indent, bool is_last)
        {
            if (node == null) return;

            string node_type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString() ?? "Unknown";
            string prefix = is_last ? "└── " : "├── ";

            switch (node_type)
            {
                case "OpNode":
                    output.AppendText(indent + prefix + "OpNode\n");
                    string operator_token = node.GetType().GetProperty("Operator")?.GetValue(node)?.ToString() ?? "?";

                    string op_indent = indent + (is_last ? "    " : "│   ");
                    output.AppendText(op_indent + "├── operator: \"" + operator_token + "\"\n");

                    var children = Get_Children(node);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Print_ExpressionNode(output, children[i], op_indent, i == children.Count - 1);
                    }
                    break;

                case "VariableNode":
                    output.AppendText(indent + prefix + "VariableNode\n");
                    string var_indent = indent + (is_last ? "    " : "│   ");

                    var var_children = Get_Children(node);
                    for (int i = 0; i < var_children.Count; i++)
                    {
                        bool is_last_child = (i == var_children.Count - 1);
                        string child_type = var_children[i].GetType().GetProperty("Type")?.GetValue(var_children[i])?.ToString() ?? "";

                        if (child_type == "name")
                        {
                            string var_name = var_children[i].GetType().GetProperty("Name")?.GetValue(var_children[i])?.ToString() ?? "?";
                            output.AppendText(var_indent + (is_last_child ? "└── " : "├── ") + "name: \"" + var_name + "\"\n");
                        }
                    }
                    break;

                case "NumberNode":
                    int value = (int)(node.GetType().GetProperty("Value")?.GetValue(node) ?? 0);
                    output.AppendText(indent + prefix + "value: " + value + "\n");
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

                var macros_node = new { Type = "Macros", Children = new List<object>() };

                var define_node = new { Type = "Define", Text = "#define", Children = new List<object>() };
                ((List<object>)macros_node.Children).Add(define_node);

                var function_call_node = new { Type = "FunctionCallNode", Name = macros_name, Parameters = (object)null, MacrosBody = (object)null, Children = new List<object>() };
                ((List<object>)macros_node.Children).Add(function_call_node);

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
                    var name_node = new { Type = "name", Name = macros_name, Children = new List<object>() };
                    ((List<object>)function_call_node.Children).Add(name_node);

                    var parameters_node = new { Type = "parameters", Children = new List<object>() };
                    ((List<object>)function_call_node.Children).Add(parameters_node);

                    foreach (string param_name in parameters)
                    {
                        var param_node = new { Type = "ParamNode", Name = param_name, Children = new List<object>() };
                        ((List<object>)parameters_node.Children).Add(param_node);

                        var param_name_node = new { Type = "name", Name = param_name, Children = new List<object>() };
                        ((List<object>)param_node.Children).Add(param_name_node);
                    }

                    object expression = Parse_Expression(ref position, parameters);
                    if (expression != null)
                    {
                        var macros_body = new { Type = "MacrosBody", Children = new List<object>() };
                        ((List<object>)function_call_node.Children).Add(macros_body);
                        ((List<object>)macros_body.Children).Add(expression);
                    }
                }
                else
                {
                    var name_node = new { Type = "name", Name = macros_name, Children = new List<object>() };
                    ((List<object>)function_call_node.Children).Add(name_node);

                    object expression = Parse_Expression(ref position, null);
                    if (expression != null)
                    {
                        var macros_body = new { Type = "MacrosBody", Children = new List<object>() };
                        ((List<object>)function_call_node.Children).Add(macros_body);
                        ((List<object>)macros_body.Children).Add(expression);
                    }
                }

                root_node = macros_node;
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

                var op_node = new { Type = "OpNode", Operator = operator_token, Children = new List<object>() };
                ((List<object>)op_node.Children).Add(left);
                ((List<object>)op_node.Children).Add(right);

                left = op_node;
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
                var var_node = new { Type = "VariableNode", Name = token, Children = new List<object>() };
                var name_node = new { Type = "name", Name = token, Children = new List<object>() };
                ((List<object>)var_node.Children).Add(name_node);
                return var_node;
            }
            else if (Is_Number(token))
            {
                position++;
                var num_node = new { Type = "NumberNode", Value = int.Parse(token), Children = new List<object>() };
                return num_node;
            }
            else
            {
                position++;
                var var_node = new { Type = "VariableNode", Name = token, Children = new List<object>() };
                var name_node = new { Type = "name", Name = token, Children = new List<object>() };
                ((List<object>)var_node.Children).Add(name_node);
                return var_node;
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
                case "Macros":
                    return "Macros";
                case "Define":
                    return node.GetType().GetProperty("Text")?.GetValue(node)?.ToString() ?? "Define";
                case "FunctionCallNode":
                    return "FunctionCallNode";
                case "name":
                    string name = node.GetType().GetProperty("Name")?.GetValue(node)?.ToString();
                    return $"{name}";
                case "parameters":
                    return "Parameters";
                case "ParamNode":
                    return "ParamNode";
                case "MacrosBody":
                    return "MacrosBody";
                case "OpNode":
                    return "OpNode";
                case "VariableNode":
                    return "VariableNode";
                case "NumberNode":
                    int value = (int)(node.GetType().GetProperty("Value")?.GetValue(node) ?? 0);
                    return $"{value}";
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
                default:
                    return Color.White;
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

            FontStyle fontStyle = FontStyle.Regular;
            string nodeText = Get_Node_Text(node);
            var type = node.GetType().GetProperty("Type")?.GetValue(node)?.ToString();

            if (type == "Define" || type == "FunctionCallNode" || type == "OpNode" || type == "Macros")
            {
                fontStyle = FontStyle.Bold;
            }

            using (Font font = new Font("Segoe UI", 10, fontStyle))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(nodeText, font, textBrush, rect, sf);
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