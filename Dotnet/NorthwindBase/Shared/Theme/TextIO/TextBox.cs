namespace Northwind.Shared.Theme.TextIO
{
    public class TextBox : ENV.IO.TextBox 
    {
        /// <summary>TextBox</summary>
        public TextBox()
        {
            InitializeComponent();
        }
        void InitializeComponent()
        {
            Alignment = System.Drawing.ContentAlignment.TopLeft;
            HeightInChars = 1;
        }
    }
}
