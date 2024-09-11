namespace Northwind.Shared.Theme.Controls
{
    partial class Line
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            Style = Firefly.Box.UI.ControlStyle.Raised;
            RightToLeftLayout = false;
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
        }
    }
}
