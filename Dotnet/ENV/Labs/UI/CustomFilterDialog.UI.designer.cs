namespace ENV.Labs.UI
{
    partial class CustomFilterDialogUI
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
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.isCaseSensitiveCheckBox = new Firefly.Box.UI.CheckBox();
            this.filterTypeComboBox = new Firefly.Box.UI.ComboBox();
            this.toValueTextBox = new Firefly.Box.UI.TextBox();
            this.label2 = new Firefly.Box.UI.Label();
            this.fromValueTextBox = new Firefly.Box.UI.TextBox();
            this.messageLabel = new Firefly.Box.UI.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(198, 92);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.okButton_Click);
            this.cancelButton.CoolEnabled = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(279, 92);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.cancelButton_Click);
            this.cancelButton.CoolEnabled = true;
            // 
            // isCaseSensitiveCheckBox
            // 
            this.isCaseSensitiveCheckBox.Location = new System.Drawing.Point(13, 90);
            this.isCaseSensitiveCheckBox.Name = "isCaseSensitiveCheckBox";
            this.isCaseSensitiveCheckBox.Size = new System.Drawing.Size(104, 24);
            this.isCaseSensitiveCheckBox.TabIndex = 3;
            this.isCaseSensitiveCheckBox.Text = "Case Sensitive";
            // 
            // filterTypeComboBox
            // 

            this.filterTypeComboBox.Location = new System.Drawing.Point(13, 38);
            this.filterTypeComboBox.Name = "filterTypeComboBox";
            this.filterTypeComboBox.Size = new System.Drawing.Size(104, 21);
            this.filterTypeComboBox.TabIndex = 1;
            this.filterTypeComboBox.Text = "comboBox1";

            // 
            // toValueTextBox
            // 
            this.toValueTextBox.Location = new System.Drawing.Point(123, 66);
            this.toValueTextBox.Name = "toValueTextBox";
            this.toValueTextBox.Size = new System.Drawing.Size(231, 20);
            this.toValueTextBox.TabIndex = 2;
            this.toValueTextBox.BindVisible += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.BooleanBindingEventArgs>(this.label2_BindVisible);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(93, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "And";
            this.label2.BindVisible += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.BooleanBindingEventArgs>(this.label2_BindVisible);
            // 
            // fromValueTextBox
            // 
            this.fromValueTextBox.Location = new System.Drawing.Point(123, 39);
            this.fromValueTextBox.Name = "fromValueTextBox";
            this.fromValueTextBox.Size = new System.Drawing.Size(231, 20);
            this.fromValueTextBox.TabIndex = 0;
            // 
            // messageLabel
            // 
            this.messageLabel.Location = new System.Drawing.Point(13, 12);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(341, 20);
            this.messageLabel.TabIndex = 0;
            this.messageLabel.Text = "Show rows where:";
            // 
            // CustomFilterDialogUI
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(363, 126);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.isCaseSensitiveCheckBox);
            this.Controls.Add(this.filterTypeComboBox);
            this.Controls.Add(this.toValueTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fromValueTextBox);
            this.Controls.Add(this.messageLabel);
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomFilterDialogUI";
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            this.TabOrderMode = Firefly.Box.UI.TabOrderMode.Manual;
            this.Text = "Custom Filter";
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.ResumeLayout(false);

        }

        #endregion

        private Firefly.Box.UI.Label label2;
        private Button okButton;
        private Button cancelButton;
        internal Firefly.Box.UI.Label messageLabel;
        internal Firefly.Box.UI.CheckBox isCaseSensitiveCheckBox;
        internal Firefly.Box.UI.TextBox fromValueTextBox;
        internal Firefly.Box.UI.TextBox toValueTextBox;
        internal Firefly.Box.UI.ComboBox filterTypeComboBox;

    }
}