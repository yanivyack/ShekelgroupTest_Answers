using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;

namespace ENV.Security.Tasks
{
    partial class SecuredValuesUI : UI.GridForm
    {
        SecuredValues _task;
        public SecuredValuesUI(SecuredValues task)
        {
            _task = task;
            InitializeComponent();
            Text = LocalizationInfo.Current.SecuredValues;
            button3.Format = LocalizationInfo.Current.Cancel;
            gridColumn1.Text = LocalizationInfo.Current.Name;
            gridColumn2.Text = LocalizationInfo.Current.Values;
        }

        private void SecuredValuesUI_Click(object sender, EventArgs e)
        {

        }
    }
}