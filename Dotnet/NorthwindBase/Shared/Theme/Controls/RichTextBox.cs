namespace Northwind.Shared.Theme.Controls
{
    public partial class RichTextBox : ENV.UI.RichTextBox 
    {
        /// <summary>RichTextBox</summary>
        public RichTextBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
