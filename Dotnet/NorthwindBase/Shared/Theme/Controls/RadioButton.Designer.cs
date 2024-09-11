namespace Northwind.Shared.Theme.Controls
{
    partial class RadioButton
    {
        void InitializeComponent()
        {
            FontScheme = new Northwind.Shared.Theme.Fonts.RadioButtonDefaultFont();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            DrawDisabledWhiteContour = false;
            FixedBackColorInNonFlatStyles = true;
            Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            Style = Firefly.Box.UI.ControlStyle.Raised;
            RightToLeftLayout = false;
            RadioTextSpacing = 5;
            Multiline = true;
        }
    }
}
