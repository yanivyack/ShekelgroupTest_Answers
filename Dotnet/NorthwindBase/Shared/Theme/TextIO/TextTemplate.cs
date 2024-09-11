namespace Northwind.Shared.Theme.TextIO
{
    public class TextTemplate : ENV.IO.TextTemplate 
    {
        /// <summary>TextTemplate</summary>
        public TextTemplate(string templateFileName) : base(templateFileName)
        {
            InitializeComponent();
        }
        void InitializeComponent()
        {
        }
    }
}
