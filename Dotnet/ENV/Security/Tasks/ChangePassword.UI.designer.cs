using System;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class ChangePasswordUI
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
            this.textBox1 = new ENV.Security.UI.TextBox();
            this.textBox2 = new ENV.Security.UI.TextBox();
            this.textBox3 = new ENV.Security.UI.TextBox();
            this.textBox4 = new ENV.Security.UI.TextBox();
            this.button1 = new ENV.Security.UI.Button();
            this.button2 = new ENV.Security.UI.Button();
            this.textBox5 = new ENV.Security.UI.TextBox();
            this.textBox6 = new ENV.Security.UI.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(144, 19);
            this.textBox1.Size = new System.Drawing.Size(140, 21);
            this.textBox1.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.textBox1.TabIndex = 5;
            this.textBox1.Text = "textBox1";
            this.textBox1.UseSystemPasswordChar = true;
            this.textBox1.Data = this._task.CurrentPassword;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(144, 46);
            this.textBox2.Size = new System.Drawing.Size(140, 21);
            this.textBox2.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.textBox2.TabIndex = 1;
            this.textBox2.Text = "textBox2";
            this.textBox2.UseSystemPasswordChar = true;
            this.textBox2.Data = this._task.NewPassword;
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Control;
            this.textBox3.Location = new System.Drawing.Point(12, 19);
            this.textBox3.Size = new System.Drawing.Size(132, 21);
            this.textBox3.TabIndex = 2;
            this.textBox3.Text = "Current Password";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Control;
            this.textBox4.Location = new System.Drawing.Point(12, 46);
            this.textBox4.Size = new System.Drawing.Size(132, 21);
            this.textBox4.TabIndex = 1;
            this.textBox4.Text = "New Password";
            // 
            // button1
            // 
            this.button1.Format = "יציאה";
            this.button1.Location = new System.Drawing.Point(209, 114);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            this.button1.Data = this._task.Cancel;
            // 
            // button2
            // 
            this.button2.Format = "כניסה";
            this.button2.Location = new System.Drawing.Point(127, 114);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "button2";
            this.button2.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button2_Click);
            this.button2.Data = this._task.Ok;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(144, 73);
            this.textBox5.Size = new System.Drawing.Size(140, 21);
            this.textBox5.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.textBox5.TabIndex = 1;
            this.textBox5.Text = "textBox2";
            this.textBox5.UseSystemPasswordChar = true;
            this.textBox5.Data = this._task.ConfirmNewPassword;
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Control;
            this.textBox6.Location = new System.Drawing.Point(12, 73);
            this.textBox6.Size = new System.Drawing.Size(132, 21);
            this.textBox6.TabIndex = 1;
            this.textBox6.Text = "Confirm New Password";
            // 
            // ChangePasswordUI
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 148);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeftLayout = true;
            this.Text = "כניסה למערכת";
            
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.Security.UI.TextBox textBox1;
        private ENV.Security.UI.TextBox textBox2;
        private ENV.Security.UI.TextBox textBox3;
        private ENV.Security.UI.TextBox textBox4;
        private ENV.Security.UI.Button button1;
        private ENV.Security.UI.Button button2;
        private ENV.Security.UI.TextBox textBox5;
        private ENV.Security.UI.TextBox textBox6;

    }
}
