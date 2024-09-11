namespace ENV.Security.Tasks
{
    partial class RolesUI
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
            this.gridColumn2 = new Firefly.Box.UI.GridColumn();
            this.textBox2 = new ENV.Security.UI.TextBox();
            this.grid1.SuspendLayout();
            this.gridColumn1.SuspendLayout();
            this.gridColumn2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.AllowUserToReorderColumns = true;
            this.grid1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.grid1.Controls.Add(this.gridColumn1);
            this.grid1.Controls.Add(this.gridColumn2);
            this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid1.DrawPartialRow = false;
            this.grid1.Location = new System.Drawing.Point(5, 15);
            this.grid1.Size = new System.Drawing.Size(428, 285);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(358, 307);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(277, 307);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Controls.Add(this.textBox1);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Text = "שם הרשאה";
            this.gridColumn1.Width = 119;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.ShowExpandButton = true;
            this.textBox1.Size = new System.Drawing.Size(113, 15);
            this.textBox1.Text = "textBox1";
            this.textBox1.Data = this._task._roles.Role;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Controls.Add(this.textBox2);
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Text = "תאור";
            this.gridColumn2.Width = 289;
            // 
            // textBox2
            // 
            this.textBox2.AllowFocus = false;
            this.textBox2.Location = new System.Drawing.Point(6, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(280, 15);
            this.textBox2.Text = "textBox1";
            this.textBox2.Data = this._task.AvailableRoles.Description;
            // 
            // RolesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(438, 335);
            this.Modal = true;
            this.RightToLeftLayout = true;
            this.Text = "הרשאות";
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.gridColumn2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.Security.UI.GridColumn gridColumn1;
        private ENV.Security.UI.TextBox textBox1;
        private Firefly.Box.UI.GridColumn gridColumn2;
        private ENV.Security.UI.TextBox textBox2;

    }
}