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
    partial class ManageGroupsUI : UI.GridForm
    {
        ManageGroups _task;
        public ManageGroupsUI(ManageGroups task)
        {
            _task = task;
            InitializeComponent();
            button1.Format = LocalizationInfo.Current.Roles;
            gridColumn1.Text = LocalizationInfo.Current.Groups;
            gridColumn2.Text = LocalizationInfo.Current.Roles;
            Text = LocalizationInfo.Current.Groups;

        }

        private void button1_Click(object sender, ButtonClickEventArgs e)
        {
            new Roles(_task._groups.ID, _task,_task._db).Run();
            _task.ResetStatistics();
        }

        private void textBox4_Expand()
        {
            new Roles(_task._groups.ID, _task, _task._db).Run();
            _task.ResetStatistics();
        }

        private void button3_Click(object sender, ButtonClickEventArgs e)
        {

        }
    }
}