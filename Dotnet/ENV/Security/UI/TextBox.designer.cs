using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace ENV.Security.UI
{
    partial class TextBox
    {
        void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TextBox
            // 
            this.InputLanguageByRightToLeft = true;
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.Size = new System.Drawing.Size(100, 15);
            this.Style = Firefly.Box.UI.ControlStyle.Flat;
            AdvancedAnchor = new AdvancedAnchor(0, 100, 0, 0);
            this.ResumeLayout(false);

        }
    }
}
