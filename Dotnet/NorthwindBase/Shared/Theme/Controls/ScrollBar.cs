namespace Northwind.Shared.Theme.Controls
{
    public partial class ScrollBar : ENV.UI.ScrollBar 
    {
        /// <summary>ScrollBar</summary>
        public ScrollBar()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
