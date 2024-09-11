using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
namespace Northwind.Views.Controls
{
    partial class V9CompatibleDefaultTable
    {
        void InitializeComponent()
        {
            this.ActiveRowStyle = Firefly.Box.UI.GridActiveRowStyle.Border;
            this.AllowUserToReorderColumns = false;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.ColumnSeparators = true;
            this.DrawPartialRow = false;
            this.HorizontalScrollbar = false;
            this.RowSeparators = true;
            this.UseVisualStyles = false;
            this.WidthByColumns = true;
        }
    }
}
