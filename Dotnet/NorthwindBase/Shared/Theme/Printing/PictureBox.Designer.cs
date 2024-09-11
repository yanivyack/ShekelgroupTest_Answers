namespace Northwind.Shared.Theme.Printing
{
    partial class PictureBox
    {
        void InitializeComponent()
        {
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            Style = Firefly.Box.UI.ControlStyle.Flat;
            RightToLeftLayout = false;
            AbsoluteBindTop = true;
        }
    }
}
