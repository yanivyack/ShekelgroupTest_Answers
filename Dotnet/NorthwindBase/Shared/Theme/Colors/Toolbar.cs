using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Toolbar #52</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Toolbar")]
    class Toolbar : ColorScheme 
    {
        public Toolbar()
        {
            this.ForeColor = SystemColors.ControlText;
            this.BackColor = SystemColors.ButtonFace;
        }
    }
}
