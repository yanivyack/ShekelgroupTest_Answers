using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Break Point #37</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Break Point")]
    class BreakPoint : ColorScheme 
    {
        public BreakPoint()
        {
            this.ForeColor = SystemColors.WindowText;
            this.BackColor = Color.FromArgb(255,0,0);
        }
    }
}
