namespace ENV.Security.Tasks
{
    partial class SecuredValuesUI
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
            this.button3 = new ENV.Security.UI.Button();
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
            this.grid1.Size = new System.Drawing.Size(621, 381);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Controls.Add(this.textBox1);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Text = "Name";
            this.gridColumn1.Width = 156;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(4, 1);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 18);
            this.textBox1.Data = this._task.securedValues.Name;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Controls.Add(this.textBox2);
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Text = "Value";
            this.gridColumn2.Width = 440;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(3, 1);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(432, 18);
            this.textBox2.Data = this._task.securedValues.Value;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Format = "הרשאות";
            this.button3.Location = new System.Drawing.Point(470, 403);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.Text = "button1";
            this.button3.Data = this._task._cancel;
            // 
            // SecuredValuesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(631, 431);
            this.Controls.Add(this.button3);
            this.Modal = true;
            this.RightToLeftLayout = true;
            this.Text = "SecuredValues";
            this.Click += new System.EventHandler(this.SecuredValuesUI_Click);
            this.Controls.SetChildIndex(this.grid1, 0);
            this.Controls.SetChildIndex(this.button3, 0);
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.gridColumn2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UI.GridColumn gridColumn1;
        private UI.TextBox textBox1;
        private UI.GridColumn gridColumn2;
        private UI.TextBox textBox2;
        private UI.Button button3;

    }
}