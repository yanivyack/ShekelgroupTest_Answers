using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Application workspace #49</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Application workspace")]
    class ApplicationWorkspace : ColorScheme 
    {
        public ApplicationWorkspace()
        {
            this.ForeColor = SystemColors.WindowText;
            this.BackColor = SystemColors.AppWorkspace;
        }
    }
}
