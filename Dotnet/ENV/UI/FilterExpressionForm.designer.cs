namespace ENV.UI
{
    partial class FilterExpressionForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.variables = new System.Windows.Forms.ListBox();
            this.functions = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBox1.Size = new System.Drawing.Size(458, 172);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(476, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(476, 41);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // variables
            // 
            this.variables.FormattingEnabled = true;
            this.variables.Location = new System.Drawing.Point(12, 190);
            this.variables.Name = "variables";
            this.variables.Size = new System.Drawing.Size(299, 134);
            this.variables.TabIndex = 3;
            this.variables.DoubleClick += new System.EventHandler(this.variables_DoubleClick);
            // 
            // functions
            // 
            this.functions.FormattingEnabled = true;
            this.functions.Location = new System.Drawing.Point(317, 190);
            this.functions.Name = "functions";
            this.functions.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.functions.Size = new System.Drawing.Size(234, 134);
            this.functions.TabIndex = 4;
            this.functions.DoubleClick += new System.EventHandler(this.functions_SelectedIndexChanged);
            // 
            // FilterExpressionForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(563, 336);
            this.Controls.Add(this.functions);
            this.Controls.Add(this.variables);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "FilterExpressionForm";
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;
            this.Text = "FilterExpressionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox variables;
        private System.Windows.Forms.ListBox functions;
    }
}