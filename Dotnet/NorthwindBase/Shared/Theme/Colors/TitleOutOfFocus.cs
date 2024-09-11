using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Title out of Focus #33</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Title out of Focus")]
    class TitleOutOfFocus : ColorScheme 
    {
        public TitleOutOfFocus()
        {
            this.ForeColor = SystemColors.InactiveCaptionText;
            this.BackColor = SystemColors.InactiveCaption;
        }
    }
}
