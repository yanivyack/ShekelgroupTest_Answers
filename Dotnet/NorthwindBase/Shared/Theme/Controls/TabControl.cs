namespace Northwind.Shared.Theme.Controls
{
    public partial class TabControl : ENV.UI.TabControl 
    {
        /// <summary>TabControl</summary>
        public TabControl()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
