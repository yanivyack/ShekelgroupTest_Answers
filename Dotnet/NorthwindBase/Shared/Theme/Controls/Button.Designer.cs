namespace Northwind.Shared.Theme.Controls
{
    partial class Button
    {
        void InitializeComponent()
        {
            AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0,0,0,0);
            AbsoluteBindTop = true;
            DrawDisabledWhiteContour = false;
            FontScheme = new Northwind.Shared.Theme.Fonts.PushButtonDefaultFont();
            RightToLeftLayout = false;
            HyperLinkColorScheme = new Northwind.Shared.Theme.Colors.HyperlinkPushbuttonText();
            HyperLinkPressedColorScheme = new Northwind.Shared.Theme.Colors.HyperlinkPushbuttonVisited();
            HyperLinkMouseEnterColorScheme = new Northwind.Shared.Theme.Colors.HyperlinkPushB_Walkthrough();
            FixedBackColorOnNormalStyle = true;
            SuppressExpandCommandInMultiSelect = true;
        }
    }
}
