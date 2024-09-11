namespace ENV.UI
{
    partial class SelectOrderByDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.OrdersByList = new System.Windows.Forms.ListBox();
            this.txtColumns = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Cancel);
            this.panel1.Controls.Add(this.Ok);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 289);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(265, 36);
            this.panel1.TabIndex = 9;
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(106, 6);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 9;
            this.Cancel.TabStop = false;
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(187, 6);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 8;
            this.Ok.TabStop = false;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.OrdersByList);
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtColumns);
            this.splitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.splitContainer1.Size = new System.Drawing.Size(265, 289);
            this.splitContainer1.SplitterDistance = 193;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
            // 
            // OrdersByList
            // 
            this.OrdersByList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OrdersByList.FormattingEnabled = true;
            this.OrdersByList.Location = new System.Drawing.Point(12, 12);
            this.OrdersByList.Name = "OrdersByList";
            this.OrdersByList.Size = new System.Drawing.Size(241, 173);
            this.OrdersByList.TabIndex = 0;
            this.OrdersByList.SelectedIndexChanged += new System.EventHandler(this.OrdersByList_SelectedIndexChanged);
            this.OrdersByList.DoubleClick += new System.EventHandler(this.OrdersByList_DoubleClick);
            // 
            // txtColumns
            // 
            this.txtColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtColumns.Location = new System.Drawing.Point(12, 3);
            this.txtColumns.Multiline = true;
            this.txtColumns.Name = "txtColumns";
            this.txtColumns.ReadOnly = true;
            this.txtColumns.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtColumns.Size = new System.Drawing.Size(241, 77);
            this.txtColumns.TabIndex = 12;
            this.txtColumns.TabStop = false;
            // 
            // SelectOrderByDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(265, 325);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(220, 300);
            this.Name = "SelectOrderByDialog";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.SelectOrderByDialog_Load);
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox OrdersByList;
        private System.Windows.Forms.TextBox txtColumns;
    }
}