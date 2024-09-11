using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Menu bar #51</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Menu bar")]
    class MenuBar : ColorScheme 
    {
        public MenuBar()
        {
            this.ForeColor = SystemColors.ControlText;
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
