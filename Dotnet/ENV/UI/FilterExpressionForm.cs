using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ENV.UI
{
    public partial class FilterExpressionForm : System.Windows.Forms.Form
    {
        EvaluateExpressions _evaluator;
        public FilterExpressionForm(Firefly.Box.Advanced.TaskCollection activeTasks, bool excludeLastTask,UserMethods userMethods)
        { 
            InitializeComponent();
            _evaluator = new EvaluateExpressions(userMethods,()=>activeTasks);
            _evaluator.ProvideMethodsTo(x => functions.Items.Add(x));
            int i = 0;
            int j = 0;
            var dict = new Dictionary<ColumnBase, string>();
            foreach (var item in EvaluateExpressions.CreateColumnIdentifierList(activeTasks))
            {
                dict.Add(item.Value, item.Key);
            }
            foreach (var task in activeTasks)
            {
                if (++j == activeTasks.Count && excludeLastTask)
                    break;
                variables.Items.Add("<< "+ ENV.UserMethods.GetControllerName(task).TrimEnd(' ') + " >>");
                foreach (var c in task.Columns)
                {
                    i++;
                    string letter;
                    if (dict.TryGetValue(c,out letter))
                        variables.Items.Add(new ColumnItem(letter, c));
                }
            }
            Text = LocalizationInfo.Current.FilterExpression;
            RightToLeft = LocalizationInfo.Current.RightToLeft;
            button1.Text = LocalizationInfo.Current.Ok;
            button2.Text = LocalizationInfo.Current.Cancel;

        }
        class ColumnItem
        {
            string _identifier;
            ColumnBase _column;

            public ColumnItem(string identifier, ColumnBase column)
            {
                _identifier = identifier;
                _column = column;
            }

            public string Identifier
            {
                get { return _identifier; }

            }

            public override string ToString()
            {
                var s = _identifier + " - " + _column.Caption;
                if (_column.Entity != null)
                    s += " (" + _column.Entity.Caption.Trim() + ")";
                return s;
            }
        }

        public string Expression
        {
            set { textBox1.Text = value; _confirmedExpression = value; }
            get { return _confirmedExpression; }
        }

        string _confirmedExpression;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = textBox1.Text.Trim();
                if (!string.IsNullOrEmpty(textBox1.Text))
                    _evaluator.Evaluate<Bool>(textBox1.Text);
                Expression = textBox1.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error In Expression - " + ex.Message);
                DialogResult = DialogResult.None;
            }
        }

        private void variables_DoubleClick(object sender, EventArgs e)
        {
            var x = variables.SelectedItem as ColumnItem;
            if (x != null)
            {
                textBox1.SelectedText = x.Identifier;
                textBox1.Select();
            }
        }

        private void functions_SelectedIndexChanged(object sender, EventArgs e)
        {
            var x = functions.SelectedItem as string;
            if (x != null)
            {
                textBox1.SelectedText = x;
                textBox1.Select();
            }
        }



    }
}
