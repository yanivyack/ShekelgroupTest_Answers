namespace Northwind.Shared.Theme.Controls
{
    partial class CompatibleForm
    {
        void InitializeComponent()
        {
            FontScheme = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            SplittedChildOnlyIfParentFormHasSplitter = false;
            IgnoreBoundClientSizeInCenteredStartPosition = true;
        }
    }
}
