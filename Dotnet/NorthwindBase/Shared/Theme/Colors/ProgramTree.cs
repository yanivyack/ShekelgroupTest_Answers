using System.Drawing;
using Firefly.Box.UI;
namespace Northwind.Shared.Theme.Colors
{
    /// <summary>Program Tree #35</summary>
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.Description("Program Tree")]
    class ProgramTree : ColorScheme 
    {
        public ProgramTree()
        {
            this.ForeColor = Color.FromArgb(0,0,128);
            this.BackColor = Color.Transparent;
            this.TransparentBackground = true;
        }
    }
}
