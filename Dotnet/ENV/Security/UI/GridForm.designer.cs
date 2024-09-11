using System.Drawing;
using Firefly.Box;
using Firefly.Box.UI.Advanced;

namespace ENV.Security.UI
{
    partial class GridForm
    {
        void InitializeComponent()
        {
            this.okBtn = new ENV.Security.UI.Button();
            this.grid1 = new ENV.Security.UI.Grid();
            this.cancelBtn = new ENV.Security.UI.Button();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBtn.Location = new System.Drawing.Point(407, 357);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.Text = "Ok";
            this.okBtn.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.okBtn_Click);
            // 
            // grid1
            // 
            this.grid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid1.Location = new System.Drawing.Point(7, 22);
            this.grid1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 50);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(475, 331);
            this.grid1.Text = "grid1";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(326, 357);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.cancelBtn_Click);
            // 
            // GridForm
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(492, 384);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.grid1);
            this.Modal = true;
            this.Padding = new System.Windows.Forms.Padding(7, 0, 10, 25);
            this.RightToLeftLayout = true;
            this.Text = " ";
            this.ResumeLayout(false);

        }
        protected Grid grid1;
        protected Button okBtn;
        protected Button cancelBtn;
    }
}
