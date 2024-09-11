namespace Northwind.Shared.Theme.Controls
{
    partial class TreeView
    {
        void InitializeComponent()
        {
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
        }
    }
}
