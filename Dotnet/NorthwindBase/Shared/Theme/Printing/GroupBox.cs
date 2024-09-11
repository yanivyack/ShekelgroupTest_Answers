namespace Northwind.Shared.Theme.Printing
{
    public partial class GroupBox : ENV.Printing.GroupBox 
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
