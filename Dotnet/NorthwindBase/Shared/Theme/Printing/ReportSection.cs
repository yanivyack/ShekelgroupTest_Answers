namespace Northwind.Shared.Theme.Printing
{
    public partial class ReportSection : ENV.Printing.ReportSection 
    {
        /// <summary>ReportSection</summary>
        public ReportSection()
        {
            DefaultLabelType = typeof(Label);
            DefaultTextBoxType = typeof(TextBox);
            InitializeComponent();
        }
    }
}
