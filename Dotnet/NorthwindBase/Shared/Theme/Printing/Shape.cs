namespace Northwind.Shared.Theme.Printing
{
    public partial class Shape : ENV.Printing.Shape 
    {
        /// <summary>Shape</summary>
        public Shape()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
