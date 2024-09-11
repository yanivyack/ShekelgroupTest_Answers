namespace Northwind.Shared.Theme.Controls
{
    public partial class CompatibleTextBox : TextBox 
    {
        /// <summary>CompatibleTextBox</summary>
        public CompatibleTextBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
