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
    partial class ManageUsersUI : UI.GridForm
    {
        ManageUsers _task;
        public ManageUsersUI(ManageUsers task)
        {
            _task = task;
            InitializeComponent();
            this.button3.Format = LocalizationInfo.Current.Roles;
            this.button4.Format = LocalizationInfo.Current.Import;
            button5.Format = LocalizationInfo.Current.Cancel;

            gridColumn1.Text = LocalizationInfo.Current.UserName;
            gridColumn2.Text = LocalizationInfo.Current.Details;
            gridColumn3.Text = LocalizationInfo.Current.Password;
            gridColumn4.Text = LocalizationInfo.Current.Roles;
            gridColumn5.Text = LocalizationInfo.Current.Groups;
            button1.Format = LocalizationInfo.Current.Groups;
            LblAdditionalInfo.Text = LocalizationInfo.Current.AdditionalInfo;
            Text = LocalizationInfo.Current.Users;
            
        }

        private void ManageUsersUI_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, ButtonClickEventArgs e)
        {
            textBox4_Expand();
        }
        void textBox4_Expand()
        {
            _task.ShowUserRoles();
            
        }

        private void button1_Click(object sender, ButtonClickEventArgs e)
        {
            textBox5_Expand();
        }
        void textBox5_Expand()
        {
            _task.ShowUserGroups();
            
        }

        private void textBox3_Change()
        {

        }

        private void textBox4_Change()
        {

        }

        private void button4_Click(object sender, ButtonClickEventArgs e)
        {
            _task.DoImport();
        }

        private void button4_BindEnabled(object sender, BooleanBindingEventArgs e)
        {
            e.Value = _task.AllowUpdate;
        }

        private void textBox3_Expand()
        {
            _task.CahangePassword();
        }
    }
}