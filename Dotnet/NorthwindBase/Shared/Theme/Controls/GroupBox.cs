namespace Northwind.Shared.Theme.Controls
{
    public partial class GroupBox : ENV.UI.GroupBox 
    {
        /// <summary>GroupBox</summary>
        public GroupBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
