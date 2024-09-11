using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Status bar #53</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Status bar")]
    class StatusBar : ColorScheme 
    {
        public StatusBar()
        {
            this.ForeColor = SystemColors.ControlText;
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
