namespace Northwind.Shared.Theme.Controls
{
    public partial class ListBox : ENV.UI.ListBox 
    {
        /// <summary>ListBox</summary>
        public ListBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
