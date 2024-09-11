using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Window #18</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Window")]
    class Window : ColorScheme 
    {
        public Window()
        {
            this.ForeColor = SystemColors.WindowText;
            this.BackColor = SystemColors.Window;
        }
    }
}
