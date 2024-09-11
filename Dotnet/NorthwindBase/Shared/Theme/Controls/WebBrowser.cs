namespace Northwind.Shared.Theme.Controls
{
    public partial class WebBrowser : ENV.UI.WebBrowser 
    {
        /// <summary>WebBrowser</summary>
        public WebBrowser()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
