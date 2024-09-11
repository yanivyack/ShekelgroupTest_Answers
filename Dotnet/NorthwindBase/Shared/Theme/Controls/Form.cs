namespace Northwind.Shared.Theme.Controls
{
    public partial class Form : ENV.UI.Form 
    {
        /// <summary>Form</summary>
        public Form()
        {
            Icon = ENV.Common.DefaultIcon;
            DefaultLabelType = typeof(Label);
            DefaultTextBoxType = typeof(TextBox);
            InitializeComponent();
        }
    }
}
