using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Labs.UI
{
    partial class CustomFilterDialogUI : ENV.UI.Form
    {
        Action _applyFilter;

        public CustomFilterDialogUI(Action applyFilter)
        {
            _applyFilter = applyFilter;
            InitializeComponent();
            BackColor = Labs.FaceLiftDemo.BackGroundColor;

        }

        private void okButton_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            _applyFilter();
            Firefly.Box.Context.Current.InvokeUICommand(() =>
            {
                Close();
            });
        }

        private void cancelButton_Click(object sender, Firefly.Box.UI.Advanced.ButtonClickEventArgs e)
        {
            Firefly.Box.Context.Current.InvokeUICommand(() => { 
            Close();
            });
        }

        private void label2_BindVisible(object sender, Firefly.Box.UI.Advanced.BooleanBindingEventArgs e)
        {
            e.Value = filterTypeComboBox.Text == "Is Between";
        }
    }
}