namespace Northwind.Shared.Theme.Controls
{
    public partial class Grid : ENV.UI.Grid 
    {
        /// <summary>Grid</summary>
        public Grid()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            GridColumnType = typeof(GridColumn);
            DefaultTextBoxType = typeof(TextBox);
            InitializeComponent();
        }
    }
}
