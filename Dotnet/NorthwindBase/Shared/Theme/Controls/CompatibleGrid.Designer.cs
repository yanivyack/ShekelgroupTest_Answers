namespace Northwind.Shared.Theme.Controls
{
    partial class CompatibleGrid
    {
        void InitializeComponent()
        {
            ColorScheme = new Northwind.Shared.Theme.Colors.DefaultWindow();
            FixedBackColorInNonFlatStyles = true;
            Border = Firefly.Box.UI.ControlBorderStyle.Thick;
            RowSeparators = false;
            ActiveRowStyle = Firefly.Box.UI.GridActiveRowStyle.RowAndControlsColor;
            ColumnSeparators = false;
            DrawPartialRow = true;
            AllowUserToReorderColumns = true;
            FixedFocusedTextboxForeColorInAlternatingRowColorStyle = true;
            UseControlAsActiveRowBackColorWhenStyleIsRowBackColorAndActiveRowColorSchemeWasNotSet = true;
            DrawPartialRowBehindScrollBar = false;
            MultiSelectRowStyle = Firefly.Box.UI.GridMultiSelectRowStyle.InvertedColors;
            UseDefaultBackColorForStandardStyleWithRowColorStyleByColumnAndControls = true;
            UseUpDownArrowKeysForControlNavigationIfThereIsOnlyOneVisibleRow = true;
        }
    }
}
