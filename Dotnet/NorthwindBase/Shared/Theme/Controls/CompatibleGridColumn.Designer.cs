namespace Northwind.Shared.Theme.Controls
{
    partial class CompatibleGridColumn
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableTitle();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            AllowSort = false;
            RightToLeftLayout = false;
            UseTextEndEllipsis = true;
            DrawDisabledWhiteContour = false;
        }
    }
}
