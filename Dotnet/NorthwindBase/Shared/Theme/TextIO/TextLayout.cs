namespace Northwind.Shared.Theme.TextIO
{
    public partial class TextLayout : ENV.IO.TextLayout 
    {
        /// <summary>TextLayout</summary>
        public TextLayout()
        {
            SectionType = typeof(TextSection);
            InitializeComponent();
        }
        public TextLayout(ENV.BusinessProcessBase controller) : base(controller)
        {
            InitializeComponent();
        }
        public TextLayout(ENV.AbstractUIController controller) : base(controller)
        {
            InitializeComponent();
        }
        public TextLayout(ENV.ApplicationControllerBase controller) : base(controller)
        {
            InitializeComponent();
        }
    }
}
