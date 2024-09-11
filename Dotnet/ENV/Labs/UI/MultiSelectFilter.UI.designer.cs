namespace ENV.Labs.UI
{
    partial class MultiSelectFilterUI
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
            this.grid2 = new Firefly.Box.UI.Grid();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.clearButton = new Button();
            this.grid1 = new Firefly.Box.UI.Grid();
            this.gridColumn1 = new Firefly.Box.UI.GridColumn();
            this.checkBox1 = new Firefly.Box.UI.CheckBox();
            this.gridColumn2 = new Firefly.Box.UI.GridColumn();
            this.textBox1 = new Firefly.Box.UI.TextBox();
            this.gridColumn3 = new Firefly.Box.UI.GridColumn();
            this.textBox2 = new Firefly.Box.UI.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.grid1.SuspendLayout();
            this.gridColumn1.SuspendLayout();
            this.gridColumn2.SuspendLayout();
            this.gridColumn3.SuspendLayout();
            this.SuspendLayout();

            // 
            // grid2
            // 
            this.grid2.AllowDrop = true;
            this.grid2.Location = new System.Drawing.Point(2, 1);
            this.grid2.Name = "grid2";
            this.grid2.Size = new System.Drawing.Size(240, 150);
            this.grid2.TabIndex = 2;
            this.grid2.Text = "grid2";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(84, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 20);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(165, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 20);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.cancelButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.okButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.clearButton, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 274);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.22222F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(243, 26);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.clearButton.Location = new System.Drawing.Point(3, 3);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 20);
            this.clearButton.TabIndex = 1;
            this.clearButton.Text = "Clear";
            // 
            // grid1
            // 
            this.grid1.AllowDrop = true;
            this.grid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grid1.ColumnSeparators = false;
            this.grid1.Controls.Add(this.gridColumn1);
            this.grid1.Controls.Add(this.gridColumn2);
            this.grid1.Controls.Add(this.gridColumn3);
            this.grid1.HeaderHeight = 0;
            this.grid1.Location = new System.Drawing.Point(12, 10);
            this.grid1.Name = "grid1";
            this.grid1.RowHeight = 20;
            this.grid1.RowSeparators = false;
            this.grid1.Size = new System.Drawing.Size(247, 258);
            this.grid1.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.grid1.TabIndex = 0;
            this.grid1.Text = "grid1";
            // 
            // gridColumn1
            // 
            this.gridColumn1.AutoResize = false;
            this.gridColumn1.Controls.Add(this.checkBox1);
            this.gridColumn1.Location = new System.Drawing.Point(1, 1);
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Size = new System.Drawing.Size(21, 256);
            this.gridColumn1.TabIndex = 1;
            this.gridColumn1.Text = "gridColumn1";
            // 
            // checkBox1
            // 
            this.checkBox1.AllowChangeInBrowse = true;
            this.checkBox1.Location = new System.Drawing.Point(4, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(16, 13);
            this.checkBox1.TabIndex = 0;

            // 
            // gridColumn2
            // 
            this.gridColumn2.Controls.Add(this.textBox1);
            this.gridColumn2.Location = new System.Drawing.Point(22, 1);
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Size = new System.Drawing.Size(144, 256);
            this.gridColumn2.TabIndex = 3;
            this.gridColumn2.Text = "gridColumn2";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(7, 3);
            this.textBox1.Size = new System.Drawing.Size(100, 15);
            this.textBox1.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.textBox1.TabIndex = 0;

            // 
            // gridColumn3
            // 
            this.gridColumn3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gridColumn3.AutoResize = false;
            this.gridColumn3.Controls.Add(this.textBox2);
            this.gridColumn3.Location = new System.Drawing.Point(166, 1);
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Size = new System.Drawing.Size(51, 256);
            this.gridColumn3.TabIndex = 4;
            this.gridColumn3.Text = "gridColumn3";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(3, 3);
            this.textBox2.Size = new System.Drawing.Size(38, 15);
            this.textBox2.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.textBox2.TabIndex = 0;

            // 
            // MultiSelectFilterUI
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(270, 307);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.grid1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.Name = "MultiSelectFilterUI";
            this.ShowInTaskbar = false;
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            this.TitleBar = false;
            this.ToolWindow = true;
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.grid1.ResumeLayout(false);
            this.gridColumn1.ResumeLayout(false);
            this.gridColumn2.ResumeLayout(false);
            this.gridColumn3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Firefly.Box.UI.Grid grid1;
        private Firefly.Box.UI.GridColumn gridColumn1;
        private Firefly.Box.UI.CheckBox checkBox1;
        private Firefly.Box.UI.GridColumn gridColumn2;
        private Firefly.Box.UI.TextBox textBox1;
        private Firefly.Box.UI.GridColumn gridColumn3;
        private Firefly.Box.UI.TextBox textBox2;
        private Firefly.Box.UI.Grid grid2;
        private Button clearButton;
        private Button okButton;
        private Button cancelButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}