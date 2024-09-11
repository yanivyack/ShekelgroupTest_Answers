namespace Northwind.Shared.Theme.IO.Html
{
    public class HtmlFrameSet : ENV.IO.Html.HtmlFrameSet 
    {
        /// <summary>HtmlFrameSet</summary>
        public HtmlFrameSet()
        {
            InitializeComponent();
        }
        void InitializeComponent()
        {
        }
        public HtmlFrameSet(ENV.BusinessProcessBase controller) : base(controller)
        {
            InitializeComponent();
        }
        public HtmlFrameSet(ENV.AbstractUIController controller) : base(controller)
        {
            InitializeComponent();
        }
        public HtmlFrameSet(ENV.ApplicationControllerBase controller) : base(controller)
        {
            InitializeComponent();
        }
    }
}
