namespace Northwind.Shared.Theme.Printing
{
    public partial class Grid : ENV.Printing.Grid 
    {
        /// <summary>Grid</summary>
        public Grid()
        {
            GridColumnType = typeof(GridColumn);
            DefaultTextBoxType = typeof(TextBox);
            InitializeComponent();
        }
    }
}
