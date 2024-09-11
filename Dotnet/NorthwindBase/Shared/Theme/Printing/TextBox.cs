namespace Northwind.Shared.Theme.Printing
{
    public partial class TextBox : ENV.Printing.TextBox 
    {
        /// <summary>TextBox</summary>
        public TextBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
