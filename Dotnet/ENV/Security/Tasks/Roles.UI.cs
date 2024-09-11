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
    partial class RolesUI : UI.GridForm
    {
        Roles _task;
        public RolesUI(Roles task)
        {
            _task = task;
            InitializeComponent();
            gridColumn1.Text = LocalizationInfo.Current.RoleName;
            gridColumn2.Text = LocalizationInfo.Current.Description;
            Text = LocalizationInfo.Current.RoleName;
        }
    }
}