namespace Northwind.Shared.Theme.Controls
{
    public partial class Line : Firefly.Box.UI.Line 
    {
        /// <summary>Line</summary>
        public Line()
        {
            if (!DesignMode)
            	FixedBackColorInNonFlatStyles = ENV.UserSettings.FixedBackColorInNonFlatStyles;
            InitializeComponent();
        }
    }
}
