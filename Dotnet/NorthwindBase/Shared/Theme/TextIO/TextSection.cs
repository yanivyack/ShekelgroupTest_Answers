namespace Northwind.Shared.Theme.TextIO
{
    public partial class TextSection : ENV.IO.TextSection 
    {
        /// <summary>TextSection</summary>
        public TextSection()
        {
            DefaultLabelType = typeof(TextLabel);
            DefaultTextBoxType = typeof(TextBox);
            InitializeComponent();
        }
    }
}
