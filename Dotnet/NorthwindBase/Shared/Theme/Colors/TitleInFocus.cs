using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Title in Focus #32</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Title in Focus")]
    class TitleInFocus : ColorScheme 
    {
        public TitleInFocus()
        {
            this.ForeColor = SystemColors.ActiveCaptionText;
            this.BackColor = SystemColors.ActiveCaption;
        }
    }
}
