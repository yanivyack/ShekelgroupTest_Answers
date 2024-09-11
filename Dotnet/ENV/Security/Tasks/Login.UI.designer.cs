using System;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class LoginUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginUI));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new ENV.Security.UI.Button();
            this.button1 = new ENV.Security.UI.Button();
            this.txtDate = new ENV.Security.UI.TextBox();
            this.lblDate = new ENV.Security.UI.TextBox();
            this.textBox4 = new ENV.Security.UI.TextBox();
            this.textBox2 = new ENV.Security.UI.TextBox();
            this.textBox3 = new ENV.Security.UI.TextBox();
            this.textBox1 = new ENV.Security.UI.TextBox();
            this.PleaseEnter = new ENV.Security.UI.TextBox();
            this.gbLogonParameters = new ENV.UI.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(17, 32);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(51, 50);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Format = "כניסה";
            this.button2.Location = new System.Drawing.Point(234, 157);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 23);
            this.button2.Text = "button2";
            this.button2.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button2_Click);
            this.button2.Data = this._task.Ok;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Format = "יציאה";
            this.button1.Location = new System.Drawing.Point(336, 157);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 23);
            this.button1.Text = "button1";
            this.button1.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            this.button1.Data = this._task.Cancel;
            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(191, 113);
            this.txtDate.Name = "txtDate";
            this.txtDate.Size = new System.Drawing.Size(245, 21);
            this.txtDate.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.txtDate.Text = "textBox2";
            this.txtDate.Change += new System.Action(this.textBox2_Change);
            this.txtDate.Data = this._task.Date;
            this.txtDate.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // lblDate
            // 
            this.lblDate.BackColor = System.Drawing.SystemColors.Control;
            this.lblDate.Location = new System.Drawing.Point(87, 113);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(98, 21);
            this.lblDate.Text = "Date";
            this.lblDate.Change += new System.Action(this.textBox4_Change);
            this.lblDate.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.Location = new System.Drawing.Point(87, 86);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(98, 21);
            this.textBox4.Text = "Password";
            
            this.textBox4.Change += new System.Action(this.textBox4_Change);
            this.textBox4.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(191, 86);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(245, 21);
            this.textBox2.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.textBox2.Text = "textBox2";
            this.textBox2.UseSystemPasswordChar = true;
            this.textBox2.Change += new System.Action(this.textBox2_Change);
            this.textBox2.Data = this._task.Password;
            this.textBox2.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            this.textBox3.Location = new System.Drawing.Point(87, 59);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(98, 21);
            this.textBox3.Text = "User Name";
            this.textBox3.Change += new System.Action(this.textBox3_Change);
            this.textBox3.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(191, 59);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(245, 21);
            this.textBox1.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.textBox1.Text = "textBox1";
            this.textBox1.Change += new System.Action(this.textBox1_Change);
            this.textBox1.Data = this._task.UserName;
            this.textBox1.AdvancedAnchor = new AdvancedAnchor(0, 0, 0, 0, false);
            // 
            // PleaseEnter
            // 
            this.PleaseEnter.BackColor = System.Drawing.SystemColors.Control;
            this.PleaseEnter.Location = new System.Drawing.Point(87, 32);
            this.PleaseEnter.Name = "PleaseEnter";
            this.PleaseEnter.Size = new System.Drawing.Size(272, 21);
            this.PleaseEnter.Text = "Please enter your system user ID and password";
            this.PleaseEnter.Change += new System.Action(this.textBox3_Change);
            // 
            // gbLogonParameters
            // 
            this.gbLogonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbLogonParameters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gbLogonParameters.Location = new System.Drawing.Point(7, 4);
            this.gbLogonParameters.Name = "gbLogonParameters";
            this.gbLogonParameters.Size = new System.Drawing.Size(441, 191);
            this.gbLogonParameters.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.gbLogonParameters.Text = " Logon Parameters";
            // 
            // LoginUI
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.ClientSize = new System.Drawing.Size(454, 201);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtDate);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.PleaseEnter);
            this.Controls.Add(this.gbLogonParameters);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Modal = true;
            this.RightToLeftLayout = true;
            this.Text = "כניסה למערכת";
            this.Load += new System.EventHandler(this.LoginUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.Security.UI.TextBox textBox1;
        private ENV.Security.UI.TextBox textBox2;
        private ENV.Security.UI.TextBox textBox3;
        private ENV.Security.UI.TextBox textBox4;
        private ENV.Security.UI.Button button1;
        private ENV.Security.UI.Button button2;
        private ENV.UI.GroupBox gbLogonParameters;
        private UI.TextBox PleaseEnter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private UI.TextBox txtDate;
        private UI.TextBox lblDate;

    }
}
