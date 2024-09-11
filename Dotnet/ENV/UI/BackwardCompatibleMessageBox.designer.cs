namespace ENV.UI
{
    partial class BackwardCompatibleMessageBoxUI
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
            this.btnNo = new ENV.UI.DialogButton();
            this.btnYes = new ENV.UI.DialogButton();
            this.panel1 = new Firefly.Box.UI.Shape();
            this.MessageText = new Firefly.Box.UI.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNo
            // 
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnNo.Location = new System.Drawing.Point(236, 98);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(87, 23);
            this.btnNo.Text = "button1";
            this.btnNo.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            this.btnNo.Data = this._task.No;
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(143, 98);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(87, 23);
            this.btnYes.Text = "button1";
            this.btnYes.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button2_Click);
            this.btnYes.Data = this._task.Yes;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(330, 84);
            this.panel1.Style = Firefly.Box.UI.ControlStyle.Flat;
            // 
            // MessageText
            // 
            this.MessageText.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.MessageText.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.MessageText.Location = new System.Drawing.Point(69, 21);
            this.MessageText.Multiline = true;
            this.MessageText.Name = "MessageText";
            this.MessageText.Size = new System.Drawing.Size(249, 52);
            this.MessageText.Text = "label1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.Location = new System.Drawing.Point(22, 21);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(39, 39);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // BackwardCompatibleMessageBoxUI
            // 
            this.AcceptButton = this.btnNo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnNo;
            this.ClientSize = new System.Drawing.Size(330, 132);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.MessageText);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Modal = true;
            this.Name = "BackwardCompatibleMessageBoxUI";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterScreen;
            this.Text = " ";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DialogButton btnNo;
        private DialogButton btnYes;
        private Firefly.Box.UI.Shape panel1;
        private Firefly.Box.UI.Label MessageText;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}