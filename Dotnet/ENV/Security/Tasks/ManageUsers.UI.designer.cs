using System;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class ManageUsersUI
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
            this.gridColumn1 = new ENV.Security.UI.GridColumn();
            this.textBox1 = new ENV.Security.UI.TextBox();
            this.gridColumn2 = new ENV.Security.UI.GridColumn();
            this.textBox2 = new ENV.Security.UI.TextBox();
            this.gridColumn3 = new ENV.Security.UI.GridColumn();
            this.textBox3 = new ENV.Security.UI.TextBox();
            this.gridColumn4 = new ENV.Security.UI.GridColumn();
            this.textBox4 = new ENV.Security.UI.TextBox();
            this.gridColumn5 = new ENV.Security.UI.GridColumn();
            this.textBox5 = new ENV.Security.UI.TextBox();
            this.button1 = new ENV.Security.UI.Button();
            this.button3 = new ENV.Security.UI.Button();
            this.AdditionalInfo = new ENV.Security.UI.TextBox();
            this.LblAdditionalInfo = new ENV.Security.UI.TextBox();
            this.button4 = new ENV.Security.UI.Button();
            this.button5 = new ENV.Security.UI.Button();
            this.grid1.SuspendLayout();
            this.gridColumn1.SuspendLayout();
            this.gridColumn2.SuspendLayout();
            this.gridColumn3.SuspendLayout();
            this.gridColumn4.SuspendLayout();
            this.gridColumn5.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.AllowUserToReorderColumns = true;
            this.grid1.Controls.Add(this.gridColumn1);
            this.grid1.Controls.Add(this.gridColumn2);
            this.grid1.Controls.Add(this.gridColumn3);
            this.grid1.Controls.Add(this.gridColumn4);
            this.grid1.Controls.Add(this.gridColumn5);
            this.grid1.DrawPartialRow = false;
            this.grid1.Location = new System.Drawing.Point(5, 15);
            this.grid1.Size = new System.Drawing.Size(592, 287);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Controls.Add(this.textBox1);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Text = "שם משתמש";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(114, 15);
            this.textBox1.Text = "textBox1";
            this.textBox1.Data = this._task._users.UserName;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Controls.Add(this.textBox2);
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Text = "פרטים נוספים";
            this.gridColumn2.Width = 250;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(3, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(240, 15);
            this.textBox2.Text = "textBox2";
            this.textBox2.Data = this._task._users.Description;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Controls.Add(this.textBox3);
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Text = "סיסמה";
            this.gridColumn3.Width = 70;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(5, 3);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.ShowExpandButton = true;
            this.textBox3.Size = new System.Drawing.Size(62, 15);
            this.textBox3.Text = "textBox3";
            this.textBox3.UseSystemPasswordChar = true;
            this.textBox3.Change += new System.Action(this.textBox3_Change);
            this.textBox3.Expand += new System.Action(this.textBox3_Expand);
            this.textBox3.Data = this._task._users.Password;
            // 
            // gridColumn4
            // 
            this.gridColumn4.Controls.Add(this.textBox4);
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.Text = "הרשאות";
            this.gridColumn4.Width = 70;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(5, 3);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(62, 15);
            this.textBox4.Text = "textBox4";
            this.textBox4.Change += new System.Action(this.textBox3_Change);
            this.textBox4.Expand += new System.Action(this.textBox4_Expand);
            this.textBox4.Data = this._task.RoleCount;
            // 
            // gridColumn5
            // 
            this.gridColumn5.Controls.Add(this.textBox5);
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.Text = "קבוצות";
            this.gridColumn5.Width = 70;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(5, 3);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(62, 15);
            this.textBox5.Text = "textBox5";
            this.textBox5.Change += new System.Action(this.textBox3_Change);
            this.textBox5.Expand += new System.Action(this.textBox5_Expand);
            this.textBox5.Data = this._task.GroupCount;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Format = "קבוצות";
            this.button1.Location = new System.Drawing.Point(280, 404);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            this.button1.Data = this._task._groups;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Format = "הרשאות";
            this.button3.Location = new System.Drawing.Point(361, 404);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button3_Click);
            this.button3.Data = this._task._roles;
            // 
            // AdditionalInfo
            // 
            this.AdditionalInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdditionalInfo.Location = new System.Drawing.Point(5, 327);
            this.AdditionalInfo.Name = "AdditionalInfo";
            this.AdditionalInfo.Size = new System.Drawing.Size(592, 71);
            this.AdditionalInfo.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.AdditionalInfo.Text = "textBox3";
            this.AdditionalInfo.Change += new System.Action(this.textBox3_Change);
            this.AdditionalInfo.Data = this._task._users.AdditionalInfo;
            // 
            // LblAdditionalInfo
            // 
            this.LblAdditionalInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblAdditionalInfo.BackColor = System.Drawing.SystemColors.Control;
            this.LblAdditionalInfo.Location = new System.Drawing.Point(5, 306);
            this.LblAdditionalInfo.Name = "LblAdditionalInfo";
            this.LblAdditionalInfo.Size = new System.Drawing.Size(150, 15);
            this.LblAdditionalInfo.Text = "מידע נוסף";
            this.LblAdditionalInfo.Change += new System.Action(this.textBox3_Change);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Format = "קבוצות";
            this.button4.Location = new System.Drawing.Point(199, 404);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button4_Click);
            this.button4.BindEnabled += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.BooleanBindingEventArgs>(this.button4_BindEnabled);
            this.button4.Data = this._task._import;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Format = "הרשאות";
            this.button5.Location = new System.Drawing.Point(441, 404);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.Data = this._task._cancel;
            // 
            // ManageUsersUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BottomOfset = 140;
            this.ClientSize = new System.Drawing.Size(602, 432);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.AdditionalInfo);
            this.Controls.Add(this.LblAdditionalInfo);
            this.Modal = true;
            this.Padding = new System.Windows.Forms.Padding(7, 0, 10, 125);
            this.RightToLeftLayout = true;
            this.Text = "משתמשים";
            this.Load += new System.EventHandler(this.ManageUsersUI_Load);
            this.Controls.SetChildIndex(this.LblAdditionalInfo, 0);
            this.Controls.SetChildIndex(this.AdditionalInfo, 0);
            this.Controls.SetChildIndex(this.grid1, 0);
            this.Controls.SetChildIndex(this.button5, 0);
            this.Controls.SetChildIndex(this.button3, 0);
            this.Controls.SetChildIndex(this.button4, 0);
            this.Controls.SetChildIndex(this.button1, 0);
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.gridColumn2.ResumeLayout(false);
            this.gridColumn3.ResumeLayout(false);
            this.gridColumn4.ResumeLayout(false);
            this.gridColumn5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        

        

        #endregion

        private ENV.Security.UI.GridColumn gridColumn1;
        private ENV.Security.UI.GridColumn gridColumn2;
        private ENV.Security.UI.GridColumn gridColumn3;
        private ENV.Security.UI.GridColumn gridColumn4;
        private ENV.Security.UI.GridColumn gridColumn5;
        private ENV.Security.UI.TextBox textBox1;
        private ENV.Security.UI.TextBox textBox2;
        private ENV.Security.UI.TextBox textBox3;
        private ENV.Security.UI.TextBox textBox4;
        private ENV.Security.UI.TextBox textBox5;
        private ENV.Security.UI.Button button1;
        private ENV.Security.UI.Button button3;
        private ENV.Security.UI.TextBox AdditionalInfo;
        private ENV.Security.UI.TextBox LblAdditionalInfo;
        private ENV.Security.UI.Button button4;
        private UI.Button button5;

    }
}