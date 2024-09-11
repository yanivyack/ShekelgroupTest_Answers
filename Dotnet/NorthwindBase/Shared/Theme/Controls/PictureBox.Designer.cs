namespace Northwind.Shared.Theme.Controls
{
    partial class PictureBox
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            Style = Firefly.Box.UI.ControlStyle.Sunken;
            RightToLeftLayout = false;
        }
    }
}
