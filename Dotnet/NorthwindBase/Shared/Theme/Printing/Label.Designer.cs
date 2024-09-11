namespace Northwind.Shared.Theme.Printing
{
    partial class Label
    {
        void InitializeComponent()
        {
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            Style = Firefly.Box.UI.ControlStyle.Flat;
            Alignment = System.Drawing.ContentAlignment.TopLeft;
            RightToLeftLayout = false;
            AbsoluteBindTop = true;
        }
    }
}
