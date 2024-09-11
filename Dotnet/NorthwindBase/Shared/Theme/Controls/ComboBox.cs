namespace Northwind.Shared.Theme.Controls
{
    public partial class ComboBox : ENV.UI.ComboBox 
    {
        /// <summary>ComboBox</summary>
        public ComboBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
