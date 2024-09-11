namespace Northwind.Shared.Theme.Printing
{
    public partial class ReportLayout : ENV.Printing.ReportLayout 
    {
        /// <summary>ReportLayout</summary>
        public ReportLayout()
        {
            SectionType = typeof(ReportSection);
            InitializeComponent();
        }
        public ReportLayout(ENV.BusinessProcessBase controller) : base(controller)
        {
            InitializeComponent();
        }
        public ReportLayout(ENV.AbstractUIController controller) : base(controller)
        {
            InitializeComponent();
        }
        public ReportLayout(ENV.ApplicationControllerBase controller) : base(controller)
        {
            InitializeComponent();
        }
    }
}
