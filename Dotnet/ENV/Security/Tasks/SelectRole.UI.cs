using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class SelectRoleUI : UI.GridForm
    {
        SelectRole _task;
        public SelectRoleUI(SelectRole task)
        {
            _task = task;
            InitializeComponent();
            gridColumn1.Text = LocalizationInfo.Current.Roles;
            gridColumn2.Text = LocalizationInfo.Current.Description;
            okBtn.Text = LocalizationInfo.Current.Choose;
            Text = LocalizationInfo.Current.ChooseRole;

        }
    }
}