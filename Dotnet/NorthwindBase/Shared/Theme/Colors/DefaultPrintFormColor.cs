using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Default Print Form Color #6</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Print Form Color")]
    public class DefaultPrintFormColor : ColorScheme 
    {
        public DefaultPrintFormColor()
        {
            this.ForeColor = SystemColors.WindowText;
            this.BackColor = Color.FromArgb(255,255,255);
        }
    }
}
