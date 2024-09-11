namespace Northwind.Shared.Theme.Controls
{
    public partial class PictureBox : ENV.UI.PictureBox 
    {
        /// <summary>PictureBox</summary>
        public PictureBox()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
