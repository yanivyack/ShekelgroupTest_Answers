namespace Northwind.Shared.Theme.Controls
{
    public partial class Shape : ENV.UI.Shape 
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
