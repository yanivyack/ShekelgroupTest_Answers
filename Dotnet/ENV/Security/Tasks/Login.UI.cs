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
    partial class LoginUI : UI.Form
    {
        Login _task;
        public LoginUI(Login task)
        {
            _task = task;
            InitializeComponent();
            this.gbLogonParameters.Text = LocalizationInfo.Current.LogonParameters;
            this.PleaseEnter.Text = LocalizationInfo.Current.PleaseEnterIdAndPassword;
            this.button1.Format = "50";
            this.button1.Data.Column.Value =  LocalizationInfo.Current.Cancel.Replace("\\","");
            
            this.textBox3.Text = LocalizationInfo.Current.UserName+":";
            this.textBox4.Text = LocalizationInfo.Current.Password+":";
            this.button2.Format="50";
            this.button2.Data.Column.Value= LocalizationInfo.Current.Ok;
            this.Text = LocalizationInfo.Current.SystemLogin;
            this.lblDate.Text = LocalizationInfo.Current.Date + ":";
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginUI));
            using (var bmp = new Bitmap(((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")))))
            {
                Icon = Icon.FromHandle(bmp.GetHicon());

            }
            ShowInTaskbar = false;
            ContextMenuStrip = new ContextMenuStrip(); //suppress context menu strip.

            if (Firefly.Box.Command.GoToNextControl.AdditionalShortcuts.Length > 0 &&
                Firefly.Box.Command.GoToNextControl.AdditionalShortcuts[0] == Keys.Enter)
                AcceptButton = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Login.InputDate)
            {
                lblDate.Visible = false;
                txtDate.Visible = false;
                Size = new Size(470, 207);
            }
        }
        private void LoginUI_Load(object sender, EventArgs e)
        {

        }

        private void textBox4_Change()
        {

        }

        private void textBox2_Change()
        {

        }

        private void textBox3_Change()
        {

        }

        private void textBox1_Change()
        {

        }

        private void button2_Click(object sender, ButtonClickEventArgs e)
        {
            _task.DoLogin();
        }

        private void button1_Click(object sender, ButtonClickEventArgs e)
        {
            e.Raise(Command.Exit);
        }
    }
}