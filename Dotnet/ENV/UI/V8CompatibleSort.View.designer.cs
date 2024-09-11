namespace ENV.UI
{
    partial class V8CompatibleSortView
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
            this.gcDescription = new ENV.UI.GridColumn();
            this.txtDescription = new ENV.UI.TextBox();
            this.gcLetter = new ENV.UI.GridColumn();
            this.txtLetter = new ENV.UI.TextBox();
            this.gcOrder = new ENV.UI.GridColumn();
            this.txtOrder = new ENV.UI.TextBox();
            this.cancelButton = new ENV.UI.Button();
            this.okButton = new ENV.UI.Button();
            this.grid1.SuspendLayout();
            this.gcDescription.SuspendLayout();
            this.gcLetter.SuspendLayout();
            this.gcOrder.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.Controls.Add(this.gcOrder);
            this.grid1.Controls.Add(this.gcLetter);
            this.grid1.Controls.Add(this.gcDescription);
            this.grid1.Location = new System.Drawing.Point(3, 3);
            this.grid1.Name = "grid1";
            this.grid1.RowHeight = 20;
            this.grid1.Size = new System.Drawing.Size(253, 339);
            this.grid1.Text = "grid1";
            // 
            // gcDescription
            // 
            this.gcDescription.Controls.Add(this.txtDescription);
            this.gcDescription.Name = "gcDescription";
            this.gcDescription.Text = "Field Name";
            this.gcDescription.Width = 169;
            // 
            // txtDescription
            // 
            this.txtDescription.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtDescription.AllowFocus = false;
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(9, 1);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(158, 15);
            this.txtDescription.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtDescription.Data = this._controller._oc.Description;
            // 
            // gcLetter
            // 
            this.gcLetter.Controls.Add(this.txtLetter);
            this.gcLetter.Name = "gcLetter";
            this.gcLetter.Text = "Field";
            this.gcLetter.Width = 37;
            // 
            // txtLetter
            // 
            this.txtLetter.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtLetter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLetter.Location = new System.Drawing.Point(4, 1);
            this.txtLetter.Name = "txtLetter";
            this.txtLetter.Size = new System.Drawing.Size(31, 15);
            this.txtLetter.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtLetter.BindReadOnly += new Firefly.Box.UI.Advanced.BindingEventHandler<Firefly.Box.UI.Advanced.BooleanBindingEventArgs>(this.txtLetter_BindReadOnly);
            this.txtLetter.Data = this._controller._sc.Letter;
            // 
            // gcOrder
            // 
            this.gcOrder.Controls.Add(this.txtOrder);
            this.gcOrder.Name = "gcOrder";
            this.gcOrder.Text = "#";
            this.gcOrder.Width = 23;
            // 
            // txtOrder
            // 
            this.txtOrder.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtOrder.AllowFocus = false;
            this.txtOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOrder.Location = new System.Drawing.Point(1, 1);
            this.txtOrder.Name = "txtOrder";
            this.txtOrder.Size = new System.Drawing.Size(20, 15);
            this.txtOrder.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtOrder.Data = this._controller._sc.Order;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(441, 350);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(62, 23);
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button1_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(373, 350);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(62, 23);
            this.okButton.Text = "OK";
            this.okButton.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.button2_Click);
            // 
            // V8CompatibleSortView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(506, 376);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.grid1);
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.Name = "V8CompatibleSortView";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.RightToLeftLayout = true;
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            this.Text = "V8CompatibleSort";
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.grid1.ResumeLayout(false);
            this.gcDescription.ResumeLayout(false);
            this.gcLetter.ResumeLayout(false);
            this.gcOrder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UI.Grid grid1;
        private UI.GridColumn gcDescription;
        private UI.TextBox txtDescription;
        private UI.GridColumn gcLetter;
        private UI.TextBox txtLetter;
        private UI.GridColumn gcOrder;
        private UI.TextBox txtOrder;
        private UI.Button cancelButton;
        private UI.Button okButton;
    }
}