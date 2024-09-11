using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Default Help Window #4</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Help Window")]
    public class DefaultHelpWindow : ColorScheme 
    {
        public DefaultHelpWindow()
        {
            this.ForeColor = SystemColors.InfoText;
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
