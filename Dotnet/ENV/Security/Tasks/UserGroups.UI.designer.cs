namespace ENV.Security.Tasks
{
    partial class UserGroupsUI
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
            this.groupsCombo = new Firefly.Box.UI.ComboBox();
            this.grid1.SuspendLayout();
            this.gridColumn1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.Controls.Add(this.gridColumn1);
            this.grid1.Location = new System.Drawing.Point(5, 15);
            this.grid1.RowHeight = 26;
            this.grid1.Size = new System.Drawing.Size(203, 299);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Controls.Add(this.groupsCombo);
            this.gridColumn1.Location = new System.Drawing.Point(2, 22);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Size = new System.Drawing.Size(175, 275);
            this.gridColumn1.TabIndex = 1;
            this.gridColumn1.Text = "קבוצות";
            // 
            // groupsCombo
            // 
            this.groupsCombo.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.groupsCombo.Location = new System.Drawing.Point(3, 1);
            this.groupsCombo.Size = new System.Drawing.Size(167, 21);
            this.groupsCombo.TabIndex = 0;
            this.groupsCombo.Text = "comboBox1";
            this.groupsCombo.Data = this._task._userGroups.GroupID;
            // 
            // UserGroupsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(213, 339);
            this.RightToLeftLayout = true;
            this.Text = "קבוצות למשתמש";
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.Security.UI.GridColumn gridColumn1;
        private Firefly.Box.UI.ComboBox groupsCombo;

    }
}