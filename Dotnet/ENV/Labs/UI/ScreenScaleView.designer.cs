namespace ENV.Labs.UI
{
    partial class ScreenScaleView
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
            this.btnOK = new ENV.Labs.UI.Button();
            this.lstSource = new ENV.UI.ListBox();
            this.lstTarget = new ENV.UI.ListBox();
            this.btnCancel = new ENV.Labs.UI.Button();
            this.label1 = new ENV.UI.Label();
            this.txtX = new ENV.UI.TextBox();
            this.txtY = new ENV.UI.TextBox();
            this.label2 = new ENV.UI.Label();
            this.label3 = new ENV.UI.Label();
            this.label4 = new ENV.UI.Label();
            this.pictureBox1 = new ENV.Labs.UI.ScreenPicture();
            this.pictureBox2 = new ENV.Labs.UI.ScreenPicture();
            this.defaultFont1 = new ENV.Labs.UI.DefaultFont();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(280, 257);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.Text = "OK";
            this.btnOK.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.btnOK_Click);
            // 
            // lstSource
            // 
            this.lstSource.FontScheme = this.defaultFont1;
            this.lstSource.Location = new System.Drawing.Point(65, 32);
            this.lstSource.Name = "lstSource";
            this.lstSource.Size = new System.Drawing.Size(113, 69);
            this.lstSource.Text = "listBox1";
            this.lstSource.Data = this._controller.SourceRes;
            // 
            // lstTarget
            // 
            this.lstTarget.FontScheme = this.defaultFont1;
            this.lstTarget.Location = new System.Drawing.Point(281, 32);
            this.lstTarget.Name = "lstTarget";
            this.lstTarget.Size = new System.Drawing.Size(113, 69);
            this.lstTarget.Text = "listBox1";
            this.lstTarget.Data = this._controller.TargetRes;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(361, 257);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.FontScheme = this.defaultFont1;
            this.label1.Location = new System.Drawing.Point(16, 257);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 20);
            this.label1.Text = "Horizontal Factor:";
            // 
            // txtX
            // 
            this.txtX.Alignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtX.FontScheme = this.defaultFont1;
            this.txtX.Location = new System.Drawing.Point(104, 257);
            this.txtX.Name = "txtX";
            this.txtX.Size = new System.Drawing.Size(38, 20);
            this.txtX.Data = this._controller.XFactor;
            // 
            // txtY
            // 
            this.txtY.Alignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtY.FontScheme = this.defaultFont1;
            this.txtY.Location = new System.Drawing.Point(232, 257);
            this.txtY.Name = "txtY";
            this.txtY.Size = new System.Drawing.Size(37, 20);
            this.txtY.Data = this._controller.YFactor;
            // 
            // label2
            // 
            this.label2.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.FontScheme = this.defaultFont1;
            this.label2.Location = new System.Drawing.Point(152, 257);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 20);
            this.label2.Text = "Vertical Factor:";
            // 
            // label3
            // 
            this.label3.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.FontScheme = this.defaultFont1;
            this.label3.Location = new System.Drawing.Point(65, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 20);
            this.label3.Text = "Original Resolution:";
            // 
            // label4
            // 
            this.label4.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.label4.FontScheme = this.defaultFont1;
            this.label4.Location = new System.Drawing.Point(281, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.Text = "New Resolution:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImageLayout = Firefly.Box.UI.ImageLayout.Stretch;
            this.pictureBox1.ImageLocation = "c:\\Temp\\Screen.jpg";
            this.pictureBox1.Location = new System.Drawing.Point(71, 114);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.Text = "pictureBox1";
            this.pictureBox1.BindHeight += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox1_BindHeight);
            this.pictureBox1.BindLeft += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox1_BindLeft);
            this.pictureBox1.BindTop += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox1_BindTop);
            this.pictureBox1.BindWidth += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox1_BindWidth);
            // 
            // pictureBox2
            // 
            this.pictureBox2.ImageLayout = Firefly.Box.UI.ImageLayout.Stretch;
            this.pictureBox2.ImageLocation = "c:\\Temp\\Screen.jpg";
            this.pictureBox2.Location = new System.Drawing.Point(287, 114);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(100, 100);
            this.pictureBox2.Text = "pictureBox1";
            this.pictureBox2.BindHeight += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox2_BindHeight);
            this.pictureBox2.BindLeft += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox2_BindLeft);
            this.pictureBox2.BindTop += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox2_BindTop);
            this.pictureBox2.BindWidth += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.IntBindingEventArgs>(this.pictureBox2_BindWidth);
            // 
            // ScreenScaleView
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(456, 293);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstSource);
            this.Controls.Add(this.lstTarget);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtX);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.Name = "ScreenScaleView";
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterScreen;
            this.Text = "Screen Scaling";
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.ResumeLayout(false);

        }

        #endregion

        private ENV.UI.ListBox lstSource;
        private ENV.UI.ListBox lstTarget;
        private ENV.UI.TextBox txtX;
        private ENV.UI.TextBox txtY;
        private UI.Button btnOK;
        private UI.Button btnCancel;
        private ENV.UI.Label label1;
        private ENV.UI.Label label2;
        private ENV.UI.Label label3;
        private ENV.UI.Label label4;
        private ScreenPicture pictureBox1;
        private ScreenPicture pictureBox2;
        private DefaultFont defaultFont1;
    }
}