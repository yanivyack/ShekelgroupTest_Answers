namespace Northwind.Shared.Theme.Printing
{
    partial class Grid
    {
        void InitializeComponent()
        {
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            DrawPartialRow = false;
            Style = Firefly.Box.UI.ControlStyle.Flat;
            Scrollbar = false;
            LastColumnHeaderSeparator = false;
            WidthByColumns = true;
            DoubleColumnSeparatorInFlatStyle = true;
            RightToLeftLayout = false;
        }
    }
}
