namespace Northwind.Shared.Theme.Printing
{
    public partial class Label : ENV.Printing.Label 
    {
        /// <summary>Label</summary>
        public Label()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
