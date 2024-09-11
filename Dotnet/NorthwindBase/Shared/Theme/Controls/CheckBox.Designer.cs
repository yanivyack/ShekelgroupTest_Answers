namespace Northwind.Shared.Theme.Controls
{
    partial class CheckBox
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            RightToLeftLayout = false;
            ColumnChangeWhileFocusedForcesRowChange = true;
            FixedForeColor = true;
            DrawDisabledWhiteContour = false;
        }
    }
}
