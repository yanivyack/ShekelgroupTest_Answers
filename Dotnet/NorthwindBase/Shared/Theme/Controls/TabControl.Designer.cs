namespace Northwind.Shared.Theme.Controls
{
    partial class TabControl
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            Alignment = System.Drawing.ContentAlignment.TopLeft;
            RightToLeftLayout = false;
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            DrawDisabledWhiteContour = false;
            AllowChangeInBrowse = false;
        }
    }
}
