namespace Northwind.Shared.Theme.Controls
{
    public partial class CompatibleLabel : Label 
    {
        /// <summary>CompatibleLabel</summary>
        public CompatibleLabel()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
