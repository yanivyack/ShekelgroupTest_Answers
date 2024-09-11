namespace ENV.UI
{
    partial class V8CompatibleSortSelectColumns
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
            this.grid1 = new ENV.UI.Grid();
            this.gcFromFile = new ENV.UI.GridColumn();
            this.txtFromFile = new ENV.UI.TextBox();
            this.gcDescription = new ENV.UI.GridColumn();
            this.txtDescription = new ENV.UI.TextBox();
            this.gcLetter = new ENV.UI.GridColumn();
            this.txtLetter = new ENV.UI.TextBox();
            this.grid1.SuspendLayout();
            this.gcFromFile.SuspendLayout();
            this.gcDescription.SuspendLayout();
            this.gcLetter.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.Controls.Add(this.gcLetter);
            this.grid1.Controls.Add(this.gcDescription);
            this.grid1.Controls.Add(this.gcFromFile);
            this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid1.HorizontalScrollbar = false;
            this.grid1.Location = new System.Drawing.Point(0, 0);
            this.grid1.Name = "grid1";
            this.grid1.RowHeight = 20;
            this.grid1.Size = new System.Drawing.Size(243, 339);
            this.grid1.Text = "grid1";
            // 
            // gcFromFile
            // 
            this.gcFromFile.Controls.Add(this.txtFromFile);
            this.gcFromFile.Name = "gcFromFile";
            this.gcFromFile.Text = "From Entity";
            this.gcFromFile.Width = 82;
            this.gcFromFile.Click += new System.EventHandler(this.gcFromFile_Click);
            // 
            // txtFromFile
            // 
            this.txtFromFile.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFromFile.Location = new System.Drawing.Point(3, 1);
            this.txtFromFile.Name = "txtFromFile";
            this.txtFromFile.Size = new System.Drawing.Size(77, 15);
            this.txtFromFile.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtFromFile.Data = this._controller.s.FromFile;
            // 
            // gcDescription
            // 
            this.gcDescription.Controls.Add(this.txtDescription);
            this.gcDescription.Name = "gcDescription";
            this.gcDescription.Text = "Name";
            // 
            // txtDescription
            // 
            this.txtDescription.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(6, 1);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(112, 15);
            this.txtDescription.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtDescription.Data = this._controller.s.Description;
            // 
            // gcLetter
            // 
            this.gcLetter.Controls.Add(this.txtLetter);
            this.gcLetter.Name = "gcLetter";
            this.gcLetter.Width = 21;
            this.gcLetter.Click += new System.EventHandler(this.gcLetter_Click);
            // 
            // txtLetter
            // 
            this.txtLetter.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtLetter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLetter.Location = new System.Drawing.Point(1, 1);
            this.txtLetter.Name = "txtLetter";
            this.txtLetter.Size = new System.Drawing.Size(18, 15);
            this.txtLetter.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtLetter.Data = this._controller.s.Letter;
            // 
            // V8CompatibleSortSelectColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Border = Firefly.Box.UI.ControlBorderStyle.None;
            this.ChildWindow = true;
            this.ClientSize = new System.Drawing.Size(243, 339);
            this.Controls.Add(this.grid1);
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.Location = new System.Drawing.Point(261, 3);
            this.Name = "V8CompatibleSortSelectColumns";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.RightToLeftLayout = true;
            this.Text = " ";
            this.TitleBar = false;
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.grid1.ResumeLayout(false);
            this.gcFromFile.ResumeLayout(false);
            this.gcDescription.ResumeLayout(false);
            this.gcLetter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Grid grid1;
        private GridColumn gcFromFile;
        private TextBox txtFromFile;
        private GridColumn gcDescription;
        private TextBox txtDescription;
        private GridColumn gcLetter;
        private TextBox txtLetter;
    }
}