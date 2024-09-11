using Firefly.Box.UI.Advanced;

namespace ENV.Security.Tasks
{
    partial class ManageGroupsUI
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
            this.button1 = new ENV.Security.UI.Button();
            this.gridColumn2 = new ENV.UI.GridColumn();
            this.textBox4 = new ENV.Security.UI.TextBox();
            this.grid1.SuspendLayout();
            this.gridColumn1.SuspendLayout();
            this.gridColumn2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.AllowUserToReorderColumns = true;
            this.grid1.Controls.Add(this.gridColumn1);
            this.grid1.Controls.Add(this.gridColumn2);
            this.grid1.DrawPartialRow = false;
            this.grid1.Location = new System.Drawing.Point(5, 15);
            this.grid1.Size = new System.Drawing.Size(325, 381);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.gridColumn1.Controls.Add(this.textBox1);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Text = "קבוצות";
            this.gridColumn1.Width = 225;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(217, 15);
            this.textBox1.Text = "textBox1";
            this.textBox1.Data = this._task._groups.Description;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Format = "הרשאות";
            this.button1.Location = new System.Drawing.Point(93, 403);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.Text = "button1";
            this.button1.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            this.button1.Data = this._task._roles;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Controls.Add(this.textBox4);
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Text = "gridColumn2";
            this.gridColumn2.Width = 75;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(3, 3);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(62, 15);
            this.textBox4.Text = "textBox4";
            this.textBox4.Expand += new System.Action(this.textBox4_Expand);
            this.textBox4.Data = this._task.RoleCount;
            // 
            // ManageGroupsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(335, 431);
            this.Controls.Add(this.button1);
            this.Modal = true;
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;
            this.Text = "קבוצות";
            this.Controls.SetChildIndex(this.grid1, 0);
            this.Controls.SetChildIndex(this.button1, 0);
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.gridColumn2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.Security.UI.GridColumn gridColumn1;
        private ENV.Security.UI.TextBox textBox1;
        private ENV.Security.UI.Button button1;
        private ENV.UI.GridColumn gridColumn2;
        private UI.TextBox textBox4;
    }
}