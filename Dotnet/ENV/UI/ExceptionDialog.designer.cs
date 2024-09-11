namespace ENV.UI
{
    partial class ExceptionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionDialog));
            this.btnQuit = new System.Windows.Forms.Button();
            this.btnIgnore = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.lblException = new System.Windows.Forms.TextBox();
            this.lblHelp = new System.Windows.Forms.Label();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.btnCopyInfo = new System.Windows.Forms.Button();
            this.errorPictureBox = new System.Windows.Forms.PictureBox();
            this.warningPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnQuit
            // 
            this.btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuit.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnQuit.Location = new System.Drawing.Point(332, 112);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(85, 23);
            this.btnQuit.TabIndex = 5;
            this.btnQuit.Text = "&Quit";
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // btnIgnore
            // 
            this.btnIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.btnIgnore.Location = new System.Drawing.Point(332, 112);
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.Size = new System.Drawing.Size(85, 23);
            this.btnIgnore.TabIndex = 4;
            this.btnIgnore.Text = "&Continue";
            this.btnIgnore.Click += new System.EventHandler(this.btnIgnore_Click);
            // 
            // btnDetails
            // 
            this.btnDetails.Location = new System.Drawing.Point(12, 112);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(85, 23);
            this.btnDetails.TabIndex = 2;
            this.btnDetails.Text = "&Details";
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // lblException
            // 
            this.lblException.BackColor = System.Drawing.SystemColors.Control;
            this.lblException.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblException.Location = new System.Drawing.Point(60, 45);
            this.lblException.Multiline = true;
            this.lblException.Name = "lblException";
            this.lblException.Size = new System.Drawing.Size(353, 61);
            this.lblException.TabIndex = 6;
            this.lblException.Text = "ErrorInformation";
            // 
            // lblHelp
            // 
            this.lblHelp.Location = new System.Drawing.Point(60, 8);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(353, 28);
            this.lblHelp.TabIndex = 0;
            this.lblHelp.Text = "An error has occured in the application. Click Continue to ignore this error and " +
    "attempt to continue. Click Quit to close the application.";
            // 
            // txtDetails
            // 
            this.txtDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetails.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDetails.Location = new System.Drawing.Point(8, 168);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDetails.Size = new System.Drawing.Size(409, 194);
            this.txtDetails.TabIndex = 6;
            this.txtDetails.TabStop = false;
            this.txtDetails.WordWrap = false;
            // 
            // btnCopyInfo
            // 
            this.btnCopyInfo.Location = new System.Drawing.Point(103, 112);
            this.btnCopyInfo.Name = "btnCopyInfo";
            this.btnCopyInfo.Size = new System.Drawing.Size(85, 23);
            this.btnCopyInfo.TabIndex = 3;
            this.btnCopyInfo.Text = "C&opy Details";
            this.btnCopyInfo.Click += new System.EventHandler(this.btnCopyInfo_Click);
            // 
            // errorPictureBox
            // 
            this.errorPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("errorPictureBox.Image")));
            this.errorPictureBox.Location = new System.Drawing.Point(8, 8);
            this.errorPictureBox.Name = "errorPictureBox";
            this.errorPictureBox.Size = new System.Drawing.Size(39, 39);
            this.errorPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.errorPictureBox.TabIndex = 7;
            this.errorPictureBox.TabStop = false;
            // 
            // warningPictureBox
            // 
            this.warningPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("warningPictureBox.Image")));
            this.warningPictureBox.Location = new System.Drawing.Point(8, 8);
            this.warningPictureBox.Name = "warningPictureBox";
            this.warningPictureBox.Size = new System.Drawing.Size(39, 39);
            this.warningPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.warningPictureBox.TabIndex = 8;
            this.warningPictureBox.TabStop = false;
            // 
            // ExceptionDialog
            // 
            this.AcceptButton = this.btnDetails;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnQuit;
            this.ClientSize = new System.Drawing.Size(428, 374);
            this.Controls.Add(this.warningPictureBox);
            this.Controls.Add(this.errorPictureBox);
            this.Controls.Add(this.btnCopyInfo);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblException);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnIgnore);
            this.Controls.Add(this.btnQuit);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(444, 180);
            this.Name = "ExceptionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Button btnIgnore;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.TextBox lblException;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Button btnCopyInfo;
        private System.Windows.Forms.PictureBox errorPictureBox;
        private System.Windows.Forms.PictureBox warningPictureBox;
    }
}