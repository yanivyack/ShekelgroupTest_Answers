using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Broken Property #45</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Broken Property")]
    class BrokenProperty : ColorScheme 
    {
        public BrokenProperty()
        {
            this.ForeColor = Color.FromArgb(0,0,255);
            this.BackColor = SystemColors.Window;
        }
    }
}
