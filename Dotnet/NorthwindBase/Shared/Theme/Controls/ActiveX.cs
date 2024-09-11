namespace Northwind.Shared.Theme.Controls
{
    public partial class ActiveX : ENV.UI.ActiveX 
    {
        /// <summary>ActiveX</summary>
        public ActiveX()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
