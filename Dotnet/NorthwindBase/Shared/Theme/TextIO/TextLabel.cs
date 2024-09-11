namespace Northwind.Shared.Theme.TextIO
{
    public class TextLabel : ENV.IO.TextLabel 
    {
        /// <summary>TextLabel</summary>
        public TextLabel()
        {
            InitializeComponent();
        }
        void InitializeComponent()
        {
            Alignment = System.Drawing.ContentAlignment.MiddleLeft;
        }
    }
}
