using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Labs.UI
{
    partial class MultiSelectFilterUI : ENV.UI.Form
    {
        Action _ok;
        public MultiSelectFilterUI(Action ok, BoolColumn checkedColumn, ColumnBase value, NumberColumn numOfRows)
        {
            _ok = ok;
            InitializeComponent();
            this.checkBox1.Data = checkedColumn;
            this.textBox1.Data = value;
            this.textBox2.Data = numOfRows;
            this.BackColor = FaceLiftDemo.BackGroundColor;

        }

        private void okButton_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            _ok();
        }

        private void cancelButton_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            Firefly.Box.Context.Current.InvokeUICommand(() => {
                Close();
            });
        }
    }
    class Button : ENV.UI.Button
    {
        public Button()
        {
            CoolEnabled = true;
            Font = new Font("Arial", 9, FontStyle.Bold);
        }
    }
}