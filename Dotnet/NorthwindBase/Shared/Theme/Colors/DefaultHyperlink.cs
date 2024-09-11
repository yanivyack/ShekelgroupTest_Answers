using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Default Hyperlink #7</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Hyperlink")]
    class DefaultHyperlink : ColorScheme 
    {
        public DefaultHyperlink()
        {
            this.ForeColor = Color.FromArgb(0,0,255);
            this.BackColor = Color.FromArgb(192,192,192);
        }
    }
}
