using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Default Edit Field #2</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Default Edit Field")]
    public class DefaultEditField : ColorScheme 
    {
        public DefaultEditField()
        {
            this.ForeColor = SystemColors.WindowText;
            this.BackColor = SystemColors.Window;
        }
    }
}
