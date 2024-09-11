using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Description Text #30</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Description Text")]
    class DescriptionText : ColorScheme 
    {
        public DescriptionText()
        {
            this.ForeColor = SystemColors.ButtonHighlight;
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
