namespace Northwind.Shared.Theme.Controls
{
    public partial class CompatibleGridColumn : GridColumn 
    {
        /// <summary>CompatibleGridColumn</summary>
        public CompatibleGridColumn()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
