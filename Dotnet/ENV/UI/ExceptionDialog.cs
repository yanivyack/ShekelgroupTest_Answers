using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public partial class ExceptionDialog : System.Windows.Forms.Form
    {
        bool showDetails;
        public bool AllowContinue { get; set; }
        public ExceptionDialog(Exception e, string additionalInfo, string[] args)
        {

            InitializeComponent();
            this.lblException.Text = e.Message;
            this.txtDetails.Text = ErrorLog.CreateErrorDescription(e, additionalInfo, args);
            ResizeDetails();
        }
        protected override void OnLoad(EventArgs e)
        {
            btnIgnore.Visible = AllowContinue;
            btnQuit.Visible = !AllowContinue;
            warningPictureBox.Visible = AllowContinue;
            errorPictureBox.Visible = !AllowContinue;
            this.Text = AllowContinue ? "Warning" : "Error";

            lblHelp.Text =
                "An error has occured in the application. ";
            if (AllowContinue)
                lblHelp.Text += "Click Continue to continue. ";
            else
                lblHelp.Text += "Click Quit to close the application.";
        }
        private void btnQuit_Click(object sender, EventArgs e)
        {

        }
        private void btnDetails_Click(object sender, EventArgs e)
        {
            showDetails = !showDetails;
            ResizeDetails();
        }

        void ResizeDetails()
        {
            if (showDetails)
            {
                btnDetails.Text = "Hide &Details";
                MaximumSize = new System.Drawing.Size(0, 0);
                Height = 410;
                MaximizeBox = true;
                SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            }
            else
            {
                MaximizeBox = false;
                btnDetails.Text = "&Details";
                Height = 180;
                Width = 444;
                MaximumSize = new System.Drawing.Size(444, 180);
                SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
                WindowState = FormWindowState.Normal;
            }
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCopyInfo_Click(object sender, EventArgs e)
        {
            ENV.UserMethods.Instance.ClipAdd(txtDetails.Text);
            ENV.UserMethods.Instance.ClipWrite();
        }
    }
}
