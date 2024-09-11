namespace Northwind.Shared.Theme.Printing
{
    partial class TextBox
    {
        void InitializeComponent()
        {
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            Style = Firefly.Box.UI.ControlStyle.Flat;
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            RightToLeftLayout = false;
            AbsoluteBindTop = true;
            RightToLeftByFormat = false;
        }
    }
}
