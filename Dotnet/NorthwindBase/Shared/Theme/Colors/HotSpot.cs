using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Hot Spot #38</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Hot Spot")]
    class HotSpot : ColorScheme 
    {
        public HotSpot()
        {
            this.ForeColor = Color.FromArgb(153,253,119);
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
