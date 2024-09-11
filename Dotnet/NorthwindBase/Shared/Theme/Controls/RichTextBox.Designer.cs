namespace Northwind.Shared.Theme.Controls
{
    partial class RichTextBox
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultEditField();
            Style = Firefly.Box.UI.ControlStyle.Standard;
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            RightToLeftLayout = false;
            OnEnterEditingSelectAllText = true;
        }
    }
}
